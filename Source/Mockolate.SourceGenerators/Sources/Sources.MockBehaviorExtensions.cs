using System.Text;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string MockBehaviorExtensions()
	{
		StringBuilder sb = InitializeBuilder([
			"System",
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
		          internal static class ExtensionsOnMockBehavior
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
		          	
		          			value = null;
		          			return false;
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
		          public static class DefaultValueGeneratorExtensions
		          {
		          	/// <inheritdoc cref="DefaultValueGeneratorExtensions" />
		          	extension(IDefaultValueGenerator generator)
		          	{
		          		/// <summary>
		          		///     Generates a <see cref="Task" /> of <typeparamref name="T" />, with
		          		///     the <paramref name="parameters" /> for context.
		          		/// </summary>
		          		public Task<T> Generate<T>(Task<T> nullValue,
		          			params object?[] parameters)
		          		{
		          			CancellationToken cancellationToken = parameters.OfType<CancellationToken>().FirstOrDefault();
		          			if (cancellationToken.IsCancellationRequested)
		          			{
		          				return Task.FromCanceled<T>(cancellationToken);
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
		          			CancellationToken cancellationToken = parameters.OfType<CancellationToken>().FirstOrDefault();
		          			if (cancellationToken.IsCancellationRequested)
		          			{
		          				return ValueTask.FromCanceled<T>(cancellationToken);
		          			}
		          
		          			return ValueTask.FromResult(generator.Generate(default(T)!, parameters));
		          		}
		          #endif
		          
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
		/*
		 *
		 * 
		static IEnumerable<Type> GetTypes(MockClass mockClass)
		{
			foreach (Class? @class in mockClass.GetAllClasses())
			{
				foreach (Property? property in @class.Properties)
				{
					yield return property.Type;
				}

				foreach (Method? method in @class.Methods.Where(m => m.ReturnType != Type.Void))
				{
					yield return method.ReturnType;
				}
			}
		}

		sb.AppendLine("\tstatic Mock()");
		sb.AppendLine("\t{");
		foreach (Type type in mocks.SelectMany(x => GetTypes(x.MockClass)).Distinct())
		{
			if (type.IsArray && type.ElementType is { IsTypeParameter: false, })
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new TypedDefaultValueFactory<").Append(type.Fullname)
					.Append(">(");
				if (type.Fullname.EndsWith("[]") && type.Fullname.IndexOf('[') == type.Fullname.LastIndexOf('['))
				{
					sb.Append("Array.Empty<").Append(type.Fullname.Substring(0, type.Fullname.Length - 2))
						.Append(">()");
				}
				else
				{
					//int[,,][,][] -> int[0,0,0][,][]
					string constructorExpression = type.Fullname;
					int idxStart = constructorExpression.IndexOf('[');
					int idxEnd = constructorExpression.IndexOf(']');
					string prefix = constructorExpression.Substring(0, idxStart);
					string firstArrayPart = constructorExpression.Substring(idxStart + 1, idxEnd - idxStart - 1);
					string suffix = constructorExpression.Substring(idxEnd + 1);
					constructorExpression = $"{prefix}[0{firstArrayPart.Replace(",", ",0")}]{suffix}";
					sb.Append("new ").Append(constructorExpression);
				}

				sb.AppendLine("));");
			}
			else if (type.Fullname.StartsWith("System.Collections.Generic.IEnumerable<")
			         && type.Fullname.EndsWith(">")
			         && type.GenericTypeParameters?.Count == 1
			         && !type.GenericTypeParameters.Value.Single().IsTypeParameter)
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new TypedDefaultValueFactory<").Append(type.Fullname)
					.Append(">(Array.Empty<").Append(type.GenericTypeParameters.Value.Single().Fullname)
					.Append(">()));").AppendLine();
			}
			else if (type.TupleTypes is not null
			         && type.GenericTypeParameters.HasValue
			         && type.GenericTypeParameters.Value.All(t => !t.IsTypeParameter))
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new CallbackDefaultValueFactory<").Append(type.Fullname)
					.Append(">(defaultValueGenerator => (").Append(string.Join(", ",
						type.TupleTypes.Value.Select(t => $"defaultValueGenerator.Generate<{t.Fullname}>()")))
					.Append(")));").AppendLine();
			}
			else if (type.Fullname.StartsWith("System.Threading.Tasks.Task<")
			         && type.Fullname.EndsWith(">")
			         && type.GenericTypeParameters?.Count == 1
			         && !type.GenericTypeParameters.Value.Single().IsTypeParameter)
			{
				string innerType = type.GenericTypeParameters.Value.Single().Fullname;
				sb.Append("\t\tDefaultValueGenerator.Register(new ParametrizedCallbackDefaultValueFactory<").Append(type.Fullname)
					.Append(">(");
				sb.Append("(defaultValueGenerator, parameters) => ").AppendLine();
				sb.Append("\t\t{").AppendLine();
				sb.Append("\t\t\tCancellationToken cancellationToken = parameters.OfType<CancellationToken>().FirstOrDefault();").AppendLine();
				sb.Append("\t\t\tif (cancellationToken.IsCancellationRequested)").AppendLine();
				sb.Append("\t\t\t{").AppendLine();
				sb.Append("\t\t\t\treturn System.Threading.Tasks.Task.FromCanceled<").Append(innerType).Append(">(cancellationToken);").AppendLine();
				sb.Append("\t\t\t}").AppendLine();
				sb.Append("\t\t\treturn System.Threading.Tasks.Task.FromResult<").Append(innerType)
					.Append(">(defaultValueGenerator.Generate<").Append(innerType).Append(">());").AppendLine();
				sb.Append("\t\t}");
				sb.Append(", type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.Task<>) && type.GenericTypeArguments[0] == typeof(")
					.Append(innerType).Append(")));").AppendLine();
			}
			else if (type.Fullname.StartsWith("System.Threading.Tasks.ValueTask<")
			         && type.Fullname.EndsWith(">")
			         && type.GenericTypeParameters?.Count == 1
			         && !type.GenericTypeParameters.Value.Single().IsTypeParameter)
			{
				string innerType = type.GenericTypeParameters.Value.Single().Fullname;
				sb.Append("\t\tDefaultValueGenerator.Register(new ParametrizedCallbackDefaultValueFactory<").Append(type.Fullname)
					.Append(">(");
				sb.Append("(defaultValueGenerator, parameters) => ").AppendLine();
				sb.Append("\t\t{").AppendLine();
				sb.Append("\t\t\tCancellationToken cancellationToken = parameters.OfType<CancellationToken>().FirstOrDefault();").AppendLine();
				sb.Append("\t\t\t#if NET8_0_OR_GREATER").AppendLine();
				sb.Append("\t\t\tif (cancellationToken.IsCancellationRequested)").AppendLine();
				sb.Append("\t\t\t{").AppendLine();
				sb.Append("\t\t\t\treturn System.Threading.Tasks.ValueTask.FromCanceled<").Append(innerType).Append(">(cancellationToken);").AppendLine();
				sb.Append("\t\t\t}").AppendLine();
				sb.Append("\t\t\t#endif").AppendLine();
				sb.Append("\t\t\treturn new System.Threading.Tasks.ValueTask<").Append(innerType)
					.Append(">(defaultValueGenerator.Generate<").Append(innerType).Append(">());").AppendLine();
				sb.Append("\t\t}");
				sb.Append(", type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.ValueTask<>) && type.GenericTypeArguments[0] == typeof(")
					.Append(innerType).Append(")));").AppendLine();
			}
			else if (type.Fullname.StartsWith("System.Lazy<")
			         && type.Fullname.EndsWith(">")
			         && type.GenericTypeParameters?.Count == 1
			         && !type.GenericTypeParameters.Value.Single().IsTypeParameter)
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new CallbackDefaultValueFactory<").Append(type.Fullname)
					.Append(">(defaultValueGenerator => new System.Lazy<")
					.Append(type.GenericTypeParameters.Value.Single().Fullname)
					.Append(">(() => defaultValueGenerator.Generate<")
					.Append(type.GenericTypeParameters.Value.Single().Fullname)
					.Append(
						">()), type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Lazy<>) && type.GenericTypeArguments[0] == typeof(")
					.Append(type.GenericTypeParameters.Value.Single().Fullname).Append(")));").AppendLine();
			}
		}

		sb.Append("\t\tDefaultValueGenerator.Register(new RecursiveMockValueFactory());").AppendLine();

		sb.AppendLine("\t}");
		sb.AppendLine();

		 * 
		 */
		return sb.ToString();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
