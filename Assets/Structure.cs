using System;
using System.Collections.Generic;
using System.Linq;

//Structure
public class Structure
{
	Dispute dispute;
	SidedGraph proponentGraph;
	SidedGraph opponentGraph;
	List<SidedGraph> sidedGraphs;
	HashSet<string> arguments;

	public SidedGraph mergedGraph;

	public Structure(Dispute _dispute)
    {
		dispute = _dispute;
		proponentGraph = new SidedGraph (Side.Proponent);
		opponentGraph = new SidedGraph (Side.Opponent);
		sidedGraphs = new List<SidedGraph> () { proponentGraph, opponentGraph };
		arguments = new HashSet<string> ();

		CreateDisputeStructure();
	}

	public void CreateDisputeStructure()
    {
		//Initialisation
		Queue<Tuple<Term, int, int, Side>> queue = new Queue<Tuple<Term, int, int, Side>> ();
		SidedGraph initialArgument = CreateArgumentStructure(dispute.subject);
		proponentGraph.AddArgument(initialArgument, 0);
		arguments.Add(initialArgument.Convert());
		List<Tuple<Term, int, int>> attackers = initialArgument.FindAttackers(dispute);
		Console.WriteLine(attackers.Count);
		for (int i = 0; i< attackers.Count; i++)
        {
			queue.Enqueue(new Tuple<Term, int,int, Side>(attackers[i].Item1, attackers[i].Item2, attackers[i].Item3, Side.Opponent));
		}

		//loop
        while (queue.Count > 0)
        {
			Tuple<Term, int, int, Side> current = queue.Dequeue();
			Term currentTerm = current.Item1;
			int currentHeight = current.Item2;
			int currentIndex = current.Item3;
			Side currentSide = current.Item4;

			SidedGraph currentArgument = CreateArgumentStructure(currentTerm, currentSide, currentIndex);
			//currentArgument.PrintStructure();
			if (arguments.Contains(currentArgument.Convert()))
				continue;
			arguments.Add(currentArgument.Convert());
			sidedGraphs[(int)currentSide].AddArgument(currentArgument, currentHeight);
			List<Tuple<Term,int,int>> currentAttackers = currentArgument.FindAttackers(dispute);
			//Console.WriteLine(currentAttackers.Count);
			for (int i = 0; i < currentAttackers.Count; i++)
			{
				queue.Enqueue(new Tuple<Term, int, int, Side>(
					currentAttackers[i].Item1, 
					currentAttackers[i].Item2, 
					currentAttackers[i].Item3, 
					(Side)Math.Abs((int)currentSide-1)));
			}
		}

		mergedGraph = MergeStructures(proponentGraph,opponentGraph);
	}

	/*
	public void BackupCreateDisputeStructure()
	{
		//Initialisation
		Queue<Tuple<Term, int, int, Side>> queue = new();
		HashSet<Tuple<Term, Term>> allAttacks = new();  //Todo strip this out!!
		SidedGraph initialArgument = CreateArgumentStructure(dispute.subject);
		proponentGraph.AddArgument(initialArgument, 0);
		(List<Tuple<Term, int>> attackers, List<Tuple<Term, Term>> attacks) = initialArgument.FindAttackers(dispute, allAttacks);
		Console.WriteLine(attackers.Count);
		for (int i = 0; i < attackers.Count; i++)
		{
			queue.Enqueue(new Tuple<Term, int, int, Side>(attackers[i].Item1, attackers[i].Item2, i, Side.Opponent));
		}
		foreach (Tuple<Term, Term> attack in attacks)
		{
			allAttacks.Add(attack);
		}

		//loop
		while (queue.Count > 0)
		{
			Tuple<Term, int, int, Side> current = queue.Dequeue();
			Term currentTerm = current.Item1;
			int currentHeight = current.Item2;
			int currentIndex = current.Item3;
			Side currentSide = current.Item4;

			SidedGraph currentArgument = CreateArgumentStructure(currentTerm, currentSide, currentIndex);
			sidedGraphs[(int)currentSide].AddArgument(currentArgument, currentHeight);
			(List<Tuple<Term, int>> currentAttackers, List<Tuple<Term, Term>> currentAttacks) = currentArgument.FindAttackers(dispute, allAttacks);
			for (int i = 0; i < currentAttackers.Count; i++)
			{
				queue.Enqueue(new Tuple<Term, int, int, Side>(currentAttackers[i].Item1, currentAttackers[i].Item2, i, (Side)Math.Abs((int)currentSide - 1)));
			}
			foreach (Tuple<Term, Term> attack in currentAttacks)
			{
				allAttacks.Add(attack);
			}
		}
	}
	*/

