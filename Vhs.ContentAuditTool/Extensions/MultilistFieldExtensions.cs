using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Vhs.ContentAuditTool.Extensions
{
    public static class MultilistFieldExtensions
    {
        public static Item[] GetLanguageItems(this MultilistField field, string langName)
        {
            return field.GetItems().Select(item => item.GetSpecificLanguageVersion(langName)).Where(langItem => langItem != null).ToArray();
        }
    }
}