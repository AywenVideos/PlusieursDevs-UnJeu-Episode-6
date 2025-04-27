using System.Reflection;
using System.Text;

namespace Habbo_Server.Utils
{
    /// <summary>
    /// Configuration class that contains all the server settings
    /// </summary>
    public class Configuration
    {
        #region Variables
        /// <summary>
        /// The path to the game data folder
        /// </summary>
        public string ServerName { get; set; }
        /// <summary>
        /// The folder where the database will be saved
        /// </summary>
        public string DatabaseFolder { get; set; }
        /// <summary>
        /// The interval between each database save
        /// </summary>
        public int DatabaseSaveInterval { get; set; }
        /// <summary>
        /// The number of database save to keep
        /// </summary>
        public int DatabaseNbSaveToKeep { get; set; }
        /// <summary>
        /// The current database file
        /// </summary>
        public string CurrentDatabaseFile { get { return DatabaseFolder + @"/Database.db"; } }
        #endregion

        /// <summary>
        /// Default constructor that set the default values
        /// </summary>
        public Configuration()
        {
            DatabaseFolder = @"[current]/Databases";
            DatabaseSaveInterval = 1;
            DatabaseNbSaveToKeep = 10;
            ServerName = "Habbo Hotel";
        }

        /// <summary>
        /// Return the configuration as a string
        /// </summary>
        /// <returns> The configuration as a string </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (PropertyInfo Info in GetType().GetProperties())
            {
                sb.Append(" - ");
                sb.Append(Info.Name);
                sb.Append(" : ");
                sb.AppendLine(GetType().GetProperty(Info.Name).GetValue(this).ToString());
            }
            return sb.ToString().TrimEnd('\n');
        }
    }
}