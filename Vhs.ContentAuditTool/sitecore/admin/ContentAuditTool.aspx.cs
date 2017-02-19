using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using Sitecore.sitecore.admin;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Diagnostics;
using Vhs.ContentAuditTool.Extensions;
using Vhs.ContentAuditTool.Models;
using Vhs.ContentAuditTool.Services;

namespace Vhs.ContentAuditTool.sitecore.admin
{
    public partial class ContentAudit : AdminPage
    {
        #region Private Variables

        private readonly Database _masterDb = Factory.GetDatabase("master");
        private Database _currentDb;

        private const string TargetDatabaseField = "Target database";
        private const string Separator = "|";

        #endregion // Private Variables


        #region Protected Methods

        #region OnInit
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            DatabasesDDL.SelectedIndexChanged += DatabasesDDL_SelectedIndexChanged;
            SitesDDL.SelectedIndexChanged += SitesDDL_SelectedIndexChanged;
            UnsupportedVersionsGridView.RowDeleting += UnsupportedVersionsGridView_RowDeleting;
            UnsupportedVersionsGridView.PageIndexChanging += UnsupportedVersionsGridView_PageIndexChanging;

            CheckSecurity();
            InitializeDatabases();
        }
        #endregion

        #region Page_Load
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            DatabasesDDL.Items.AddRange(Databases.ToArray());
            DatabasesDDL.DataBind();

