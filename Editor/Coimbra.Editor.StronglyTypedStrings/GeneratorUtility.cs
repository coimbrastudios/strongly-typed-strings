using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Coimbra.StronglyTypedStrings
{
    /// <summary>
    /// General utility class to use on your own <see cref="GeneratorUnit"/>.
    /// </summary>
    public static class GeneratorUtility
    {
        private static readonly string[] InvalidChars;

        static GeneratorUtility()
        {
            var invalidChars = new List<string> { "." };

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                var s = c.ToString();

                if (string.IsNullOrEmpty(s) || s == "\\" || s == "/")
                {
                    continue;
                }

                invalidChars.Add(s);
            }

            InvalidChars = invalidChars.ToArray();
        }

        /// <summary>
        /// Get a valid folder path based on the received path.
        /// </summary>
        public static string GetValidFolderPath(string path, string defaultValue = "")
        {
            foreach (string invalidChar in InvalidChars)
            {
                path = path.Replace(invalidChar, string.Empty);

                if (path == "")
                {
                    return defaultValue;
                }
            }

            while (path[0] == '\\' || path[0] == '/')
            {
                path = path.Remove(0, 1);

                if (path == "")
                {
                    return defaultValue;
                }
            }

            while (path[path.Length - 1] == '\\' || path[path.Length - 1] == '/')
            {
                path = path.Remove(path.Length - 1);

                if (path == "")
                {
                    return defaultValue;
                }
            }

            return path.Replace("\\", "/");
        }

        /// <summary>
        /// Get a valid script member name based on the received name.
        /// </summary>
        public static string GetValidMemberName(string name, string defaultValue = "_")
        {
            if (string.IsNullOrEmpty(name))
            {
                return defaultValue;
            }

            name = name.Replace("-", "_");
            name = name.Replace("\\", "_");
            name = name.Replace("/", "_");
            name = Regex.Replace(name, "[^a-zA-Z0-9_]", "", RegexOptions.Compiled);

            if (string.IsNullOrEmpty(name))
            {
                return defaultValue;
            }

            return char.IsDigit(name[0]) ? $"_{name}" : name;
        }

        /// <summary>
        /// Get a valid script namespace name based on the received name.
        /// </summary>
        public static string GetValidNamespaceName(string name, string defaultValue = "")
        {
            name = Regex.Replace(name, "[^a-zA-Z0-9_.]", "", RegexOptions.Compiled);

            while (string.IsNullOrEmpty(name) == false)
            {
                if (char.IsDigit(name[0]))
                {
                    name = name.Remove(0, 1);
                }
                else
                {
                    return name;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Check if the search matches the given target.
        /// </summary>
        public static bool TryMatchSearch(string searchContext, string target)
        {
            if (searchContext == null)
            {
                return true;
            }

            searchContext = searchContext.Trim();

            if (string.IsNullOrEmpty(searchContext))
            {
                return true;
            }

            string[] split = searchContext.Split(' ');

            foreach (string value in split)
            {
                if (!string.IsNullOrEmpty(value) && target.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
