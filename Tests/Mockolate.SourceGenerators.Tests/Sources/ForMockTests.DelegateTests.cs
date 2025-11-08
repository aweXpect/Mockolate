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
			          		public ReturnMethodSetup<int, int, int> Delegate(Match.IParameter<int>? x, Match.IParameter<int>? y)
			          		{
			          			var methodSetup = new ReturnMethodSetup<int, int, int>("MyCode.Program.DoSomething.Invoke", new Match.NamedParameter("x", x ?? Match.Null<int>()), new Match.NamedParameter("y", y ?? Match.Null<int>()));
			          			if (setup is IMockSetup mockSetup)
			          			{
			          				mockSetup.RegisterMethod(methodSetup);
			          			}
			          			return methodSetup;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public VerificationResult<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>> Invoked(Match.IParameter<int>? x, Match.IParameter<int>? y)
			          		{
			          			IMockInvoked<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>> invoked = (IMockInvoked<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>>)verify;
			          			return invoked.Method("MyCode.Program.DoSomething.Invoke", x ?? Match.Null<int>(), y ?? Match.Null<int>());
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
			          		public VoidMethodSetup<int, int, int> Delegate(Match.IParameter<int>? x, Match.IRefParameter<int> y, Match.IOutParameter<int> z)
			          		{
			          			var methodSetup = new VoidMethodSetup<int, int, int>("MyCode.Program.DoSomething.Invoke", new Match.NamedParameter("x", x ?? Match.Null<int>()), new Match.NamedParameter("y", y), new Match.NamedParameter("z", z));
			          			if (setup is IMockSetup mockSetup)
			          			{
			          				mockSetup.RegisterMethod(methodSetup);
			          			}
			          			return methodSetup;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public VerificationResult<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>> Invoked(Match.IParameter<int>? x, Match.IVerifyRefParameter<int> y, Match.IVerifyOutParameter<int> z)
			          		{
			          			IMockInvoked<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>> invoked = (IMockInvoked<IMockVerify<MyCode.Program.DoSomething, Mock<MyCode.Program.DoSomething>>>)verify;
			          			return invoked.Method("MyCode.Program.DoSomething.Invoke", x ?? Match.Null<int>(), y, z);
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
