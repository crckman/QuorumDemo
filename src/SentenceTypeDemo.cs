using Microsoft.SemanticKernel.ChatCompletion;

namespace Quorum;

internal sealed class SentenceTypeDemo : QuorumDemo<SentenceType>
{
    public override ChatHistory CreatePrompt()
    {
        var history = new ChatHistory();

        history.AddSystemMessage(
            """
            Think step-by-step to evaluate the intent of the user message with the goal of categorizing as: question, statement, or both.
            Respond only with the categorization.
            A question could be a statement intended to elicit a response from the listener, if so categorize as both.
            A question that is missing a question mark should be categorized as question.
            A statement could be mislabled with a question mark, focus on the meaning.
            """);

        return history;
    }

    public override IEnumerable<Question<SentenceType>> GetQuestions()
    {
        // Simple, comlex, compound
        yield return new("Are you cool", SentenceType.Question, QuestionCategory.Control);
        yield return new("I am cool", SentenceType.Statement, QuestionCategory.Control);
        yield return new("I am cool?", SentenceType.Both, QuestionCategory.Control);
        yield return new("I am curious on what you think", SentenceType.Both, QuestionCategory.Experiment);
        yield return new("would ya look at that?", SentenceType.Both, QuestionCategory.Experiment);
        yield return new("You think I'm kidding you", SentenceType.Both, QuestionCategory.Experiment);
        yield return new("You think I'm kidding you?", SentenceType.Both, QuestionCategory.Experiment);
        yield return new("He asked me what I wanted", SentenceType.Statement, QuestionCategory.Experiment);
        yield return new("He asked me what I wanted?", SentenceType.Statement, QuestionCategory.Experiment);
        yield return new("This room is warm", SentenceType.Statement, QuestionCategory.Experiment);
        yield return new("This room is warm?", SentenceType.Both, QuestionCategory.Experiment);
        yield return new("You like me", SentenceType.Both, QuestionCategory.Experiment);
        yield return new("She asked if I was hungry", SentenceType.Statement, QuestionCategory.Experiment);
        yield return new("Pearls melt in vinegar", SentenceType.Statement, QuestionCategory.Experiment);
        yield return new("I like the movie", SentenceType.Statement, QuestionCategory.Experiment);
    }
}

internal enum SentenceType
{
    // $$$ FLAGS
    // Imperative
    // Declarative
    // Exclamation
    // Interrogative
    Question,
    Statement,
    Both,
}