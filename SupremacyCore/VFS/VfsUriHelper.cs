using System;
using System.Diagnostics;

namespace Supremacy.VFS
{
    public static class VfsUriHelper
    {
        private static readonly Uri _defaultUri = new Uri("http://defaultcontainer/");

        internal static void ValidateAndGetVfsUriComponents(Uri vfsUri, out Uri sourceUri, out Uri resourceUri)
        {
            //Validate if its not null and is an absolute Uri, has pack:// Scheme. 
            vfsUri = ValidateVfsUri(vfsUri);
            sourceUri = GetSourceUriComponent(vfsUri);
            resourceUri = GetFileUriComponent(vfsUri);
        }

        private static Uri ValidateVfsUri(Uri vfsUri)
        {
            if (vfsUri == null)
            {
                throw new ArgumentNullException("vfsUri");
            }

            if (!vfsUri.IsAbsoluteUri)
            {
                throw new ArgumentException("Uri should be absolute.");
            }

            if (vfsUri.Scheme != VfsService.UriScheme)
            {
                throw new ArgumentException(
                    string.Format(
                        "Uri should have '{0}' scheme.",
                        VfsService.UriScheme));
            }

            return vfsUri;
        }

        /// <summary>
        ///   This method parses the pack uri and returns the inner
        ///   Uri that points to the package as a whole.
        /// </summary>
        /// <param name = "vfsUri">Uri which has pack:// scheme</param>
        /// <returns>Returns the inner uri that points to the entire package</returns>
        /// <exception cref = "ArgumentNullException">If vfsUri parameter is null</exception>
        /// <exception cref = "ArgumentException">If vfsUri parameter is not an absolute Uri</exception>
        /// <exception cref = "ArgumentException">If vfsUri parameter does not have "pack://" scheme</exception>
        /// <exception cref = "ArgumentException">If inner sourceUri extracted from the vfsUri has a fragment component</exception>
        public static Uri GetSourceUri(Uri vfsUri)
        {
            //Parameter Validation is done in the follwoing method
            ValidateAndGetVfsUriComponents(vfsUri, out Uri sourceUri, out Uri resourceUri);

            return sourceUri;
        }

        /// <summary>
        ///   This method parses the pack uri and returns the absolute
        ///   path of the URI. This corresponds to the resource within the 
        ///   package. This corresponds to the absolute path component in
        ///   the Uri. If there is no resource component present, this method
        ///   returns a null
        /// </summary>
        /// <param name = "vfsUri">Returns a relative Uri that represents the
        ///   resource within the package. If the pack Uri points to the entire 
        ///   package then we return a null</param>
        /// <returns>Returns a relative URI with an absolute path that points to a resource within a package</returns>
        /// <exception cref = "ArgumentNullException">If packUri parameter is null</exception>
        /// <exception cref = "ArgumentException">If packUri parameter is not an absolute Uri</exception>
        /// <exception cref = "ArgumentException">If packUri parameter does not have "pack://" scheme</exception>
        /// <exception cref = "ArgumentException">If partUri extracted from packUri does not conform to the valid partUri syntax</exception>
        public static Uri GetPartUri(Uri vfsUri)
        {

            //Parameter Validation is done in the follwoing method 
            ValidateAndGetVfsUriComponents(vfsUri, out Uri sourceUri, out Uri resourceUri);

            return resourceUri;
        }

        private static Uri GetSourceUriComponent(Uri vfsUri)
        {
            Debug.Assert(vfsUri != null, "vfsUri parameter cannot be null");

            //Step 1 - Get the authority part of the URI. This section represents that package URI
            string hostAndPort = vfsUri.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);

            //Step 2 - Replace the ',' with '/' to reconstruct the package URI
            hostAndPort = hostAndPort.Replace(',', '/');

            if (string.IsNullOrWhiteSpace(hostAndPort))
            {
                return null;
            }

            //Step 3 - Unescape the special characters that we had escaped to construct the vfsUri
            Uri sourceUri = new Uri(Uri.UnescapeDataString(hostAndPort));
            if (sourceUri.Fragment != string.Empty)
            {
                throw new ArgumentException(@"Uri cannot have a fragment.", "vfsUri");
            }

            return sourceUri;
        }

        private static Uri GetFileUriComponent(Uri vfsUri)
        {
            Debug.Assert(vfsUri != null, "vfsUri parameter cannot be null");

            string partName = GetStringForFileUriFromAnyUri(vfsUri);

            if (partName == string.Empty)
            {
                return null;
            }

            return ValidateFileUri(new Uri(partName, UriKind.Relative));
        }

        private static string GetStringForFileUriFromAnyUri(Uri resourceUri)
        {
            Debug.Assert(
                resourceUri != null, "Null reference check for this uri parameter should have been made earlier");

            Uri safeUnescapedUri;

            // Step 1: Get the safe-unescaped form of the URI first. This will unescape all the characters
            // that can be safely un-escaped, unreserved characters, unicode characters, etc. 
            if (!resourceUri.IsAbsoluteUri)
            {
                //We assume a well formed Resource URI has been passed to this method 
                safeUnescapedUri = new Uri(
                    resourceUri.GetComponents(
                        UriComponents.SerializationInfoString,
                        UriFormat.SafeUnescaped),
                    UriKind.Relative);
            }
            else
            {
                safeUnescapedUri = new Uri(
                    resourceUri.GetComponents(
                        UriComponents.Path | UriComponents.KeepDelimiter,
                        UriFormat.SafeUnescaped),
                    UriKind.Relative);
            }

            // Step 2: Get the canonically escaped Path with only ascii characters
            //Get the escaped string for the part name as part names should have only ascii characters 
            string partName = safeUnescapedUri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);

