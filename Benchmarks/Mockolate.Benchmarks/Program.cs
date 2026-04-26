using BenchmarkDotNet.Running;
using Mockolate.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
