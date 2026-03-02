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

		await That(result.Sources).ContainsKey("MockBehaviorExtensions.g.cs").WhoseValue
			.Contains("""
			          		public T[] Generate<T>(T[] nullValue, params object?[] parameters)
			          			=> Array.Empty<T>();
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

		await That(result.Sources).ContainsKey("MockBehaviorExtensions.g.cs").WhoseValue
			.Contains("""
			          		public IEnumerable<T> Generate<T>(IEnumerable<T> nullValue, params object?[] parameters)
			          			=> Array.Empty<T>();
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public List<T> Generate<T>(List<T> nullValue, params object?[] parameters)
			          			=> new List<T>();
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

		await That(result.Sources).ContainsKey("MockBehaviorExtensions.g.cs").WhoseValue
			.Contains("""
			          		public Task<T> Generate<T>(Task<T> nullValue, params object?[] parameters)
			          		{
			          			CancellationToken cancellationToken = parameters.OfType<object?[]>().FirstOrDefault()?
			          				.OfType<CancellationToken>().FirstOrDefault() ?? CancellationToken.None;
			          			if (cancellationToken.IsCancellationRequested)
			          			{
			          				return Task.FromCanceled<T>(cancellationToken);
			          			}
			          			
			          			if (parameters.Length > 0 && parameters[0] is Func<T> func)
			          			{
			          				return Task.FromResult(func());
			          			}
			          			
			          			return Task.FromResult(generator.Generate(default(T)!, parameters));
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

		await That(result.Sources).ContainsKey("MockBehaviorExtensions.g.cs").WhoseValue
			.Contains("""
			          		public ValueTask<T> Generate<T>(ValueTask<T> nullValue, params object?[] parameters)
			          		{
			          			CancellationToken cancellationToken = parameters.OfType<object?[]>().FirstOrDefault()?
			          				.OfType<CancellationToken>().FirstOrDefault() ?? CancellationToken.None;
			          			if (cancellationToken.IsCancellationRequested)
			          			{
			          				return ValueTask.FromCanceled<T>(cancellationToken);
			          			}
			          			
			          			if (parameters.Length > 0 && parameters[0] is Func<T> func)
			          			{
			          				return ValueTask.FromResult(func());
			          			}
			          			
			          			return ValueTask.FromResult(generator.Generate(default(T)!, parameters));
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

		await That(result.Sources).ContainsKey("MockBehaviorExtensions.g.cs").WhoseValue
			.Contains("""
			          		public (T1, T2) Generate<T1, T2>((T1, T2) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 2 && parameters[0] is Func<T1> func1 && parameters[1] is Func<T2> func2)
			          			{
			          				return (func1(), func2());
			          			}
			          			
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3) Generate<T1, T2, T3>((T1, T2, T3) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 3 && parameters[0] is Func<T1> func1 && parameters[1] is Func<T2> func2 && parameters[2] is Func<T3> func3)
			          			{
			          				return (func1(), func2(), func3());
			          			}
			          			
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3, T4) Generate<T1, T2, T3, T4>((T1, T2, T3, T4) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 4 && parameters[0] is Func<T1> func1 && parameters[1] is Func<T2> func2 && parameters[2] is Func<T3> func3 && parameters[3] is Func<T4> func4)
			          			{
			          				return (func1(), func2(), func3(), func4());
			          			}
			          			
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters), generator.Generate(default(T4)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3, T4, T5) Generate<T1, T2, T3, T4, T5>((T1, T2, T3, T4, T5) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 5 && parameters[0] is Func<T1> func1 && parameters[1] is Func<T2> func2 && parameters[2] is Func<T3> func3 && parameters[3] is Func<T4> func4 && parameters[4] is Func<T5> func5)
			          			{
			          				return (func1(), func2(), func3(), func4(), func5());
			          			}
			          			
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters), generator.Generate(default(T4)!, parameters), generator.Generate(default(T5)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3, T4, T5, T6) Generate<T1, T2, T3, T4, T5, T6>((T1, T2, T3, T4, T5, T6) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 6 && parameters[0] is Func<T1> func1 && parameters[1] is Func<T2> func2 && parameters[2] is Func<T3> func3 && parameters[3] is Func<T4> func4 && parameters[4] is Func<T5> func5 && parameters[5] is Func<T6> func6)
			          			{
			          				return (func1(), func2(), func3(), func4(), func5(), func6());
			          			}
			          			
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters), generator.Generate(default(T4)!, parameters), generator.Generate(default(T5)!, parameters), generator.Generate(default(T6)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3, T4, T5, T6, T7) Generate<T1, T2, T3, T4, T5, T6, T7>((T1, T2, T3, T4, T5, T6, T7) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 7 && parameters[0] is Func<T1> func1 && parameters[1] is Func<T2> func2 && parameters[2] is Func<T3> func3 && parameters[3] is Func<T4> func4 && parameters[4] is Func<T5> func5 && parameters[5] is Func<T6> func6 && parameters[6] is Func<T7> func7)
			          			{
			          				return (func1(), func2(), func3(), func4(), func5(), func6(), func7());
			          			}
			          			
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters), generator.Generate(default(T4)!, parameters), generator.Generate(default(T5)!, parameters), generator.Generate(default(T6)!, parameters), generator.Generate(default(T7)!, parameters));
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public (T1, T2, T3, T4, T5, T6, T7, T8) Generate<T1, T2, T3, T4, T5, T6, T7, T8>((T1, T2, T3, T4, T5, T6, T7, T8) nullValue, params object?[] parameters)
			          		{
			          			if (parameters.Length >= 8 && parameters[0] is Func<T1> func1 && parameters[1] is Func<T2> func2 && parameters[2] is Func<T3> func3 && parameters[3] is Func<T4> func4 && parameters[4] is Func<T5> func5 && parameters[5] is Func<T6> func6 && parameters[6] is Func<T7> func7 && parameters[7] is Func<T8> func8)
			          			{
			          				return (func1(), func2(), func3(), func4(), func5(), func6(), func7(), func8());
			          			}
			          			
			          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters), generator.Generate(default(T3)!, parameters), generator.Generate(default(T4)!, parameters), generator.Generate(default(T5)!, parameters), generator.Generate(default(T6)!, parameters), generator.Generate(default(T7)!, parameters), generator.Generate(default(T8)!, parameters));
			          		}
			          """).IgnoringNewlineStyle();
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
			          		partial void Generate<T>(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior, Action<IMockSetup<T>>[] setups, params Type[] types)
			          		{
			          			IMockBehaviorAccess mockBehaviorAccess = (IMockBehaviorAccess)mockBehavior;
			          			if (mockBehaviorAccess.TryInitialize<T>(out Action<IMockSetup<T>>[]? additionalSetups))
			          			{
			          				if (setups.Length > 0)
			          				{
			          					Action<IMockSetup<T>>[] concatenatedSetups = new Action<IMockSetup<T>>[additionalSetups.Length + setups.Length];
			          					additionalSetups.CopyTo(concatenatedSetups, 0);
			          					setups.CopyTo(concatenatedSetups, additionalSetups.Length);
			          					setups = concatenatedSetups;
			          				}
			          				else
			          				{
			          					setups = additionalSetups;
			          				}
			          			}
			          
			          			if (constructorParameters is null && mockBehaviorAccess.TryGetConstructorParameters<T>(out object?[]? parameters))
			          			{
			          				constructorParameters = new BaseClass.ConstructorParameters(parameters);
			          			}
			          
			          			if (types.Length == 1 &&
			          			    types[0] == typeof(MyCode.IMyInterface))
			          			{
			          				_value = new MockForIMyInterface(mockBehavior);
			          				if (setups.Length > 0)
			          				{
			          					IMockSetup<MyCode.IMyInterface> setupTarget = ((IMockSubject<MyCode.IMyInterface>)_value).Mock;
			          					foreach (Action<IMockSetup<MyCode.IMyInterface>> setup in setups)
			          					{
			          						setup.Invoke(setupTarget);
			          					}
			          				}
			          			}
			          			else if (types.Length == 2 &&
			          			         types[0] == typeof(MyCode.MyBaseClass) &&
			          			         types[1] == typeof(MyCode.IMyInterface))
			          			{
			          				if (constructorParameters is null || constructorParameters.Parameters.Length == 0)
			          				{
			          					MockRegistration mockRegistration = new MockRegistration(mockBehavior, "MyCode.MyBaseClass");
			          					MockForMyBaseClass_IMyInterface.MockRegistrationsProvider.Value = mockRegistration;
			          					if (setups.Length > 0)
			          					{
			          						#pragma warning disable CS0618
			          						IMockSetup<MyCode.MyBaseClass> setupTarget = new MockSetup<MyCode.MyBaseClass>(mockRegistration);
			          						#pragma warning restore CS0618
			          						foreach (Action<IMockSetup<MyCode.MyBaseClass>> setup in setups)
			          						{
			          							setup.Invoke(setupTarget);
			          						}
			          					}
			          					_value = new MockForMyBaseClass_IMyInterface(mockRegistration);
			          				}
			          				else if (constructorParameters.Parameters.Length == 0)
			          				{
			          					MockRegistration mockRegistration = new MockRegistration(mockBehavior, "MyCode.MyBaseClass");
			          					MockForMyBaseClass_IMyInterface.MockRegistrationsProvider.Value = mockRegistration;
			          					if (setups.Length > 0)
			          					{
			          						#pragma warning disable CS0618
			          						IMockSetup<MyCode.MyBaseClass> setupTarget = new MockSetup<MyCode.MyBaseClass>(mockRegistration);
			          						#pragma warning restore CS0618
			          						foreach (Action<IMockSetup<MyCode.MyBaseClass>> setup in setups)
			          						{
			          							setup.Invoke(setupTarget);
			          						}
			          					}
			          					_value = new MockForMyBaseClass_IMyInterface(mockRegistration);
			          				}
			          				else
			          				{
			          					throw new MockException($"Could not find any constructor for 'MyCode.MyBaseClass' that matches the {constructorParameters.Parameters.Length} given parameters ({string.Join(", ", constructorParameters.Parameters)}).");
			          				}
			          			}
			          		}
			          """).IgnoringNewlineStyle();
	}
}
