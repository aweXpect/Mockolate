using System.Threading.Tasks;
using Xunit;
using Verifier = Mockolate.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<Mockolate.Analyzers.MockabilityAnalyzer>;

namespace Mockolate.Analyzers.Tests;

public class MockabilityAnalyzerTests
{
	[Fact]
	public async Task WhenMockingClass_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}

			  namespace MyNamespace
			  {
			  	public class MyBaseClass
			  	{
			  		public virtual void DoSomething() { }
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			Mockolate.Mock.Create<MyBaseClass>();
			  		}
			  	}
			  }
			  """
		);

	[Fact]
	public async Task WhenMockingDelegate_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}

			  namespace MyNamespace
			  {
			  	public delegate void MyDelegate();

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			Mockolate.Mock.Create<MyDelegate>();
			  		}
			  	}
			  }
			  """
		);

	[Fact]
	public async Task WhenMockingEnum_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}

			  namespace MyNamespace
			  {
			  	public enum MyEnum
			  	{
			  		Value1,
			  		Value2
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			Mockolate.Mock.Create<{|#0:MyEnum|}>();
			  		}
			  	}
			  }
			  """,
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("MyNamespace.MyEnum", "type is an enum")
		);

	[Fact]
	public async Task WhenMockingGlobalNamespaceType_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}

			  public interface IGlobalInterface
			  {
			  	void DoSomething();
			  }

			  public class MyClass
			  {
			  	public void MyTest()
			  	{
			  		Mockolate.Mock.Create<{|#0:IGlobalInterface|}>();
			  	}
			  }
			  """,
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("IGlobalInterface", "type is declared in the global namespace")
		);

	[Fact]
	public async Task WhenMockingInterface_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}
			    
			  namespace MyNamespace
			  {
			  	public interface IMyInterface
			  	{
			  		void DoSomething();
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			Mock.Create<IMyInterface>();
			  		}
			  	}
			  }
			  """
		);

	[Fact]
	public async Task WhenMockingRecord_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}
			    
			  namespace MyNamespace
			  {
			  	public record MyRecord(int Value);

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			Mockolate.Mock.Create<{|#0:MyRecord|}>();
			  		}
			  	}
			  }
			  """,
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("MyNamespace.MyRecord", "type is a record")
		);

	[Fact]
	public async Task WhenMockingSealedClass_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}

			  namespace MyNamespace
			  {
			  	public sealed class MySealedClass
			  	{
			  		public void DoSomething() { }
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			Mockolate.Mock.Create<{|#0:MySealedClass|}>();
			  		}
			  	}
			  }
			  """,
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("MyNamespace.MySealedClass", "type is sealed")
		);

	[Fact]
	public async Task WhenMockingStruct_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}


			  namespace MyNamespace
			  {
			  	public struct MyStruct
			  	{
			  		public int Value { get; set; }
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			Mockolate.Mock.Create<{|#0:MyStruct|}>();
			  		}
			  	}
			  }
			  """,
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("MyNamespace.MyStruct", "type is a struct")
		);

	private const string GeneratedPrefix =
		"""
		namespace Mockolate
		{
			public static class Mock
			{
				[MockGenerator]
				public static T Create<T>() => default!;
			}
			
			[AttributeUsage(AttributeTargets.Method)]
			public class MockGeneratorAttribute : Attribute { }
		}
		""";
}
