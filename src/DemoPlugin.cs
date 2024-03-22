namespace Quorum;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Threading.Tasks;

public abstract class DemoPlugin
{
    protected readonly IChatCompletionService aiService;
    protected readonly ChatHistory prompt;

    public OpenAIPromptExecutionSettings? ExecutionSettings { get; set; }

    internal MetricsWriter? MetricsWriter { get; set; }

    public abstract Task<string?> InvokeResult(string input, CancellationToken cancellationToken = default);

    protected OpenAIPromptExecutionSettings GetSettings(int resultsPerPrompt = 1)
    {
        return this.ExecutionSettings ??= new OpenAIPromptExecutionSettings() { ResultsPerPrompt = resultsPerPrompt };
    }

    protected static string ProcessResult(ChatMessageContent m)
    {
        var result = CleanResult(m.Content) ?? string.Empty;
        System.Console.WriteLine($"> {result}"); // $$$
        return result;
    }

    private static string? CleanResult(string? content)
    {
        if (content == null)
        {
            return null;
        }

        char[] buffer = new char[content.Length];

        int length = 0;
        foreach (char c in content)
        {
            if (char.IsPunctuation(c) || char.IsWhiteSpace(c))
            {
                continue;
            }

            buffer[length++] = char.ToUpperInvariant(c);
        }

        return new string(buffer, startIndex: 0, length);
    }

    public DemoPlugin(IChatCompletionService aiService, ChatHistory prompt)
    {
        this.aiService = aiService;
        this.prompt = prompt;
    }

    public DemoPlugin(IChatCompletionService aiService, string prompt)
    {
        this.aiService = aiService;
        this.prompt = new ChatHistory(systemMessage: prompt);
    }
}
