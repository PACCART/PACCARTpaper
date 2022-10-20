using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text.Json.Serialization;

public interface IDispute
{
    public string Convert();
}


public class Dispute
{
    public string disputeID;

    public Side winner; 

    public int argumentCount;
    //subject
    public Term subject;

    public List<Agent> agents;

    //Teamwork Module feature
    public List<TeamworkModule.Team> teams;

    //Understandability Module feature
    //public UnderstandabilityModule.TextualFeedback textualFeedback;
    public List<Argument> lastArguments;
    public Argument opponentsLastArgument;
    public Argument proponentsLastArgument;


    public bool ongoing;

    public KnowledgeBase sharedKnowledgeBase;
    EvaluationManager evaluationManager;

    public List<HashSet<string>> revealedContent;
    public HashSet<string> revealedProponentContent;
    public HashSet<string> revealedOpponentContent;

    public List<Term>[] contrariesToAttack;
    List<Term> proponentContrariesToAttack;
    List<Term> opponentContrariesToAttack;


    //ASPIC+ Features
    //Link preference principle:  Weakest(default) or last link
    public Link link;
    //Semantics: Grounded (default), preferred or semi-stable
    public Semantics semantics;
    //true or false (default); whether or not to close the theory under transposition
    public bool transposition;

    public Dispute()
    {
        InitializeDispute();
    }

    public Dispute(string _subject, bool b = true)
    {
        subject = new Term(_subject, b);
        InitializeDispute();
    }

    public void InitializeDispute()
    {
        disputeID = "";
        argumentCount = 0;
        evaluationManager = new EvaluationManager();

        
        opponentsLastArgument = new Argument();
        proponentsLastArgument = new Argument();
        lastArguments = new List<Argument>() { proponentsLastArgument, opponentsLastArgument };

        Reset();

        ongoing = true;
        link = Link.Weakest;
        semantics = Semantics.Grounded;
        transposition = false;

        if (ModuleSettings.Instance.teamworkModule.IsActive())
        {
            agents = new List<Agent>();
            teams = new List<TeamworkModule.Team>();
        }
        else
        {
            agents = new List<Agent>();
        }
    }

    public void Reset()
    {
        sharedKnowledgeBase = new KnowledgeBase();

        proponentContrariesToAttack = new List<Term>();
        opponentContrariesToAttack = new List<Term>();
        contrariesToAttack = new List<Term>[] { proponentContrariesToAttack, opponentContrariesToAttack };

        revealedProponentContent = new HashSet<string>();
        revealedOpponentContent = new HashSet<string>();
        revealedContent = new List<HashSet<string>>() { revealedProponentContent, revealedOpponentContent };


    }

