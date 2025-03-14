using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cell : MonoBehaviour, IPointerDownHandler
{
    public GameObject imageHolder;

    public GameObject cardImageBg;
    
    public Image cardImage;

    public float flipDuration = 1f; // Time it takes to complete the flip

    [HideInInspector]
    public int spriteId;

    [HideInInspector]
    public bool isClickable;

    // Start is called before the first frame update
    void Start()
    {
        isClickable = false;
        ShowCardBack(false);
        StartCoroutine(FlipCard());
    }

    public IEnumerator FlipCard(){
        yield return new WaitForSeconds(1f);

        float elapsedTime = 0f;

        // Store the starting scale
        float startScaleX = imageHolder.transform.localScale.x;
        float startScaleY = imageHolder.transform.localScale.y;

        // Calculate the target scale
        // float targetScaleX = startScaleX * -1;
        float targetScaleY = startScaleY * -1;

        // Flip the image over time
        while (elapsedTime < flipDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate the current scale using linear interpolation (Lerp)
            // float currentScaleX = Mathf.Lerp(startScaleX, targetScaleX, elapsedTime / flipDuration);
            float currentScaleY = Mathf.Lerp(startScaleX, targetScaleY, elapsedTime / flipDuration);

            if(currentScaleY <= 0){
                ShowCardBack(true);
            }

            // Apply the scale to the transform
            imageHolder.transform.localScale = new Vector3(imageHolder.transform.localScale.x, currentScaleY, imageHolder.transform.localScale.z);
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        // Ensure the image is fully flipped at the end of the coroutine
        imageHolder.transform.localScale = new Vector3(imageHolder.transform.localScale.x, targetScaleY, imageHolder.transform.localScale.z);

        imageHolder.transform.localScale = Vector3.one;

        isClickable = true;
    }

    void ShowCardBack(bool toShow){
        cardImageBg.SetActive(!toShow);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!isClickable) 
            return;
        GameManager.Instance.selectedCells.Add(this);
        GameManager.Instance.CheckMatch();
        ShowCardBack(false);
        isClickable = false;
    }

    public IEnumerator RemoveCard(){
        yield return new WaitForSeconds(1f);
        imageHolder.SetActive(false);
    }
}
