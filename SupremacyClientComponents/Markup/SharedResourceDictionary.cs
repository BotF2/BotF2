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
                _sourceUri = value;

                //works 
                GameLog.Client.UI.DebugFormat("SharedResourceDictionary.cs: _sourceUri={0}", value);

                if (SharedDictionaries.TryGetValue(value, out ResourceDictionary sharedDictionary))
                {
                    // If the dictionary is already loaded, get it from the cache
                    MergedDictionaries.Add(sharedDictionary);

                    if (MergedDictionaries != null)
                    {
                        //int _dicCount = MergedDictionaries.Count;
                        string _allText = Environment.NewLine + "a;b;c;d;e;(Headline for Excel);g;" + Environment.NewLine;
                        int _allValue = 0;
                        string _text0 = MergedDictionaries[0].Source.ToString();

                        foreach (ResourceDictionary item in MergedDictionaries[0].MergedDictionaries)
                        {
                            string _text1 = item.Source.ToString(); Console.WriteLine(_text1);
                            _allValue += 10000;  // 10.000 step each file

                            foreach (object key in item.Keys)
                            {
                                string _text2 = key.ToString();
                                //Console.WriteLine(_text1 + "-" +_text2);   // MergedDictionaries
                                _allValue += +1;
                                _allText += _text0 + ";" + _text1 + ";" + _allValue + ";" + _text2 + Environment.NewLine;

                            }
                        }
                        GameLog.Client.UI.DebugFormat(_allText);
                    }
                }
                else
                {
                    // If the dictionary is not yet loaded, load it by setting
                    // the source of the base class
                    base.Source = value;

                    // add it to the cache
                    SharedDictionaries.Add(value, this);

                    GameLog.Client.UI.DebugFormat("SharedResourceDictionary.cs: Count={0}, Added to sharedDictionary={1}", SharedDictionaries.Count, value);
                }
            }
        }
    }
}