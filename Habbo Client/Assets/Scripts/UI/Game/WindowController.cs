using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WindowController : MonoBehaviour
{
    [SerializeField]
    Canvas canvas;

    [Header("Window Values")]
    public bool IsOpened;
    [SerializeField]
    float SpawnTime;
    [SerializeField]
    AnimationCurve SpawnCurve;
    [SerializeField]
    float DespawnTime;
    [SerializeField]
    AnimationCurve DespawnCurve;

    public bool IsBeingDraged;
    Vector2 MouseOffset;

    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

#pragma warning disable IDE0052 // Supprimer les membres privés non lus
    private bool isConstructionWindowOpen = false;
#pragma warning restore IDE0052 // Supprimer les membres privés non lus

    [HideInInspector] public List<BuildingBlockData> blocks = new List<BuildingBlockData>();
    [HideInInspector] public List<Vector3> blocksPos = new List<Vector3>();

    public void Start()
    {
        eventSystem = EventSystem.current;
        raycaster = canvas.GetComponent<GraphicRaycaster>();

        // Positionnez la fenêtre de construction dans le coin supérieur droit
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (gameObject.name == "BuildWindow") // Vérifiez si c'est la fenêtre de construction
        {
        }
        else
        {
            // Positionnez les autres fenêtres normalement
            rectTransform.anchorMin = new Vector2(0, 0); // Par exemple, coin inférieur gauche
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.anchoredPosition = Vector2.zero; // Ajustez selon vos besoins
        }
    }

    public virtual void OpenWindow()
    {
        CloseAllWindows();

        if (IsOpened == false)
        {
            MusicManager.Instance.PlaySFX(SFX.Menu_Opening);
            IsOpened = true;
            GetComponent<RectTransform>().localScale = Vector3.zero;
            StartCoroutine(LerpSize(Vector3.one, SpawnTime, SpawnCurve));

            RectTransform rectTransform = GetComponent<RectTransform>();
            if (gameObject.name != "BuildWindow") // Vérifiez si ce n'est pas la fenêtre de construction
            {
                rectTransform.anchoredPosition = Vector2.zero; // Ajustez selon vos besoins
            }

            if (gameObject.name == "ConstructionWindow")
            {
                isConstructionWindowOpen = true;
            }
        }
        else
        {
            MusicManager.Instance.PlaySFX(SFX.Menu_Opening);
            GetComponent<RectTransform>().localScale = Vector3.one;
            StartCoroutine(LerpSize(Vector3.zero, DespawnTime, DespawnCurve));
            Invoke("ResetOpen", DespawnTime);
        }
    }

    public void OpenWindowcredit()
    {
        if (IsOpened) // Vérifiez si la fenêtre est déjà ouverte
        {
            CloseWindow(); // Fermez la fenêtre si elle est déjà ouverte
            return; // Sortir de la méthode
        }

        MusicManager.Instance.PlaySFX(SFX.Menu_Opening);

        IsOpened = true;
        GetComponent<RectTransform>().localScale = Vector3.zero;

        // Positionnez la fenêtre de crédit au centre de l'écran
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Centre
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero; // Ajustez selon vos besoins

        StartCoroutine(LerpSize(Vector3.one, SpawnTime, SpawnCurve));
    }

    public void ResetOpen()
    {
        IsOpened = false;
        isConstructionWindowOpen = false;
    }

    IEnumerator LerpSize(Vector3 TargetSize, float TotalTime, AnimationCurve curve)
    {
        Vector3 DefaultSize = GetComponent<RectTransform>().localScale;
        float elapsedTime = 0.0f;

        while (elapsedTime < TotalTime)
        {
            GetComponent<RectTransform>().localScale = Vector3.Lerp(DefaultSize, TargetSize, curve.Evaluate(elapsedTime / TotalTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void Update()
    {
        if (IsOpened && !IsConstructionWindow()) // Vérifiez si la fenêtre est ouverte et si ce n'est pas la fenêtre de construction
        {
            HandleWindowMovement(); // Gérer le déplacement des autres fenêtres
        }
        else
        {
            // Réactivez les clics sur les objets en arrière-plan
            Physics2D.queriesHitTriggers = true; // Pour 2D
            // Physics.queriesHitTriggers = true; // Pour 3D
        }

        if (IsOpened)
        {
            // Désactivez les clics sur les objets en arrière-plan
            Physics2D.queriesHitTriggers = false; // Pour 2D
            // Physics.queriesHitTriggers = false; // Pour 3D

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())
            {
                if (gameObject.name != "BuildWindow")
                {
                    CloseWindow();
                }
            }

            // close on esc press
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseWindow();
            }

            // Désactivez le déplacement des objets
            return; // Sortir de la méthode si une fenêtre est ouverte
        }

        GetComponent<RectTransform>().anchoredPosition = KeepFullyOnScreen(gameObject, transform.GetComponent<RectTransform>().anchoredPosition);
    }

    private bool IsConstructionWindow()
    {
        return gameObject.name == "BuildWindow"; // Vérifiez si c'est la fenêtre de construction
    }

    private void HandleWindowMovement()
    {
        if (IsPointerOverChild("WindowTop") && Input.GetMouseButtonDown(0))
        {
            IsBeingDraged = true;
            var viewportPosition = new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height, 0);
            MouseOffset = GetComponent<RectTransform>().anchoredPosition - ViewportToCanvasPosition(canvas, viewportPosition);
        }

        if (IsBeingDraged && Input.GetMouseButton(0)) // Vérifiez si la fenêtre est en cours de déplacement
        {
            var viewportPosition = new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height, 0);
            GetComponent<RectTransform>().anchoredPosition = ViewportToCanvasPosition(canvas, viewportPosition) + MouseOffset;
        }
        else
        {
            IsBeingDraged = false;
        }
    }

    private bool IsPointerOverChild(string childName)
    {
        pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.name == childName && result.gameObject.transform.IsChildOf(transform))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsPointerOverUIElement()
    {
        pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        return results.Count > 0;
    }

    Vector3 KeepFullyOnScreen(GameObject panel, Vector3 newPos)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        RectTransform CanvasRect = canvas.GetComponent<RectTransform>();

        float minX = (CanvasRect.sizeDelta.x - rect.sizeDelta.x) * -0.5f;
        float maxX = (CanvasRect.sizeDelta.x - rect.sizeDelta.x) * 0.5f;
        float minY = (CanvasRect.sizeDelta.y - rect.sizeDelta.y) * -0.5f;
        float maxY = (CanvasRect.sizeDelta.y - rect.sizeDelta.y) * 0.5f;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        return newPos;
    }

    public Vector2 ViewportToCanvasPosition(Canvas canvas, Vector2 viewportPosition)
    {
        var centerBasedViewPortPosition = viewportPosition - new Vector2(0.5f, 0.5f);
        var canvasRect = canvas.GetComponent<RectTransform>();
        var scale = canvasRect.sizeDelta;
        return Vector2.Scale(centerBasedViewPortPosition, scale);
    }

    private void CloseAllWindows()
    {
        var windows = UnityEngine.Object.FindObjectsByType<WindowController>(FindObjectsSortMode.None);
        foreach (WindowController window in windows)
        {
            if (window.IsOpened)
            {
                window.CloseWindow();
            }
        }
    }

    public virtual void CloseWindow()
    {
        if (IsOpened)
        {
            MusicManager.Instance.PlaySFX(SFX.Menu_Opening);
            GetComponent<RectTransform>().localScale = Vector3.one;
            StartCoroutine(LerpSize(Vector3.zero, DespawnTime, DespawnCurve));
            Invoke("ResetOpen", DespawnTime);
        }
    }

    public void ChangeGroundTilemapSize(Vector2Int newSize)
    {
        Tilemap groundTilemap = GameObject.Find("GroundTilemap").GetComponent<Tilemap>();
        groundTilemap.size = new Vector3Int(newSize.x, newSize.y, groundTilemap.size.z);
    }

    public bool IsMouseOverWindow()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 localPoint;
        // Vérifiez si la souris est au-dessus de la fenêtre
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint))
        {
            return rectTransform.rect.Contains(localPoint);
        }
        return false;
    }

    public void DisableAnimator()
    {
        GetComponent<Animator>().enabled = false;
    }
}