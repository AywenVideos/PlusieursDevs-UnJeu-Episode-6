using TMPro;
using UnityEngine;

public class MoneyTextManager : MonoBehaviour
{
    void Start()
    {
        EventManager.OnUpdateMoney += EventManager_UpdateMoney;
    }

    /// <summary>
    /// Update the money text.
    /// </summary>
    /// <param name="money"> The money to display.</param>
    private void EventManager_UpdateMoney(int money)
    {
        GetComponent<TMP_Text>().text = money.ToString();
    }
}