	public SidedGraph CreateArgumentStructure(Term _term, Side _side = Side.Proponent, int _index = 0)
    {
		SidedGraph sidedGraph = new SidedGraph (_side);
		sidedGraph.CreateArgumentStructure(dispute, _term, _index);
		//sidedGraph.PrintStructure();
		return sidedGraph;
    }

	public SidedGraph MergeStructures(SidedGraph _p, SidedGraph _o)
    {
		int max = 0;
		foreach (SidedNode sidedNode in _p.nodes)
			max = Math.Max(max, sidedNode.x);
		_p.width = max;
		//int width = _p.width + _o.width;
		//int height = Math.Max(_p.height, _o.height);
		SidedGraph merged = new SidedGraph (Side.Proponent);
		//merged.width = width;
		//merged.height = height;

		foreach (SidedEdge sidedEdge in _p.edges)
			merged.edges.Add(sidedEdge);
		foreach (SidedEdge sidedEdge in _o.edges)
			merged.edges.Add(sidedEdge);
		foreach (SidedNode sidedNode in _p.nodes)
			merged.nodes.Add(sidedNode);
		foreach (SidedNode sidedNode in _o.nodes)
        {
			sidedNode.x += _p.width + 1;
			merged.nodes.Add(sidedNode);
		}
		return merged;
	}

	public void PrintSidedGraphs()
    {
		Console.WriteLine("Proponent Graph:");
		proponentGraph.PrintStructure();
		Console.WriteLine("width "+proponentGraph.width);
		Console.WriteLine();
		Console.WriteLine("Opponent Graph:");
		opponentGraph.PrintStructure();
		Console.WriteLine();
		Console.WriteLine("Merged Graph:");
		mergedGraph.PrintStructure();
		

	}

}

public class SidedGraph
{
	Side side;
	public List<SidedEdge> edges;
	public List<SidedNode> nodes;
	public int width;
	public int height;

	public SidedGraph(Side _side)
	{
		side = _side;
		edges = new List<SidedEdge> ();
		nodes = new List<SidedNode> ();
		width = 0;
		height = 0;

	}

	Stack<Tuple<Term, int>> stack;
	public void CreateArgumentStructure(Dispute _dispute, Term _term, int _index = 0)
	{
		bool first = _index == 0 ? false : true;
		stack = new Stack<Tuple<Term, int>> ();
		KnowledgeBase kb = _dispute.sharedKnowledgeBase;
		stack.Push(new Tuple<Term, int>(_term, 0));

		while (stack.Count != 0)
		{
			Tuple<Term, int> current = stack.Pop();
			Term term = current.Item1;
			int currentHeight = current.Item2;
			height = Math.Max(height, currentHeight);

			SidedNode n = new SidedNode (Side.Proponent,new Term ("null"),0,0);

			//If premise, add
			if (kb.ContainsPremise(term))
			{
				AddNode(term, width, currentHeight);
				width++;
			}
			else
			{
				Rule rule;

				if (first)
				{
					rule = kb.FindRules(term)[_index];
					first = false;
				}
				else
				{
					rule = kb.FindRules(term)[0];
				}

				n = AddNode(term, width, currentHeight);
				AddEdge(rule, n);
				List<Term> antecedents = rule.antecedents;

				for (int i = antecedents.Count - 1; i >= 0; i--)
				{
					stack.Push(new Tuple<Term, int>(antecedents[i], currentHeight + 2));
				}

			}
		}

	}

