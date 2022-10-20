using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

//##########################################################
//################### Module Settings ######################
//##########################################################

public class ModuleSettings
{
	public static ModuleSettings Instance;
	public Module concealmentModule;
	public Module equityModule;
	public Module teamworkModule;
	public Module understandabilityModule;

	public ModuleSettings()
	{
		if (Instance == null)
			Instance = this;

		concealmentModule = Module.On;
		equityModule = Module.Off;
		teamworkModule = Module.Off;
		understandabilityModule = Module.Off;
	}

	public ModuleSettings(Module _con, Module _equ, Module _tea, Module _und)
	{
		if (Instance == null)
			Instance = this;

		concealmentModule = _con;
		equityModule = _equ;
		teamworkModule = _tea;
		understandabilityModule = _und;
	}

    public void SetDefaultSettings(Agent _agent)
    {
        if (Instance.concealmentModule.IsActive())
            ConcealmentModule.SetDefaultConcealmentTypes(_agent);
    }


}
public enum Module { On, Off };
public static class Extensions
{
	public static bool IsActive(this Module _module) { return _module == Module.On; }
}

//##########################################################
//################### CONCEALMENT MODULE ###################
//##########################################################

public static class ConcealmentModule
{
    public static void SetDefaultConcealmentTypes(this Agent _agent)
    {
        _agent.argumentType = ArgumentType.All;
        _agent.dividingType = DividingType.None; //Console.WriteLine("allarg");
        _agent.droppingType = DroppingType.Never;
    }


    //----------------------------------------------------------------------------------
    //--------------------------- Argument Choosing Behavior ---------------------------
    //----------------------------------------------------------------------------------

    //Concealment Types
    public enum ArgumentType
    {
        All,
        Random,
        Shortest,
        Longest,
    }

    public static void SetArgumentTypes(Dispute _dispute, List<ArgumentType> _argumentTypes)
    {
        List<Agent> agents = _dispute.GetAgents();
        for (int i = 0; i< agents.Count;i++)
        {
            SetArgumentType(agents[i], _argumentTypes[i]);
        }
    }

    public static void SetArgumentType(Agent _agent, ArgumentType _argumentType)
    {
        _agent.argumentType = _argumentType;
    }

    public static void AddArguments(Dispute _dispute, List<Argument> _arguments, ArgumentType _argumentType)
    {
        switch (_argumentType)
        {
            case ArgumentType.All: AddAllArguments(_dispute, _arguments); break;
            case ArgumentType.Longest: AddLongestArgument(_dispute, _arguments); break;
            case ArgumentType.Random: AddRandomArgument(_dispute, _arguments); break;
            case ArgumentType.Shortest: AddShortestArgument(_dispute, _arguments); break;
            default: break;
        }
    }
    public static Dispute AddAllArguments(Dispute _dispute, List<Argument> _arguments)
    {
        foreach (Argument argument in _arguments)
        {
            _dispute.AddArgument(argument);
        }
        return _dispute;
    }
    public static Dispute AddLongestArgument(Dispute _dispute, List<Argument> _arguments)
    {
        int longestArgumentSize = int.MinValue;
        Argument longestArgument = null;
        foreach (Argument argument in _arguments)
        {
            int argumentSize = argument.GetSize();
            if (argumentSize > longestArgumentSize)
            {
                longestArgumentSize = argumentSize;
                longestArgument = argument;
            }
        }
        _dispute.AddArgument(longestArgument);
        return _dispute;
    }
    public static Dispute AddRandomArgument(Dispute _dispute, List<Argument> _arguments)
    {
        _dispute.AddArgument(_arguments[new Random().Next(0, _arguments.Count)]);
        return _dispute;
    }
    public static Dispute AddShortestArgument(Dispute _dispute, List<Argument> _arguments)
    {
        int shortestArgumentSize = int.MaxValue;
        Argument shortestArgument = null;
        foreach (Argument argument in _arguments)
        {
            int argumentSize = argument.GetSize();
            if (argumentSize < shortestArgumentSize)
            {
                shortestArgumentSize = argumentSize;
                shortestArgument = argument;
            }
        }
        _dispute.AddArgument(shortestArgument);
        return _dispute;
    }

    //----------------------------------------------------------------------------------
    //--------------------------- Level Dividing Behavior ------------------------------
    //----------------------------------------------------------------------------------

