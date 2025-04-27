using Habbo_Common;
using Habbo_Common.GameEngine;
using System;

public static class EventManager
{
    // Déclaration des événements
    public static event Action<Player> OnUserLoggedIn;
    public static event Action OnUserLoggedOut;  // Nouvel événement pour la déconnexion
    public static event Action OnUserRegistered;  // Nouvel événement pour l'inscription réussie
    public static event Action<string> OnUserRegistrationError;  // Nouvel événement pour l'erreur d'inscription
    public static event Action<string> OnErrorOccurred;  // Nouvel événement pour l'erreur générale
    public static event Action<MapNames, MapNames> OnMapLoaded;
    public static event Action<Cell, string> OnCellBlockTypeChange;
    public static event Action<int> OnUpdateMoney;

    public static void TriggerUpdateMoney(int money)
    {
        OnUpdateMoney?.Invoke(money);
    }

    // Déclaration des événements pour les actions de la carte
    public static void TriggerMapLoaded(MapNames mapName, MapNames lastMapName)
    {
        OnMapLoaded?.Invoke(mapName, lastMapName);
    }

    public static void TriggerCellBlockTypeChange(Cell cell, string blockType)
    {
        OnCellBlockTypeChange?.Invoke(cell, blockType);
    }

    // Déclenchement de l'événement de connexion
    public static void TriggerUserLogin(Player player)
    {
        OnUserLoggedIn?.Invoke(player);
    }

    // Déclenchement de l'événement de déconnexion
    public static void TriggerUserLogout()
    {
        OnUserLoggedOut?.Invoke();
    }

    // Déclenchement de l'événement d'inscription
    public static void TriggerUserRegistration()
    {
        OnUserRegistered?.Invoke();
    }

    // Déclenchement de l'événement d'erreur d'inscription
    public static void TriggerUserRegistrationError(string errorMessage)
    {
        OnUserRegistrationError?.Invoke(errorMessage);
    }

    // Déclenchement de l'événement d'erreur générale
    public static void TriggerError(string errorMessage)
    {
        OnErrorOccurred?.Invoke(errorMessage);
    }
}