            LoadSites();
        }
        #endregion

        #region UnsupportedVersionsGridView_PageIndexChanging
        protected void UnsupportedVersionsGridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            UnsupportedVersionsGridView.PageIndex = e.NewPageIndex;
            LoadData();
        }
        #endregion

        #region UnsupportedVersionsGridView_RowDeleting
        protected void UnsupportedVersionsGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            if (CurrentDatabase == null)
                return;

            var language = e.Keys["Language"].ToString();
            if (!language.HasText())
                return;

            var versionId = new ID(e.Keys["Id"].ToString());
            var versionItem = CurrentDatabase.GetItem(versionId, Language.Parse(language));
            if (versionItem == null)
                return;

            versionItem.Versions.RemoveAll(false);

            LoadData();
        }
        #endregion

        #region SitesDDL_SelectedIndexChanged
        protected void SitesDDL_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SitesDDL.SelectedValue.HasText())
                LoadData();
            else
                ClearData();
        }
        #endregion

        #region DatabasesDDL_SelectedIndexChanged
        protected void DatabasesDDL_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSites();
            ClearData();
        }
        #endregion

        #endregion // Protected Methods


        #region Private Methods

        #region InitializeSites
        private void InitializeSites(Database database)
        {
            const string PleaseSelectSiteItem = "-- Please select site --";
            const string SiteListWarning =
                "WARNING: cannot get any site, please try to correct \"Vhs.ContentAuditTool.SiteRootTemplateNames\" setting";

            Sites = new List<ListItem> { new ListItem(PleaseSelectSiteItem, string.Empty) };
            if (database == null)
                return;

            var templateNames = ConfigurationService.SiteRootTemplateNames.Split(new[] { Separator },
                StringSplitOptions.RemoveEmptyEntries);

            var sites = new List<Item>();
            foreach (var templateName in templateNames)
            {
                sites.AddRange(database.SelectItems(string.Format("/sitecore/content//*[@@templatename='{0}']", templateName)));
            }

            SiteListWarningLiteral.Text = !sites.Any() ? SiteListWarning : string.Empty;

            var supportMultiTenant = ConfigurationService.SupportMultiTenant;
            foreach (var site in sites)
            {
                var text = supportMultiTenant
                    ? string.Format("{0} - {1}", site.Parent.Name, site.DisplayName)
                    : site.DisplayName;
                Sites.Add(new ListItem(text, site.Paths.Path));
            }
        }
        #endregion

        #region InitializeDatabases
        private void InitializeDatabases()
        {
            Databases = new List<ListItem>();
            var dbs = _masterDb.SelectItems("/sitecore/system/Publishing targets//*[@@templatekey='publishing target']");
            Databases.Add(new ListItem("master", "master"));
            if (dbs != null)
            {
                foreach (var db in dbs.Where(db => db.Fields[TargetDatabaseField].Value.HasText()))
                {
                    Databases.Add(new ListItem(db.DisplayName, db.Fields[TargetDatabaseField].Value));
                }
            }
        }
        #endregion

        #region LoadData
        private void LoadData()
        {
            LoadSupportedLanguages(CurrentDatabase);
            LoadUnsupportedVersions();
        }
        #endregion

        #region LoadSites
        private void LoadSites()
        {
            InitializeSites(CurrentDatabase);

            SitesDDL.Items.Clear();
            SitesDDL.Items.AddRange(Sites.ToArray());
            SitesDDL.DataBind();
        }
        #endregion

        #region LoadUnsupportedVersions
        private void LoadUnsupportedVersions()
        {
            if (SiteItem == null || SiteLanguageItems.Count == 0)
            {
                UnsupportedVersionsGridView.DataSource = null;
                UnsupportedVersionsGridView.DataBind();
                return;
            }

            var contentItems = SiteItem.Axes.GetDescendants();
            var unsupportedVersions = new List<ItemVersion>();
            foreach (var contentItem in contentItems)
            {
                var allItemVersions = contentItem.Versions.GetVersions(true);
                foreach (var itemVersion in allItemVersions)
                {
                    if (unsupportedVersions.Any(v =>
                                            v.Id.Equals(itemVersion.ID.ToGuid().ToString()) &&
                                            v.Language.Equals(itemVersion.Language.Name)))
                        continue;
                    if (!IsSupportedVersion(itemVersion))
                    {
                        unsupportedVersions.Add(new ItemVersion
                        {
                            Id = itemVersion.ID.ToGuid().ToString(),
                            Path = itemVersion.Paths.Path,
                            Language = itemVersion.Language.Name
                        });
                    }
                }
            }

            UnsupportedVersionsGridView.DataSource = unsupportedVersions;
            UnsupportedVersionsGridView.DataBind();
        }
        #endregion

        #region IsUnsupportedVersion
        private bool IsSupportedVersion(Item itemVersion)
        {
            return SiteLanguageItems.Any(sl => sl.Name.Equals(itemVersion.Language.Name));
        }
        #endregion

        #region LoadSupportedLanguages
        private void LoadSupportedLanguages(Database database)
        {
            const string SiteSettingWarning = "WARNING: cannot get the site's setting item, please try to correct \"Vhs.ContentAuditTool.SiteSettingsTemplateNames\" setting";
            const string SupportedLanguagesWarning = "WARNING: cannot get the site's supported languages, please try to correct \"Vhs.ContentAuditTool.LanguagesFieldNames\" setting"; ;

            SupportedLanguagesLiteral.Text = SiteSettingWarning;
            SiteLanguageItems = new List<Item>();
            if (database == null)
                return;

            SiteItem = database.SelectSingleItem(SitesDDL.SelectedValue);
            if (SiteItem == null)
                return;

            var templateNames = ConfigurationService.SiteSettingsTemplateNames.Split(new[] { Separator },
                StringSplitOptions.RemoveEmptyEntries);

            Item siteSettingsItem = null;
            foreach (var templateName in templateNames)
            {
                siteSettingsItem =
                    SiteItem.Axes.SelectSingleItem(string.Format("descendant-or-self::*[@@templatename='{0}']",
                        templateName));

                if (siteSettingsItem != null) break;
            }

            if (siteSettingsItem == null)
                return;

            var languagesFieldNames = ConfigurationService.LanguagesFieldNames.Split(new[] { Separator },
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var languagesFieldName in languagesFieldNames)
            {
                SiteLanguageItems = siteSettingsItem.GetMultipleItemSelectionsFromField(languagesFieldName, false);
                if (SiteLanguageItems.Any()) break;
            }

            if (SiteLanguageItems.Count == 0)
            {
                SupportedLanguagesLiteral.Text = SupportedLanguagesWarning;
                return;
            }

            var supportedLanguagesBuilder = new StringBuilder("<ul>");
            foreach (var languageItem in SiteLanguageItems)
            {
                supportedLanguagesBuilder = supportedLanguagesBuilder.Append("<li>");
                supportedLanguagesBuilder = supportedLanguagesBuilder.Append(languageItem.Name);
                supportedLanguagesBuilder = supportedLanguagesBuilder.Append("</li>");
            }

            supportedLanguagesBuilder = supportedLanguagesBuilder.Append("</ul>");

            SupportedLanguagesLiteral.Text = supportedLanguagesBuilder.ToString();
        }
        #endregion

        #region ClearData
        private void ClearData()
        {
            UnsupportedVersionsGridView.DataSource = null;
            UnsupportedVersionsGridView.DataBind();
            SupportedLanguagesLiteral.Text = string.Empty;
        }
        #endregion

        #endregion // Private Methods


        #region Protected Properties

        protected Item SiteItem { get; set; }
        protected List<Item> SiteLanguageItems { get; set; }
        protected List<ListItem> Sites { get; set; }
        protected List<ListItem> Databases { get; set; }

        protected Database CurrentDatabase
        {
            get
            {
                if (_currentDb == null)
                {
                    try
                    {
                        _currentDb = Factory.GetDatabase(DatabasesDDL.SelectedValue);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Format("Vhs Content Audit Tool: {0}", ex.Message), this);
                    }
                }

                return _currentDb;
            }
        }

        #endregion // Protected Properties
    }
}