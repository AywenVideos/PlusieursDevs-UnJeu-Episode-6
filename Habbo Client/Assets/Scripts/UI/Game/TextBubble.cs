using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBubble : MonoBehaviour
{
    [Header("Text Bubble Values")]
    public string TextInBubble;
    public string PlayerName;
    public Sprite PlayerHeadSprite;
    public Color BubbleColor;

    [Header("Bubble Creation")]
    [SerializeField]
    GameObject StartBubblePrefab;
    [SerializeField]
    GameObject SimpleBubblePrefab;
    [SerializeField]
    GameObject MiddleBubblePrefab;
    [SerializeField]
    GameObject EndBubblePrefab;


    [Header("Bubble Despawn")]
    [SerializeField]
    float BubbleSpawnTime;
    [SerializeField]
    AnimationCurve SpawnCurve;
    [SerializeField]
    float BubbleLifeTime;
    [SerializeField,Range(0f,1f)]
    float MessageLengthLifeTimeInfluence;
    [SerializeField]

    Transform Bubble;
    TextMeshPro PlayerNameText;
    TextMeshPro TextInBubbleText;
    SpriteRenderer PlayerHead;
    ChatManager chatManager;

    List<SpriteRenderer> Bubbles = new List<SpriteRenderer>();

    private void Awake()
    {
        transform.localScale = Vector3.zero;
        Bubble = transform.Find("Bubble");
        PlayerNameText = transform.Find("PlayerName").GetComponent<TextMeshPro>();
        TextInBubbleText = transform.Find("Text").GetComponent<TextMeshPro>();
        PlayerHead = transform.Find("PlayerHead").GetComponent<SpriteRenderer>();
        chatManager = GameObject.FindAnyObjectByType<ChatManager>();
    }

    public void BubbleSetUp()
    {
        chatManager.AddBubble(gameObject);
        transform.localScale = Vector3.one;
        transform.position = new Vector3 (transform.position.x, transform.position.y - 10, 10);

        int bubbleCount = Mathf.Max(Mathf.CeilToInt((TextInBubble.Length + PlayerName.Length + 5)/5f),3);
        bubbleCount += (bubbleCount % 2 == 0 ? 0 : 1);

        int NameBubble = bubbleCount - Mathf.CeilToInt((TextInBubble.Length + 5) / 5f);

        PlayerNameText.text = PlayerName.ToUpper() + "  :";
        TextInBubbleText.text = TextInBubble;

        PlayerNameText.rectTransform.anchoredPosition = new Vector3((-bubbleCount / 4f ) - bubbleCount *0.005f, 0, -1);
        TextInBubbleText.rectTransform.anchoredPosition = new Vector3((PlayerNameText.text.Length* 0.0035f - (bubbleCount / 4f)), 0, -1);

        PlayerHead.sprite = PlayerHeadSprite;
        PlayerHead.transform.localScale = Vector3.one/2;
        PlayerHead.transform.localPosition = new Vector3( - (bubbleCount / 2f) / 2f - 0.135f, 0, 0);

        for (int i = 0; i < bubbleCount; i++)
        {
            Vector3 BubblePos = new Vector3(transform.position.x + (i- (bubbleCount/2f))/2f, transform.position.y, transform.position.z);
            GameObject bubble;

            if (i == 0)
            {
                 bubble = Instantiate(StartBubblePrefab, BubblePos, Quaternion.identity, Bubble);
            }else if(i == (bubbleCount/2f))
            {
                 bubble = Instantiate(MiddleBubblePrefab, BubblePos, Quaternion.identity, Bubble);
            }
            else if(i == bubbleCount - 1)
            {
                 bubble = Instantiate(EndBubblePrefab, BubblePos, Quaternion.identity, Bubble);
            }
            else
            {
                 bubble = Instantiate(SimpleBubblePrefab, BubblePos, Quaternion.identity, Bubble);
            }

            bubble.GetComponent<SpriteRenderer>().color = BubbleColor;

            Bubbles.Add(bubble.GetComponent<SpriteRenderer>());
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        StartCoroutine(SpawnMessage());
        StartCoroutine(deleteMessage());
    }

    IEnumerator SpawnMessage()
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < BubbleSpawnTime)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero,Vector3.one, SpawnCurve.Evaluate(elapsedTime / BubbleSpawnTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator deleteMessage()
    {
        yield return new WaitForSeconds(BubbleLifeTime + 0.5f);
        while (transform.position.y < 3.0f)
        {
            foreach (var bubble in Bubbles)
            {
                bubble.color = Color.Lerp(BubbleColor, new Color(BubbleColor.r, BubbleColor.g, BubbleColor.b, 0f), Time.deltaTime / BubbleLifeTime);
                bubble.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.Lerp(BubbleColor, new Color(0f, 0f, 0f, 0f), Time.deltaTime / BubbleLifeTime);
            }

            PlayerHead.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), Time.deltaTime / BubbleLifeTime);
            PlayerNameText.color = Color.Lerp(Color.black, new Color(0f, 0f, 0f, 0f), Time.deltaTime / BubbleLifeTime);
            TextInBubbleText.color = Color.Lerp(Color.black, new Color(0f, 0f, 0f, 0f), Time.deltaTime / BubbleLifeTime);

            yield return null;
        }

        foreach (var bubble in Bubbles)
        {
            bubble.color = new Color(BubbleColor.r, BubbleColor.g, BubbleColor.b, 0f);
        }

        PlayerHead.color = new Color(1f, 1f, 1f, 0f);
        PlayerNameText.color = new Color(0f, 0f, 0f, 0f);
        TextInBubbleText.color = new Color(0f, 0f, 0f, 0f);

        chatManager.RemoveBubble(gameObject);

        Destroy(gameObject);
    }
}
