using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class Environment
{
    public List<Dispute> disputes;
    public Dispute dispute;
    private EvaluationManager evaluationManager;
    private ModuleSettings moduleSettings;
    List<Agent> agents;
    List<TeamworkModule.Team> teams;

    //Predefined Settings:
    //Concealment Settings
    //List<ConcealmentModule.ArgumentType> argumentTypes;

    //Concealment Settings
    //List<EquityModule.User> users;

    public Environment()
    {
        disputes = new List<Dispute> ();
        evaluationManager = new EvaluationManager ();
        moduleSettings = new ModuleSettings ();
    }

    public Environment(Module _con, Module _equ, Module _tea, Module _und)
    {
        disputes = new List<Dispute> ();
        evaluationManager = new EvaluationManager ();
        moduleSettings = new ModuleSettings (_con, _equ, _tea, _und);
    }


    public void InitializeAgents(List<Agent> _agents, TeamworkModule.EffortType _effortType = TeamworkModule.EffortType.No, 
        TeamworkModule.Overlap _overlap = TeamworkModule.Overlap.Full)
    {
        if (ModuleSettings.Instance.teamworkModule.IsActive())
        {
            teams = TeamworkModule.CreateTeams(_agents, _effortType, _overlap);
            agents = _agents;
        }
            
        else
            agents = _agents;
    }

    public void InitializeAgents(List<EquityModule.User> _users, TeamworkModule.EffortType _effortType = TeamworkModule.EffortType.No, TeamworkModule.Overlap _overlap = TeamworkModule.Overlap.Full)
    {
        if (ModuleSettings.Instance.equityModule.IsActive())
        {
            if (ModuleSettings.Instance.teamworkModule.IsActive())
            {
                teams = EquityModule.MatchTeams(_users, _effortType, _overlap);
                agents = EquityModule.MatchAgents(_users);
            }
            else
                agents = EquityModule.MatchAgents(_users);
        }
        else
        {
            agents = new List<Agent> ();
            for (int i = 0; i < _users.Count; i++)
                agents.Add(new Agent ());
        }
    }

    public void RunDisputes(List<Dispute> _disputes, bool _print = false)
    {
        disputes = new List<Dispute> ();
        foreach (Dispute _dispute in _disputes)
        {
            dispute = _dispute;
            StartDispute(_print); 
            disputes.Add(dispute);
        }
    }

    public void LoadScenario(string name)
    {
        dispute = ScenarioManager.LoadScenario(name);
        disputes = new List<Dispute> () { dispute };
    }

    public void StartDispute(bool _print = false)  
    {
       // DisputeRenderer.Instance.StartCoroutine(StartAnimatedDispute()); 
    //}

   // IEnumerator StartAnimatedDispute() 
    //{

        //Initialize
        dispute.Reset();
        if (ModuleSettings.Instance.teamworkModule.IsActive())
        {
            dispute.teams = teams;
            TeamworkModule.UpdateTeamSettings(dispute.teams, dispute);
        }
        else
            dispute.UpdateAgentSettings(agents);
        dispute.MergeContraries();

        //#######################
        //# Initialization Step #
        //#######################

        
        //if (_print) Console.WriteLine("Initializing");

        //CONCEALMENT MODULE: Divide kb of all agents based on their Dividing Type
        if (ModuleSettings.Instance.concealmentModule.IsActive())
        {
            ConcealmentModule.Divide(dispute);
            //Console.WriteLine("count: "+dispute.agents[0].knowledgeBase.levels.Count);
        }
        //yield return new WaitForSeconds(DisputeRenderer.Instance.animationspeed);

        //TEAMWORK MODULE: Proponent Team gets to create the case together
        if (ModuleSettings.Instance.teamworkModule.IsActive())
            dispute.teams[0].CreateCase(dispute);
        else
            dispute.agents[0].CreateCase(dispute);

        //yield return new WaitForSeconds(DisputeRenderer.Instance.animationspeed);
        //dispute.Print();

        //###################
        //# Extension Steps #
        //###################
        while (dispute.ongoing)
        {
            for (int i = 1; i >= 0; i--)
            {
                
                //Console.WriteLine("teams: " + teams.Count);
                //if (_print) Console.WriteLine("Extending: The turn is for " +dispute.agents[i].side.ToString());
                if (ModuleSettings.Instance.teamworkModule.IsActive())
                    dispute.teams[i].ExtendCase(dispute);
                else
                    dispute.agents[i].ExtendCase(dispute);
                if (!dispute.ongoing)
                    break;

                //DisputeRenderer.Instance.ShowGraph(new Structure(dispute).mergedGraph);
                //yield return new WaitForSeconds(DisputeRenderer.Instance.animationspeed);



            }
        }
    }

    public void EvaluateDispute()
    {
        //Evaluate end result
        bool eval = EvaluationManager.Instance.Wins(dispute, dispute.subject.Convert(), true).GetAwaiter().GetResult();
        Console.WriteLine(eval ? "The Evaluation for {0} results in a win for Agent {1}!" : "The Evaluation for {0} results in a loss for Agent {1}!",
            dispute.subject.Convert(), dispute.agents[0].side);

        dispute.Print();
    }


}



