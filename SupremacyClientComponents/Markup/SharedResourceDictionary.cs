using System;
using System.Windows;

using Supremacy.Collections;
using Supremacy.Utility;

namespace Supremacy.Client.Markup
{
    public class SharedResourceDictionary : ResourceDictionary
    {
        /// <summary>
        /// Internal cache of loaded dictionaries 
        /// </summary>
        private static readonly WeakDictionary<Uri, ResourceDictionary> SharedDictionaries = new WeakDictionary<Uri, ResourceDictionary>();

        /// <summary>
        /// Local member of the source uri
        /// </summary>
        private Uri _sourceUri;

        /// <summary>
        /// Gets or sets the uniform resource identifier (URI) to load resources from.
        /// </summary>
        public new Uri Source
        {
            get { return _sourceUri; }
            set
            {
                bool _tracingSharedResourceDictionary = false;

                _sourceUri = value;

                if (_tracingSharedResourceDictionary)
                    GameLog.Client.GameData.DebugFormat("SharedResourceDictionary.cs: _sourceUri={0}", value);

                ResourceDictionary sharedDictionary;

                if (SharedDictionaries.TryGetValue(value, out sharedDictionary))
                {
                    // If the dictionary is already loaded, get it from the cache
                    MergedDictionaries.Add(sharedDictionary);
                }
                else
                {
                    // If the dictionary is not yet loaded, load it by setting
                    // the source of the base class
                    base.Source = value;

                    // add it to the cache
                    SharedDictionaries.Add(value, this);

                    if (_tracingSharedResourceDictionary)
                        GameLog.Print("SharedResourceDictionary.cs: Count={0}, Added to sharedDictionary={1}", SharedDictionaries.Count, value);
                }
            }
        }
    }
}