    public void AddArgument(Argument _argument, bool addContraries = true, bool _print = false)
    {
        argumentCount++;

        int side = (int)_argument.side;
        int opponentSide = Math.Abs(side - 1);

        lastArguments[side] = _argument;

        foreach (Rule rule in _argument.rules)
        {
            if (sharedKnowledgeBase.rules.Find(t => t.Equals(rule)) == null)
            {
                if (_print) Console.WriteLine("Adding to Revealed Content: " + rule.Convert());
                revealedContent[(int)_argument.side].Add(rule.Convert());
                //concealedContent[(int)_argument.side].Remove(rule.Convert());
                //_argument.side == Side.Proponent ? revealedProponentContent.Add(rule.Convert()) : revealedOpponentContent.Add(rule.Convert());
                //rule.revealed = true;
                sharedKnowledgeBase.rules.Add(rule);
                //Console.WriteLine("Adding to shared knowledge base: " + rule.Convert());
            }

            //_dispute.sharedKnowledgeBase.rules.Add(rule);

        }

        foreach (Term term in _argument.premises)
        {
            if (sharedKnowledgeBase.premises.Find(t => t.Equals(term)) == null)
            {
                if (_print) Console.WriteLine("Adding to Revealed Content: " + term.Convert());

                revealedContent[(int)_argument.side].Add(term.Convert());
                //concealedContent[(int)_argument.side].Remove(term.Convert());
                //term.revealed = true;
                sharedKnowledgeBase.premises.Add(term);
                //Console.WriteLine("Adding to shared knowledge base: " + term.Convert());
            }

            // term.revealed = true;
            //_dispute.sharedKnowledgeBase.premises.Add(term);

        }

        if (!addContraries)
        {
            return;
        }

        foreach (Rule rule in _argument.rules)
        {
            Term consequent = rule.consequent;

            if (sharedKnowledgeBase.contrariness.Count == 0)
            {
                contrariesToAttack[opponentSide].Add(consequent);
                if (_print) Console.WriteLine("Adding to the contraries: " + consequent.Convert() + " from rule " + rule.Convert() + "  to side " + (Side)opponentSide);
            }
            else
            {
                if (evaluationManager.Wins(this, consequent.Convert()).GetAwaiter().GetResult())
                {
                    if (_print) Console.WriteLine("Adding to the contraries: " + consequent.Convert() + " from rule " + rule.Convert() + "  to side " + (Side)opponentSide);
                    contrariesToAttack[opponentSide].Add(consequent);
                }
                else
                {
                    if (_print) Console.WriteLine("Not adding to the contraries: " + consequent.Convert());
                }
            }
        }

        foreach (Term term in _argument.premises)
        {
            if (sharedKnowledgeBase.contrariness.Count == 0) //because if zero then tree structure so no API call necessary
            {
                if (_print) Console.WriteLine("Adding to the contraries: Premise " + term.Convert() + " to side " + (Side)opponentSide);
                contrariesToAttack[opponentSide].Add(term);
            }
            else
            {
                if (evaluationManager.Wins(this, term.Convert()).GetAwaiter().GetResult())
                {
                    if (_print) Console.WriteLine("Adding to the contraries: Premise " + term.Convert() + " to side " + (Side)opponentSide);
                    contrariesToAttack[opponentSide].Add(term);
                }
                else
                {
                    if (_print) Console.WriteLine("Not adding to the contraries: " + term.Convert());
                }
            }
                
        }
    }

    public void MergeContraries()
    {
        if (ModuleSettings.Instance.teamworkModule.IsActive())
        {
            //sharedKnowledgeBase.contrariness = agents[0].knowledgeBase.contrariness.Concat(agents[1].knowledgeBase.contrariness).ToList();
            
        }
        else
            sharedKnowledgeBase.contrariness = agents[0].knowledgeBase.contrariness.Concat(agents[1].knowledgeBase.contrariness).ToList();
    }

    //E.g.    Console.WriteLine(dispute.Equals(new Term("test"),new Term("test", true)));
    public bool Equals(IDispute a, IDispute b)
    {
        return a.Convert() == b.Convert();
    }

    public List<Term> GetContrariesToAttack(Agent a)
    {
        return a.side == Side.Proponent ? proponentContrariesToAttack : opponentContrariesToAttack;
    }

    public Side GetWinner()
    {
        return winner;//evaluationManager.Wins(this, subject.Convert()).GetAwaiter().GetResult() ? Side.Proponent : Side.Opponent;
    }

    public List<Agent> GetAgents()
    {
        if (ModuleSettings.Instance.teamworkModule.IsActive())
        {
            return teams[0].agents.Concat(teams[1].agents).ToList();
        }
        else
        {
            return agents;
        }
    }

    public void UpdateAgentSettings(List<Agent> _agents)
    {
        if (ModuleSettings.Instance.teamworkModule.IsActive())
        {
            agents = new List<Agent>();
            foreach (Agent agent in _agents)
            {
                agents.Add(new Agent(agent.side, agent.argumentType, agent.dividingType, agent.droppingType));
            }
        }
        else
        {
            for (int i = 0; i < _agents.Count; i++)
            {
                agents[i].argumentType = _agents[i].argumentType;
                agents[i].dividingType = _agents[i].dividingType;
                agents[i].droppingType = _agents[i].droppingType;
            }
        }
        
    }




