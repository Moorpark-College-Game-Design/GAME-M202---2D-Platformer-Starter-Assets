using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    [SerializeField] float levelLoadDelay = 2.0f;

    [SerializeField] float sloMoFactor = 0.2f;

    void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        Time.timeScale = sloMoFactor;

        yield return new WaitForSecondsRealtime(levelLoadDelay);

        Time.timeScale = 1.0f;

        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SceneManager.LoadScene(currentSceneIndex + 1);
    }

}
