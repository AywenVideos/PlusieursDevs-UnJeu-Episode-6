using Habbo_Common;
using Habbo_Common.GameEngine;
using NetSquare.Core;

namespace Habbo_Server.Datas
{
    public class DbTable_Players : DatabaseTable_EID<Player>
    {
        public DbTable_Players()
        {
            Name = "Players";
        }

        #region table logic
        public override uint GetKey(Player value)
        {
            return value.ID;
        }

        public override void LoadFromDatabase(NetSquareSerializer database)
        {
            base.LoadFromDatabase(database);

            // reset all players states
            foreach (var player in this)
            {
                player.Value.PlayerState = PlayerState.Disconnected;
            }
        }

        public override void SaveToDatabase(NetSquareSerializer database)
        {
            base.SaveToDatabase(database);
        }
        #endregion

        #region Player utils
        /// <summary>
        /// Check if a player exists
        /// </summary>
        /// <param name="playerID"> id of the player</param>
        /// <returns> true if exists</returns>
        public bool Exists(uint playerID)
        {
            return ContainsKey(playerID);
        }

        /// <summary>
        /// Get a player by its player id
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns> null if not exists</returns>
        public bool TryGetPlayer(uint playerID, out Player player)
        {
            // check if the player exists
            if (!ContainsKey(playerID))
            {
                player = null;
                return false;
            }

            player = this[playerID];
            return true;
        }
        #endregion

        #region Player creation
        /// <summary>
        /// Check if a pseudo is available
        /// </summary>
        /// <param name="pseudo"> pseudo to check</param>
        /// <returns> true if available</returns>
        public bool IsAvailablePseudo(string pseudo)
        {
            foreach (var client in this)
                if (client.Value.Name == pseudo)
                    return false;
            return true;
        }
        #endregion
    }
}