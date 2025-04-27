using Habbo_Common;
using UnityEngine;

public class SavedDataManager : MonoBehaviour
{
    public PlayerBuild playerBuild;
    public WindowController windowController;

    public void Start()
    {
        EventManager.OnMapLoaded += EventManager_OnMapLoaded;
    }

    private void EventManager_OnMapLoaded(MapNames mapName, MapNames lastMapName)
    {
        if (mapName != MapNames.Home)
            return;

        NetworkManager.GetBlocks();
    }
}