            //The part name can be empty in cases where we were passed a pack URI that has no part component
            if (IsPartNameEmpty(partName))
            {
                return string.Empty;
            }

            return partName;
        }

        private static bool IsPartNameEmpty(string partName)
        {
            Debug.Assert(
                partName != null, "Null reference check for this partName parameter should have been made earlier");

            // Uri.GetComponents may return a single forward slash when there is no absolute path.
            // This is Whidbey PS399695.  Until that is changed, we check for both cases - either an entirely empty string, 
            // or a single forward slash character.  Either case means there is no part name.
            if (partName.Length == 0 || ((partName.Length == 1) && (partName[0] == '/')))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///   This method is used to validate a Resource URI
        ///   This method does not perform a case sensitive check of the Uri
        /// </summary>
        /// <param name = "resourceUri">The string that represents the part within a package</param>
        /// <returns>Returns the Resource URI if it is valid</returns>
        /// <exception cref = "ArgumentNullException">If resourceUri parameter is null</exception>
        /// <exception cref = "ArgumentException">If resourceUri parameter is an absolute Uri</exception>
        /// <exception cref = "ArgumentException">If resourceUri parameter is empty</exception>
        /// <exception cref = "ArgumentException">If resourceUri parameter does not start with a "/"</exception>
        /// <exception cref = "ArgumentException">If resourceUri parameter starts with two "/"</exception>
        /// <exception cref = "ArgumentException">If resourceUri parameter ends with a "/"</exception>
        /// <exception cref = "ArgumentException">If resourceUri parameter has a fragment</exception>
        /// <exception cref = "ArgumentException">If resourceUri parameter has some escaped characters that should not be escaped 
        ///   or some characters that should be escaped are not escaped.</exception>
        internal static Uri ValidateFileUri(Uri resourceUri)
        {

            Exception exception = GetExceptionIfFileUriInvalid(resourceUri, out string fileUriString);
            if (exception != null)
            {
                Debug.Assert(fileUriString != null && fileUriString.Length == 0);
                throw exception;
            }

            Debug.Assert(!string.IsNullOrEmpty(fileUriString));

            return new Uri(fileUriString, UriKind.Relative);
        }

        private static Exception GetExceptionIfFileUriInvalid(Uri resourceUri, out string fileUriString)
        {
            fileUriString = string.Empty;

            if (resourceUri == null)
            {
                return new ArgumentNullException("resourceUri");
            }

            if (resourceUri.IsAbsoluteUri)
            {
                return new ArgumentException("Resource URI cannot be an absolute URI.");
            }

            string partName = GetStringForFileUriFromAnyUri(resourceUri);

            //We need to make sure that the URI passed to us is not just "/"
            //"/" is a valid relative uri, but is not a valid partname
            if (partName == string.Empty)
            {
                return new ArgumentException("Resource URI is empty.");
            }

            if (partName[0] != '/')
            {
                return new ArgumentException("Resource URI should start with a forward slash.");
            }

            if (partName.StartsWith("//"))
            {
                return new ArgumentException("Resource URI should not start with two slashes.");
            }

            if (partName[partName.Length - 1] == '/')
            {
                return new ArgumentException("Resource URI should not end with a slash.");
            }

            if (resourceUri.IsAbsoluteUri && !string.IsNullOrWhiteSpace(resourceUri.Fragment))
            {
                return new ArgumentException("Resource URI should not have a fragment.");
            }

            //We test if the URI is wellformed and refined.
            //The relative URI that was passed to us may not be correctly escaped and so we test that. 
            //Also there might be navigation "/../" present in the URI which we need to detect.
            string wellFormedPartName = new Uri(_defaultUri, partName).GetComponents(
                UriComponents.Path |
                UriComponents.KeepDelimiter,
                UriFormat.UriEscaped);

            //Note - For Relative Uris the output of ToString() and OriginalString property
            //are the same as per the current implementation of System.Uri
            //Need to use OriginalString property or ToString() here as we are want to
            //validate that the input uri given to us was valid in the first place. 
            //We do not want to use GetComponents in this case as it might lead to
            //performing escaping or unescaping as per the UriFormat enum value and that 
            //may alter the string that the user created the Uri with and we may not be able 
            //to verify the uri correctly.
            //We perform the comparison in a case-insensitive manner, as at this point, 
            //only escaped hex digits (A-F) might vary in casing.
            if (string.Compare(resourceUri.ToString(), wellFormedPartName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return new ArgumentException("Invalid Resource URI.");
            }

            //if we get here, the resourceUri is valid and so we return null, as there is no exception.
            fileUriString = partName;
            return null;
        }
    }
}