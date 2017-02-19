using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Vhs.ContentAuditTool.Extensions
{
    public static class ItemExtensions
    {
        public static List<Item> GetMultipleItemSelectionsFromField(this Item item, string fieldName, bool onlySameLanguage = true)
        {
            var result = new List<Item>();

            if (item != null && !string.IsNullOrWhiteSpace(fieldName))
            {
                MultilistField multiSelectionField = item.Fields[fieldName];
                if (multiSelectionField != null && multiSelectionField.Items.HasItems())
                    result = onlySameLanguage
                        ? multiSelectionField.GetLanguageItems(item.Language.Name).ToList()
                        : multiSelectionField.GetItems().ToList();
            }

            return result;
        }

        public static Item GetSpecificLanguageVersion(this Item item, string langName)
        {
            if (item != null)
            {
                // check if we already have the right version
                if (item.Language.Name.Equals(langName) && item.IsValidLanguageVersion())
                    return item;

                var db = item.Database;
                var languages = db.Languages;
                System.Diagnostics.Debug.Assert(languages.Any(l => l.Name.Equals(langName)), string.Format(" language {0} does not exist", langName));
                return languages
                    .Where(lang => lang.Name.Equals(langName))
                    .Select(lang => db.GetItem(item.ID, lang))
                    .FirstOrDefault(langSpecificItem => langSpecificItem.IsValidLanguageVersion());
            }
            return null;
        }

        public static bool IsValidLanguageVersion(this Item itemLangVersion)
        {
            return itemLangVersion != null && itemLangVersion.Versions.Count > 0;
        }
    }
}