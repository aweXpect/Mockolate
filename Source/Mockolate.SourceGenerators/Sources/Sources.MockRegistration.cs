using System.Text;
using Mockolate.SourceGenerators.Entities;

namespace Mockolate.SourceGenerators.Internals;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string MockRegistration(ICollection<(string Name, MockClass MockClass)> mocks)
	{
		static IEnumerable<Entities.Type> GetTypes(MockClass mockClass)
		{
			foreach (var @class in mockClass.GetAllClasses())
			{
				foreach (var property in @class.Properties)
				{
					yield return property.Type;
				}
				foreach (var method in @class.Methods.Where(m => m.ReturnType != Entities.Type.Void))
				{
					yield return method.ReturnType;
				}
			}
		}

		StringBuilder sb = InitializeBuilder(mocks.Any()
			? [
				"System",
				"Mockolate.Generated",
				"Mockolate.DefaultValues",
			]
			: [
				"System"
			]);

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.AppendLine("internal static partial class Mock");
		sb.AppendLine("{");
		sb.AppendLine("\tstatic Mock()");
		sb.AppendLine("\t{");
		foreach (Entities.Type type in mocks.SelectMany(x => GetTypes(x.MockClass)).Distinct())
		{
			if (type.IsArray && type.ElementType != null && !type.ElementType.IsTypeParameter)
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new TypedDefaultValueFactory<").Append(type.Fullname).Append(">(");
				if (type.Fullname.EndsWith("[]") && type.Fullname.IndexOf('[') == type.Fullname.LastIndexOf('['))
				{
					sb.Append("Array.Empty<").Append(type.Fullname.Substring(0, type.Fullname.Length -2)).Append(">()");
				}
				else
				{
					//int[,,][,][] -> int[0,0,0][,][]
					var constructorExpression = type.Fullname;
					var idxStart = constructorExpression.IndexOf('[');
					var idxEnd = constructorExpression.IndexOf(']');
					var prefix = constructorExpression.Substring(0, idxStart);
					var firstArrayPart = constructorExpression.Substring(idxStart + 1, idxEnd - idxStart - 1);
					var suffix = constructorExpression.Substring(idxEnd + 1);
					constructorExpression = $"{prefix}[0{firstArrayPart.Replace(",", ",0")}]{suffix}";
					sb.Append("new ").Append(constructorExpression);
				}
				sb.AppendLine("));");
			}
			else if (type.Fullname.StartsWith("System.Collections.Generic.IEnumerable<") && type.Fullname.EndsWith(">")
				&& type.GenericTypeParameters?.Count == 1 && !type.GenericTypeParameters.Value.Single().IsTypeParameter)
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new TypedDefaultValueFactory<").Append(type.Fullname).Append(">(Array.Empty<").Append(type.GenericTypeParameters.Value.Single().Fullname).Append(">()));").AppendLine();
			}
			else if (type.TupleTypes is not null && type.GenericTypeParameters.HasValue && type.GenericTypeParameters.Value.All(t => !t.IsTypeParameter))
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new CallbackDefaultValueFactory<").Append(type.Fullname).Append(">(defaultValueGenerator => (").Append(string.Join(", ", type.TupleTypes.Value.Select(t => $"defaultValueGenerator.Generate<{t.Fullname}>()"))).Append(")));").AppendLine();
			}
			else if (type.Fullname.StartsWith("System.Threading.Tasks.Task<") && type.Fullname.EndsWith(">")
				&& type.GenericTypeParameters?.Count == 1 && !type.GenericTypeParameters.Value.Single().IsTypeParameter)
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new CallbackDefaultValueFactory<").Append(type.Fullname).Append(">(defaultValueGenerator => System.Threading.Tasks.Task.FromResult<").Append(type.GenericTypeParameters.Value.Single().Fullname).Append(">(defaultValueGenerator.Generate<").Append(type.GenericTypeParameters.Value.Single().Fullname).Append(">()), type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.Task<>) && type.GenericTypeArguments[0] == typeof(").Append(type.GenericTypeParameters.Value.Single().Fullname).Append(")));").AppendLine();
			}
			else if (type.Fullname.StartsWith("System.Threading.Tasks.ValueTask<") && type.Fullname.EndsWith(">")
				&& type.GenericTypeParameters?.Count == 1 && !type.GenericTypeParameters.Value.Single().IsTypeParameter)
			{
				sb.Append("\t\tDefaultValueGenerator.Register(new CallbackDefaultValueFactory<").Append(type.Fullname).Append(">(defaultValueGenerator => new System.Threading.Tasks.ValueTask<").Append(type.GenericTypeParameters.Value.Single().Fullname).Append(">(defaultValueGenerator.Generate<").Append(type.GenericTypeParameters.Value.Single().Fullname).Append(">()), type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.ValueTask<>) && type.GenericTypeArguments[0] == typeof(").Append(type.GenericTypeParameters.Value.Single().Fullname).Append(")));").AppendLine();
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
			sb.Append("\t\t\t\t_value = new For").Append(mock.Name)
				.Append(".Mock(constructorParameters, mockBehavior);").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
		}

		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");

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
		sb.AppendLine("\tpublic class CallbackDefaultValueFactory<T>(Func<IDefaultValueGenerator, T> callback, Func<Type, bool>? isMatch = null) : IDefaultValueFactory");
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
