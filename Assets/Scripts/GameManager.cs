using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    [Header("Scenes")]
    [SerializeField] private string firstSceneName = "FirstScene";
    [SerializeField] private string menuSceneName = "MainMenu";
   

    [Header("Levels")]
    [SerializeField] private int numLevels = 3;

    [Header("Timing")]
    [SerializeField] private float splashSeconds = 5f;
    [SerializeField] private float transitionSeconds = 1f;

    public int level { get; private set; } = 1;
    public int lives { get; private set; } = 3;

    private void Start()
    {
        var active = SceneManager.GetActiveScene().name;
        if (active == firstSceneName)
            StartCoroutine(FirstSceneFlow());
    }
    
    private IEnumerator FirstSceneFlow()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, splashSeconds));
        SceneManager.LoadScene(menuSceneName);
    }

    public void OnStartButton()
    {
        lives = 3;
        NewGame();
    }
    public void OnQuitButton()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void OnContinueButton()
    {
        LoadLevel();
    }
    private void NewGame()
    {
        lives = 3;
        level = 1;
        LoadLevel();
    }

    private void LoadLevel()
    {
        if (level > numLevels)
            level = 1;

        var cam = Camera.main;
        if (cam) cam.cullingMask = 0;

        StartCoroutine(LoadLevelAfterDelay());
    }

    private IEnumerator LoadLevelAfterDelay()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, transitionSeconds));
        SceneManager.LoadScene($"Level{level}");
    }

    public void LevelComplete()
    {
        level++;
        if (level > numLevels)
        {
            SceneManager.LoadScene("WinningScene");
        }
        else
        {
            SceneManager.LoadScene("BetwenLevelsWin");
          
            
        }
        
        
       
    }

    public void LevelFailed()
    {
        lives--;
        if (lives <= 0)
        {
            SceneManager.LoadScene("LosingScene");
        }
        else
        {            
            SceneManager.LoadScene("BetwenLevelsLose");
            
        }
    }
}
