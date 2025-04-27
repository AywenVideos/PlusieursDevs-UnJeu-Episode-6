using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    // Vitesse de déplacement de la caméra
    public float smoothSpeed = 5f;

    [Header("Dead Zone Settings (as percentage of screen)")]
    // Pourcentage de la largeur de l'écran défini comme zone morte
    [Range(0f, 1f)]
    public float deadZonePercentX = 0.3f;
    // Pourcentage de la hauteur de l'écran défini comme zone morte
    [Range(0f, 1f)]
    public float deadZonePercentY = 0.3f;

    [Header("Zoom Settings")]
    // Vitesse d'ajustement du zoom via la molette
    public float zoomSpeed = 5f;
    // Vitesse de lissage du zoom
    public float zoomSmoothSpeed = 10f;
    // Zoom minimum (orthographicSize minimum)
    public float minZoom = 3f;
    // Zoom maximum (orthographicSize maximum)
    public float maxZoom = 10f;

    private Camera cam;
    // Valeur cible du zoom
    private float targetZoom;

    void Start()
    {
        // Récupère le composant Camera sur cet objet ou la caméra principale
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        // Initialise le zoom cible avec la valeur initiale de l'orthographicSize
        targetZoom = cam.orthographicSize;
    }

    void LateUpdate()
    {
        // Si le bouton droit de la souris est pressé, on ne suit pas automatiquement la cible
        if (Input.GetMouseButton(1))
            return;
        if (NetworkManager.CurrentPlayerController == null)
            return;

        // --- Gestion du zoom via la molette de la souris ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Ajuste targetZoom en fonction de la molette, et le limite entre minZoom et maxZoom
            targetZoom = Mathf.Clamp(targetZoom - scroll * zoomSpeed, minZoom, maxZoom);
        }
        // Lisse la transition du zoom
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSmoothSpeed);

        // --- Suivi de la cible avec zone morte ---
        // Convertit la position du joueur en coordonnées viewport (0 à 1)
        Vector3 viewportPos = cam.WorldToViewportPoint(NetworkManager.CurrentPlayerController.transform.position);

        // Calcul des limites de la zone morte
        float deadZoneMinX = 0.5f - deadZonePercentX * 0.5f;
        float deadZoneMaxX = 0.5f + deadZonePercentX * 0.5f;
        float deadZoneMinY = 0.5f - deadZonePercentY * 0.5f;
        float deadZoneMaxY = 0.5f + deadZonePercentY * 0.5f;

        // Calcul du décalage du joueur hors de la zone morte (en coordonnées viewport)
        float offsetX = 0f;
        if (viewportPos.x < deadZoneMinX)
            offsetX = viewportPos.x - deadZoneMinX;
        else if (viewportPos.x > deadZoneMaxX)
            offsetX = viewportPos.x - deadZoneMaxX;

        float offsetY = 0f;
        if (viewportPos.y < deadZoneMinY)
            offsetY = viewportPos.y - deadZoneMinY;
        else if (viewportPos.y > deadZoneMaxY)
            offsetY = viewportPos.y - deadZoneMaxY;

        // Si le joueur est bien dans la zone morte, la caméra ne bouge pas
        if (Mathf.Approximately(offsetX, 0f) && Mathf.Approximately(offsetY, 0f))
            return;

        // Conversion de l'offset en coordonnées world
        Vector3 viewportCenter = new Vector3(0.5f, 0.5f, viewportPos.z);
        Vector3 shiftedViewportPoint = new Vector3(0.5f + offsetX, 0.5f + offsetY, viewportPos.z);
        Vector3 worldCenter = cam.ViewportToWorldPoint(viewportCenter);
        Vector3 worldShifted = cam.ViewportToWorldPoint(shiftedViewportPoint);
        Vector3 worldOffset = worldShifted - worldCenter;

        // Calcul de la nouvelle position cible de la caméra en ajoutant l'offset world
        Vector3 targetCamPos = transform.position + worldOffset;
        // Conserve la position en Z (2D)
        targetCamPos.z = transform.position.z;

        // Déplacement lissé de la caméra vers la position cible
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothSpeed * Time.deltaTime);
    }
}
