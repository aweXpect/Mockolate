using System.Collections.Generic;

namespace Mockolate.SourceGenerators.Tests;

public sealed class MockBehaviorExtensionsTests
{
	[Fact]
	public async Task DefaultValueGenerator_ShouldRegisterArrays()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
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
			             int[] SimpleArray { get; }
			             int[,,][,][] MultiDimensionalArray { get; }
			             T[] GenericArray<T>();
			         }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockBehaviorExtensions.g.cs").WhoseValue
			.Contains("""
			          		public T[] Generate<T>(T[] nullValue, params object?[] parameters)
			          			=> global::System.Array.Empty<T>();
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public T[,] Generate<T>(T[,] nullValue, params object?[] parameters)
			          			=> new T[,] { };
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public T[,,] Generate<T>(T[,,] nullValue, params object?[] parameters)
			          			=> new T[,,] { };
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public T[,,,] Generate<T>(T[,,,] nullValue, params object?[] parameters)
			          			=> new T[,,,] { };
			          """).IgnoringNewlineStyle();
	}

	[Fact]
	public async Task DefaultValueGenerator_ShouldRegisterIEnumerables()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Collections.Generic;
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
			             IEnumerable<int> EnumerableOfInt { get; }
			             IEnumerable<int[]> EnumerableOfIntArray { get; }
			             IEnumerable<T> GenericEnumerable<T>();
			         }
			     }
			     """, typeof(IEnumerable<>));

		await That(result.Sources).ContainsKey("MockBehaviorExtensions.g.cs").WhoseValue
			.Contains("""
			          		public global::System.Collections.Generic.IEnumerable<T> Generate<T>(global::System.Collections.Generic.IEnumerable<T> nullValue, params object?[] parameters)
			          			=> global::System.Array.Empty<T>();
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public global::System.Collections.Generic.List<T> Generate<T>(global::System.Collections.Generic.List<T> nullValue, params object?[] parameters)
			          			=> new global::System.Collections.Generic.List<T>();
			          """).IgnoringNewlineStyle();
	}

	[Fact]
	public async Task DefaultValueGenerator_ShouldRegisterTasks()
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
			     			_ = IMyInterface.CreateMock();
			             }
			         }

			         public interface IMyInterface
			         {
			             Task<int> IntTask { get; }
			             Task<int[]> IntArrayTask { get; }
			             Task<T> GenericValueTask<T>();
			         }
			     }
			     """, typeof(Task));

		await That(result.Sources).ContainsKey("MockBehaviorExtensions.g.cs").WhoseValue
			.Contains("""
			          		public global::System.Threading.Tasks.Task<T> Generate<T>(global::System.Threading.Tasks.Task<T> nullValue, params object?[] parameters)
			          		{
			          			global::System.Threading.CancellationToken cancellationToken = global::System.Linq.Enumerable.FirstOrDefault(
			          				global::System.Linq.Enumerable.OfType<global::System.Threading.CancellationToken?>(parameters)) ?? global::System.Threading.CancellationToken.None;
			          			if (cancellationToken.IsCancellationRequested)
			          			{
			          				return global::System.Threading.Tasks.Task.FromCanceled<T>(cancellationToken);
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			if (parameters.Length > 0 && parameters[0] is global::System.Func<T> func)
			          			{
			          				return global::System.Threading.Tasks.Task.FromResult(func());
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			return global::System.Threading.Tasks.Task.FromResult(generator.Generate(default(T)!, parameters));
			          		}
			          """).IgnoringNewlineStyle();
	}

	[Fact]
	public async Task DefaultValueGenerator_ShouldRegisterValueTasks()
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
			     			_ = IMyInterface.CreateMock();
			             }
			         }

			         public interface IMyInterface
			         {
			             ValueTask<int> IntValueTask { get; }
			             ValueTask<int[]> IntArrayValueTask { get; }
			             ValueTask<T> GenericValueTask<T>();
			         }
			     }
			     """, typeof(ValueTask));

		await That(result.Sources).ContainsKey("MockBehaviorExtensions.g.cs").WhoseValue
			.Contains("""
			          		public global::System.Threading.Tasks.ValueTask<T> Generate<T>(global::System.Threading.Tasks.ValueTask<T> nullValue, params object?[] parameters)
			          		{
			          			global::System.Threading.CancellationToken cancellationToken = global::System.Linq.Enumerable.FirstOrDefault(
			          				global::System.Linq.Enumerable.OfType<global::System.Threading.CancellationToken?>(parameters)) ?? global::System.Threading.CancellationToken.None;
			          			if (cancellationToken.IsCancellationRequested)
			          			{
			          				return global::System.Threading.Tasks.ValueTask.FromCanceled<T>(cancellationToken);
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			if (parameters.Length > 0 && parameters[0] is global::System.Func<T> func)
			          			{
			          				return global::System.Threading.Tasks.ValueTask.FromResult(func());
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			return global::System.Threading.Tasks.ValueTask.FromResult(generator.Generate(default(T)!, parameters));
			          		}
			          """).IgnoringNewlineStyle();
	}

	[Fact]
	public async Task DefaultValueGenerator_ShouldRegisterValueTuples()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
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
			             (int V1, string V2) NamedValueTuple { get; }
			             (int, string, int, string, int, string, int, string) ValueTuple8 { get; }
			             (int, T1, T2) GenericValueTuple<T1, T2>();
			         }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockBehaviorExtensions.g.cs").WhoseValue
			.Contains("""
			          		public (T1, T2) Generate<T1, T2>((T1, T2) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 2 && parameters[0] is global::System.Func<T1> func1 && parameters[1] is global::System.Func<T2> func2)
			          			{
			          				return (func1(), func2());
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3) Generate<T1, T2, T3>((T1, T2, T3) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 3 && parameters[0] is global::System.Func<T1> func1 && parameters[1] is global::System.Func<T2> func2 && parameters[2] is global::System.Func<T3> func3)
			          			{
			          				return (func1(), func2(), func3());
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3, T4) Generate<T1, T2, T3, T4>((T1, T2, T3, T4) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 4 && parameters[0] is global::System.Func<T1> func1 && parameters[1] is global::System.Func<T2> func2 && parameters[2] is global::System.Func<T3> func3 && parameters[3] is global::System.Func<T4> func4)
			          			{
			          				return (func1(), func2(), func3(), func4());
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters), generator.Generate(default(T4)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3, T4, T5) Generate<T1, T2, T3, T4, T5>((T1, T2, T3, T4, T5) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 5 && parameters[0] is global::System.Func<T1> func1 && parameters[1] is global::System.Func<T2> func2 && parameters[2] is global::System.Func<T3> func3 && parameters[3] is global::System.Func<T4> func4 && parameters[4] is global::System.Func<T5> func5)
			          			{
			          				return (func1(), func2(), func3(), func4(), func5());
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters), generator.Generate(default(T4)!, parameters), generator.Generate(default(T5)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3, T4, T5, T6) Generate<T1, T2, T3, T4, T5, T6>((T1, T2, T3, T4, T5, T6) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 6 && parameters[0] is global::System.Func<T1> func1 && parameters[1] is global::System.Func<T2> func2 && parameters[2] is global::System.Func<T3> func3 && parameters[3] is global::System.Func<T4> func4 && parameters[4] is global::System.Func<T5> func5 && parameters[5] is global::System.Func<T6> func6)
			          			{
			          				return (func1(), func2(), func3(), func4(), func5(), func6());
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters), generator.Generate(default(T4)!, parameters), generator.Generate(default(T5)!, parameters), generator.Generate(default(T6)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3, T4, T5, T6, T7) Generate<T1, T2, T3, T4, T5, T6, T7>((T1, T2, T3, T4, T5, T6, T7) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 7 && parameters[0] is global::System.Func<T1> func1 && parameters[1] is global::System.Func<T2> func2 && parameters[2] is global::System.Func<T3> func3 && parameters[3] is global::System.Func<T4> func4 && parameters[4] is global::System.Func<T5> func5 && parameters[5] is global::System.Func<T6> func6 && parameters[6] is global::System.Func<T7> func7)
			          			{
			          				return (func1(), func2(), func3(), func4(), func5(), func6(), func7());
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters), generator.Generate(default(T4)!, parameters), generator.Generate(default(T5)!, parameters), generator.Generate(default(T6)!, parameters), generator.Generate(default(T7)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3, T4, T5, T6, T7, T8) Generate<T1, T2, T3, T4, T5, T6, T7, T8>((T1, T2, T3, T4, T5, T6, T7, T8) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 8 && parameters[0] is global::System.Func<T1> func1 && parameters[1] is global::System.Func<T2> func2 && parameters[2] is global::System.Func<T3> func3 && parameters[3] is global::System.Func<T4> func4 && parameters[4] is global::System.Func<T5> func5 && parameters[5] is global::System.Func<T6> func6 && parameters[6] is global::System.Func<T7> func7 && parameters[7] is global::System.Func<T8> func8)
			          			{
			          				return (func1(), func2(), func3(), func4(), func5(), func6(), func7(), func8());
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters), generator.Generate(default(T4)!, parameters), generator.Generate(default(T5)!, parameters), generator.Generate(default(T6)!, parameters), generator.Generate(default(T7)!, parameters), generator.Generate(default(T8)!, parameters));
			          		}
			          """).IgnoringNewlineStyle();
	}
}
