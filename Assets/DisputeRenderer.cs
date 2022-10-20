using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
//using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
//using System.Diagnostics;
using System.Linq;

public class DisputeRenderer : MonoBehaviour
{
    public float animationspeed = 0f;
    public static DisputeRenderer Instance;
    //TEMPORARY
    //public GameObject premiseA;
    //PremiseRenderer a;
   // public List<PremiseRenderer> premiseBs;
    //public GameObject premiseB;
    //PremiseRenderer b;

    //public GameObject simpleRule;
    //private RuleRenderer ruleRenderer;

    //Permanent:
    public Transform parent;
    public GameObject vlinePrefab;

    public GameObject premisePrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        //ruleRenderer = simpleRule.GetComponent<RuleRenderer>();
        // a = premiseA.GetComponent<PremiseRenderer>();
        // b = premiseB.GetComponent<PremiseRenderer>();
    }

    // Start is called before the first frame update
    Dictionary<SidedNode, PremiseRenderer> premiseRenderers;

    void Start()
    {
        //Showcase different scenarios here
        //ShowcaseScenarioArgument1();
        ShowcaseScenarioRecord();

    }

    public void ShowCaseGeneratedDispute()
    {
        Environment environment = new Environment(Module.On, Module.On, Module.Off, Module.On);
        environment.InitializeAgents(new List<Agent>() {
            new Agent (Side.Proponent,ConcealmentModule.ArgumentType.All),
            new Agent (Side.Opponent, ConcealmentModule.ArgumentType.All) });
        DisputeGenerator disputeGenerator = new DisputeGenerator();
        List<Dispute> disputes = disputeGenerator.GenerateDisputes(1, MaxArgumentSize.Short, MaxDisputeSize.Medium, MaxBranches.Some);
        environment.RunDisputes(disputes);
        //environment.dispute.Print();
        Structure structure = new Structure(disputes[0]);
        ShowGraph(structure.mergedGraph);
    }


    public void ShowcaseScenarioArgument1()
    {
        Environment environment = new Environment(Module.On, Module.On, Module.Off, Module.On);
        environment.InitializeAgents(new List<Agent>() {
            new Agent (Side.Proponent,ConcealmentModule.ArgumentType.All),
            new Agent (Side.Opponent, ConcealmentModule.ArgumentType.All) });
        Dispute dispute = ScenarioManager.LoadScenarioArgument();
        environment.RunDisputes(new List<Dispute>() { dispute });
        //environment.dispute.Print();
        Structure structure = new Structure(dispute);
        ShowGraph(structure.mergedGraph);
    }

    public void ShowcaseScenarioArgument2()
    {
        Environment environment = new Environment(Module.On, Module.On, Module.Off, Module.On);
        environment.InitializeAgents(new List<Agent>() {
            new Agent (Side.Proponent,ConcealmentModule.ArgumentType.All),
            new Agent (Side.Opponent, ConcealmentModule.ArgumentType.All) });
        Dispute dispute = ScenarioManager.LoadScenarioArgument2();
        environment.RunDisputes(new List<Dispute>() { dispute });
        //environment.dispute.Print();
        Structure structure = new Structure(dispute);
        ShowGraph(structure.mergedGraph);
    }

    public void ShowcaseScenarioRecord()
    {
        Environment environment = new Environment(Module.On, Module.On, Module.Off, Module.On);
        environment.InitializeAgents(new List<Agent>() {
            new Agent (Side.Proponent,ConcealmentModule.ArgumentType.All),
            new Agent (Side.Opponent, ConcealmentModule.ArgumentType.All) });
        Dispute dispute = ScenarioManager.LoadScenarioRecord();
        environment.RunDisputes(new List<Dispute>() { dispute });
        //environment.dispute.Print();
        Structure structure = new Structure(dispute);
        ShowGraph(structure.mergedGraph);
    }

    public void ShowcaseLargeDispute()
    {
        Environment environment = new Environment(Module.On, Module.On, Module.Off, Module.On);
        environment.InitializeAgents(new List<Agent>() {
            new Agent (Side.Proponent,ConcealmentModule.ArgumentType.All),
            new Agent (Side.Opponent, ConcealmentModule.ArgumentType.All) });
        DisputeGenerator disputeGenerator = new DisputeGenerator();
        List<Dispute> disputes = disputeGenerator.GenerateDisputes(1, MaxArgumentSize.Long, MaxDisputeSize.Medium, MaxBranches.Some);
        environment.RunDisputes(disputes);
        //environment.dispute.Print();
        Structure structure = new Structure(disputes[0]);
        ShowGraph(structure.mergedGraph);
    }


    public void ShowGraph(SidedGraph _graph)
    {
        premiseRenderers = ShowAllPremises(_graph);
        ShowAllRules(_graph);
        ShowAllAttacks(_graph);
    }

    public float lineWidth = 9f;

    public Dictionary<SidedNode, PremiseRenderer> ShowAllPremises(SidedGraph _graph)
    {
        Dictionary<SidedNode,PremiseRenderer> l = new Dictionary<SidedNode, PremiseRenderer>();
        foreach(SidedNode sidedNode in _graph.nodes)
        {
            GameObject node = Instantiate(premisePrefab, parent);
            PremiseRenderer premiseRenderer = node.GetComponent<PremiseRenderer>();
            RectTransform rectTransform = node.GetComponent<RectTransform>();
            
            premiseRenderer.name = sidedNode.term.Convert();
            premiseRenderer.text = sidedNode.term.Convert();

            rectTransform.anchoredPosition = new Vector2(sidedNode.x * 200, sidedNode.y * -80);
            premiseRenderer.ResetTopBottom();
            l[sidedNode] =  premiseRenderer;
        }
        return l;
    }

    public void ShowAllRules(SidedGraph _graph)
    {
        Debug.Log(_graph.edges.Count);
        foreach (SidedEdge sidedEdge in _graph.edges)
        {
            //Debug.Log(sidedEdge.consequent.Convert());
            //Debug.Log(sidedEdge.consequent.Convert());
            if (sidedEdge.antecedents.Count == 1)
            {
                ShowSimpleRule(sidedEdge);
            }
            else
            {
                //ShowSimpleRule(sidedEdge.consequent, sidedEdge.antecedents[0]);
                ShowDividedRule(sidedEdge);
            }
        }
    }

    public void ShowSimpleRule(SidedEdge _sidedEdge)
    {
        PremiseRenderer consequent = premiseRenderers[_sidedEdge.consequentNode];// premiseRenderers.Find(premiseRenderer => premiseRenderer.text == _c.Convert());
        PremiseRenderer antecedent = premiseRenderers.Values.ToList().Find(premiseRenderer => premiseRenderer.text == _sidedEdge.antecedents[0].Convert());

        float vdistance = consequent.bottom.y - antecedent.top.y;

        GameObject vline = Instantiate(vlinePrefab, parent);
        RectTransform vlineRectTransform = vline.GetComponent<RectTransform>();
        consequent.ResetTopBottom();
        vlineRectTransform.anchoredPosition = new Vector2(consequent.bottom.x, consequent.bottom.y - vdistance * 0.5f);
        Debug.Log(consequent.bottom.y);

        Image vlineImage = vline.GetComponentInChildren<Image>();
        vlineImage.rectTransform.sizeDelta = new Vector2(lineWidth, vdistance);


        //Connect(consequent, new List<PremiseRenderer>() { antecedent });
        //consequent.bottom, antecedent.top;
    }

    public void ShowDividedRule(SidedEdge _sidedEdge)
    {
        PremiseRenderer topTerm = premiseRenderers[_sidedEdge.consequentNode];
        foreach(Term antecedent in _sidedEdge.antecedents)
        {
            PremiseRenderer bottomTerm = premiseRenderers.Values.ToList().Find(premiseRenderer => premiseRenderer.text == antecedent.Convert());

            float vdistance = topTerm.bottom.y - bottomTerm.top.y;

            //Top Half
            GameObject topvline = Instantiate(vlinePrefab, parent);
            RectTransform topvlineRectTransform = topvline.GetComponent<RectTransform>();
            topvlineRectTransform.anchoredPosition = new Vector2(topTerm.bottom.x, topTerm.bottom.y - vdistance * 0.25f);
            Image topvlineImage = topvline.GetComponentInChildren<Image>();
            //topvlineImage.color = Color.red;
            topvlineImage.rectTransform.sizeDelta = new Vector2(lineWidth, vdistance * 0.5f);

            //Bottom Half
            GameObject bottomvline = Instantiate(vlinePrefab, parent);
            RectTransform bottomvlineRectTransform = bottomvline.GetComponent<RectTransform>();
            bottomvlineRectTransform.anchoredPosition = new Vector2(bottomTerm.top.x, bottomTerm.top.y + vdistance * 0.25f);
            Image bottomvlineImage = bottomvline.GetComponentInChildren<Image>();
            //bottomvlineImage.color = Color.red;
            bottomvlineImage.rectTransform.sizeDelta = new Vector2(lineWidth, vdistance * 0.5f);

            //Horizontal line
            if (topTerm.bottom.x - bottomTerm.top.x < 0)
            {
                float hdistance = Math.Abs(topTerm.bottom.x - bottomTerm.top.x);
                GameObject hline = Instantiate(vlinePrefab, parent);
                RectTransform hlineRectTransform = hline.GetComponent<RectTransform>();
                hlineRectTransform.anchoredPosition = new Vector2(topTerm.bottom.x + hdistance / 2, bottomTerm.top.y + vdistance * 0.5f);
                Image hlineImage = hline.GetComponentInChildren<Image>();
                // hlineImage.color = Color.red;
                hlineImage.rectTransform.sizeDelta = new Vector2(hdistance + lineWidth, lineWidth);
            }
            else
            {
                float hdistance = Math.Abs(topTerm.bottom.x - bottomTerm.top.x);
                GameObject hline = Instantiate(vlinePrefab, parent);
                RectTransform hlineRectTransform = hline.GetComponent<RectTransform>();
                hlineRectTransform.anchoredPosition = new Vector2(topTerm.bottom.x - hdistance / 2, bottomTerm.top.y + vdistance * 0.5f);
                Image hlineImage = hline.GetComponentInChildren<Image>();
                //hlineImage.color = Color.red;
                hlineImage.rectTransform.sizeDelta = new Vector2(hdistance + lineWidth, lineWidth);
            }
        }
        
    }

    public void ShowAllAttacks(SidedGraph _graph)
    {
        foreach(SidedNode sidedNode in _graph.nodes)
        {
            if (sidedNode.term.Convert()[0] == '~')
            {
                Debug.Log(sidedNode.term.Convert());
                SidedNode attacker = _graph.nodes.Find(s => "~" + s.term.Convert() == sidedNode.term.Convert());
                ShowAttack(premiseRenderers[attacker], premiseRenderers[sidedNode]);
            }
        }

    }

    public void ShowAttack(PremiseRenderer topTerm, PremiseRenderer bottomTerm)
    {
        
        float vdistance = topTerm.bottom.y - bottomTerm.top.y;
        //Debug.Log(vdistance);

        //Top Half
        GameObject topvline = Instantiate(vlinePrefab, parent);
        RectTransform topvlineRectTransform = topvline.GetComponent<RectTransform>();
        if (topTerm.bottom.x - bottomTerm.top.x < 0)
            topvlineRectTransform.anchoredPosition = new Vector2(topTerm.bottom.x + 30, topTerm.bottom.y - vdistance * 0.25f +10);
        else
            topvlineRectTransform.anchoredPosition = new Vector2(topTerm.bottom.x - 30, topTerm.bottom.y - vdistance * 0.25f + 10);

        Image topvlineImage = topvline.GetComponentInChildren<Image>();
        topvlineImage.color = Color.red;
        topvlineImage.rectTransform.sizeDelta = new Vector2(lineWidth, vdistance * 0.5f -20);

        //Bottom Half
        GameObject bottomvline = Instantiate(vlinePrefab, parent);
        RectTransform bottomvlineRectTransform = bottomvline.GetComponent<RectTransform>();
        if (topTerm.bottom.x - bottomTerm.top.x < 0)
            bottomvlineRectTransform.anchoredPosition = new Vector2(bottomTerm.top.x - 30, bottomTerm.top.y+ vdistance * 0.25f + 10);
        else
            bottomvlineRectTransform.anchoredPosition = new Vector2(bottomTerm.top.x + 30, bottomTerm.top.y + vdistance * 0.25f + 10);
        Image bottomvlineImage = bottomvline.GetComponentInChildren<Image>();
        bottomvlineImage.color = Color.red;
        bottomvlineImage.rectTransform.sizeDelta = new Vector2(lineWidth, vdistance * 0.5f +20);

        //Horizontal line
        if(topTerm.bottom.x - bottomTerm.top.x < 0)
        {
            float hdistance = Math.Abs(topTerm.bottom.x - bottomTerm.top.x);
            GameObject hline = Instantiate(vlinePrefab, parent);
            RectTransform hlineRectTransform = hline.GetComponent<RectTransform>();
            hlineRectTransform.anchoredPosition = new Vector2(topTerm.bottom.x + hdistance / 2, bottomTerm.top.y + vdistance * 0.5f +20);
            Image hlineImage = hline.GetComponentInChildren<Image>();
            hlineImage.color = Color.red;
            hlineImage.rectTransform.sizeDelta = new Vector2(hdistance + lineWidth - 60, lineWidth);
        }
        else
        {
            float hdistance = Math.Abs(topTerm.bottom.x - bottomTerm.top.x);
            GameObject hline = Instantiate(vlinePrefab, parent);
            RectTransform hlineRectTransform = hline.GetComponent<RectTransform>();
            hlineRectTransform.anchoredPosition = new Vector2(topTerm.bottom.x - hdistance / 2, bottomTerm.top.y + vdistance * 0.5f + 20);
            Image hlineImage = hline.GetComponentInChildren<Image>();
            hlineImage.color = Color.red;
            hlineImage.rectTransform.sizeDelta = new Vector2(hdistance + lineWidth - 60, lineWidth);
        }
    }



    public void Connect(PremiseRenderer consequent, List<PremiseRenderer> antecedents)
    {
        //Debug.Log(consequent.bottom.y);
        //Debug.Log(antecedents[0].top.y);

        float vdistance = consequent.bottom.y - antecedents[0].top.y;
       

        //Create top line
        GameObject topvline = Instantiate(vlinePrefab, parent);
        RectTransform vlineRectTransform = topvline.GetComponent<RectTransform>();
        vlineRectTransform.anchoredPosition = new Vector2(consequent.bottom.x, consequent.bottom.y - vdistance * 0.25f);

        Image topvlineImage = topvline.GetComponentInChildren<Image>();
        topvlineImage.rectTransform.sizeDelta = new Vector2(lineWidth, vdistance * 0.5f);

        //create bottom vlines
        foreach (PremiseRenderer antecedent in antecedents)
        {
            GameObject bottomvline = Instantiate(vlinePrefab, parent);

            RectTransform bottomvlineRectTransform = bottomvline.GetComponent<RectTransform>();
            bottomvlineRectTransform.anchoredPosition = new Vector2(antecedent.top.x, antecedent.top.y + vdistance * 0.25f);

            Image bottomvlineImage = bottomvline.GetComponentInChildren<Image>();
            bottomvlineImage.rectTransform.sizeDelta = new Vector2(lineWidth, vdistance * 0.5f);
        }

        //create hline
        Vector2 left = new Vector2(antecedents[0].top.x, antecedents[0].top.y + vdistance * 0.25f);
        Vector2 right = new Vector2(antecedents[antecedents.Count-1].top.x, antecedents[antecedents.Count - 1].top.y + vdistance * 0.25f);
        Vector2 between = right - left;
        GameObject hline = Instantiate(vlinePrefab, parent);

        RectTransform hlineRectTransform = hline.GetComponent<RectTransform>();
        hlineRectTransform.anchoredPosition = new Vector2((left.x + right.x) * 0.5f, right.y + vdistance * 0.25f);

        Image hlineImage = hline.GetComponentInChildren<Image>();
        hlineImage.rectTransform.sizeDelta = new Vector2(between.x + lineWidth, lineWidth);

    }

}



