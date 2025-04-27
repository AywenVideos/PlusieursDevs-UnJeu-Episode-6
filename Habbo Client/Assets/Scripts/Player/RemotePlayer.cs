using Habbo_Common;
using Habbo_Common.GameEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    #region Properties
    public Player Player;
    [Header("Mouvement Values")]
    [SerializeField] internal float MoveSpeed;

    [Header("Player Sprite")]
    [SerializeField] internal SpriteRenderer PlayerSpriteRenderer;

    [Header("Animations par Skin")]
    [SerializeField] internal SkinAnimationSet[] skinAnimationSets;

    [Header("Animation Timing")]
    [SerializeField, Range(0.01f, 1f)] internal float frameRate;
    [SerializeField] float stepSFXDelay = 0.3f; // Delay between step sound effects

    Coroutine WalkCoroutine;
    private int currentFrame;
    private float timer;
    public LocomotionDirection CurrentDirection = LocomotionDirection.Down;
    internal Cell CurrentCell;
    [HideInInspector] public bool IsMoving = false;

    public int currentSkinIndex = 0;
    private SkinAnimationSet currentSkinSet;
    #endregion

    #region Unity Methods
    internal virtual void OnDestroy()
    {
        if (WalkCoroutine != null)
            StopCoroutine(WalkCoroutine);
    }

    public virtual void Update()
    {
        AnimateCharacter(CurrentDirection, !IsMoving);
        timer += Time.deltaTime;
    }
    #endregion

    #region Initialization
    public void Initialize(Player player)
    {
        Player = player;
        SetSkin(player.Skin);
        PlayerSpriteRenderer.material = Instantiate(PlayerSpriteRenderer.material);
        PlayerSkin.Instance.SetColor(PlayerSpriteRenderer.material, player.Color);
        StartCoroutine(PlayStepSFX());
    }
    #endregion

    #region Locomotion
    /// <summary>
    /// Move the player to a specific position.
    /// </summary>
    /// <param name="targetPos"> The position to move to. </param>
    public void GoToPosition(Vector3 targetPos)
    {
        // Get the current cell
        Cell currentCell = MapManager.Instance.CurrentMap.GetCell(transform.position - MapManager.Instance.CurrentMap.PlayerOffset);
        // check if the cell is walkable
        if (currentCell == null || !currentCell.CanWalkOn)
        {
            return;
        }

        // Get the destination cell
        Cell destinationCell = MapManager.Instance.CurrentMap.GetCell(targetPos);
        // check if the destination cell is walkable
        if (destinationCell == null || !destinationCell.CanWalkOn)
        {
            return;
        }

        // Get the path between the current cell and the destination cell
        Stack<Cell> path = MapManager.Instance.CurrentMap.FindPath(currentCell, destinationCell);
        if (path == null || path.Count == 0)
        {
            return;
        }
        CurrentCell = currentCell;
        // Start the coroutine to follow the path
        if (WalkCoroutine != null)
            StopCoroutine(WalkCoroutine);
        WalkCoroutine = StartCoroutine(followPath(path));
    }

    /// <summary>
    /// Coroutine to follow a path of cells
    /// </summary>
    /// <param name="path"> The path to follow. </param>
    /// <returns> Wait for the next cell. </returns>
    IEnumerator followPath(Stack<Cell> path)
    {
        IsMoving = true;
        while (path.Count > 0)
        {
            var cell = path.Pop();
            Vector3 targetPos = cell.Position + MapManager.Instance.CurrentMap.PlayerOffset;
            Vector3 startPosition = transform.position;
            Vector3 direction = targetPos - startPosition;
            float distance = direction.magnitude;

            // get direction based on current cell and target cell
            int currentCX = CurrentCell.X;
            int currentCY = CurrentCell.Y;
            int targetCX = cell.X;
            int targetCY = cell.Y;

            if (currentCX == targetCX)
            {
                if (currentCY < targetCY)
                {
                    CurrentDirection = LocomotionDirection.Left;
                }
                else
                {
                    CurrentDirection = LocomotionDirection.Right;
                }
            }
            else if (currentCY == targetCY)
            {
                if (currentCX < targetCX)
                {
                    CurrentDirection = LocomotionDirection.Up;
                }
                else
                {
                    CurrentDirection = LocomotionDirection.Down;
                }
            }

            // Move the player to the target position
            float duration = distance / MoveSpeed;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPos;

            CurrentCell = cell;
        }
        IsMoving = false;
    }

    /// <summary>
    /// Animate the character based on the current direction and movement state.
    /// </summary>
    /// <param name="direction"> The current movement direction. </param>
    /// <param name="idle"> Whether the character is idle. </param>
    private void AnimateCharacter(LocomotionDirection direction, bool idle)
    {
        if (idle)
        {
            currentFrame = 0;
            timer = 0f;

        }
        else
        {
            // Update the animation timer and cycle frames if enough time has passed
            if (timer >= frameRate)
            {
                timer = 0f;
                // Cycle through walking frames (assumes both walk sets have the same number of frames)
                currentFrame = (currentFrame + 1) % currentSkinSet.walkFrontFrames.Length;
            }
        }

        // Determine the dominant movement axis (vertical vs. horizontal)
        switch (direction)
        {
            case LocomotionDirection.Up:
                PlayerSpriteRenderer.sprite = currentSkinSet.walkBehindFrames[currentFrame];
                PlayerSpriteRenderer.flipX = false;
                break;
            case LocomotionDirection.Down:
                PlayerSpriteRenderer.sprite = currentSkinSet.walkFrontFrames[currentFrame];
                PlayerSpriteRenderer.flipX = true;
                break;
            case LocomotionDirection.Left:
                PlayerSpriteRenderer.sprite = currentSkinSet.walkBehindFrames[currentFrame];
                PlayerSpriteRenderer.flipX = true;
                break;
            case LocomotionDirection.Right:
                PlayerSpriteRenderer.sprite = currentSkinSet.walkFrontFrames[currentFrame];
                PlayerSpriteRenderer.flipX = false;
                break;
        }
    }

    /// <summary>
    /// Coroutine to play the step sound
    /// </summary>
    /// <returns> Wait for the next step sfx. </returns>
    IEnumerator PlayStepSFX()
    {
        while (true)
        {
            if (IsMoving)
            {
                MusicManager.Instance.PlaySFX(SFX.Step);
            }
            yield return new WaitForSeconds(stepSFXDelay);
        }
    }
    #endregion

    #region Utils
    /// <summary>
    /// Set the skin of the player.
    /// </summary>
    /// <param name="index"> The index of the skin to set. </param>
    public void SetSkin(int index)
    {
        if (index >= 0 && index < skinAnimationSets.Length)
        {
            Vector3 currentPos = transform.position;
            Vector3 currentScale = PlayerSpriteRenderer.transform.localScale;

            currentSkinIndex = index;
            currentSkinSet = skinAnimationSets[index];

            currentFrame = 0;
            timer = 0f;

            transform.position = currentPos;
            PlayerSpriteRenderer.transform.localScale = currentScale;
        }
        else
        {
            Debug.LogWarning("Skin index invalide dans PlayerMouvement !");
        }
    }
    #endregion
}