# C# Solution Explorer MCP Server

A Model Context Protocol (MCP) server that provides C# development tools for analyzing projects and solutions.

## Features

- **ListCSharpProjects**: Find all .csproj and .sln files in a directory
- **AnalyzeProject**: Analyze project dependencies, target framework, and properties
- **SearchCSharpCode**: Search for classes, methods, or any code patterns across .cs files

## Setup

1. **Build the project**:
   ```bash
   dotnet build
   ```

2. **Configure in Claude Code**:
   Add to your MCP configuration file (usually in VS Code settings):
   ```json
   {
     "servers": {
       "csharp-solution-explorer": {
         "type": "stdio",
         "command": "dotnet",
         "args": ["run", "--project", "/path/to/CSharpSolutionExplorer.csproj"]
       }
     }
   }
   ```

3. **Test the server**:
   ```bash
   dotnet run
   ```

## Usage Examples

Once configured with Claude Code, you can use commands like:
- "List all C# projects in this directory"
- "Analyze the main project file"
- "Search for 'HttpClient' usage in the codebase"

## Extension Ideas

- Add support for analyzing NuGet packages
- Implement code generation helpers
- Add project template creation
- Include test discovery and analysis
- Add performance metrics extraction