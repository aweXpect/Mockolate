using System.Text;
using Mockolate.SourceGenerators.Entities;
using Mockolate.SourceGenerators.Internals;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string MockRegistration(ICollection<(string Name, MockClass MockClass)> mocks)
	{
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

		StringBuilder sb = InitializeBuilder(mocks.Any()
			?
			[
				"System",
				"Mockolate.Exceptions",
				"Mockolate.Generated",
				"Mockolate.DefaultValues",
			]
			:
			[
				"System",
			]);

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.AppendLine("internal static partial class Mock");
		sb.AppendLine("{");
		sb.AppendLine("\tstatic Mock()");
		sb.AppendLine("\t{");
		foreach (Type type in mocks.SelectMany(x => GetTypes(x.MockClass)).Distinct())
		{
			if (type.IsArray && type.ElementType != null && !type.ElementType.IsTypeParameter)
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
			else if (type.Fullname.StartsWith("System.Collections.Generic.IEnumerable<") && type.Fullname.EndsWith(">")
			         && type.GenericTypeParameters?.Count == 1 &&
			         !type.GenericTypeParameters.Value.Single().IsTypeParameter)
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new TypedDefaultValueFactory<").Append(type.Fullname)
					.Append(">(Array.Empty<").Append(type.GenericTypeParameters.Value.Single().Fullname)
					.Append(">()));").AppendLine();
			}
			else if (type.TupleTypes is not null && type.GenericTypeParameters.HasValue &&
			         type.GenericTypeParameters.Value.All(t => !t.IsTypeParameter))
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new CallbackDefaultValueFactory<").Append(type.Fullname)
					.Append(">(defaultValueGenerator => (").Append(string.Join(", ",
						type.TupleTypes.Value.Select(t => $"defaultValueGenerator.Generate<{t.Fullname}>()")))
					.Append(")));").AppendLine();
			}
			else if (type.Fullname.StartsWith("System.Threading.Tasks.Task<") && type.Fullname.EndsWith(">")
																			  && type.GenericTypeParameters?.Count ==
																			  1 && !type.GenericTypeParameters.Value
																				  .Single().IsTypeParameter)
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new CallbackDefaultValueFactory<").Append(type.Fullname)
					.Append(">(defaultValueGenerator => System.Threading.Tasks.Task.FromResult<")
					.Append(type.GenericTypeParameters.Value.Single().Fullname)
					.Append(">(defaultValueGenerator.Generate<")
					.Append(type.GenericTypeParameters.Value.Single().Fullname)
					.Append(
						">()), type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.Task<>) && type.GenericTypeArguments[0] == typeof(")
					.Append(type.GenericTypeParameters.Value.Single().Fullname).Append(")));").AppendLine();
			}
			else if (type.Fullname.StartsWith("System.Lazy<") && type.Fullname.EndsWith(">")
																			  && type.GenericTypeParameters?.Count ==
																			  1 && !type.GenericTypeParameters.Value
																				  .Single().IsTypeParameter)
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
			else if (type.Fullname.StartsWith("System.Threading.Tasks.ValueTask<") && type.Fullname.EndsWith(">")
			         && type.GenericTypeParameters?.Count == 1 &&
			         !type.GenericTypeParameters.Value.Single().IsTypeParameter)
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new CallbackDefaultValueFactory<").Append(type.Fullname)
					.Append(">(defaultValueGenerator => new System.Threading.Tasks.ValueTask<")
					.Append(type.GenericTypeParameters.Value.Single().Fullname)
					.Append(">(defaultValueGenerator.Generate<")
					.Append(type.GenericTypeParameters.Value.Single().Fullname)
					.Append(
						">()), type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.ValueTask<>) && type.GenericTypeArguments[0] == typeof(")
					.Append(type.GenericTypeParameters.Value.Single().Fullname).Append(")));").AppendLine();
			}
		}

		sb.AppendLine("\t}");

		sb.AppendLine();
		sb.AppendLine("\tprivate partial class MockGenerator");
		sb.AppendLine("\t{");
		sb.AppendLine(
			"\t\tpartial void Generate(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior, params Type[] types)");
		sb.AppendLine("\t\t{");
		int index = 0;
		foreach ((string Name, MockClass MockClass) mock in mocks)
		{
			string prefix;
			if (index++ > 0)
			{
				sb.Append("\t\t\telse ");
				prefix = "\t\t\t         ";
			}
			else
			{
				sb.Append("\t\t\t");
				prefix = "\t\t\t    ";
			}

			sb.Append("if (types.Length == ").Append(mock.MockClass.AdditionalImplementations.Count + 1).Append(" &&")
				.AppendLine();
			sb.Append(prefix).Append("types[0] == typeof(").Append(mock.MockClass.ClassFullName).Append(")");
			int idx = 1;
			foreach (Class? item in mock.MockClass.AdditionalImplementations)
			{
				sb.AppendLine(" &&");
				sb.Append(prefix).Append("types[").Append(idx++).Append("] == typeof(").Append(item.ClassFullName)
					.Append(")");
			}

			sb.AppendLine(")");
			sb.Append("\t\t\t{").AppendLine();
			if (mock.MockClass.IsInterface)
			{
				sb.Append("\t\t\t\t_value = new MockFor").Append(mock.Name).Append("(mockBehavior);").AppendLine();
			}
			else if (mock.MockClass.Constructors?.Count > 0)
			{
				sb.Append(
						"\t\t\t\tif (constructorParameters is null || constructorParameters.Parameters.Length == 0)")
					.AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				if (mock.MockClass.Constructors.Value.Any(mockClass => mockClass.Parameters.Count == 0))
				{
					sb.Append("\t\t\t\t\t_value = new MockFor").Append(mock.Name).Append("(mockBehavior);").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\t\tthrow new MockException(\"No parameterless constructor found for '")
						.Append(mock.MockClass.ClassFullName).Append("'. Please provide constructor parameters.\");")
						.AppendLine();
				}

				sb.Append("\t\t\t\t}").AppendLine();
				int constructorIndex = 0;
				foreach (EquatableArray<MethodParameter> constructorParameters in mock.MockClass.Constructors.Value
					         .Select(constructor => constructor.Parameters))
				{
					constructorIndex++;
					sb.Append("\t\t\t\telse if (constructorParameters.Parameters.Length == ")
						.Append(constructorParameters.Count);
					int constructorParameterIndex = 0;
					foreach (MethodParameter parameter in constructorParameters)
					{
						sb.AppendLine().Append("\t\t\t\t    && TryCast(constructorParameters.Parameters[")
							.Append(constructorParameterIndex++)
							.Append("], mockBehavior, out ").Append(parameter.Type.Fullname).Append(" c").Append(constructorIndex)
							.Append('p')
							.Append(constructorParameterIndex).Append(")");
					}

					sb.Append(")").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\t_value = new MockFor").Append(mock.Name).Append("(");
					for (int i = 1; i <= constructorParameters.Count; i++)
					{
						sb.Append('c').Append(constructorIndex).Append('p').Append(i).Append(", ");
					}

					sb.Append("mockBehavior);").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
				}

				sb.Append("\t\t\t\telse").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\tthrow new MockException($\"Could not find any constructor for '")
					.Append(mock.MockClass.ClassFullName)
					.Append(
						"' that matches the {constructorParameters.Parameters.Length} given parameters ({string.Join(\", \", constructorParameters.Parameters)}).\");")
					.AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
			}

			sb.Append("\t\t\t}").AppendLine();
		}

		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine();
		sb.Append("""
		          	private static bool TryCast<TValue>(object? value, MockBehavior behavior, out TValue result)
		          	{
		          		if (value is TValue typedValue)
		          		{
		          			result = typedValue;
		          			return true;
		          		}
		          	
		          		result = behavior.DefaultValue.Generate<TValue>();
		          		return value is null;
		          	}
		          """);
		sb.AppendLine();
		sb.AppendLine();
		sb.AppendLine("\tprivate class TypedDefaultValueFactory<T>(T value) : IDefaultValueFactory");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t/// <inheritdoc cref=\"IDefaultValueFactory.IsMatch(Type)\" />");
		sb.AppendLine("\t\tpublic bool IsMatch(Type type)");
		sb.AppendLine("\t\t\t=> type == typeof(T);");
		sb.AppendLine();
		sb.AppendLine("\t\t/// <inheritdoc cref=\"IDefaultValueFactory.Create(Type, IDefaultValueGenerator)\" />");
		sb.AppendLine("\t\tpublic object? Create(Type type, IDefaultValueGenerator defaultValueGenerator)");
		sb.AppendLine("\t\t\t=> value;");
		sb.AppendLine("\t}");

		sb.AppendLine();
		sb.AppendLine(
			"\tpublic class CallbackDefaultValueFactory<T>(Func<IDefaultValueGenerator, T> callback, Func<Type, bool>? isMatch = null) : IDefaultValueFactory");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t/// <inheritdoc cref=\"IDefaultValueFactory.IsMatch(Type)\" />");
		sb.AppendLine("\t\tpublic bool IsMatch(Type type)");
		sb.AppendLine("\t\t\t=> isMatch?.Invoke(type) ?? type == typeof(T);");
		sb.AppendLine();
		sb.AppendLine("\t\t/// <inheritdoc cref=\"IDefaultValueFactory.Create(Type, IDefaultValueGenerator)\" />");
		sb.AppendLine("\t\tpublic object? Create(Type type, IDefaultValueGenerator defaultValueGenerator)");
		sb.AppendLine("\t\t\t=> callback(defaultValueGenerator);");
		sb.AppendLine("\t}");
		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
