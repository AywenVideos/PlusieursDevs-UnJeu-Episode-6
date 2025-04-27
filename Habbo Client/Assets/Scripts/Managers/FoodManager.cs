using System.Collections;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    public static FoodManager Instance;
    public bool inInventory;
    public bool isSpawned;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        Instance = this;
        StartCoroutine(WaitThenSpawn());
    }

    public void TakeFood()
    {
        if (CarrotManager.Instance.CurrentCarrot > 0)
        {
            MusicManager.Instance.PlaySFX(SFX.Grab);
            CarrotManager.Instance.RemoveCarrot();
            inInventory = true;
            GetComponent<SpriteRenderer>().enabled = false;
            isSpawned = false;
        }
        else
        {
            NotificationManager.Instance.ShowNotification("Il vous faut des carottes pour préparer la boisson.");
        }
    }

    public void GiveFood()
    {
        MusicManager.Instance.PlaySFX(SFX.Coin);
        inInventory = false;
        StartCoroutine(WaitThenSpawn());
        NetworkManager.AddGold(Random.Range(5, 15));
    }

    public IEnumerator WaitThenSpawn()
    {
        yield return new WaitForSeconds(Random.Range(2, 5));
        GetComponent<SpriteRenderer>().enabled = true;
        isSpawned = true;
    }
}