    public enum DividingType
    {
        None,
        HalfArg,
        AllArg,
        AllContent
    }

    public static void Divide(Dispute _dispute)
    {
        foreach (Agent agent in _dispute.GetAgents())
        {
            switch (agent.dividingType)
            {
                case DividingType.None:
                    DivideNone(agent.knowledgeBase); break;
                case DividingType.HalfArg:
                    DivideHalfArguments(agent.knowledgeBase); break;
                case DividingType.AllArg:
                    DivideAllArguments(agent.knowledgeBase); break;
                case DividingType.AllContent:
                    DivideAllContent(agent.knowledgeBase); break;
            }

            InitializeDivision(agent.knowledgeBase);
            //PrintLevels(agent.knowledgeBase.levels);
        }
    }

    public static void DivideNone(KnowledgeBase kb)
    {
        kb.levels = new List<Level> ();
        kb.levels.Add(new Level (kb.premises, kb.rules));
    }

    public static void DivideHalfArguments(KnowledgeBase kb)
    {
        kb.levels = new List<Level> ();
        Random rnd = new Random ();
        List<Argument> arguments = kb.GetArguments();
        List<Argument> halfArguments = arguments.OrderBy(x => rnd.Next()).Take((int)(arguments.Count * 0.5f)).ToList();
        List<Rule> rulesFirst = new List<Rule> ();
        List<Term> premisesFirst = new List<Term> ();
        List<Rule> rulesSecond = new List<Rule> ();
        List<Term> premisesSecond = new List<Term> ();
        foreach (Argument argument in arguments)
        {// no top half want dat kan je niet weten
            if (halfArguments.Contains(argument))
            {
                premisesFirst = premisesFirst.Concat(argument.premises).ToList();
                rulesFirst = rulesFirst.Concat(argument.rules).ToList();
            }
            else
            {
                premisesSecond = premisesSecond.Concat(argument.premises).ToList();
                rulesSecond = rulesSecond.Concat(argument.rules).ToList();
            }
        }
        
        kb.levels.Add(new Level (premisesFirst,rulesFirst));
        kb.levels.Add(new Level (premisesSecond,rulesSecond));
    }

    public static void DivideAllArguments(KnowledgeBase kb)
    {
        kb.levels = new List<Level> ();
        Random rnd = new Random ();
        List<Argument> arguments = kb.GetArguments();
        List<Argument> randomizedArguments = arguments.OrderBy(x => rnd.Next()).ToList();
        for(int i =0; i< randomizedArguments.Count; i++)
        {
            kb.levels.Add(new Level (randomizedArguments[i].premises, randomizedArguments[i].rules));
        }
    }

    public static void DivideAllContent(KnowledgeBase kb)
    {
        List<Level> levels = new List<Level> ();
        foreach(Term premise in kb.premises)
        {
            levels.Add(new Level (new List<Term> () { premise }, new List<Rule> ()));
        }
        foreach (Rule rule in kb.rules)
        {
            levels.Add(new Level (new List<Term> (), new List<Rule> () { rule }));
        }
        Random rnd = new Random ();
        List<Level> randomizedLevels = levels.OrderBy(x => rnd.Next()).ToList();
        kb.levels = randomizedLevels;
    }

    public static void InitializeDivision(KnowledgeBase kb)
    {
        //Console.WriteLine("Levels count "+ kb.levels.Count);
        kb.premises = kb.levels[0].premises;
        kb.levels[0].premises = new List<Term> ();
        kb.rules = kb.levels[0].rules;
        kb.levels[0].rules = new List<Rule> ();
    }

    public static void PrintLevels(List<Level> levels)
    {
        Console.WriteLine("--------------------------------------------");
        Console.WriteLine("This knowledge base contains the {0} following levels:",levels.Count);


        for (int i = 0;i< levels.Count; i++)
        {
            Console.WriteLine("-------");
            Console.WriteLine("Level " + i);
            Console.WriteLine("\nPremises:");
            foreach (Term t in levels[i].premises)
                Console.WriteLine(t.Convert() + ";");
            Console.WriteLine("\nRules:");
            foreach (Rule r in levels[i].rules)
                Console.WriteLine(r.Convert() + ";");
            Console.WriteLine("-------");
        }
    }


