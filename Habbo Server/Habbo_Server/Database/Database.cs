using Habbo_Server.Utils;
using NetSquare.Core;
using NetSquare.Server.Utils;
using System;
using System.IO;
using System.Threading;

namespace Habbo_Server.Datas
{
    /// <summary>
    /// Database class
    /// </summary>
    public static class Database
    {
        #region Tables
        // client Data
        public static DbTable_Players Players = new DbTable_Players();
        #endregion

        #region Load And Save
        /// <summary>
        /// Connect to the database
        /// </summary>
        public static void Connect()
        {
            // get newest db file
            long maxTicks = long.MinValue;
            string latestSaveFile = ConfigurationManager.Configuration.CurrentDatabaseFile;
            string DatabaseFolder = ConfigurationManager.Configuration.DatabaseFolder;
            string[] files = Directory.GetFiles(DatabaseFolder);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                long ticks = -1;
                string[] spl = fileName.Split('_');
                if (spl.Length > 0)
                    if (long.TryParse(spl[0], out ticks))
                    {
                        // this is a save
                        if (ticks > maxTicks && ticks > -1 && !file.EndsWith("-log.db"))
                        {
                            maxTicks = ticks;
                            latestSaveFile = DatabaseFolder + @"/" + Path.GetFileName(file);
                        }
                    }
            }

            // create new database if not exists
            if (!File.Exists(latestSaveFile))
            {
                Save();
            }
            // open newest db file
            LoadDatabase(latestSaveFile);

            // start DB autosave
            AutoSaveDatabase();

            // save current db file
            Save();
        }

        /// <summary>
        /// Auto Save Database
        /// </summary>
        private static void AutoSaveDatabase()
        {
            int NbSaveToKeep = ConfigurationManager.Configuration.DatabaseNbSaveToKeep;
            int SaveInterval = ConfigurationManager.Configuration.DatabaseSaveInterval;
            SaveInterval = SaveInterval * 60 * 1000; // minutes

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(SaveInterval);
                    Save();
                }
            });
            thread.Start();
        }

        /// <summary>
        /// Save Database
        /// </summary>
        public static void Save()
        {
            // Save New Database
            string DatabaseFolder = ConfigurationManager.Configuration.DatabaseFolder;
            string saveFileName = DateTime.Now.Ticks + "_Database.db";
            string saveDbFile = DatabaseFolder + @"/" + saveFileName;
            SaveDatabase(saveDbFile);

            // Remove oldest Database File
            int NbSaveToKeep = ConfigurationManager.Configuration.DatabaseNbSaveToKeep;
            string[] files = Directory.GetFiles(DatabaseFolder);
            if (files.Length > NbSaveToKeep)
            {
                long minTicks = long.MaxValue;
                string latestSaveFile = string.Empty;
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    long ticks = -1;
                    string[] spl = fileName.Split('_');
                    if (spl.Length > 0)
                        if (long.TryParse(spl[0], out ticks))
                        {
                            // this is a save
                            if (ticks < minTicks && ticks > -1)
                            {
                                minTicks = ticks;
                                latestSaveFile = file;
                            }
                        }
                }
                if (!string.IsNullOrEmpty(latestSaveFile))
                {
                    File.Delete(latestSaveFile);
                }
            }
        }

        /// <summary>
        /// Load Database
        /// </summary>
        /// <param name="path"> Path to the database file </param>
        public static void LoadDatabase(string path)
        {
            // check if file exists
            if (!File.Exists(path))
            {
                Writer.Write("No Database", ConsoleColor.Red);
                return;
            }

            Writer.Write_Database("Opening Database...", ConsoleColor.DarkYellow, false);
            byte[] bytes = File.ReadAllBytes(path);
            NetSquareSerializer serializer = new NetSquareSerializer();
            serializer.StartReading(bytes);
            Writer.Write("OK", ConsoleColor.Green);

            // players data
            Players.LoadFromDatabase(serializer);
            RoomsManager.Blocks = new Habbo_Common.GameEngine.Blocks(serializer);
        }

        /// <summary>
        /// Save Database to a file
        /// </summary>
        /// <param name="path"> Path to the database file </param>
        public static void SaveDatabase(string path)
        {
            // Create and Init Database
            Writer.Write_Database("Saving database...", ConsoleColor.DarkYellow, false);
            NetSquareSerializer serializer = new NetSquareSerializer();
            int aproxSize = Players.Count * 2048;
            serializer.StartWriting(aproxSize, 2048);
            try
            {
                Players.SaveToDatabase(serializer);
                RoomsManager.Blocks.Serialize(serializer);

                // save to file
                File.WriteAllBytes(path, serializer.ToArray());
                Writer.Write("OK", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Writer.Write("Fail", ConsoleColor.Green);
                Writer.Write(ex.ToString(), ConsoleColor.Red);
            }
        }
        #endregion
    }
}