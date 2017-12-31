using System;

using Supremacy.Annotations;

namespace Supremacy.VFS
{
    ///<summary>
    /// Provides extension methods for the <see cref="IFilesSource"/> interface.
    ///</summary>
    public static class FilesSourceExtensions
    {
        /// <summary>
        /// Gets the metadata for the specified virtual file.
        /// </summary>
        /// <param name="source">The source <see cref="IFilesSource"/> instance.</param>
        /// <param name="path">The virtual path of the file.</param>
        /// <returns>The metadata for the specified virtual file.</returns>
        public static IVirtualFileInfo GetFileInfo([NotNull] this IFilesSource source, string path)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return source.GetFileInfo(path, false);
        }
    }
}