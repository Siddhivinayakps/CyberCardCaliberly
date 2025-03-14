using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private List<int> generatedRandomIndexes = new List<int>();

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

    private int highestStreak = -1;

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
        
        SaveLoadManager.LoadGame();
        if(SaveLoadManager.playerLevelArray == null){
            currentLevelIndex = 0;
        } else {
            currentLevelIndex = SaveLoadManager.playerLevelArray.playerLevelDataArray.Count();
        }

        if(currentLevelIndex >= levelDataArray.levelDatas.Count())
            currentLevelIndex = 0;
    }

    private void Start()
    {
        cellsUsed = new();
        UIManager.Instance.UpdateLevelText(currentLevelIndex + 1);

        if(currentLevelIndex == 0) {
            UIManager.Instance.LeftButtonState(false);
            UIManager.Instance.RightButtonState(false);
        } else {
            UIManager.Instance.LeftButtonState(true);
            UIManager.Instance.RightButtonState(false);
        }
        UpdateMenuUI();
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
        generatedRandomIndexes.Clear();
        // Populate Grid with Items
        for (int i = 0; i < totalCellsToPopulate; i++)
        {
            Cell newCell = Instantiate(cellPrefab, customGridLayout.transform);
            newCell.name = "Cell_" + i;
            if (i % cardCountToMatch == 0)
            {
                indexOfImage = GetNonRepeatingRandomIndex(0, spriteList.sprites.Length);
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
        if(highestStreak > 0){
            Score += currentLevel.streakScoreMultiplier * highestStreak;
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
            MatchCount++;
        } else {
            AudioManager.Instance.PlayAudio(misMatchAudioClip);
            if(StreakCount > highestStreak && StreakCount > 0) {
                highestStreak = StreakCount;
                UIManager.Instance.UpdateLongestStreakText(highestStreak);
            }
            StreakCount = -1;
            FlipSelectedCards();
        }
        selectedCells.Clear();
        TurnCount++;

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
        if(highestStreak > 0)
            UIManager.Instance.UpdateLongestStreakText(highestStreak);
        UIManager.Instance.ShowCongatsText();
        GiveStreakScoreIfAny();
        yield return new WaitForSeconds(1.5f);
        PlayerLevelData playerLevelData = new()
        {
            levelNumber = currentLevel.levelNumber,
            matchScore = Score,
            turnCount = TurnCount
        };

        if(SaveLoadManager.playerLevelArray == null || currentLevelIndex >= SaveLoadManager.playerLevelArray.playerLevelDataArray.Count()){
            SaveLoadManager.playerLevelArray ??= new PlayerLevelArray();
            SaveLoadManager.playerLevelArray.playerLevelDataArray.Add(playerLevelData);
        } else {
            SaveLoadManager.playerLevelArray.playerLevelDataArray[currentLevelIndex] = playerLevelData;
        }
        SaveLoadManager.SaveGame();
        UIManager.Instance.ShowWinPanel();
        //Update Level Index
        if(currentLevelIndex < levelDataArray.levelDatas.Count() - 1)
            currentLevelIndex++;
        else
            currentLevelIndex = 0;
        
        UpdateMenuUI();        
    }

    public void ReduceCurrentLevelIndex(){
        if(currentLevelIndex <= 0)
            return;
        currentLevelIndex--;        
        UpdateMenuUI();
    }

    public void IncreaseCurrentLevelIndex(){
        if(currentLevelIndex >= levelDataArray.levelDatas.Count() - 1)
            return;
        currentLevelIndex++;
        UpdateMenuUI();
    }

    private void UpdateMenuUI(){
        int playerLevelCount = SaveLoadManager.playerLevelArray == null ? 0 : SaveLoadManager.playerLevelArray.playerLevelDataArray.Count();
        int totalLevelCont = levelDataArray.levelDatas.Count();
        int levelCountCondition = playerLevelCount < totalLevelCont ? playerLevelCount : totalLevelCont - 1;
        
        
        
        if(currentLevelIndex >= playerLevelCount) {
            UIManager.Instance.UpdateMenuLevelUIStat(new PlayerLevelData {matchScore = 0, turnCount = 0});
        } else {
            UIManager.Instance.UpdateMenuLevelUIStat(SaveLoadManager.playerLevelArray.playerLevelDataArray[currentLevelIndex]);
        }
        UIManager.Instance.UpdateLevelText(currentLevelIndex + 1);
        UIManager.Instance.LeftButtonState(true);
        UIManager.Instance.RightButtonState(true);
        if(currentLevelIndex <= 0){
            UIManager.Instance.LeftButtonState(false);
        }

        if(currentLevelIndex >= levelCountCondition){
            UIManager.Instance.RightButtonState(false);
        }


        UIManager.Instance.UpdateLevelNote(levelDataArray.levelDatas[currentLevelIndex].cardCountToMatch);
    }

    private int GetNonRepeatingRandomIndex(int firstInclusive, int lastExclusive){

        int rand = Random.Range(firstInclusive,lastExclusive);
        while(generatedRandomIndexes.Contains(rand))
        {
            rand = Random.Range(firstInclusive,lastExclusive);
        }
        generatedRandomIndexes.Add(rand);
        if(generatedRandomIndexes.Count == lastExclusive) generatedRandomIndexes.Clear();
        return rand;
    }
}
