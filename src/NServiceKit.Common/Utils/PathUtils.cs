using System;
using System.IO;
using System.Text;

namespace NServiceKit.Common.Utils
{
    /// <summary>A path utilities.</summary>
    public static class PathUtils
    {
        /// <summary>Maps the path of a file in a self-hosted scenario.</summary>
        ///
        /// <param name="relativePath">             the relative path.</param>
        /// <param name="appendPartialPathModifier">The append partial path modifier.</param>
        ///
        /// <returns>the absolute path.</returns>
        public static string MapAbsolutePath(string relativePath, string appendPartialPathModifier)
        {
#if !SILVERLIGHT 
            if (relativePath.StartsWith("~"))
            {
                var assemblyDirectoryPath = Path.GetDirectoryName(new Uri(typeof(PathUtils).Assembly.EscapedCodeBase).LocalPath);

                // Escape the assembly bin directory to the hostname directory
                var hostDirectoryPath = appendPartialPathModifier != null
                                            ? assemblyDirectoryPath + appendPartialPathModifier
                                            : assemblyDirectoryPath;

                return Path.GetFullPath(relativePath.Replace("~", hostDirectoryPath));
            }
#endif
            return relativePath;
        }

        /// <summary>
        /// Maps the path of a file in the context of a VS project
        /// </summary>
        /// <param name="relativePath">the relative path</param>
        /// <returns>the absolute path</returns>
        /// <remarks>Assumes static content is two directories above the /bin/ directory,
        /// eg. in a unit test scenario  the assembly would be in /bin/Debug/.</remarks>
        public static string MapProjectPath(this string relativePath)
        {
            var mapPath = MapAbsolutePath(relativePath, string.Format("{0}..{0}..", Text.StringExtensions.DirSeparatorChar));
            return mapPath;
        }

        /// <summary>
        /// Maps the path of a file in a self-hosted scenario
        /// </summary>
        /// <param name="relativePath">the relative path</param>
        /// <returns>the absolute path</returns>
        /// <remarks>Assumes static content is copied to /bin/ folder with the assemblies</remarks>
        public static string MapAbsolutePath(this string relativePath)
        {
            var mapPath = MapAbsolutePath(relativePath, null);
            return mapPath;
        }

        /// <summary>
        /// Maps the path of a file in an Asp.Net hosted scenario
        /// </summary>
        /// <param name="relativePath">the relative path</param>
        /// <returns>the absolute path</returns>
        /// <remarks>Assumes static content is in the parent folder of the /bin/ directory</remarks>
        public static string MapHostAbsolutePath(this string relativePath)
        {
            var mapPath = MapAbsolutePath(relativePath, string.Format("{0}..", Text.StringExtensions.DirSeparatorChar));
            return mapPath;
        }

        internal static string CombinePaths(StringBuilder sb, params string[] paths)
        {
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path))
                    continue;

                if (sb.Length > 0 && sb[sb.Length - 1] != '/')
                    sb.Append("/");

                sb.Append(path.Replace('\\', '/').TrimStart('/'));
            }

            return sb.ToString();
        }

        /// <summary>Combine paths.</summary>
        ///
        /// <param name="paths">A variable-length parameters list containing paths.</param>
        ///
        /// <returns>A string.</returns>
        public static string CombinePaths(params string[] paths)
        {
            return CombinePaths(new StringBuilder(), paths);
        }

        /// <summary>A string extension method that assert dir.</summary>
        ///
        /// <param name="dirPath">The dirPath to act on.</param>
        ///
        /// <returns>A string.</returns>
        public static string AssertDir(this string dirPath)
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            return dirPath;
        }
    }


}
