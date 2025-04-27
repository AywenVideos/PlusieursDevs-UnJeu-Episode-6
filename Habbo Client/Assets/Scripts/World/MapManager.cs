using Habbo_Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    [SerializeField] List<MapData> MapsAssetData;
    [SerializeField] MapNames DefaultMap;
    public Vector3 WorldCellOffset;
    public Vector2Int CellCoordOffset;
    public Map CurrentMap { get; private set; }
    private Dictionary<MapNames, MapData> maps = new Dictionary<MapNames, MapData>();
    private GameObject currentMapObject;
    [SerializeField] VolumeProfile PostProcessVolumeProfile;
    [SerializeField] float FadeOutIntensity = 0.22f;
    [SerializeField] float FadeInIntensity = -10f;
    [SerializeField] float TransitionDuration = 0.5f;

    /// <summary>
    /// Start is called once before the first execution of Update after the MonoBehaviour is created
    /// </summary>
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // bind dictionary with maps
        foreach (MapData mapData in MapsAssetData)
        {
            maps[mapData.MapName] = mapData;
        }

        // load default map
        if (DefaultMap != MapNames.None)
            LoadMap(DefaultMap);

        EventManager.OnCellBlockTypeChange += EventManager_OnCellBlockTypeChange;
    }

    /// <summary>
    /// Event handler for cell block type change.
    /// </summary>
    /// <param name="cell"> The cell that has changed. </param>
    /// <param name="blockType"> The new block type. </param>
    private void EventManager_OnCellBlockTypeChange(Cell cell, string blockType)
    {
        if (cell != null && CurrentMap != null)
        {
            cell.BlockType = blockType;
            CurrentMap.GetCell(cell.Position).BlockType = blockType;
        }
    }

    /// <summary>
    /// Load a map by its name.
    /// </summary>
    /// <param name="mapName"> The name of the map to load. </param>
    public void LoadMap(MapNames mapName)
    {
        // check if map exists
        if (!maps.ContainsKey(mapName))
        {
            Debug.LogError($"Map {mapName} not found.");
            return;
        }
        ClothesSkinSetter.skinIndex = 0; // ugly to do this here, but no time to add manager

        // Load the map coroutine
        StartCoroutine(LoadMap_Routine(mapName));
    }

    /// <summary>
    /// Coroutine to load a map with a fade animation.
    /// </summary>
    /// <returns> IEnumerator for the animation. </returns>
    IEnumerator LoadMap_Routine(MapNames mapName)
    {
        float fadeInDuration = TransitionDuration / 2f;
        float fadeOutDuration = TransitionDuration / 2f;
        float elapsedTime = 0f;
        ColorAdjustments colorAdjustment;

        // Fade out the sun
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeOutDuration);
            if (PostProcessVolumeProfile.TryGet<ColorAdjustments>(out colorAdjustment))
            {
                colorAdjustment.postExposure.Override(Mathf.Lerp(FadeOutIntensity, FadeInIntensity, t));
            }
            yield return null;
        }
        if (PostProcessVolumeProfile.TryGet<ColorAdjustments>(out colorAdjustment))
        {
            colorAdjustment.postExposure.Override(FadeInIntensity);
        }

        // unload previous map
        if (currentMapObject != null)
        {
            Destroy(currentMapObject);
        }

        // load new map
        MapNames lastMapName = CurrentMap != null ? CurrentMap.Name : MapNames.None;
        currentMapObject = Instantiate(maps[mapName].MapPrefab);
        CurrentMap = new Map(maps[mapName], currentMapObject);
        currentMapObject.transform.SetParent(transform, false);
        currentMapObject.name = mapName.ToString();
        currentMapObject.transform.localPosition = Vector3.zero;
        currentMapObject.transform.localScale = Vector3.one;
        currentMapObject.transform.localRotation = Quaternion.identity;

        // Trigger the map loaded event
        EventManager.TriggerMapLoaded(mapName, lastMapName);
        yield return new WaitForSeconds(0.5f);

        // Fade in the sun
        elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeInDuration);
            if (PostProcessVolumeProfile.TryGet<ColorAdjustments>(out colorAdjustment))
            {
                colorAdjustment.postExposure.Override(Mathf.Lerp(FadeInIntensity, FadeOutIntensity, t));
            }
            yield return null;
        }
    }
}

