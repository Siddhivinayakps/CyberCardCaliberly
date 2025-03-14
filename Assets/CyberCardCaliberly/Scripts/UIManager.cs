using TMPro;
using UnityEngine;

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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        congratsText.gameObject.SetActive(true);
    }
}
