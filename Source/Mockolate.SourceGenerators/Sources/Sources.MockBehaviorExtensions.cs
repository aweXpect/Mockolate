using System.Text;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string MockBehaviorExtensions()
	{
		StringBuilder sb = InitializeBuilder([
			"System",
			"System.Collections.Generic",
			"System.Collections.Concurrent",
			"System.Diagnostics",
			"System.Linq",
			"System.Threading",
			"System.Threading.Tasks",
			"Mockolate",
		]);

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable annotations

		          /// <summary>
		          ///     Extensions for <see cref="MockBehavior" />.
		          /// </summary>
		          internal static partial class Mock
		          {
		          	private static MockBehavior _default = new MockBehavior(new DefaultValueGenerator());
		          	
		          	extension(MockBehavior)
		          	{
		          		/// <summary>
		          		///     The default mock behavior settings.
		          		/// </summary>
		          		public static MockBehavior Default => _default;
		          	}
		          	
		          	/// <summary>
		          	///     Defines a factory for creating default values for a specified type.
		          	/// </summary>
		          	public interface IDefaultValueFactory
		          	{
		          		/// <summary>
		          		///     Determines whether the specified <paramref name="type" /> can be created by this factory.
		          		/// </summary>
		          		bool IsMatch(Type type);
		          	
		          		/// <summary>
		          		///     Creates a new instance of the specified type.
		          		/// </summary>
		          		object? Create(Type type, IDefaultValueGenerator defaultValueGenerator, object?[] parameters);
		          	}
		          	
		          	/// <summary>
		          	///     A <see cref="IDefaultValueFactory" /> that returns a specified <paramref name="value" /> for the given type
		          	///     parameter <typeparamref name="T" />.
		          	/// </summary>
		          	internal class TypedDefaultValueFactory<T>(T value) : IDefaultValueFactory
		          	{
		          		/// <inheritdoc cref="IDefaultValueFactory.IsMatch(Type)" />
		          		public bool IsMatch(Type type)
		          			=> type == typeof(T);
		          	
		          		/// <inheritdoc cref="IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object[])" />
		          		public object? Create(Type type, IDefaultValueGenerator defaultValueGenerator, params object?[] parameters)
		          			=> value;
		          	}
		          	
		          	/// <summary>
		          	///     Provides default values for common types used in mocking scenarios.
		          	/// </summary>
		          	private class DefaultValueGenerator : IDefaultValueGenerator
		          	{
		          		private static readonly ConcurrentQueue<IDefaultValueFactory> _factories = new([
		          			new TypedDefaultValueFactory<string>(""),
		          			new CancellableTaskFactory(),
		          	#if NET8_0_OR_GREATER
		          			new CancellableValueTaskFactory(),
		          	#endif
		          			new TypedDefaultValueFactory<CancellationToken>(CancellationToken.None),
		          			new TypedDefaultValueFactory<System.Collections.IEnumerable>(Array.Empty<object?>()),
		          		]);
		          	
		          		/// <inheritdoc cref="IDefaultValueGenerator.Generate(Type, object?[])" />
		          		public object? Generate(Type type, params object?[] parameters)
		          		{
		          			if (TryGenerate(type, parameters, out object? value))
		          			{
		          				return value;
		          			}
		          	
		          			return null;
		          		}
		          	
		          		/// <summary>
		          		///     Registers a <paramref name="defaultValueFactory" /> to provide default values for a specific type.
		          		/// </summary>
		          		public static void Register(IDefaultValueFactory defaultValueFactory)
		          			=> _factories.Enqueue(defaultValueFactory);
		          	
		          		/// <summary>
		          		///     Tries to generate a default value for the specified type.
		          		/// </summary>
		          		protected virtual bool TryGenerate(Type type, object?[] parameters, out object? value)
		          		{
		          			IDefaultValueFactory? matchingFactory = _factories.FirstOrDefault(f => f.IsMatch(type));
		          			if (matchingFactory is not null)
		          			{
		          				value = matchingFactory.Create(type, this, parameters);
		          				return true;
		          			}
		          			
		          			value = new MockGenerator().Get(null, _default, type);
		          			return value is not null;
		          		}
		          	
		          		private static bool HasCanceledCancellationToken(object?[] parameters, out CancellationToken cancellationToken)
		          		{
		          			CancellationToken parameter = parameters.OfType<CancellationToken>().FirstOrDefault();
		          			if (parameter.IsCancellationRequested)
		          			{
		          				cancellationToken = parameter;
		          				return true;
		          			}
		          	
		          			cancellationToken = CancellationToken.None;
		          			return false;
		          		}
		          	
		          		private sealed class CancellableTaskFactory : IDefaultValueFactory
		          		{
		          			/// <inheritdoc cref="IDefaultValueFactory.IsMatch(Type)" />
		          			public bool IsMatch(Type type)
		          				=> type == typeof(Task);
		          	
		          			/// <inheritdoc cref="IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object[])" />
		          			public object Create(Type type, IDefaultValueGenerator defaultValueGenerator, params object?[] parameters)
		          			{
		          				if (HasCanceledCancellationToken(parameters, out CancellationToken cancellationToken))
		          				{
		          					return Task.FromCanceled(cancellationToken);
		          				}
		          	
		          				return Task.CompletedTask;
		          			}
		          		}
		          	#if NET8_0_OR_GREATER
		          		private sealed class CancellableValueTaskFactory : IDefaultValueFactory
		          		{
		          			/// <inheritdoc cref="IDefaultValueFactory.IsMatch(Type)" />
		          			public bool IsMatch(Type type)
		          				=> type == typeof(ValueTask);
		          	
		          			/// <inheritdoc cref="IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object[])" />
		          			public object Create(Type type, IDefaultValueGenerator defaultValueGenerator, params object?[] parameters)
		          			{
		          				if (HasCanceledCancellationToken(parameters, out CancellationToken cancellationToken))
		          				{
		          					return ValueTask.FromCanceled(cancellationToken);
		          				}
		          	
		          				return ValueTask.CompletedTask;
		          			}
		          		}
		          	#endif
		          	}
		          }

		          /// <summary>
		          ///     Extensions on <see cref="IDefaultValueGenerator" />
		          /// </summary>
		          internal static class DefaultValueGeneratorExtensions
		          {
		          	/// <summary>
		          	///     Adds a generic <see cref="IDefaultValueGenerator.Generate" /> method for specific types.
		          	/// </summary>
		          	extension(IDefaultValueGenerator generator)
		          	{
		          		/// <summary>
		          		///     Generates a <see cref="Task" /> of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
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

		          #if NET8_0_OR_GREATER
		          		/// <summary>
		          		///     Generates a <see cref="ValueTask" /> of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
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
		          #endif

		          		/// <summary>
		          		///     Generates a tuple of (<typeparamref name="T1" />, <typeparamref name="T2" />), with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public (T1, T2) Generate<T1, T2>((T1, T2) nullValue, params object?[] parameters)
		          		{
		          			if (parameters.Length >= 2 && parameters[0] is Func<T1> func1 && parameters[1] is Func<T2> func2)
		          			{
		          				return (func1(), func2());
		          			}
		          			
		          			return (generator.Generate(default(T1)!, parameters), generator.Generate(default(T2)!, parameters));
		          		}
		          """).AppendLine();
		for (int i = 3; i <= 8; i++)
		{
			string ts = string.Join(", ", Enumerable.Range(1, i).Select(x => $"T{x}"));
			sb.Append($$"""
			            		/// <summary>
			            		///     Generates a tuple of ({{string.Join(", ", Enumerable.Range(1, i).Select(x => $"<typeparamref name=\"T{x}\" />"))}}), with
			            		///     the <paramref name="parameters" /> for context.
			            		/// </summary>
			            		public ({{ts}}) Generate<{{ts}}>(({{ts}}) nullValue, params object?[] parameters)
			            		{
			            			if (parameters.Length >= {{i}} && {{string.Join(" && ", Enumerable.Range(1, i).Select(x => $"parameters[{x - 1}] is Func<T{x}> func{x}"))}})
			            			{
			            				return ({{string.Join(", ", Enumerable.Range(1, i).Select(x => $"func{x}()"))}});
			            			}
			            			
			            			return ({{string.Join(", ", Enumerable.Range(1, i).Select(x => $"generator.Generate(default(T{x})!, parameters)"))}});
			            		}
			            """).AppendLine();
		}

		sb.Append("""
		          		/// <summary>
		          		///     Generates an empty enumerable of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public IEnumerable<T> Generate<T>(IEnumerable<T> nullValue, params object?[] parameters)
		          			=> Array.Empty<T>();
		          		
		          		/// <summary>
		          		///     Generates an empty enumerable of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public List<T> Generate<T>(List<T> nullValue, params object?[] parameters)
		          			=> new List<T>();
		          		
		          		/// <summary>
		          		///     Generates an empty array of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public T[] Generate<T>(T[] nullValue, params object?[] parameters)
		          			=> Array.Empty<T>();
		          		
		          		/// <summary>
		          		///     Generates an empty two-dimensional array of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public T[,] Generate<T>(T[,] nullValue, params object?[] parameters)
		          			=> new T[,] { };
		          				
		          		/// <summary>
		          		///     Generates an empty three-dimensional array of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public T[,,] Generate<T>(T[,,] nullValue, params object?[] parameters)
		          			=> new T[,,] { };
		          				
		          		/// <summary>
		          		///     Generates an empty four-dimensional array of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public T[,,,] Generate<T>(T[,,,] nullValue, params object?[] parameters)
		          			=> new T[,,,] { };

		          		/// <summary>
		          		///     Generates a default value of type <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public T Generate<T>(T nullValue, params object?[] parameters)
		          		{
		          			if (generator.Generate(typeof(T), parameters) is T value)
		          			{
		          				return value;
		          			}

		          			return nullValue;
		          		}
		          	}
		          }

		          #nullable disable
		          """);
		return sb.ToString();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