/// <summary>
/// Class representing a map.
/// </summary>
public class Map
{
    public MapNames Name { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Cell[,] Cells { get; private set; }
    public Tilemap GroundTileMap { get; private set; }
    public Tilemap DecorationTileMap { get; private set; }
    public LayerMask DecorationLayerMask { get; private set; }
    public Vector3 TileOffset { get; private set; }
    public Vector3 PlayerOffset { get; private set; }
    public Vector2Int PlayerStartPos { get; private set; }
    public GameObject MapPrefab { get; private set; }

    /// <summary>
    /// Constructor of the Map class.
    /// </summary>
    /// <param name="mapData"> The data of the map. </param>
    public Map(MapData mapData, GameObject currentMapObject)
    {
        // initialize map properties
        Name = mapData.MapName;
        PlayerStartPos = mapData.PlayerStartPos;
        PlayerOffset = mapData.PlayerOffset;
        MapPrefab = mapData.MapPrefab;
        GroundTileMap = currentMapObject.transform.FindChildWithTag("Ground").gameObject.GetComponent<Tilemap>();
        DecorationTileMap = currentMapObject.transform.FindChildWithTag("Deco").gameObject.GetComponent<Tilemap>();

        // initialize cells
        BoundsInt bounds = GroundTileMap.cellBounds;
        TileBase[] allTiles = GroundTileMap.GetTilesBlock(bounds);
        Width = (int)bounds.size.x;
        Height = (int)bounds.size.y;
        Cells = new Cell[Width, Height];
        DecorationLayerMask = mapData.DecorationLayerMask;
        TileOffset = mapData.TileOffset;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                Cells[x, y] = new Cell
                {
                    X = x + bounds.position.x,
                    Y = y + bounds.position.y,
                    IsWalkable = tile != null,
                    Position = GroundTileMap.GetCellCenterWorld(new Vector3Int(x + bounds.position.x, y + bounds.position.y, 0)) - new Vector3(0.2f, 0.2f, 0),
                    MapName = MapNames.None // Default value, no TP
                };
            }
        }

        // if it's home, let's add custom blocks
        if (Name == MapNames.Home)
        {
            int i = 0;
            // iterate over all blocks
            while (PlayerPrefs.HasKey("BlockId" + i))
            {
                // get block position
                float floatVec3X = PlayerPrefs.GetFloat(("BlockPosX" + i));
                float floatVec3Y = PlayerPrefs.GetFloat(("BlockPosY" + i));
                float floatVec3Z = PlayerPrefs.GetFloat(("BlockPosZ" + i));

                // Get the block position as a Vector3
                Vector3 floatToVec3 = new Vector3(floatVec3X, floatVec3Y, floatVec3Z);

                // get block type
                string blockType = PlayerPrefs.GetInt("BlockId" + i).ToString();
                // get cell at position
                Cell cell = GetCell(floatToVec3);
                // set block type
                cell.BlockType = blockType;

                i++;
            }
        }

        // add Teleport cells
        TeleportCell[] teleportCells = currentMapObject.GetComponentsInChildren<TeleportCell>();
        foreach (TeleportCell teleportCell in teleportCells)
        {
            Cell cell = GetCell(teleportCell.transform.position);
            if (cell != null)
            {
                cell.MapName = teleportCell.MapName;
            }
        }

        // add interractible entities
        InterractibleCell[] interractibleEntities = currentMapObject.GetComponentsInChildren<InterractibleCell>();
        foreach (InterractibleCell interractibleEntity in interractibleEntities)
        {
            Cell cell = GetCell(interractibleEntity.transform.position);
            if (cell != null)
            {
                cell.Interractible = interractibleEntity.Interractible;
            }
        }

        // add Unwalkable Cells
        UnwalkableCell[] unwalkableCells = currentMapObject.GetComponentsInChildren<UnwalkableCell>();
        foreach (UnwalkableCell unwalkableCell in unwalkableCells)
        {
            Cell cell = GetCell(unwalkableCell.transform.position);
            if (cell != null)
            {
                cell.IsWalkable = false;
            }
        }

