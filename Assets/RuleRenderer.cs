using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuleRenderer : MonoBehaviour
{
    private Image lineImage;
    RectTransform rectTransform;

    GameObject vlinePrefab;
    GameObject hlinePrefab;


    private void Awake()
    {
        lineImage = GetComponentInChildren<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Connect(PremiseRenderer consequent, PremiseRenderer antecedent)
    {
        float vdistance = consequent.bottom.y - antecedent.top.y;
        Debug.Log(vdistance);
        rectTransform.anchoredPosition = new Vector2(consequent.bottom.x, consequent.bottom.y - vdistance * 0.5f);
        //lineImage.rectTransform.anchoredPosition = new Vector2(lineImage.rectTransform.anchoredPosition.x, a.bottom.y - vdistance * 0.5f);
        lineImage.rectTransform.sizeDelta = new Vector2(lineImage.rectTransform.rect.width, vdistance);
            //rect.height = SetSizeWithCurrentAnchors(lineImage.rectTransform.rect., = new Vector2(lineImage.rectTransform.width)
    }

    public void Connect(PremiseRenderer consequent, List<PremiseRenderer> antecedents)
    {
        float vdistance = consequent.bottom.y - antecedents[0].top.y;

        //Create top line
        GameObject topvline = Instantiate(vlinePrefab);
        RectTransform vlineRectTransform = topvline.GetComponent<RectTransform>();
        vlineRectTransform.anchoredPosition = new Vector2(consequent.bottom.x, consequent.bottom.y - vdistance * 0.25f);

        Image topvlineImage = topvline.GetComponentInChildren<Image>();
        topvlineImage.rectTransform.sizeDelta = new Vector2(topvlineImage.rectTransform.rect.width, vdistance);

        //create bottom vlines
        foreach (PremiseRenderer antecedent in antecedents)
        {
            GameObject bottomvline = Instantiate(vlinePrefab);
            Image bottomvlineImage = topvline.GetComponentInChildren<Image>();
            bottomvlineImage.rectTransform.sizeDelta = new Vector2(bottomvlineImage.rectTransform.rect.width, vdistance);
            RectTransform bottomvlineRectTransform = bottomvline.GetComponent<RectTransform>();
            bottomvlineRectTransform.anchoredPosition = new Vector2(antecedent.top.x, antecedent.top.y - vdistance * 0.25f);
        }

        //create hline


    }


}
