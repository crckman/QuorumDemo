namespace Quorum;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Threading.Tasks;

public class DiscreteStrategy : DemoStrategy
{
    public override async Task<string?> InvokeResult(string input, CancellationToken cancellationToken = default)
    {
        var query = new ChatHistory(this.prompt);
        query.AddUserMessage(input);

        var message = await this.aiService.GetChatMessageContentAsync(query, GetSettings(), kernel: null, cancellationToken);

        var result = ProcessResult(message);

        return result;
    }

    public DiscreteStrategy(IChatCompletionService aiService, ChatHistory prompt)
        : base(aiService, prompt)
    {
    }

    public DiscreteStrategy(IChatCompletionService aiService, string prompt)
        : base(aiService, prompt)
    {
    }
}