    public void Print()
    {
        Console.WriteLine("--------------------------------------------");
        Console.WriteLine("This Dispute contains the following content:");
        Console.WriteLine("\nPremises:");
        foreach (Term t in sharedKnowledgeBase.premises)
            Console.WriteLine(t.Convert()+";");
        Console.WriteLine("\nRules:");
        foreach (Rule r in sharedKnowledgeBase.rules)
            Console.WriteLine(r.Convert() + ";");
        Console.WriteLine("\nContrariness:");
        foreach (Contrary c in sharedKnowledgeBase.contrariness)
            Console.WriteLine(c.Convert());
        Console.WriteLine("--------------------------------------------");
    }

}

public class KnowledgeBase
{
    //ASPIC+ features used in this research
    // List of premises
    public List<Term> premises;
    // List of rules
    public List<Rule> rules;
    //List of contraries and contradictories
    public List<Contrary> contrariness;

    //Unused ASPIC+ features, but possible to use in further works:
    // List of axioms
    public Term[] axioms;
    // List of assumptions
    public Term[] assumptions;
    // List of knowledge base preferences (note: cannot contain any axioms)
    public string[] kbPrefs;
    //List of rule preferences
    public string[] rulePrefs;


    //PriSMART Module Extension Features
    public List<Level> levels;
    public int currentLevel;

    public KnowledgeBase()
    {
        axioms = Array.Empty<Term>();
        premises = new List<Term>();
        assumptions = Array.Empty<Term>();
        kbPrefs = Array.Empty<string>();
        rules = new List<Rule>();
        rulePrefs = Array.Empty<string>();
        contrariness = new List<Contrary>();


        levels = new List<Level>();
        currentLevel = 0;
    }

    public KnowledgeBase(Term[] _axioms, List<Term> _premises, Term[] _assumptions,
        string[] _kbPrefs, List<Rule> _rules, string[] _rulePrefs, List<Contrary> _contrariness, List<Level> _levels)
    {
        axioms = _axioms;
        premises = _premises;
        assumptions = _assumptions;
        kbPrefs = _kbPrefs;
        rules = _rules;
        rulePrefs = _rulePrefs;
        contrariness = _contrariness;

        levels = _levels;
        currentLevel = 0;
    }

    public KnowledgeBase(List<Argument> arguments)
    {
        axioms = Array.Empty<Term>();
        
        assumptions = Array.Empty<Term>();
        kbPrefs = Array.Empty<string>();
        
        rulePrefs = Array.Empty<string>();
        contrariness = new List<Contrary>();

        rules = new List<Rule>();
        premises = new List<Term>();

        foreach (Argument argument in arguments)
        {
            foreach (Term premise in argument.premises)
                premises.Add(premise);
            foreach (Rule rule in argument.rules)
                rules.Add(rule);

        }



        levels = new List<Level>();
        currentLevel = 0;
    }


    public KnowledgeBase(List<Argument> arguments, List<Argument> arguments2)
    {
        axioms = Array.Empty<Term>();

        assumptions = Array.Empty<Term>();
        kbPrefs = Array.Empty<string>();

        rulePrefs = Array.Empty<string>();
        contrariness = new List<Contrary>();

        rules = new List<Rule>();
        premises = new List<Term>();

        foreach (Argument argument in arguments)
        {
            foreach (Term premise in argument.premises)
                premises.Add(premise);
            foreach (Rule rule in argument.rules)
                rules.Add(rule);

        }

        foreach (Argument argument in arguments2)
        {
            foreach (Term premise in argument.premises)
                premises.Add(premise);
            foreach (Rule rule in argument.rules)
                rules.Add(rule);

        }



        levels = new List<Level>();
        currentLevel = 0;
    }


