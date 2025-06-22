using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject canvasHome;
    public GameObject canvasHelp;
    public GameObject canvasLevel;
    public GameObject canvasWin;
    public GameObject canvasLoad;
    public GameObject[] levelPrefabs;
    public GameObject currentLevel;

    public Transform levelParent;

    public Button buttonHelp;
    public Button buttonContinue;
    public Button buttonNewGame;
    public Button buttonHomeInHelp;
    public Button buttonHomeInLevel;
    public Button buttonReloadInWin;
    public Button buttonToHomeInWin;
    public Button buttonNextInWin;

    public Button[] levelButtons;

    public Text currentTimeText;
    public Text scoreText;
    public Text highScoreText;

    private float bestTime = float.MaxValue;

    private int currentLevelIndex = -1;

    private float currentTime = 0f;
    private bool isTiming = false;

    private Button skipButton;
    private bool skipButtonShown = false;

    void Start()
    {
        ShowCanvas(canvasHome);

        buttonHelp.onClick.AddListener(() => ShowCanvas(canvasHelp));
        buttonNewGame.onClick.AddListener(() => LevelSelected(1));
        buttonContinue.onClick.AddListener(() => ShowCanvas(canvasLevel));
        buttonHomeInHelp.onClick.AddListener(() => ShowCanvas(canvasHome));
        buttonHomeInLevel.onClick.AddListener(() => ShowCanvas(canvasHome));

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1;
            levelButtons[i].onClick.AddListener(() => LevelSelected(levelIndex));
        }

        buttonReloadInWin.onClick.AddListener(ResetCurrentLevel);
        buttonNextInWin.onClick.AddListener(LoadNextLevelManual);
        buttonToHomeInWin.onClick.AddListener(() => ShowCanvas(canvasHome));
    }

    void Update()
    {
        bool isAnyCanvasActive = canvasHome.activeSelf
                           || canvasHelp.activeSelf
                           || canvasLevel.activeSelf
                           || canvasWin.activeSelf
                           || canvasLoad.activeSelf;

        if (Input.GetKeyDown(KeyCode.R) && !isAnyCanvasActive && currentLevelIndex >= 0 && currentLevelIndex < levelPrefabs.Length)
        {
            ResetCurrentLevel();
        }

        if (isTiming && currentTimeText != null)
        {
            currentTime += Time.deltaTime;

            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            currentTimeText.text = $"{minutes:00}:{seconds:00}";

            if (!skipButtonShown && currentTime >= 30f && skipButton != null)
            {
                skipButton.gameObject.SetActive(true);
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(LoadNextLevelManual);
                skipButtonShown = true;

                skipButton.transform.localScale = Vector3.one;
                skipButton.transform.DOScale(1.2f, 1f).SetLoops(-1, LoopType.Yoyo);
            }
        }
    }

    public void ShowCanvas(GameObject canvasToShow)
    {
        canvasHome.SetActive(canvasToShow == canvasHome);
        canvasHelp.SetActive(canvasToShow == canvasHelp);
        canvasLevel.SetActive(canvasToShow == canvasLevel);
        canvasWin.SetActive(canvasToShow == canvasWin);
    }

    public void LevelSelected(int level)
    {
        StartCoroutine(LoadLevelRoutine(level));
    }

    private System.Collections.IEnumerator LoadLevelRoutine(int level)
    {
        currentLevelIndex = level - 1;

        canvasHome.SetActive(false);
        canvasHelp.SetActive(false);
        canvasLevel.SetActive(false);
        canvasWin.SetActive(false);

        canvasLoad.SetActive(true);
        CanvasGroup loadGroup = canvasLoad.GetComponent<CanvasGroup>();
        loadGroup.alpha = 1;

        if (currentLevel != null) Destroy(currentLevel);

        skipButton = null;

        if (level > 0 && level <= levelPrefabs.Length)
        {
            currentLevel = Instantiate(levelPrefabs[level - 1], levelParent);

            Button[] buttons = currentLevel.GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                if (btn.name.Contains("home"))
                    btn.onClick.AddListener(() => ShowCanvas(canvasHome));
                if (btn.name.Contains("replay"))
                    btn.onClick.AddListener(ResetCurrentLevel);
                if (btn.name.ToLower().Contains("skip"))
                {
                    skipButton = btn;
                    skipButton.gameObject.SetActive(false);
                }
            }
        }

        skipButtonShown = false;

        currentTime = 0f;
        isTiming = true;

        if (currentTimeText != null) currentTimeText.text = "00:00";

        yield return new WaitForSeconds(1f);
        yield return loadGroup.DOFade(0, 1f).WaitForCompletion();

        canvasLoad.SetActive(false);
    }

    public void ResetCurrentLevel()
    {
        if (currentLevelIndex >= 0 && currentLevelIndex < levelPrefabs.Length)
        {
            LevelSelected(currentLevelIndex + 1);
        }
    }

    public void LoadNextLevelManual()
    {
        if (currentLevelIndex + 1 < levelPrefabs.Length)
        {
            LevelSelected(currentLevelIndex + 2);
        }
        else
        {
            ShowCanvas(canvasHome);
        }
    }

    public void OnLevelCompleted()
    {
        isTiming = false;

        scoreText.text = $"Score: {FormatTime(currentTime)}";

        if (currentTime < bestTime)
        {
            bestTime = currentTime;
        }

        highScoreText.text = bestTime == float.MaxValue
            ? "High Score: --:--"
            : $"High Score: {FormatTime(bestTime)}";

        ShowCanvas(canvasWin);
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
