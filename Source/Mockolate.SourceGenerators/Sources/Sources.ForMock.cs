using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;

namespace Mockolate.SourceGenerators.Internals;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string ForMock(string name, MockClass mockClass)
	{
		StringBuilder sb = InitializeBuilder([
			"Mockolate.Events",
			"Mockolate.Exceptions",
			"Mockolate.Setup",
			"Mockolate.Verify",
		]);

		sb.Append("""
		          namespace Mockolate.Generated;

		          #nullable enable

		          """);
		sb.Append("internal static class For").Append(name).AppendLine();
		sb.AppendLine("{");

		AppendMock(sb, mockClass);
		sb.AppendLine();

		if (mockClass.Delegate is null && (mockClass.IsInterface || mockClass.Constructors?.Any() == true))
		{
			AppendMockSubject(sb, mockClass);
		}
		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendMock(StringBuilder sb, MockClass mockClass)
	{
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     The mock class for <see cref=\"").Append(mockClass.ClassFullName.EscapeForXmlDoc()).Append("\" />");
		foreach (Class? additional in mockClass.AdditionalImplementations)
		{
			sb.Append(" and <see cref=\"").Append(additional.ClassFullName.EscapeForXmlDoc()).Append("\" />");
		}
		sb.AppendLine(".");
		sb.Append("\t/// </summary>").AppendLine();

		sb.Append("\tpublic class Mock : Mock<").Append(mockClass.ClassFullName);
		foreach (Class? item in mockClass.AdditionalImplementations)
		{
			sb.Append(", ").Append(item.ClassFullName);
		}
		sb.AppendLine(">");
		sb.AppendLine("\t{");
		sb.AppendLine("""
					/// <inheritdoc cref="Mock" />
					public Mock(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior) : base(mockBehavior)
					{
			""");
		if (mockClass.Delegate is not null)
		{
			AppendMock_DelegateSubject(sb, mockClass, mockClass.Delegate);
		}
		else if (mockClass.IsInterface ||
			(mockClass.Constructors?.Count > 0 &&
			 mockClass.Constructors.Value.All(m => m.Parameters.Count == 0)))
		{
			AppendMock_InterfaceSubject(sb);
		}
		else if (mockClass.Constructors?.Count > 0)
		{
			AppendMock_BaseClassSubject(sb, mockClass, mockClass.Constructors.Value);
		}
		else
		{
			sb.Append("\t\t\tthrow new MockException(\"Could not find any constructor at all for the base type '").Append(mockClass.ClassFullName).Append("'. Therefore mocking is not supported!\");").AppendLine();
		}
		sb.AppendLine("""
					}
			""");
		sb.AppendLine();
		sb.Append("\t\t/// <inheritdoc cref=\"Mock{").Append(mockClass.ClassFullName.EscapeForXmlDoc())
			.Append(string.Join(", ", mockClass.AdditionalImplementations.Select(x => x.ClassFullName.EscapeForXmlDoc())))
			.AppendLine("}.Subject\" />");
		sb.Append("\t\tpublic override ").Append(mockClass.ClassFullName).AppendLine(" Subject { get; }");
		sb.AppendLine("\t}");
	}

	private static void AppendMock_BaseClassSubject(StringBuilder sb, MockClass mockClass, EquatableArray<Method> constructors)
	{
		sb.Append("\t\t\tif (constructorParameters is null || constructorParameters.Parameters.Length == 0)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		if (constructors.Any(mockClass => mockClass.Parameters.Count == 0))
		{
			sb.Append("\t\t\t\tSubject = new MockSubject(this);").AppendLine();
		}
		else
		{
			sb.Append("\t\t\t\tthrow new MockException(\"No parameterless constructor found for '").Append(mockClass.ClassFullName).Append("'. Please provide constructor parameters.\");").AppendLine();
		}
		sb.Append("\t\t\t}").AppendLine();
		foreach (var constructorParameters in constructors.Select(constructor => constructor.Parameters))
		{
			sb.Append("\t\t\telse if (constructorParameters.Parameters.Length == ").Append(constructorParameters.Count);
			int index = 0;
			foreach (MethodParameter parameter in constructorParameters)
			{
				sb.AppendLine().Append("\t\t\t    && TryCast(constructorParameters.Parameters[").Append(index++).Append("], out ").Append(parameter.Type.Fullname).Append(" p").Append(index).Append(")");
			}
			sb.Append(")").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tSubject = new MockSubject(this");
			for (int i = 1; i <= constructorParameters.Count; i++)
			{
				sb.Append(", p").Append(i);
			}
			sb.Append(");").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
		}
		sb.Append("\t\t\telse").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tthrow new MockException($\"Could not find any constructor for '").Append(mockClass.ClassFullName).Append("' that matches the {constructorParameters.Parameters.Length} given parameters ({string.Join(\", \", constructorParameters.Parameters)}).\");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
	}

	private static void AppendMock_InterfaceSubject(StringBuilder sb)
	{
		sb.Append("\t\t\tSubject = new MockSubject(this);").AppendLine();
	}

	private static void AppendMock_DelegateSubject(StringBuilder sb, MockClass mockClass, Method @delegate)
	{
		sb.Append("\t\t\tSubject = new ").Append(mockClass.ClassFullName).Append("((")
				.Append(string.Join(", ", @delegate.Parameters.Select(p => $"{p.RefKind.GetString()}{p.Name}")))
				.Append(") =>").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		if (@delegate.ReturnType != Entities.Type.Void)
		{
			sb.Append("\t\t\t\tvar result = ((IMock)this).Execute<")
				.Append(@delegate.ReturnType.Fullname)
				.Append(">(\"").Append(mockClass.ClassFullName).Append('.').Append(@delegate.Name).Append("\"");
			foreach (MethodParameter p in @delegate.Parameters)
			{
				sb.Append(", ").Append(p.RefKind == RefKind.Out ? "null" : p.Name);
			}

			sb.AppendLine(");");
		}
		else
		{
			sb.Append("\t\t\t\tvar result = ((IMock)this).Execute(\"").Append(mockClass.ClassFullName).Append('.').Append(@delegate.Name).Append("\"");
			foreach (MethodParameter p in @delegate.Parameters)
			{
				sb.Append(", ").Append(p.RefKind == RefKind.Out ? "null" : p.Name);
			}

			sb.AppendLine(");");
		}

		foreach (MethodParameter parameter in @delegate.Parameters)
		{
			if (parameter.RefKind == RefKind.Out)
			{
				sb.Append("\t\t\t\t").Append(parameter.Name).Append(" = result.SetOutParameter<")
					.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name)
					.AppendLine("\");");
			}
			else if (parameter.RefKind == RefKind.Ref)
			{
				sb.Append("\t\t\t\t").Append(parameter.Name).Append(" = result.SetRefParameter<")
					.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name)
					.Append("\", ").Append(parameter.Name).Append(");").AppendLine();
			}
		}

		if (@delegate.ReturnType != Entities.Type.Void)
		{
			sb.Append("\t\t\t\treturn result.Result;").AppendLine();
		}

		sb.Append("\t\t\t});").AppendLine();
	}

	private static void AppendMockSubject(StringBuilder sb, MockClass mockClass)
	{
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     The actual mock subject implementing <see cref=\"").Append(mockClass.ClassFullName.EscapeForXmlDoc())
			.Append("\" />");
		foreach (Class? additional in mockClass.DistinctAdditionalImplementations())
		{
			sb.Append(" and <see cref=\"").Append(additional.ClassFullName.EscapeForXmlDoc()).Append("\" />");
		}

		sb.AppendLine(".");
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic class MockSubject : ").Append(mockClass.ClassFullName);
		foreach (Class? additional in mockClass.DistinctAdditionalImplementations())
		{
			sb.Append(",").AppendLine();
			sb.Append("\t\t").Append(additional.ClassFullName);
		}

		sb.AppendLine("""
				{
					private IMock _mock;
			
					/// <inheritdoc cref="MockSubject" />
			""");
		if (mockClass.IsInterface)
		{
			AppendMockSubject_InterfaceConstructor(sb);
		}
		else if (mockClass.Constructors?.Count > 0)
		{
			foreach (Method constructor in mockClass.Constructors)
			{
				AppendMockSubject_BaseClassConstructor(sb, constructor);
			}
		}

		sb.AppendLine();
		AppendMockSubject_ImplementClass(sb, mockClass, false);
		foreach (Class? additional in mockClass.DistinctAdditionalImplementations())
		{
			sb.AppendLine();
			AppendMockSubject_ImplementClass(sb, additional, true);
		}

		sb.AppendLine("\t}");
	}

	private static void AppendMockSubject_InterfaceConstructor(StringBuilder sb)
	{
		sb.AppendLine("""
					public MockSubject(IMock mock)
					{
						_mock = mock;
					}
			""");
	}

	private static void AppendMockSubject_BaseClassConstructor(StringBuilder sb, Method constructor)
	{
		sb.Append("\t\tpublic MockSubject(IMock mock");
		foreach (MethodParameter parameter in constructor.Parameters)
		{
			sb.Append(", ");
			sb.Append(parameter.Type.Fullname).Append(' ').Append(parameter.Name);
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
		sb.AppendLine("""
							{
								_mock = mock;
							}
					""");
	}

	private static void AppendMockSubject_ImplementClass(StringBuilder sb, Class @class, bool explicitInterfaceImplementation)
	{
		var className = @class.ClassFullName;
		sb.Append("\t\t#region ").Append(className).AppendLine();
		int count = 0;
		foreach (Event @event in @class.AllEvents())
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			AppendMockSubject_ImplementClass_AddEvent(sb, @event, className, explicitInterfaceImplementation, @class.IsInterface);
		}

		foreach (Property property in @class.AllProperties())
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			AppendMockSubject_ImplementClass_AddProperty(sb, property, className, explicitInterfaceImplementation, @class.IsInterface);
		}

		foreach (Method method in @class.AllMethods())
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			AppendMockSubject_ImplementClass_AddMethod(sb, method, className, explicitInterfaceImplementation, @class.IsInterface);
		}

		sb.Append("\t\t#endregion ").Append(className).AppendLine();
	}

	private static void AppendMockSubject_ImplementClass_AddEvent(StringBuilder sb, Event @event, string className, bool explicitInterfaceImplementation, bool isClassInterface)
	{
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(@event.ContainingType.EscapeForXmlDoc()).Append('.').Append(@event.Name.EscapeForXmlDoc())
			.AppendLine("\" />");
		if (explicitInterfaceImplementation)
		{
			sb.Append("\t\tevent ").Append(@event.Type.Fullname.TrimEnd('?'))
				.Append("? ").Append(className).Append('.').Append(@event.Name).AppendLine();
		}
		else
		{
			sb.Append("\t\t").Append(@event.Accessibility.ToVisibilityString()).Append(' ');
			if (!isClassInterface && @event.UseOverride)
			{
				sb.Append("override ");
			}

			sb.Append("event ").Append(@event.Type.Fullname.TrimEnd('?'))
				.Append("? ").Append(@event.Name).AppendLine();
		}

		sb.AppendLine("\t\t{");
		sb.Append("\t\t\tadd => _mock.Raise.AddEvent(\"").Append(@event.ContainingType).Append('.').Append(@event.Name)
			.Append("\", value?.Target, value?.Method);").AppendLine();
		sb.Append("\t\t\tremove => _mock.Raise.RemoveEvent(\"").Append(@event.ContainingType).Append('.').Append(@event.Name)
			.Append("\", value?.Target, value?.Method);").AppendLine();
		sb.AppendLine("\t\t}");
	}

	private static void AppendMockSubject_ImplementClass_AddProperty(StringBuilder sb, Property property, string className, bool explicitInterfaceImplementation, bool isClassInterface)
	{
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(property.ContainingType.EscapeForXmlDoc()).Append('.').Append(property.IndexerParameters is not null
				? property.Name.Replace("[]",
					$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname}"))}]").EscapeForXmlDoc()
				: property.Name.EscapeForXmlDoc())
			.AppendLine("\" />");
		if (explicitInterfaceImplementation)
		{
			sb.Append("\t\t").Append(property.Type.Fullname)
				.Append(" ").Append(className).Append('.').Append(property.IndexerParameters is not null
					? property.Name.Replace("[]",
						$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname} {p.Name}"))}]")
					: property.Name).AppendLine();
		}
		else
		{
			sb.Append("\t\t").Append(property.Accessibility.ToVisibilityString()).Append(' ');
			if (!isClassInterface && property.UseOverride)
			{
				sb.Append("override ");
			}

			sb.Append(property.Type.Fullname)
				.Append(" ").Append(property.IndexerParameters is not null
					? property.Name.Replace("[]",
						$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname} {p.Name}"))}]")
					: property.Name).AppendLine();
		}

		sb.AppendLine("\t\t{");
		if (property.Getter != null && property.Getter.Accessibility != Accessibility.Private)
		{
			sb.Append("\t\t\t");
			if (property.Getter.Accessibility != property.Accessibility)
			{
				sb.Append(property.Getter.Accessibility.ToVisibilityString()).Append(' ');
			}

			sb.AppendLine("get");
			sb.AppendLine("\t\t\t{");
			if (property.IsIndexer && property.IndexerParameters is not null)
			{
				sb.Append("\t\t\t\treturn _mock.GetIndexer<")
					.Append(property.Type.Fullname)
					.Append(">(").Append(string.Join(", ", property.IndexerParameters.Value.Select(p => p.Name))).AppendLine(");");
			}
			else
			{
				sb.Append("\t\t\t\treturn _mock.Get<")
					.Append(property.Type.Fullname)
					.Append(">(\"").Append(property.ContainingType).Append('.').Append(property.Name).AppendLine("\");");
			}
			sb.AppendLine("\t\t\t}");
		}

		if (property.Setter != null && property.Setter.Accessibility != Accessibility.Private)
		{
			sb.Append("\t\t\t");
			if (property.Setter.Accessibility != property.Accessibility)
			{
				sb.Append(property.Setter.Accessibility.ToVisibilityString()).Append(' ');
			}

			sb.AppendLine("set");
			sb.AppendLine("\t\t\t{");
			if (property.IsIndexer && property.IndexerParameters is not null)
			{
				sb.Append("\t\t\t\t_mock.SetIndexer<")
					.Append(property.Type.Fullname)
					.Append(">(value, ").Append(string.Join(", ", property.IndexerParameters.Value.Select(p => p.Name))).AppendLine(");");
			}
			else
			{
				sb.Append("\t\t\t\t_mock.Set(\"").Append(property.ContainingType).Append('.').Append(property.Name).AppendLine("\", value);");
			}
			sb.AppendLine("\t\t\t}");
		}

		sb.AppendLine("\t\t}");
	}

	private static void AppendMockSubject_ImplementClass_AddMethod(StringBuilder sb, Method method, string className, bool explicitInterfaceImplementation, bool isClassInterface)
	{

		sb.Append("\t\t/// <inheritdoc cref=\"").Append(method.ContainingType.EscapeForXmlDoc()).Append('.').Append(method.Name.EscapeForXmlDoc())
			.Append('(').Append(string.Join(", ",
				method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname)))
			.AppendLine(")\" />");
		if (explicitInterfaceImplementation)
		{
			sb.Append("\t\t");
			sb.Append(method.ReturnType.Fullname).Append(' ')
				.Append(className).Append('.').Append(method.Name).Append('(');
		}
		else
		{
			sb.Append("\t\t");
			if (method.ExplicitImplementation is null)
			{
				sb.Append(method.Accessibility.ToVisibilityString()).Append(' ');
				if (!isClassInterface && method.UseOverride)
				{
					sb.Append("override ");
				}

				sb.Append(method.ReturnType.Fullname).Append(' ')
					.Append(method.Name).Append('(');
			}
			else
			{
				sb.Append(method.ReturnType.Fullname).Append(' ')
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
			sb.Append(parameter.Type.Fullname).Append(' ').Append(parameter.Name);
		}

		sb.Append(')');
		if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
		{
			foreach (GenericParameter gp in method.GenericParameters.Value)
			{
				sb.AppendLine();
				sb.Append("\t\t\t");
				gp.AppendWhereConstraint(sb);
			}
		}

		sb.AppendLine();
		sb.AppendLine("\t\t{");
		if (method.ReturnType != Entities.Type.Void)
		{
			sb.Append("\t\t\tvar result = _mock.Execute<")
				.Append(method.ReturnType.Fullname)
				.Append(">(\"").Append(method.ContainingType).Append('.').Append(method.Name).Append("\"");
			foreach (MethodParameter p in method.Parameters)
			{
				sb.Append(", ").Append(p.RefKind == RefKind.Out ? "null" : p.Name);
			}

			sb.AppendLine(");");
		}
		else
		{
			sb.Append("\t\t\tvar result = _mock.Execute(\"").Append(className).Append('.').Append(method.Name).Append("\"");
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
					.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name)
					.AppendLine("\");");
			}
			else if (parameter.RefKind == RefKind.Ref)
			{
				sb.Append("\t\t\t").Append(parameter.Name).Append(" = result.SetRefParameter<")
					.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name)
					.Append("\", ").Append(parameter.Name).Append(");").AppendLine();
			}
		}

		if (method.ReturnType != Entities.Type.Void)
		{
			sb.Append("\t\t\treturn result.Result;").AppendLine();
		}

		sb.AppendLine("\t\t}");
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
