using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;

namespace Mockolate.SourceGenerators.Internals;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class SourceGeneration
{
	public static string ForMock(string name, MockClass mockClass)
	{
		string[] namespaces =
		[
			..GlobalUsings,
			..mockClass.GetAllNamespaces(),
			"Mockolate.Checks",
			"Mockolate.Events",
			"Mockolate.Exceptions",
			"Mockolate.Protected",
			"Mockolate.Setup",
		];
		StringBuilder sb = new();
		sb.AppendLine(Header);
		foreach (string @namespace in namespaces.Distinct().OrderBy(n => n))
		{
			sb.Append("using ").Append(@namespace).AppendLine(";");
		}

		sb.Append("""

		          namespace Mockolate.Generated;

		          #nullable enable

		          """);
		sb.Append("internal static class For").Append(name).AppendLine();
		sb.AppendLine("{");

		AppendMock(sb, mockClass, namespaces);
		sb.AppendLine();

		if (mockClass.IsInterface || mockClass.Constructors?.Any() == true)
		{
			AppendMockObject(sb, mockClass, namespaces);
		}
		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendMock(StringBuilder sb, MockClass mockClass, string[] namespaces)
	{
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     The mock class for <see cref=\"").Append(mockClass.ClassName.Replace('<', '{').Replace('>', '}')).Append("\" />");
		foreach (Class? additional in mockClass.AdditionalImplementations)
		{
			sb.Append(" and <see cref=\"").Append(additional.ClassName.Replace('<', '{').Replace('>', '}')).Append("\" />");
		}

		sb.AppendLine(".");
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic class Mock : Mock<").Append(mockClass.ClassName);
		foreach (Class? item in mockClass.AdditionalImplementations)
		{
			sb.Append(", ").Append(item.ClassName);
		}

		sb.AppendLine(">");
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <inheritdoc cref=\"Mock\" />").AppendLine();
		sb.Append(
				"\t\tpublic Mock(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior) : base(mockBehavior)")
			.AppendLine();
		sb.AppendLine("\t\t{");
		if (mockClass.IsInterface ||
			(mockClass.Constructors?.Count > 0 &&
			 mockClass.Constructors.Value.All(m => m.Parameters.Count == 0)))
		{
			sb.Append("\t\t\tObject = new MockObject(this);").AppendLine();
		}
		else if (mockClass.Constructors?.Count > 0)
		{
			sb.Append("\t\t\tif (constructorParameters is null || constructorParameters.Parameters.Length == 0)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			if (mockClass.Constructors.Value.Any(mockClass => mockClass.Parameters.Count == 0))
			{
				sb.Append("\t\t\t\tObject = new MockObject(this);").AppendLine();
			}
			else
			{
				sb.Append("\t\t\t\tthrow new MockException(\"No parameterless constructor found for '").Append(mockClass.ClassName).Append("'. Please provide constructor parameters.\");").AppendLine();
			}
			sb.Append("\t\t\t}").AppendLine();
			foreach (Method constructor in mockClass.Constructors)
			{
				sb.Append("\t\t\telse if (constructorParameters.Parameters.Length == ").Append(constructor.Parameters.Count);
				int index = 0;
				foreach (MethodParameter parameter in constructor.Parameters)
				{
					sb.AppendLine().Append("\t\t\t    && TryCast(constructorParameters.Parameters[").Append(index++).Append("], out ").Append(parameter.Type.GetMinimizedString(namespaces)).Append(" p").Append(index).Append(")");
				}
				sb.Append(")").AppendLine();
				sb.Append("\t\t\t{").AppendLine();
				sb.Append("\t\t\t\tObject = new MockObject(this");
				for (int i = 1; i <= constructor.Parameters.Count; i++)
				{
					sb.Append(", p").Append(i);
				}
				sb.Append(");").AppendLine();
				sb.Append("\t\t\t}").AppendLine();
			}
			sb.Append("\t\t\telse").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tthrow new MockException($\"Could not find any constructor for '").Append(mockClass.ClassName).Append("' that matches the {constructorParameters.Parameters.Length} given parameters ({string.Join(\", \", constructorParameters.Parameters)}).\");").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
		}
		else
		{
			sb.Append("\t\t\tthrow new MockException(\"Could not find any constructor at all for the base type '").Append(mockClass.ClassName).Append("'. Therefore mocking is not supported!\");").AppendLine();
		}
		sb.AppendLine("\t\t}");
		sb.AppendLine();
		sb.Append("\t\t/// <inheritdoc cref=\"Mock{").Append(mockClass.ClassName.Replace('<','{').Replace('>','}'))
			.Append(string.Join(", ", mockClass.AdditionalImplementations.Select(x => x.ClassName.Replace('<', '{').Replace('>', '}'))))
			.AppendLine("}.Object\" />");
		sb.Append("\t\tpublic override ").Append(mockClass.ClassName).AppendLine(" Object { get; }");
		sb.AppendLine("\t}");
	}

