using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

public class ScenarioManager
{
    public ScenarioManager()
    {
        //dictionary of hand crafted disputes here
    }

    public static Dispute LoadScenario(string name)
    {
        switch (name)
        {
            case "1": return LoadScenario1();
            case "2": return LoadScenario2();
            case "3": return LoadScenario3();
            case "4": return LoadScenario4();
            case "5": return LoadScenarioRecord();
            case "arg": return LoadScenarioArgument();
        }

        return CreateExampleDispute();
    }

    public static Dispute LoadScenario1()
    {
        List<Agent> agents = new List<Agent> ();
        Dispute dispute = new Dispute("sp", true);

        Agent Alice = new Agent()
        {
            side = Side.Proponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> {
                    new Term("sp", true),
                    new Term("fo", true),
                    new Term("df", true),
                    new Term("u2", true) },
                rules = new List<Rule> {
                    new Rule(1,new List<Term> (){new Term("fo") }, new Term("or")),
                    new Rule(2, new List<Term> (){new Term("df"), new Term("u2") }, new Term("ub"))
                },
                contrariness = new List<Contrary> {
                new Contrary(
                    new Term("u1", true),
                    new Term("ub", true))},
            }
        };
        agents.Add(Alice);
        Agent David = new Agent()
        {
            side = Side.Opponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> {
                    new Term("is", true),
                    new Term("ha", true),
                    new Term("in", true),
                    new Term("or", false),
                    new Term("ok", true),
                    new Term("ac", false),
                    new Term("u1", true),
                    new Term("ob", true) },
                rules = new List<Rule> {
                    new Rule(3, new List<Term> (){new Term("c"), new Term("is") }, new Term("sp", false)),
                    new Rule(4, new List<Term> (){
                        //new Term("c"),
                        new Term("is"),
                        new Term("ha"),
                        new Term("in"),
                        new Term("ob"),
                        new Term("or", false),
                        new Term("ok") },
                            new Term("c", true)),
                     new Rule(5, new List<Term> (){new Term("u1"), new Term("ac", false) }, new Term("cl")),
                },
                contrariness = new List<Contrary> {
                new Contrary(
                    new Term("cl", true),
                    new Term("fo", true))},
            }
        };
        agents.Add(David);
        dispute.agents = agents;

        return dispute;
    }

    public static Dispute LoadScenario2()
    {
        List<Agent> agents = new List<Agent> ();
        Dispute dispute = new Dispute("sp", true);

        Agent David = new Agent()
        {
            side = Side.Proponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> {
                    new Term("sp", true),
                    new Term("pE", true),
                    new Term("bN", true) },
                rules = new List<Rule> {
                    new Rule(1,new List<Term> (){new Term("pE") }, new Term("pP")), //[r1] pE => pP
                    new Rule(2, new List<Term> (){new Term("pP") }, new Term("pF")), //[r2] pP => pF
                    new Rule(3, new List<Term> (){new Term("bN") },new Term("bF")), //[r3] bN => bF
                    new Rule(4, new List<Term> (){new Term("bF") }, new Term("hF")), //[r4] bF => hF
                    new Rule(5, new List<Term> (){new Term("pF"), new Term("hF") }, new Term("hp"))//[r5] pF, hF => hp"
                },
                contrariness = new List<Contrary> {
                new Contrary(
                    new Term("hp", true),
                    new Term("ap", true))},
            }
        };
        agents.Add(David);
        Agent Bob = new Agent()
        {
            side = Side.Opponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> { new Term("ap") },
                rules = new List<Rule> {
                    new Rule(6, new List<Term> (){new Term("fsp") }, new Term("sp", false)),//[r6] fsp => ~sp
                    new Rule(7, new List<Term> (){new Term("ap") }, new Term("fsp"))//[r2] ap => fsp
                }
            }
        };
        agents.Add(Bob);
        //agents.Reverse();
        dispute.agents = agents;

        return dispute;
    }

    public static Dispute LoadScenario3()
    {
        List<Agent> agents = new List<Agent> ();
        Dispute dispute = new Dispute("sp", true);

        Agent Bob = new Agent()
        {
            side = Side.Proponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> {
                    new Term("sp"),
                    new Term("in"),
                    new Term("mc"),
                    new Term("lo"),
                    new Term("ci"),
                    new Term("dt"),
                    new Term("ti"),
                    new Term("eq") },
                rules = new List<Rule> {
                    new Rule(1, new List<Term> (){
                        new Term("in"),
                        new Term("mc"),
                        new Term("lo"),
                        new Term("ci"),
                        new Term("dt"),
                        new Term("ti"),
                        new Term("eq"),
                    },
                        new Term("co")),
                },
                contrariness = new List<Contrary> {
                new Contrary(
                    new Term("co", true),
                    new Term("fa", true))},
            }
        };
        agents.Add(Bob);

        Agent Alice = new Agent()
        {
            side = Side.Opponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> {
                    new Term("fa"),
                    new Term("in"),
                    new Term("di"),
                    new Term("gt")
                },
                rules = new List<Rule> {
                    new Rule(2, new List<Term> (){
                        new Term("fa"),
                        new Term("in")
                    },
                    new Term("sp",false)),
                    new Rule(3, new List<Term> (){
                        new Term("di"),
                        new Term("gt")
                    },
                    new Term("lo",false)),

                }
            }
        };
        agents.Add(Alice);
        //agents.Reverse();
        dispute.agents = agents;

        return dispute;
    }

    public static Dispute LoadScenario4()
    {
        List<Agent> agents = new List<Agent> ();
        Dispute dispute = new Dispute("sp", true);

        Agent Bob = new Agent()
        {
            side = Side.Proponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> {
                    new Term("sp"),
                    new Term("in"),
                    new Term("me"),
                    new Term("13"),
                    new Term("un"),
                },
                rules = new List<Rule> {
                    new Rule(1, new List<Term> (){
                        new Term("in"),
                        new Term("me"),
                        new Term("13"),
                        new Term("un"),
                    },
                        new Term("pr", false)),
                },
            }
        };
        agents.Add(Bob);

        Agent Carol = new Agent()
        {
            side = Side.Opponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> {
                    new Term("in"),
                    new Term("pr"),
                    new Term("da"),
                    new Term("ev"),
                    new Term("eq"),
                    new Term("t1"),
                    new Term("t2"),
                    new Term("gt"),
                    new Term("lo"),
                    new Term("sa"),
                },
                rules = new List<Rule> {
                    new Rule(2, new List<Term> (){
                        new Term("pr"),
                        new Term("in")
                    },
                    new Term("sp",false)),
                    new Rule(3, new List<Term> (){
                        new Term("da"),
                        new Term("ev"),
                        new Term("eq"),
                        new Term("t1"),
                        new Term("t2"),
                        new Term("gt"),
                        new Term("lo"),
                        new Term("sa"),
                    },
                    new Term("re")),

                },
                contrariness = new List<Contrary> {
                new Contrary(
                    new Term("re", true),
                    new Term("un", true))},
            }
        };
        agents.Add(Carol);
        //agents.Reverse();
        dispute.agents = agents;

        return dispute;
    }

    public static Dispute LoadScenarioRecord()
    {
        List<Agent> agents = new List<Agent> ();
        Dispute dispute = new Dispute("record", true) { disputeID = "record"};

        Agent Alice = new Agent()
        {
            side = Side.Proponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> {
                    new Term("thief"),
                },
                rules = new List<Rule> {
                    new Rule(1, new List<Term> (){
                        new Term("thief"),
                    },
                    new Term("security")),
                    new Rule(2, new List<Term> (){
                        new Term("security"),
                    },
                    new Term("record"))
                },
            }
        };
        agents.Add(Alice);

        Agent Bob = new Agent()
        {
            side = Side.Opponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> {
                    new Term("doctor"),
                    new Term("rich")
                },
                rules = new List<Rule> {
                    new Rule(3, new List<Term> (){
                        new Term("doctor")
                    },
                    new Term("jogging")),

                    new Rule(4, new List<Term> (){
                        new Term("jogging")
                    },
                    new Term("thief",false)),

                    new Rule(5, new List<Term> (){
                        new Term("rich")
                    },
                    new Term("thief",false))

                },
                contrariness = new List<Contrary>
                {
                }
            }
        };
        agents.Add(Bob);
        //agents.Reverse();
        dispute.agents = agents;

        return dispute;
    }

    public static Dispute LoadScenarioArgument()
    {
        List<Agent> agents = new List<Agent> ();
        Dispute dispute = new Dispute("a", true) { disputeID = "a" };

        Agent Alice = new Agent()
        {
            side = Side.Proponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> {
                    new Term("d"),
                    new Term("e"),
                    new Term("f"),
                    new Term("g"),
                    new Term("h"),
                    new Term("i"),
                    new Term("j"),
                },
                rules = new List<Rule> {
                    new Rule(1, new List<Term> (){
                        new Term("b"),
                        new Term("c")
                    },
                    new Term("a")),
                    new Rule(2, new List<Term> (){
                        new Term("d"),
                        new Term("e"),
                        new Term("f"),
                    },
                    new Term("b")),
                    new Rule(3, new List<Term> (){
                        new Term("g"),
                        new Term("h"),
                        new Term("i"),
                        new Term("j"),
                    },
                    new Term("c")),
                },
            }
        };
         
        agents.Add(Alice);

        Agent Bob = new Agent()
        {
            side = Side.Opponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term>
                {
                    new Term("unuseful")
                }
            }
        };
        agents.Add(Bob);


        dispute.agents = agents;
        //dispute.sharedKnowledgeBase = Alice.knowledgeBase;

        return dispute;
    }

    public static Dispute LoadScenarioArgument2()
    {
        List<Agent> agents = new List<Agent> ();
        Dispute dispute = new Dispute("a", true) { disputeID = "a" };

        Agent Alice = new Agent()
        {
            side = Side.Proponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> {
                    new Term("k"),
                    new Term("l"),
                    new Term("m"),
                    new Term("n"),
                    new Term("o"),
                    new Term("h"),
                    new Term("i"),
                    new Term("j"),
                    new Term("f"),
                },
                rules = new List<Rule> {
                    new Rule(1, new List<Term> (){
                        new Term("b"),
                        new Term("c")
                    },
                    new Term("a")),
                    new Rule(2, new List<Term> (){
                        new Term("d"),
                        new Term("e"),
                        new Term("f"),
                    },
                    new Term("b")),
                    new Rule(3, new List<Term> (){
                        new Term("o"),
                    },
                    new Term("c")),
                    new Rule(4, new List<Term> (){
                        new Term("g"),
                    },
                    new Term("d")),
                    new Rule(5, new List<Term> (){
                        new Term("h"),
                        new Term("i"),
                        new Term("j"),
                    },
                    new Term("e")),
                    new Rule(6, new List<Term> (){
                        new Term("k"),
                        new Term("l"),
                        new Term("m"),
                        new Term("n"),
                    },
                    new Term("g")),
                },
            }
        };
        agents.Add(Alice);

        Agent Bob = new Agent()
        {
            side = Side.Opponent,
            knowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term>
                {
                    new Term("unuseful")
                }
            }
        };
        agents.Add(Bob);

        dispute.agents = agents;
        //dispute.sharedKnowledgeBase = Alice.knowledgeBase;

        return dispute;
    }



    public static Dispute CreateExampleDispute()
    {
        //Create Example Dispute
        Dispute dispute = new Dispute()
        {
            sharedKnowledgeBase = new KnowledgeBase()
            {
                premises = new List<Term> { new Term("p"), new Term("q") },
                kbPrefs = new string[] { "p < q" },
                rules = new List<Rule> {
                    new Rule(1, new List<Term> (){new Term ("p") }, new Term ("s")),//"[r1] p => s"
                    new Rule(2, new List<Term> (){new Term ("q") }, new Term ("t")) },//"[r2] q => t" 
                rulePrefs = new string[] { "[r1] < [r2]" },
                contrariness = new List<Contrary> { new Contrary(new Term("s"), new Term("t")) },
            },
            link = Link.Last
        };
        return dispute;
    }
}


