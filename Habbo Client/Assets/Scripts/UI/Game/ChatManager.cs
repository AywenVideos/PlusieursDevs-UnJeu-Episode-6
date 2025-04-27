using UnityEngine;
using System.Collections.Generic;

public class Bubble
{
    public GameObject obj;
    public int BubbleIndex;
    public float TimeSinceSpawned = 0;
    public float TimeSinceLastMove = 0; // Since the bubble was created
    public float OffsetY = 0;
}

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance;
    [Header("Chat Settings")]
    [SerializeField]
    float defaultChatHeight;
    [SerializeField]
    float BubbleHeigth;
    [SerializeField, Range(0f, 1f)]
    float BubblesLerpSpeed;
    [SerializeField]
    List<Bubble> TextBubbles = new List<Bubble>();
    [SerializeField, Range(0.05f, 1.0f)]
    float BubblesSpeed;
    [SerializeField]
    int TotalBubblesCounts;
    public bool HasFocus = false;

    public void Awake()
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

    /// <summary>
    /// Add a bubble to the chat.
    /// </summary>
    /// <param name="textBubble"> The bubble to add.</param>
    public void AddBubble(GameObject textBubble)
    {
        textBubble.transform.parent = transform;
        TotalBubblesCounts++;
        Bubble bubble = new Bubble
        {
            obj = textBubble,
            BubbleIndex = TotalBubblesCounts,
        };
        TextBubbles.Add(bubble);
    }

    /// <summary>
    /// Remove a bubble from the chat.
    /// </summary>
    /// <param name="textBubble"> The bubble to remove.</param>
    public void RemoveBubble(GameObject textBubble)
    {
        textBubble.transform.parent = transform;

        foreach (Bubble bbl in TextBubbles)
        {
            if (bbl.obj == textBubble)
            {
                TextBubbles.Remove(bbl);
                return;
            }
        }

    }

    private void Update()
    {
        UpdateBubblePositions();
    }

    /// <summary>
    /// Update the position of all bubbles in the chat.
    /// </summary>
    void UpdateBubblePositions()
    {
        for (int i = 0; i < TextBubbles.Count; i++)
        {
            if (TextBubbles[i].obj != null)
            {
                TextBubbles[i].TimeSinceSpawned += Time.deltaTime;
                if (TextBubbles[i].TimeSinceLastMove + 0.5f < TextBubbles[i].TimeSinceSpawned)
                {
                    TextBubbles[i].TimeSinceLastMove = TextBubbles[i].TimeSinceSpawned;
                    TextBubbles[i].OffsetY += BubblesSpeed;

                }
                TextBubbles[i].obj.transform.position = 
                    Vector3.Lerp(
                        TextBubbles[i].obj.transform.position, 
                        new Vector3(
                            TextBubbles[i].obj.transform.position.x, 
                            (TotalBubblesCounts - TextBubbles[i].BubbleIndex) * BubbleHeigth + defaultChatHeight + TextBubbles[i].OffsetY, 
                            TextBubbles[i].obj.transform.position.z
                            ), 
                        BubblesLerpSpeed);
            }
        }
    }
}