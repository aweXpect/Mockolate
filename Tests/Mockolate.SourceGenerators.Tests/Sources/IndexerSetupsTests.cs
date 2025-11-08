using System.Threading;

namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed class IndexerSetupsTests
{
	[Fact]
	public async Task GenerateIndexerSetupsForIndexersWithMoreParameters()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyInterface>();
			             }
			         }

			         public interface IMyInterface
			         {
			             int this[int v1, bool v2, double v3, long v4, CancellationToken v5];
			         }
			     }
			     """, typeof(CancellationToken));

		await That(result.Sources).ContainsKey("IndexerSetups.g.cs").WhoseValue
			.Contains(
				"internal class IndexerSetup<TValue, T1, T2, T3, T4, T5>(IParameter match1, IParameter match2, IParameter match3, IParameter match4, IParameter match5) : IndexerSetup");
	}

	[Fact]
	public async Task WhenAllIndexersHaveUpTo4Parameters_ShouldNotGenerateIndexerSetups()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyInterface>();
			             }
			         }

			         public interface IMyInterface
			         {
			             int this[int v1, bool v2, double v3, long v4];
			             int this[int v1, bool v2, double v3];
			             int this[int v1, bool v2];
			             int this[int v1];
			         }
			     }
			     """);

		await That(result.Sources).DoesNotContainKey("IndexerSetups.g.cs");
	}

	[Fact]
	public async Task WithComplexIndexer_ShouldOnlyGenerateNecessaryExtensions()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyInterface>();
			             }
			         }

			         public interface IMyInterface
			         {
			             int this[int v1, bool v2, double v3, long v4, string v5, int v6];
			         }
			     }
			     """);

		await That(result.Sources).ContainsKey("IndexerSetups.g.cs").WhoseValue
			.Contains("class IndexerSetup<TValue, T1, T2, T3, T4, T5, T6>(").And
			.DoesNotContain("class IndexerSetup<TValue, T1, T2, T3, T4, T5>(").And
			.DoesNotContain("class IndexerSetup<TValue, T1, T2, T3, T4, T5, T6, T7>(");
	}
}