    public List<Term> GetNegations(List<Term> ts)
    {
        List<Term> negations = new List<Term>();
        foreach (Term t in ts)
        {
            Term n = new Term(t.name, !t.value);
            negations.Add(n);

            //Console.WriteLine("Adding to negations: " + n.Convert());

            foreach (Contrary c in contrariness)
            {
                if (c.a.Equals(t))
                {
                    negations.Add(c.b);
                   // Console.WriteLine("Adding to negations: " + c.b.Convert());
                }
                if (c.b.Equals(t))
                {
                    negations.Add(c.a);
                   // Console.WriteLine("Adding to negations: " + c.a.Convert());
                }
            }
        }
        return negations;
    }

    public bool ContainsPremise(Term _premise)
    {
        return FindPremises(_premise).Count != 0 ;
    }

    public List<Term> FindPremises(Term _premise)
    {
        return premises.FindAll(p => p.Convert() == _premise.Convert());
    }

    public bool ContainsRule(Term _consequent)
    {
        return FindRules(_consequent).Count != 0;
    }

    public List<Rule> FindRules(Term _consequent) //based on consequent
    {
        return rules.FindAll(r => r.consequent.Convert() == _consequent.Convert());
    }

    public List<Argument> GetArguments()
    {
        List<Argument> arguments = new List<Argument>();

        foreach(Rule rule in rules)
        {
            if (rule.ruleType == Rule.RuleType.Strict)
            {
                Argument argument = new Argument();
                Stack<Term> terms = new Stack<Term>();

                argument.rules.Add(rule);
                argument.top = rule.consequent;
                terms.Push(rule.antecedents[0]);

                while (terms.Count > 0)
                {
                    Term currentTerm = terms.Pop();
                    if (ContainsPremise(currentTerm))
                    {
                        argument.premises.Add(currentTerm);
                    }
                    else
                    {
                        List<Rule> rules = FindRules(currentTerm);
                        argument.rules.Add(rules[0]);
                        foreach (Term antecedent in rules[0].antecedents)
                        {
                            terms.Push(antecedent);
                        }
                    }
                }
                arguments.Add(argument);
            }
        }
        return arguments;
        throw new NotImplementedException();
    }

    public void MergeLevels()
    {
        foreach(Level level in levels)
        {
            premises = premises.Concat(level.premises).ToList();
            rules = rules.Concat(level.rules).ToList();
        }
    }

}

public enum Side
{
    Proponent = 0,
    Opponent = 1
}

public static class Extension
{
    public static Side GetOppositeSide(this Side _side)
    {
        return (Side)Math.Abs((int)_side - 1);
    }
}

public class Level
{
    public List<Term> premises;
    public List<Rule> rules;

    public Level()
    {
        premises = new List<Term>();
        rules = new List<Rule>();
    }

    public Level(List<Term> _premises, List<Rule> _rules)
    {
        premises = _premises;
        rules = _rules;
    }
}

public class Argument
{
    public Side side;
    public List<Rule> rules;
    public List<Term> premises;
    List<Contrary> contraries;

    public Term top;
    public int incomingAttacks;
    public int outgoingAttacks;

    public Argument()
    {
        side = new Side();
        rules = new List<Rule>();
        premises = new List<Term>();
        contraries = new List<Contrary>();
    }

    public Argument(Side _side, List<Rule> _rules, List<Term> _premises, List<Contrary> _contraries)
    {
        side = _side;
        rules = _rules;
        premises = _premises;
        contraries = _contraries;
    }

    public bool IsConcealed(HashSet<string> _revealedContent)
    {
        foreach (Rule rule in rules)
        {
            if (!_revealedContent.Contains(rule.Convert()))
                return true;
        }

        foreach (Term premise in premises)
        {
            if (!_revealedContent.Contains(premise.Convert()))
                return true;
        }

        return false;
    }

    public bool IsEmpty()
    {
        return rules.Count == 0 && premises.Count == 0;
    }

    public int GetSize()
    {
        return rules.Count() + premises.Count();
    }

    public string Convert()
    {
        List<string> strings = new List<string>(); 
        foreach (Term premise in premises)
            strings.Add( premise.Convert()); 
        foreach (Rule rule in rules)
            strings.Add(rule.Convert());
        return string.Join(", ",strings);
    }


}