	private static void AppendMockObject(StringBuilder sb, MockClass mockClass, string[] namespaces)
	{
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     The actual mock object implementing <see cref=\"").Append(mockClass.ClassName.Replace('<', '{').Replace('>', '}'))
			.Append("\" />");
		foreach (Class? additional in mockClass.AdditionalImplementations)
		{
			sb.Append(" and <see cref=\"").Append(additional.ClassName.Replace('<', '{').Replace('>', '}')).Append("\" />");
		}

		sb.AppendLine(".");
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic partial class MockObject : ").Append(mockClass.ClassName);
		foreach (Class? additional in mockClass.AdditionalImplementations)
		{
			sb.Append(",").AppendLine();
			sb.Append("\t\t").Append(additional.ClassName);
		}

		sb.AppendLine().AppendLine("\t{");
		sb.AppendLine("\t\tprivate IMock _mock;");
		sb.AppendLine();
		sb.Append("\t\t/// <inheritdoc cref=\"MockObject\" />").AppendLine();
		if (mockClass.IsInterface ||
			(mockClass.Constructors?.Count > 0 && 
			 mockClass.Constructors.Value.All(m => m.Parameters.Count == 0)))
		{
			sb.AppendLine("\t\tpublic MockObject(IMock mock)");
			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\t_mock = mock;");
			sb.AppendLine("\t\t}");
		}
		else if (mockClass.Constructors?.Count > 0)
		{
			foreach (Method constructor in mockClass.Constructors)
			{
				sb.Append("\t\tpublic MockObject(IMock mock");
				foreach (MethodParameter parameter in constructor.Parameters)
				{
					sb.Append(", ");
					sb.Append(parameter.Type.GetMinimizedString(namespaces)).Append(' ').Append(parameter.Name);
				}

				sb.AppendLine(")");
				sb.Append("\t\t\t: base(");
				int index = 0;
				foreach (MethodParameter parameter in constructor.Parameters)
				{
					if (index++ > 0)
					{
						sb.Append(", ");
					}

					sb.Append(parameter.Name);
				}

				sb.AppendLine(")");
				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t\t_mock = mock;");
				sb.AppendLine("\t\t}");
			}
		}

		sb.AppendLine();
		ImplementClass(sb, mockClass, namespaces, false);
		foreach (Class? additional in mockClass.AdditionalImplementations)
		{
			sb.AppendLine();
			ImplementClass(sb, additional, namespaces, true);
		}

