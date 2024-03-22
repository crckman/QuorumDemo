namespace Quorum;

internal sealed class MetricsWriter(TextWriter writer) : IDisposable
{
    private readonly TextWriter writer = writer;

    private bool hasHeader = false;

    public static MetricsWriter CreateFromFile(Type pluginType, Type demoType, int? quorumCount = null)
    {
        var timestamp = DateTime.Now;

        return new(File.CreateText(Path.Combine(Config.ResultPath, $"{demoType.Name}-{pluginType.Name}-{(quorumCount.HasValue ? $"{quorumCount.Value}-" : string.Empty)}{Config.ModelName}-{timestamp:yyMMdd-HHmmss}.csv")));
    }

    public async Task<bool> WriteAsync(string category, string expected, string? result, TimeSpan duration, string input)
    {
        if (!this.hasHeader)
        {
            await this.writer.WriteLineAsync($"Category, Success, Expected, Result, Duration, Input");
            this.hasHeader = true;
        }

        var isSuccess = string.Equals(expected, result, StringComparison.OrdinalIgnoreCase);

        await this.writer.WriteLineAsync($"{category}, {isSuccess}, {expected.ToUpperInvariant()}, {result}, {duration}, {input}");

        return isSuccess;
    }

    public void Dispose()
    {
        this.writer.Dispose();
    }
}
