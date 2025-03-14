using System.Collections.Generic;
using UnityEngine;

public enum GameState {
    // MainMenu,
    Idle,
    Selected,
}


public class GameManager : MonoBehaviour
{
    #region Serializeable Fileds

    //Scriptable object reference for level array
    [SerializeField]
    private LevelDataArray levelDataArray;

    //Scriptable object reference for sprite list 
    [SerializeField]
    private SpriteList spriteList;

    //Refer custom grid, To Do: Refactor if possible
    [SerializeField]
    private CustomGridLayout customGridLayout;

    [SerializeField]
    AudioClip matchAudioClip;

    [SerializeField]
    AudioClip misMatchAudioClip;

    [SerializeField]
    AudioClip winAudioClip;

    #endregion

    #region Private Fields

    //Current Level Index
    private int currentLevelIndex;

    //Current Level Data
    private LevelData currentLevel;

    private int cardCountToMatch;

    private List<Cell> cellsUsed;

    #endregion

    #region Public Fields

    //Cell prefab
    [HideInInspector]
    public Cell cellPrefab;

    //Tapped Cell
    public Cell selectedCell;
    [HideInInspector]
    public List<Cell> selectedCells;

    private int score = 0;
    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
            UIManager.Instance.UpdateScoreText(score);
        }
    }

    //To Keep Track of game state
    public GameState gameState;
    private int turnCount = 0;
    public int TurnCount
    {
        get
        {
            return turnCount;
        }
        set
        {
            turnCount = value;
            UIManager.Instance.UpdateTurnText(turnCount);
        }
    }

    private int matchCount = 0;
    public int MatchCount
    {
        get
        {
            return matchCount;
        }
        set
        {
            matchCount = value;
            UIManager.Instance.UpdateMatchText(matchCount);
        }
    }
    private int streakCount = 0;
    public int StreakCount
    {
        get
        {
            return streakCount;
        }
        set
        {
            streakCount = value;
        }
    }
    #endregion

    #region SingleTon Setup

    // Setting Singleton
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    GameObject go = new GameObject("Game Manager");
                    instance = go.AddComponent<GameManager>();
                }
            }

            return instance;
        }
    }

    #endregion

    private void Awake()
    {
        gameState = GameState.Idle;
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        cellsUsed = new();
    }

    private void Start()
    {
        InitializeGame();
    }

    void InitializeGame(){
        //Read from playerprefs
        
        // if(PlayerPrefs.HasKey("currentLevelIndex")){
        //     currentLevelIndex = PlayerPrefs.GetInt("currentLevelIndex",0); 
        // } else {
        //     currentLevelIndex = 0;
        // }

        //Set current Level
        if(currentLevelIndex < levelDataArray.levelDatas.Length) {
            currentLevel = levelDataArray.levelDatas[currentLevelIndex];
        } else {
            Debug.Log("Levels finished, Add more levels to continue");
            return;
        }
        
        //Set Up Ui for the level
        customGridLayout.SetUpUIForLevel(currentLevel);

        //Randomize Image To Cell
        AddImageToTheCells();
    }

    public void LoadNextLevel(){
        //Update Level Index
        currentLevelIndex++;
        // PlayerPrefs.SetInt("currentLevelIndex",currentLevelIndex); 

        InitializeGame();
    }


    private void AddImageToTheCells(){
        int totalCellsToPopulate = currentLevel.rows * currentLevel.columns;
        int indexOfImage = 0;
        // Populate Grid with Items
        for (int i = 0; i < totalCellsToPopulate; i++)
        {
            Cell newCell = Instantiate(cellPrefab, customGridLayout.transform);
            newCell.name = "Cell_" + i;
            if(i % 2 == 0){
                indexOfImage = Random.Range(0,spriteList.sprites.Length);
            }
            newCell.CardImage.sprite = spriteList.sprites[indexOfImage];
            newCell.spriteId = indexOfImage;
        }
        RandomizeCellAndParent();
    }

    public void RandomizeCellAndParent(){
        List<Transform> children = new();
        for (int i = 0; i < customGridLayout.transform.childCount; i++)
        {
            children.Add(customGridLayout.transform.GetChild(i));
        }

        // Shuffle the list
        IListExtensions.Shuffle(children);

        // Reassign sibling indices based on the shuffled list
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }
}