public class Term : IDispute
{
    public bool value; //false indicates a negation
    public string name;
    //public Side side;
    //public bool revealed;

    public Term(string _name, bool _value = true) //Side _side = Side.Proponent)
    {
        
        value = _value;
        name = _name;
        //side = _side;
     //   revealed = false;
    }

    public string Convert()
    {
        //e.g. ~cool
        return value ? name : "~" + name;

    }

    //e.g. Console.WriteLine(new Term("test").Equals(new Term("test", true)));
    public bool Equals(Term _otherTerm)
    {
        return Convert() == _otherTerm.Convert(); //&& revealed == _term.revealed;
    }

    public bool IsRevealed(Dispute d, Side side = Side.Proponent)
    {
        return d.revealedContent[(int)side].Contains(Convert());
    }

    /*
    public bool IsConcealed(Dispute d, Side side = Side.Proponent)
    {
        return d.concealedContent[(int)side].Contains(Convert());
    }

    
    public bool IsContent(Dispute d, Side side = Side.Proponent)
    {
        return d.content[(int)side].Contains(Convert());
    }*/

}

public class Rule : IDispute
{
    int ID;
    //public Side side;
    public List<Term> antecedents;
    public Term consequent;
    public RuleType ruleType;

    public Rule(int _id, List<Term> _antecedents, Term _consequent, RuleType _ruleType = RuleType.Strict)
    {
        //InitializeRule(_id, _antecedent, _consequent, _ruleType);
        ID = _id;
        antecedents = _antecedents;
        consequent = _consequent;
        ruleType = _ruleType;
    }

    /*
    [JsonConstructor]
    public Rule(int _id, string _antecedent, string _consequent, RuleType _ruleType = RuleType.Strict)
    {
        InitializeRule(_id, new List<Term>(){ new Term(_antecedent)}, new Term(_consequent), _ruleType);
    }

    public void InitializeRule(int _id, List<Term> _antecedents, Term _consequent, RuleType _ruleType)
    {
        ID = _id;
        antecedents = _antecedents;
        consequent = _consequent;
        ruleType = _ruleType;
    }*/

    public string Convert()
    {
        //e.g. [r1] a,b,c -> d
        string rule = "[r" + ID + "] "; //[r1]
        string[] _antecedents = DisputeParser.Convert(antecedents);
        rule += string.Join(", ", _antecedents); //[r1] a,b,c 
        rule += ruleType == RuleType.Defeasible ? "=>" : "->"; //[r1] a,b,c ->
        rule += consequent.Convert(); //[r1] a,b,c -> d
        //rule += ";"; //[r1] a,b,c -> d;

        return rule;

    }

    public bool Equals(Rule _rule)
    {
        return Convert() == _rule.Convert();
    }

    public bool IsRevealed(Dispute d, Side side = Side.Proponent)
    {
        return d.revealedContent[(int)side].Contains(Convert());
    }

    /*
    public bool IsConcealed(Dispute d, Side side = Side.Proponent)
    {
        return d.concealedContent[(int)side].Contains(Convert());
    }

    
    public bool IsContent(Dispute d, Side side = Side.Proponent)
    {
        return d.content[(int)side].Contains(Convert());
    }*/

    public enum RuleType
    {
        Defeasible,
        Strict
    }
}

public class Contrary : IDispute
{
    public Term a;
    public Term b;

    public Contrary(Term _a, Term _b)
    {
        a = _a;
        b = _b;
    }

    public string Convert()
    {
        return a.Convert() + "-" + b.Convert();
    }

    public bool Equals(Contrary _contrary)
    {
        return (a.Equals(_contrary.a) && b.Equals(_contrary.b)) || (a.Equals(_contrary.b) && b.Equals(_contrary.a));
        //return Convert() == _contrary.Convert();
    }

}
public enum Link
{
    Weakest,
    Last
}

public enum Semantics
{
    Grounded,
    Stable,
    Preferred
}
