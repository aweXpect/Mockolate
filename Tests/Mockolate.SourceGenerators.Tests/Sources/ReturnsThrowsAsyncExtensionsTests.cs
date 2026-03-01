using System.Threading;

namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed class ReturnsThrowsAsyncExtensionsTests
{
	[Test]
	public async Task ForVoidMethods_ShouldNotGenerateReturnsAsyncExtensions()
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
			             void MyMethod(int v1, bool v2, double v3, long v4, CancellationToken v5);
			         }
			     }
			     """, typeof(DateTime), typeof(Task), typeof(CancellationToken));

		await That(result.Sources).DoesNotContainKey("ReturnsThrowsAsyncExtensions.g.cs");
	}

	[Test]
	public async Task GenerateAsyncExtensionsForMethodsWithMoreParameters()
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
			             int MyMethod(int v1, bool v2, double v3, long v4, CancellationToken v5);
			         }
			     }
			     """, typeof(DateTime), typeof(Task), typeof(CancellationToken));

		await That(result.Sources).ContainsKey("ReturnsThrowsAsyncExtensions.g.cs").WhoseValue
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4, T5> ReturnsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4, T5> setup, TReturn returnValue)")
			.And
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4, T5> ReturnsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4, T5> setup, Func<TReturn> callback)")
			.And
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4, T5> ReturnsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4, T5> setup, Func<T1, T2, T3, T4, T5, TReturn> callback)")
			.And
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4, T5> ReturnsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4, T5> setup, TReturn returnValue)")
			.And
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4, T5> ReturnsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4, T5> setup, Func<TReturn> callback)")
			.And
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4, T5> ReturnsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4, T5> setup, Func<T1, T2, T3, T4, T5, TReturn> callback)")
			.And
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4, T5> ThrowsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4, T5> setup, Exception exception)")
			.And
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4, T5> ThrowsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4, T5> setup, Func<Exception> callback)")
			.And
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4, T5> ThrowsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4, T5> setup, Func<T1, T2, T3, T4, T5, Exception> callback)")
			.And
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4, T5> ThrowsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4, T5> setup, Exception exception)")
			.And
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4, T5> ThrowsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4, T5> setup, Func<Exception> callback)")
			.And
			.Contains(
				"public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4, T5> ThrowsAsync<TReturn, T1, T2, T3, T4, T5>(this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4, T5> setup, Func<T1, T2, T3, T4, T5, Exception> callback)");
	}

	[Test]
	public async Task ShouldOnlyGenerateNecessaryExtensions()
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
			             int MyMethod(int v1, bool v2, double v3, long v4, string v5, CancellationToken v6);
			         }
			     }
			     """, typeof(DateTime), typeof(Task), typeof(CancellationToken));

		await That(result.Sources).ContainsKey("ReturnsThrowsAsyncExtensions.g.cs").WhoseValue
			.Contains("ReturnsAsync<TReturn, T1, T2, T3, T4, T5, T6>(this").And
			.Contains("ThrowsAsync<TReturn, T1, T2, T3, T4, T5, T6>(this").And
			.DoesNotContain("ReturnsAsync<TReturn, T1, T2, T3, T4, T5>(this").And
			.DoesNotContain("ThrowsAsync<TReturn, T1, T2, T3, T4, T5>(this").And
			.DoesNotContain("ReturnsAsync<TReturn, T1, T2, T3, T4, T5, T6, T7>(this").And
			.DoesNotContain("ThrowsAsync<TReturn, T1, T2, T3, T4, T5, T6, T7>(this");
	}

	[Test]
	public async Task WhenAllMethodsHaveUpTo4Parameters_ShouldNotGenerateReturnsAsyncExtensions()
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
			             Task<int> MyMethod(int v1, bool v2, double v3, long v4);
			             void MyVoidMethod(int v1, bool v2, double v3, long v4);
			         }
			     }
			     """, typeof(DateTime), typeof(Task), typeof(CancellationToken));

		await That(result.Sources).DoesNotContainKey("ReturnsThrowsAsyncExtensions.g.cs");
	}
}
