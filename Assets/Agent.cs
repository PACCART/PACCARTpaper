using System;
using System.Collections.Generic;
using System.Linq;

public class Agent
{
    public Side side;
    

    public KnowledgeBase knowledgeBase;

    public ConcealmentModule.ArgumentType argumentType;
    public ConcealmentModule.DividingType dividingType;
    public ConcealmentModule.DroppingType droppingType;

    public EquityModule.PrivacyType privacyType;


    //public TeamworkModule.EffortType effortType;
    //public TeamworkModule.Overlap overlap;
    public TeamworkModule.TeamStatus teamStatus;
    public TeamworkModule.Team team;

    bool creating;

    public Agent()
    {
        knowledgeBase = new KnowledgeBase();
        ModuleSettings.Instance.SetDefaultSettings(this);
        creating = false;

        //effortType = TeamworkModule.EffortType.No;
        //overlap = TeamworkModule.Overlap.Full;
        teamStatus = TeamworkModule.TeamStatus.Ongoing;
    }

    public Agent(Side _side)
    {
        knowledgeBase = new KnowledgeBase();
        side = _side;
        ModuleSettings.Instance.SetDefaultSettings(this);
        creating = false;

        //effortType = TeamworkModule.EffortType.No;
        //overlap = TeamworkModule.Overlap.Full;
        teamStatus = TeamworkModule.TeamStatus.Ongoing;
    }

    public Agent(Side _side,
        ConcealmentModule.ArgumentType _argumentType = ConcealmentModule.ArgumentType.Random,
        ConcealmentModule.DividingType _dividingType = ConcealmentModule.DividingType.None,
        ConcealmentModule.DroppingType _droppingType = ConcealmentModule.DroppingType.Always)
    {
        knowledgeBase = new KnowledgeBase();
        side = _side;
        argumentType = _argumentType;
        dividingType = _dividingType;
        droppingType = _droppingType;
        creating = false;

        //effortType = TeamworkModule.EffortType.No;
        //overlap = TeamworkModule.Overlap.Full;
        teamStatus = TeamworkModule.TeamStatus.Ongoing;

    }

    public Dispute CreateCase(Dispute _dispute)
    {
        creating = true;
        List<Term> goal = new List<Term>() { _dispute.subject };
        return Defend(goal, _dispute);
    }

    public Dispute ExtendCase(Dispute _dispute, bool _print  = false)
    {
        creating = false; 
        List<Term> contrariesToAttack = GetContrariesToAttack(_dispute);
        if (_print) Console.WriteLine("Amount of contraries to attack: "+contrariesToAttack.Count);
        List<Term> negations = knowledgeBase.GetNegations(contrariesToAttack);
        //Console.WriteLine("{0} negations", negations.Count);
        return Defend(negations, _dispute);
    }

    //Later maybe add: amortized way of decreasing computation time by saving found arguments
    public Dispute Defend(List<Term> goals, Dispute _dispute)
    {
        //graph search through rules until a premise is found on all leaves, add all rules and premise to the case
        List<Rule> rules = new List<Rule>();
        List<Term> premises = new List<Term>();
        List<Argument> possibleArguments = new List<Argument>();

        foreach (Term goal in goals)
        {
            List<Argument> arguments = FindArguments(goal, rules, premises);
            possibleArguments = possibleArguments.Concat(arguments).ToList();
        }
        ChooseArgument(_dispute, possibleArguments);

        //Console.WriteLine(_dispute.sharedKnowledgeBase.premises.Count + _dispute.sharedKnowledgeBase.rules.Count);
        //(float a, float b, float c) = Metrics.GetSidedConcealmentMetrics(new List<Dispute>() { _dispute },Side.Proponent,false);
        //Console.WriteLine("Proponent (All) Win: "+a + ", con: " + b);
        //(float aa, float bb, float cc) = Metrics.GetSidedConcealmentMetrics(new List<Dispute>() { _dispute }, Side.Opponent, false);
        //Console.WriteLine("Proponent (Longest) Win: " + aa + ", con: " + bb);
        //_dispute.Print();

        return _dispute;

    }

    private List<Term> GetContrariesToAttack(Dispute _dispute)
    {
        return _dispute.contrariesToAttack[(int)side];
    }

