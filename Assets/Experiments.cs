using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;

public static class Experiments
{
    ////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////CONCEALMENT EXPERIMENTS///////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////



    public static void RunConcealmentExperiments()
    {
        DisputeGenerator disputeGenerator = new DisputeGenerator();
        Environment environment = new Environment();
        Console.WriteLine("Generating Disputes");
        GenerateDisputes();
        Console.WriteLine("Running Disputes Experiment 1");
        RunConcealmentExperiment1();
        Console.WriteLine("Running Disputes Experiment 2");
        RunConcealmentExperiment2();
        return;
    }


    public static void GenerateDisputes()
    {
        List<Enum> defaults = new List<Enum>() { MaxArgumentSize.Long, MaxDisputeSize.Medium, MaxBranches.Some};
        DisputeGenerator disputeGenerator = new DisputeGenerator();
        List<Dispute> disputes;

        foreach (MaxArgumentSize maxArgumentSize in Enum.GetValues(typeof(MaxArgumentSize)))
        {
            disputes = disputeGenerator.GenerateDisputes(200, maxArgumentSize, (MaxDisputeSize)defaults[1], (MaxBranches)defaults[2]);
            JsonSerialization.SaveDisputes(disputes, "ConcealmentData\\" + maxArgumentSize.ToString() + defaults[1].ToString() + defaults[2].ToString());
        }

        foreach (MaxDisputeSize maxDisputeSize in Enum.GetValues(typeof(MaxDisputeSize)))
        {
            disputes = disputeGenerator.GenerateDisputes(200, (MaxArgumentSize)defaults[0], maxDisputeSize, (MaxBranches)defaults[2]);
            JsonSerialization.SaveDisputes(disputes, "ConcealmentData\\" + defaults[0].ToString() + maxDisputeSize.ToString() + defaults[2].ToString());
        }

        foreach (MaxBranches maxBranches in Enum.GetValues(typeof(MaxBranches)))
        {
            disputes = disputeGenerator.GenerateDisputes(200, (MaxArgumentSize)defaults[0], (MaxDisputeSize)defaults[1], maxBranches);
            JsonSerialization.SaveDisputes(disputes, "ConcealmentData\\" + defaults[0].ToString() + defaults[1].ToString() + maxBranches.ToString());
        }
    }

    ////////////////////////////
    //CONCEALMENT EXPERIMENT 1//
    ////////////////////////////
    public static void RunConcealmentExperiment1()
    {
        CSVSerialisation csv = new CSVSerialisation("Data\\ConcealmentData\\ConcealmentExperiment1");

        List<Enum> defaults = new List<Enum>() { MaxArgumentSize.Long, MaxDisputeSize.Medium, MaxBranches.Some };

        float pwins, pcons, owins, ocons, avgwins, avgcons;
        string path;

        foreach (MaxArgumentSize maxArgumentSize in Enum.GetValues(typeof(MaxArgumentSize)))
        {
            path = "ConcealmentData\\"+maxArgumentSize.ToString() + defaults[1].ToString() + defaults[2].ToString();
            foreach (ConcealmentModule.ArgumentType argumentType in Enum.GetValues(typeof(ConcealmentModule.ArgumentType)))
            {
                
                (pwins, pcons, owins, ocons, avgwins, avgcons) = RunAgainstSameArgumentType(path, argumentType);
                csv.WriteData(argumentType.ToString(), maxArgumentSize.ToString(), defaults[1].ToString(),
                                defaults[2].ToString(),
                                pwins.ToString(), pcons.ToString(),
                                owins.ToString(), ocons.ToString(),
                                avgwins.ToString(), avgcons.ToString());
            }
        }

        foreach (MaxDisputeSize maxDisputeSize in Enum.GetValues(typeof(MaxDisputeSize)))
        {
            path = "ConcealmentData\\" + defaults[0].ToString() + maxDisputeSize.ToString() + defaults[2].ToString();
            foreach (ConcealmentModule.ArgumentType argumentType in Enum.GetValues(typeof(ConcealmentModule.ArgumentType)))
            {
                (pwins, pcons, owins, ocons, avgwins, avgcons) = RunAgainstSameArgumentType(path, argumentType);
                csv.WriteData(argumentType.ToString(), defaults[0].ToString(), maxDisputeSize.ToString(),
                    defaults[2].ToString(),
                                pwins.ToString(), pcons.ToString(),
                                owins.ToString(), ocons.ToString(),
                                avgwins.ToString(), avgcons.ToString());
            }
        }

        foreach (MaxBranches maxBranches in Enum.GetValues(typeof(MaxBranches)))
        {
            path = "ConcealmentData\\" + defaults[0].ToString() + defaults[1].ToString() + maxBranches.ToString();
            foreach (ConcealmentModule.ArgumentType argumentType in Enum.GetValues(typeof(ConcealmentModule.ArgumentType)))
            {
                (pwins, pcons, owins, ocons, avgwins, avgcons) = RunAgainstSameArgumentType(path, argumentType);
                csv.WriteData(argumentType.ToString(), defaults[0].ToString(), defaults[1].ToString(), maxBranches.ToString(),
                                pwins.ToString(), pcons.ToString(),
                                owins.ToString(), ocons.ToString(),
                                avgwins.ToString(), avgcons.ToString());
            }
        }
    }

    public static(float, float, float, float, float, float) RunAgainstSameArgumentType(string _disputesFilePath,
        ConcealmentModule.ArgumentType _argumentType = ConcealmentModule.ArgumentType.Random, bool _print = false)
    {
        Environment environment = new Environment ();
        List<float> allwins = new List<float> (), allcons = new List<float> (), allrats = new List<float> ();

        //PROPONENT
        List<Dispute> disputes = JsonSerialization.LoadDisputes(_disputesFilePath);
        Console.WriteLine(disputes.Count);
        environment.InitializeAgents(new List<Agent>() {
                new Agent (Side.Proponent, _argumentType, ConcealmentModule.DividingType.None),
                new Agent (Side.Opponent, _argumentType, ConcealmentModule.DividingType.None) });
        environment.RunDisputes(disputes);
        if (_print) Console.WriteLine("**********************************");
        if (_print) Console.WriteLine("Current Opponent Argument Type: " + _argumentType);
        (float pwin, float pcon, float prat) = Metrics.GetSidedConcealmentMetrics(disputes, Side.Proponent, _print);
        (float owin, float ocon, float orat) = Metrics.GetSidedConcealmentMetrics(disputes, Side.Opponent, _print);
        allwins.Add(pwin); allcons.Add(pcon);
        allwins.Add(owin); allcons.Add(ocon);
        if (_print) Console.WriteLine("**********************************");
        if (_print) Console.WriteLine("Result for Argument Type: " + _argumentType);
        if (_print) Console.WriteLine("Wins: " + allwins.Average());
        if (_print) Console.WriteLine("Concealment: " + allcons.Average());
        if (_print) Console.WriteLine("Ratio: " + allrats.Average());

        return (pwin, pcon, owin, ocon, allwins.Average(), allcons.Average());
    }

