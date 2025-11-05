namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed partial class ForMockTests
{
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
			          		public VerificationResult<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>> Invoked(With.Parameter<int>? x, With.Parameter<int>? y)
			          		{
			          			IMockInvoked<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>> invoked = (IMockInvoked<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>>)verify;
			          			return invoked.Method("MyCode.Program.DoSomething.Invoke", x ?? With.Null<int>(), y ?? With.Null<int>());
			          		}
			          """).IgnoringNewlineStyle();
	}

	[Fact]
	public async Task CustomVoidDelegates_ShouldOnlyCreateTwoExtensions()
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

			         public delegate void DoSomething(int x, ref int y, out int z);
			     }
			     """);

		await That(result.Sources)
			.DoesNotContainKey("ForProgramDoSomething.SetupExtensions.g.cs").And
			.ContainsKey("ForProgramDoSomething.Extensions.g.cs").WhoseValue
			.Contains("""
			          		public VoidMethodSetup<int, int, int> Delegate(With.Parameter<int>? x, With.RefParameter<int> y, With.OutParameter<int> z)
			          		{
			          			var methodSetup = new VoidMethodSetup<int, int, int>("MyCode.Program.DoSomething.Invoke", new With.NamedParameter("x", x ?? With.Null<int>()), new With.NamedParameter("y", y), new With.NamedParameter("z", z));
			          			if (setup is IMockSetup mockSetup)
			          			{
			          				mockSetup.RegisterMethod(methodSetup);
			          			}
			          			return methodSetup;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public VerificationResult<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>> Invoked(With.Parameter<int>? x, With.InvokedRefParameter<int> y, With.InvokedOutParameter<int> z)
			          		{
			          			IMockInvoked<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>> invoked = (IMockInvoked<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>>)verify;
			          			return invoked.Method("MyCode.Program.DoSomething.Invoke", x ?? With.Null<int>(), y, z);
			          		}
			          """).IgnoringNewlineStyle();
	}

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
			.Contains("_subject = new System.Func<int,bool>(").IgnoringNewlineStyle();
	}
}
