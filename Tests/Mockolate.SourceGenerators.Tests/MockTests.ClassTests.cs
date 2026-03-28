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
		
		[Fact]
		public async Task InheritedInterface_ShouldHaveCorrectReferenceInXMLDocumentation()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				     		_ = IMyService.CreateMock();
				         }
				     }
				     
				     public interface IMyService : IMyBaseService
				     {
				     }
				     public interface IMyBaseService
				     {
				         int BaseProperty { get; }
				         event EventHandler<int> BaseEvent;
				         int BaseMethod();
				         int this[string baseIndexer] { get; }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
				.Contains("Setup for the int property <see cref=\"global::MyCode.IMyBaseService.BaseProperty\" />.").And
				.Contains("Verify interactions with the int property <see cref=\"global::MyCode.IMyBaseService.BaseProperty\" />.").And
				.Contains("Setup for the int indexer <see cref=\"global::MyCode.IMyBaseService.this[string]\" />").And
				.Contains("Verify interactions with the int indexer <see cref=\"global::MyCode.IMyBaseService.this[string]\" />.").And
				.Contains("Setup for the method <see cref=\"global::MyCode.IMyBaseService.BaseMethod()\"/>.").And
				.Contains("Verify invocations for the method <see cref=\"global::MyCode.IMyBaseService.BaseMethod()\"/>.").And
				.Contains("Raise the <see cref=\"global::MyCode.IMyBaseService.BaseEvent\"/> event.").And
				.Contains("Verify subscriptions on the BaseEvent event of <see cref=\"global::MyCode.IMyBaseService.BaseEvent\" />.");
		}
	}
}
