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
			.ContainsKey("MockForProgramDoSomethingExtensions.g.cs").WhoseValue
			.Contains("""
			          		public ReturnMethodSetup<int, int, int> Invoke(Match.IParameter<int>? x, Match.IParameter<int>? y)
			          		{
			          			var methodSetup = new ReturnMethodSetup<int, int, int>("MyCode.Program.DoSomething.Invoke", new Match.NamedParameter("x", x ?? Match.Null<int>()), new Match.NamedParameter("y", y ?? Match.Null<int>()));
			          			CastToMockRegistrationOrThrow(setup).SetupMethod(methodSetup);
			          			return methodSetup;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public VerificationResult<MyCode.Program.DoSomething> Invoke(Match.IParameter<int>? x, Match.IParameter<int>? y)
			          			=> CastToMockOrThrow(verify).Method("MyCode.Program.DoSomething.Invoke", x ?? Match.Null<int>(), y ?? Match.Null<int>());
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
			.ContainsKey("MockForProgramDoSomethingExtensions.g.cs").WhoseValue
			.Contains("""
			          		public VoidMethodSetup<int, int, int> Invoke(Match.IParameter<int>? x, Match.IRefParameter<int> y, Match.IOutParameter<int> z)
			          		{
			          			var methodSetup = new VoidMethodSetup<int, int, int>("MyCode.Program.DoSomething.Invoke", new Match.NamedParameter("x", x ?? Match.Null<int>()), new Match.NamedParameter("y", y), new Match.NamedParameter("z", z));
			          			CastToMockRegistrationOrThrow(setup).SetupMethod(methodSetup);
			          			return methodSetup;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public VerificationResult<MyCode.Program.DoSomething> Invoke(Match.IParameters parameters)
			          			=> CastToMockOrThrow(verify).Method("MyCode.Program.DoSomething.Invoke", parameters);
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

		await That(result.Sources).ContainsKey("MockForFuncintbool.g.cs").WhoseValue
			.Contains("_mock.Registrations.InvokeMethod<bool>(\"System.Func<int, bool>.Invoke\", arg)")
			.IgnoringNewlineStyle().And
			.Contains("System.Func<int,bool> Object").IgnoringNewlineStyle();
	}
}
