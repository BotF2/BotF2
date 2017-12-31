using System;

using Supremacy.Annotations;

namespace Supremacy.Resources
{
    public interface IResourceManager
    {
        string GetString(string key);
        string GetResourcePath(string path);
        string GetGameResourcePath(string path);
        string GetSystemResourcePath(string path);
        Uri GetResourceUri(string path);
    }

    public static class ResourceManagerExtensions
    {
        public static string GetStringFormat([NotNull] this IResourceManager self, [NotNull] string key, params object[] args)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (key == null)
                throw new ArgumentNullException("key");

            return String.Format(self.GetString(key), args);
        }
    }
}