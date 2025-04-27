using UnityEngine;

public class PlayerSkin : MonoBehaviour
{
    public static PlayerSkin Instance;
    public Texture2D[] Colors;
    private int currentColorIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetColor(Material material, int index)
    {
        if (index < 0 || index >= Colors.Length)
        {
            Debug.LogError("Invalid color index");
            return;
        }
        material.SetTexture("_PaletteTarget", Colors[index]);
    }
}