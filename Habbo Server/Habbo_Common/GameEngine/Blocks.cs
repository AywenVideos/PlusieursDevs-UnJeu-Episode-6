using NetSquare.Core;
using System;
using System.Collections.Generic;

namespace Habbo_Common.GameEngine
{
    /// <summary>
    /// Represents a collection of blocks in a 2D space.
    /// </summary>
    public class Blocks : INetSquareSerializable
    {
        public Dictionary<Pos, int> blocs = new Dictionary<Pos, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Blocks"/> class.
        /// </summary>
        public Blocks()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blocks"/> class by deserializing from a NetSquareSerializer.
        /// </summary>
        /// <param name="serializer"> The serializer to use for deserialization.</param>
        public Blocks(NetSquareSerializer serializer)
        {
            Deserialize(serializer);
        }

        public bool Contains(Pos pos)
        {
            return blocs.ContainsKey(pos);
        }

        #region Serialization
        /// <summary>
        /// Serializes the blocks to a NetSquareSerializer.
        /// </summary>
        /// <param name="serializer"> The serializer to use for serialization.</param>
        public void Serialize(NetSquareSerializer serializer)
        {
            serializer.Set(blocs.Count);
            foreach (var bloc in blocs)
            {
                serializer.Set(bloc.Value);
                serializer.Set(bloc.Key.x);
                serializer.Set(bloc.Key.y);
            }
        }

        /// <summary>
        /// Deserializes the blocks from a NetSquareSerializer.
        /// </summary>
        /// <param name="serializer"> The serializer to use for deserialization.</param>
        public void Deserialize(NetSquareSerializer serializer)
        {
            blocs = new Dictionary<Pos, int>();
            int count = serializer.GetInt();
            for (int i = 0; i < count; i++)
            {
                int id = serializer.GetInt();
                int x = serializer.GetInt();
                int y = serializer.GetInt();
                blocs.Add(new Pos(x, y), id);
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents a position in a 2D space with integer coordinates.
    /// </summary>
    public struct Pos : IEquatable<Pos>, IComparable<Pos>
    {
        public int x;
        public int y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pos"/> struct with the specified coordinates.
        /// </summary>
        /// <param name="x"> The x-coordinate.</param>
        /// <param name="y"> The y-coordinate.</param>
        public Pos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Compares this instance with another <see cref="Pos"/> instance.
        /// </summary>
        /// <param name="other"> The other <see cref="Pos"/> instance to compare with.</param>
        /// <returns> A value indicating the relative order of the instances.</returns>
        public int CompareTo(Pos other)
        {
            if (x == other.x)
            {
                return y.CompareTo(other.y);
            }
            return x.CompareTo(other.x);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Pos"/> instance is equal to the current instance.
        /// </summary>
        /// <param name="other"> The other <see cref="Pos"/> instance to compare with.</param>
        /// <returns> true if the specified instance is equal to the current instance; otherwise, false.</returns>
        public bool Equals(Pos other)
        {
            return x == other.x && y == other.y;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj"> The object to compare with the current instance.</param>
        /// <returns> true if the specified object is equal to the current instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Pos other)
            {
                return Equals(other);
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns> A hash code for the current instance.</returns>
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }
    }
}