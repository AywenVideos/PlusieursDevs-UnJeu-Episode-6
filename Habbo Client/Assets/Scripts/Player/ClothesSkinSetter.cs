using UnityEngine;

public class ClothesSkinSetter : MonoBehaviour
{
    public Material ClotheMaterial;
    public SpriteRenderer ClotheSpriteRenderer;
    [HideInInspector] public int CellSkinIndex = 0;
    public static int skinIndex = 0;

    void Awake()
    {
        Material mat = Instantiate(ClotheMaterial);
        ClotheSpriteRenderer.material = mat;
        PlayerSkin.Instance.SetColor(mat, skinIndex);
        int i = skinIndex;
        CellSkinIndex = i;
        skinIndex++;
    }
}