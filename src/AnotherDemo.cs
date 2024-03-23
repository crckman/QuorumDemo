namespace Quorum;

using Microsoft.SemanticKernel.ChatCompletion;

internal sealed class AnotherDemo : QuorumDemo<AnotherType>
{
    public override ChatHistory CreatePrompt()
    {
        var history = new ChatHistory();

        history.AddSystemMessage(
            """
            Think step-by-step to evaluate if the user message sounds like a useful plan with the goal of categorizing as: unknown, yes, or no.
            Respond only with the categorization.
            """);

        return history;
    }

    public override IEnumerable<Question<AnotherType>> GetQuestions()
    {
        yield return new("I plan to to bed at 10pm", AnotherType.Yes, QuestionCategory.Control);
        yield return new("I will crash my car", AnotherType.No, QuestionCategory.Control);
        yield return new("I don't like you", AnotherType.Unknown, QuestionCategory.Control);
    }
}

internal enum AnotherType
{
    // $$$ FLAGS
    // Imperative
    // Declarative
    // Exclamation
    // Interrogative
    Unknown,
    Yes,
    No,
}