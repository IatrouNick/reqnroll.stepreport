// Reqnroll.StepReport/Program.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

string targetDir = args.FirstOrDefault() ?? Directory.GetCurrentDirectory();
Console.WriteLine($"🔍 Scanning directory: {targetDir}");

var stepAttributes = new[] { "Given", "When", "Then" };
var results = new List<StepDefinition>();

var files = Directory.GetFiles(targetDir, "*.cs", SearchOption.AllDirectories);
Console.WriteLine($"📄 Found {files.Length} .cs files");

foreach (var file in files)
{
    Console.WriteLine($"📂 Scanning file: {Path.GetFileName(file)}");

    var lines = File.ReadAllLines(file);
    for (int i = 0; i < lines.Length - 1; i++)
    {
        var line = lines[i].Trim();

        // Match [StepDefinition("Given", "...")]
        var stepDefFullMatch = Regex.Match(line, @"\[StepDefinition\(\s*""(Given|When|Then)""\s*,\s*@?""(.+?)""\s*\)\]");

        // Match [StepDefinition("...")] (no step type specified)
        var stepDefGenericMatch = Regex.Match(line, @"\[StepDefinition\(\s*@?""(.+?)""\s*\)\]");

        // Match legacy attributes: [Given("...")], etc.
        Match legacyMatch = null;
        string matchedAttr = null;

        foreach (var attr in stepAttributes)
        {
            var pattern = $@"\[{attr}\(\s*@?""(.+?)""\s*\)\]";
            var match = Regex.Match(line, pattern);
            if (match.Success)
            {
                legacyMatch = match;
                matchedAttr = attr;
                break;
            }
        }

        // Prepare method parameters from the next line
        var nextLine = lines[i + 1].Trim();
        var methodPattern = new Regex(@"\w+\s+\w+\s*\(([^)]*)\)");
        var methodMatch = methodPattern.Match(nextLine);

        var parameters = new List<string>();
        if (methodMatch.Success)
        {
            var paramList = methodMatch.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(paramList))
            {
                var paramPairs = paramList.Split(',');
                foreach (var param in paramPairs)
                {
                    var parts = param.Trim().Split(' ');
                    if (parts.Length >= 2)
                    {
                        parameters.Add(parts[0]); // just the type
                    }
                }
            }
        }

        var folder = Path.GetFileName(Path.GetDirectoryName(file))!;

        // Add step for legacy [Given]/[When]/[Then]
        if (legacyMatch != null)
        {
            string expression = legacyMatch.Groups[1].Value;
            AddStep(matchedAttr!, expression, parameters, folder);
        }
        // Add step for [StepDefinition("Given", "...")]
        else if (stepDefFullMatch.Success)
        {
            string attr = stepDefFullMatch.Groups[1].Value;
            string expression = stepDefFullMatch.Groups[2].Value;
            AddStep(attr, expression, parameters, folder);
        }
        // Add step for [StepDefinition("...")] — apply to all 3
        else if (stepDefGenericMatch.Success)
        {
            string expression = stepDefGenericMatch.Groups[1].Value;
            foreach (var attr in stepAttributes)
            {
                AddStep(attr, expression, parameters, folder);
            }
        }
    }
}

// ✅ Normalize and remove duplicates
var grouped = results
    .GroupBy(r => $"{r.StepDefinitionType}:{Regex.Unescape(r.Expression).Trim()}");

foreach (var group in grouped)
{
    if (group.Count() > 1)
    {
        Console.WriteLine($"⚠️ Duplicate ignored: {group.Key} found in multiple files.");
    }
}

var uniqueResults = grouped.Select(g => g.First()).ToList();

string outputFile = Path.Combine(targetDir, "steps.json");

if (uniqueResults.Count > 0)
{
    var json = JsonSerializer.Serialize(uniqueResults, new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });

    File.WriteAllText(outputFile, json);
    Console.WriteLine($"✅ Wrote {uniqueResults.Count} unique step definitions to {outputFile}");
}
else
{
    Console.WriteLine("⚠️ No step definitions found.");
}

void AddStep(string attr, string expression, List<string> parameters, string folder)
{
    Console.WriteLine($"✅ Matched [{attr}] {expression}");

    results.Add(new StepDefinition
    {
        StepDefinitionType = attr,
        Expression = expression,
        Parameters = parameters,
        Folder = folder
    });
}

record StepDefinition
{
    public string StepDefinitionType { get; set; }
    public string Expression { get; set; }
    public List<string> Parameters { get; set; }
    public string Folder { get; set; }
}
