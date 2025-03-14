using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        SetCurrentLevelIndex();
    }

    public void InitializeGame()
    {
        if (currentLevelIndex < levelDataArray.levelDatas.Length)
        {
            currentLevel = levelDataArray.levelDatas[currentLevelIndex];
        }
        else
        {
            Debug.Log("Levels finished, Add more levels to continue");
            return;
        }

        //Reset Score Value
        Score = 0;
        TurnCount = 0;
        MatchCount = 0;
        StreakCount = -1;

        cardCountToMatch = currentLevel.cardCountToMatch;

        //Set Up Ui for the level
        customGridLayout.SetUpUIForLevel(currentLevel);

        //Randomize Image To Cell
        AddImageToTheCells();
    }

    private void AddImageToTheCells()
    {
        int totalCellsToPopulate = currentLevel.rows * currentLevel.columns;
        int indexOfImage = 0;
        cellsUsed.Clear();
        // Populate Grid with Items
        for (int i = 0; i < totalCellsToPopulate; i++)
        {
            Cell newCell = Instantiate(cellPrefab, customGridLayout.transform);
            newCell.name = "Cell_" + i;
            if (i % cardCountToMatch == 0)
            {
                indexOfImage = Random.Range(0, spriteList.sprites.Length);
            }
            newCell.cardImage.sprite = spriteList.sprites[indexOfImage];
            newCell.spriteId = indexOfImage;
            cellsUsed.Add(newCell);
        }
        RandomizeCellAndParent();
    }

    public void RandomizeCellAndParent()
    {
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

    public void SetCurrentLevelIndex()
    {
        //Read from playerprefs
        if (PlayerPrefs.HasKey("currentLevelIndex"))
        {
            currentLevelIndex = PlayerPrefs.GetInt("currentLevelIndex", 0);
        }
        else
        {
            currentLevelIndex = 0;
        }
        UIManager.Instance.UpdateLevelText(currentLevelIndex + 1);
    }

    public void RemoveCompletedCell(Cell cellToRemove)
    {
        cellsUsed.Remove(cellToRemove);
        if (cellsUsed.Count == 0)
        {
            StartCoroutine(ShowWinPanel());
        }
    }

    public void GiveStreakScoreIfAny()
    {
        if(StreakCount > 0){
            Score += currentLevel.streakScoreMultiplier * StreakCount;
        }
    }

    public void CheckMatch(){
        if(selectedCells.Count != cardCountToMatch) {
            return;
        }

        int spriteId = -1;
        bool isMatched = false;
        for(int i = 0; i < cardCountToMatch; i++){
            if(spriteId == -1){
                spriteId = selectedCells[i].spriteId;
                isMatched = false;
            } else if(spriteId == selectedCells[i].spriteId) {
                isMatched = true;
            } else {
                isMatched = false;
                break;
            }
        }
        
        if(isMatched){
            AudioManager.Instance.PlayAudio(matchAudioClip);
            DisableCards();
            Score += currentLevel.matchScore;
            StreakCount++;
        } else {
            AudioManager.Instance.PlayAudio(misMatchAudioClip);
            StreakCount = -1;
            GiveStreakScoreIfAny();
            FlipSelectedCards();
        }
        selectedCells.Clear();
    }

    private void DisableCards(){
        foreach(Cell cell in selectedCells){
            StartCoroutine(cell.RemoveCard());
            RemoveCompletedCell(cell);
        }
    }

    private void FlipSelectedCards(){
        foreach(Cell cell in selectedCells){
            StartCoroutine(cell.FlipCard());
        }
    }

    private IEnumerator ShowWinPanel(){
        GiveStreakScoreIfAny();
        yield return new WaitForSeconds(1f);
        UIManager.Instance.ShowWinPanel();
        //Update Level Index
        currentLevelIndex++;
        PlayerPrefs.SetInt("currentLevelIndex", currentLevelIndex);
        UIManager.Instance.UpdateLevelText(currentLevelIndex + 1);
    }
}
