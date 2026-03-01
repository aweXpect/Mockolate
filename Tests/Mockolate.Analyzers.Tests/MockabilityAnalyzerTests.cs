using System.Threading.Tasks;
using Verifier = Mockolate.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<Mockolate.Analyzers.MockabilityAnalyzer>;

namespace Mockolate.Analyzers.Tests;

public class MockabilityAnalyzerTests
{
	[Test]
	public async Task WhenMockingADelegate_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}
			    
			  namespace MyNamespace
			  {
			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			Mock.Create<System.Action>();
			  		}
			  	}
			  }
			  """
		);

	[Test]
	public async Task WhenMockingArray_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}

			  namespace MyNamespace
			  {
			  	public interface IMyInterface
			  	{
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			Mockolate.Mock.Create<{|#0:IMyInterface[]|}>();
			  		}
			  	}
			  }
			  """,
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("MyNamespace.IMyInterface[]", "type kind 'Array' is not supported")
		);

	[Test]
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

	[Test]
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

	[Test]
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

	[Test]
	public async Task WhenMockingGlobalNamespaceType_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}

			  public interface IGlobalInterface
			  {
			  	void DoSomething();
			  }

			  namespace MyNamespace
			  {
			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			Mockolate.Mock.Create<{|#0:IGlobalInterface|}>();
			  		}
			  	}
			  }
			  """,
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("IGlobalInterface", "type is declared in the global namespace")
		);

	[Test]
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

	[Test]
	public async Task WhenMockingMockGeneratorWithoutAttributes_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}

			  namespace MyNamespace
			  {
			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			Mockolate.MyMock.Create();
			  		}
			  	}
			  }
			  namespace Mockolate
			  {
			      public interface IGlobalInterface
			      {
			      	void DoSomething();
			      }

			      [AttributeUsage(AttributeTargets.Method)]
			      internal class MockGeneratorAttribute : Attribute
			      {
			      }
			      
			      public static class MyMock
			      {
			          [MockGenerator]
			          public static IGlobalInterface Create() => default!;
			      }
			  }
			  """
		);

	[Test]
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

	[Test]
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

	[Test]
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
