using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private TextMeshProUGUI matchText;

    [SerializeField]
    private TextMeshProUGUI turnText;

    [SerializeField]
    private TextMeshProUGUI levelText;

    [SerializeField]
    private TextMeshProUGUI congratsText;

    [SerializeField]
    private GameObject mainMenuPanel;

    [SerializeField]
    private Button leftButton;
    [SerializeField]
    private Button rightButton;

    [SerializeField]
    private TextMeshProUGUI menuScoreText;

    [SerializeField]
    private TextMeshProUGUI menuTurnText;

    [SerializeField]
    private TextMeshProUGUI noteText;

    private float fadeDuration = 1f;
    
    private static UIManager instance;

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();

                if (instance == null)
                {
                    GameObject go = new GameObject("UIManager");
                    instance = go.AddComponent<UIManager>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateLevelText(int levelNumber){
        levelText.text = $"Level - {levelNumber}";
    }

    public void UpdateScoreText(int score){
        scoreText.text = $"{score}";
    }

    public void UpdateMatchText(int matchCount){
        matchText.text = $"{matchCount}";
    }

    public void UpdateTurnText(int turnCount){
        turnText.text = $"{turnCount}";
    }

    public void ShowWinPanel(){
        mainMenuPanel.SetActive(true);
        StartCoroutine(FadeOutCongratulationsText());
    }

    public void RightButtonState(bool isInteractable){
        rightButton.interactable = isInteractable;
    }

    public void LeftButtonState(bool isInteractable){
        leftButton.interactable = isInteractable;
    }

    public void UpdateMenuLevelUIStat(PlayerLevelData playerLevelData){
        menuScoreText.text = $"Score: {playerLevelData.matchScore}";
        menuTurnText.text = $"Turns: {playerLevelData.turnCount}";
    }

    public void UpdateLevelNote(int matchCardCount){
        noteText.text = $"Match {matchCardCount} images to score";
    }

    IEnumerator FadeOutCongratulationsText(){
        congratsText.gameObject.SetActive(true);
        congratsText.color = new Color(congratsText.color.r, congratsText.color.g, congratsText.color.b, 1);
        while (congratsText.color.a > 0.0f)
        {
            congratsText.color = new Color(congratsText.color.r, congratsText.color.g, congratsText.color.b, congratsText.color.a - (Time.deltaTime / fadeDuration));
            yield return null;
        }
        congratsText.gameObject.SetActive(false);
    }
}
