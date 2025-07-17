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

        foreach (var attr in stepAttributes)
        {
            var pattern = $@"\[{attr}\(\s*""(.+?)""\s*\)\]";
            var match = Regex.Match(line, pattern);

            if (match.Success)
            {
                string expression = match.Groups[1].Value;
                var parameters = new List<string>();

                // Look at the next line for method signature
                var nextLine = lines[i + 1].Trim();
                var methodPattern = new Regex(@"\w+\s+\w+\s*\(([^)]*)\)");
                var methodMatch = methodPattern.Match(nextLine);

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

                var folder = Path.GetFileName(Path.GetDirectoryName(file));
                Console.WriteLine($"✅ Matched [{attr}] {expression}");

                results.Add(new StepDefinition
                {
                    StepDefinitionType = attr,
                    Expression = expression,
                    Parameters = parameters,
                    Folder = folder
                });
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

record StepDefinition
{
    public string StepDefinitionType { get; set; }
    public string Expression { get; set; }
    public List<string> Parameters { get; set; }
    public string Folder { get; set; }
}
