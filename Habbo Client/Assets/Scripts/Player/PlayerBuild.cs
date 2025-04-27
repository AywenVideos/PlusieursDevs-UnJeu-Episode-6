using Habbo_Common.GameEngine;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerBuild : MonoBehaviour
{
    public static PlayerBuild Instance;
    [Header("Build Values")]
    public BuildingBlockData CurrentBuildingBlock;
    [HideInInspector]
    public bool IsBuilding;

    [Header("Componants")]
    [SerializeField]
    WindowController BuildingWindow;
    [SerializeField]
    Vector3 DecorationTileMapScale;
    [SerializeField]
    GameObject PredictionObj;
    SpriteRenderer PredictionSprite;
    bool CanPlaceBlockAtPosition;

    public WindowController windowController;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ChangeBuilingBlock(0);
    }

    private void Update()
    {
        CheckForBuilding();
        ChangePredictionColor();
        IsBuilding = BuildingWindow != null && BuildingWindow.IsOpened;
    }

    #region Prediction
    void CheckForBuilding()
    {
        if (IsBuilding)
        {
            if (PredictionSprite == null) CreatePrediction(CurrentBuildingBlock.BlockTile.sprite);
            PredictionObj.transform.position = new Vector3(PlayerSelector.Instance.SelectorPosition.x, PlayerSelector.Instance.SelectorPosition.y, -1);
            PredictionObj.transform.localScale = DecorationTileMapScale;

            if (MapManager.Instance.CurrentMap.DecorationTileMap.HasTile(MapManager.Instance.CurrentMap.DecorationTileMap.WorldToCell(PlayerSelector.Instance.SelectorPosition)) || !MapManager.Instance.CurrentMap.GroundTileMap.HasTile(MapManager.Instance.CurrentMap.GroundTileMap.WorldToCell(PlayerSelector.Instance.SelectorPosition)))
            {
                CanPlaceBlockAtPosition = false;
            }
            else
            {
                CanPlaceBlockAtPosition = true;
            }
        }
        else
        {
            if (PredictionSprite != null) DestroyPrediction();
        }
    }

    void CreatePrediction(Sprite sprite)
    {
        PredictionSprite = PredictionObj.AddComponent<SpriteRenderer>();
        PredictionSprite.sprite = sprite;
        PredictionSprite.sortingOrder = 1;
    }

    void ChangePredictionColor()
    {
        if (IsBuilding && PredictionSprite != null)
        {
            if (CanPlaceBlockAtPosition)
            {
                PredictionSprite.color = new Color(0.6f, 1f, 0.6f, 0.4f);
            }
            else
            {
                PredictionSprite.color = new Color(1f, 0.6f, 0.6f, 0.4f);
            }
        }

    }

    void DestroyPrediction()
    {
        Destroy(PredictionSprite);
    }
    #endregion

    /// <summary>
    /// Place the block on the map.
    /// </summary>
    public void PlaceBuild()
    {
        if (IsBuilding)
        {
            MusicManager.Instance.PlaySFX(SFX.Block_Place);
            Cell cell = MapManager.Instance.CurrentMap.GetCell(PlayerSelector.Instance.SelectorPosition);
            NetworkManager.AskToPlaceBlock(cell, CurrentBuildingBlock.id);
        }
    }

    /// <summary>
    /// Destroy the block on the map.
    /// </summary>
    public void DestroyBuild()
    {
        if (IsBuilding)
        {
            MusicManager.Instance.PlaySFX(SFX.Block_Break);
            Cell cell = MapManager.Instance.CurrentMap.GetCell(PlayerSelector.Instance.SelectorPosition);
            NetworkManager.AskToRemoveBlock(cell);
        }
    }

    /// <summary>
    /// Change the building block.
    /// </summary>
    /// <param name="index"> The index of the block to change.</param>
    public void ChangeBuilingBlock(int index)
    {
        // Load tile from resources
        Tile tile = Resources.Load<Tile>("Blocks/" + index);
        if (tile == null)
        {
            Debug.LogError($"Le bloc avec l'index {index} n'a pas pu être trouvé dans les ressources !");
            return;
        }
        CurrentBuildingBlock = new BuildingBlockData()
        {
            BlockName = tile.name,
            BlockTile = tile,
            BlockSize = Vector2.zero,
            id = index
        };

        if (PredictionSprite != null) PredictionSprite.sprite = CurrentBuildingBlock.BlockTile.sprite;
    }

    #region Set Blocks
    Blocks _blocks = new Blocks();
    /// <summary>
    /// Set the blocks on the map.
    /// </summary>
    /// <param name="blocks"> The blocks to set.</param>
    public void SetBlocks(Blocks blocks)
    {
        // Clear the current blocks
        foreach (var block in _blocks.blocs)
        {
            Cell cell = MapManager.Instance.CurrentMap.GetCell(block.Key.x, block.Key.y);
            Vector3Int position = MapManager.Instance.CurrentMap.DecorationTileMap.WorldToCell(cell.Position);
            MapManager.Instance.CurrentMap.DecorationTileMap.SetTile(position, null);
            EventManager.TriggerCellBlockTypeChange(cell, null);
        }

        // Set the new blocks
        foreach (var block in blocks.blocs)
        {
            Cell cell = MapManager.Instance.CurrentMap.GetCell(block.Key.x, block.Key.y);
            if (cell == null)
                continue;
            Tile tile = Resources.Load<Tile>("Blocks/" + block.Value);
            if (tile == null)
                continue;
            Vector3Int position = MapManager.Instance.CurrentMap.DecorationTileMap.WorldToCell(cell.Position);
            MapManager.Instance.CurrentMap.DecorationTileMap.SetTile(position, tile);
            EventManager.TriggerCellBlockTypeChange(cell, tile.name);
        }

        // Update the blocks list
        _blocks = blocks;
    }
    #endregion
}
