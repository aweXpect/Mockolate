namespace Mockolate.SourceGenerators.Tests;

public class MockGeneratorTests
{
	[Fact]
	public async Task SealedClass_ShouldNotBeIncluded()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<MyService>();
			             }
			         }

			         public sealed class MyService { }
			     }
			     """);

		await ThatAll(
			That(result.Sources.Keys).IsEqualTo([
				"Mock.g.cs",
				"MockGeneratorAttribute.g.cs",
				"MockRegistration.g.cs",
			]).InAnyOrder().IgnoringCase(),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WhenMethodContainsMoreThan16Parameters_ShouldAddCustomAction()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyInterface>();
			             }
			         }

			         public interface IMyInterface
			         {
			             void MyMethod(int v1, int v2, int v3, int v4, int v5, int v6, int v7, int v8, int v9, int v10, int v11, int v12, int v13, int v14, int v15, int v16, int v17);
			         }
			     }
			     """, typeof(DateTime), typeof(Task));

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("MethodSetups.ActionFunc.g.cs").WhoseValue
			.Contains(
				"public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, in T17>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17);")
			.And
			.Contains(
				"public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, in T17, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17);");
	}

	[Fact]
	public async Task WhenNamesConflict_ShouldAppendAnIndex()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyInt>();
			     			_ = Mock.Create<I.MyInt>();
			     			_ = Mock.Create<IMy<int>>();
			             }
			         }

			         public interface IMyInt { }

			         public class I
			         {
			     		public interface MyInt { }
			         }

			         public interface IMy<T> { }
			     }
			     """);

		await ThatAll(
			That(result.Sources.Keys).Contains([
				"MockForIMyInt.g.cs",
				"MockForIMyInt_1.g.cs",
				"MockForIMyInt_2.g.cs",
				"MockForIMyIntExtensions.g.cs",
				"MockForIMyInt_1Extensions.g.cs",
				"MockForIMyInt_2Extensions.g.cs",
			]).InAnyOrder().IgnoringCase(),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WhenNamesConflictForAdditionalClasses_ShouldAppendAnIndex()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<I, IMyInt, I.MyInt, IMy<int>>();
			             }
			         }

			         public interface IMyInt { }

			         public class I
			         {
			     		public interface MyInt { }
			         }

			         public interface IMy<T> { }
			     }
			     """);

		await ThatAll(
			That(result.Sources.Keys).Contains([
				"MockForI_IMyInt_IMyInt_IMyint.g.cs",
				"MockForI_IMyInt_IMyInt_IMyintExtensions.g.cs",
			]).InAnyOrder().IgnoringCase(),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WhenUsingMockCreateFromOtherNamespace_ShouldNotBeIncluded()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyInterface>();
			             }
			         }

			         public interface IMyInterface { }

			         public class Mock
			         {
			     		public static Mock<T> Create<T>() => new Mock<T>();
			         }

			         public class Mock<T>{ }
			     }
			     """);

		await ThatAll(
			That(result.Sources.Keys).IsEqualTo([
				"Mock.g.cs",
				"MockGeneratorAttribute.g.cs",
				"MockRegistration.g.cs",
			]).InAnyOrder().IgnoringCase(),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WhenUsingMockFactory_ShouldGenerateMocksAndExtensions()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			                var factory = new Mock.Factory(MockBehavior.Default);
			     			_ = factory.Create<IMyInterface>();
			             }
			         }

			         public interface IMyInterface
			         {
			             void MyMethod(int v1, bool v2, double v3, long v4, uint v5, string v6, DateTime v7);
			         }
			     }
			     """, typeof(DateTime), typeof(Task));

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).HasCount().AtLeast(5);
		await That(result.Sources).ContainsKey("MockForIMyInterface.g.cs");
		await That(result.Sources).ContainsKey("MockForIMyInterfaceExtensions.g.cs");
	}

	[Fact]
	public async Task WithClassAsAdditionalImplementation_ShouldNotThrow()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyInterface1, MyService, IMyInterface2, MyOtherService>();
			             }
			         }

			     	public interface IMyInterface1 { }
			     	public class MyService { }
			     	public interface IMyInterface2 { }
			     	public class MyOtherService { }
			     }
			     """);

		await ThatAll(
			That(result.Sources.Keys).IsEqualTo([
				"Mock.g.cs",
				"MockGeneratorAttribute.g.cs",
				"MockForIMyInterface1Extensions.g.cs",
				"MockForMyServiceExtensions.g.cs",
				"MockForIMyInterface2Extensions.g.cs",
				"MockForMyOtherServiceExtensions.g.cs",
				"MockRegistration.g.cs",
			]).InAnyOrder(),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WithCustomGenerator_ShouldWork()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			                 MyGenerator.Create<IMyInterface>();
			             }
			         }

			         public static class MyGenerator
			         {
			             [MockGenerator]
			             public static void Create<T>()
			                 where T : class
			             {
			                 _ = Mock.Create<T>();
			             }
			         }

			         public interface IMyInterface
			         {
			             void MyMethod(int v1, bool v2, double v3, long v4, uint v5, string v6, DateTime v7);
			         }
			     }
			     """, typeof(DateTime), typeof(Task));

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).HasCount().AtLeast(5);
		await That(result.Sources).ContainsKey("MockForIMyInterface.g.cs");
		await That(result.Sources).ContainsKey("MockForIMyInterfaceExtensions.g.cs");
	}
}
