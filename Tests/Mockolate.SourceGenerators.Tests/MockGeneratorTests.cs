using System.Net.Http;
using System.Text.RegularExpressions;

namespace Mockolate.SourceGenerators.Tests;

public partial class MockGeneratorTests
{
	[GeneratedRegex(@"CreateMock\(global::Mockolate\.MockBehavior\s+\w+\)")]
	private static partial Regex CreateMockBehaviorSignatureRegex();


	[Fact]
	public async Task SameMethodDifferingOnlyByNullability_ShouldUseExplicitImplementationForConflictingInterface()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     #nullable enable
			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = IInterface1.CreateMock().Implementing<IInterface2>();
			         }
			     }

			     public interface IInterface1
			     {
			         void Method(string? value);
			     }

			     public interface IInterface2
			     {
			         void Method(string value);
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();

		await That(result.Sources).ContainsKey("Mock.IInterface1__IInterface2.g.cs").WhoseValue
			.Contains("public void Method(string? value)").And
			.Contains("void global::MyCode.IInterface2.Method(string value)");
	}

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
	public async Task WhenImplementingAdditionalInterface_WithBaseClassHavingOptionalConstructorParameter_ShouldGenerateTryCastWithDefaultValue()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock().Implementing<IMyInterface>();
			         }
			     }

			     public class MyService
			     {
			         public MyService(int value = 0) { }
			     }

			     public interface IMyInterface
			     {
			         void DoWork();
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();

		await That(result.Sources).ContainsKey("Mock.MyService__IMyInterface.g.cs").WhoseValue
			.Contains("static bool TryCastWithDefaultValue<TValue>(object?[] values, int index, TValue defaultValue, global::Mockolate.MockBehavior behavior, out TValue result)")
			.IgnoringNewlineStyle().And
			.Contains("mock.MockRegistry.ConstructorParameters.Length >= 0 && mock.MockRegistry.ConstructorParameters.Length <= 1")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenImplementingAdditionalInterface_WithBaseClassHavingRequiredConstructorParameter_ShouldGenerateTryCast()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock([42]).Implementing<IMyInterface>();
			         }
			     }

			     public class MyService
			     {
			         public MyService(int value) { }
			     }

			     public interface IMyInterface
			     {
			         void DoWork();
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();

		await That(result.Sources).ContainsKey("Mock.MyService__IMyInterface.g.cs").WhoseValue
			.Contains("static bool TryCast<TValue>(object?[] values, int index, global::Mockolate.MockBehavior behavior, out TValue result)")
			.IgnoringNewlineStyle().And
			.Contains("No parameterless constructor found for 'MyCode.MyService'")
			.IgnoringNewlineStyle().And
			.Contains("mock.MockRegistry.ConstructorParameters.Length == 1")
			.IgnoringNewlineStyle().And
			.DoesNotContain("TryCastWithDefaultValue")
			.IgnoringNewlineStyle();
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
	public async Task WhenClassHasConstructorWithParameters_ShouldGenerateTypedCreateMockOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock(42);
			         }
			     }

			     public class MyService
			     {
			         public MyService(int value) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("public static global::MyCode.MyService CreateMock(int value)")
			.IgnoringNewlineStyle().And
			.Contains("=> CreateMock(null, null, new object?[] { value });")
			.IgnoringNewlineStyle().And
			.Contains("public static global::MyCode.MyService CreateMock(global::Mockolate.MockBehavior mockBehavior, int value)")
			.IgnoringNewlineStyle().And
			.Contains("public static global::MyCode.MyService CreateMock(global::System.Action<global::Mockolate.Mock.IMockSetupForMyService> setup, int value)")
			.IgnoringNewlineStyle().And
			.Contains("public static global::MyCode.MyService CreateMock(global::Mockolate.MockBehavior mockBehavior, global::System.Action<global::Mockolate.Mock.IMockSetupForMyService> setup, int value)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenClassHasMultipleConstructors_ShouldGenerateTypedOverloadPerConstructor()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock("foo");
			     		_ = MyService.CreateMock(1, "foo");
			         }
			     }

			     public class MyService
			     {
			         public MyService(string text) { }
			         public MyService(int number, string text) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("public static global::MyCode.MyService CreateMock(string text)")
			.IgnoringNewlineStyle().And
			.Contains("public static global::MyCode.MyService CreateMock(int number, string text)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorHasRefOrParamsParameter_ShouldNotEmitTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock([1, 2, 3]);
			         }
			     }

			     public class MyService
			     {
			         public MyService(params int[] values) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.DoesNotContain("public static global::MyCode.MyService CreateMock(int[] values)")
			.IgnoringNewlineStyle().And
			.DoesNotContain("public static global::MyCode.MyService CreateMock(params int[] values)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenClassHasOnlyParameterlessConstructor_ShouldNotEmitAdditionalTypedOverloads()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock();
			     	}
			     }

			     public class MyService { }
			     """);

		await That(result.Diagnostics).IsEmpty();
		// No typed overloads beyond the existing hand-written ones; the existing parameterless
		// CreateMock() overload is still there and no typed overload is emitted for it.
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("public static global::MyCode.MyService CreateMock()")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorHasStringDefaultWithQuotes_ShouldEscapeInTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock();
			         }
			     }

			     public class MyService
			     {
			         public MyService(string text = "has \"quotes\"") { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("CreateMock(string text = \"has \\\"quotes\\\"\")")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorHasCharDefault_ShouldEscapeInTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock();
			         }
			     }

			     public class MyService
			     {
			         public MyService(char c = '\n') { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("CreateMock(char c = '\\n')")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorHasEnumDefault_ShouldCastToFullyQualifiedEnumTypeInTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public enum MyKind { Alpha, Beta }

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock();
			         }
			     }

			     public class MyService
			     {
			         public MyService(MyKind kind = MyKind.Beta) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("CreateMock(global::MyCode.MyKind kind = (global::MyCode.MyKind)1)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorHasNullableReferenceDefaultNull_ShouldEmitNullLiteralInTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     #nullable enable
			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock();
			         }
			     }

			     public class MyService
			     {
			         public MyService(string? text = null) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("CreateMock(string? text = null)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorHasValueTypeDefaultNull_ShouldEmitDefaultInTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public struct MyToken { public int Value; }

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock();
			         }
			     }

			     public class MyService
			     {
			         public MyService(MyToken token = default) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("CreateMock(global::MyCode.MyToken token = default)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorsDifferOnlyByNullableValueType_ShouldEmitBothTypedOverloads()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     #nullable enable
			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock(1);
			     		_ = MyService.CreateMock((int?)1);
			         }
			     }

			     public class MyService
			     {
			         public MyService(int value) { }
			         public MyService(int? value) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("public static global::MyCode.MyService CreateMock(int value)")
			.IgnoringNewlineStyle().And
			.Contains("public static global::MyCode.MyService CreateMock(int? value)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorParameterIsMockBehavior_ShouldNotEmitAmbiguousTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock(Mockolate.MockBehavior.Default);
			         }
			     }

			     public class MyService
			     {
			         public MyService(Mockolate.MockBehavior behavior) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		// The hand-written CreateMock(MockBehavior) overload already covers this signature.
		// A typed single-parameter overload with the same signature would produce a duplicate
		// definition, so the generator must skip it.
		int matches = CreateMockBehaviorSignatureRegex().Count(result.Sources["Mock.MyService.g.cs"]);
		await That(matches).IsEqualTo(1);
	}

	[Fact]
	public async Task WhenConstructorParameterIsUnrelatedAction_ShouldEmitTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;
			     using System;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		Action<string> callback = s => { };
			     		_ = MyService.CreateMock(callback);
			         }
			     }

			     public class MyService
			     {
			         public MyService(Action<string> callback) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		// Action<string> does not collide with the setup action type (Action<IMockSetupForMyService>),
		// so the generator should emit the typed overload.
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("CreateMock(global::System.Action<string> callback)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorHasInParameter_ShouldNotEmitTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		int value = 42;
			     		_ = MyService.CreateMock([value]);
			         }
			     }

			     public class MyService
			     {
			         public MyService(in int value) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.DoesNotContain("CreateMock(in int value)")
			.IgnoringNewlineStyle().And
			.DoesNotContain("CreateMock(int value)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenGenericClassHasTypedConstructor_ShouldEmitTypedOverloadForClosedType()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService<string>.CreateMock("foo");
			         }
			     }

			     public class MyService<T>
			     {
			         public MyService(T value) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		// The generator emits a mock for the closed generic MyService<string>. Locate the generated
		// file and assert the typed overload was emitted with the substituted type argument.
		string generated = string.Concat(result.Sources.Values);
		await That(generated)
			.Contains("CreateMock(string value)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenDerivedClassHasBaseClassConstructorParameters_ShouldEmitTypedOverloadForDerived()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = DerivedService.CreateMock("foo");
			         }
			     }

			     public class BaseService
			     {
			         public BaseService(string text) { }
			     }

			     public class DerivedService : BaseService
			     {
			         public DerivedService(string text) : base(text) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.DerivedService.g.cs").WhoseValue
			.Contains("public static global::MyCode.DerivedService CreateMock(string text)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorHasDecimalDefault_ShouldAppendMSuffixInTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock();
			         }
			     }

			     public class MyService
			     {
			         public MyService(decimal price = 19.95m) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("CreateMock(decimal price = 19.95m)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorHasFloatDefault_ShouldAppendFSuffixInTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock();
			         }
			     }

			     public class MyService
			     {
			         public MyService(float factor = 3.14f) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("CreateMock(float factor = 3.14f)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorHasNullableDecimalDefault_ShouldAppendMSuffixInTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     #nullable enable
			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock();
			         }
			     }

			     public class MyService
			     {
			         public MyService(decimal? price = 19.95m) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("CreateMock(decimal? price = 19.95m)")
			.IgnoringNewlineStyle();
	}

	[Fact]
	public async Task WhenConstructorHasLongAndDoubleDefaults_ShouldPreserveLiteralsInTypedOverload()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock();
			         }
			     }

			     public class MyService
			     {
			         public MyService(long big = 9999999999L, double pi = 3.14) { }
			     }
			     """);

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.Contains("CreateMock(long big = 9999999999, double pi = 3.14)")
			.IgnoringNewlineStyle();
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
}
