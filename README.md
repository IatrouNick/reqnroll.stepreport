# \# Reqnroll.StepReport 🧪

# 

# A .NET CLI tool that extracts all Reqnroll step definitions from a project's C# files and outputs them into a structured JSON file (`steps.json`). Designed to help QA/SDETs integrate step definitions into documentation tools or custom Gherkin editors.

# 

# ---

# 

# \## 📦 Installation

# 

# You must have \[.NET SDK 6.0+](https://dotnet.microsoft.com/download) installed.

# 

# Install the global tool via:

# 

# ```bash

# dotnet tool install --global reqnroll.stepreport

🚀 Usage
Run this command inside your Reqnroll project or point it to the folder:

stepreport "C:\Path\To\Your\Project"

It will scan all .cs files and output a steps.json in that directory.

