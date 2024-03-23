namespace Quorum;

using Microsoft.SemanticKernel.ChatCompletion;

public abstract class QuorumDemo<TResult> where TResult : Enum
{
    public virtual bool JsonMode => false;

    public abstract ChatHistory CreatePrompt();

    public abstract IEnumerable<Question<TResult>> GetQuestions();
}

public enum QuestionCategory
{
    Control,
    Experiment,
}

public record Question<TResult>(string Text, TResult Expected, QuestionCategory Category) where TResult : Enum;