		sb.AppendLine("\t}");
	}

	private static void ImplementClass(StringBuilder sb, Class @class, string[] namespaces,
		bool explicitInterfaceImplementation)
	{
		sb.Append("\t\t# region ").Append(@class.ClassName).AppendLine();
		int count = 0;
		foreach (Event @event in @class.Events)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <inheritdoc cref=\"").Append(@class.ClassName.Replace('<', '{').Replace('>', '}')).Append('.').Append(@event.Name.Replace('<', '{').Replace('>', '}'))
				.AppendLine("\" />");
			if (explicitInterfaceImplementation)
			{
				sb.Append("\t\tevent ").Append(@event.Type.GetMinimizedString(namespaces))
					.Append("? ").Append(@class.ClassName).Append('.').Append(@event.Name).AppendLine();
			}
			else
			{
				sb.Append("\t\t").Append(@event.Accessibility.ToVisibilityString()).Append(' ');
				if (!@class.IsInterface && @event.UseOverride)
				{
					sb.Append("override ");
				}

				sb.Append("event ").Append(@event.Type.GetMinimizedString(namespaces).TrimEnd('?'))
					.Append("? ").Append(@event.Name).AppendLine();
			}

			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tadd => _mock.Raise.AddEvent(\"").Append(@class.GetFullName(@event.Name))
				.Append("\", value?.Target, value?.Method);").AppendLine();
			sb.Append("\t\t\tremove => _mock.Raise.RemoveEvent(\"").Append(@class.GetFullName(@event.Name))
				.Append("\", value?.Target, value?.Method);").AppendLine();
			sb.AppendLine("\t\t}");
		}

		foreach (Property property in @class.Properties)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <inheritdoc cref=\"").Append(@class.ClassName.Replace('<', '{').Replace('>', '}')).Append('.').Append(property.Name.Replace('<', '{').Replace('>', '}'))
				.AppendLine("\" />");
			if (explicitInterfaceImplementation)
			{
				sb.Append("\t\t").Append(property.Type.GetMinimizedString(namespaces))
					.Append(" ").Append(@class.ClassName).Append('.').Append(property.Name).AppendLine();
			}
			else
			{
				sb.Append("\t\t").Append(property.Accessibility.ToVisibilityString()).Append(' ');
				if (!@class.IsInterface && property.UseOverride)
				{
					sb.Append("override ");
				}

				sb.Append(property.Type.GetMinimizedString(namespaces))
					.Append(" ").Append(property.IndexerParameters is not null
						? property.Name.Replace("[]",
							$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.GetMinimizedString(namespaces)} {p.Name}"))}]")
						: property.Name).AppendLine();
			}

			sb.AppendLine("\t\t{");
			if (property.Getter != null && property.Getter.Value.Accessibility != Accessibility.Private)
			{
				sb.Append("\t\t\t");
				if (property.Getter.Value.Accessibility != property.Accessibility)
				{
					sb.Append(property.Getter.Value.Accessibility.ToVisibilityString()).Append(' ');
				}

				sb.AppendLine("get");
				sb.AppendLine("\t\t\t{");
				if (property.IsIndexer && property.IndexerParameters is not null)
				{
					sb.Append("\t\t\t\treturn _mock.GetIndexer<")
						.Append(property.Type.GetMinimizedString(namespaces))
						.Append(">(").Append(string.Join(", ", property.IndexerParameters.Value.Select(p => p.Name))).AppendLine(");");
				}
				else
				{
					sb.Append("\t\t\t\treturn _mock.Get<")
						.Append(property.Type.GetMinimizedString(namespaces))
						.Append(">(\"").Append(@class.GetFullName(property.Name)).AppendLine("\");");
				}
				sb.AppendLine("\t\t\t}");
			}

			if (property.Setter != null && property.Setter.Value.Accessibility != Accessibility.Private)
			{
				sb.Append("\t\t\t");
				if (property.Setter.Value.Accessibility != property.Accessibility)
				{
					sb.Append(property.Setter.Value.Accessibility.ToVisibilityString()).Append(' ');
				}

				sb.AppendLine("set");
				sb.AppendLine("\t\t\t{");
				if (property.IsIndexer && property.IndexerParameters is not null)
				{
					sb.Append("\t\t\t\t_mock.SetIndexer<")
						.Append(property.Type.GetMinimizedString(namespaces))
						.Append(">(value, ").Append(string.Join(", ", property.IndexerParameters.Value.Select(p => p.Name))).AppendLine(");");
				}
				else
				{
					sb.Append("\t\t\t\t_mock.Set(\"").Append(@class.GetFullName(property.Name)).AppendLine("\", value);");
				}
				sb.AppendLine("\t\t\t}");
			}

			sb.AppendLine("\t\t}");
		}

		foreach (Method method in @class.Methods)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <inheritdoc cref=\"").Append(@class.ClassName.Replace('<', '{').Replace('>', '}')).Append('.').Append(method.Name.Replace('<', '{').Replace('>', '}'))
				.Append('(').Append(string.Join(", ",
					method.Parameters.Select(p => p.RefKind.GetString() + p.Type.GetMinimizedString(namespaces))))
				.AppendLine(")\" />");
			if (explicitInterfaceImplementation)
			{
				sb.Append("\t\t");
				sb.Append(method.ReturnType.GetMinimizedString(namespaces)).Append(' ')
					.Append(@class.ClassName).Append('.').Append(method.Name).Append('(');
			}
			else
			{
				sb.Append("\t\t");
				if (method.ExplicitImplementation is null)
				{
					sb.Append(method.Accessibility.ToVisibilityString()).Append(' ');
					if (!@class.IsInterface && method.UseOverride)
					{
						sb.Append("override ");
					}

					sb.Append(method.ReturnType.GetMinimizedString(namespaces)).Append(' ')
						.Append(method.Name).Append('(');
				}
				else
				{
					sb.Append(method.ReturnType.GetMinimizedString(namespaces)).Append(' ')
						.Append(method.ExplicitImplementation).Append('.').Append(method.Name).Append('(');
				}
			}

			int index = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (index++ > 0)
				{
					sb.Append(", ");
				}

				sb.Append(parameter.RefKind.GetString());
				sb.Append(parameter.Type.GetMinimizedString(namespaces)).Append(' ').Append(parameter.Name);
			}

			sb.Append(')');
			sb.AppendLine();
			sb.AppendLine("\t\t{");
			if (method.ReturnType != Entities.Type.Void)
			{
				sb.Append("\t\t\tvar result = _mock.Execute<")
					.Append(method.ReturnType.GetMinimizedString(namespaces))
					.Append(">(\"").Append(@class.GetFullName(method.Name)).Append("\"");
				foreach (MethodParameter p in method.Parameters)
				{
					sb.Append(", ").Append(p.RefKind == RefKind.Out ? "null" : p.Name);
				}

				sb.AppendLine(");");
			}
			else
			{
				sb.Append("\t\t\tvar result = _mock.Execute(\"").Append(@class.GetFullName(method.Name)).Append("\"");
				foreach (MethodParameter p in method.Parameters)
				{
					sb.Append(", ").Append(p.RefKind == RefKind.Out ? "null" : p.Name);
				}

				sb.AppendLine(");");
			}

			foreach (MethodParameter parameter in method.Parameters)
			{
				if (parameter.RefKind == RefKind.Out)
				{
					sb.Append("\t\t\t").Append(parameter.Name).Append(" = result.SetOutParameter<")
						.Append(parameter.Type.GetMinimizedString(namespaces)).Append(">(\"").Append(parameter.Name)
						.AppendLine("\");");
				}
				else if (parameter.RefKind == RefKind.Ref)
				{
					sb.Append("\t\t\t").Append(parameter.Name).Append(" = result.SetRefParameter<")
						.Append(parameter.Type.GetMinimizedString(namespaces)).Append(">(\"").Append(parameter.Name)
						.AppendLine("\", ").Append(parameter.Name).Append(");");
				}
			}


			if (method.ReturnType != Entities.Type.Void)
			{
				sb.Append("\t\t\treturn result.Result;").AppendLine();
			}

			sb.AppendLine("\t\t}");
		}

		sb.Append("\t\t# endregion ").Append(@class.ClassName).AppendLine();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
