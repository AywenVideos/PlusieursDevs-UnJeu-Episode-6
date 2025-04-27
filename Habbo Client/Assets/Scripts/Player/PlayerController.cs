using Habbo_Common;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : RemotePlayer
{
    #region Properties
    [Header("Debugging")]
    [SerializeField] bool debugMode;
    string[] phrasesPNJ = new string[]
    {
        "T'as ramassé toutes ces carottes ? Pour ça ? Sérieux ?",
        "Tu sais que je pourrais le faire moi-même… mais j’ai pas envie.",
        "Ah, le jus ! Rien de tel pour… euh… rien du tout.",
        "Merci pour les carottes, esclave des skins.",
        "Tu t'es sali les mains pour un t-shirt rose fluo ? Respect.",
        "Le jus est peut-être toxique, mais au moins t’as du style.",
        "Encore une livraison… Tu comptes faire ça toute ta vie ?",
        "Un héros ? Non. Juste un livreur de légumes.",
        "Tu veux un skin ? Rampe encore un peu, va.",
        "Je sais même pas pourquoi je prends ces carottes. Mais continue.",
        "Ton dévouement à la mode est presque émouvant. Presque.",
        "Allez, bois ton jus et tais-toi.",
        "Tu sais que ce jeu n’a aucun sens, hein ?",
        "Moi je reste ici, peinard. Toi tu transpires. Bon deal.",
        "Encore toi ? T’as rien d’autre dans la vie que ramasser des racines ?",
        "T’as bu ce truc ?! Wow. J’te respecte pas, mais wow.",
        "Les carottes, c’est la monnaie du futur. Enfin, peut-être.",
        "Tu fais tout le travail, moi je touche la thune. Merci hein.",
        "Tu veux mon respect ? Il est dans le prochain jus. Peut-être.",
        "T’as mis 3 heures à farmer ça ? J’te file 2 pièces. Marché équitable."
    };
    string[] phrasesSansJus = new string[]
    {
        "T’as les carottes, mais pas le jus ? Amateur.",
        "Ramasseur du dimanche.",
        "Sans jus, pas d’affaire, l’ami.",
        "Carottes ? Ok. Et alors ?",
        "J’attends. Je juge.",
        "C’est pas un musée ici.",
        "Le jus, c’est la clé. Littéralement.",
        "Pas de jus, pas de respect.",
        "On ne troque pas à sec.",
        "Va boire ton destin.",
        "Tu veux un merci ? Ramène du liquide.",
        "Carottes crues ? J’suis pas un lapin.",
        "Reviens quand t’auras du goût.",
        "T’as oublié quelque chose… le cerveau ?",
        "Le jus t’attend. Et moi aussi.",
        "Tu crois que c’est un speedrun ?",
        "Faut le jus pour que ça mousse.",
        "Tu sens cette frustration ? Moi oui.",
        "T'as la moitié du puzzle, Sherlock.",
        "La vie, c’est plus que des carottes."
    };
    #endregion

    #region Unity Methods
    public void Awake()
    {
        EventManager.OnMapLoaded += EventManager_OnMapLoaded;
    }

    internal override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.OnMapLoaded -= EventManager_OnMapLoaded;
    }

    public override void Update()
    {
        base.Update();
        PlayerInterractions();
    }
    #endregion

    #region Events
    /// <summary>
    /// Event handler for when a new map is loaded.
    /// </summary>
    /// <param name="mapName"> The name of the loaded map. </param>
    private void EventManager_OnMapLoaded(MapNames mapName, MapNames lastMapName)
    {
        // Set player position to the spawn point of the new map
        Vector3 spawnpos = MapManager.Instance.CurrentMap.GroundTileMap.GetCellCenterWorld(
            new Vector3Int(MapManager.Instance.CurrentMap.PlayerStartPos.x, MapManager.Instance.CurrentMap.PlayerStartPos.y, 0));
        // check if there is a TP cell on the map that go to lastMapName
        if (lastMapName != MapNames.None)
        {
            foreach (Cell cell in MapManager.Instance.CurrentMap.Cells)
            {
                if (cell.MapName == lastMapName && cell.CanWalkOn)
                {
                    spawnpos = cell.Position;
                    break;
                }
            }
        }

        // set the player position
        NetworkManager.AskToTeleport(Player, spawnpos);
    }
    #endregion

    #region Interractions
    /// <summary>
    /// Handle player interactions with objects in the game world.
    /// </summary>
    void PlayerInterractions()
    {
        if (MapManager.Instance.CurrentMap == null)
            return;

        Vector3Int currentCell = MapManager.Instance.CurrentMap.GroundTileMap.WorldToCell(transform.position);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Handle Teleportation
            if (CurrentCell.MapName != MapNames.None)
            {
                NetworkManager.EnterMap(CurrentCell.MapName, 0, 0, 0);
            }

            // Handle Skin color set
            if (CurrentCell.SkinIndex != -1)
            {
                int cost = 10;
                // check if the player has enough money
                if (NetworkManager.CurrentPlayer.Money > cost)
                {
                    NetworkManager.AddGold(-cost);
                    NetworkManager.PlayerSetColor(CurrentCell.SkinIndex);
                }
                else
                {
                    NotificationManager.Instance.ShowNotification("Vous n'avez pas assès de pièces d'or pour acheter cette tenue. (Coût : 10)");
                }
            }

            // Handle Interactions with objects
            switch (CurrentCell.Interractible)
            {
                // Handle TV interactions
                case InterractibleEntities.TV:
                    if (!TVWindow.Instance.IsOpened)
                        TVWindow.Instance.OpenWindow();
                    else
                        TVWindow.Instance.CloseWindow();
                    break;

                // Handle Food interactions
                case InterractibleEntities.Food:
                    if (FoodManager.Instance.isSpawned)
                        FoodManager.Instance.TakeFood();
                    break;

                // Handle Table interactions
                case InterractibleEntities.Table:
                    if (FoodManager.Instance.inInventory)
                    {
                        FoodManager.Instance.GiveFood();
                        NotificationManager.Instance.ShowNotification(phrasesPNJ[Random.Range(0, phrasesPNJ.Length)]);
                    }
                    else
                    {
                        NotificationManager.Instance.ShowNotification(phrasesSansJus[Random.Range(0, phrasesSansJus.Length)]);
                    }
                    break;
            }
        }
    }
    #endregion

    /// <summary>
    /// Draw gizmos for debugging purposes.
    /// </summary>
    public void OnDrawGizmos()
    {
        if (!debugMode)
            return;

        // draw target cell
        Gizmos.color = Color.yellow;
        if (PlayerSelector.Instance.TargetCell != null)
            Gizmos.DrawCube(PlayerSelector.Instance.TargetCell.Position, Vector3.one * .3f);

        // draw map cells
        for (int i = 0; MapManager.Instance.CurrentMap.Cells != null && i < MapManager.Instance.CurrentMap.Cells.GetLength(0); i++)
        {
            for (int j = 0; j < MapManager.Instance.CurrentMap.Cells.GetLength(1); j++)
            {
                if (!string.IsNullOrEmpty(MapManager.Instance.CurrentMap.Cells[i, j].BlockType))
                    Gizmos.color = Color.red;
                else
                {
                    if (MapManager.Instance.CurrentMap.Cells[i, j].IsWalkable)
                        Gizmos.color = Color.white;
                    else
                        Gizmos.color = Color.black;
                }
                Gizmos.DrawCube(MapManager.Instance.CurrentMap.Cells[i, j].Position, Vector3.one * .2f);
            }
        }

        // draw path
        Cell currentCell = MapManager.Instance.CurrentMap.GetCell(transform.position);
        // draw current cell
        Gizmos.color = Color.red;
        Gizmos.DrawCube(currentCell.Position, Vector3.one * .2f);

        if (PlayerSelector.Instance.TargetCell != null)
        {
            Stack<Cell> path = MapManager.Instance.CurrentMap.FindPath(currentCell, PlayerSelector.Instance.TargetCell);
            if (path != null)
            {
                while (path.Count > 0)
                {
                    var cell = path.Pop();
                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(cell.Position, Vector3.one * .1f);
                }
            }
        }
    }
}

/// <summary>
/// Enum representing the direction of locomotion.
/// </summary>
public enum LocomotionDirection
{
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// Enum representing the different types of interactable entities.
/// </summary>
public enum InterractibleEntities
{
    None,
    TV,
    Work,
    OutDoor,
    Food,
    Table
}

/// <summary>
/// Class representing a set of animations for a skin.
/// </summary>
[System.Serializable]
public class SkinAnimationSet
{
    [Header("Idle Animations")]
    public Sprite[] idleSprites;

    [Header("Walk Animations")]
    public Sprite[] walkFrontFrames;
    public Sprite[] walkBehindFrames;
}