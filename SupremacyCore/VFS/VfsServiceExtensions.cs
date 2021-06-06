using System;

namespace Supremacy.VFS
{
    public static class VfsServiceExtensions
    {
        static VfsServiceExtensions()
        {
            VfsWebRequestFactory.EnsureRegistered();
        }

        public static bool TryGetFileInfo(this IVfsService vfsService, Uri uri, out IVirtualFileInfo virtualFileInfo)
        {
            Uri sourceUri;
            Uri resourceUri;

            VfsUriHelper.ValidateAndGetVfsUriComponents(uri, out sourceUri, out resourceUri);

            string virtualPath = resourceUri.ToString().Substring(1);

            if (sourceUri != null)
            {
                string sourceName = sourceUri.Scheme;

                IFilesSource source = vfsService.GetSource(sourceName);
                if (source == null)
                {
                    throw new ArgumentException(
                        string.Format(
                            "VFS source '{0}' not found.",
                            sourceName));
                }

                virtualFileInfo = source.GetFileInfo(virtualPath, true);

                return (virtualFileInfo != null);
            }

            virtualFileInfo = vfsService.GetFile(virtualPath);

            return (virtualFileInfo != null);
        }
    }
}