    /*
    public static (List<Term>, List<Rule>) GetContent(this List<Argument> _arguments)
    {
        List<Term> premises = new();
        List<Rule> rules = new();
        foreach (Argument argument in _arguments)
        {

            premises = premises.Concat(argument.premises).ToList();
            rules = rules.Concat(argument.rules).ToList();
           
        }
        return (premises, rules);
    }*/

    //----------------------------------------------------------------------------------
    //--------------------------- Level Dropping Behavior ------------------------------
    //----------------------------------------------------------------------------------

    public enum DroppingType
    {
        Always,
        Threshold75,
        Threshold50,
        Threshold25,
        Never
    }

    public static bool ConsiderDropping(Agent _agent)
    {
        if (_agent.HasLevelsLeft())
            if (_agent.WillDrop())
            {
                return true;
            }
        return false;
    }

    private static bool HasLevelsLeft(this Agent _agent)
    {
        return _agent.knowledgeBase.currentLevel < _agent.knowledgeBase.levels.Count - 1;
    }

    private static bool WillDrop(this Agent _agent)
    {

        switch (_agent.droppingType)
        {
            case DroppingType.Always: return DropAlways();
            case DroppingType.Threshold25: return DropThreshold(0.25f, _agent.knowledgeBase.levels.Count);
            case DroppingType.Threshold50: return DropThreshold(0.50f, _agent.knowledgeBase.levels.Count);
            case DroppingType.Threshold75: return DropThreshold(0.75f, _agent.knowledgeBase.levels.Count);
            case DroppingType.Never:  return DropNever();
            default: return true;
        }
    }

    public static bool DropAlways()
    {
        return true;
    }

    public static bool DropThreshold(float _treshold, int _levels)
    {
        double rnd = new Random().NextDouble();
        //Console.WriteLine("{0} is bigger than {1} ?",rnd, (1 - _treshold) / _levels);

        return rnd > (1 - _treshold) / _levels; // * 2;
        //return new Random().Next(0, 100) < _treshold * 100;


        return new Random().Next(0, 100) > (1- _treshold) / _levels * 100; 
    }

    public static bool DropNever()
    {
        return false;
    }

    public static void DropLevel(this Agent _agent)
    {
        _agent.knowledgeBase.currentLevel++;
        AddCurrentLevel(_agent.knowledgeBase);
    }

    public static void AddCurrentLevel(KnowledgeBase kb)
    {
        kb.premises = kb.premises.Concat(kb.levels[kb.currentLevel].premises).ToList();
        kb.levels[kb.currentLevel].premises = new List<Term> ();
        kb.rules = kb.rules.Concat(kb.levels[kb.currentLevel].rules).ToList();
        kb.levels[kb.currentLevel].rules = new List<Rule> ();
    }
}

//##########################################################
//################### EQUITY MODULE ########################
//##########################################################

public static class EquityModule
{

    public static List<Agent> MatchAgents(List<User> _users)
    {
        List<Agent> agents = new List<Agent> ();
        foreach(User user in _users)
        {
            Agent agent = MatchAgent(user);
            agents.Add(agent);
        }
        return agents;
    }

    public static List<TeamworkModule.Team> MatchTeams(List<User> _users,TeamworkModule.EffortType _effortType, TeamworkModule.Overlap _overlap)
    {
        return TeamworkModule.CreateTeams(MatchAgents(_users), _effortType, _overlap);
    }

    public static Agent MatchAgent(User user)
    {

        ConcealmentModule.ArgumentType argumentType;
        if (user.privacyType == PrivacyType.Default)
            argumentType = ConcealmentModule.ArgumentType.All;
        else
            argumentType = ConcealmentModule.ArgumentType.Shortest;

        ConcealmentModule.DividingType dividingType;
        switch (user.knowledge)
        {
            case (Knowledge.High):
                dividingType = ConcealmentModule.DividingType.AllContent;break;
            case (Knowledge.Medium):
                dividingType = ConcealmentModule.DividingType.AllArg; break;
            case (Knowledge.Low):
                dividingType = ConcealmentModule.DividingType.HalfArg; break;
            default: 
                dividingType = ConcealmentModule.DividingType.None;break;
        }

        ConcealmentModule.DroppingType droppingType;
        switch (user.motivation)
        {
            case (Motivation.High):
                droppingType = ConcealmentModule.DroppingType.Threshold25; break;
            case (Motivation.Medium):
                droppingType = ConcealmentModule.DroppingType.Threshold50; break;
            case (Motivation.Low):
                droppingType = ConcealmentModule.DroppingType.Threshold75; break;
            default:
                droppingType = ConcealmentModule.DroppingType.Always; break;
        }

        Agent agent = new Agent (user.side, argumentType, dividingType, droppingType);
        return agent;
    }