    ////////////////////////////
    //CONCEALMENT EXPERIMENT 2//
    ////////////////////////////
    
    static public void RunConcealmentExperiment2()
    {
        CSVSerialisation csv = new CSVSerialisation("Data\\ConcealmentData\\ConcealmentExperiment2");

        List<Enum> defaults = new List<Enum> () { MaxArgumentSize.Long, MaxDisputeSize.Medium, MaxBranches.Some};

        float pwins, pcons, owins, ocons, avgwins, avgcons;
        string path;

        foreach (MaxArgumentSize maxArgumentSize in Enum.GetValues(typeof(MaxArgumentSize)))
        {
            path = "ConcealmentData\\" + maxArgumentSize.ToString() + defaults[1].ToString() + defaults[2].ToString();
            foreach (ConcealmentModule.ArgumentType argumentType in Enum.GetValues(typeof(ConcealmentModule.ArgumentType)))
            {
                (pwins, pcons, owins, ocons, avgwins, avgcons) = RunAgainstAllArgumentTypes(path, argumentType);
                csv.WriteData(argumentType.ToString(), maxArgumentSize.ToString(), defaults[1].ToString(),
                                                defaults[2].ToString(),
                                                pwins.ToString(), pcons.ToString(),
                                                owins.ToString(), ocons.ToString(),
                                                avgwins.ToString(), avgcons.ToString());
            }
        }

        foreach (MaxDisputeSize maxDisputeSize in Enum.GetValues(typeof(MaxDisputeSize)))
        {
            path = "ConcealmentData\\" + defaults[0].ToString() + maxDisputeSize.ToString() + defaults[2].ToString();
            foreach (ConcealmentModule.ArgumentType argumentType in Enum.GetValues(typeof(ConcealmentModule.ArgumentType)))
            {
                (pwins, pcons, owins, ocons, avgwins, avgcons) = RunAgainstAllArgumentTypes(path, argumentType);
                csv.WriteData(argumentType.ToString(), defaults[0].ToString(), maxDisputeSize.ToString(),
                    defaults[2].ToString(),
                                pwins.ToString(), pcons.ToString(),
                                owins.ToString(), ocons.ToString(),
                                avgwins.ToString(), avgcons.ToString());
            }
        }

        foreach (MaxBranches maxBranches in Enum.GetValues(typeof(MaxBranches)))
        {
            path = "ConcealmentData\\" + defaults[0].ToString() + defaults[1].ToString() + maxBranches.ToString();
            foreach (ConcealmentModule.ArgumentType argumentType in Enum.GetValues(typeof(ConcealmentModule.ArgumentType)))
            {
                (pwins, pcons, owins, ocons, avgwins, avgcons) = RunAgainstAllArgumentTypes(path, argumentType);
                csv.WriteData(argumentType.ToString(), defaults[0].ToString(), defaults[1].ToString(), maxBranches.ToString(),
                                pwins.ToString(), pcons.ToString(),
                                owins.ToString(), ocons.ToString(),
                                avgwins.ToString(), avgcons.ToString());
            }
        }

    }

    static public (float, float, float, float, float, float) RunAgainstAllArgumentTypes(string _disputesFilePath,
        ConcealmentModule.ArgumentType _argumentType = ConcealmentModule.ArgumentType.Random,
        bool _print = false, Side _side = Side.Proponent)
    {
        List<ConcealmentModule.ArgumentType> argumentTypes = new List<ConcealmentModule.ArgumentType> ()
        {
            ConcealmentModule.ArgumentType.All,
            ConcealmentModule.ArgumentType.Longest,
            ConcealmentModule.ArgumentType.Random,
            ConcealmentModule.ArgumentType.Shortest
        };
        Environment environment = new Environment ();
        List<float> allwins = new List<float> (), allcons = new List<float> (), allrats = new List<float> ();

        //PROPONENT
        List<float> pwins = new List<float> (), pcons = new List<float> (), prats = new List<float> ();
        for (int i = 0; i < argumentTypes.Count; i++)
        {
            List<Dispute> disputes = JsonSerialization.LoadDisputes(_disputesFilePath);
            environment.InitializeAgents(new List<Agent>() {
                new Agent (Side.Proponent, _argumentType),
                new Agent (Side.Opponent, argumentTypes[i]) });
            environment.RunDisputes(disputes);
            if (_print) Console.WriteLine("**********************************");
            if (_print) Console.WriteLine("Current Opponent Argument Type: " + argumentTypes[i]);
            (float pwin, float pcon, float prat) = Metrics.GetSidedConcealmentMetrics(disputes, _side, _print);
            pwins.Add(pwin); pcons.Add(pcon); prats.Add(prat);
            allwins.Add(pwin); allcons.Add(pcon); allrats.Add(prat);
            if (_print) Console.WriteLine("**********************************");
        }

        if (_print) Console.WriteLine("Result for Argument Type: " + _argumentType + " as " + Side.Proponent);
        if (_print) Console.WriteLine("Wins: " + pwins.Average());
        if (_print) Console.WriteLine("Concealment: " + pcons.Average());
        if (_print) Console.WriteLine("Ratio: " + prats.Average());

        //OPPONENT
        List<float> owins = new List<float> (), ocons = new List<float> (), orats = new List<float> ();
        for (int i = 0; i < argumentTypes.Count; i++)
        {
            List<Dispute> disputes = JsonSerialization.LoadDisputes(_disputesFilePath);
            environment.InitializeAgents(new List<Agent>() {
                new Agent (Side.Proponent, argumentTypes[i]),
                new Agent (Side.Opponent, _argumentType) });
            environment.RunDisputes(disputes);
            if (_print) Console.WriteLine("**********************************");
            if (_print) Console.WriteLine("Current Opponent Argument Type: " + argumentTypes[i]);
            (float owin, float ocon, float orat) = Metrics.GetSidedConcealmentMetrics(disputes, _side.GetOppositeSide(), _print);
            owins.Add(owin); ocons.Add(ocon); orats.Add(orat);
            allwins.Add(owin); allcons.Add(ocon); allrats.Add(orat);
            if (_print) Console.WriteLine("**********************************");
        }

        if (_print) Console.WriteLine("Result for Argument Type: " + _argumentType + " as " + Side.Opponent);
        if (_print) Console.WriteLine("Wins: " + owins.Average());
        if (_print) Console.WriteLine("Concealment: " + ocons.Average());
        //if (_print) Console.WriteLine("Ratio: " + rats.Average());

        Console.WriteLine("##################################");
        Console.WriteLine("FINAL total Result for Argument Type: " + _argumentType);
        Console.WriteLine("Wins: " + allwins.Average());
        Console.WriteLine("Concealment: " + allcons.Average());
        Console.WriteLine("##################################");
        return (pwins.Average(), pcons.Average(), owins.Average(), ocons.Average(), allwins.Average(), allcons.Average());
    }


 

