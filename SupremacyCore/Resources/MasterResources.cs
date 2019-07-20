using Supremacy.Entities;
using Supremacy.Utility;
using System;
using System.IO;
using System.Windows;

namespace Supremacy.Resources
{
    public class MasterResources
    {
        private static CivDatabase m_CivDatabase;

        public static CivDatabase CivDB
        {
            get
            {
                if (m_CivDatabase == null)
                {
                    GameLog.Core.GameData.DebugFormat("Loading master copy of civilization database...");
                    m_CivDatabase = CivDatabase.Load();
                    GameLog.Core.GameData.DebugFormat("Master civilization database loaded");

                    #region traceCivilizationsXML_To_CSV

                    bool _traceCivilizationsXML = true;  // file is writen while starting a game -> Federation -> Start
                    GameLog.Core.XML2CSVOutput.DebugFormat("{0} for writing _FromCivilizationsXML_(autoCreated).csv - may hang up a start of the game", _traceCivilizationsXML);
                    if (_traceCivilizationsXML == true)
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
                            file = pathOutputFile + "_FromCivilizationsXML_(autoCreated).csv";

                            Console.WriteLine("writing {0}", file);

                            if (file == null)
                                goto WriterClose;

                            streamWriter = new StreamWriter(file);

                            strHeader =    // Head line
                                                "CE_Civilization" + separator +
                                                "ATT_Key" + separator +

                                                "CE_Race" + separator +
                                                "CE_HomeSystemName" + separator +
                                                "CE_RACE_HomePlanetType" + separator +
                                                "CE_RACE_CombatEffectiveness" + separator +
                                                "CE_Color" + separator +
                                                "CE_HomeQuadrant" + separator +
                                                "CE_CivilizationType" + separator +
                                                "CE_IndustryToCredits" + separator +
                                                "CE_TechCurve" + separator +
                                                "CE_ShipPrefix" + separator +
                                                "CE_Traits"
                                                ;

                            streamWriter.WriteLine(strHeader);
                            // End of head line

                            GameLog.Core.GameData.InfoFormat("begin writing _FromCivilizationsXML_(autoCreated).csv ... would breaks if dismatch of Keys between Civ..xml and Races.xml");
                            string RaceName = "";
                            foreach (var civ in m_CivDatabase)   // each civ
                            {
                                //App.DoEvents();  // for avoid error after 60 seconds

                                try
                                {
                                    RaceName = civ.Race.Name;
                                }
                                catch (Exception e)
                                {
                                    string message = "check whether all race entries in Races.xml exists, last line was: " + line + Environment.NewLine + e;
                                    // Supremacy Style:   var result = MessageDialog.Show(message, MessageDialogButtons.OK);
                                    MessageBox.Show(message, "WARNING", MessageBoxButton.OK);
                                }

                                line =
                                "Civilization" + separator +
                                //civ.Key + separator +

                                //civ.Race.Name + separator +  // Race and others are NOT a string !!   it's a Race type

                                civ.Key + separator +
                                RaceName + separator +
                                civ.HomeSystemName + separator +
                                civ.Race.HomePlanetType + separator +
                                civ.Race.CombatEffectiveness + separator +
                                civ.Color + separator +
                                civ.HomeQuadrant.ToString() + separator +
                                civ.CivilizationType.ToString() + separator +
                                civ.IndustryToCreditsConversionRatio.ToString() + separator +
                                civ.TechCurve.ToString() + separator +
                                civ.ShipPrefix + separator +
                                civ.Traits;

                                // Debug only
                                //GameLog.Core.GameData.DebugFormat("CivLine = {0}", line);
                                //Console.WriteLine(line);



                                //Console.WriteLine("{0}", line);

                                streamWriter.WriteLine(line);
                            }
                        WriterClose:
                            streamWriter.Close();
                            GameLog.Core.GameData.WarnFormat("successfully ended writing _FromCivilizationsXML_(autoCreated).csv");
                        }
                        catch (Exception e)
                        {
                            GameLog.Core.GameData.Error("Cannot write ... _FromCivilizationsXML_(autoCreated).csv", e);
                        }
                    }
                    #endregion traceCivilizationsXML_To_CSV
                }
                return m_CivDatabase;
            }
            private set { }
        }
    }
}
