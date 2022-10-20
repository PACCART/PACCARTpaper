using System;
using System.Collections.Generic;
using System.Linq;

public class DisputeGenerator
{

	public List<Dispute> disputes;
	public HashSet<string> disputeIDs;

	//private 
	private Side currentSide;
	private List<int> premiseCounters;
	private int ruleCounter;

	private List<List<Tuple<Term,int>>> attackableTerms;
	private List<List<Term>> contraryPossibilities;
	private List<Contrary> contraryTops;




	public DisputeGenerator()//int _maxArgumentSize, int _maxDisputeSize, int _maxBranches, float _interconnectiveness)
	{
		//maxArgumentSize = _maxArgumentSize;
		//maxDisputeSize = _maxDisputeSize;
		//maxBranches = _maxBranches;
		//interconnectiveness = _interconnectiveness;

	}

	private int maxArgumentSize;
	private int maxDisputeSize;
	private int maxBranches;
	//private float interconnectiveness;

	public List<Dispute> GenerateDisputes(int _amount, MaxArgumentSize _maxArgumentSize, 
		MaxDisputeSize _maxDisputeSize, MaxBranches _maxBranches)//, Interconnectiveness _interconnectiveness)
    {
		return GenerateDisputes(_amount, (int)_maxArgumentSize, (int)_maxDisputeSize, (int)_maxBranches);//, //(float)_interconnectiveness / 100);
	}

	public List<Dispute> GenerateDisputes(int amount, int _maxArgumentSize, int _maxDisputeSize, int _maxBranches)//, //float _interconnectiveness)
    {
		disputes = new List<Dispute> ();
		disputeIDs = new HashSet<string> ();
		maxArgumentSize = _maxArgumentSize;
		maxDisputeSize = _maxDisputeSize;
		maxBranches = _maxBranches;
		//interconnectiveness = _interconnectiveness;

		int fails = 0;
		while (disputes.Count < amount)
        {
			if (fails > 100)
            {
				Console.WriteLine("Generator ended with {0} disputes for parameter settings: {1},{2},{3}", disputes.Count, maxArgumentSize, maxDisputeSize, maxBranches);
				break;
            }

			Dispute dispute = GenerateDispute();

			if (!disputeIDs.Contains(dispute.disputeID))
            {
				disputeIDs.Add(dispute.disputeID);
				//Console.WriteLine(dispute.disputeID);
				disputes.Add(dispute);
			}
			else
            {
				//Console.WriteLine("fail");
				fails++;
			}
        }

		return disputes;

	}

	private Dispute dispute;
	private Agent proponent;
	private Agent opponent;
	//List<Agent> agents;
	private List<Argument> arguments;

	private Dispute GenerateDispute()
	{
		//Initialize
		premiseCounters = new List<int> ()
		{
			1, //[0] proponentPremiseCounter, 
			1, //[1] opponentPremiseCounter, 
		};

		ruleCounter = 1;

		attackableTerms = new List<List<Tuple<Term, int>>> () { new List<Tuple<Term, int>> (), new List<Tuple<Term, int>> () };
		contraryPossibilities = new List<List<Term>> () { new List<Term> (), new List<Term> () };
		contraryTops = new List<Contrary> ();



		//Add Agents & divide
		dispute = new Dispute ();
		proponent = new Agent (Side.Proponent);
		opponent = new Agent (Side.Opponent);
		arguments = new List<Argument> ();
		//agents = new() { proponent, opponent };
		dispute.agents.Add(proponent);
		dispute.agents.Add(opponent);


		dispute.subject = new Term("subject");
		currentSide = Side.Proponent;

		//First step
		GenerateArgument(dispute.subject);
		SwitchSide();


		while (dispute.argumentCount < maxDisputeSize && attackableTerms[(int)currentSide].Count!=0)
        {
			//Find random attackable term and generate argument against it
			int index = new Random().Next(0, attackableTerms[(int)currentSide].Count);
			Term attackableTerm = attackableTerms[(int)currentSide][index].Item1;
			int times = attackableTerms[(int)currentSide][index].Item2;
			//Console.WriteLine(times);
			attackableTerms[(int)currentSide].RemoveAt(index);

			Term negationTerm = new Term (attackableTerm.name, !attackableTerm.value);

			for(int i = 0; i <= times; i++)
				GenerateArgument(negationTerm);
			SwitchSide();
			contraryTops.Add(new Contrary(attackableTerm,negationTerm));
			contraryTops.Add(new Contrary(negationTerm, attackableTerm));

		}

		//dispute.sharedKnowledgeBase.contrariness = GenerateContraries();		
		dispute.disputeID = GenerateDisputeID();

		return dispute;
    }