    //Equity Types
    public enum PrivacyType
    {
        Default,
        MarginallyConcerned,
        Amateur,
        Technician,
        LazyExpert,
        Fundamentalist
    }

    public enum Knowledge
    {
        Low, Medium, High, Default
    }

    public enum Motivation
    {
        Low, Medium, High, Default
    }

    public class User 
    {
        public Side side;
        public Knowledge knowledge;
        public Motivation motivation;
        public PrivacyType privacyType;

        public User(Side _side, Knowledge _knowledge, Motivation _motivation)
        {
            side = _side;
            knowledge = _knowledge;
            motivation = _motivation;
            DeterminePrivacyType();
        }

        public User(Side _side, PrivacyType _privacyType)
        {
            side = _side;
            privacyType = _privacyType;
            DetermineKM();
        }

        /*
        public User(Side _side, Knowledge _knowledge, Motivation _motivation, PrivacyType _privacyType)
        {
            knowledge = _knowledge;
            motivation = _motivation;
            privacyType = _privacyType;
        }*/

        public void DeterminePrivacyType()
        {
            switch((knowledge, motivation))
            {
                case (Knowledge.High, Motivation.High):
                    privacyType = PrivacyType.Fundamentalist;break;
                case (Knowledge.High, Motivation.Low):
                    privacyType = PrivacyType.LazyExpert; break;
                case (Knowledge.Medium, Motivation.High):
                    privacyType = PrivacyType.Technician; break;
                case (Knowledge.Medium, Motivation.Medium):
                    privacyType = PrivacyType.Amateur; break;
                case (Knowledge.Low, Motivation.Low):
                    privacyType = PrivacyType.MarginallyConcerned; break;
                default:
                    privacyType = PrivacyType.Default; break;
            }

        }

        public void DetermineKM()
        {
            switch (privacyType)
            {
                case (PrivacyType.Fundamentalist):
                    knowledge = Knowledge.High; motivation = Motivation.High; break;
                case ( PrivacyType.LazyExpert):
                    knowledge = Knowledge.High; motivation = Motivation.Low; break;
                case ( PrivacyType.Technician):
                    knowledge = Knowledge.Medium; motivation = Motivation.High; break;
                case (PrivacyType.Amateur):
                    knowledge = Knowledge.Medium; motivation = Motivation.Medium; break;
                case ( PrivacyType.MarginallyConcerned):
                    knowledge = Knowledge.Low; motivation = Motivation.Low; break;
                default:
                    knowledge = Knowledge.Default; motivation = Motivation.Default; break;
            }
        }
    }
}


//##########################################################
//################### TEAMWORK MODULE ######################
//##########################################################

public static class TeamworkModule
{
    //----------------------------------------------------------------------------------
    //--------------------------- Concession Behavior ----------------------------------
    //----------------------------------------------------------------------------------
    //stuff like tit for tat etc.

    //Teamwork Types
    public enum EffortType
    {
        Shared,
        Minimal,
        Maximum,
        No
    }

    public enum Overlap
    {
        None = 0, Quarter = 25, Half = 50, ThreeQuarters = 75, Full = 100
    }

    public enum TeamStatus
    {
        Ongoing, Hold, Forfeit, TimeOut
    }


    public static List<Team> CreateTeams(List<Agent> _agents, EffortType _effortType, Overlap _overlap)
    {
        Team proponents = new Team (Side.Proponent, _effortType, _overlap);
        Team opponents = new Team (Side.Opponent, _effortType, _overlap);
        List<Team> teams = new List<Team> () { proponents, opponents };
        foreach (Agent agent in _agents)
        {
            teams[(int)agent.side].AddAgent(agent);
            agent.team = teams[(int)agent.side];
        }

        foreach (Team team in teams)
        {
            team.ResetCurrentAgent();
        }
            

        return teams;
    }

