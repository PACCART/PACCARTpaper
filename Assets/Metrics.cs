using System;
using System.Collections.Generic;
using System.Linq;

public static class Metrics
{
	//CONCEALMENT METRICS
	public static (float, float, float) GetConcealmentMetrics(List<Dispute> disputes, bool print = true)
    {
		float win = GetFullAverageWin(disputes);
		float con = GetFullAverageConcealment(disputes);
		float rat = GetFullWinConcealmentRatio(disputes);
		if (print)
        {
			Console.WriteLine("--------------------");
			Console.WriteLine("CONCEALMENT METRICS:");
			//Console.WriteLine("Average Win: " + GetAllWin(disputes));
			Console.WriteLine("Average Win: " + win);
			Console.WriteLine("Average Concealment: " + con);
			Console.WriteLine("Average Ratio: " + rat);
			Console.WriteLine("--------------------");
		}

		return (win, con, rat);
	}

	public static (float, float, float) GetSidedConcealmentMetrics(List<Dispute> disputes, Side side, bool print = true)
	{
		float win = GetAverageWin(disputes, side);
		float con = GetAverageConcealment(disputes, side);
		float rat = GetWinConcealmentRatio(disputes, side);
		if (print)
		{
			Console.WriteLine("--------------------");
			Console.WriteLine("CONCEALMENT METRICS:");
			//Console.WriteLine("Average Win: " + GetAllWin(disputes));
			Console.WriteLine("Average Win: " + win);
			Console.WriteLine("Average Concealment: " + con);
			Console.WriteLine("Average Ratio: " + rat);
			Console.WriteLine("--------------------");
		}

		return (win, con, rat);
	}

	//Ratio
	public static float GetFullWinConcealmentRatio(List<Dispute> disputes, float factor = 1)
    {
		float a = (GetAverageWin(disputes, Side.Proponent) + GetAverageWin(disputes, Side.Opponent)) / 2;
		float b = (GetAverageConcealment(disputes, Side.Proponent) + GetAverageConcealment(disputes, Side.Opponent)) / 2;
		return (float)(a * Math.Pow(b, factor));
	}
	public static float GetWinConcealmentRatio(List<Dispute> disputes, Side side, float factor = 1)
	{
		if (factor == 1) return GetAverageWin(disputes, side) * GetAverageConcealment(disputes, side);
		return (float)(GetAverageWin(disputes, side) * Math.Pow(GetAverageConcealment(disputes, side), factor));
	}

	//Concealment
	public static float GetFullAverageConcealment(List<Dispute> disputes)
    {
		return (GetAverageConcealment(disputes, Side.Proponent) + GetAverageConcealment(disputes, Side.Opponent)) / 2;
	}

	public static float GetAverageConcealment(List<Dispute> disputes, Side side)
	{
		
		float total = 0;

		foreach (Dispute dispute in disputes)
		{
			total += GetDisputeConcealment(dispute, side);
		}
		//Console.WriteLine(total / disputes.Count);
		return total / disputes.Count;
	}

	public static float GetDisputeConcealment(Dispute dispute, Side side, bool _print = false)
    {
		//float dRc = dispute.sharedKnowledgeBase.rules.Count(r => !r.IsConcealed(dispute, side));
		dispute.agents[(int)side].knowledgeBase.MergeLevels();
		float dRc = dispute.agents[(int)side].knowledgeBase.rules.Count(r => !r.IsRevealed(dispute, side));
		
		//&& dispute.agents[(int)side].knowledgeBase.rules.Count(r2=>r2.Convert() == r.Convert())!=0);
		if (_print) Console.WriteLine("dRc "+dRc);
		//float dPc = dispute.sharedKnowledgeBase.premises.Count(p => p.IsConcealed(dispute, side));
		float dPc = dispute.agents[(int)side].knowledgeBase.premises.Count(p => !p.IsRevealed(dispute, side));

		//&& dispute.agents[(int)side].knowledgeBase.premises.Count(p2 => p2.Convert() == p.Convert()) != 0);
		//Console.WriteLine("test" + dispute.sharedKnowledgeBase.premises.FindAll(p => !p.IsRevealed(dispute, side)).ToList()[0].Convert());
		if (_print) Console.WriteLine("dPc " + dPc);
		float dR = dispute.agents[(int)side].knowledgeBase.rules.Count;// (r => r.IsContent(dispute, side));
		if (_print) Console.WriteLine("dR " + dR);// + " "+ side + " "+ dispute.sharedKnowledgeBase.rules.Find(r => r.IsContent(dispute, side)).Convert());
		float dP = dispute.agents[(int)side].knowledgeBase.premises.Count;//.sharedKnowledgeBase.premises.Count(p => p.IsContent(dispute, side));
		if (_print) Console.WriteLine("dP " + dP);

		if (_print) Console.WriteLine("result " + (dRc + dPc) / (dR + dP));

		return (dRc + dPc) / (dR + dP);

    }

