using System.Threading.Tasks;
using Verifier = Mockolate.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<Mockolate.Analyzers.WrappabilityAnalyzer>;

namespace Mockolate.Analyzers.Tests;

public class WrappabilityAnalyzerTests
{
	[Test]
	public async Task WhenWrappingAbstractClass_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}

			  namespace MyNamespace
			  {
			  		public abstract class MyAbstractClass
			  		{
			  				public abstract void DoSomething();
			  		}

			  		public class MyImplementation : MyAbstractClass
			  		{
			  				public override void DoSomething() { }
			  		}

			  		public class MyClass
			  		{
			  				public void MyTest()
			  				{
			  						MyImplementation instance = new MyImplementation();
			  						Mockolate.Mock.Wrap<{|#0:MyAbstractClass|}>(instance);
			  				}
			  		}
			  }
			  """,
			Verifier.Diagnostic(Rules.WrappabilityRule)
				.WithLocation(0)
				.WithArguments("MyNamespace.MyAbstractClass", "only interface types can be wrapped")
		);

	[Test]
	public async Task WhenWrappingADelegate_ShouldBeFlagged() => await Verifier
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
			  			System.Action instance = () => { };
			  			Mockolate.Mock.Wrap<{|#0:System.Action|}>(instance);
			  		}
			  	}
			  }
			  """,
			Verifier.Diagnostic(Rules.WrappabilityRule)
				.WithLocation(0)
				.WithArguments("System.Action", "only interface types can be wrapped")
		);

	[Test]
	public async Task WhenWrappingClass_ShouldBeFlagged() => await Verifier
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
			  			MyBaseClass instance = new MyBaseClass();
			  			Mockolate.Mock.Wrap<{|#0:MyBaseClass|}>(instance);
			  		}
			  	}
			  }
			  """,
			Verifier.Diagnostic(Rules.WrappabilityRule)
				.WithLocation(0)
				.WithArguments("MyNamespace.MyBaseClass", "only interface types can be wrapped")
		);

	[Test]
	public async Task WhenWrappingGenericInterface_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  using System;

			  {{GeneratedPrefix}}

			  namespace MyNamespace
			  {
			  	public interface IMyInterface<T>
			  	{
			  		void DoSomething(T value);
			  	}

			  	public class MyImplementation : IMyInterface<int>
			  	{
			  		public void DoSomething(int value) { }
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			MyImplementation instance = new MyImplementation();
			  			Mockolate.Mock.Wrap<IMyInterface<int>>(instance);
			  		}
			  	}
			  }
			  """
		);

	[Test]
	public async Task WhenWrappingInterface_ShouldNotBeFlagged() => await Verifier
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

			  	public class MyImplementation : IMyInterface
			  	{
			  		public void DoSomething() { }
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			MyImplementation instance = new MyImplementation();
			  			Mockolate.Mock.Wrap<IMyInterface>(instance);
			  		}
			  	}
			  }
			  """
		);

	[Test]
	public async Task WhenWrappingSealedClass_ShouldBeFlagged() => await Verifier
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
			  			MySealedClass instance = new MySealedClass();
			  			Mockolate.Mock.Wrap<{|#0:MySealedClass|}>(instance);
			  		}
			  	}
			  }
			  """,
			Verifier.Diagnostic(Rules.WrappabilityRule)
				.WithLocation(0)
				.WithArguments("MyNamespace.MySealedClass", "only interface types can be wrapped")
		);

	private const string GeneratedPrefix =
		"""
		namespace Mockolate
		{
			public static class Mock
			{
				[MockGenerator]
				public static T Wrap<T>(T instance) where T : class => default!;
			}

			[AttributeUsage(AttributeTargets.Method)]
			public class MockGeneratorAttribute : Attribute { }
		}
		""";
}