    public class Team
    {
        Side side;
        EffortType effortType;
        public Overlap overlap;
        public List<Agent> agents;
        public Queue<Agent> agentqueue;
        List<Agent> concessionHistory;

        private int currentDropper;

        private int currentAgent;

        public Team(Side _side, EffortType _effortType = EffortType.Minimal, Overlap _overlap = Overlap.Half)
        {
            side = _side;
            effortType = _effortType;
            overlap = _overlap;
            agents = new List<Agent> ();
            concessionHistory = new List<Agent> ();
            currentAgent = 0;
            agentqueue = new Queue<Agent> ();
        }

        public void CreateCase(Dispute _dispute)
        {
            

            int premisecount = _dispute.sharedKnowledgeBase.premises.Count;
            //Console.WriteLine("Before: " + arguments);

            switch (effortType)
            {
                case EffortType.No:
                case EffortType.Minimal:
                    break;
                case EffortType.Shared:
                case EffortType.Maximum:
                    agentqueue = new Queue<Agent>(agentqueue.Shuffle());
                    //agentqueue = new Queue<Agent>(agents);
                    break;
            }


            foreach (Agent agent in agents)
            {
                agent.CreateCase(_dispute);
                //Console.WriteLine("After: " + _dispute.sharedKnowledgeBase.GetArguments().Count);
                if (premisecount != _dispute.sharedKnowledgeBase.premises.Count)
                    return;
            }
            //Console.WriteLine("Forfeit");
            _dispute.winner = (Side)Math.Abs(1 - (int)side);
            _dispute.ongoing = false;
            return;


            switch (effortType)
            {
                case EffortType.No:
                    agents[0].CreateCase(_dispute);break;
                case EffortType.Minimal:
                    agents[0].CreateCase(_dispute); break;
                case EffortType.Shared:
                    agents[new Random().Next(0, agents.Count)].CreateCase(_dispute); break;
                case EffortType.Maximum:
                    agents[new Random().Next(0, agents.Count)].CreateCase(_dispute); break;
            }
                
            
        }