        // add unwalkable tiles from tilemap
        if (mapData.UnwalkableTilemap != null)
        {
            BoundsInt tilemapBounds = mapData.UnwalkableTilemap.cellBounds;
            TileBase[] tilemapTiles = mapData.UnwalkableTilemap.GetTilesBlock(tilemapBounds);
            for (int x = 0; x < tilemapBounds.size.x; x++)
            {
                for (int y = 0; y < tilemapBounds.size.y; y++)
                {
                    TileBase tile = tilemapTiles[x + y * tilemapBounds.size.x];
                    if (tile != null)
                    {
                        Vector3 worldPos = mapData.UnwalkableTilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));
                        Cell cell = Cells[x, y];
                        if (cell != null)
                        {
                            cell.IsWalkable = false;
                            cell.BlockType = "Unwalkable";
                        }
                    }
                }
            }
        }

        // add ClothesSkinSetter
        ClothesSkinSetter[] clothesSkinSetters = currentMapObject.GetComponentsInChildren<ClothesSkinSetter>();
        foreach (ClothesSkinSetter clothesSkinSetter in clothesSkinSetters)
        {
            Cell cell = GetCell(clothesSkinSetter.transform.position);
            if (cell != null)
            {
                cell.SkinIndex = clothesSkinSetter.CellSkinIndex;
            }
        }

        // bind cells neighbors
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Cells[x, y].Neighbors = new List<Cell>();
                if (x > 0) Cells[x, y].Neighbors.Add(Cells[x - 1, y]);
                if (x < Width - 1) Cells[x, y].Neighbors.Add(Cells[x + 1, y]);
                if (y > 0) Cells[x, y].Neighbors.Add(Cells[x, y - 1]);
                if (y < Height - 1) Cells[x, y].Neighbors.Add(Cells[x, y + 1]);
            }
        }
    }

    /// <summary>
    /// Get a cell at a specific position.
    /// </summary>
    /// <param name="worldPosition"> The world position of the cell. </param>
    /// <returns> The cell at the specified position. </returns>
    public Cell GetCell(Vector3 worldPosition)
    {
        worldPosition.x += MapManager.Instance.WorldCellOffset.x;
        worldPosition.y += MapManager.Instance.WorldCellOffset.y;
        Vector3Int cellPosition = GroundTileMap.WorldToCell(worldPosition);

        int x = cellPosition.x;
        int y = cellPosition.y;
        x -= GroundTileMap.cellBounds.position.x;
        y -= GroundTileMap.cellBounds.position.y;
        x += MapManager.Instance.CellCoordOffset.x;
        y += MapManager.Instance.CellCoordOffset.y;
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return null;
        }
        return Cells[x, y];
    }

    /// <summary>
    /// Get a cell at a specific position.
    /// </summary>
    /// <param name="x"> The x coordinate of the cell. </param>
    /// <param name="y"> The y coordinate of the cell. </param>
    /// <returns> The cell at the specified position. </returns>
    public Cell GetCell(int x, int y)
    {
        Cell cell = null;
        foreach (Cell c in Cells)
        {
            if (c.X == x && c.Y == y)
            {
                cell = c;
                break;
            }
        }

        return cell;
    }

    /// <summary>
    /// Get the best path between two cells using the A* algorithm.
    /// </summary>
    public Stack<Cell> FindPath(Cell start, Cell end)
    {
        Stack<Cell> path = new Stack<Cell>();
        List<Cell> openList = new List<Cell>();
        List<Cell> closedList = new List<Cell>();
        Cell current = start;

        // Ajoute le nœud de départ à l'OpenList
        openList.Add(start);

        while (openList.Count > 0 && !closedList.Exists(x => x.Position == end.Position))
        {
            // Sélectionne le nœud de l'OpenList ayant le plus petit F
            current = openList.OrderBy(n => n.F).First();
            openList.Remove(current);
            closedList.Add(current);

            foreach (Cell n in current.Neighbors)
            {
                if (!closedList.Contains(n) && n.CanWalkOn)
                {
                    // Si le nœud n'est pas déjà présent dans l'OpenList, on le configure et l'ajoute
                    if (!openList.Contains(n))
                    {
                        n.Parent = current;
                        n.DistanceToTarget = Mathf.Abs(n.Position.x - end.Position.x) + Mathf.Abs(n.Position.y - end.Position.y);
                        n.Cost = n.Weight + n.Parent.Cost;
                        openList.Add(n);
                    }
                }
            }
        }

        // Si le nœud d'arrivée n'a pas été fermé, aucun chemin n'a été trouvé
        if (!closedList.Exists(x => x.Position == end.Position))
        {
            return null;
        }

        // Reconstitution du chemin depuis le nœud d'arrivée
        Cell temp = closedList.First(x => x.Position == end.Position);
        if (temp == null) return null;

        while (temp != start && temp != null)
        {
            path.Push(temp);
            temp = temp.Parent;
        }
        // Optionnellement, on peut pousser le point de départ
        path.Push(start);

        return path;
    }
}

/// <summary>
/// Class representing a cell in the map.
/// </summary>
public class Cell
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsWalkable { get; set; }
    public string BlockType { get; set; }
    public Vector3 Position { get; set; }
    public List<Cell> Neighbors { get; set; }
    public Cell Parent { get; set; } // for pathfinding
    public float DistanceToTarget = -1f;
    public float Cost = 1f;
    public float Weight = 1f;
    public int SkinIndex { get; set; } = -1; // for skin index
    public MapNames MapName { get; set; } // for teleportation
    public InterractibleEntities Interractible { get; set; } // for interractible entities

    /// <summary>
    /// F value for A* pathfinding.
    /// </summary>
    public float F
    {
        get
        {
            if (DistanceToTarget != -1 && Cost != -1)
                return DistanceToTarget + Cost;
            else
                return -1;
        }
    }

    /// <summary>
    /// Check if the cell is walkable and not blocked by an object.
    /// </summary>
    public bool CanWalkOn => IsWalkable && string.IsNullOrEmpty(BlockType);
}