using SC = Sitecore;

namespace Vhs.ContentAuditTool.Services
{
    public static class ConfigurationService
    {
        public static string SiteRootTemplateNames
            => SC.Configuration.Settings.GetSetting("Vhs.ContentAuditTool.SiteRootTemplateNames", string.Empty);

        public static string SiteSettingsTemplateNames
            => SC.Configuration.Settings.GetSetting("Vhs.ContentAuditTool.SiteSettingsTemplateNames", string.Empty);

        public static string LanguagesFieldNames
            => SC.Configuration.Settings.GetSetting("Vhs.ContentAuditTool.LanguagesFieldNames", string.Empty);

        public static bool SupportMultiTenant
        {
            get
            {
                var supportMultiTenant = false;
                bool.TryParse(SC.Configuration.Settings.GetSetting("Vhs.ContentAuditTool.SupportMultiTenant", "false"),
                    out supportMultiTenant);
                return supportMultiTenant;
            }
            
        }
        
    }
}