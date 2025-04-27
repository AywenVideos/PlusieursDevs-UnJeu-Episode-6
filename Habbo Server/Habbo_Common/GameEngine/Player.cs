using NetSquare.Core;

namespace Habbo_Common.GameEngine
{
    public class Player : INetSquareSerializable
    {
        public uint ID { get; set; } // Unique identifier for the player
        public uint ClientID { get; set; } // Client identifier for the player
        public string Account { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public int Skin { get; set; }
        public int Color { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public MapNames MapName { get; set; }
        public PlayerState PlayerState { get; set; }
        public int Money { get; set; }

        public Player() { }

        /// <summary>
        /// Constructor for creating a new player object.
        /// </summary>
        /// <param name="name"> The name of the player.</param>
        /// <param name="skin"> The skin of the player.</param>
        /// <param name="color"> The color of the player.</param>
        /// <param name="x"> The x coordinate of the player.</param>
        /// <param name="y"> The y coordinate of the player.</param>
        /// <param name="mapName"> The map name of the player.</param>
        /// <param name="money"> The amount of money the player has.</param>
        public Player(string name, int skin, int color, float x, float y, float z, MapNames mapName, int money)
        {
            Name = name;
            Skin = skin;
            Color = color;
            MapName = mapName;
            Money = money;
            PlayerState = PlayerState.Disconnected;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Constructor for deserializing a player object.
        /// </summary>
        /// <param name="serializer"> The serializer used to deserialize the player object.</param>
        public Player(NetSquareSerializer serializer)
        {
            Deserialize(serializer);
        }

        #region Serialization
        /// <summary>
        /// Serializes the player object.
        /// </summary>
        /// <param name="serializer"> The serializer used to serialize the player object.</param>
        public void Serialize(NetSquareSerializer serializer)
        {
            serializer.Set(Name);
            serializer.Set(Skin);
            serializer.Set(Color);
            serializer.Set((int)MapName);
            serializer.Set(Money);
            serializer.Set((int)PlayerState);
            serializer.Set(ID);
            serializer.Set(Account);
            serializer.Set(Password);
            serializer.Set(x);
            serializer.Set(y);
            serializer.Set(z);
        }

        /// <summary>
        /// Deserializes the player object.
        /// </summary>
        /// <param name="serializer"> The serializer used to deserialize the player object.</param>
        public void Deserialize(NetSquareSerializer serializer)
        {
            Name = serializer.GetString();
            Skin = serializer.GetInt();
            Color = serializer.GetInt();
            MapName = (MapNames)serializer.GetInt();
            Money = serializer.GetInt();
            PlayerState = (PlayerState)serializer.GetInt();
            ID = serializer.GetUInt();
            Account = serializer.GetString();
            Password = serializer.GetString();
            x = serializer.GetFloat();
            y = serializer.GetFloat();
            z = serializer.GetFloat();
        }
        #endregion
    }
}