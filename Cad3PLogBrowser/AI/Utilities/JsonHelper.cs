using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Cad3PLogBrowser.AI.Utilities
{
    /// <summary>
    /// Simple JSON serialization helper using DataContractJsonSerializer.
    /// Provides a compatible interface for the AI framework without requiring Newtonsoft.Json.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Serializes an object to JSON string.
        /// </summary>
        public static string Serialize<T>(T obj)
        {
            if (obj == null)
                return "null";

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, obj);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON Serialization error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deserializes a JSON string to an object.
        /// </summary>
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json) || json == "null")
                return default(T);

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    return (T)serializer.ReadObject(ms);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON Deserialization error: {ex.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Serializes a dictionary to JSON manually (for simple cases).
        /// </summary>
        public static string SerializeDictionary(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0)
                return "{}";

            var sb = new StringBuilder();
            sb.Append("{");

            bool first = true;
            foreach (var kvp in dict)
            {
                if (!first) sb.Append(",");
                first = false;

                sb.Append("\"");
                sb.Append(EscapeString(kvp.Key));
                sb.Append("\":");
                sb.Append(SerializeValue(kvp.Value));
            }

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// Serializes a list to JSON manually.
        /// </summary>
        public static string SerializeList<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                return "[]";

            var sb = new StringBuilder();
            sb.Append("[");

            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(SerializeValue(list[i]));
            }

            sb.Append("]");
            return sb.ToString();
        }

        private static string SerializeValue(object value)
        {
            if (value == null)
                return "null";

            if (value is string)
                return "\"" + EscapeString(value.ToString()) + "\"";

            if (value is bool)
                return value.ToString().ToLower();

            if (value is int || value is long || value is double || value is float || value is decimal)
                return value.ToString();

            if (value is Dictionary<string, object>)
                return SerializeDictionary((Dictionary<string, object>)value);

            if (value is Dictionary<string, string>)
            {
                var dict = (Dictionary<string, string>)value;
                var objDict = new Dictionary<string, object>();
                foreach (var kvp in dict)
                    objDict[kvp.Key] = kvp.Value;
                return SerializeDictionary(objDict);
            }

            if (value is DateTime)
                return "\"" + ((DateTime)value).ToString("o") + "\"";

            // Handle any IEnumerable (lists, arrays, etc.)
            if (value is System.Collections.IEnumerable && !(value is string))
            {
                var sb = new StringBuilder();
                sb.Append("[");
                bool first = true;
                foreach (var item in (System.Collections.IEnumerable)value)
                {
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append(SerializeValue(item));
                }
                sb.Append("]");
                return sb.ToString();
            }

            // Fallback: try DataContractJsonSerializer
            try
            {
                var serializer = new DataContractJsonSerializer(value.GetType());
                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, value);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch
            {
                return "\"" + EscapeString(value.ToString()) + "\"";
            }
        }

        private static string EscapeString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        /// <summary>
        /// Simple JSON parsing for responses.
        /// Returns a dictionary of key-value pairs.
        /// </summary>
        public static Dictionary<string, string> ParseSimpleJson(string json)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(json))
                return result;

            json = json.Trim();
            if (!json.StartsWith("{") || !json.EndsWith("}"))
                return result;

            json = json.Substring(1, json.Length - 2); // Remove { }

            var parts = SplitJsonPairs(json);
            foreach (var part in parts)
            {
                var colonIndex = part.IndexOf(':');
                if (colonIndex > 0)
                {
                    var key = part.Substring(0, colonIndex).Trim().Trim('"');
                    var value = part.Substring(colonIndex + 1).Trim().Trim('"');
                    result[key] = UnescapeString(value);
                }
            }

            return result;
        }

        private static List<string> SplitJsonPairs(string json)
        {
            var pairs = new List<string>();
            var current = new StringBuilder();
            int depth = 0;
            bool inString = false;
            bool escaped = false;

            foreach (char c in json)
            {
                if (escaped)
                {
                    current.Append(c);
                    escaped = false;
                    continue;
                }

                if (c == '\\')
                {
                    escaped = true;
                    current.Append(c);
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    current.Append(c);
                    continue;
                }

                if (!inString)
                {
                    if (c == '{' || c == '[') depth++;
                    if (c == '}' || c == ']') depth--;

                    if (c == ',' && depth == 0)
                    {
                        pairs.Add(current.ToString());
                        current.Clear();
                        continue;
                    }
                }

                current.Append(c);
            }

            if (current.Length > 0)
                pairs.Add(current.ToString());

            return pairs;
        }

        private static string UnescapeString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t");
        }
    }
}
