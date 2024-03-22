using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Diagnostics;

namespace Quorum;

internal class Program
{
    private const double DemoTemperature = 0.3;
    private const int DemoIterations = 2;
    private const int DemoQuorum = 3;

    private enum Demo
    {
        Both,
        Discrete,
        Quorum,
    }

    /// <summary>
    /// Entry point for demo.
    /// For configuration: See <see cref="Config"/>.
    /// </summary>
    static async Task Main(string[] args)
    {
        Demo flavor = Demo.Both;

        if (args.Length > 1 ||
            args.Length == 1 && !Enum.TryParse(args[0], ignoreCase: true, out flavor))
        {
            Console.WriteLine();
            Console.WriteLine($"Provide a single argument to select a demo-mode (default: Both):");
            Console.WriteLine($"\n\t{string.Join("\n\t", Enum.GetValues<Demo>())}");
            Console.WriteLine();
            return;
        }

        if (flavor == Demo.Discrete || flavor == Demo.Both)
        {
            await RunDiscreteAsync();
        }

        if (flavor == Demo.Quorum || flavor == Demo.Both)
        {
            await RunQuorumAsync(quorumCount: DemoQuorum);
        }
    }

    private static async Task RunDiscreteAsync()
    {
        var plugin =
            new DiscretePlugin(CreateChatCompletionService(), instruction)
            {
                ExecutionSettings = new()
                {
                    Temperature = DemoTemperature,
                }
            };

        using var writer = CreateWriter(plugin);

        await RunDemoAsync(plugin, writer, iterations: DemoIterations);
    }

    private static async Task RunQuorumAsync(int quorumCount = 3)
    {
        var plugin =
            new QuorumPlugin(CreateChatCompletionService(), instruction)
            {
                ExecutionSettings = new()
                {
                    Temperature = DemoTemperature,
                    ResultsPerPrompt = quorumCount,
                }
            };

        using var writer = CreateWriter(plugin, quorumCount);

        await RunDemoAsync(plugin, writer, iterations: DemoIterations);
    }

    private static async Task RunDemoAsync(DemoPlugin plugin, MetricsWriter writer, int iterations = 1)
    {
        for (int index = 0; index < iterations; ++index)
        {
            Console.WriteLine($"\n# {index}");
            foreach (var question in questions)
            {
                try
                {
                    var timer = Stopwatch.StartNew();
                    var result = await plugin.InvokeResult(question.Text);
                    await WriteAsync(question, result ?? "-", timer.Elapsed);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"FAIL: {exception.Message}");
                }
            }
        }

        async Task WriteAsync(Question question, string result, TimeSpan duration)
        {
            Console.WriteLine($"{question.Expected.ToString().ToUpperInvariant()} ?== {result} ({question.Category})");
            await (writer.WriteAsync(question.Category.ToString(), question.Expected.ToString().ToUpperInvariant(), result, duration, question.Text) ?? Task.CompletedTask);
        }
    }

    private static OpenAIChatCompletionService CreateChatCompletionService()
    {
        return new OpenAIChatCompletionService(Config.ModelName, Config.ApiKey);
    }

    private static MetricsWriter CreateWriter(DemoPlugin plugin, int? quorumCount = null)
    {
        return MetricsWriter.CreateFromFile(plugin.GetType(), quorumCount);
    }

    private static readonly string instruction =
        """
        Think step-by-step to evaulate the meaning of the most recent user message.
        Respond only with question, statement, or both in determination of whether the most recent user message could be interpreted as a question or statement.
        """;

    private static readonly Question[] questions =
        [
            new("are you cool", QuestionResult.Question, QuestionCategory.Control),
            new("i am cool", QuestionResult.Statement, QuestionCategory.Control),
            new("i am cool?", QuestionResult.Question, QuestionCategory.Control),
            new("i am curious on what you think", QuestionResult.Both, QuestionCategory.Experiment),
            new("huh, would ya look at that?", QuestionResult.Both, QuestionCategory.Experiment),
        ];

    private enum QuestionResult
    {
        Question,
        Statement,
        Both,
    }

    private enum QuestionCategory
    {
        Control,
        Experiment,
    }

    private record Question(string Text, QuestionResult Expected, QuestionCategory Category);
}
