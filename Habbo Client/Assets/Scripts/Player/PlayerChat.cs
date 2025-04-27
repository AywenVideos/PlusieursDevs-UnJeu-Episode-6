using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerChat : MonoBehaviour
{
    public static PlayerChat Instance;
    [Header("Componants")]
    [SerializeField]
    TMP_InputField InputField;

    [Header("Write Settings")]
    [SerializeField]
    Color[] MessageDiferentsColors;
    [SerializeField]
    int CurrentMessageColor;

    [Header("Text Bubble Settings")]
    [SerializeField]
    GameObject TextBubblePrefab;

    private float timer = 0.0f;

    [SerializeField, Range(0f, 3f)]
    private float antiSpam = 0.5f;
    public bool HasFocus => InputField.isFocused;

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

    public void Update()
    {
        if (InputField != null && InputField.text != string.Empty && Input.GetKeyDown(KeyCode.Return))
        {
            NetworkManager.Chat(InputField.text);
            InputField.text = "";
        }
        timer += Time.deltaTime;
    }

    /// <summary>
    /// Instantiate a text bubble prefab and set its values.
    /// </summary>
    public void WriteMessage(string name, string text, Vector3 pos)
    {
        if (timer > antiSpam) //Anti-spam
        {
            GameObject TextBubble = Instantiate(TextBubblePrefab, pos + Vector3.up * 2, Quaternion.identity, null);
            TextBubble bubble = TextBubble.GetComponent<TextBubble>();
            bubble.PlayerName = name;
            bubble.TextInBubble = text;
            bubble.BubbleColor = MessageDiferentsColors[CurrentMessageColor];
            bubble.BubbleSetUp();
            timer = 0.0f;
        }
    }

    /// <summary>
    /// Change the color of the input field and the color picker image to the next color in the array.
    /// </summary>
    public void ChangeMessageColor()
    {
        if (CurrentMessageColor >= (MessageDiferentsColors.Length - 1))
        {
            CurrentMessageColor = 0;
        }
        else
        {
            CurrentMessageColor++;
        }
    }
}