	public void AddArgument(SidedGraph _argumentGraph, int _height)
	{
		int argumentWidth = _argumentGraph.width;
		int argumentHeight = _argumentGraph.height;

		int xIndex = 0;
		int yIndex = _height;
		while (true)
		{
			bool fits = true;
			for (int x = 0; x < argumentWidth; x++)
			{
				for (int y = 0; y < argumentHeight; y++)
				{
					if (nodes.FindAll(n => n.x == xIndex && n.y == yIndex).Count > 0)
					{
						fits = false;
					}
				}
			}
			if (fits)
			{
				foreach (SidedNode node in _argumentGraph.nodes)
				{
					node.x += xIndex;
					node.y += yIndex;
					nodes.Add(node);
				}
				foreach (SidedEdge edge in _argumentGraph.edges)
				{
					edges.Add(edge);
				}
				return;
			}
			else
			{
				xIndex++;
			}
		}
	}

	public List<Tuple<Term, int, int>> FindAttackers(Dispute _dispute)
	{
		List<Tuple<Term, int, int>> attackers = new List<Tuple<Term, int, int>> ();
		KnowledgeBase kb = _dispute.sharedKnowledgeBase;
		foreach (SidedNode node in nodes)
        {
			List<Term> opposites = kb.GetNegations(new List<Term> () { node.term });
			for (int i = 0; i< opposites.Count;i++)
			{
				List<Rule> rules = kb.FindRules(opposites[i]);
				for (int j = 0; j< rules.Count; j++)
				{
					if (rules[j].consequent.Convert()[0] == '~')
                    {
						Term consequent = rules[j].consequent;
						attackers.Add(new Tuple<Term, int, int>(consequent, node.y + 2, j));
					}
					
				}
			}
		}

		return attackers;
	}

