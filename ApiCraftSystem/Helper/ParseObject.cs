using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using PathSegment = ApiCraftSystem.Repositories.ApiServices.Dtos.PathSegment;
namespace ApiCraftSystem.Helper
{
    public static class ParseObject
    {
        // Parse "data.cols[*].id" into list of PathSegment
        public static List<PathSegment> ParsePathToSegments(string path)
        {
            return path.Split('.').Select(part =>
            {
                if (part.EndsWith("[*]"))
                {
                    return new PathSegment
                    {
                        Key = part.Replace("[*]", ""),
                        Map = "*"
                    };
                }
                return new PathSegment { Key = part };
            }).ToList();
        }


        public static List<JToken> GetRootArray(JToken root, string basePath)
        {
            try
            {
                var normalized = basePath.Replace("[*]", "");
                var token = root.SelectToken(normalized);

                return token switch
                {
                    JArray arr => arr.ToList(),
                    JObject obj => new List<JToken> { obj },
                    null => new List<JToken>(),
                    _ => new List<JToken> { token }
                };
            }
            catch
            {
                return new List<JToken>();
            }
        }


        public static object? GetTokenFromAny(JToken token, string path)
        {
            try
            {
                // Remove wildcards before extracting single value
                var normalizedPath = path.Replace("[*]", "");

                var selected = token.SelectToken(normalizedPath);
                return selected?.Type switch
                {
                    JTokenType.Integer => selected.Value<int>(),
                    JTokenType.Float => selected.Value<double>(),
                    JTokenType.Boolean => selected.Value<bool>(),
                    JTokenType.Date => selected.Value<DateTime>(),
                    JTokenType.String => selected.Value<string>(),
                    _ => selected?.ToString()
                };
            }
            catch
            {
                return null;
            }
        }

        public static string ExtractRootPath(string fullPath)
        {
            // Example: "data.items[*].id" → "data.items[*]"
            var segments = fullPath.Split('.');
            for (int i = segments.Length - 1; i >= 0; i--)
            {
                if (segments[i].Contains("[*]") || segments[i].Contains("[")) break;
                segments = segments.Take(i).ToArray();
            }
            return string.Join('.', segments);
        }


        public static List<object?> ResolveWildcardValues(JToken token, string path)
        {
            var normalizedPath = path.Replace("[*]", "");

            // If path contains space or special characters → wrap it
            if (normalizedPath.Contains(" "))
            {
                normalizedPath = $"['{normalizedPath}']";
            }
            var selectedTokens = token.SelectTokens(normalizedPath).ToList();

            var results = new List<object?>();
            foreach (var t in selectedTokens)
            {
                if (t == null || t.Type == JTokenType.Null) continue;

                results.Add(t.Type switch
                {
                    JTokenType.Integer => t.Value<int>(),
                    JTokenType.Float => t.Value<double>(),
                    JTokenType.Boolean => t.Value<bool>(),
                    JTokenType.Date => t.Value<DateTime>(),
                    JTokenType.String => t.Value<string>(),
                    _ => t.ToString()
                });
            }

            return results;
        }

        public static string GetLastCleanSegment(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            var segments = path.Split('.');
            var last = segments.LastOrDefault();

            if (string.IsNullOrEmpty(last)) return string.Empty;

            // Remove array brackets, e.g., "[*]", "[0]"
            var bracketIndex = last.IndexOf('[');
            return bracketIndex > -1 ? last.Substring(0, bracketIndex) : last;
        }


    }
}