    static public void AggregateDisputes()
    {
        List<Dispute> disputes = new List<Dispute> ();
        List<string> datasets = new List<string> ()
        {
            "ExtremeMediumSome",
            "LongLargeSome",
            "LongMediumSome",
            "LongMediumMany",
            "LongMediumSingle",
            "LongSmallSome",
            "ShortMediumSome"
        };
        foreach(string dataset in datasets)
        {
            disputes = disputes.Concat(JsonSerialization.LoadDisputes("ConcealmentData\\" + dataset)).ToList();
        }
        JsonSerialization.SaveDisputes(disputes, "ConcealmentData\\Aggregated");
        
        
    }
    static public void AggregateDisputesLight()
    {
        List<Dispute> disputes = new List<Dispute> ();
        List<string> datasets = new List<string> ()
        {
            //"ExtremeMediumSome",
            "LongLargeSome",
            "LongMediumSome",
            "LongMediumMany",
            "LongMediumSingle",
            "LongSmallSome",
            "ShortMediumSome"
        };
        foreach (string dataset in datasets)
        {
            disputes = disputes.Concat(JsonSerialization.LoadDisputes("ConcealmentData\\" + dataset)).ToList();
        }
        JsonSerialization.SaveDisputes(disputes, "ConcealmentData\\AggregatedLight");


    }

    static public void RunConcealmentExperiment3()
    {
        CSVSerialisation csv = new CSVSerialisation("Data\\ConcealmentData\\ConcealmentExperiment3");

        string path = "ConcealmentData\\LongMediumSome";
        float pwins, pcons, owins, ocons, avgwins, avgcons;

        foreach (ConcealmentModule.ArgumentType argumentType in Enum.GetValues(typeof(ConcealmentModule.ArgumentType)))
        {
            foreach (ConcealmentModule.DividingType dividingType in Enum.GetValues(typeof(ConcealmentModule.DividingType)))
            {
                foreach (ConcealmentModule.DroppingType droppingType in Enum.GetValues(typeof(ConcealmentModule.DroppingType))) 
                {
                    (pwins, pcons, owins, ocons, avgwins, avgcons) = RunAgainstAllAgentTypes(path, argumentType, dividingType, droppingType);
                    csv.WriteData(argumentType.ToString(),
                                  dividingType.ToString(),
                                  droppingType.ToString(),
                                pwins.ToString(), pcons.ToString(),
                                owins.ToString(), ocons.ToString(),
                                avgwins.ToString(), avgcons.ToString());
                }
            }
        }
            

    }


    static public (float, float, float, float, float, float) RunAgainstAllAgentTypes(string _disputesFilePath,
        ConcealmentModule.ArgumentType _argumentType = ConcealmentModule.ArgumentType.Random,
        ConcealmentModule.DividingType _dividingType = ConcealmentModule.DividingType.None,
        ConcealmentModule.DroppingType _droppingType = ConcealmentModule.DroppingType.Always,
        bool _print = false, Side _side = Side.Proponent)
    {
        Environment environment = new Environment ();
        List<float> allwins = new List<float> (), allcons = new List<float> (), allrats = new List<float> ();

        //PROPONENT
        List<float> pwins = new List<float> (), pcons = new List<float> (), prats = new List<float> ();
        foreach (ConcealmentModule.ArgumentType argumentType in Enum.GetValues(typeof(ConcealmentModule.ArgumentType)))
        {
            foreach (ConcealmentModule.DividingType dividingType in Enum.GetValues(typeof(ConcealmentModule.DividingType)))
            {
                foreach (ConcealmentModule.DroppingType droppingType in Enum.GetValues(typeof(ConcealmentModule.DroppingType)))
                {
                    List<Dispute> disputes = JsonSerialization.LoadDisputes(_disputesFilePath);
                    environment.InitializeAgents(new List<Agent>() {
                    new Agent (Side.Proponent, _argumentType,_dividingType,_droppingType),
                    new Agent (Side.Opponent, argumentType, dividingType, droppingType) });
                    environment.RunDisputes(disputes);
                    if (_print) Console.WriteLine("**********************************");
                    if (_print) Console.WriteLine("Current Opponent Agent Type: {0} {1} {2}", argumentType, dividingType, droppingType);
                    (float pwin, float pcon, float prat) = Metrics.GetSidedConcealmentMetrics(disputes, _side, _print);
                    pwins.Add(pwin); pcons.Add(pcon); prats.Add(prat);
                    allwins.Add(pwin); allcons.Add(pcon); allrats.Add(prat);
                    if (_print) Console.WriteLine("**********************************");
                }
            }
        }

        if (_print) Console.WriteLine("Result for Argument Type: {0} {1} {2} as {3}", _argumentType, _dividingType, _droppingType, Side.Proponent);
        if (_print) Console.WriteLine("Wins: " + pwins.Average());
        if (_print) Console.WriteLine("Concealment: " + pcons.Average());

        //OPPONENT
        List<float> owins = new List<float> (), ocons = new List<float> (), orats = new List<float> ();
        foreach (ConcealmentModule.ArgumentType argumentType in Enum.GetValues(typeof(ConcealmentModule.ArgumentType)))
        {
            foreach (ConcealmentModule.DividingType dividingType in Enum.GetValues(typeof(ConcealmentModule.DividingType)))
            {
                foreach (ConcealmentModule.DroppingType droppingType in Enum.GetValues(typeof(ConcealmentModule.DroppingType)))
                {
                    List<Dispute> disputes = JsonSerialization.LoadDisputes(_disputesFilePath);
                    environment.InitializeAgents(new List<Agent>() {
                new Agent (Side.Proponent, argumentType, dividingType, droppingType),
                new Agent (Side.Opponent, _argumentType, _dividingType, _droppingType) });
                    environment.RunDisputes(disputes);
                    if (_print) Console.WriteLine("**********************************");
                    if (_print) Console.WriteLine("Current Opponent Agent Type: {0} {1} {2}", argumentType, dividingType, droppingType);
                    (float owin, float ocon, float orat) = Metrics.GetSidedConcealmentMetrics(disputes, _side.GetOppositeSide(), _print);
                    owins.Add(owin); ocons.Add(ocon); orats.Add(orat);
                    allwins.Add(owin); allcons.Add(ocon); allrats.Add(orat);
                    if (_print) Console.WriteLine("**********************************");
                }
            }
        }

        if (_print) Console.WriteLine("Result for Agent Type: {0} {1} {2} as {3}", _argumentType, _dividingType, _droppingType, Side.Opponent);
        if (_print) Console.WriteLine("Wins: " + owins.Average());
        if (_print) Console.WriteLine("Concealment: " + ocons.Average());
        //if (_print) Console.WriteLine("Ratio: " + rats.Average());

        Console.WriteLine("##################################");
        Console.WriteLine("FINAL total Result for Agent Type: {0} {1} {2}", _argumentType, _dividingType, _droppingType);
        Console.WriteLine("Wins: " + allwins.Average());
        Console.WriteLine("Concealment: " + allcons.Average());
        Console.WriteLine("##################################");
        return (pwins.Average(), pcons.Average(), owins.Average(), ocons.Average(), allwins.Average(), allcons.Average());
    }