	/*
	public (List<Tuple<Term, int>>, List<Tuple<Term, Term>>) BackupFindAttackers(Dispute _dispute, HashSet<Tuple<Term, Term>> allAttacks)
	{
		List<Tuple<Term, int>> attackers = new();
		List<Tuple<Term, Term>> attacks = new();
		KnowledgeBase kb = _dispute.sharedKnowledgeBase;
		foreach (SidedNode node in nodes)
		{
			List<Term> opposites = kb.GetNegations(new() { node.term });
			foreach (Term opposite in opposites)
			{
				//Term opposite = new Term(node.term.name, !node.term.value);
				//Console.WriteLine(opposite.Convert());
				//Console.WriteLine(kb.ContainsRule(opposite));
				if (kb.ContainsRule(opposite) && !allAttacks.Contains(new Tuple<Term, Term>(opposite, node.term)))
				{
					foreach (Rule rule in kb.FindRules(opposite))
					{
						Term consequent = rule.consequent;
						attackers.Add(new Tuple<Term, int>(consequent, node.y + 2));
						attacks.Add(new Tuple<Term, Term>(consequent, node.term));
						attacks.Add(new Tuple<Term, Term>(node.term, consequent));
					}
				}
			}
		}

		return (attackers, attacks);
	}
	*/
	/*
	public class SidedGraph{

		Side currentSide;
		List<SidedEdge> edges;
		List<SidedNode> nodes;

		int proponentWidth;
		int opponentWidth;
		List<int> width;
		//Dictionary<int, int> proponentRows;
		//Dictionary<int, int> opponentRows;
		//List<Dictionary<int, int>> rows;
		int height;

		public SidedGraph()
		{
			currentSide = Side.Proponent;
			edges = new();
			nodes = new();

			proponentWidth = 0;
			opponentWidth = 0;
			width = new() { proponentWidth, opponentWidth };
			//proponentRows = new();
			//opponentRows = new();
			//rows = new() { proponentRows, opponentRows };
			height = 0;

		}

		public void CreateStructure(Dispute _dispute)
		{
			knowledgeBase = _dispute.sharedKnowledgeBase;
			Term subject = _dispute.subject;
			Rule subjectrule = knowledgeBase.rules.Find(t => t.consequent.Convert() == subject.Convert());
			//rows[0][0] = 0;

			AddArgument(subjectrule, 0, Side.Proponent);

			PrintStructure();



			//AddArgument()
			//returns list of attackers


			/*
			//Add Subject Consequent Node
			AddNode(subject, 0, 0);
			Rule subjectrule = knowledgeBase.rules.Find(t => t.consequent.Convert() == subject.Convert());
			int width = subjectrule.antecedents.Count -1;

			//Add Subject Antecedent Nodes
			for (int i = 0; i <= width; i++) 
			{
				AddNode(subjectrule.antecedents[i], i, 2);
			}

			if (width > proponentWidth)
				proponentWidth = width;

			//Add Subject Rule Edge
			AddEdge(subjectrule);


			//Repeat for all other arguments in the dispute by finding all attackers of this argument
			List<Rule> allAttackers = new();
			for (int i = 0; i <= width; i++)
			{
				List<Rule> attackers = knowledgeBase.rules.FindAll(r => r.consequent.Convert() == new Term(subjectrule.antecedents[i].name, false).Convert());
				allAttackers.Concat(attackers);
			}

			return allAttackers;



		}

		private KnowledgeBase knowledgeBase;

		public void AddArgument(Rule _subject, int _currentHeight, Side _currentSide) //Dictionary<List<Rule>, int>
		{
			//Add all rules and its premises to the Nodes & Edges
			//Keep track of all attackers (with corresponding heights) + how wide and deep the argument goes 
			(List<SidedNode> argumentNodes,  Dictionary<Rule, int> attackers, int argumentWidth, int argumentDepth) = 
				AddRule(_subject, _currentHeight, _currentSide, 1, 0);

			//check where this argument fits
			int maxX = 0;
			int localMaxX = 0;
			foreach(SidedNode sidedNode in argumentNodes)
			{
				localMaxX = Math.Max(sidedNode.x, localMaxX);
			}



			//check whether this argument is the widest argument so far
			if (width[(int)currentSide] < maxX)
				width[(int)currentSide] = maxX;


			//Add all attacking arguments to the structure
			foreach (KeyValuePair<Rule, int> attacker in attackers)
			{
				int attackerHeight = attacker.Value;
				int opponentSide = Math.Abs((int)currentSide - 1);

				AddArgument(attacker.Key, attackerHeight, (Side)opponentSide);
			}
		}

		public (List<SidedNode>, Dictionary<Rule, int>, int, int) AddRule(Rule _subject, int _currentHeight, Side _currentSide, int _currentWidth, int _argumentWidth, int _argumentDepth)
		{
			//Add Subject Rule Edge
			AddEdge(_subject);

			//Console.WriteLine(_currentHeight);
			//Add Subject Consequent Node 
			List<SidedNode> argumentNodes = new();
			argumentNodes.Add(new SidedNode(_currentSide, _subject.consequent, 0, _currentHeight));

			//AddNode(_subject.consequent, rows[(int)_currentSide][_currentHeight] + _currentWidth, _currentHeight);



			//Add Subject Antecedent Nodes, or further rules
			int argumentWidth = _subject.antecedents.Count;
			_argumentWidth = Math.Max(argumentWidth, _argumentWidth);


			Dictionary<Rule, int> allAttackers = new();
			for (int i = 0; i < argumentWidth; i++)
			{
				//If a premise is found, add the node
				List<Rule> defenders = knowledgeBase.rules.FindAll(r => r.consequent.Convert() == _subject.antecedents[i].Convert());
				if (defenders.Count == 0)
				{
					argumentNodes.Add(new SidedNode(_currentSide, _subject.antecedents[i], i, _currentHeight + 2));

					//AddNode(_subject.antecedents[i], rows[(int)_currentSide][_currentHeight] + _currentWidth + i, _currentHeight + 2);
				}
				//otherwise, continue searching for rules
				else if (defenders.Count == 1)
				{
					(List<SidedNode> foundNodes, Dictionary<Rule, int> recAttackers, int maxFoundWidth, int maxFoundDept)
						= AddRule(defenders[0], _currentHeight + 2, _currentSide, _argumentWidth,
						_argumentDepth);
					argumentNodes = argumentNodes.Concat(foundNodes).ToList();
					allAttackers = allAttackers.Concat(recAttackers).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
					_argumentWidth = Math.Max(Math.Max(maxFoundWidth, argumentWidth), _argumentWidth);
					_argumentDepth = maxFoundDept;
				}
				else
				{
					Console.WriteLine("Error: defenders cannot extend 1");
				}
			}

			if (height < _currentHeight)
				height = _currentHeight;

			//Repeat for all other arguments in the dispute by finding all attackers of this argument
			//List<Rule> attackers = new();
			for (int i = 0; i < argumentWidth; i++)
			{
				List<Rule> antecedentAttackers = knowledgeBase.rules.FindAll(r => r.consequent.Convert() == new Term(_subject.antecedents[i].name, false).Convert());
				foreach (Rule attacker in antecedentAttackers)
				{
					allAttackers[attacker] = _currentHeight + 4;
				}
			}

			return (argumentNodes, allAttackers, _argumentWidth, _argumentDepth + 1);
		}


		/*
		public void AddArgument(Rule _subject, int _currentWidth, int _currentHeight, Side _currentSide) //Dictionary<List<Rule>, int>
		{
			//Add all rules and its premises to the Nodes & Edges
			//Keep track of all attackers (with corresponding heights) + how wide and deep the argument goes 
			(Dictionary<Rule, int> attackers, int argumentWidth, int argumentDepth) = AddRule(_subject, _currentWidth, _currentHeight, _currentSide, 1, 0); 

			//check whether this argument is the widest argument so far
			if (width[(int)currentSide] < argumentWidth + _currentWidth)
				width[(int)currentSide] = argumentWidth + _currentWidth;

			//For each line that this argument covers, add the width
			for(int y = _currentHeight; y< argumentDepth; y++)
			{
				if (!rows[(int)_currentSide].ContainsKey(y))
					rows[(int)_currentSide][y] = argumentWidth + _currentWidth;
				else
					rows[(int)_currentSide][y] += argumentWidth + _currentWidth;
			}

			//Add all attacking arguments to the structure
			foreach(KeyValuePair<Rule, int> attacker in attackers)
			{
				int attackerHeight = attacker.Value;
				int opponentSide = Math.Abs((int)currentSide - 1);

				if (!rows[opponentSide].ContainsKey(attackerHeight))
				{
					rows[opponentSide][attackerHeight] = 1;
				}

				int attackerWidth = rows[opponentSide][attackerHeight];
				AddArgument(attacker.Key, attackerWidth, attackerHeight, (Side)opponentSide);
			}
		}


		public (Dictionary<Rule,int>, int, int) AddRule(Rule _subject, int _currentWidth, int _currentHeight, Side _currentSide, int _argumentWidth, int _argumentDepth)
		{
			//Console.WriteLine(_currentHeight);
			//Add Subject Consequent Node
			if (!rows[(int)_currentSide].ContainsKey(_currentHeight))
			{
				rows[(int)_currentSide][_currentHeight] = 0;
			}

			Console.WriteLine(rows[(int)_currentSide][_currentHeight]);
			AddNode(_subject.consequent, rows[(int)_currentSide][_currentHeight] + _currentWidth, _currentHeight);

			//Add Subject Rule Edge
			AddEdge(_subject);

			//Add Subject Antecedent Nodes, or further rules
			int argumentWidth = _subject.antecedents.Count;
			_argumentWidth = Math.Max(argumentWidth, _argumentWidth);


			Dictionary<Rule, int> allAttackers = new();
			for (int i = 0; i < argumentWidth; i++)
			{
				//If a premise is found, add the node
				List<Rule> defenders = knowledgeBase.rules.FindAll(r => r.consequent.Convert() == _subject.antecedents[i].Convert());
				if (defenders.Count == 0)
				{
					AddNode(_subject.antecedents[i], rows[(int)_currentSide][_currentHeight] + _currentWidth + i, _currentHeight + 2);
				}
				//otherwise, continue searching for rules
				else if (defenders.Count == 1)
				{
					(Dictionary<Rule, int> recAttackers, int maxFoundWidth, int maxFoundDept) 
						= AddRule(defenders[0], rows[(int)_currentSide][_currentHeight] + _currentWidth + i, _currentHeight + 2, _currentSide, _argumentWidth,
						_argumentDepth);
					_argumentWidth = Math.Max(Math.Max(maxFoundWidth, argumentWidth), _argumentWidth);
					allAttackers = allAttackers.Concat(recAttackers).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
					_argumentDepth = maxFoundDept;
				}
				else
				{
					Console.WriteLine("Error: defenders cannot extend 1");
				}
			}

			if (height < _currentHeight)
				height = _currentHeight;

			//Repeat for all other arguments in the dispute by finding all attackers of this argument

			//List<Rule> attackers = new();
			for (int i = 0; i < argumentWidth; i++)
			{
				List<Rule> antecedentAttackers = knowledgeBase.rules.FindAll(r => r.consequent.Convert() == new Term(_subject.antecedents[i].name, false).Convert());
				foreach (Rule attacker in antecedentAttackers)
				{
					allAttackers[attacker] = _currentHeight + 4;
				}
			}

			return (allAttackers, _argumentWidth, _argumentDepth + 1);
		}/*
		/*
		public void AddArgument(Rule _subject, int _currentWidth, int _currentHeight, Side _currentSide)
		{
			//Add Subject Consequent Node
			AddNode(_subject.consequent, _currentWidth, _currentHeight);

			//Add Subject Antecedent Nodes, or further rules
			int argumentWidth = _subject.antecedents.Count - 1;
			for (int i = 0; i <= argumentWidth; i++)
			{
				rows[(int)currentSide][_currentHeight]++;
				List<Rule> defenders = knowledgeBase.rules.FindAll(r => r.consequent.Convert() == _subject.antecedents[i].Convert());
				if (defenders.Count == 0)
				{
					AddNode(_subject.antecedents[i], i, _currentHeight + 2);
				}
				else
				{
					for (int j = 0; j < defenders.Count; j++)
					{
						AddArgument(defenders[j], i+ j * argumentWidth, _currentHeight + 2, _currentSide);
					}
				}
			}

			if (width[(int)currentSide] < argumentWidth)
				width[(int)currentSide] = argumentWidth;

			if (height < _currentHeight)
				height = _currentHeight;

			//Add Subject Rule Edge
			AddEdge(_subject);

			//Repeat for all other arguments in the dispute by finding all attackers of this argument
			List<Rule> allAttackers = new();
			for (int i = 0; i <= argumentWidth; i++)
			{
				List<Rule> attackers = knowledgeBase.rules.FindAll(r => r.consequent.Convert() == new Term(_subject.antecedents[i].name, false).Convert());
				//Console.WriteLine(attackers.Count);
				allAttackers = allAttackers.Concat(attackers).ToList();
			}

			for(int i = 0; i<allAttackers.Count; i++)
			{
				AddArgument(allAttackers[i],i,_currentHeight+4, (Side)Math.Abs((int)_currentSide - 1));
			}


			//return allAttackers;
		}
		*/

