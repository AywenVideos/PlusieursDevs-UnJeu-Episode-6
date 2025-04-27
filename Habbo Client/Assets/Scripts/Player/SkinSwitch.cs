using UnityEngine;

public class SkinSwitch : MonoBehaviour
{
    [Header("Skins pour l'UI (icônes)")]
    [SerializeField] private Sprite[] uiSkins;

    [Header("Animations par Skin pour le joueur")]
    [SerializeField] private SkinAnimationSet[] skinAnimationSets;

    [Header("Références")]
    [SerializeField] private RemotePlayer playerMovement;  // Composant PlayerMouvement du joueur

    private int currentSkinIndex = 0;

    private void Start()
    {
        if (uiSkins.Length != skinAnimationSets.Length)
        {
            Debug.LogError("Le nombre d'icônes UI doit être égal au nombre de SkinAnimationSet !");
            return;
        }

        if (uiSkins.Length > 0)
        {
            UIController.Instance.uiSkinImage.sprite = uiSkins[0];
            playerMovement.SetSkin(0); // Initialisation du skin en jeu
        }
    }

    public void ChangeSkin(int index)
    {
        if (index < 0 || index >= uiSkins.Length)
        {
            Debug.LogWarning("Index de skin invalide !");
            return;
        }

        currentSkinIndex = index;
        // Met à jour l'UI
        UIController.Instance.uiSkinImage.sprite = uiSkins[currentSkinIndex];
        // Notifie le player pour changer d'animation et de skin
        playerMovement.SetSkin(currentSkinIndex);
    }

    public void NextSkin()
    {
        currentSkinIndex = (currentSkinIndex + 1) % uiSkins.Length;
        ChangeSkin(currentSkinIndex);
    }
}
