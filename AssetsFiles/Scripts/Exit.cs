using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Exit : MonoBehaviour
{
    [SerializeField] float     loadNextLevelDelay = 1f;
    [SerializeField] AudioClip exitSound;

    public bool IsLevelCompleted { get; private set; } = false;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            IsLevelCompleted = true; //lock player's movement
            AudioSource.PlayClipAtPoint(exitSound, transform.position);

            var nextLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextLevelIndex < SceneManager.sceneCountInBuildSettings)
                StartCoroutine(LoadNextLevel());
            else //if no other scenes in build settings
                StartCoroutine(BackToFirstLevel());
        }
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(loadNextLevelDelay);
        FindObjectOfType<ScenePersist>().ResetScenePersist();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    IEnumerator BackToFirstLevel()
    {
        yield return new WaitForSecondsRealtime(loadNextLevelDelay);
        FindObjectOfType<ScenePersist>().ResetScenePersist();
        SceneManager.LoadScene(0);
    }
}
