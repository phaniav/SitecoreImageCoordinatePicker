namespace Vhs.ContentAuditTool.Extensions
{
    public static class StringExtensions
    {
        public static bool HasText(this string source)
        {
            return !string.IsNullOrWhiteSpace(source);
        }
    }
}