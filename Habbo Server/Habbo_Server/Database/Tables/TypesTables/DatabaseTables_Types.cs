using NetSquare.Core;

namespace Habbo_Server.Datas
{
    public abstract class DatabaseTable_Int<T> : DatabaseTable<int, T> where T : INetSquareSerializable, new()
    {
    }

    public abstract class DatabaseTable_UInt<T> : DatabaseTable<uint, T> where T : INetSquareSerializable, new()
    {
    }

    public abstract class DatabaseTable_EID<T> : DatabaseTable<uint, T> where T : INetSquareSerializable, new()
    {
    }

    public abstract class DatabaseTable_UShort<T> : DatabaseTable<ushort, T> where T : INetSquareSerializable, new()
    {
    }
}