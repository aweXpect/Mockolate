namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed partial class ClassTests
	{
		[Fact]
		public async Task BaseClassMethodWithParameterNamedBaseResult_ShouldGenerateUniqueLocalVariableName()
		{
			GeneratorResult result = Generator
				.Run("""
				     using Mockolate;

				     namespace MyCode;

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				     		_ = MyService.CreateMock();
				         }
				     }

				     public class MyService
				     {
				         public virtual int ProcessData(int baseResult) => baseResult;
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
				.Contains("MethodSetupResult<int> methodExecution = MockRegistry.InvokeMethod<int>(")
				.IgnoringNewlineStyle().And
				.Contains("if (methodExecution.SkipBaseClass)")
				.IgnoringNewlineStyle().And
				.Contains("var baseResult1 = base.ProcessData(baseResult);")
				.IgnoringNewlineStyle().And
				.Contains("return methodExecution.GetResult(baseResult1);")
				.Or
				.Contains("if (!methodExecution.HasSetupResult)")
				.IgnoringNewlineStyle().And
				.Contains("return baseResult1;")
				.IgnoringNewlineStyle();
		}
	}
}
