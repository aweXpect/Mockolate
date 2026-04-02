namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed class DelegateTests
	{
		[Fact]
		public async Task CustomDelegates_ShouldOnlyCreateTwoExtensionsForSetupAndVerify()
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
				     		_ = DoSomething.CreateMock();
				         }

				         public delegate int DoSomething(int x, int y, out bool success);
				     }
				     """);

			await That(result.Sources)
				.ContainsKey("Mock.Program_DoSomething.g.cs").WhoseValue
				.Contains("""
				          		global::Mockolate.Setup.IReturnMethodSetup<int, int, int, bool> global::Mockolate.Mock.IMockSetupForProgram_DoSomething.Setup(global::Mockolate.Parameters.IParameter<int>? x, global::Mockolate.Parameters.IParameter<int>? y, global::Mockolate.Parameters.IOutParameter<bool> success)
				          		{
				          			var methodSetup = new global::Mockolate.Setup.ReturnMethodSetup<int, int, int, bool>("global::MyCode.Program.DoSomething.Invoke", new global::Mockolate.Parameters.NamedParameter("x", (global::Mockolate.Parameters.IParameter)(x ?? global::Mockolate.It.IsNull<int>("null"))), new global::Mockolate.Parameters.NamedParameter("y", (global::Mockolate.Parameters.IParameter)(y ?? global::Mockolate.It.IsNull<int>("null"))), new global::Mockolate.Parameters.NamedParameter("success", (global::Mockolate.Parameters.IParameter)(success)));
				          			this.MockRegistry.SetupMethod(methodSetup);
				          			return methodSetup;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		global::Mockolate.Setup.IReturnMethodSetup<int, int, int, bool> global::Mockolate.Mock.IMockSetupForProgram_DoSomething.Setup(global::Mockolate.Parameters.IParameters parameters)
				          		{
				          			var methodSetup = new global::Mockolate.Setup.ReturnMethodSetup<int, int, int, bool>("global::MyCode.Program.DoSomething.Invoke", parameters);
				          			this.MockRegistry.SetupMethod(methodSetup);
				          			return methodSetup;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		global::Mockolate.Verify.VerificationResult<IMockVerifyForProgram_DoSomething> IMockVerifyForProgram_DoSomething.Verify(global::Mockolate.Parameters.IParameter<int>? x, global::Mockolate.Parameters.IParameter<int>? y, global::Mockolate.Parameters.IVerifyOutParameter<bool> success)
				          			=> this.MockRegistry.Method<IMockVerifyForProgram_DoSomething>(this, new global::Mockolate.Setup.MethodParameterMatch("global::MyCode.Program.DoSomething.Invoke", [ new global::Mockolate.Parameters.NamedParameter("x", (global::Mockolate.Parameters.IParameter)(x ?? global::Mockolate.It.IsNull<int>("null"))), new global::Mockolate.Parameters.NamedParameter("y", (global::Mockolate.Parameters.IParameter)(y ?? global::Mockolate.It.IsNull<int>("null"))), new global::Mockolate.Parameters.NamedParameter("success", (global::Mockolate.Parameters.IParameter)(success)), ]));
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		global::Mockolate.Verify.VerificationResult<IMockVerifyForProgram_DoSomething> IMockVerifyForProgram_DoSomething.Verify(global::Mockolate.Parameters.IParameters parameters)
				          			=> this.MockRegistry.Method<IMockVerifyForProgram_DoSomething>(this, new global::Mockolate.Setup.MethodParametersMatch("global::MyCode.Program.DoSomething.Invoke", parameters));
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
				     		_ = DoSomething1.CreateMock();
				     		_ = DoSomething2.CreateMock();
				         }

				         public delegate Span<char> DoSomething1(int x);
				         public delegate ReadOnlySpan<char> DoSomething2(int x);
				     }
				     """);

			await That(result.Sources)
				.ContainsKey("Mock.Program_DoSomething1.g.cs").WhoseValue
				.Contains("""
				          		public global::MyCode.Program.DoSomething1 Object => new(Invoke);
				          		private global::System.Span<char> Invoke(int x)
				          		{
				          			var result = this.MockRegistry.InvokeMethod<global::System.Span<char>>("global::MyCode.Program.DoSomething1.Invoke", p => this.MockRegistry.Behavior.DefaultValue.Generate(default(global::Mockolate.Setup.SpanWrapper<char>)!, () => this.MockRegistry.Behavior.DefaultValue.Generate(default(char)!), p), new global::Mockolate.Parameters.NamedParameterValue("x", x));
				          			result.TriggerCallbacks(x);
				          			return result.Result;
				          		}
				          """).IgnoringNewlineStyle();
			await That(result.Sources)
				.ContainsKey("Mock.Program_DoSomething2.g.cs").WhoseValue
				.Contains("""
				          		public global::MyCode.Program.DoSomething2 Object => new(Invoke);
				          		private global::System.ReadOnlySpan<char> Invoke(int x)
				          		{
				          			var result = this.MockRegistry.InvokeMethod<global::System.ReadOnlySpan<char>>("global::MyCode.Program.DoSomething2.Invoke", p => this.MockRegistry.Behavior.DefaultValue.Generate(default(global::Mockolate.Setup.ReadOnlySpanWrapper<char>)!, () => this.MockRegistry.Behavior.DefaultValue.Generate(default(char)!), p), new global::Mockolate.Parameters.NamedParameterValue("x", x));
				          			result.TriggerCallbacks(x);
				          			return result.Result;
				          		}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task CustomVoidDelegates_ShouldOnlyCreateTwoExtensionsForSetupAndVerify()
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
				     		_ = DoSomething.CreateMock();
				         }

				         public delegate void DoSomething(int x, ref int y, out int z);
				     }
				     """);

			await That(result.Sources)
				.ContainsKey("Mock.Program_DoSomething.g.cs").WhoseValue
				.Contains("""
				          		global::Mockolate.Setup.IVoidMethodSetup<int, int, int> global::Mockolate.Mock.IMockSetupForProgram_DoSomething.Setup(global::Mockolate.Parameters.IParameter<int>? x, global::Mockolate.Parameters.IRefParameter<int> y, global::Mockolate.Parameters.IOutParameter<int> z)
				          		{
				          			var methodSetup = new global::Mockolate.Setup.VoidMethodSetup<int, int, int>("global::MyCode.Program.DoSomething.Invoke", new global::Mockolate.Parameters.NamedParameter("x", (global::Mockolate.Parameters.IParameter)(x ?? global::Mockolate.It.IsNull<int>("null"))), new global::Mockolate.Parameters.NamedParameter("y", (global::Mockolate.Parameters.IParameter)(y)), new global::Mockolate.Parameters.NamedParameter("z", (global::Mockolate.Parameters.IParameter)(z)));
				          			this.MockRegistry.SetupMethod(methodSetup);
				          			return methodSetup;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		global::Mockolate.Setup.IVoidMethodSetup<int, int, int> global::Mockolate.Mock.IMockSetupForProgram_DoSomething.Setup(global::Mockolate.Parameters.IParameters parameters)
				          		{
				          			var methodSetup = new global::Mockolate.Setup.VoidMethodSetup<int, int, int>("global::MyCode.Program.DoSomething.Invoke", parameters);
				          			this.MockRegistry.SetupMethod(methodSetup);
				          			return methodSetup;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		global::Mockolate.Verify.VerificationResult<IMockVerifyForProgram_DoSomething> IMockVerifyForProgram_DoSomething.Verify(global::Mockolate.Parameters.IParameter<int>? x, global::Mockolate.Parameters.IVerifyRefParameter<int> y, global::Mockolate.Parameters.IVerifyOutParameter<int> z)
				          			=> this.MockRegistry.Method<IMockVerifyForProgram_DoSomething>(this, new global::Mockolate.Setup.MethodParameterMatch("global::MyCode.Program.DoSomething.Invoke", [ new global::Mockolate.Parameters.NamedParameter("x", (global::Mockolate.Parameters.IParameter)(x ?? global::Mockolate.It.IsNull<int>("null"))), new global::Mockolate.Parameters.NamedParameter("y", (global::Mockolate.Parameters.IParameter)(y)), new global::Mockolate.Parameters.NamedParameter("z", (global::Mockolate.Parameters.IParameter)(z)), ]));
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		global::Mockolate.Verify.VerificationResult<IMockVerifyForProgram_DoSomething> IMockVerifyForProgram_DoSomething.Verify(global::Mockolate.Parameters.IParameters parameters)
				          			=> this.MockRegistry.Method<IMockVerifyForProgram_DoSomething>(this, new global::Mockolate.Setup.MethodParametersMatch("global::MyCode.Program.DoSomething.Invoke", parameters));
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
				     		_ = Func<int, bool>.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.Func_int_bool.g.cs").WhoseValue
				.Contains("this.MockRegistry.InvokeMethod<bool>(\"global::System.Func<int, bool>.Invoke\", p => this.MockRegistry.Behavior.DefaultValue.Generate(default(bool)!, p), new global::Mockolate.Parameters.NamedParameterValue(\"arg\", arg));")
				.IgnoringNewlineStyle().And
				.Contains("global::System.Func<int, bool> Object").IgnoringNewlineStyle();
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
				     		_ = ProcessResult.CreateMock();
				         }

				         public delegate int ProcessResult(int result);
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.Program_ProcessResult.g.cs").WhoseValue
				.Contains("var result1 = this.MockRegistry.InvokeMethod<int>(")
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
				     		_ = ProcessResult.CreateMock();
				         }

				         public delegate void ProcessResult(string result, out int value);
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.Program_ProcessResult.g.cs").WhoseValue
				.Contains("var result1 = this.MockRegistry.InvokeMethod(")
				.IgnoringNewlineStyle().And
				.Contains("value = result1.SetOutParameter<int>(\"value\", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));")
				.IgnoringNewlineStyle().And
				.Contains("result1.TriggerCallbacks(result, value)")
				.IgnoringNewlineStyle();
		}
	}
}
