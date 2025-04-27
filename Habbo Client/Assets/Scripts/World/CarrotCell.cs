using System.Collections;
using UnityEngine;

public class CarrotCell : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && spriteRenderer.enabled)
        {
            // check if player is near the carrot
            if (Vector3.Distance(transform.position, NetworkManager.CurrentPlayerController.transform.position) < 1.5f)
            {
                // add carrot to the container
                CarrotManager.Instance.AddCarrot();

                MusicManager.Instance.PlaySFX(SFX.Grab);
                // hide for a while
                StartCoroutine(HideCarrot());
            }
        }
    }

    IEnumerator HideCarrot()
    {
        spriteRenderer.enabled = false;
        yield return new WaitForSeconds(Random.Range(8, 16));
        spriteRenderer.enabled = true;
    }
}