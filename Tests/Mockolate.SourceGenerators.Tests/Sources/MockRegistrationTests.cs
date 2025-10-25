using System.Collections.Generic;
using System.Threading;

namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed class MockRegistrationTests
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
			     			_ = Mock.Create<IMyInterface>();
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

		await That(result.Sources).ContainsKey("MockRegistration.g.cs").WhoseValue
			.Contains("DefaultValueGenerator.Register(new TypedDefaultValueFactory<int[]>(Array.Empty<int>()));").And
			.Contains("DefaultValueGenerator.Register(new TypedDefaultValueFactory<int[,,][,][]>(new int[0,0,0][,][]));").And
			.DoesNotContain("T[]");
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
			     			_ = Mock.Create<IMyInterface>();
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

		await That(result.Sources).ContainsKey("MockRegistration.g.cs").WhoseValue
			.Contains("DefaultValueGenerator.Register(new CallbackDefaultValueFactory<System.Threading.Tasks.Task<int>>(defaultValueGenerator => System.Threading.Tasks.Task.FromResult<int>(defaultValueGenerator.Generate<int>()), type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.Task<>) && type.GenericTypeArguments[0] == typeof(int)));").And
			.Contains("DefaultValueGenerator.Register(new CallbackDefaultValueFactory<System.Threading.Tasks.Task<int[]>>(defaultValueGenerator => System.Threading.Tasks.Task.FromResult<int[]>(defaultValueGenerator.Generate<int[]>()), type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.Task<>) && type.GenericTypeArguments[0] == typeof(int[])));").And
			.DoesNotContain("Task<T>");
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
			     			_ = Mock.Create<IMyInterface>();
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

		await That(result.Sources).ContainsKey("MockRegistration.g.cs").WhoseValue
			.Contains("DefaultValueGenerator.Register(new CallbackDefaultValueFactory<System.Threading.Tasks.ValueTask<int>>(defaultValueGenerator => new System.Threading.Tasks.ValueTask<int>(defaultValueGenerator.Generate<int>()), type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.ValueTask<>) && type.GenericTypeArguments[0] == typeof(int)));").And
			.Contains("DefaultValueGenerator.Register(new CallbackDefaultValueFactory<System.Threading.Tasks.ValueTask<int[]>>(defaultValueGenerator => new System.Threading.Tasks.ValueTask<int[]>(defaultValueGenerator.Generate<int[]>()), type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.ValueTask<>) && type.GenericTypeArguments[0] == typeof(int[])));").And
			.DoesNotContain("ValueTask<T>");
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
			     			_ = Mock.Create<IMyInterface>();
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

		await That(result.Sources).ContainsKey("MockRegistration.g.cs").WhoseValue
			.Contains("DefaultValueGenerator.Register(new TypedDefaultValueFactory<System.Collections.Generic.IEnumerable<int>>(Array.Empty<int>()));").And
			.Contains("DefaultValueGenerator.Register(new TypedDefaultValueFactory<System.Collections.Generic.IEnumerable<int[]>>(Array.Empty<int[]>()));").And
			.DoesNotContain("IEnumerable<T>");
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
			     			_ = Mock.Create<IMyInterface>();
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

		await That(result.Sources).ContainsKey("MockRegistration.g.cs").WhoseValue
			.Contains("DefaultValueGenerator.Register(new CallbackDefaultValueFactory<(int V1, string V2)>(defaultValueGenerator => (defaultValueGenerator.Generate<int>(), defaultValueGenerator.Generate<string>())));").And
			.Contains("DefaultValueGenerator.Register(new CallbackDefaultValueFactory<(int, string, int, string, int, string, int, string)>(defaultValueGenerator => (defaultValueGenerator.Generate<int>(), defaultValueGenerator.Generate<string>(), defaultValueGenerator.Generate<int>(), defaultValueGenerator.Generate<string>(), defaultValueGenerator.Generate<int>(), defaultValueGenerator.Generate<string>(), defaultValueGenerator.Generate<int>(), defaultValueGenerator.Generate<string>())));").And
			.DoesNotContain("(int, T1, T2)");
	}

	[Fact]
	public async Task ShouldRegisterAllTypesInTheMockGenerator()
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
			     			var y = Mock.Create<IMyInterface>();
			     			var z = Mock.Create<MyBaseClass, IMyInterface>();
			             }
			         }

			         public interface IMyInterface
			         {
			             void MyMethod(int value);
			         }

			         public class MyBaseClass
			         {
			             protected virtual Task<int> MyMethod(int v1, bool v2, double v3, long v4, uint v5, string v6, DateTime v7, TimeSpan v8, CancellationToken v9)
			             {
			                 return Task.FromResult(1);
			             }
			         }
			     }
			     """, typeof(DateTime), typeof(Task), typeof(CancellationToken));

		await That(result.Sources).ContainsKey("MockRegistration.g.cs").WhoseValue
			.Contains("""
			          		partial void Generate(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior, params Type[] types)
			          		{
			          			if (types.Length == 1 &&
			          			    types[0] == typeof(MyCode.IMyInterface))
			          			{
			          				_value = new ForIMyInterface.Mock(constructorParameters, mockBehavior);
			          			}
			          			else if (types.Length == 2 &&
			          			         types[0] == typeof(MyCode.MyBaseClass) &&
			          			         types[1] == typeof(MyCode.IMyInterface))
			          			{
			          				_value = new ForMyBaseClass_IMyInterface.Mock(constructorParameters, mockBehavior);
			          			}
			          		}
			          """).IgnoringNewlineStyle();
	}
}
