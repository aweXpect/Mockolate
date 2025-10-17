namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed partial class ForMockTests
{
	[Fact]
	public async Task Delegates_ShouldCreateDelegateInsteadOfMockSubject()
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
			     		_ = Mock.Create<Func<int, bool>>();
			         }
			     }
			     """);

		await That(result.Sources).ContainsKey("ForFuncintbool.g.cs").WhoseValue
			.DoesNotContain("MockSubject").IgnoringNewlineStyle().And
			.Contains("Subject = new System.Func<int,bool>(").IgnoringNewlineStyle();
	}

	[Fact]
	public async Task CustomDelegates_ShouldOnlyCreateTwoExtensions()
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
			     		_ = Mock.Create<DoSomething>();
			         }

			         public delegate int DoSomething(int x, int y);
			     }
			     """);

		await That(result.Sources)
			.DoesNotContainKey("ForProgramDoSomething.SetupExtensions.g.cs").And
			.ContainsKey("ForProgramDoSomething.Extensions.g.cs").WhoseValue
				.Contains("""
				          		public ReturnMethodSetup<int, int, int> Delegate(With.Parameter<int>? x, With.Parameter<int>? y)
				          		{
				          			var methodSetup = new ReturnMethodSetup<int, int, int>("MyCode.Program.DoSomething.Invoke", new With.NamedParameter("x", x ?? With.Null<int>()), new With.NamedParameter("y", y ?? With.Null<int>()));
				          			if (setup is IMockSetup mockSetup)
				          			{
				          				mockSetup.RegisterMethod(methodSetup);
				          			}
				          			return methodSetup;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		public VerificationResult<MockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>> Invoked(With.Parameter<int>? x, With.Parameter<int>? y)
				          		{
				          			IMockInvoked<MockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>> invoked = new MockInvoked<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>(verify);
				          			return invoked.Method("MyCode.Program.DoSomething.Invoke", x ?? With.Null<int>(), y ?? With.Null<int>());
				          		}
				          """).IgnoringNewlineStyle();
	}
}
