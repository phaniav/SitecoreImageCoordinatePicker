using System.Collections.Generic;

namespace Vhs.ContentAuditTool.Extensions
{
    public static class CollectionExtensions
    {
        public static bool HasItems<T>(this ICollection<T> collection)
        {
            return collection != null && collection.Count != 0;
        }
    }
}