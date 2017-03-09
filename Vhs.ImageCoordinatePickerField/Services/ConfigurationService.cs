using SC = Sitecore;

namespace Vhs.ImageCoordinatePickerField.Services
{
    public static class ConfigurationService
    {
        public static string DialogWidth
            => SC.Configuration.Settings.GetSetting("Vhs.ImageCoordinatePickerField.DialogWidth", "800");

        public static string DialogHeight
            => SC.Configuration.Settings.GetSetting("Vhs.ImageCoordinatePickerField.DialogHeight", "600");

        public static string ImageFieldName
            =>
                SC.Configuration.Settings.GetSetting("Vhs.ImageCoordinatePickerField.ImageFieldName",
                    "Hero Image");

        public static string ImageAlternateText
            =>
                SC.Configuration.Settings.GetSetting("Vhs.ImageCoordinatePickerField.ImageAlternateText",
                    "There is NO image in the parent item.");
    }
}