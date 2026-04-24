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
				.Contains("var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<int, int>, int>(0, \"global::MyCode.MyService.ProcessData\", baseResult);")
				.IgnoringNewlineStyle().And
				.Contains("wrappedResult = base.ProcessData(baseResult);")
				.IgnoringNewlineStyle().And
				.Contains("methodSetup?.TriggerCallbacks(baseResult);")
				.IgnoringNewlineStyle().And
				.Contains("return methodSetup?.TryGetReturnValue(baseResult, out var returnValue) == true ? returnValue : this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!, baseResult);")
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
				.Contains("Setup for the int property <see cref=\"global::MyCode.IMyBaseService.BaseProperty\">BaseProperty</see>.").And
				.Contains("Verify interactions with the int property <see cref=\"global::MyCode.IMyBaseService.BaseProperty\">BaseProperty</see>.").And
				.Contains("Setup for the int indexer <see cref=\"global::MyCode.IMyBaseService.this[string]\">this[string]</see>").And
				.Contains("Verify interactions with the int indexer <see cref=\"global::MyCode.IMyBaseService.this[string]\">this[string]</see>.").And
				.Contains("Setup for the method <see cref=\"global::MyCode.IMyBaseService.BaseMethod()\">BaseMethod()</see>.").And
				.Contains("Verify invocations for the method <see cref=\"global::MyCode.IMyBaseService.BaseMethod()\">BaseMethod()</see>.").And
				.Contains("Raise the <see cref=\"global::MyCode.IMyBaseService.BaseEvent\">BaseEvent</see> event.").And
				.Contains("Verify subscriptions on the BaseEvent event of <see cref=\"global::MyCode.IMyBaseService.BaseEvent\">BaseEvent</see>.");
		}

		[Fact]
		public async Task PlainInterface_ShouldOmitConditionalRemarkBullets()
		{
			GeneratorResult result = Generator
				.Run("""
				     using Mockolate;

				     namespace MyCode;

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				     		_ = IPlain.CreateMock();
				         }
				     }

				     public interface IPlain
				     {
				         int GetValue();
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IPlain.g.cs").WhoseValue
				.DoesNotContain("<c>Raise</c> - trigger events declared on the mocked type.").And
				.DoesNotContain("<c>SetupProtected</c> / <c>VerifyProtected</c> / <c>RaiseProtected</c>").And
				.DoesNotContain("<c>SetupStatic</c> / <c>VerifyStatic</c> / <c>RaiseStatic</c>").And
				.DoesNotContain("<c>.Mock.Raise</c> triggers events declared on the mocked type.");
		}

		[Fact]
		public async Task InterfaceWithEvents_ShouldIncludeRaiseRemarkBullet()
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
				     		_ = IHasEvent.CreateMock();
				         }
				     }

				     public interface IHasEvent
				     {
				         event EventHandler Changed;
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IHasEvent.g.cs").WhoseValue
				.Contains("<c>Raise</c> - trigger events declared on the mocked type.").And
				.Contains("<c>.Mock.Raise</c> triggers events declared on the mocked type.").And
				.DoesNotContain("<c>SetupProtected</c> / <c>VerifyProtected</c> / <c>RaiseProtected</c>").And
				.DoesNotContain("<c>SetupStatic</c> / <c>VerifyStatic</c> / <c>RaiseStatic</c>");
		}
	}
}
