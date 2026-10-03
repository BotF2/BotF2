using System;

using Supremacy.Resources;

namespace Supremacy.Client.Services
{
    public class ClientResourceManager : IResourceManager
    {
        #region Implementation of IResourceManager
        public string GetString(string key)
        {
            return ResourceManager.GetString(key);
        }
        //public string Get_DE_String(string key)
        //{
        //    return ResourceManager.Get_DE_String(key);
        //}

        //public string Get_FR_String(string key)
        //{
        //    return ResourceManager.Get_FR_String(key);
        //}

        public string GetResourcePath(string path)
        {
            return ResourceManager.GetResourcePath(path);
        }

        public string GetGameResourcePath(string path)
        {
            return ResourceManager.GetInternalResourcePath(path);
        }

        public string GetSystemResourcePath(string path)
        {
            return ResourceManager.GetSystemResourcePath(path);
        }

        public Uri GetResourceUri(string path)
        {
            return ResourceManager.GetResourceUri(path);
        }
        #endregion
    }
}