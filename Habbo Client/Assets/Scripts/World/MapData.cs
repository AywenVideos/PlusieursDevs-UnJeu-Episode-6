using Habbo_Common;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapData : MonoBehaviour
{
    public MapNames MapName;
    public GameObject MapPrefab;
    public Tilemap UnwalkableTilemap;
    public LayerMask DecorationLayerMask;
    public Vector3 TileOffset;
    public Vector3 PlayerOffset;
    public Vector2Int PlayerStartPos = new Vector2Int(0, 0);
}