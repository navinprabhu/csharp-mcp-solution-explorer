using System.Text.Json;
using CSharpSolutionExplorer;

class Program
{
    static async Task Main(string[] args)
    {
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) break;
            
            try
            {
                var request = JsonSerializer.Deserialize<McpRequest>(line);
                var response = await HandleRequest(request);
                Console.WriteLine(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                var errorResponse = new { error = ex.Message };
                Console.WriteLine(JsonSerializer.Serialize(errorResponse));
            }
        }
    }
    
    static async Task<object> HandleRequest(McpRequest request)
    {
        return request.Method switch
        {
            "tools/list" => new
            {
                tools = new[]
                {
                    new { name = "list_csharp_projects", description = "Lists all .csproj and .sln files in a directory" },
                    new { name = "analyze_project", description = "Analyzes a C# project file and shows its dependencies" },
                    new { name = "search_csharp_code", description = "Searches for C# classes, interfaces, or methods" }
                }
            },
            "tools/call" => await CallTool(request),
            _ => new { error = "Unknown method" }
        };
    }
    
    static async Task<object> CallTool(McpRequest request)
    {
        var toolName = request.Params?.GetProperty("name").GetString();
        var args = request.Params?.GetProperty("arguments");
        
        return toolName switch
        {
            "list_csharp_projects" => new { content = new[] { new { type = "text", text = await SolutionExplorerTools.ListCSharpProjects(args?.GetProperty("path").GetString() ?? ".") } } },
            "analyze_project" => new { content = new[] { new { type = "text", text = await SolutionExplorerTools.AnalyzeProject(args?.GetProperty("projectPath").GetString() ?? "") } } },
            "search_csharp_code" => new { content = new[] { new { type = "text", text = await SolutionExplorerTools.SearchCSharpCode(args?.GetProperty("path").GetString() ?? ".", args?.GetProperty("searchTerm").GetString() ?? "") } } },
            _ => new { error = "Unknown tool" }
        };
    }
}

public class McpRequest
{
    public string? Method { get; set; }
    public JsonElement? Params { get; set; }
}