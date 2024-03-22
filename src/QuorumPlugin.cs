namespace Quorum;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

public class QuorumPlugin : DemoPlugin
{
    public const int DefaultQuorumCount = 3;

    public int QuorumCount => this.ExecutionSettings?.ResultsPerPrompt ?? DefaultQuorumCount;

    [KernelFunction]
    [Description("$$$ DYNAMIC")]
    public override async Task<string?> InvokeResult(string input, CancellationToken cancellationToken = default)
    {
        var query = new ChatHistory(this.prompt);
        query.AddUserMessage(input);

        var messages = await this.aiService.GetChatMessageContentsAsync(query, GetSettings(DefaultQuorumCount), kernel: null, cancellationToken);
        var votes = messages.Select(m => ProcessResult(m));

        var quorum = TallyQuorum(votes);

        return quorum;
    }

    private static string? TallyQuorum(IEnumerable<string> votes)
    {
        return votes.GroupBy(v => v).MaxBy(g => g.Count())?.Key;
    }

    public QuorumPlugin(IChatCompletionService aiService, ChatHistory prompt)
        :base(aiService, prompt)
    {
    }

    public QuorumPlugin(IChatCompletionService aiService, string prompt)
        : base(aiService, prompt)
    {
    }
}
