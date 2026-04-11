using System.Collections.Immutable;
using System.Text;
using Mockolate.SourceGenerators.Entities;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string MockBehaviorExtensions(ImmutableArray<MockClass> mockClasses)
	{
		bool includeHttpClient = mockClasses.Any(m => m.ClassFullName == "global::System.Net.Http.HttpClient");
		StringBuilder sb = InitializeBuilder();

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable annotations

		          /// <summary>
		          ///     Extensions for <see cref="global::Mockolate.MockBehavior" />.
		          /// </summary>
		          internal static partial class Mock
		          {
		          	private static readonly global::Mockolate.MockBehavior _default;
		          	
		          	static Mock()
		          	{
		          		_default = new global::Mockolate.MockBehavior(new DefaultValueGenerator());
		          	}
		          	
		          	extension(global::Mockolate.MockBehavior)
		          	{
		          		/// <summary>
		          		///     The default mock behavior settings.
		          		/// </summary>
		          		public static global::Mockolate.MockBehavior Default => _default;
		          	}
		          	
		          	/// <summary>
		          	///     Defines a factory for creating default values for a specified type.
		          	/// </summary>
		          	public interface IDefaultValueFactory
		          	{
		          		/// <summary>
		          		///     Determines whether the specified <paramref name="type" /> can be created by this factory.
		          		/// </summary>
		          		bool IsMatch(global::System.Type type);
		          	
		          		/// <summary>
		          		///     Creates a new instance of the specified type.
		          		/// </summary>
		          		object? Create(global::System.Type type, IDefaultValueGenerator defaultValueGenerator, global::Mockolate.Parameters.INamedParameterValue[] parameters);
		          	}
		          	
		          	/// <summary>
		          	///     A <see cref="IDefaultValueFactory" /> that returns a specified <paramref name="value" /> for the given type
		          	///     parameter <typeparamref name="T" />.
		          	/// </summary>
		          """);
#if RELEASE
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.AppendLine("""
		              	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		              	internal class TypedDefaultValueFactory<T>(T value) : IDefaultValueFactory
		              	{
		              		/// <inheritdoc cref="IDefaultValueFactory.IsMatch(global::System.Type)" />
		              		public bool IsMatch(global::System.Type type)
		              			=> type == typeof(T);
		              	
		              		/// <inheritdoc cref="IDefaultValueFactory.Create(global::System.Type, IDefaultValueGenerator, global::Mockolate.Parameters.INamedParameterValue[])" />
		              		public object? Create(global::System.Type type, IDefaultValueGenerator defaultValueGenerator, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		              			=> value;
		              	}
		              """).AppendLine();
		if (includeHttpClient)
		{
			sb.Append("""
			          	/// <summary>
			          	///     A <see cref="IDefaultValueFactory" /> that returns an empty <see cref="global::System.Net.Http.HttpResponseMessage" /> with the specified
			          	///     <paramref name="statusCode" />.
			          	/// </summary>
			          """);
#if RELEASE
			sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
			sb.AppendLine("""
			              	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
			              	private sealed class HttpResponseMessageFactory(global::System.Net.HttpStatusCode statusCode) : IDefaultValueFactory
			              	{
			              		/// <inheritdoc cref="IDefaultValueFactory.IsMatch(global::System.Type)" />
			              		public bool IsMatch(global::System.Type type)
			              			=> type == typeof(global::System.Net.Http.HttpResponseMessage);
			              	
			              		/// <inheritdoc cref="IDefaultValueFactory.Create(global::System.Type, IDefaultValueGenerator, object[])" />
			              		public object? Create(global::System.Type type, IDefaultValueGenerator defaultValueGenerator, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
			              			=> new global::System.Net.Http.HttpResponseMessage(statusCode) { Content = new global::System.Net.Http.StringContent(string.Empty) };
			              	}
			              """).AppendLine();
		}

		sb.Append("""
		          	
		          	/// <summary>
		          	///     Provides default values for common types used in mocking scenarios.
		          	/// </summary>
		          """);
#if RELEASE
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.AppendLine("""
		              	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		              	private class DefaultValueGenerator : IDefaultValueGenerator
		              	{
		              		private static readonly global::System.Collections.Concurrent.ConcurrentQueue<IDefaultValueFactory> _factories = new([
		              			new TypedDefaultValueFactory<string>(""),
		              """).AppendLine();
		if (includeHttpClient)
		{
			sb.Append("""
			          			new HttpResponseMessageFactory(global::System.Net.HttpStatusCode.NotImplemented),
			          """).AppendLine();
		}

		sb.Append("""
		          			new CancellableTaskFactory(),
		          	#if NET8_0_OR_GREATER
		          			new CancellableValueTaskFactory(),
		          	#endif
		          			new TypedDefaultValueFactory<global::System.Threading.CancellationToken>(global::System.Threading.CancellationToken.None),
		          			new TypedDefaultValueFactory<System.Collections.IEnumerable>(global::System.Array.Empty<object?>()),
		          		]);
		          	
		          		/// <inheritdoc cref="IDefaultValueGenerator.GenerateValue(global::System.Type, global::Mockolate.Parameters.INamedParameterValue[])" />
		          		public object? GenerateValue(global::System.Type type, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
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
		          		protected virtual bool TryGenerate(global::System.Type type, global::Mockolate.Parameters.INamedParameterValue[] parameters, out object? value)
		          		{
		          			IDefaultValueFactory? matchingFactory = global::System.Linq.Enumerable.FirstOrDefault(_factories, Predicate);
		          			if (matchingFactory is not null)
		          			{
		          				value = matchingFactory.Create(type, this, parameters);
		          				return true;
		          			}

		          			value = null;
		          			return false;

		          			[global::System.Diagnostics.DebuggerNonUserCode]
		          			bool Predicate(global::Mockolate.Mock.IDefaultValueFactory f)
		          				=> f.IsMatch(type);
		          		}
		          	
		          		private static bool HasCanceledCancellationToken(global::Mockolate.Parameters.INamedParameterValue[] parameters, out global::System.Threading.CancellationToken cancellationToken)
		          		{
		          			foreach (var parameter in parameters)
		          			{
		          			    if (parameter.TryGetValue(out global::System.Threading.CancellationToken token) && token.IsCancellationRequested)
		          				{
		          					cancellationToken = token;
		          					return true;
		          				}
		          			}

		          			cancellationToken = global::System.Threading.CancellationToken.None;
		          			return false;
		          		}

		          """);
#if RELEASE
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.AppendLine("""
		              	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		              	private sealed class CancellableTaskFactory : IDefaultValueFactory
		              	{
		              		/// <inheritdoc cref="IDefaultValueFactory.IsMatch(global::System.Type)" />
		              		public bool IsMatch(global::System.Type type)
		              			=> type == typeof(global::System.Threading.Tasks.Task);

		              		/// <inheritdoc cref="IDefaultValueFactory.Create(global::System.Type, IDefaultValueGenerator, object[])" />
		              		public object Create(global::System.Type type, IDefaultValueGenerator defaultValueGenerator, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		              		{
		              			if (HasCanceledCancellationToken(parameters, out global::System.Threading.CancellationToken cancellationToken))
		              			{
		              				return global::System.Threading.Tasks.Task.FromCanceled(cancellationToken);
		              			}

		              			return global::System.Threading.Tasks.Task.CompletedTask;
		              		}
		              	}
		              #if NET8_0_OR_GREATER
		              """);
#if RELEASE
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.AppendLine("""
		              		[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		              		private sealed class CancellableValueTaskFactory : IDefaultValueFactory
		              		{
		              			/// <inheritdoc cref="IDefaultValueFactory.IsMatch(global::System.Type)" />
		              			public bool IsMatch(global::System.Type type)
		              				=> type == typeof(global::System.Threading.Tasks.ValueTask);
		              	
		              			/// <inheritdoc cref="IDefaultValueFactory.Create(global::System.Type, IDefaultValueGenerator, object[])" />
		              			public object Create(global::System.Type type, IDefaultValueGenerator defaultValueGenerator, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		              			{
		              				if (HasCanceledCancellationToken(parameters, out global::System.Threading.CancellationToken cancellationToken))
		              				{
		              					return global::System.Threading.Tasks.ValueTask.FromCanceled(cancellationToken);
		              				}
		              	
		              				return global::System.Threading.Tasks.ValueTask.CompletedTask;
		              			}
		              		}
		              	#endif
		              	}
		              }

		              /// <summary>
		              ///     Extensions on <see cref="IDefaultValueGenerator" />
		              /// </summary>
		              """);
#if RELEASE
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.AppendLine("""
		              [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		              internal static class DefaultValueGeneratorExtensions
		              {
		              	/// <summary>
		              	///     Adds a generic <c>Generate</c> method for specific types.
		              	/// </summary>
		              	extension(IDefaultValueGenerator generator)
		              	{
		              		/// <summary>
		              		///     Generates a <see cref="global::System.Threading.Tasks.Task" /> of <typeparamref name="T" />, with
		              		///     the <paramref name="parameters" /> for context.
		              		/// </summary>
		              		public global::System.Threading.Tasks.Task<T> Generate<T>(global::System.Threading.Tasks.Task<T> nullValue, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		              		{
		              			global::System.Threading.CancellationToken cancellationToken = global::System.Threading.CancellationToken.None;
		              			foreach (var parameter in parameters)
		              			{
		              			    if (parameter.TryGetValue(out global::System.Threading.CancellationToken token))
		              				{
		              					cancellationToken = token;
		              					break;
		              				}
		              			}

		              			if (cancellationToken.IsCancellationRequested)
		              			{
		              				return global::System.Threading.Tasks.Task.FromCanceled<T>(cancellationToken);
		              			}

		              			foreach (global::Mockolate.Parameters.INamedParameterValue parameter in parameters)
		              			{
		              				if (parameter.TryGetValue(out global::System.Func<T> func))
		              				{
		              					return global::System.Threading.Tasks.Task.FromResult(func());
		              				}
		              			}

		              			return global::System.Threading.Tasks.Task.FromResult(generator.Generate(default(T)!, parameters));
		              		}

		              #if NET8_0_OR_GREATER
		              		/// <summary>
		              		///     Generates a <see cref="global::System.Threading.Tasks.ValueTask" /> of <typeparamref name="T" />, with
		              		///     the <paramref name="parameters" /> for context.
		              		/// </summary>
		              		public global::System.Threading.Tasks.ValueTask<T> Generate<T>(global::System.Threading.Tasks.ValueTask<T> nullValue, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		              		{
		              			global::System.Threading.CancellationToken cancellationToken = global::System.Threading.CancellationToken.None;
		              			foreach (var parameter in parameters)
		              			{
		              			    if (parameter.TryGetValue(out global::System.Threading.CancellationToken token))
		              				{
		              					cancellationToken = token;
		              					break;
		              				}
		              			}

		              			if (cancellationToken.IsCancellationRequested)
		              			{
		              				return global::System.Threading.Tasks.ValueTask.FromCanceled<T>(cancellationToken);
		              			}

		              			foreach (global::Mockolate.Parameters.INamedParameterValue parameter in parameters)
		              			{
		              				if (parameter.TryGetValue(out global::System.Func<T> func))
		              				{
		              					return global::System.Threading.Tasks.ValueTask.FromResult(func());
		              				}
		              			}

		              			return global::System.Threading.Tasks.ValueTask.FromResult(generator.Generate(default(T)!, parameters));
		              		}
		              #endif

		              		/// <summary>
		              		///     Generates a tuple of (<typeparamref name="T1" />, <typeparamref name="T2" />), with
		              		///     the <paramref name="parameters" /> for context.
		              		/// </summary>
		              		public (T1, T2) Generate<T1, T2>((T1, T2) nullValue, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		              		{
		              			if (parameters.Length >= 2 && parameters[0].TryGetValue(out global::System.Func<T1> func1) && parameters[1].TryGetValue(out global::System.Func<T2> func2))
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
			            		public ({{ts}}) Generate<{{ts}}>(({{ts}}) nullValue, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
			            		{
			            			if (parameters.Length >= {{i}} && {{string.Join(" && ", Enumerable.Range(1, i).Select(x => $"parameters[{x - 1}].TryGetValue(out global::System.Func<T{x}> func{x})"))}})
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
		          		public global::System.Collections.Generic.IEnumerable<T> Generate<T>(global::System.Collections.Generic.IEnumerable<T> nullValue, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		          			=> global::System.Array.Empty<T>();
		          		
		          		/// <summary>
		          		///     Generates an empty enumerable of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public global::System.Collections.Generic.List<T> Generate<T>(global::System.Collections.Generic.List<T> nullValue, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		          			=> new global::System.Collections.Generic.List<T>();
		          		
		          		/// <summary>
		          		///     Generates an empty array of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public T[] Generate<T>(T[] nullValue, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		          			=> global::System.Array.Empty<T>();
		          		
		          		/// <summary>
		          		///     Generates an empty two-dimensional array of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public T[,] Generate<T>(T[,] nullValue, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		          			=> new T[,] { };
		          				
		          		/// <summary>
		          		///     Generates an empty three-dimensional array of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public T[,,] Generate<T>(T[,,] nullValue, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		          			=> new T[,,] { };
		          				
		          		/// <summary>
		          		///     Generates an empty four-dimensional array of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public T[,,,] Generate<T>(T[,,,] nullValue, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		          			=> new T[,,,] { };

		          		/// <summary>
		          		///     Generates a default value of type <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public T Generate<T>(T nullValue, params global::Mockolate.Parameters.INamedParameterValue[] parameters)
		          		{
		          			if (generator.GenerateValue(typeof(T), parameters) is T value)
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
