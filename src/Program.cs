using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Diagnostics;

namespace Quorum;

internal class Program
{
    private const double DemoTemperature = 1;
    private const int DemoIterations = 1;
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

        var demo = new SentenceTypeDemo();

        if (flavor == Demo.Discrete || flavor == Demo.Both)
        {
            await RunDiscreteAsync();
        }

        if (flavor == Demo.Quorum || flavor == Demo.Both)
        {
            await RunQuorumAsync(quorumCount: DemoQuorum);
        }

        async Task RunDiscreteAsync()
        {
            var plugin =
                new DiscretePlugin(CreateChatCompletionService(), demo.CreatePrompt())
                {
                    ExecutionSettings = new()
                    {
                        Temperature = DemoTemperature,
                    }
                };

            using var writer = CreateWriter(plugin);

            await RunDemoAsync(plugin, demo, writer, iterations: DemoIterations);
        }

        async Task RunQuorumAsync(int quorumCount = 3)
        {
            var plugin =
                new QuorumPlugin(CreateChatCompletionService(), demo.CreatePrompt())
                {
                    ExecutionSettings = new()
                    {
                        Temperature = DemoTemperature,
                        ResultsPerPrompt = quorumCount,
                    }
                };

            using var writer = CreateWriter(plugin, quorumCount);

            await RunDemoAsync(plugin, demo, writer, iterations: DemoIterations);
        }

        MetricsWriter CreateWriter(DemoPlugin plugin, int? quorumCount = null)
        {
            return MetricsWriter.CreateFromFile(plugin.GetType(), demo.GetType(), quorumCount);
        }
    }

    public static async Task RunDemoAsync<TResult>(DemoPlugin plugin, QuorumDemo<TResult> demo, MetricsWriter writer, int iterations = 1) where TResult : Enum
    {
        int questionNumber = 1;
        int questionSuccess = 0;
        int questionCount = 0;
        for (int index = 0; index < iterations; ++index)
        {
            questionNumber = 1;
            Console.WriteLine($"\n# {index:00}");
            foreach (var question in demo.GetQuestions())
            {
                try
                {
                    var timer = Stopwatch.StartNew();
                    var result = await plugin.InvokeResult(question.Text);
                    if (await WriteAsync(question, result ?? "-", timer.Elapsed))
                    {
                        ++questionSuccess;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"FAIL: {exception.Message}");
                }

                ++questionNumber;
                ++questionCount;
            }
        }

        Console.WriteLine($"{questionSuccess}/{questionCount}");

        async Task<bool> WriteAsync(Question<TResult> question, string result, TimeSpan duration)
        {
            Console.WriteLine($"- {questionNumber:00} {question.Expected.ToString().ToUpperInvariant()} ?== {result} ({question.Category})");
            return await writer.WriteAsync(question.Category.ToString(), question.Expected.ToString().ToUpperInvariant(), result, duration, question.Text);
        }
    }

    private static OpenAIChatCompletionService CreateChatCompletionService()
    {
        return new OpenAIChatCompletionService(Config.ModelName, Config.ApiKey);
    }
}
