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
        private static string _text;

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

                    bool _traceCivilizationsXML = false;  // file is writen while starting a game -> Federation -> Start

                    _text = "Step_0720: " + _traceCivilizationsXML + " for writing CivilizationsXML_To_CSV - may hang up a start of the game";
                    Console.WriteLine(_text);
                    GameLog.Core.XML2CSVOutput.DebugFormat(_text);

                    if (_traceCivilizationsXML == true)
                    {
                        string pathOutputFile = "./lib/";  // instead of ./Resources/Data/
                        string separator = ";";
                        string line = "";
                        StreamWriter streamWriter;
                        string file = pathOutputFile + "test-Output.txt";
                        streamWriter = new StreamWriter(file);
                        streamWriter.Close();
                        string strHeader;  // first line of output files

                        try // avoid hang up if this file is opened by another program 
                        {
                            file = pathOutputFile + "_Civilizations-xml_List(autoCreated).csv";

                            Console.WriteLine("writing {0}", file);

                            if (file == null)
                            {
                                goto WriterClose;
                            }

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

                            _text = "begin writing " + file + " ... would breaks if dismatch of Keys between Civ..xml and Races.xml";
                            Console.WriteLine(_text);
                            GameLog.Core.GameData.InfoFormat(_text);

                            string RaceName = "";
                            foreach (Civilization civ in m_CivDatabase)   // each civ
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
                                    _ = MessageBox.Show(message, "WARNING", MessageBoxButton.OK);
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



                                Console.WriteLine("{0}", line);

                                streamWriter.WriteLine(line);
                            }
                        WriterClose:
                            streamWriter.Close();
                            _text = "Step_1280: successfully ended writing " + file;
                            Console.WriteLine(_text);
                            GameLog.Core.GameDataDetails.DebugFormat(_text);
                        }
                        catch (Exception e)
                        {
                            _text = "Step_1280: Cannot write ... " + file + e;
                            Console.WriteLine(_text);
                            GameLog.Core.GameData.ErrorFormat(_text);
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
