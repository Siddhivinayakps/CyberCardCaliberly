using System.Collections.Generic;
using UnityEngine;

public enum GameState {
    // MainMenu,
    Idle,
    Selected,
}


public class GameManager : MonoBehaviour
{
    //Current Level Index
    int currentLevelIndex;

    //Scriptable object reference for level array
    [SerializeField]
    LevelDataArray levelDataArray;

    //Scriptable object reference for sprite list 
    [SerializeField]
    SpriteList spriteList;

    //Current Level Data
    LevelData currentLevel;

    //Refer custom grid, To Do: Refactor if possible
    [SerializeField]
    CustomGridLayout customGridLayout;

    //Cell prefab
    public Cell cellPrefab;

    //Tapped Cell
    public Cell selectedCell;

    //To Keep Track of game state
    public GameState gameState;

    // Setting Singleton
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("Game Manager");
                    _instance = go.AddComponent<GameManager>();
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        gameState = GameState.Idle;
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
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
