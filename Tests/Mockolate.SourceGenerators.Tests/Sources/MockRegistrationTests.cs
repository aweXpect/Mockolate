using System.Linq;
using System.Threading;

namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed class MockRegistrationTests
{
	[Fact]
	public async Task ShouldRegisterAllTypesInTheMockGenerator()
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
			     			var y = Mock.Create<IMyInterface>();
			     			var z = Mock.Create<MyBaseClass, IMyInterface>();
			             }
			         }

			         public interface IMyInterface
			         {
			             void MyMethod(int value);
			         }

			         public class MyBaseClass
			         {
			             protected virtual Task<int> MyMethod(int v1, bool v2, double v3, long v4, uint v5, string v6, DateTime v7, TimeSpan v8, CancellationToken v9)
			             {
			                 return Task.FromResult(1);
			             }
			         }
			     }
			     """, typeof(DateTime), typeof(Task), typeof(CancellationToken));

		await That(result.Sources).ContainsKey("MockRegistration.g.cs").WhoseValue
			.Contains("""
			internal static partial class Mock
			{
				private partial class MockGenerator
				{
					partial void Generate(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior, params Type[] types)
					{
						if (types.Length == 1 &&
						    types[0] == typeof(IMyInterface))
						{
							_value = new ForIMyInterface.Mock(constructorParameters, mockBehavior);
						}
						else if (types.Length == 2 &&
						         types[0] == typeof(MyBaseClass) &&
						         types[1] == typeof(IMyInterface))
						{
							_value = new ForMyBaseClass_IMyInterface.Mock(constructorParameters, mockBehavior);
						}
					}
				}
			}
			""");
	}
}
