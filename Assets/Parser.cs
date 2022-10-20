using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using Excel = Microsoft.Office.Interop.Excel;

public static class DisputeParser
{

    public static EvaluationDispute DisputeToStringDispute(Dispute dispute)
    {
        return new EvaluationDispute(
            Convert(dispute.sharedKnowledgeBase.axioms),
            Convert(dispute.sharedKnowledgeBase.premises),
            Convert(dispute.sharedKnowledgeBase.assumptions),
            dispute.sharedKnowledgeBase.kbPrefs,
            Convert(dispute.sharedKnowledgeBase.rules),
            dispute.sharedKnowledgeBase.rulePrefs,
            Convert(dispute.sharedKnowledgeBase.contrariness),
            Convert(dispute.link),
            Convert(dispute.semantics),
            Convert(dispute.transposition)
        );
    }

    
    public static string[] Convert(IDispute[] rs)
    {
        return rs.Select(x => x.Convert()).ToArray();
    }
    
    public static string[] Convert(List<Term> rs)
    {
        return rs.Select(x => x.Convert()).ToArray();
    }

    public static string[] Convert(List<Rule> rs)
    {
        return rs.Select(x => x.Convert()).ToArray();
    }

    public static string[] Convert(List<Contrary> rs)
    {
        return rs.Select(x => x.Convert()).ToArray();
    }

    public static string Convert(Enum e)
    {
        return e.ToString();
    }
    public static string Convert(bool b)
    {
        return b.ToString().ToLower();
    }

}


/// <summary>
/// This class is a moment in a dispute, 
/// All parts are in strings specifically adhering to https://toast.arg.tech/help/api input,
/// DO NOT CHANGE ANYTHING OF THIS FUNCTION
/// </summary>
public class EvaluationDispute
{
    // List of axioms
    public string[] axioms;

    // List of premises
    public string[] premises;

    // List of assumptions
    public string[] assumptions;

    // List of knowledge base preferences (note: cannot contain any axioms)
    public string[] kbPrefs;

    // List of rules
    public string[] rules;

    //List of rule preferences
    public string[] rulePrefs;

    //List of contraries and contradictories
    public string[] contrariness;

    //Link preference principle:  Weakest(default) or last link
    public string link;

    //Semantics: Grounded (default), preferred or semi-stable
    public string semantics;

    //Formula to find an(un)acceptable argument for
    public string query;

    //True or false (default); whether or not to close the theory under transposition
    public string transposition;

    public EvaluationDispute(string[] _axioms, string[] _premises, string[] _assumptions,
        string[] _kbPrefs, string[] _rules, string[] _rulePrefs, string[] _contrariness,
        string _link, string _semantics, string _transposition)
    {
        axioms = _axioms;
        premises = _premises;
        assumptions = _assumptions;
        kbPrefs = _kbPrefs;
        rules = _rules;
        rulePrefs = _rulePrefs;
        contrariness = _contrariness;
        link = _link;
        semantics = _semantics;
        transposition = _transposition;
    }

}






/// <summary>
/// Functions for performing common Json Serialization operations.
/// <para>Requires the Newtonsoft.Json assembly (Json.Net package in NuGet Gallery) to be referenced in your project.</para>
/// <para>Only public properties and variables will be serialized.</para>
/// <para>Use the [JsonIgnore] attribute to ignore specific public properties or variables.</para>
/// <para>Object to be serialized must have a parameterless constructor.</para>
/// </summary>
public static class JsonSerialization
{
    public static void SaveDispute(Dispute _dispute, string _filename)
    {
        WriteToJsonFile("Data\\" + _filename + ".txt", _dispute);
    }

    public static Dispute LoadDispute(string _filename)
    {
        return ReadFromJsonFile<Dispute>("Data\\" + _filename + ".txt");
    }

    public static void SaveDisputes(List<Dispute> _disputes, string _filename)
    {
        WriteToJsonFile("Data\\" + _filename + ".txt", _disputes);
    }

    public static List<Dispute> LoadDisputes(string _filename)
    {
        return ReadFromJsonFile<List<Dispute>>("Data\\" + _filename + ".txt");
    }

    //https://blog.danskingdom.com/saving-and-loading-a-c-objects-data-to-an-xml-json-or-binary-file/

