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

            VfsUriHelper.ValidateAndGetVfsUriComponents(uri, out Uri sourceUri, out Uri resourceUri);

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

                return virtualFileInfo != null;
            }

            virtualFileInfo = vfsService.GetFile(virtualPath);

            return virtualFileInfo != null;
        }
    }
}