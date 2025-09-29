using System.Text;
using Mockerade.SourceGenerators.Entities;
using Microsoft.CodeAnalysis;
using Type = Mockerade.SourceGenerators.Entities.Type;

namespace Mockerade.SourceGenerators.Internals;

#pragma warning disable S3779 // Cognitive Complexity of methods should not be too high
internal static partial class SourceGeneration
{
	public static string GetMockClass(string name, MockClass mockClass)
	{
		string[] namespaces = mockClass.GetAllNamespaces();
		StringBuilder sb = new();
		foreach (string @namespace in namespaces)
		{
			sb.Append("using ").Append(@namespace).AppendLine(";");
		}

		sb.Append("""
		          using Mockerade.Checks;
		          using Mockerade.Events;
		          using Mockerade.Setup;

		          namespace Mockerade;

		          #nullable enable
		          
		          """);
		sb.Append("public static class For").Append(name).AppendLine();
		sb.AppendLine("{");
		
		AppendMock(sb, name, mockClass);
		sb.AppendLine();

		AppendMockObject(sb, name, mockClass, namespaces);
		if (mockClass.AdditionalImplementations.Any())
		{
			sb.AppendLine();

			AppendMockExtensions(sb, name, mockClass, namespaces);
		}

		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendMock(StringBuilder sb, string name, MockClass mockClass)
	{
		sb.Append("\tpublic class Mock : Mock<").Append(mockClass.ClassName);
		foreach (var item in mockClass.AdditionalImplementations)
		{
			sb.Append(", ").Append(item.ClassName);
		}
		sb.AppendLine(">");
		sb.AppendLine("\t{");
		sb.Append("\t\tpublic Mock(MockBehavior mockBehavior) : base(mockBehavior)").AppendLine();
		sb.AppendLine("\t\t{");
		sb.Append("\t\t\tObject = new MockObject(this);").AppendLine();
		sb.AppendLine("\t\t}");
		sb.AppendLine();
		sb.Append("\t\t/// <inheritdoc cref=\"Mock{").Append(mockClass.ClassName).Append(string.Join(", ", mockClass.AdditionalImplementations.Select(x => x.ClassName))).AppendLine("}.Object\" />");
		sb.Append("\t\tpublic override ").Append(mockClass.ClassName).AppendLine(" Object { get; }");
		sb.AppendLine("\t}");
	}

	private static void AppendMockObject(StringBuilder sb, string name, MockClass mockClass, string[] namespaces)
	{
		sb.Append("\tpublic partial class MockObject(IMock mock) :").AppendLine()
			.Append("\t\t").Append(mockClass.ClassName);
		foreach (var additional in mockClass.AdditionalImplementations)
		{
			sb.AppendLine(",").Append("\t\t").Append(additional.ClassName);
		}
		sb.AppendLine().AppendLine("\t{");
		
		ImplementClass(sb, mockClass, namespaces, false);
		foreach (var additional in mockClass.AdditionalImplementations)
		{
			sb.AppendLine();
			ImplementClass(sb, additional, namespaces, true);
		}

		sb.AppendLine("\t}");
	}

	private static void AppendMockExtensions(StringBuilder sb, string name, MockClass mockClass, string[] namespaces)
	{
		sb.Append("\textension(Mock<").Append(mockClass.ClassName);
		foreach (var item in mockClass.AdditionalImplementations)
		{
			sb.Append(", ").Append(item.ClassName);
		}
		sb.AppendLine("> mock)");
		sb.AppendLine("\t{");
		int count = 0;
		foreach (var @class in mockClass.AdditionalImplementations)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Setup the mock for <see cref=\"").Append(@class.ClassName).Append("\" />").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockSetups<").Append(@class.ClassName).Append("> Setup").Append(@class.ClassName).AppendLine();
			sb.Append("\t\t\t=> new MockSetups<").Append(@class.ClassName).Append(">.Proxy(mock.Setup);").AppendLine();
			if (@class.Events.Any())
			{
				sb.AppendLine();
				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Raise events on the mock for <see cref=\"").Append(@class.ClassName).Append("\" />").AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic MockRaises<").Append(@class.ClassName).Append("> RaiseOn").Append(@class.ClassName).AppendLine();
				sb.Append("\t\t\t=> new MockRaises<").Append(@class.ClassName).Append(">(mock.Setup, ((IMock)mock).Invocations);").AppendLine();
			}
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Check which methods got invoked on the mocked instance for <see cref=\"").Append(@class.ClassName).Append("\" />").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockInvoked<").Append(@class.ClassName).Append("> InvokedOn").Append(@class.ClassName).AppendLine();
			sb.Append("\t\t\t=> new MockInvoked<").Append(@class.ClassName).Append(">.Proxy(mock.Invoked, ((IMock)mock).Invocations);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Check which properties were accessed on the mocked instance for <see cref=\"").Append(@class.ClassName).Append("\" />").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockAccessed<").Append(@class.ClassName).Append("> AccessedOn").Append(@class.ClassName).AppendLine();
			sb.Append("\t\t\t=> new MockAccessed<").Append(@class.ClassName).Append(">.Proxy(mock.Accessed, ((IMock)mock).Invocations);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Exposes the mocked object instance of type <see cref=\"").Append(@class.ClassName).Append("\" />").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic ").Append(@class.ClassName).Append(" ObjectFor").Append(@class.ClassName).AppendLine();
			sb.Append("\t\t\t=> (").Append(@class.ClassName).Append(")mock.Object;").AppendLine();
		}
		sb.AppendLine("\t}");
	}

	private static void ImplementClass(StringBuilder sb, Class @class, string[] namespaces, bool explicitInterfaceImplementation)
	{
		sb.Append("\t\t# region ").Append(@class.ClassName).AppendLine();
		int count = 0;
		foreach (Event @event in @class.Events)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(@class.ClassName).Append('.').Append(@event.Name).AppendLine("\" />");
			if (explicitInterfaceImplementation)
			{
				sb.Append("\t\tevent ").Append(@event.Type.GetMinimizedString(namespaces))
					.Append("? ").Append(@class.ClassName).Append('.').Append(@event.Name).AppendLine();
			}
			else
			{
				sb.Append("\t\t").Append(@event.Accessibility.ToVisibilityString()).Append(' ');
				if (!@class.IsInterface && @event.IsVirtual)
				{
					sb.Append("override ");
				}
				sb.Append("event ").Append(@event.Type.GetMinimizedString(namespaces))
					.Append("? ").Append(@event.Name).AppendLine();
			}
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tadd => mock.Raise.AddEvent(\"").Append(@class.GetFullName(@event.Name)).Append("\", value?.Target, value?.Method);").AppendLine();
			sb.Append("\t\t\tremove => mock.Raise.RemoveEvent(\"").Append(@class.GetFullName(@event.Name)).Append("\", value?.Target, value?.Method);").AppendLine();
			sb.AppendLine("\t\t}");
		}

		foreach (Property property in @class.Properties)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(@class.ClassName).Append('.').Append(property.Name).AppendLine("\" />");
			if (explicitInterfaceImplementation)
			{
				sb.Append("\t\t").Append(property.Type.GetMinimizedString(namespaces))
					.Append(" ").Append(@class.ClassName).Append('.').Append(property.Name).AppendLine();
			}
			else
			{
				sb.Append("\t\t").Append(property.Accessibility.ToVisibilityString()).Append(' ');
				if (!@class.IsInterface && property.IsVirtual)
				{
					sb.Append("override ");
				}
				sb.Append(property.Type.GetMinimizedString(namespaces))
					.Append(" ").Append(property.Name).AppendLine();
			}
			sb.AppendLine("\t\t{");
			if (property.Getter != null && property.Getter.Value.Accessibility != Microsoft.CodeAnalysis.Accessibility.Private)
			{
				sb.Append("\t\t\t");
				if (property.Getter.Value.Accessibility != property.Accessibility)
				{
					sb.Append(property.Getter.Value.Accessibility.ToVisibilityString()).Append(' ');
				}
				sb.AppendLine("get");
				sb.AppendLine("\t\t\t{");
				sb.Append("\t\t\t\treturn mock.Get<")
					.Append(property.Type.GetMinimizedString(namespaces))
					.Append(">(\"").Append(@class.GetFullName(property.Name)).AppendLine("\");");
				sb.AppendLine("\t\t\t}");
			}
			if (property.Setter != null && property.Setter.Value.Accessibility != Microsoft.CodeAnalysis.Accessibility.Private)
			{
				sb.Append("\t\t\t");
				if (property.Setter.Value.Accessibility != property.Accessibility)
				{
					sb.Append(property.Setter.Value.Accessibility.ToVisibilityString()).Append(' ');
				}
				sb.AppendLine("set");
				sb.AppendLine("\t\t\t{");
				sb.Append("\t\t\t\tmock.Set(\"").Append(@class.GetFullName(property.Name)).AppendLine("\", value);");
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
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(@class.ClassName).Append('.').Append(method.Name)
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
				sb.Append("\t\t").Append(method.Accessibility.ToVisibilityString()).Append(' ');
				if (!@class.IsInterface && method.IsVirtual)
				{
					sb.Append("override ");
				}
				sb.Append(method.ReturnType.GetMinimizedString(namespaces)).Append(' ')
					.Append(method.Name).Append('(');
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
			if (method.ReturnType != Type.Void)
			{
				sb.Append("\t\t\tvar result = mock.Execute<")
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
				sb.Append("\t\t\tvar result = mock.Execute(\"").Append(@class.GetFullName(method.Name)).Append("\"");
				foreach (MethodParameter p in method.Parameters)
				{
					sb.Append(", ").Append(p.RefKind == RefKind.Out ? "null" : p.Name);
				}

				sb.AppendLine(");");
			}

			foreach (var parameter in method.Parameters)
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


			if (method.ReturnType != Type.Void)
			{
				sb.Append("\t\t\treturn result.Result;").AppendLine();
			}

			sb.AppendLine("\t\t}");
		}
		sb.Append("\t\t# endregion ").Append(@class.ClassName).AppendLine();
	}
}
#pragma warning restore S3779 // Cognitive Complexity of methods should not be too high
