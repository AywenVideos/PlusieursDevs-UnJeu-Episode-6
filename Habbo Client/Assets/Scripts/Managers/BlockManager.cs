using UnityEngine;

public class BlockManager : MonoBehaviour
{
    private static BlockManager instance;
    public static BlockManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        if (LayerMask.NameToLayer("Block") == -1)
        {
            Debug.LogError("Le layer 'Block' n'existe pas! Créez-le dans Project Settings > Tags and Layers");
        }
    }
}