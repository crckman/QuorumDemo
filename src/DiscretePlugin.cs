namespace Quorum;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Threading.Tasks;

public class DiscretePlugin : DemoPlugin
{
    [KernelFunction]
    [Description("$$$ DYNAMIC")]
    public override async Task<string?> InvokeResult(string input, CancellationToken cancellationToken = default)
    {
        var query = new ChatHistory(this.prompt);
        query.AddUserMessage(input);

        var message = await this.aiService.GetChatMessageContentAsync(query, GetSettings(), kernel: null, cancellationToken);

        var result = ProcessResult(message);

        return result;
    }

    public DiscretePlugin(IChatCompletionService aiService, ChatHistory prompt)
        : base(aiService, prompt)
    {
    }

    public DiscretePlugin(IChatCompletionService aiService, string prompt)
        : base(aiService, prompt)
    {
    }
}
