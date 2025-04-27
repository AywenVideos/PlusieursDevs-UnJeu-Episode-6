using UnityEngine;
using System.Collections;

public class EasterEggManager : MonoBehaviour
{
    public Animator imageAnim;
    private KeyCode[] konamiCode = {
        KeyCode.UpArrow, KeyCode.UpArrow,
        KeyCode.DownArrow, KeyCode.DownArrow,
        KeyCode.LeftArrow, KeyCode.RightArrow,
        KeyCode.LeftArrow, KeyCode.RightArrow,
        KeyCode.B, KeyCode.Q
    };

    private int currentIndex = 0;
    private Coroutine pitchCoroutine;

    void Update()
    {
        KonamiCodeDetector();
    }

    void KonamiCodeDetector()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(konamiCode[currentIndex]))
            {
                currentIndex++;
                if (currentIndex == konamiCode.Length)
                {
                    StartCoroutine(ReducePitchOverTime());
                    currentIndex = 0;
                }
            }
            else
            {
                currentIndex = 0;
            }
        }
    }

    IEnumerator ReducePitchOverTime()
    {
        GameObject musicManager = GameObject.Find("MusicManager");
        AudioSource musicSource = musicManager.GetComponent<AudioSource>();
        musicSource.volume = 1.0f;

        while (musicSource.pitch > 0.1f)
        {
            musicSource.pitch -= 0.01f;
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(5.0f);
        MusicManager.Instance.PlaySFX(SFX.Scream);
        StartCoroutine(CameraShake());
        imageAnim.Play("Appear");
        yield return new WaitForSeconds(8.0f);

        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    IEnumerator CameraShake()
    {
        Vector3 originalPos = Camera.main.transform.position;

        float shakeMagnitude = 0.5f;
        float shakeDuration = 8.0f;

        float timeElapsed = 0f;

        while (timeElapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            Camera.main.transform.position = new Vector3(x, y, originalPos.z);
            timeElapsed += Time.deltaTime;

            shakeMagnitude = timeElapsed / 3;

            yield return null;
        }

        Camera.main.transform.position = originalPos;
    }
}