	public SidedNode AddNode(Term _term, int _x, int _y)
    {
		SidedNode n = new SidedNode(side, _term, _x, _y);
		//Console.WriteLine(_term.Convert()+" "+_x +" "+_y);
		nodes.Add(n);
		return n;
	}

	public void AddEdge(Rule _rule, SidedNode _consequent)//, List<SidedNode> _antecedents)
    {
		edges.Add(new SidedEdge(side, _rule, _rule.consequent, _rule.antecedents, _consequent));//, _antecedents));
    }

	public string Convert()
    {
		string id = "";
		foreach (SidedNode n in nodes)
		{
			id += n.term.Convert() + ":";
			id+= n.x + " " + n.y;
		}
		return id;
	}

	public void PrintStructure()
    {
		Console.WriteLine("--------------------------------------------");
		foreach (SidedNode n in nodes)
        {
			Console.Write(n.term.Convert()+":");
			Console.Write(n.x + " "+n.y);
			Console.WriteLine();
		}
		Console.WriteLine("--------------------------------------------");
	}
}



public class SidedNode
{
	public Term term;
	Side side;
	public int x;
	public int y;

	public SidedNode(Side _side, Term _term,  int _x, int _y)
	{
		side = _side;
		term = _term;
		x = _x;
		y = _y;
	}

}

public class SidedEdge
{
	Side side;
	Rule rule;
	public Term consequent;
	public List<Term> antecedents;
	public SidedNode consequentNode;
	//public List<SidedNode> antecedentsNode;

	public SidedEdge(Side _side, Rule _rule, Term _consequent, List<Term> _antecedents, SidedNode _consequentNode)//, List<SidedNode> _antecedentsNode)
	{
		side = _side;
		rule = _rule;
		consequent = _consequent;
		antecedents = _antecedents;
		consequentNode =_consequentNode;
		//antecedentsNode =_antecedentsNode;
	}

}