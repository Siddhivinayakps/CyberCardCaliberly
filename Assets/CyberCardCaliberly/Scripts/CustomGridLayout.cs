using UnityEngine;
using UnityEngine.UI;
public class CustomGridLayout : MonoBehaviour
{
    public RectTransform rectTransform;    
    
    public Vector2 spacing = new Vector2(10, 10);

    void Start()
    {
        
    }

    void OnDisable()
    {
        ClearExistingCells();
    }


    public void SetUpUIForLevel(LevelData currentLevel){
        //Clear cells if added
        ClearExistingCells();

        // Get or Add Grid Layout Group
        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        if (grid == null)
            grid = gameObject.AddComponent<GridLayoutGroup>();


        int rows = currentLevel.rows;
        int columns = currentLevel.columns;

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellWidth = parentWidth / (float)columns - ((spacing.x / (float)columns) * (columns - 1));
        float cellHeight = parentHeight / (float)rows - ((spacing.y / (float)rows) * (rows - 1));

        // Set Grid Layout Properties
        grid.cellSize = new Vector2(cellWidth, cellHeight);
        grid.spacing = spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
    }

    private void ClearExistingCells(){
        for(int i = 0; i < rectTransform.childCount; i++){
            Destroy(rectTransform.GetChild(i).gameObject);
        }
    }
}