	/*
	private List<Contrary> GenerateContrariesSlow()
    {
		List<Contrary> contraries = new();

		if (interconnectiveness == 0)
			return contraries;

		for (int i = 0; i < contraryPossibilities[0].Count; i++)
        {
			for (int j = 0; j < contraryPossibilities[1].Count; j++)
            {
				Contrary newContrary = new Contrary(contraryPossibilities[0][i], contraryPossibilities[1][j]);
				//Console.WriteLine("this: "+newContrary.Convert());
				if (contraries.Where(c => c.Equals(newContrary)).ToList().Count == 0)
                {
					if (contraryTops.Where(c => c.Equals(newContrary)).ToList().Count == 0)
						contraries.Add(newContrary);
				}
            }
        }

		float amount = contraries.Count * interconnectiveness;
		Random rnd = new();
		return contraries.OrderBy(x => rnd.Next()).Take((int)amount).ToList();

    }

	private List<Contrary> GenerateContraries()
	{
		List<Contrary> contraries = new();

		if (interconnectiveness == 0)
			return contraries;

		Random rnd = new();
		//Console.WriteLine(contraryPossibilities[0].Count + " " + contraryPossibilities[1].Count);
		float amount = contraryPossibilities[0].Count * interconnectiveness;
		List<Term> a = contraryPossibilities[0].OrderBy(x => rnd.Next()).Take((int)amount).ToList();
		amount = contraryPossibilities[1].Count * interconnectiveness;
		List<Term> b = contraryPossibilities[1].OrderBy(x => rnd.Next()).Take((int)amount).ToList();
		//Console.WriteLine(a.Count + " " + b.Count);

		for (int i = 0; i < a.Count; i++)
		{
			for (int j = 0; j < b.Count; j++)
			{
				Contrary newContrary = new Contrary(a[i], b[j]);
				//Console.WriteLine("this: "+newContrary.Convert());
				if (contraries.Where(c => c.Equals(newContrary)).ToList().Count == 0)
				{
					if (contraryTops.Where(c => c.Equals(newContrary)).ToList().Count == 0)
						contraries.Add(newContrary);
				}
			}
		}

		//float amount = contraries.Count * interconnectiveness;
		//Console.WriteLine(contraries.Count);
		return contraries;//.OrderBy(x => rnd.Next()).Take((int)amount).ToList();

	}
	*/

	private void GenerateArgument(Term _goal)
    {
		int argumentSize = new Random().Next(0, maxArgumentSize - 1);
		//argumentSize--;
		//Console.WriteLine(argumentSize);
		Argument argument = new Argument ();

		//generate top
		Term consequent = _goal;
		Term antecedent = new Term(GeneratePremiseID());
		Rule topRule = new Rule (GenerateRuleID(), new List<Term> () { antecedent }, consequent, Rule.RuleType.Strict);

		if (argumentSize > 0)
			argument = GenerateRecursiveArgument(antecedent, argumentSize);
		else
			argument.premises.Add(antecedent);
		argument.top = _goal;
		argument.rules.Insert(0,topRule);
		

		UpdateAttackableTerms(argument);
		dispute.AddArgument(argument, false);
		dispute.agents[(int)currentSide].AddArgument(argument);
		arguments.Add(argument);
		
	}

	private void UpdateAttackableTerms(Argument _argument)
    {
		//attackable weak points are stored
		foreach (Rule rule in _argument.rules)
		{
			contraryPossibilities[(int)GetOtherSide()].Add(rule.consequent);
			if (rule.consequent.Equals(_argument.top))
				continue;

			//for (int i = 0; i<= new Random().Next(0, maxBranches); i++)
            //{
			attackableTerms[(int)GetOtherSide()].Add(new Tuple<Term, int>(rule.consequent, new Random().Next(0, maxBranches)));
			//}
			//if (!contraryPossibilities[(int)GetOtherSide()].Contains(rule.consequent))
		}

		foreach (Term premise in _argument.premises)
		{
			contraryPossibilities[(int)GetOtherSide()].Add(premise);
			//for (int i = 0; i <= new Random().Next(0, maxBranches); i++)
            //{
			attackableTerms[(int)GetOtherSide()].Add(new Tuple<Term, int>(premise, new Random().Next(0, maxBranches)));
				//if (!contraryPossibilities[(int)GetOtherSide()].Contains(premise))
			//}
		}
	}

	//Recursive way of generating a random argument shape
	//consisting of smaller subarguments 
	private Argument GenerateRecursiveArgument(Term goal, int size)
    {
		//Base case:
		//if one leaf is left, turn it into an argument with 1 rule and 1 premise
		if (size == 1)
        {
			return GenerateLongArgument(goal, size);
			//return new Random().Next(0, 2) == 0 ? GenerateLongArgument(goal, size) : GenerateWideArgument(goal, size);
		}

		//We partition in a random location in the argument size
		int leftsize = new Random().Next(1, size-1);
		int rightsize = size - leftsize;

		//One side gets recursively generated further
		Argument leftargument = GenerateRecursiveArgument(goal, leftsize);
		Term rightgoal = leftargument.premises[0];

		//The other side randomly generates a long or wide argument
		Argument rightargument = new Random().Next(0, 2) == 0 ? GenerateLongArgument(rightgoal, rightsize) : GenerateWideArgument(rightgoal, rightsize);

		//Both sides are combined to be returned as larger argument of desired size
		//Watch out, you cannot just concatenate the premises as some premises are no longer necessary!!
		//The top of one argument should be removed
		List<Rule> rules = leftargument.rules.Concat(rightargument.rules).ToList();
		List<Term> premises = leftargument.premises.Concat(rightargument.premises).ToList();
		premises.Remove(rightgoal);

		return new Argument(currentSide, rules, premises,new List<Contrary> ());
	}

