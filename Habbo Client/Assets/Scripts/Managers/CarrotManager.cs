using UnityEngine;

public class CarrotManager : MonoBehaviour
{
    public static CarrotManager Instance;
    public Transform Container;
    public GameObject CarrotPrefab;
    public int CurrentCarrot = 0;
    public int maxCarrot = 10;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Add a carrot to the container.
    /// </summary>
    public void AddCarrot()
    {
        if (CurrentCarrot < maxCarrot)
        {
            GameObject carrot = Instantiate(CarrotPrefab, Container);
            carrot.transform.localPosition = new Vector3(0, 0, 0);
            CurrentCarrot++;
        }
    }

    /// <summary>
    /// Remove a carrot from the container.
    /// </summary>
    public void RemoveCarrot()
    {
        if (CurrentCarrot > 0)
        {
            Destroy(Container.GetChild(CurrentCarrot - 1).gameObject);
            CurrentCarrot--;
        }
    }
}