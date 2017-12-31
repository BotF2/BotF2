// RaceDatabase.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Xml.Linq;
using System.Xml.Schema;

using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Entities
{
    /// <summary>
    /// Represents a collection of all races in the game, keyed by the
    /// <see cref="P:Supremacy.Entities.Race.Key"/> property.
    /// </summary>
    [Serializable]
    public sealed class RaceDatabase : Collections.KeyedCollectionBase<string, Race>
    {
        /// <summary>
        /// The default local path for the race database file.
        /// </summary>
        private const string DefaultDatabasePath = @"Resources\Data\Races.xml";

        /// <summary>
        /// The set of XML schemas needed to validate the race database.
        /// </summary>
        private static XmlSchemaSet _xmlSchemas;

        /// <summary>
        /// Constructs a new RaceDatabase
        /// </summary>
        public RaceDatabase() : base(o => o.Key) {}

        /// <summary>
        /// Saves the race database to XML.
        /// </summary>
        public void Save()
        {
            Save(ResourceManager.GetResourcePath(DefaultDatabasePath));
        }

        /// <summary>
        /// Saves the race database to XML.
        /// </summary>
        /// <param name="fileName">Name of the output file.</param>
        public void Save(string fileName)
        {
            var ns = XNamespace.Get("Supremacy:Races.xsd");
            var supremacyNamespace = XNamespace.Get("Supremacy:Supremacy.xsd");
            
            var rootElement = new XElement(
                ns + "Races",
                new XAttribute(
                    XNamespace.Xmlns + "s",
                    supremacyNamespace));
            
            var xmlDoc = new XDocument(rootElement);

            foreach (var race in this)
                race.AppendXml(rootElement);
            
            xmlDoc.Save(fileName, SaveOptions.None);
        }

        /// <summary>
        /// Loads the XML schemas.
        /// </summary>
        private static void LoadSchemas()
        {
            _xmlSchemas = new XmlSchemaSet();
            _xmlSchemas.Add("Supremacy:Supremacy.xsd",
                           "vfs:///Resources/Data/Supremacy.xsd");
            _xmlSchemas.Add("Supremacy:Races.xsd",
                           "vfs:///Resources/Data/Races.xsd");
            _xmlSchemas.Add("Supremacy:Civilizations.xsd",
                           "vfs:///Resources/Data/Civilizations.xsd");
        }

        /// <summary>
        /// Validates the XML.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ValidationEventArgs"/> instance containing the event data.</param>
        private static void ValidateXml(object sender, ValidationEventArgs e)
        {
            XmlHelper.ValidateXml(DefaultDatabasePath, e);
        }

        /// <summary>
        /// Loads the race database from XML.
        /// </summary>
        /// <returns>The race database.</returns>
        public static RaceDatabase Load()
        {
            try
            {
                var raceDatabase = new RaceDatabase();

                var ns = XNamespace.Get("Supremacy:Races.xsd");
                var xmlDoc = XDocument.Load(ResourceManager.GetResourcePath(DefaultDatabasePath));

                if (_xmlSchemas == null)
                    LoadSchemas();

                xmlDoc.Validate(_xmlSchemas, ValidateXml, true);

                foreach (var raceElement in xmlDoc.Root.Elements(ns + "Race"))
                    raceDatabase.Add(new Race(raceElement));

                return raceDatabase;
            }
            catch (SupremacyException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SupremacyException(
                    "An error occurred while loading the Race Database: " + e.Message,
                    SupremacyExceptionAction.Disconnect);
            }
        }
    }
}
