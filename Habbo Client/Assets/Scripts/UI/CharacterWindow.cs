using TMPro;
using UnityEngine.UI;

public class CharacterWindow : WindowController
{
    public TMP_InputField NameField;
    public Image AvatarImage;

    public override void OpenWindow()
    {
        base.OpenWindow();

        NameField.text = NetworkManager.CurrentPlayer.Name;
        NameField.Select();
        NameField.ActivateInputField();

        PlayerSkin.Instance.SetColor(AvatarImage.material, NetworkManager.CurrentPlayer.Color);
    }

    // on value changed
    public void OnNameFieldValueChanged(string value)
    {
        NetworkManager.AskUpdatePlayerName(value);
    }
}