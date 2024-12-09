using Babble.Core;
using Babble.Core.Enums;
using Babble.Core.Settings;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace Babble.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[Config(typeof(CoreBenchmarkConfig))]
public class CoreBenchmark
{
    private BabbleCore _core;
    private Dictionary<UnifiedExpression, float> _expressions;
    private byte[]? _imageData;
    private (int width, int height) _dimensions;

    public static BabbleSettings Settings { get; set; } = null;

    private class CoreBenchmarkConfig : ManualConfig
    {
        public CoreBenchmarkConfig()
        {
            AddJob(Job.Default
                .WithWarmupCount(3)     // Number of warmup iterations
                .WithIterationCount(10)  // Number of target iterations
                .WithInvocationCount(64) // How many times to invoke per iteration, must be a multiple of 16
            );
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        _core = BabbleCore.Instance;
        _core.Start(Settings ?? new BabbleSettings());
        _expressions = new Dictionary<UnifiedExpression, float>();

        // Give camera time to initialize
        Thread.Sleep(1000);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _core.Stop();
    }

    [Benchmark(Description = "Expression Inference")]
    public bool BenchmarkExpressionInference()
    {
        return _core.GetExpressionData(out _expressions);
    }

    [Benchmark(Description = "Raw Frame Capture")]
    public bool BenchmarkRawFrameCapture()
    {
        return _core.GetRawImage(out _imageData, out _dimensions);
    }

    [Benchmark(Description = "Processed Frame")]
    public bool BenchmarkProcessedFrame()
    {
        return _core.GetImage(out _imageData, out _dimensions);
    }

    [Benchmark(Description = "Full Pipeline")]
    public (bool frameSuccess, bool inferenceSuccess) BenchmarkFullPipeline()
    {
        bool frameSuccess = _core.GetImage(out _imageData, out _dimensions);
        bool inferenceSuccess = _core.GetExpressionData(out _expressions);
        return (frameSuccess, inferenceSuccess);
    }
}