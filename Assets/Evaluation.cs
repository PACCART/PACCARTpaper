using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Text.Json;
using System.Threading.Tasks;
using System.IO;



public class EvaluationManager
{
    public static EvaluationManager Instance;

    public HttpClient client;
    public EvaluationManager()
    {
        if (Instance == null)
            Instance = this;

        client = new HttpClient();
        InitializeApiConnection();
    }
 
    public void InitializeApiConnection()
    {
        //Find API
        client.BaseAddress = new Uri("http://toast.arg.tech/api");
        client.DefaultRequestHeaders.Accept.Clear();
    }

    public async Task<bool> Wins(Dispute dispute, string query, bool print = false)
    {
        EvaluationResult er = await Evaluate(dispute, query, print);
        return er.Wins();
    }

    public async Task<EvaluationResult> Evaluate(Dispute dispute, string query, bool print = false)
    {
        /*

        EvaluationDispute evaluationDispute = DisputeParser.DisputeToStringDispute(dispute);
        evaluationDispute.query = query;

        HttpResponseMessage response = await client.PostAsJsonAsync("api/evaluate", evaluationDispute);
        response.EnsureSuccessStatusCode();

        if (print)
            Console.WriteLine(await response.Content.ReadAsStringAsync());


        EvaluationResult evaluationResult = await response.Content.ReadAsAsync<EvaluationResult>();

        if (!evaluationResult.IsWellformed())
            Console.WriteLine("Warning, your argumentation theory is not well formed.");
        
    
        return evaluationResult; //evaluationResult.Wins();*/
        return null;
    }

}

public class EvaluationResult
{
    //Adhering to https://toast.arg.tech/help/api output

    public string wellformed;
    public string result;
    public KeyValuePair<string, object> acceptableConclusions;
    public string[] messages;
    public string[] arguments;
    public KeyValuePair<string, object> extensions;
    public string[] defeat;
    public EvaluationResult() { }

    public bool IsWellformed()
    {
        return wellformed == "true";
    }

    public bool Wins()
    {
        return result == "true";
    }

}