        public void ExtendCase(Dispute _dispute)
        {
            //Console.WriteLine("Extending the dispute for " + side+ " with" + effortType);

            //Console.WriteLine(agents[0].knowledgeBase.premises.Count);

            switch (effortType)
            {
                case EffortType.No:
                    {
                        foreach(Agent agent in agents)
                        {
                            if (agent.teamStatus == TeamStatus.Hold)
                            {
                                agent.teamStatus = TeamStatus.Ongoing;
                                ConcealmentModule.DropLevel(agent);
                                int premisecount = _dispute.sharedKnowledgeBase.premises.Count;
                                agent.ExtendCase(_dispute);
                                if (premisecount != _dispute.sharedKnowledgeBase.premises.Count)
                                    return;
                            }
                            else if (agent.teamStatus == TeamStatus.Ongoing)
                            {
                                int premisecount = _dispute.sharedKnowledgeBase.premises.Count;
                                agent.ExtendCase(_dispute);
                                if (premisecount != _dispute.sharedKnowledgeBase.premises.Count)
                                    return;
                            }
                        }
                        
                        _dispute.winner = (Side)Math.Abs(1 - (int)side);
                        //Console.WriteLine(_dispute.winner);
                        _dispute.ongoing = false;

                    }
                    break;
                case EffortType.Minimal:
                    {
                        //Check if all team members have been considered this turn
                        if (currentAgent == agents.Count)
                        {
                            _dispute.winner = (Side)Math.Abs(1 - (int)side);
                            _dispute.ongoing = false;
                            return;
                        }

                        //check whether current agent can extend the case
                        if (agents[currentAgent].teamStatus == TeamStatus.Ongoing)
                        {
                            agents[currentAgent].ExtendCase(_dispute);
                            currentAgent = 0;
                        }

                        //if the current agent is holding, it drops 
                        else if (agents[currentAgent].teamStatus == TeamStatus.Hold)
                        {
                            foreach (Agent agent in agents)
                            {
                                if (agent.teamStatus == TeamStatus.Ongoing)
                                {
                                    agent.ExtendCase(_dispute);
                                    break;
                                }
                            }
                            agents[currentAgent].teamStatus = TeamStatus.Ongoing;
                            ConcealmentModule.DropLevel(agents[currentAgent]);
                            agents[currentAgent].ExtendCase(_dispute);
                        }
                        else if (agents[currentAgent].teamStatus == TeamStatus.Forfeit)
                        {
                            currentAgent++;
                            ExtendCase(_dispute);
                        }
                    }break;
                case EffortType.Shared:
                    {
                        foreach(Agent agent in agentqueue)
                        {
                            if (agent.teamStatus != TeamStatus.Hold)
                            {
                                agent.teamStatus = TeamStatus.Ongoing;
                            }
                        }
                        //Console.WriteLine("until hier" + side);
                        //If all agents forfeit, the team forfeits
                        if (CheckForfeitStatus(this))
                        {
                            _dispute.winner = (Side)Math.Abs(1 - (int)side);
                            _dispute.ongoing = false;
                            return;
                        }

                        //If some agents are holding, all agents drop
                        if (CheckHoldorForfeitStatus(this))
                        {
                            foreach(Agent agent in agentqueue)
                            {
                                if (agent.teamStatus == TeamStatus.Hold)
                                {
                                    agent.teamStatus = TeamStatus.Ongoing;
                                    ConcealmentModule.DropLevel(agent);
                                }
                            }
                        }
                        //Console.WriteLine("until hie2r" + side); 
                       // Console.WriteLine(agentqueue.LastOrDefault().teamStatus);

                        //otherwise, go through each agent and see who can add something to the dispute
                        foreach (Agent agent in agentqueue)
                        {
                            if (agent.teamStatus == TeamStatus.Ongoing)
                            {
                                int premisecount = _dispute.sharedKnowledgeBase.premises.Count;
                                agent.ExtendCase(_dispute);
                                if (premisecount != _dispute.sharedKnowledgeBase.premises.Count)
                                {
                                    //next turn, the order between agents shifts
                                    agentqueue.Enqueue(agentqueue.Dequeue());
                                    return;
                                }
                            }
                        }

                        //If no agent can add something to the dispute, forfeit:
                        _dispute.winner = (Side)Math.Abs(1 - (int)side);
                        _dispute.ongoing = false;
                        return;


                        /*
                        if (agents[currentAgent].teamStatus != TeamStatus.Ongoing)
                        {
                            //Console.WriteLine("Not ongoing Currentagent " + side + " " + currentAgent+" " +agents[currentAgent].teamStatus);
                            currentAgent++;
                            ExtendCase(_dispute);
                        }
                        else
                        {
                            //Console.WriteLine("Currentagent "+side+" " +currentAgent);
                            agents[currentAgent].ExtendCase(_dispute);
                            currentAgent++;
                            
                            return;
                        }*/
                    }
                    break;
                case EffortType.Maximum:
                    {
                        foreach (Agent agent in agentqueue)
                        {
                            if (agent.teamStatus != TeamStatus.Hold)
                            {
                                agent.teamStatus = TeamStatus.Ongoing;
                            }
                        }
                        //Console.WriteLine("until hier" + side);
                        //If all agents forfeit, the team forfeits
                        if (CheckForfeitStatus(this))
                        {
                            _dispute.winner = (Side)Math.Abs(1 - (int)side);
                            _dispute.ongoing = false;
                            return;
                        }

                        //If some agents are holding, all agents drop
                        if (CheckHoldorForfeitStatus(this))
                        {
                            List<Agent> holders = agentqueue.ToList().FindAll(a => a.teamStatus == TeamStatus.Hold);
                            Agent a = holders[new Random().Next(0, holders.Count)];
                            a.teamStatus = TeamStatus.Ongoing;
                            ConcealmentModule.DropLevel(a);

                        }
                        //Console.WriteLine("until hie2r" + side); 
                        //Console.WriteLine(agentqueue.LastOrDefault().teamStatus);

                        //otherwise, go through each agent and see who can add something to the dispute
                        foreach (Agent agent in agentqueue)
                        {
                            if (agent.teamStatus == TeamStatus.Ongoing)
                            {
                                int premisecount = _dispute.sharedKnowledgeBase.premises.Count;
                                agent.ExtendCase(_dispute);
                                if (premisecount != _dispute.sharedKnowledgeBase.premises.Count)
                                {
                                    //next turn, the order between agents shifts
                                    agentqueue.Enqueue(agentqueue.Dequeue());
                                    return;
                                }
                            }
                        }

                        //If no agent can add something to the dispute, forfeit:
                        _dispute.winner = (Side)Math.Abs(1 - (int)side);
                        _dispute.ongoing = false;
                        return;
                    }
            }
            
        }

