// RaceDatabase.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.IO;
using System.Windows;
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


                #region traceRacesXML_To_CSV

                bool _traceRacesXML = true;  // file is writen while starting a game -> Federation -> Start

                if (_traceRacesXML == true)
                {
                    var pathOutputFile = "./lib/";  // instead of ./Resources/Data/
                    var separator = ";";
                    var line = "";
                    StreamWriter streamWriter;
                    var file = "./lib/testCiv.txt";
                    streamWriter = new StreamWriter(file);
                    String strHeader = "";  // first line of output files

                    try // avoid hang up if this file is opened by another program 
                    {
                        file = pathOutputFile + "FromRacesXML_(autoCreated).csv";

                        Console.WriteLine("writing {0}", file);

                        if (file == null)
                            goto WriterClose;

                        streamWriter = new StreamWriter(file);

                        strHeader =    // Head line
                                            "CE_Race" + separator +
                                            "ATT_Key" + separator +

                                            "CE_Name" + separator +
                                            "CE_SingularName" + separator +
                                            "CE_PluralName" + separator +
                                            //"CE_RACE_CombatEffectiveness" + separator +
                                            //"CE_Color" + separator +
                                            //"CE_HomeQuadrant" + separator +
                                            //"CE_RaceType" + separator +
                                            //"CE_IndustryToCredits" + separator +
                                            //"CE_TechCurve" + separator +
                                            //"CE_ShipPrefix" + separator +
                                            //"CE_Description"                 // delivers a lot of new line breaks...
                                            "CE_HomePlanetType"
                                            ;

                        streamWriter.WriteLine(strHeader);
                        // End of head line


                        GameLog.Core.GameData.DebugFormat("begin writing FromRacesXML_(autoCreated).csv ... beware of NO dismatch of Keys between Civ..xml and Races.xml");
                        string RaceName = "";
                        foreach (var race in raceDatabase)   // each race
                        {
                            //App.DoEvents();  // for avoid error after 60 seconds

                            try
                            {
                                RaceName = race.Name;   // missing: check for civs   (code was just CopyPaste from civ at the moment)
                                
                            }
                            catch
                            {
                                string message = "check whether all race entries in Races.xml exists, last line was: " + line;
                                // Supremacy Style:   var result = MessageDialog.Show(message, MessageDialogButtons.OK);
                                MessageBox.Show(message, "WARNING", MessageBoxButton.OK);
                            }

                            line =
                            "Race" + separator +
                            //race.Key + separator +

                            //race.Race.Name + separator +  // Race and others are NOT a string !!   it's a Race type

                            race.Key + separator +
                            race.Name + separator +
                            race.SingularName + separator +
                            race.PluralName + separator +
                            //race.Description + separator +
                            //race.HomeQuadrant.ToString() + separator +
                            //race.RaceType.ToString() + separator +
                            //race.IndustryToCreditsConversionRatio.ToString() + separator +
                            //race.TechCurve.ToString() + separator +
                            //race.ShipPrefix + separator +
                            //race.Description;           // delivers a lot of new line breaks...
                            race.HomePlanetType;

                            // Debug only
                            //GameLog.Core.GameData.DebugFormat("raceLine = {0}", line);
                            //Console.WriteLine(line);



                            //Console.WriteLine("{0}", line);

                            streamWriter.WriteLine(line);
                        }
                    WriterClose:
                        streamWriter.Close();
                    }
                    catch (Exception e)
                    {
                        GameLog.Core.GameData.Error("Cannot write ... FromRacesXML_(autoCreated).csv", e);
                    }


                }


                #endregion traceRacesXML_To_CSV

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
