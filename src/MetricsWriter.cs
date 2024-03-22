namespace Quorum;

internal sealed class MetricsWriter(TextWriter writer) : IDisposable
{
    private readonly TextWriter writer = writer;

    private bool hasHeader = false;

    public static MetricsWriter CreateFromFile(Type pluginType, int? quorumCount = null)
    {
        var timestamp = DateTime.Now;

        return new(File.CreateText(Path.Combine(Config.ResultPath, $"{pluginType.Name}-{(quorumCount.HasValue ? $"{quorumCount.Value}-" : string.Empty)}{Config.ModelName}-{timestamp:yyMMdd-HHmmss}.csv")));
    }

    public async Task WriteAsync(string category, string expected, string? result, TimeSpan duration, string input)
    {
        if (!this.hasHeader)
        {
            await this.writer.WriteLineAsync($"Category, Success, Expected, Result, Duration, Input");
            this.hasHeader = true;
        }

        await this.writer.WriteLineAsync($"{category}, {string.Equals(expected, result, StringComparison.OrdinalIgnoreCase)}, {expected.ToUpperInvariant()}, {result}, {duration}, {input}");
    }

    public void Dispose()
    {
        this.writer.Dispose();
    }
}
