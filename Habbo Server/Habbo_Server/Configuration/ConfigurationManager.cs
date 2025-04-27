using NetSquare.Server.Utils;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Habbo_Server.Utils
{
    /// <summary>
    /// Configuration manager for the server
    /// </summary>
    public static class ConfigurationManager
    {
        #region Variables
        /// <summary>
        /// The configuration of the server
        /// </summary>
        public static Configuration Configuration;
        /// <summary>
        /// The path of the configuration file
        /// </summary>
        private static string configurationPath;
        /// <summary>
        /// Flag to check if the configuration is already loaded
        /// </summary>
        private static bool initialized = false;
        #endregion

        /// <summary>
        /// Load the configuration from the json file
        /// </summary>
        static ConfigurationManager()
        {
            Load(false);
        }

        /// <summary>
        /// Load the configuration from the json file
        /// </summary>
        /// <param name="displayConfig"> Display the configuration in the console</param>
        public static void Load(bool displayConfig)
        {
            // If the configuration is already loaded, return
            if (initialized)
                return;
            // Load the configuration from the json file
            Writer.Write_Server("Loading Server Configuration...", ConsoleColor.DarkYellow, false);
            configurationPath = Environment.CurrentDirectory + @"/config.json";
            if (File.Exists(configurationPath))
                Configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(configurationPath));
            else
            {
                Configuration = new Configuration();
                File.WriteAllText(configurationPath, JsonConvert.SerializeObject(Configuration));
            }

            // Replace the [current] tag with the current directory
            Configuration.DatabaseFolder = Configuration.DatabaseFolder.Replace("[current]", Environment.CurrentDirectory);

            // Create the database folder if it doesn't exist
            if (!Directory.Exists(Configuration.DatabaseFolder))
                Directory.CreateDirectory(Configuration.DatabaseFolder);
            Writer.Write("OK", ConsoleColor.Green);

            // Display the configuration in the console
            if (displayConfig)
                Writer.Write(Configuration.ToString(), ConsoleColor.Yellow);

            // Set the initialized flag to true
            initialized = true;
        }

        /// <summary>
        /// Save the given configuration as json next to the server dll
        /// </summary>
        /// <param name="configuration">The configuration to save</param>
        public static void SaveConfiguration(Configuration configuration)
        {
            Configuration = configuration;
            Configuration.DatabaseFolder = Configuration.DatabaseFolder.Replace("[current]", Environment.CurrentDirectory);
            File.WriteAllText(configurationPath, JsonConvert.SerializeObject(configuration));
        }
    }
}