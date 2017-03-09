using System.Collections.Specialized;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.Sheer;
using Vhs.ImageCoordinatePickerField.Constants;
using Vhs.ImageCoordinatePickerField.Services;
using SC = Sitecore;

namespace Vhs.ImageCoordinatePickerField.Fields
{
    public class ImageCoordinateField : Text, IContentField
    {
        public string GetValue()
        {
            return this.Value;
        }

        public void SetValue(string value)
        {
            this.Value = value;
        }

        public override void HandleMessage(Message message)
        {
            base.HandleMessage(message);

            if (!message.Name.Equals("vhs:PickCoordinate"))
                return;

            // manage to get the container id (Sitecore item id) of this field
            // onFocus attribute's value will be like this
            // javascript:return scContent.activate(this,event,'sitecore://master/{6DB97A83-480E-4F51-A9D5-B6C688C651B3}?lang=en&ver=1&fld={AA29FFC7-DEA0-4053-87A3-967C198AB652}&ctl=FIELD313682656')
            // in this case, container id = 6DB97A83-480E-4F51-A9D5-B6C688C651B3
            var onFocus = this.Attributes["onfocus"];

            var containerId = onFocus.Split('?')[0].Split('{')[1].Replace("}", string.Empty);

            SC.Context.ClientPage.Start(
                this,
                "PickCoordinate",
                new NameValueCollection { { QueryStringKeys.ContainerId, containerId } });
        }

        protected void PickCoordinate(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (args.HasResult && args.Result != this.Value)
                {
                    this.SetModified();
                    this.SetValue(args.Result);
                }
            }
            else
            {
                var uri = string.Format(
                    "{0}&{1}={2}&{3}={4}",
                    SC.UIUtil.GetUri("control:ImageCoordinatePickerDialog"),
                    QueryStringKeys.Value,
                    this.GetValue(),
                    QueryStringKeys.ContainerId,
                    args.Parameters[QueryStringKeys.ContainerId]);
                SheerResponse.ShowModalDialog(uri, ConfigurationService.DialogWidth, ConfigurationService.DialogHeight,
                    string.Empty, true);
                args.WaitForPostBack();
            }
        }
    }
}