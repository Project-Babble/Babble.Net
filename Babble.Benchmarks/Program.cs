using Babble.Benchmarks.Babble.Avalonia;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Babble.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        var config = DefaultConfig.Instance
                    .WithOption(ConfigOptions.DisableOptimizationsValidator, true)
                    .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend));

        // var summary = BenchmarkRunner.Run<CoreBenchmark>(config);
        var summary = BenchmarkRunner.Run<AvaloniaBenchmarks>(config);

        // For specific benchmarks:
        // var summary = BenchmarkRunner.Run<BabbleBenchmarks>(config, 
        //     new[] { "BenchmarkExpressionInference", "BenchmarkFullPipeline" });
    }
}
