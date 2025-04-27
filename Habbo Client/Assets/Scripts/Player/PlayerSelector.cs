using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerSelector : MonoBehaviour
{
    [Header("Composants")]
    [SerializeField] Camera MainCam;
    PlayerController playerMouvement;
    PlayerBuild playerBuild;
    public static PlayerSelector Instance;

    [Header("Sélecteur")]
    [SerializeField] Transform Selector;
    bool CanSelect = false;
    [SerializeField, Range(0f, 1f)] float SelectorMoveSpeed = 0.1f;
    [SerializeField] float MouseMinY;
    [SerializeField] Sprite SelectorDefaultSprite, SelectorClicSprite;
    SpriteRenderer SelectorSpriteRenderer;

    [HideInInspector] public Vector3 SelectorPosition;
    WindowController[] WindowsController;
    public Cell TargetCell;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SelectorSpriteRenderer = Selector.GetComponent<SpriteRenderer>();
        playerMouvement = GetComponent<PlayerController>();
        playerBuild = GetComponent<PlayerBuild>();

        if (MainCam == null) Debug.LogError("MainCam n'est pas assigné !");
        if (Selector == null) Debug.LogError("Selector n'est pas assigné !");

        var wcs = FindObjectsByType<WindowController>(FindObjectsSortMode.None);
        WindowsController = new WindowController[wcs.Length];
        for (int i = 0; i < wcs.Length; i++)
        {
            WindowsController[i] = wcs[i];
        }
    }

    void Update()
    {
        if (IsAnyWindowOpen() && !playerBuild.IsBuilding)
        {
            return;
        }

        if (playerBuild.IsBuilding && IsMouseOverConstructionWindow())
        {
            return;
        }

        if(MapManager.Instance.CurrentMap == null)
        {
            return;
        }
        SetSelectorPos();
        SelectorInteraction();
    }

    bool IsAnyWindowOpen()
    {
        foreach (var win in WindowsController)
        {
            if (win.IsOpened)
            {
                return true;
            }
        }
        return false;
    }

    bool CheckForClicWindow()
    {
        foreach (var win in WindowsController)
        {
            if (win.IsBeingDraged)
            {
                return true;
            }
        }
        return false;
    }

    bool IsMouseOverConstructionWindow()
    {
        foreach (var win in WindowsController)
        {
            if (win.IsOpened && win.gameObject.name == "ConstructionWindow")
            {
                if (win.IsMouseOverWindow())
                {
                    return true;
                }
            }
        }
        return false;
    }

    void SetSelectorPos()
    {
        Vector2 MouseWorldPos = MainCam.ScreenToWorldPoint(Input.mousePosition);
        Tilemap GroundTileMap = MapManager.Instance.CurrentMap.GroundTileMap;
        Vector3Int MouseCelPos = GroundTileMap.WorldToCell(MouseWorldPos);
        SelectorPosition = GroundTileMap.CellToWorld(MouseCelPos) + new Vector3(0, 0.25f, 0);
        TargetCell = MapManager.Instance.CurrentMap.GetCell(GroundTileMap.CellToWorld(MouseCelPos));

        if (GroundTileMap.HasTile(MouseCelPos) && Input.mousePosition.y > MouseMinY && !CheckForClicWindow())
        {
            Vector3 TargetPos = Vector3.Slerp(Selector.position, SelectorPosition, SelectorMoveSpeed);
            TargetPos.z = -0.2f;
            Selector.position = TargetPos;
            Selector.gameObject.SetActive(true);
            CanSelect = true;
        }
        else
        {
            CanSelect = false;
            Selector.gameObject.SetActive(false);
        }
    }

    void SelectorInteraction()
    {
        if (CanSelect)
        {
            if (Input.GetMouseButtonDown(0))
            {
                LeftInteract();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (playerBuild.IsBuilding)
                {
                    playerBuild.DestroyBuild();
                }
            }

            SelectorSpriteRenderer.sprite = Input.GetMouseButton(0) ? SelectorClicSprite : SelectorDefaultSprite;
        }
    }

    void LeftInteract()
    {
        if (playerBuild.IsBuilding) playerBuild.PlaceBuild();
        else NetworkManager.AskToMove(SelectorPosition);
    }
}
