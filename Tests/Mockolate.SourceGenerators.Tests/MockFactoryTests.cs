namespace Mockolate.SourceGenerators.Tests;

public class MockFactoryTests
{
	[Fact]
	public async Task Factory_ShouldGenerateMocksAndExtensions()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			                var factory = new Mock.Factory(MockBehavior.Default);
			     			var y = factory.Create<IMyInterface>();
			             }
			         }
			     
			         public interface IMyInterface
			         {
			             void MyMethod(int v1, bool v2, double v3, long v4, uint v5, string v6, DateTime v7);
			         }
			     }
			     """, typeof(DateTime), typeof(Task));

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).HasCount().AtLeast(5);
		await That(result.Sources).ContainsKey("ForIMyInterface.g.cs");
		await That(result.Sources).ContainsKey("ForIMyInterface.Extensions.g.cs");
		await That(result.Sources).ContainsKey("ForIMyInterface.SetupExtensions.g.cs");
	}
}
