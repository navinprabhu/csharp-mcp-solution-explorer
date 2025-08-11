using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Attributes;

namespace CSharpSolutionExplorer;

[McpServerToolType]
public static class SolutionExplorerTools
{
    [McpServerTool]
    [Description("Lists all .csproj and .sln files in a directory")]
    public static async Task<string> ListCSharpProjects(
        [Description("Directory path to search (default: current directory)")]
        string path = ".")
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(fullPath))
                return $"Directory not found: {fullPath}";

            var projects = Directory.GetFiles(fullPath, "*.csproj", SearchOption.AllDirectories);
            var solutions = Directory.GetFiles(fullPath, "*.sln", SearchOption.AllDirectories);
            
            var result = new
            {
                Directory = fullPath,
                Solutions = solutions.Select(s => new { 
                    Name = Path.GetFileName(s), 
                    Path = Path.GetRelativePath(fullPath, s) 
                }),
                Projects = projects.Select(p => new { 
                    Name = Path.GetFileName(p), 
                    Path = Path.GetRelativePath(fullPath, p),
                    Directory = Path.GetDirectoryName(Path.GetRelativePath(fullPath, p))
                })
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Analyzes a C# project file and shows its dependencies and properties")]
    public static async Task<string> AnalyzeProject(
        [Description("Path to the .csproj file")]
        string projectPath)
    {
        try
        {
            if (!File.Exists(projectPath))
                return $"Project file not found: {projectPath}";

            var content = await File.ReadAllTextAsync(projectPath);
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            
            // Simple XML parsing for demo - in real world, use XDocument
            var lines = content.Split('\n');
            var targetFramework = ExtractValue(lines, "TargetFramework");
            var outputType = ExtractValue(lines, "OutputType");
            var packages = ExtractPackages(lines);

            var result = new
            {
                ProjectName = projectName,
                ProjectPath = projectPath,
                TargetFramework = targetFramework,
                OutputType = outputType,
                PackageReferences = packages,
                ProjectSize = new FileInfo(projectPath).Length
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error analyzing project: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Searches for C# classes, interfaces, or methods in .cs files")]
    public static async Task<string> SearchCSharpCode(
        [Description("Directory path to search")]
        string path,
        [Description("Search term (class name, method name, etc.)")]
        string searchTerm)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(fullPath))
                return $"Directory not found: {fullPath}";

            var csFiles = Directory.GetFiles(fullPath, "*.cs", SearchOption.AllDirectories);
            var results = new List<object>();

            foreach (var file in csFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var lines = content.Split('\n');
                
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new
                        {
                            File = Path.GetRelativePath(fullPath, file),
                            LineNumber = i + 1,
                            Line = lines[i].Trim(),
                            Context = GetContext(lines, i, 2)
                        });
                    }
                }
            }

            return JsonSerializer.Serialize(new 
            { 
                SearchTerm = searchTerm,
                ResultCount = results.Count,
                Results = results.Take(20) // Limit results
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error searching code: {ex.Message}";
        }
    }

    private static string ExtractValue(string[] lines, string tag)
    {
        var line = lines.FirstOrDefault(l => l.Contains($"<{tag}>"));
        if (line == null) return "Not specified";
        
        var start = line.IndexOf($"<{tag}>") + tag.Length + 2;
        var end = line.IndexOf($"</{tag}>");
        
        return end > start ? line.Substring(start, end - start) : "Not specified";
    }

    private static List<string> ExtractPackages(string[] lines)
    {
        return lines
            .Where(l => l.Contains("PackageReference"))
            .Select(l =>
            {
                var start = l.IndexOf("Include=\"") + 9;
                var end = l.IndexOf("\"", start);
                return end > start ? l.Substring(start, end - start) : "";
            })
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();
    }

    private static string[] GetContext(string[] lines, int lineIndex, int contextLines)
    {
        var start = Math.Max(0, lineIndex - contextLines);
        var end = Math.Min(lines.Length - 1, lineIndex + contextLines);
        
        return lines[start..(end + 1)];
    }
}