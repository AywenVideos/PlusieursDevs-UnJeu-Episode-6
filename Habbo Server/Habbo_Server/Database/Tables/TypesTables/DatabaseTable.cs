using NetSquare.Core;
using NetSquare.Server.Utils;
using System;
using System.Collections.Generic;

namespace Habbo_Server.Datas
{
    public abstract partial class DatabaseTable<T, V> : Dictionary<T, V> where T : IComparable where V : INetSquareSerializable, new()
    {
        public string Name { get; set; }

        public abstract T GetKey(V value);

        public virtual void LoadFromDatabase(NetSquareSerializer reader)
        {
            Writer.Write_Database("Loading " + Name + "... ", ConsoleColor.DarkYellow, false);
            int length = reader.GetInt();
            for (int i = 0; i < length; i++)
            {
                V val = reader.GetSerializable<V>();
                T key = GetKey(val);
                Add(key, val);
            }
            Writer.Write(Count.ToString(), ConsoleColor.Green);
        }

        public virtual void SaveToDatabase(NetSquareSerializer writer)
        {
            writer.Set(Count);
            foreach(V val in Values)
            {
                writer.Set<V>(val);
            }
        }
    }
}