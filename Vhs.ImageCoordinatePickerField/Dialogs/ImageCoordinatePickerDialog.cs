using System;
using System.Web.UI.WebControls;
using SC = Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Resources.Media;
using Sitecore.Web;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Vhs.ImageCoordinatePickerField.Constants;
using Vhs.ImageCoordinatePickerField.Services;
using ImageField = Sitecore.Data.Fields.ImageField;

namespace Vhs.ImageCoordinatePickerField.Dialogs
{
    public class ImageCoordinatePickerDialog : DialogForm
    {
        private const string Separator = "|";

        private readonly Database _masterDb = Factory.GetDatabase("master");

        public SC.Web.UI.HtmlControls.Image ImageFrame;

        public SC.Web.UI.HtmlControls.Edit TextBoxCoordinate;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (TextBoxCoordinate == null || ImageFrame == null) return;

            TextBoxCoordinate.Value = WebUtil.GetQueryString(QueryStringKeys.Value);

            var containerId = WebUtil.GetQueryString(QueryStringKeys.ContainerId);

            var currentItem = _masterDb.Items.GetItem(containerId);

            var parentItem = currentItem.Parent;

            var imageFieldNames = ConfigurationService.ImageFieldName.Split(new[] { Separator },
                StringSplitOptions.RemoveEmptyEntries);

            ImageField imageField = null;
            foreach (var imageFieldName in imageFieldNames)
            {
                imageField = parentItem.Fields[imageFieldName];
                if (imageField != null) break;
            }

            if (imageField == null || string.IsNullOrWhiteSpace(imageField.Value))
            {
                ImageFrame.Alt = ConfigurationService.ImageAlternateText;
                ImageFrame.Src = "#";
                return;
            }

            ImageFrame.Width = new Unit(double.Parse(imageField.Width), UnitType.Pixel);
            ImageFrame.Height = new Unit(double.Parse(imageField.Height), UnitType.Pixel);

            var imageSrc = MediaManager.GetMediaUrl(
                _masterDb.Items.GetItem(imageField.MediaID),
                new MediaUrlOptions
                {
                    Database = _masterDb,
                    DisableMediaCache = true,
                    DisableBrowserCache = true,
                    AllowStretch = false
                });

            if (!string.IsNullOrWhiteSpace(imageSrc))
            {
                imageSrc += "&usecustomfunctions=1&centercrop=1";
            }

            ImageFrame.Src = imageSrc;
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            SheerResponse.SetDialogValue(this.TextBoxCoordinate.Value);
            base.OnOK(sender, args);
        }
    }
}