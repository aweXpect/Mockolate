using System.Net.Http;

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
			     			_ = MyService.CreateMock();
			             }
			         }

			         public sealed class MyService { }
			     }
			     """);

		await ThatAll(
			That(result.Sources.Keys).DoesNotContain("Mock.MyService.g.cs"),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WhenImplementingAdditionalClass_ShouldCreateCombinationMock()
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
			                 _ = MyService.CreateMock()
			                     .Implementing<IMyInterface1>()
			                     .Implementing<IMyInterface2>();
			             }
			         }

			     	public interface IMyInterface1 { }
			     	public class MyService { }
			     	public interface IMyInterface2 { }
			     }
			     """);

		await That(result.Sources.Keys).IsEqualTo([
			"Mock.g.cs",
			"MockBehaviorExtensions.g.cs",
			"Mock.IMyInterface1.g.cs",
			"Mock.IMyInterface2.g.cs",
			"Mock.MyService.g.cs",
			"Mock.MyService__IMyInterface1.g.cs",
			"Mock.MyService__IMyInterface1__IMyInterface2.g.cs",
		]).InAnyOrder();
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
			     			_ = IMyInterface.CreateMock();
			             }
			         }

			         public interface IMyInterface
			         {
			             void MyMethod(int v1, int v2, int v3, int v4, int v5, int v6, int v7, int v8, int v9, int v10, int v11, int v12, int v13, int v14, int v15, int v16, int v17);
			         }
			     }
			     """, typeof(DateTime), typeof(Task));

		await That(result.Sources).ContainsKey("ActionFunc.g.cs").WhoseValue
			.Contains(
				"public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, in T17>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17);")
			.And
			.Contains(
				"public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, in T17, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17);");
	}

	[Fact]
	public async Task WhenNamesConflict_ShouldBeDistinguishable()
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
			     			_ = IMyInt.CreateMock();
			     			_ = I.MyInt.CreateMock();
			     			_ = IMy<int>.CreateMock();
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
				"Mock.IMyInt.g.cs",
				"Mock.I_MyInt.g.cs",
				"Mock.IMy_int.g.cs",
			]).InAnyOrder().IgnoringCase(),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WhenNamesConflictForAdditionalClasses_ShouldBeDistinguishable()
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
			     			_ = I.CreateMock()
			     				.Implementing<IMyInt>()
			     				.Implementing<I.MyInt>()
			     				.Implementing<IMy<int>>();
			             }
			         }

			         public interface IMyInt { }

			         public class I
			         {
			     		public interface MyInt { }
			         }

			         public interface IMy<T> { }
			     }
			     """, typeof(HttpResponseMessage));

		await ThatAll(
			That(result.Sources.Keys).Contains([
				"Mock.I.g.cs",
				"Mock.IMyInt.g.cs",
				"Mock.I_MyInt.g.cs",
				"Mock.IMy_int.g.cs",
				"Mock.I__IMyInt__I_MyInt__IMy_int.g.cs",
				"Mock.I__IMyInt__I_MyInt.g.cs",
				"Mock.I__IMyInt.g.cs",
			]).InAnyOrder().IgnoringCase()
		);
	}

	[Fact]
	public async Task WhenNamesConflictForAdditionalClassesInDifferentNamespaces_ShouldBeDistinguishable()
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
			     			_ = IMyService.CreateMock()
			     				.Implementing<MyCode.IMyInt>();
			     			_ = IMyService.CreateMock()
			     				.Implementing<OtherNamespace.IMyInt>();
			             }
			         }

			         public interface IMyInt { }

			         public interface IMyService { }
			     }
			     namespace OtherNamespace
			     {
			         public interface IMyInt { }
			     }
			     """, typeof(HttpResponseMessage));

		await ThatAll(
			That(result.Sources.Keys).Contains([
				"Mock.IMyService.g.cs",
				"Mock.IMyInt.g.cs",
				"Mock.IMyInt_1.g.cs",
				"Mock.IMyService__IMyInt.g.cs",
				"Mock.IMyService__IMyInt_1.g.cs",
			]).InAnyOrder(),
			That(result.Sources["Mock.IMyService__IMyInt.g.cs"])
				.Contains("global::MyCode.IMyInt, IMockForIMyInt, IMockSetupForIMyInt, IMockVerifyForIMyInt,"),
			That(result.Sources["Mock.IMyService__IMyInt_1.g.cs"])
				.Contains("global::OtherNamespace.IMyInt, IMockForIMyInt_1, IMockSetupForIMyInt_1, IMockVerifyForIMyInt_1,"),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WhenSameTypeImplementsDifferentCombinationsOfSameInterface_ShouldGenerateAllCombinations()
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
			     			_ = IBaseInterface.CreateMock().Implementing<ICommonInterface>();
			     			_ = IBaseInterface.CreateMock().Implementing<ICommonInterface>().Implementing<IAdditionalInterface1>();
			     			_ = IBaseInterface.CreateMock().Implementing<IAdditionalInterface2>().Implementing<ICommonInterface>();
			     			_ = IBaseInterface.CreateMock().Implementing<IAdditionalInterface1>().Implementing<IAdditionalInterface2>().Implementing<ICommonInterface>();
			             }
			         }

			         public interface IBaseInterface { }
			         public interface ICommonInterface { }
			         public interface IAdditionalInterface1 { }
			         public interface IAdditionalInterface2 { }
			     }
			     """);

		await That(result.Sources)
			.ContainsKey("Mock.IBaseInterface__ICommonInterface.g.cs").And
			.ContainsKey("Mock.IBaseInterface__ICommonInterface__IAdditionalInterface1.g.cs").And
			.ContainsKey("Mock.IBaseInterface__IAdditionalInterface2__ICommonInterface.g.cs").And
			.ContainsKey("Mock.IBaseInterface__IAdditionalInterface1__IAdditionalInterface2__ICommonInterface.g.cs").And
			.ContainsKey("Mock.IBaseInterface__IAdditionalInterface1__IAdditionalInterface2.g.cs").And
			.ContainsKey("Mock.IBaseInterface__IAdditionalInterface1.g.cs");
	}

	[Fact]
	public async Task WhenUsingSetups_ShouldGenerateMocksAndExtensions()
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
			     			_ = IMyInterface.CreateMock(setup => setup.Method.MyMethod().Returns(42));
			             }
			         }

			         public interface IMyInterface
			         {
			             int MyMethod();
			         }
			     }
			     """, typeof(DateTime), typeof(Task), typeof(HttpResponseMessage));

		await That(result.Sources).HasCount().AtLeast(3);
		await That(result.Sources).ContainsKey("Mock.IMyInterface.g.cs");
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
			                 _ = IMyInterface1.CreateMock()
			                    .Implementing<MyService>()
			                    .Implementing<IMyInterface2>()
			                    .Implementing<MyOtherService>();
			             }
			         }

			     	public interface IMyInterface1 { }
			     	public class MyService { }
			     	public interface IMyInterface2 { }
			     	public class MyOtherService { }
			     }
			     """);

		await That(result.Sources.Keys).IsEqualTo([
			"Mock.g.cs",
			"MockBehaviorExtensions.g.cs",
			"Mock.IMyInterface1.g.cs",
		]).InAnyOrder();
	}

	[Fact]
	public async Task WithHttpClient_ShouldAlsoGenerateMockForHttpMessageHandler()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Net.Http;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;
			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = HttpClient.CreateMock();
			             }
			         }
			     }
			     """, typeof(HttpClient));

		await That(result.Sources).ContainsKey("Mock.HttpMessageHandler.g.cs").And
			.ContainsKey("Mock.HttpClient.g.cs");
	}

	[Fact]
	public async Task WhenUsingCreateMockFromNonMockolateNamespace_ShouldNotBeIncluded()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using MockolateExtensions;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			                 _ = new MyService().CreateMock();
			             }
			         }

			         public class MyService { }
			     }

			     namespace MockolateExtensions
			     {
			         public static class MockExtensions
			         {
			             public static T CreateMock<T>(this T value)
			                 where T : class
			                 => value;
			         }
			     }
			     """);

		await ThatAll(
			That(result.Sources.Keys).IsEqualTo([
				"Mock.g.cs",
				"MockBehaviorExtensions.g.cs",
			]).InAnyOrder().IgnoringCase(),
			That(result.Diagnostics).IsEmpty()
		);
	}
}
