using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public enum Difficulty { Low, High }

public class GameManager : Singleton<GameManager>
{
    [Header("Difficulty")]
    [SerializeField] public Difficulty difficulty = Difficulty.Low;

    [SerializeField] private int livesLow = 5;
    [SerializeField] private int livesHigh = 3;

    [SerializeField] private float enemySpeedMulLow = 0.7f;
    [SerializeField] private float enemySpeedMulHigh = 1.0f;

    [SerializeField] private float spawnIntervalMulLow = 2f; 
    [SerializeField] private float spawnIntervalMulHigh = 1f;

    [Header("Scenes")]
    [SerializeField] private string firstSceneName = "FirstScene";
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string LevelScene = "LevelScene";


    [Header("Levels")]
    [SerializeField] private int numLevels = 3;

    [Header("Timing")]
    [SerializeField] private float splashSeconds = 5f;
    [SerializeField] private float transitionSeconds = 1f;
    [Header("Audio")]
    
    
    [SerializeField] private AudioSource StartLevel;
    [SerializeField] private AudioSource Background;
    [SerializeField] private AudioSource WinLevel;


    public int level { get; private set; } = 1;
    public int lives { get; private set; }


    public float EnemySpeedMul =>
        (difficulty == Difficulty.High) ? enemySpeedMulHigh : enemySpeedMulLow;

    public float SpawnIntervalMul =>
        (difficulty == Difficulty.High) ? spawnIntervalMulHigh : spawnIntervalMulLow;

   


private void Start()
    {
        var active = SceneManager.GetActiveScene().name;
        if (active == firstSceneName)
            StartCoroutine(FirstSceneFlow());
    }

        public void SetDifficultyLow()
    {
        difficulty = Difficulty.Low;
        lives = livesLow;
      
    }

    public void SetDifficultyHigh()
    {
        difficulty = Difficulty.High;
        lives = livesHigh;
        
    }


    private IEnumerator FirstSceneFlow()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, splashSeconds));
        SceneManager.LoadScene(menuSceneName);
    }

    public void OnStartButton()
    {
        SceneManager.LoadScene(LevelScene);
        
    }
    public void OnQuitButton()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnSelectHighLevelButton()
    {
        SetDifficultyHigh();
        NewGame();
        if (Background.isPlaying) Background.Stop();
        StartLevel.Play();
    }

    public void OnSelectLowLevelButton()
    {
        SetDifficultyLow();
        NewGame();
        if (Background.isPlaying) Background.Stop();
        StartLevel.Play();

    }


    public void OnContinueButton()
    {
        if(WinLevel.isPlaying) WinLevel.Stop();
        LoadLevel();
        StartLevel.Play();
    }
    public void OnMainMenuButton()
    {
        if(WinLevel.isPlaying) WinLevel.Stop();
        SceneManager.LoadScene(menuSceneName);
        Background.Play();
        
    }
    


    void NewGame()
    {
        lives = (difficulty == Difficulty.High) ? livesHigh : livesLow;
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
        WinLevel.Play();
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