	//Wide argument -> 1 rule with lots of premises;
	private Argument GenerateWideArgument(Term goal, int size)
	{
		//Generate a random amount of antecedents based on maxArgumentSize
		List<Term> antecedents = new List<Term> ();
		
		for (int i = 0; i < size; i++)
		{ 
			antecedents.Add(new Term(GeneratePremiseID()));
		}

		//Generate 1 rule with all antecedents 
		List<Rule> rule = new List<Rule> () { new Rule (GenerateRuleID(), antecedents, goal, Rule.RuleType.Defeasible) };

		//Generate 1 argument with this rule and all antecedents as premises
		return new Argument (currentSide, rule, antecedents, new List<Contrary> ());
	}

	//Long argument -> multiple rules with lots of premises;
	private Argument GenerateLongArgument(Term goal, int size)
    {
		Term consequent = goal;
		Term antecedent = null;
		List<Rule> rules = new List<Rule> ();
		for (int i = 0; i < size; i++)
        {
			antecedent = new Term(GeneratePremiseID());

			Rule rule = new Rule (GenerateRuleID(), new List<Term> (){ antecedent }, consequent,Rule.RuleType.Defeasible);
			rules.Add(rule);
			
			consequent = antecedent;
		}

		//Generate 1 argument with this rule and all antecedents as premises
		return new Argument (currentSide, rules, new List<Term> () { antecedent }, new List<Contrary> ());
	}

	private Argument GenerateTopArgument(Term goal)
    {
		Term consequent = goal;
		Term antecedent = new Term(GeneratePremiseID());
		Rule rule = new Rule (GenerateRuleID(), new List<Term> () { antecedent }, consequent,Rule.RuleType.Strict);
		return new Argument (currentSide, new List<Rule> () { rule}, new List<Term> () { antecedent }, new List<Contrary> ());

	}
	private int GenerateRuleID()
    {
		//int ruleID = counters[(int)currentSide][1];
		return ruleCounter++;
	}

	private string GeneratePremiseID()
	{
		if (currentSide == Side.Opponent)
			return "o" + (premiseCounters[(int)currentSide]++).ToString();
		else
			return "p" + (premiseCounters[(int)currentSide]++).ToString();
	}

	private string GenerateDisputeID()
    {
		//count ins, outs
		CountAttacks();

		//generate
		List<Tuple<int, int, int>> subIDs = new List<Tuple<int, int, int>> ();
		foreach (Argument argument in arguments)
        {
			int size = argument.GetSize();
			int outs = argument.outgoingAttacks;
			int ins = argument.incomingAttacks;
			subIDs.Add(new Tuple<int, int, int>(size, outs, ins));
		}

		subIDs.Sort();
		string ID = "";
		for (int i = 0; i < subIDs.Count; i++) 
		{
			string subID = "" + subIDs[i].Item1 + subIDs[i].Item2 + subIDs[i].Item3;
			ID += subID;
		}

		//Console.WriteLine(ID);
		return ID;

	}

	private void CountAttacks()
    {
		//todo, include contraries
		foreach (Argument argument in arguments)
		{
			if (argument.top == dispute.subject) //.rules.Where(rule => rule.consequent == dispute.subject) != null)
			{
				argument.outgoingAttacks = 0;
			}
			else
			{
				argument.outgoingAttacks = 1;
			}

			foreach (Argument otherArgument in arguments)
			{
				if (otherArgument == argument)
					continue;

				Term negation = new Term (argument.top.name, !argument.top.value);

				foreach (Term premise in otherArgument.premises)
				{
					if (negation.Equals(premise))
					{
						//Console.WriteLine("There is an incoming attack from {0} to {1}", argument.top.Convert(), premise.Convert());
						otherArgument.incomingAttacks++;
					}
				}

				foreach (Rule rule in otherArgument.rules)
				{
					if (negation.Equals(rule.consequent) && !rule.consequent.Equals(otherArgument.top))
					{
						//Console.WriteLine("!! There is an incoming attack from {0} to {1}", argument.top.Convert(), rule.consequent.Convert());
						otherArgument.incomingAttacks++;
					}
				}
			}

		}
	}

	public void SwitchSide()
    {
		currentSide = GetOtherSide();
    }

	public Side GetOtherSide()
    {
		return (Side)Math.Abs(1 - (int)currentSide);
	}

}

public enum MaxArgumentSize
{
	Short = 3,
	Long = 10,
	Extreme = 100,
}

public enum MaxDisputeSize
{
	Small = 5,
	Medium = 20,
	Large = 100,
};
public enum MaxBranches
{
	Single = 1,
	Some = 2,
	Many = 5
}
public enum Interconnectiveness
{
	None = 0,
	Additional = 10,
	Very = 40
};