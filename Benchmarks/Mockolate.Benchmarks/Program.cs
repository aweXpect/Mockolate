using BenchmarkDotNet.Running;
using Mockolate.Benchmarks;

if (args.Length == 1 && args[0] == "--smoke")
{
	SuggestedMocksSmokeTest.Run();
	System.Console.WriteLine("smoke ok");
	return;
}

SuggestedMocksSmokeTest.Run();
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
