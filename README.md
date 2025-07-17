# Reqnroll.StepReport 🧪

A .NET CLI tool that extracts all [Reqnroll](https://reqnroll.dev) step definitions from your C# files and outputs them into a structured JSON file (`steps.json`).

Perfect for QA/SDETs who want to integrate step definitions into documentation tools, Gherkin editors, or test step visualizers.

---

## 📦 Installation

Make sure you have [.NET SDK 6.0 or later](https://dotnet.microsoft.com/download) installed.

```bash
dotnet tool install -g stepreport.cli

🚀 Usage
Navigate to your Reqnroll project folder or pass the path as an argument:

bash
Copy
Edit
stepreport "C:\Path\To\Your\Project"
This will:

Scan all .cs files for [Given], [When], and [Then] attributes

Extract each step with its expression, parameters, and folder name

Output a steps.json file in the same directory

🧾 Example Output of steps.json

[
  {
    "StepDefinitionType": "Given",
    "Expression": "the user is on the login page",
    "Parameters": [],
    "Folder": "Authentication"
  },
  {
    "StepDefinitionType": "When",
    "Expression": "the user submits their email \\\"(.*)\\\"",
    "Parameters": ["string"],
    "Folder": "Authentication"
  }
]

🛠 Features
Supports nested folders and multiple step definition files

Recognizes parameter types like int, decimal, bool, and string

Avoids duplicates across files/folders

