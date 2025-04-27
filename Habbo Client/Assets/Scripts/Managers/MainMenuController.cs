using Habbo_Common;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public List<GameObject> OnlyHomeButtons;

    /// <summary>
    /// Subscribes to the event when the object is instantiated.
    /// </summary>
    private void Awake()
    {
        EventManager.OnMapLoaded += EventManager_OnMapLoaded;
    }

    /// <summary>
    ///  Unsubscribes from the event when the object is destroyed.
    ///  </summary>
    private void OnDestroy()
    {
        EventManager.OnMapLoaded -= EventManager_OnMapLoaded;
    }

    /// <summary>
    /// This method is called when the map is loaded.
    /// </summary>
    /// <param name="mapName"> The name of the map that was loaded.</param>
    private void EventManager_OnMapLoaded(MapNames mapName, MapNames lastMapName)
    {
        // Check if the map is Home, if so, show the buttons
        if (mapName == MapNames.Home)
        {
            foreach (var button in OnlyHomeButtons)
            {
                button.SetActive(true);
            }
        }
        // If the map is not Home, hide the buttons
        else
        {
            foreach (var button in OnlyHomeButtons)
            {
                button.SetActive(false);
            }
        }
    }
}