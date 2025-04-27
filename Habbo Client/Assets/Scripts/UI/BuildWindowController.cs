using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BuildWindowController : MonoBehaviour
{
    [SerializeField] RectTransform Container;
    [SerializeField] GameObject ButtonPrefab;

    private void Start()
    {
        // load all from resources
        int i = 0;
        while(true)
        {
            Tile tile = Resources.Load<Tile>("Blocks/" + i);
            if(tile == null)
            {
                break;
            }

            GameObject button = Instantiate(ButtonPrefab, Container);
            Button btn = button.GetComponent<Button>();
            int j = i;
            btn.onClick.AddListener(() => { PlayerBuild.Instance.ChangeBuilingBlock(j); });
            button.transform.GetChild(0).GetComponent<Image>().sprite = tile.sprite;

            i++;
        }
    }
}