    public List<Argument> FindArguments(Term _term, List<Rule> _rules, List<Term> _premises)
    {
        //returns arguments of subarguments based on finding recusrively premises and returning wokring rules to get to such an argument.
        List<Argument> arguments = new List<Argument>();

        //check if it is a premise
        if (knowledgeBase.ContainsPremise(_term)) //if (knowledgeBase.premises.Find(t => t.Equals(_term)) != null)
        {
            Argument argument = new Argument(side, new List<Rule>(), new List<Term>(), new List<Contrary>());
            argument.premises.Add(_term);
            arguments.Add(argument);
        }

        //else continue search for premise through rules
        List<Rule> rules = knowledgeBase.FindRules(_term);//knowledgeBase.rules.FindAll(r => r.consequent.Equals(_term));

        //Go through each found rule
        foreach (Rule rule in rules)
        {
            //Check based on all its antecedents whether this rule is useful
            List<Argument> ruleArguments = new List<Argument>();
            List<bool> ruleWorksOptions = new List<bool>();
            List<List<List<Rule>>> antecedentRulesOptions = new List<List<List<Rule>>>() { new List<List<Rule>>()};
            List<List<List<Term>>> antecedentPremisesOptions = new List<List<List<Term>>>() { new List<List<Term>>()};

            //If all antecedents are also supported, this rule is good
            foreach (Term t in rule.antecedents)
            {
                int premisesCount = _premises.Count;
                ruleArguments = FindArguments(t, _rules, _premises);
                for (int i = 0; i< ruleArguments.Count; i++)
                {
                    antecedentRulesOptions[i].Add(ruleArguments[i].rules);
                    antecedentPremisesOptions[i].Add(ruleArguments[i].premises);
                    if (ruleWorksOptions.Count<=i)
                    {
                        ruleWorksOptions.Add(ruleArguments[i].premises.Count != premisesCount);
                    }
                    else
                    {
                        ruleWorksOptions[i] = ruleArguments[i].premises.Count != premisesCount;
                    }
                }
            }

            for (int i = 0; i< ruleWorksOptions.Count; i++)
            {
                Argument ar = new Argument(side, new List<Rule>(), new List<Term>(), new List<Contrary>());
                ar.rules = antecedentRulesOptions[i].SelectMany(i => i).ToList();
                ar.premises = antecedentPremisesOptions[i].SelectMany(i => i).ToList();
                ar.rules.Add(rule);
                arguments.Add(ar);
            }
        }

        return arguments;

    }

    //This function chooses which argument of the possible arguments to add to the dispute
    //if it cannot find any suitable argument to add to the dispute (already added, empty list of possibilities, the dispute is forfeited)
    public void ChooseArgument(Dispute _dispute, List<Argument> _possibleArguments, bool _print = false)
    {
        List<Argument> filteredArguments = _possibleArguments.Where(i => i.IsConcealed(_dispute.revealedContent[(int)side]) && !i.IsEmpty()).ToList();
        //Console.WriteLine(filteredArguments.Count);
        

        if (filteredArguments.Count == 0)
        {
            if (ModuleSettings.Instance.concealmentModule.IsActive() && ConcealmentModule.ConsiderDropping(this))
            {
                if (ModuleSettings.Instance.teamworkModule.IsActive() && !creating)
                {
                    teamStatus = TeamworkModule.TeamStatus.Hold;
                }
                else
                {
                    ConcealmentModule.DropLevel(this);
                    //Console.WriteLine("Drop!");
                    //Console.WriteLine(knowledgeBase.premises.Count); HIER WEER NAAR KIJKEN VANDAAG
                    if (creating)
                        CreateCase(_dispute);
                    else
                        ExtendCase(_dispute);
                }
                
            }
            else
            {
                Forfeit(_dispute);
            }
                
        }
        else
        {
            if (ModuleSettings.Instance.concealmentModule.IsActive())
            {
                ConcealmentModule.AddArguments(_dispute, filteredArguments, argumentType);
            }
            else
            {
                AddAllArguments(_dispute, filteredArguments);
            }
        }
      
    }

    public static void AddAllArguments(Dispute _dispute, List<Argument> _arguments)
    {
        foreach (Argument argument in _arguments)
        {
            _dispute.AddArgument(argument);
        }
    }

    public void AddArgument(Argument _argument)
    {
        foreach (Rule rule in _argument.rules)
        {
            knowledgeBase.rules.Add(rule);
        }

        foreach (Term premise in _argument.premises)
        {
            knowledgeBase.premises.Add(premise);
        }

       
    }

    public void Forfeit(Dispute _dispute)
    {
        if (ModuleSettings.Instance.teamworkModule.IsActive())
        {
            teamStatus = TeamworkModule.TeamStatus.Forfeit;
            if (TeamworkModule.CheckForfeitStatus(team))
            {
                _dispute.winner = (Side)Math.Abs(1 - (int)side);
                _dispute.ongoing = false;
            }
        }
        else
        {
            _dispute.winner = (Side)Math.Abs(1 - (int)side);
            _dispute.ongoing = false;
        }


    }
}
