using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PremiseRenderer : MonoBehaviour
{
    public string text;
    Image cellImage;
    TextMeshProUGUI textHolder;
    public Vector2 top;
    public Vector2 bottom;
    RectTransform rectTransform;


    private void Awake()
    {
        cellImage = GetComponentInChildren<Image>();
        textHolder = GetComponentInChildren<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("test "+cellImage.transform.position.y);
        //Debug.Log(cellImage.rectTransform.rect.height);
        var rect = cellImage.rectTransform.rect;

        top = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + rect.height * 0.5f);
        //Debug.Log(top);
        bottom = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y - rect.height * 0.5f);
        //Debug.Log(bottom);
        //top = new Vector2(rect.x + rect.xMax + transform.position.x, rect.yMax + transform.position.y);
        //Image.rectTransform.rect.y); // ;  ;//new Vector2(cellImage.rectTransform.rect.x, cellImage.rectTransform.rect. + cellImage.rectTransform.rect.height * 0.5f);
        //bottom = new Vector2(cellImage.rectTransform.rect.x, cellImage.rectTransform.rect.y - cellImage.rectTransform.rect.height * 0.5f);
        //bottom = new Vector2(rect.x + rect.xMax + transform.position.x, rect.y + transform.position.y);

        //Debug.Log("rectposy "+rect.position.y);
        //Debug.Log(cellImage.rectTransform.localScale.y);
        //Debug.Log(cellImage.rectTransform.position.y);
        // Debug.Log(cellImage.transform.localPosition.y);
        //Debug.Log(cellImage.transform.position.y);
        //Debug.Log(cellImage.rectTransform.anchoredPosition.y);

        textHolder.text = text;
        //Debug.Log(top.x+" "+top.y+" "+bottom.x+" "+bottom.y);
        
    }

    public void ResetTopBottom()
    {
        var rect = cellImage.rectTransform.rect;
        top = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + rect.height * 0.5f);
        bottom = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y - rect.height * 0.5f);
    }
}
