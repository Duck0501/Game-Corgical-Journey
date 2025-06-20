using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject canvasHome;
    public GameObject canvasHelp;
    public GameObject canvasLevel;
    public GameObject canvasWin;
    public GameObject[] levelPrefabs;
    private GameObject currentLevel;

    public Transform levelParent;

    public Button buttonHelp;
    public Button buttonNewGame;
    public Button buttonHomeInHelp;
    public Button buttonHomeInLevel;
    public Button[] levelButtons; 

    void Start()
    {
        ShowCanvas(canvasHome);

        buttonHelp.onClick.AddListener(() => ShowCanvas(canvasHelp));
        buttonNewGame.onClick.AddListener(() => ShowCanvas(canvasLevel));
        buttonHomeInHelp.onClick.AddListener(() => ShowCanvas(canvasHome));
        buttonHomeInLevel.onClick.AddListener(() => ShowCanvas(canvasHome));

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1; 
            levelButtons[i].onClick.AddListener(() => LevelSelected(levelIndex));
        }
    }

    void ShowCanvas(GameObject canvasToShow)
    {
        canvasHome.SetActive(canvasToShow == canvasHome);
        canvasHelp.SetActive(canvasToShow == canvasHelp);
        canvasLevel.SetActive(canvasToShow == canvasLevel);
        canvasWin.SetActive(canvasToShow == canvasWin);
    }

    void LevelSelected(int level)
    {
        ShowCanvas(null);

        if (currentLevel != null)
            Destroy(currentLevel);

        if (level > 0 && level <= levelPrefabs.Length)
        {
            currentLevel = Instantiate(levelPrefabs[level - 1], levelParent);
        }

        Button homeButtonInLevel = currentLevel.GetComponentInChildren<Button>();
        if (homeButtonInLevel != null)
        {
            homeButtonInLevel.onClick.AddListener(() => ShowCanvas(canvasHome));
        }
    }
}