	//TEAMWORK

	public static float GetAverageAgentConcealment(List<Dispute> disputes, Agent agent)
	{
		agent.knowledgeBase.MergeLevels();
		float total = 0;

		foreach (Dispute dispute in disputes)
		{
			total += GetAgentConcealment(dispute, agent);
		}
		//Console.WriteLine(total / disputes.Count);
		return total / disputes.Count;
	}

	public static float GetAgentConcealment(Dispute dispute, Agent agent)
    {
		//Console.WriteLine(agent.knowledgeBase.rules.Count+ " "+ agent.knowledgeBase.premises.Count);

		float dRc = agent.knowledgeBase.rules.Count(r => !r.IsRevealed(dispute, agent.side));
		
		
		float dPc = agent.knowledgeBase.premises.Count(p => !p.IsRevealed(dispute, agent.side));
		Console.WriteLine(dPc);

		float dR = agent.knowledgeBase.rules.Count;// (r => r.IsContent(dispute, side));
		float dP = agent.knowledgeBase.premises.Count;//.sharedKnowledgeBase.premises.Count(p => p.IsContent(dispute, side));
		Console.WriteLine(dP);


		return (dRc + dPc) / (dR + dP);
	}

	public static float GetAverageTeamIndexConcealment(List<Dispute> disputes, Side side, int teamIndex)
	{
		float total = 0;

		foreach (Dispute dispute in disputes)
		{
			total += GetTeamIndexConcealment(dispute, side, teamIndex);
		}
		//Console.WriteLine(total / disputes.Count);
		return total / disputes.Count;
	}

	public static float GetTeamIndexConcealment(Dispute dispute, Side side, int teamIndex)
	{
		Agent agent = dispute.teams[(int)side].agents[teamIndex];

		agent.knowledgeBase.MergeLevels();
		//Console.WriteLine(agent.knowledgeBase.rules.Count+ " "+ agent.knowledgeBase.premises.Count);
		//throw new NullReferenceException();

		float dRc = agent.knowledgeBase.rules.Count(r => !r.IsRevealed(dispute, agent.side));
		//Console.WriteLine("drc " +dRc);

		float dPc = agent.knowledgeBase.premises.Count(p => !p.IsRevealed(dispute, agent.side));
		//Console.WriteLine(dPc);

		float dR = agent.knowledgeBase.rules.Count;// (r => r.IsContent(dispute, side));
		//Console.WriteLine("dr " + dR);

		float dP = agent.knowledgeBase.premises.Count;//.sharedKnowledgeBase.premises.Count(p => p.IsContent(dispute, side));
													  //Console.WriteLine(dP);
		//Console.WriteLine((dRc + dPc) / (dR + dP));

		return (dRc + dPc) / (dR + dP);
	}





	//Win
	public static float GetFullAverageWin(List<Dispute> disputes)
	{
		return (GetAverageWin(disputes, Side.Proponent) + GetAverageWin(disputes, Side.Opponent)) / 2;
	}

	public static float GetAverageWin(List<Dispute> disputes, Side side)
    {
		float total = 0;

		foreach (Dispute dispute in disputes)
		{
			total += GetDisputeWin(dispute, side);
		}

		//Console.WriteLine("Win " + total / disputes.Count);
		return total / disputes.Count;
	}

	public static float GetDisputeWin(Dispute dispute, Side side)
    {
		return dispute.GetWinner() == side ? 1 : 0;
    }



}