        public void AddAgent(Agent _agent)
        {
            agents.Add(_agent);
            agentqueue.Enqueue(_agent);
        }

        public void ResetCurrentAgent()
        {
            switch(effortType)
            {
                case EffortType.No:
                    currentAgent = 0;
                    break;
                case EffortType.Minimal:
                    //agents = agents.Shuffle().ToList();
                    currentAgent = 0;
                    break;
                case EffortType.Shared:
                    //agentqueue = new Queue<Agent> (agentqueue.Shuffle());
                    currentAgent = new Random().Next(0, agents.Count);
                    break;
                case EffortType.Maximum:
                    //agents = agents.Shuffle().ToList();
                    //currentAgent = 0;
                    //agentqueue = new Queue<Agent> (agentqueue.Shuffle());
                    currentAgent = new Random().Next(0, agents.Count);
                    break;
            }

            //Console.WriteLine(currentAgent);


        }
    }

    public static void UpdateTeamSettings(List<Team> _teams, Dispute _dispute)
    {
        foreach (Team team in _teams)
        {
            foreach (Agent agent in team.agents)
                agent.teamStatus = TeamStatus.Ongoing;

            team.ResetCurrentAgent();

            ShareKnowledgeBase(team, _dispute.agents.Find(a => a.side == team.agents[0].side).knowledgeBase);
        }
    }

    public static void ShareKnowledgeBase(Team team, KnowledgeBase _knowledgeBase)
    {
        int teamMembers = team.agents.Count;
        List<Argument> arguments = _knowledgeBase.GetArguments();
        List<Argument> overlappingarguments = arguments.Take((arguments.Count * (int)team.overlap / 100)).ToList();

        if (team.overlap != Overlap.Full)
        {
            List<Argument> nonoverlappingarguments = arguments.FindAll(a => !overlappingarguments.Contains(a)).ToList();


            //new!
            for (int i = 0; i < teamMembers; i++)
            {
                team.agents[i].knowledgeBase = new KnowledgeBase(overlappingarguments);
            }

            int a = 0;
            foreach (Argument argument in nonoverlappingarguments)
            {
                if (a == team.agents.Count)
                    a = 0;
                team.agents[a].AddArgument(argument);
                a++;
            }

        }
        else
        {
            for (int i = 0; i < teamMembers; i++)
            {
                team.agents[i].knowledgeBase = new KnowledgeBase(overlappingarguments);
                //Console.WriteLine("rules: "+ team.agents[i].knowledgeBase.rules.Count);
                //Console.WriteLine("premises: " + team.agents[i].knowledgeBase.premises.Count);
            }
        }
        

    }

    public static bool CheckOngoingStatus(Team team)
    {
        foreach (Agent a in team.agents)
        {
            if (a.teamStatus != TeamStatus.Ongoing)
                return false;
        }
        return true;
    }

    public static bool CheckHoldStatus(Team team)
    {
        foreach (Agent a in team.agents)
        {
            if (a.teamStatus != TeamStatus.Hold)
                return false;
        }
        return true;
    }

    public static bool CheckForfeitStatus(Team team)
    {
        foreach(Agent a in team.agents)
        {
            if (a.teamStatus != TeamStatus.Forfeit)
                return false;
        }
        return true;
    }

    public static bool CheckHoldorForfeitStatus(Team team)
    {
        foreach (Agent a in team.agents)
        {
            if (a.teamStatus == TeamStatus.Ongoing)
                return false;
        }
        return true;
    }



}

//##########################################################
//################ UNDERSTANDABILITY MODULE ################
//##########################################################

public static class UnderstandabilityModule
{
    public enum FeedbackType
    {
        No, Summarized, Extensive, Advisory
    }


    public class TextualFeedback
    {
        //public FeedbackType feedbackType = FeedbackType.Advisory;
        public Argument opponentsLastArgument;
        public Argument proponentsLastArgument;

        public TextualFeedback()
        {
            opponentsLastArgument = new Argument ();
            proponentsLastArgument = new Argument ();
        }
    }