    /// <summary>
    /// Writes the given object instance to a Json file.
    /// <para>Object type must have a parameterless constructor.</para>
    /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
    /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
    /// </summary>
    /// <typeparam name="T">The type of object being written to the file.</typeparam>
    /// <param name="filePath">The file path to write the object instance to.</param>
    /// <param name="objectToWrite">The object instance to write to the file.</param>
    /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
    public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
    {
        TextWriter writer = null;
        try
        {
            var contentsToWriteToFile = Newtonsoft.Json.JsonConvert.SerializeObject(objectToWrite);
            writer = new StreamWriter(filePath, append);
            writer.Write(contentsToWriteToFile);
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }
    }

    /// <summary>
    /// Reads an object instance from an Json file.
    /// <para>Object type must have a parameterless constructor.</para>
    /// </summary>
    /// <typeparam name="T">The type of object to read from the file.</typeparam>
    /// <param name="filePath">The file path to read the object instance from.</param>
    /// <returns>Returns a new instance of the object read from the Json file.</returns>
    public static T ReadFromJsonFile<T>(string filePath) where T : new()
    {
        TextReader reader = null;
        try
        {
            reader = new StreamReader(filePath);
            var fileContents = reader.ReadToEnd();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(fileContents);
        }
        finally
        {
            if (reader != null)
                reader.Close();
        }
    }
}


public class CSVSerialisation
{
    string path;
    StringBuilder csv;
    //string seperator;

    public CSVSerialisation(string _path)
    {
        csv = new StringBuilder ();
        path = _path + ".csv";
        //seperator = ",";
    }

    public void WriteData(string arg, string _0, string _1, string _2, string _3, string _4, string _5, string _6, string _7, string _8)
    {
        csv.AppendLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};", arg, _0, _1, _2, _3, _4, _5, _6, _7, _8));
        Save();
    }

    public void WriteData(string _0, string _1, string _2, string _3, string _4, string _5, string _6, string _7, string _8)
    {
        csv.AppendLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};", _0, _1, _2, _3, _4, _5, _6, _7, _8));
        Save();
    }

    public void WriteData(string _0, string _1, string _2, string _3, string _4, string _5, string _6, string _7)
    {
        csv.AppendLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};", _0, _1, _2, _3, _4, _5, _6, _7));
        Save();
    }


    public void WriteData(string arg,string _0, string _1, string _2, string _3, string _4, string _5)
    {
        csv.AppendLine(string.Format("{0};{1};{2};{3};{4};{5};{6};", arg,_0, _1, _2, _3, _4, _5));
        Save();
    }

    public void WriteData(string arg, string _0, string _1, string _2, string _3, string _4, string _5, string _6, string _7, string _8, string _9)
    {
        csv.AppendLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};", arg, _0, _1, _2, _3, _4, _5, _6, _7, _8, _9));
        Save();
    }

    public void Save(string _version = "")
    {
        File.WriteAllText(path, csv.ToString());
    }
}
/*
public class ExcelSerialisation {

    string path;// = "";
    Excel.Application excel;// = new Excel.Application();
    Excel.Workbook workbook;// = new Excel.Workbook();
    Excel.Worksheet worksheet;// = new Excel.Worksheet();

    public ExcelSerialisation(string _path, int _sheet = 1)
    {
        path = _path+".xlsx";
        excel = new Excel.Application();
        workbook = excel.Workbooks.Open(path);
        worksheet = (Excel.Worksheet)workbook.Worksheets[_sheet];
    }

    public void WriteData(int index, string _0, string _1, string _2, string _3, string _4, string _5)
    {
        WriteToCell(0, index, _0);
        WriteToCell(1, index, _1);
        WriteToCell(2, index, _2);
        WriteToCell(3, index, _3);
        WriteToCell(4, index, _4);
        WriteToCell(5, index, _5);

    }


    public void WriteToCell(int _i, int _j, string _data)
    {
        worksheet.Cells[_i, _j] = _data;
    }

    public void SaveAndClose()
    {
        workbook.Save();
        workbook.Close();
    }
        //Excel.Application myexcelApplication = new Excel.Application();
        //if (myexcelApplication!= null)
        //{
        //    Excel.Workbook myexcelWorkbook = myexcelApplication.Workbooks.Add();
        //    Excel.Worksheet myexcelWorksheet = (Excel.Worksheet)myexcelWorkbook.Sheets.Add();

        //}



}*/