    ////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////EQUITY EXPERIMENTS///////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////

    static public void RunEquityExperiment1()
    {
        CSVSerialisation csv = new CSVSerialisation("Data\\EquityData\\EquityExperiment1");

        string path = "ConcealmentData\\LongMediumSome";
        float pwins, pcons, owins, ocons, avgwins, avgcons;

        List<EquityModule.User> usersp = new List<EquityModule.User> ()
        {
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Default),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Amateur),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Fundamentalist),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.LazyExpert),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.MarginallyConcerned),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Technician),
        };
        List<EquityModule.User> userso = new List<EquityModule.User> ()
        {
            new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.Amateur),
            new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.Fundamentalist),
            new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.LazyExpert),
            new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.MarginallyConcerned),
            new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.Technician),
        };


        foreach (EquityModule.User p in usersp)
        {
            foreach (EquityModule.User o in userso)
            {
                
                (pwins, pcons, owins, ocons, avgwins, avgcons) = RunAgainstUserType(path, p, o);
                csv.WriteData(p.privacyType.ToString(),
                    o.privacyType.ToString(),
                                    pwins.ToString(), pcons.ToString(),
                                    owins.ToString(), ocons.ToString(),
                                    avgwins.ToString(), avgcons.ToString());
            }  
        }
    }

    static public (float, float, float, float, float, float) RunAgainstUserType(string _disputesFilePath, EquityModule.User _p, EquityModule.User _o, bool _print = false)
    {
        Environment environment = new Environment (Module.On, Module.On, Module.Off, Module.Off);
        List<float> allwins = new List<float> (), allcons = new List<float> (), allrats = new List<float> ();
        Side _side = Side.Proponent;

        //PROPONENT
        List<float> pwins = new List<float> (), pcons = new List<float> (), prats = new List<float> ();
        List<Dispute> disputes = JsonSerialization.LoadDisputes(_disputesFilePath);
            environment.InitializeAgents(new List<EquityModule.User>() {
                    _p,
                    _o});

            environment.RunDisputes(disputes);
            if (_print) Console.WriteLine("**********************************");
            //if (_print) Console.WriteLine("Current Opponent Agent Type: {0} {1} {2}", argumentType, dividingType, droppingType);
            (float pwin, float pcon, float prat) = Metrics.GetSidedConcealmentMetrics(disputes, _side, _print);
            pwins.Add(pwin); pcons.Add(pcon); prats.Add(prat);
            allwins.Add(pwin); allcons.Add(pcon); allrats.Add(prat);
            if (_print) Console.WriteLine("**********************************");


        //if (_print) Console.WriteLine("Result for Argument Type: {0} {1} {2} as {3}", _argumentType, _dividingType, _droppingType, Side.Proponent);
        if (_print) Console.WriteLine("Wins: " + pwins.Average());
        if (_print) Console.WriteLine("Concealment: " + pcons.Average());

        //OPPONENT
        List<float> owins = new List<float> (), ocons = new List<float> (), orats = new List<float> ();

        
            disputes = JsonSerialization.LoadDisputes(_disputesFilePath);
            environment.InitializeAgents(new List<EquityModule.User>() {
                    _o,
                    _p});

            environment.RunDisputes(disputes);
            if (_print) Console.WriteLine("**********************************");
            //if (_print) Console.WriteLine("Current Opponent Agent Type: {0} {1} {2}", argumentType, dividingType, droppingType);
            (float owin, float ocon, float orat) = Metrics.GetSidedConcealmentMetrics(disputes, _side.GetOppositeSide(), _print);
            owins.Add(owin); ocons.Add(ocon); orats.Add(orat);
            allwins.Add(owin); allcons.Add(ocon); allrats.Add(orat);
            if (_print) Console.WriteLine("**********************************");

        

        //if (_print) Console.WriteLine("Result for Agent Type: {0} {1} {2} as {3}", _argumentType, _dividingType, _droppingType, Side.Opponent);
        if (_print) Console.WriteLine("Wins: " + owins.Average());
        if (_print) Console.WriteLine("Concealment: " + ocons.Average());
        //if (_print) Console.WriteLine("Ratio: " + rats.Average());

        Console.WriteLine("##################################");
        //Console.WriteLine("FINAL total Result for Agent Type: {0} {1} {2}", _argumentType, _dividingType, _droppingType);
        Console.WriteLine("Wins: " + allwins.Average());
        Console.WriteLine("Concealment: " + allcons.Average());
        Console.WriteLine("##################################");
        return (pwins.Average(), pcons.Average(), owins.Average(), ocons.Average(), allwins.Average(), allcons.Average());
    }



    static public void RunEquityExperiment2()
    {
        CSVSerialisation csv = new CSVSerialisation("Data\\EquityData\\EquityExperiment2");

        string path = "ConcealmentData\\LongMediumSome";
        float pwins, pcons, owins, ocons, avgwins, avgcons;

        List<EquityModule.User> usersp = new List<EquityModule.User> ()
        {
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Default),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Amateur),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Fundamentalist),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.LazyExpert),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.MarginallyConcerned),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Technician),
        };
       

        foreach (EquityModule.User u in usersp)
        {

            (pwins, pcons, owins, ocons, avgwins, avgcons) = RunAgainstAllUserTypes(path, u);
            csv.WriteData(u.privacyType.ToString(),
                            
                                pwins.ToString(), pcons.ToString(),
                                owins.ToString(), ocons.ToString(),
                                avgwins.ToString(), avgcons.ToString());
        }




    }


    static public (float, float, float, float, float, float) RunAgainstAllUserTypes(string _disputesFilePath, EquityModule.User _user, bool _print = false)
    {
        Environment environment = new Environment (Module.On, Module.On, Module.Off, Module.Off);
        List<float> allwins = new List<float> (), allcons = new List<float> (), allrats = new List<float> ();


        List<EquityModule.User> userso = new List<EquityModule.User> ()
        {
            new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.Amateur),
            new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.Fundamentalist),
            new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.LazyExpert),
            new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.MarginallyConcerned),
            new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.Technician),
        };
        Side _side = Side.Proponent;

        //PROPONENT
        List<float> pwins = new List<float> (), pcons = new List<float> (), prats = new List<float> ();
        foreach (EquityModule.User opponent in userso)
        {
            List<Dispute> disputes = JsonSerialization.LoadDisputes(_disputesFilePath);
            environment.InitializeAgents(new List<EquityModule.User>() {
                    _user,
                    opponent});

            environment.RunDisputes(disputes);
            if (_print) Console.WriteLine("**********************************");
            //if (_print) Console.WriteLine("Current Opponent Agent Type: {0} {1} {2}", argumentType, dividingType, droppingType);
            (float pwin, float pcon, float prat) = Metrics.GetSidedConcealmentMetrics(disputes, _side, _print);
            pwins.Add(pwin); pcons.Add(pcon); prats.Add(prat);
            allwins.Add(pwin); allcons.Add(pcon); allrats.Add(prat);
            if (_print) Console.WriteLine("**********************************");


        }

        //if (_print) Console.WriteLine("Result for Argument Type: {0} {1} {2} as {3}", _argumentType, _dividingType, _droppingType, Side.Proponent);
        if (_print) Console.WriteLine("Wins: " + pwins.Average());
        if (_print) Console.WriteLine("Concealment: " + pcons.Average());

        //OPPONENT
        List<float> owins = new List<float> (), ocons = new List<float> (), orats = new List<float> ();
        foreach (EquityModule.User opponent in userso)
        {
            List<Dispute> disputes = JsonSerialization.LoadDisputes(_disputesFilePath);
            environment.InitializeAgents(new List<EquityModule.User>() {
                    opponent,
                    _user});

            environment.RunDisputes(disputes);
            if (_print) Console.WriteLine("**********************************");
            //if (_print) Console.WriteLine("Current Opponent Agent Type: {0} {1} {2}", argumentType, dividingType, droppingType);
            (float owin, float ocon, float orat) = Metrics.GetSidedConcealmentMetrics(disputes, _side.GetOppositeSide(), _print);
            owins.Add(owin); ocons.Add(ocon); orats.Add(orat);
            allwins.Add(owin); allcons.Add(ocon); allrats.Add(orat);
            if (_print) Console.WriteLine("**********************************");

        }

        //if (_print) Console.WriteLine("Result for Agent Type: {0} {1} {2} as {3}", _argumentType, _dividingType, _droppingType, Side.Opponent);
        if (_print) Console.WriteLine("Wins: " + owins.Average());
        if (_print) Console.WriteLine("Concealment: " + ocons.Average());
        //if (_print) Console.WriteLine("Ratio: " + rats.Average());

        Console.WriteLine("##################################");
        //Console.WriteLine("FINAL total Result for Agent Type: {0} {1} {2}", _argumentType, _dividingType, _droppingType);
        Console.WriteLine("Wins: " + allwins.Average());
        Console.WriteLine("Concealment: " + allcons.Average());
        Console.WriteLine("##################################");
        return (pwins.Average(), pcons.Average(), owins.Average(), ocons.Average(), allwins.Average(), allcons.Average());
    }



    static public void RunEquityExperiment2b()
    {
        CSVSerialisation csv = new CSVSerialisation("Data\\EquityData\\EquityExperiment2b");

        string path = "ConcealmentData\\LongMediumSome";
        float pwins, pcons, owins, ocons, avgwins, avgcons;

        List<EquityModule.User> usersp = new List<EquityModule.User> ()
        {
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Default),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Amateur),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Fundamentalist),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.LazyExpert),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.MarginallyConcerned),
            new EquityModule.User (Side.Proponent, EquityModule.PrivacyType.Technician),
        };


        foreach (EquityModule.User u in usersp)
        {

            (pwins, pcons, owins, ocons, avgwins, avgcons) = RunAgainstUserDistributedSet(path, u);
            csv.WriteData(u.privacyType.ToString(),

                                pwins.ToString(), pcons.ToString(),
                                owins.ToString(), ocons.ToString(),
                                avgwins.ToString(), avgcons.ToString());
        }
    }


    static public (float, float, float, float, float, float) RunAgainstUserDistributedSet(string _disputesFilePath, EquityModule.User _user, bool _print = false)
    {
        Environment environment = new Environment (Module.On, Module.On, Module.Off, Module.Off);
        List<float> allwins = new List<float> (), allcons = new List<float> (), allrats = new List<float> ();


        List<EquityModule.User> distributedset = new List<EquityModule.User> ();
        for (int i = 0; i < 3; i++)
            distributedset.Add(new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.Fundamentalist));
        for (int i = 0; i < 34; i++)
            distributedset.Add(new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.Amateur));
        for (int i = 0; i < 22; i++)
            distributedset.Add(new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.LazyExpert));
        for (int i = 0; i < 23; i++)
            distributedset.Add(new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.MarginallyConcerned));
        for (int i = 0; i < 18; i++)
            distributedset.Add(new EquityModule.User (Side.Opponent, EquityModule.PrivacyType.Technician));



        Side _side = Side.Proponent;

        //PROPONENT
        List<float> pwins = new List<float> (), pcons = new List<float> (), prats = new List<float> ();
        foreach (EquityModule.User opponent in distributedset)
        {
            List<Dispute> disputes = JsonSerialization.LoadDisputes(_disputesFilePath);
            environment.InitializeAgents(new List<EquityModule.User>() {
                    _user,
                    opponent});

            environment.RunDisputes(disputes);
            if (_print) Console.WriteLine("**********************************");
            //if (_print) Console.WriteLine("Current Opponent Agent Type: {0} {1} {2}", argumentType, dividingType, droppingType);
            (float pwin, float pcon, float prat) = Metrics.GetSidedConcealmentMetrics(disputes, _side, _print);
            pwins.Add(pwin); pcons.Add(pcon); prats.Add(prat);
            allwins.Add(pwin); allcons.Add(pcon); allrats.Add(prat);
            if (_print) Console.WriteLine("**********************************");


        }

        //if (_print) Console.WriteLine("Result for Argument Type: {0} {1} {2} as {3}", _argumentType, _dividingType, _droppingType, Side.Proponent);
        if (_print) Console.WriteLine("Wins: " + pwins.Average());
        if (_print) Console.WriteLine("Concealment: " + pcons.Average());

        //OPPONENT
        List<float> owins = new List<float> (), ocons = new List<float> (), orats = new List<float> ();
        foreach (EquityModule.User opponent in distributedset)
        {
            List<Dispute> disputes = JsonSerialization.LoadDisputes(_disputesFilePath);
            environment.InitializeAgents(new List<EquityModule.User>() {
                    opponent,
                    _user});

            environment.RunDisputes(disputes);
            if (_print) Console.WriteLine("**********************************");
            //if (_print) Console.WriteLine("Current Opponent Agent Type: {0} {1} {2}", argumentType, dividingType, droppingType);
            (float owin, float ocon, float orat) = Metrics.GetSidedConcealmentMetrics(disputes, _side.GetOppositeSide(), _print);
            owins.Add(owin); ocons.Add(ocon); orats.Add(orat);
            allwins.Add(owin); allcons.Add(ocon); allrats.Add(orat);
            if (_print) Console.WriteLine("**********************************");

        }

        //if (_print) Console.WriteLine("Result for Agent Type: {0} {1} {2} as {3}", _argumentType, _dividingType, _droppingType, Side.Opponent);
        if (_print) Console.WriteLine("Wins: " + owins.Average());
        if (_print) Console.WriteLine("Concealment: " + ocons.Average());
        //if (_print) Console.WriteLine("Ratio: " + rats.Average());

        Console.WriteLine("##################################");
        //Console.WriteLine("FINAL total Result for Agent Type: {0} {1} {2}", _argumentType, _dividingType, _droppingType);
        Console.WriteLine("Wins: " + allwins.Average());
        Console.WriteLine("Concealment: " + allcons.Average());
        Console.WriteLine("##################################");
        return (pwins.Average(), pcons.Average(), owins.Average(), ocons.Average(), allwins.Average(), allcons.Average());
    }


    ////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////TEAMWORK EXPERIMENTS/////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////


    static public void RunTeamworkExperiment1TeamOf2()
    {

        CSVSerialisation csv = new CSVSerialisation("Data\\TeamworkData\\TeamworkExperiment1Team2");

        DisputeGenerator disputeGenerator = new DisputeGenerator ();

        string path = "ConcealmentData\\LongMediumSome";
        //float pwins, pcons, owins, ocons, avgwins, avgcons;
        List<TeamworkModule.EffortType> effortTypes = new List<TeamworkModule.EffortType> ()
        { TeamworkModule.EffortType.No, TeamworkModule.EffortType.Minimal, TeamworkModule.EffortType.Shared, TeamworkModule.EffortType.Maximum };

        Environment environment = new Environment (Module.On, Module.On, Module.On, Module.Off);
        List<Agent> agenttypes = new List<Agent> ()
        {
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.All),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Longest),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Random),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllContent, ConcealmentModule.DroppingType.Threshold25),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllArg, ConcealmentModule.DroppingType.Threshold25),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllArg, ConcealmentModule.DroppingType.Threshold50),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllContent, ConcealmentModule.DroppingType.Threshold75),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.HalfArg, ConcealmentModule.DroppingType.Threshold75)

        };


        foreach (TeamworkModule.EffortType effortType in effortTypes)
        {
            foreach (Agent agentType in agenttypes)
            {
                (float wins, float cons, float cons1, float cons2) =
                    RunTeamAgainstAllAgentTypes2(effortType, agentType, agenttypes, path);
                csv.WriteData(
                    effortType.ToString(),
                    agentType.argumentType.ToString(),
                    agentType.dividingType.ToString(),
                    agentType.droppingType.ToString(),
                    wins.ToString(),
                    cons.ToString(),
                    cons1.ToString(),
                    cons2.ToString());
            }
        }
    }

    public static (float, float, float, float) RunTeamAgainstAllAgentTypes2(
        TeamworkModule.EffortType _effortType, Agent _agentType, List<Agent> _agentTypes, string _path)
    {

        DisputeGenerator disputeGenerator = new DisputeGenerator ();
        List<Dispute> disputes = disputeGenerator.GenerateDisputes(200, MaxArgumentSize.Long, MaxDisputeSize.Large, MaxBranches.Some);


        List<float> wins = new List<float> ();
        List<float> cons = new List<float> ();
        List<float> cons1 = new List<float> ();
        List<float> cons2 = new List<float> ();
        List<List<float>> allcons = new List<List<float>> () { cons1, cons2 };

        foreach (Dispute dispute in disputes)
        {
            foreach (Agent opponentType in _agentTypes)
            {
                Environment environment = new Environment (Module.On, Module.On, Module.On, Module.Off);

                Agent p1 = new Agent (Side.Proponent);
                Agent p2 = new Agent (Side.Proponent);
                List<Agent> agents = new List<Agent> () { p1, p2};

                foreach (Agent agent in agents)
                {
                    agent.argumentType = _agentType.argumentType;
                    agent.dividingType = _agentType.dividingType;
                    agent.droppingType = _agentType.droppingType;
                }

                Agent opponent = new Agent (Side.Opponent, opponentType.argumentType, opponentType.dividingType, opponentType.droppingType);

                environment.InitializeAgents(new List<Agent>() { p1, p2, opponent
                }, _effortType, TeamworkModule.Overlap.None);

                List<Dispute> d = new List<Dispute> () { dispute };
                environment.RunDisputes(d);

                wins.Add(Metrics.GetDisputeWin(dispute, Side.Proponent));
                cons.Add(Metrics.GetDisputeConcealment(dispute, Side.Proponent));

                for (int i = 0; i < allcons.Count; i++)
                {
                    allcons[i].Add(Metrics.GetTeamIndexConcealment(dispute, Side.Proponent, i));
                }
            }
        }

        return (wins.Average(), cons.Average(), cons1.Average(), cons2.Average());
    }


    static public void RunTeamworkExperiment1TeamOf3()
    {
        
        CSVSerialisation csv = new CSVSerialisation("Data\\TeamworkData\\TeamworkExperiment1Team3");

        DisputeGenerator disputeGenerator = new DisputeGenerator ();

        string path = "ConcealmentData\\LongMediumSome";
        //float pwins, pcons, owins, ocons, avgwins, avgcons;
        List<TeamworkModule.EffortType> effortTypes = new List<TeamworkModule.EffortType> ()
        { TeamworkModule.EffortType.No, TeamworkModule.EffortType.Minimal, TeamworkModule.EffortType.Shared, TeamworkModule.EffortType.Maximum };

        Environment environment = new Environment (Module.On, Module.On, Module.On, Module.Off);
        List<Agent> agenttypes = new List<Agent> ()
        {
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.All),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Longest),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Random),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllContent, ConcealmentModule.DroppingType.Threshold25),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllArg, ConcealmentModule.DroppingType.Threshold25),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllArg, ConcealmentModule.DroppingType.Threshold50),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllContent, ConcealmentModule.DroppingType.Threshold75),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.HalfArg, ConcealmentModule.DroppingType.Threshold75)

        };

        //Agent p1 = new();
        //Agent p2 = new();
        //Agent p3 = new();
        //List<Agent> agents = new() { p1, p2, p3 };


        foreach (TeamworkModule.EffortType effortType in effortTypes)
        {
            foreach (Agent agentType in agenttypes)
            {
                (float wins, float cons, float cons1, float cons2, float cons3) = 
                    RunTeamAgainstAllAgentTypes3(effortType, agentType, agenttypes, path);
                csv.WriteData(
                    effortType.ToString(),
                    agentType.argumentType.ToString(),
                    agentType.dividingType.ToString(),
                    agentType.droppingType.ToString(),
                    wins.ToString(),
                    cons.ToString(),
                    cons1.ToString(),
                    cons2.ToString(),
                    cons3.ToString());


            }
        }

    }

    public static (float,float,float,float,float) RunTeamAgainstAllAgentTypes3(
        TeamworkModule.EffortType _effortType, Agent _agentType, List<Agent> _agentTypes, string _path)
    {

        DisputeGenerator disputeGenerator = new DisputeGenerator ();
        List<Dispute> disputes = disputeGenerator.GenerateDisputes(200, MaxArgumentSize.Long, MaxDisputeSize.Large, MaxBranches.Some);


        List<float> wins = new List<float> ();
        List<float> cons = new List<float> ();
        List<float> cons1 = new List<float> ();
        List<float> cons2 = new List<float> ();
        List<float> cons3 = new List<float> ();
        //List<float> cons4 = new();
        //List<float> cons5 = new();
        List<List<float>> allcons = new List<List<float>> () { cons1, cons2, cons3 };//, //cons4, cons5 };

        foreach (Dispute dispute in disputes)
        {
            foreach(Agent opponentType in _agentTypes)
            {
                Environment environment = new Environment (Module.On, Module.On, Module.On, Module.Off);

                Agent p1 = new Agent (Side.Proponent);
                Agent p2 = new Agent (Side.Proponent);
                Agent p3 = new Agent (Side.Proponent);
                List<Agent> agents = new List<Agent> () { p1, p2, p3 };

                foreach (Agent agent in agents)
                {
                    agent.argumentType = _agentType.argumentType;
                    agent.dividingType = _agentType.dividingType;
                    agent.droppingType = _agentType.droppingType;
                }

                Agent opponent = new Agent (Side.Opponent, opponentType.argumentType, opponentType.dividingType, opponentType.droppingType);

                environment.InitializeAgents(new List<Agent>() { p1, p2, p3, opponent
                }, _effortType, TeamworkModule.Overlap.None);

                List<Dispute> d = new List<Dispute> () { dispute };
                environment.RunDisputes(d);

                wins.Add(Metrics.GetDisputeWin(dispute, Side.Proponent));
                cons.Add(Metrics.GetDisputeConcealment(dispute, Side.Proponent));

                for (int i = 0; i < allcons.Count; i++)
                {
                    allcons[i].Add(Metrics.GetTeamIndexConcealment(dispute, Side.Proponent, i));
                }
            }
        }

        return (wins.Average(), cons.Average(), cons1.Average(), cons2.Average(), cons3.Average());
    }


    static public void RunTeamworkExperiment1TeamOf4()
    {

        CSVSerialisation csv = new CSVSerialisation("Data\\TeamworkData\\TeamworkExperiment1Team4");

        DisputeGenerator disputeGenerator = new DisputeGenerator ();

        string path = "ConcealmentData\\LongMediumSome";
        //float pwins, pcons, owins, ocons, avgwins, avgcons;
        List<TeamworkModule.EffortType> effortTypes = new List<TeamworkModule.EffortType> ()
        { TeamworkModule.EffortType.No, TeamworkModule.EffortType.Minimal, TeamworkModule.EffortType.Shared, TeamworkModule.EffortType.Maximum };

        Environment environment = new Environment (Module.On, Module.On, Module.On, Module.Off);
        List<Agent> agenttypes = new List<Agent> ()
        {
          // new Agent(Side.Proponent, ConcealmentModule.ArgumentType.All),
           // new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Longest),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Random),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllContent, ConcealmentModule.DroppingType.Threshold25),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllArg, ConcealmentModule.DroppingType.Threshold25),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllArg, ConcealmentModule.DroppingType.Threshold50),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllContent, ConcealmentModule.DroppingType.Threshold75),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.HalfArg, ConcealmentModule.DroppingType.Threshold75)

        };


        foreach (TeamworkModule.EffortType effortType in effortTypes)
        {
            foreach (Agent agentType in agenttypes)
            {
                (float wins, float cons, float cons1, float cons2, float cons3, float cons4) =
                    RunTeamAgainstAllAgentTypes4(effortType, agentType, agenttypes, path);
                csv.WriteData(
                    effortType.ToString(),
                    agentType.argumentType.ToString(),
                    agentType.dividingType.ToString(),
                    agentType.droppingType.ToString(),
                    wins.ToString(),
                    cons.ToString(),
                    cons1.ToString(),
                    cons2.ToString(),
                    cons3.ToString(),
                    cons4.ToString());


            }
        }

    }

    public static (float, float, float, float, float,float) RunTeamAgainstAllAgentTypes4(
        TeamworkModule.EffortType _effortType, Agent _agentType, List<Agent> _agentTypes, string _path)
    {

        DisputeGenerator disputeGenerator = new DisputeGenerator ();
        List<Dispute> disputes = disputeGenerator.GenerateDisputes(200, MaxArgumentSize.Long, MaxDisputeSize.Large, MaxBranches.Some);


        List<float> wins = new List<float> ();
        List<float> cons = new List<float> ();
        List<float> cons1 = new List<float> ();
        List<float> cons2 = new List<float> ();
        List<float> cons3 = new List<float> ();
        List<float> cons4 = new List<float> ();
        //List<float> cons5 = new();
        List<List<float>> allcons = new List<List<float>> () { cons1, cons2, cons3, cons4 };//, cons5 };

        foreach (Dispute dispute in disputes)
        {
            foreach (Agent opponentType in _agentTypes)
            {
                Environment environment = new Environment (Module.On, Module.On, Module.On, Module.Off);

                Agent p1 = new Agent (Side.Proponent);
                Agent p2 = new Agent (Side.Proponent);
                Agent p3 = new Agent (Side.Proponent);
                Agent p4 = new Agent (Side.Proponent);
                List<Agent> agents = new List<Agent> () { p1, p2, p3,p4 };

                foreach (Agent agent in agents)
                {
                    agent.argumentType = _agentType.argumentType;
                    agent.dividingType = _agentType.dividingType;
                    agent.droppingType = _agentType.droppingType;
                }

                Agent opponent = new Agent (Side.Opponent, opponentType.argumentType, opponentType.dividingType, opponentType.droppingType);

                environment.InitializeAgents(new List<Agent>() { p1, p2, p3, p4, opponent
                }, _effortType, TeamworkModule.Overlap.None);

                List<Dispute> d = new List<Dispute> () { dispute };
                environment.RunDisputes(d);

                wins.Add(Metrics.GetDisputeWin(dispute, Side.Proponent));
                cons.Add(Metrics.GetDisputeConcealment(dispute, Side.Proponent));

                for (int i = 0; i < allcons.Count; i++)
                {
                    allcons[i].Add(Metrics.GetTeamIndexConcealment(dispute, Side.Proponent, i));
                }
            }
        }

        return (wins.Average(), cons.Average(), cons1.Average(), cons2.Average(), cons3.Average(), cons4.Average());
    }

    static public void RunTeamworkExperiment2()
    {

        CSVSerialisation csv = new CSVSerialisation("Data\\TeamworkData\\TeamworkExperiment2");

        DisputeGenerator disputeGenerator = new DisputeGenerator ();

        string path = "ConcealmentData\\LongMediumSome";
        //float pwins, pcons, owins, ocons, avgwins, avgcons;
        List<TeamworkModule.EffortType> effortTypes = new List<TeamworkModule.EffortType> ()
        { TeamworkModule.EffortType.No, TeamworkModule.EffortType.Minimal, TeamworkModule.EffortType.Shared, TeamworkModule.EffortType.Maximum };

        Environment environment = new Environment (Module.On, Module.On, Module.On, Module.Off);
        List<Agent> agenttypes = new List<Agent> ()
        {
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.All),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Longest),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Random),
            new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllContent, ConcealmentModule.DroppingType.Threshold25),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllArg, ConcealmentModule.DroppingType.Threshold25),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllArg, ConcealmentModule.DroppingType.Threshold50),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.AllContent, ConcealmentModule.DroppingType.Threshold75),
            //new Agent(Side.Proponent, ConcealmentModule.ArgumentType.Shortest, ConcealmentModule.DividingType.HalfArg, ConcealmentModule.DroppingType.Threshold75)

        };


        foreach (TeamworkModule.EffortType effortType in effortTypes)
        {
            foreach (Agent agentType in agenttypes)
            {
                (float wins, float cons, float cons1, float cons2, float cons3) =
                    RunTeamworkExperiment2(effortType, agentType, agenttypes, path);
                csv.WriteData(
                    effortType.ToString(),
                    agentType.argumentType.ToString(),
                    agentType.dividingType.ToString(),
                    agentType.droppingType.ToString(),
                    wins.ToString(),
                    cons.ToString(),
                    cons1.ToString(),
                    cons2.ToString(),
                    cons3.ToString());


            }
        }

    }

    public static (float, float, float, float, float) RunTeamworkExperiment2(
        TeamworkModule.EffortType _effortType, Agent _agentType, List<Agent> _agentTypes, string _path)
    {

        DisputeGenerator disputeGenerator = new DisputeGenerator ();
        List<Dispute> disputes = disputeGenerator.GenerateDisputes(200, MaxArgumentSize.Long, MaxDisputeSize.Large, MaxBranches.Some);


        List<float> wins = new List<float> ();
        List<float> cons = new List<float> ();
        List<float> cons1 = new List<float> ();
        List<float> cons2 = new List<float> ();
        List<float> cons3 = new List<float> ();
        //List<float> cons4 = new();
        //List<float> cons5 = new();
        List<List<float>> allcons = new List<List<float>> () { cons1, cons2, cons3 };//, //cons4, cons5 };

        foreach (Dispute dispute in disputes)
        {
            foreach (Agent opponentType in _agentTypes)
            {
                Environment environment = new Environment (Module.On, Module.On, Module.On, Module.Off);

                Agent p1 = new Agent (Side.Proponent);
                Agent p2 = new Agent (Side.Proponent);
                Agent p3 = new Agent (Side.Proponent);
                List<Agent> agents = new List<Agent> () { p1, p2, p3 };

                foreach (Agent agent in agents)
                {
                    agent.argumentType = _agentType.argumentType;
                    agent.dividingType = _agentType.dividingType;
                    agent.droppingType = _agentType.droppingType;
                }

                Agent opponent = new Agent (Side.Opponent, opponentType.argumentType, opponentType.dividingType, opponentType.droppingType);

                environment.InitializeAgents(new List<Agent>() { p1, p2, p3, opponent
                }, _effortType, TeamworkModule.Overlap.Full);

                List<Dispute> d = new List<Dispute> () { dispute };
                environment.RunDisputes(d);

                wins.Add(Metrics.GetDisputeWin(dispute, Side.Proponent));
                cons.Add(Metrics.GetDisputeConcealment(dispute, Side.Proponent));

                for (int i = 0; i < allcons.Count; i++)
                {
                    allcons[i].Add(Metrics.GetTeamIndexConcealment(dispute, Side.Proponent, i));
                }
            }
        }

        return (wins.Average(), cons.Average(), cons1.Average(), cons2.Average(), cons3.Average());
    }


    ////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////UNDERSTANDABILITY EXPERIMENTS////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////

    public static void RunUnderstandabilityExperiment1a()
    {
        Environment environment = new Environment (Module.On, Module.On, Module.Off, Module.On);
        
        DisputeGenerator disputeGenerator = new DisputeGenerator ();
        List<Dispute> disputes = disputeGenerator.GenerateDisputes(200, MaxArgumentSize.Long, MaxDisputeSize.Medium, MaxBranches.Some);
        environment.InitializeAgents(new List<Agent>() { new Agent(Side.Proponent), new Agent(Side.Opponent) });
        environment.RunDisputes(disputes);
        string feedback = UnderstandabilityModule.GiveFeedback(disputes, Side.Proponent,UnderstandabilityModule.FeedbackType.Summarized);
        Console.WriteLine(feedback);
    }

    public static void RunUnderstandabilityExperiment1b()
    {
        Environment environment = new Environment (Module.On, Module.On, Module.Off, Module.On);

        DisputeGenerator disputeGenerator = new DisputeGenerator ();
        List<Dispute> disputes = disputeGenerator.GenerateDisputes(2, MaxArgumentSize.Long, MaxDisputeSize.Medium, MaxBranches.Some);
        environment.InitializeAgents(new List<EquityModule.User>() { new EquityModule.User(Side.Proponent,EquityModule.PrivacyType.Amateur), 
            new EquityModule.User(Side.Opponent,EquityModule.PrivacyType.Amateur) });
        environment.RunDisputes(disputes);
        string feedback = UnderstandabilityModule.GiveFeedback(disputes, Side.Proponent, UnderstandabilityModule.FeedbackType.Extensive);
        //disputes[0].Print();
        Metrics.GetConcealmentMetrics(environment.disputes);
        Console.WriteLine(feedback);
    }

    public static void RunUnderstandabilityExperiment1c()
    {
        Environment environment = new Environment (Module.On, Module.On, Module.Off, Module.On);

        DisputeGenerator disputeGenerator = new DisputeGenerator ();
        List<Dispute> disputes = disputeGenerator.GenerateDisputes(2, MaxArgumentSize.Long, MaxDisputeSize.Medium, MaxBranches.Some);
        environment.InitializeAgents(new List<EquityModule.User>() { new EquityModule.User(Side.Proponent,EquityModule.PrivacyType.Fundamentalist),
            new EquityModule.User(Side.Opponent,EquityModule.PrivacyType.Fundamentalist) });
        environment.RunDisputes(disputes);
        string feedback = UnderstandabilityModule.GiveFeedback(disputes, Side.Proponent, UnderstandabilityModule.FeedbackType.Advisory);
        Console.WriteLine(feedback);
    }

    public static void RunUnderstandabilityExperiment2()
    {
            Environment environment = new Environment (Module.On, Module.On, Module.Off, Module.On);
            environment.InitializeAgents(new List<Agent>() {
            new Agent (Side.Proponent,ConcealmentModule.ArgumentType.All),
            new Agent (Side.Opponent, ConcealmentModule.ArgumentType.All) });
            Dispute dispute = ScenarioManager.LoadScenarioRecord();
            environment.RunDisputes(new List<Dispute> () { dispute });
            environment.dispute.Print();
            Structure structure = new Structure (environment.dispute);
            structure.PrintSidedGraphs();

            //Or:
            dispute = ScenarioManager.LoadScenario1();
            environment = new Environment (Module.On, Module.On, Module.Off, Module.On);
            environment.InitializeAgents(new List<Agent>() {
            new Agent (Side.Proponent,ConcealmentModule.ArgumentType.All),
            new Agent (Side.Opponent, ConcealmentModule.ArgumentType.All) });
            environment.RunDisputes(new List<Dispute> () { dispute });
            environment.dispute.Print();
            structure = new Structure (environment.dispute);
            structure.PrintSidedGraphs();



            //Or:
            dispute = ScenarioManager.LoadScenarioArgument();
            structure = new Structure (dispute);
            SidedGraph str = structure.CreateArgumentStructure(new Term("a"));
            str.PrintStructure();
        
    }
}