        public static string GiveFeedback(List<Dispute> _disputes, Side _side, FeedbackType _feedbackType)
        {
            switch (_feedbackType)
            {
                case FeedbackType.No: return "";
                case FeedbackType.Summarized: return GiveSummary(_disputes, _side);
                case FeedbackType.Extensive: return GiveExtensive(_disputes, _side);
                case FeedbackType.Advisory: return GiveAdvice(_disputes, _side);
                default: return "";
            }
        }

        public static string GiveSummary(List<Dispute> _disputes, Side _side)
        {
            float win = Metrics.GetAverageWin(_disputes, _side) * 100;
            float con = Metrics.GetAverageConcealment(_disputes, _side) * 100;
            return "Your agent has won "+ win
                +"% of disputes out of "+ _disputes.Count
                +" today, with an average content concealment of "+ con
                +"%";
        }

    public static string GiveExtensive(List<Dispute> _disputes, Side _side, bool _advice = false)
    {
        string feedback = GiveSummary(_disputes, _side);

        if (!_advice)
            feedback += "\nAn extensive feedback follows for each dispute:";
        else
            feedback += "\nAn advisory feedback follows for each dispute:";

        int disputes = 0;
        foreach (Dispute dispute in _disputes)
        {
            feedback += "\n-----------------------------------------\n";
            disputes++;
            if (_side == dispute.winner)
            {
                feedback += "\nYour agent won Dispute "+disputes+ ", as its opponent did not have a counterargument against your argument consisting of: \n";
                feedback += dispute.lastArguments[(int)_side].Convert();
                if (_advice)
                    feedback += "\nSo, this is a strong argument and it is a good choice to keep it in the knowledge base.";
            }
            else
            {
                feedback += "\nYour agent lost Dispute " + disputes + ", as your agent did not have a counterargument against your opponent's argument consisting of: \n";
                feedback += dispute.lastArguments[Math.Abs((int)_side-1)].Convert();

                if (_advice)
                {
                    feedback += "\nA possible improvement would be to add counterarguments to this opponent's argument to the knowledge base.";
                    feedback += "\nYour opponent's winning argument was in response to your agent's argument: " + dispute.lastArguments[(int)_side].Convert() +
                        "\n so another possible improvement would be for your agent to have chosen another argument (either remove this argument or move it down a level).";

                    if (dispute.agents[(int)_side].knowledgeBase.currentLevel < dispute.agents[(int)_side].knowledgeBase.levels.Count - 1)
                        feedback += "\nYour agent depleted " + (dispute.agents[(int)_side].knowledgeBase.currentLevel + 1)
                            + " out of "+ dispute.agents[(int)_side].knowledgeBase.levels.Count
                            + " levels, so another possible improvement would be for your agent to have been more willing to drop";

                    

                }
                    
            }
            
        }

        return feedback;
    }


    public static string GiveAdvice(List<Dispute> _disputes, Side _side)
        {
            return GiveExtensive(_disputes, _side, true);
        }
    

}

/*
 case EffortType.Shared:
                    {
                        if (CheckForfeitStatus(this))
                        {
                            _dispute.winner = (Side)Math.Abs(1 - (int)side);
                            _dispute.ongoing = false;
                            return;
                        }
                        if (currentAgent == agents.Count)
                        {
                            currentAgent = 0;
                        }

                        if (CheckHoldorForfeitStatus(this))
                        {
                            foreach(Agent agent in agents)
                            {
                                if (agent.teamStatus == TeamStatus.Hold)
                                {
                                    agent.teamStatus = TeamStatus.Ongoing;
                                    ConcealmentModule.DropLevel(agent);
                                }
                            }
                        }

                        //Console.WriteLine(currentAgent);

                        if (agents[currentAgent].teamStatus != TeamStatus.Ongoing)
                        {
                            //Console.WriteLine("Not ongoing Currentagent " + side + " " + currentAgent+" " +agents[currentAgent].teamStatus);
                            currentAgent++;
                            ExtendCase(_dispute);
                        }
                        else
                        {
                            //Console.WriteLine("Currentagent "+side+" " +currentAgent);
                            agents[currentAgent].ExtendCase(_dispute);
                            currentAgent++;
                            
                            return;
                        }
                    }
                    break;
 
 */

