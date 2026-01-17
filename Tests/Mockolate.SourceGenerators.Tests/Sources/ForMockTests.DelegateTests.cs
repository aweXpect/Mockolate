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

			         public delegate int DoSomething(int x, int y, out bool success);
			     }
			     """);

		await That(result.Sources)
			.ContainsKey("MockForProgramDoSomethingExtensions.g.cs").WhoseValue
			.Contains("""
			          		public IReturnMethodSetup<int, int, int, bool> Delegate(IParameter<int>? x, IParameter<int>? y, IOutParameter<bool> success)
			          		{
			          			var methodSetup = new ReturnMethodSetup<int, int, int, bool>("MyCode.Program.DoSomething.Invoke", new NamedParameter("x", (IParameter)(x ?? It.IsNull<int>())), new NamedParameter("y", (IParameter)(y ?? It.IsNull<int>())), new NamedParameter("success", (IParameter)(success)));
			          			CastToMockRegistrationOrThrow(setup).SetupMethod(methodSetup);
			          			return methodSetup;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public VerificationResult<MyCode.Program.DoSomething> Invoked(IParameter<int>? x, IParameter<int>? y, IVerifyOutParameter<bool> success)
			          			=> CastToMockOrThrow(verify).Method("MyCode.Program.DoSomething.Invoke", new NamedParameter("x", (IParameter)(x ?? It.IsNull<int>())), new NamedParameter("y", (IParameter)(y ?? It.IsNull<int>())), new NamedParameter("success", (IParameter)(success)));
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
			          		public IVoidMethodSetup<int, int, int> Delegate(IParameter<int>? x, IRefParameter<int> y, IOutParameter<int> z)
			          		{
			          			var methodSetup = new VoidMethodSetup<int, int, int>("MyCode.Program.DoSomething.Invoke", new NamedParameter("x", (IParameter)(x ?? It.IsNull<int>())), new NamedParameter("y", (IParameter)(y)), new NamedParameter("z", (IParameter)(z)));
			          			CastToMockRegistrationOrThrow(setup).SetupMethod(methodSetup);
			          			return methodSetup;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public VerificationResult<MyCode.Program.DoSomething> Invoked(IParameters parameters)
			          			=> CastToMockOrThrow(verify).Method("MyCode.Program.DoSomething.Invoke", parameters);
			          """).IgnoringNewlineStyle();
	}

	[Fact]
	public async Task CustomDelegates_ShouldSupportSpanAndReadOnlySpanParameters()
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
			     		_ = Mock.Create<DoSomething1>();
			     		_ = Mock.Create<DoSomething2>();
			         }
			     
			         public delegate Span<char> DoSomething1(int x);
			         public delegate ReadOnlySpan<char> DoSomething2(int x);
			     }
			     """);

		await That(result.Sources)
			.ContainsKey("MockForProgramDoSomething1.g.cs").WhoseValue
			.Contains("""
			          	public MyCode.Program.DoSomething1 Object => new(Invoke);
			          	private System.Span<char> Invoke(int x)
			          	{
			          		var result = _mock.Registrations.InvokeMethod<System.Span<char>>("MyCode.Program.DoSomething1.Invoke", p => _mock.Registrations.Behavior.DefaultValue.Generate(default(SpanWrapper<char>)!, () => _mock.Registrations.Behavior.DefaultValue.Generate(default(char)!), p), ("x", x));
			          		result.TriggerCallbacks(x);
			          		return result.Result;
			          	}
			          """).IgnoringNewlineStyle();
		await That(result.Sources)
			.ContainsKey("MockForProgramDoSomething2.g.cs").WhoseValue
			.Contains("""
			          	public MyCode.Program.DoSomething2 Object => new(Invoke);
			          	private System.ReadOnlySpan<char> Invoke(int x)
			          	{
			          		var result = _mock.Registrations.InvokeMethod<System.ReadOnlySpan<char>>("MyCode.Program.DoSomething2.Invoke", p => _mock.Registrations.Behavior.DefaultValue.Generate(default(ReadOnlySpanWrapper<char>)!, () => _mock.Registrations.Behavior.DefaultValue.Generate(default(char)!), p), ("x", x));
			          		result.TriggerCallbacks(x);
			          		return result.Result;
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

		await That(result.Sources).ContainsKey("MockForFuncintbool.g.cs").WhoseValue
			.Contains("_mock.Registrations.InvokeMethod<bool>(\"System.Func<int, bool>.Invoke\", p => _mock.Registrations.Behavior.DefaultValue.Generate(default(bool)!, p), (\"arg\", arg))")
			.IgnoringNewlineStyle().And
			.Contains("System.Func<int,bool> Object").IgnoringNewlineStyle();
	}

	[Fact]
	public async Task DelegateWithParameterNamedResult_ShouldGenerateUniqueLocalVariableName()
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
			     		_ = Mock.Create<ProcessResult>();
			         }

			         public delegate int ProcessResult(int result);
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForProgramProcessResult.g.cs").WhoseValue
			.Contains("var result1 = _mock.Registrations.InvokeMethod<int>(")
			.IgnoringNewlineStyle().And
			.Contains("result1.TriggerCallbacks(result)")
			.IgnoringNewlineStyle().And
			.Contains("return result1.Result;")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task VoidDelegateWithParameterNamedResult_ShouldGenerateUniqueLocalVariableName()
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
			     		_ = Mock.Create<ProcessResult>();
			         }

			         public delegate void ProcessResult(string result, out int value);
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForProgramProcessResult.g.cs").WhoseValue
			.Contains("var result1 = _mock.Registrations.InvokeMethod(")
			.IgnoringNewlineStyle().And
			.Contains("value = result1.SetOutParameter<int>(\"value\", () => _mock.Registrations.Behavior.DefaultValue.Generate(default(int)!));")
			.IgnoringNewlineStyle().And
			.Contains("result1.TriggerCallbacks(result, value)")
			.IgnoringNewlineStyle();
	}
}
