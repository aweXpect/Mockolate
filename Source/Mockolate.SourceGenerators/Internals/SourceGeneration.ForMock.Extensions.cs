using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Internals;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class SourceGeneration
{
	public static string ForMockExtensions(string name, MockClass mockClass)
	{
		string allClasses = mockClass.ClassName;
		if (mockClass.AdditionalImplementations.Any())
		{
			allClasses += ", " + string.Join(",", mockClass.AdditionalImplementations.Select(c => c.ClassName));
		}

		string[] namespaces =
		[
			.. mockClass.GetAllNamespaces(),
			"Mockolate.Checks",
			"Mockolate.Events",
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

		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.Append("public static class ExtensionsFor").Append(name).AppendLine();
		sb.AppendLine("{");
		if (mockClass.AdditionalImplementations.Any())
		{
			AppendMockExtensions(sb, mockClass, allClasses);
			sb.AppendLine();
		}

		foreach (Class? @class in mockClass.GetAllClasses())
		{
			AppendInvokedExtensions(sb, @class, namespaces, allClasses);
			AppendAccessedExtensions(sb, @class, namespaces, allClasses);
			AppendEventExtensions(sb, @class, namespaces, allClasses);

			if (AppendProtectedMock(sb, @class))
			{
				AppendInvokedExtensions(sb, @class, namespaces, allClasses, true);
				AppendAccessedExtensions(sb, @class, namespaces, allClasses, true);
				AppendEventExtensions(sb, @class, namespaces, allClasses, true);
			}
		}

		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendMockExtensions(StringBuilder sb, MockClass mockClass, string allClasses)
	{
		sb.Append("\textension(Mock<").Append(allClasses).AppendLine("> mock)");
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Class? @class in mockClass.AdditionalImplementations)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up the mock for <see cref=\"").Append(@class.ClassName).Append("\" />")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockSetups<").Append(@class.ClassName).Append("> Setup").Append(@class.ClassName.Replace('.', '_'))
				.AppendLine();
			sb.Append("\t\t\t=> new MockSetups<").Append(@class.ClassName).Append(">.Proxy(mock.Setup);").AppendLine();
			if (@class.Events.Any())
			{
				sb.AppendLine();
				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Raise events on the mock for <see cref=\"").Append(@class.ClassName)
					.Append("\" />").AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic MockRaises<").Append(@class.ClassName).Append("> RaiseOn")
					.Append(@class.ClassName.Replace('.', '_')).AppendLine();
				sb.Append("\t\t\t=> new MockRaises<").Append(@class.ClassName)
					.Append(">(mock.Setup, ((IMock)mock).Interactions);").AppendLine();
			}

			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Check which methods got invoked on the mocked instance for <see cref=\"")
				.Append(@class.ClassName).Append("\" />").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockInvoked<").Append(@class.ClassName).Append(", Mock<").Append(allClasses)
				.Append(">> InvokedOn").Append(@class.ClassName.Replace('.', '_'))
				.AppendLine();
			sb.Append("\t\t\t=> new MockInvoked<").Append(@class.ClassName).Append(", Mock<").Append(allClasses)
				.Append(">>.Proxy(mock.Invoked, ((IMock)mock).Interactions, mock);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Check which properties were accessed on the mocked instance for <see cref=\"")
				.Append(@class.ClassName).Append("\" />").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockAccessed<").Append(@class.ClassName).Append(", Mock<").Append(allClasses)
				.Append(">> AccessedOn").Append(@class.ClassName.Replace('.', '_')).AppendLine();
			sb.Append("\t\t\t=> new MockAccessed<").Append(@class.ClassName).Append(", Mock<").Append(allClasses)
				.Append(">>.Proxy(mock.Accessed, ((IMock)mock).Interactions, mock);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append(
					"\t\t///     Check which events were subscribed or unsubscribed on the mocked instance for <see cref=\"")
				.Append(@class.ClassName).Append("\" />").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockEvent<").Append(@class.ClassName).Append(", Mock<").Append(allClasses)
				.Append(">> EventOn").Append(@class.ClassName.Replace('.', '_')).AppendLine();
			sb.Append("\t\t\t=> new MockEvent<").Append(@class.ClassName).Append(", Mock<").Append(allClasses)
				.Append(">>.Proxy(mock.Event, ((IMock)mock).Interactions, mock);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Exposes the mocked object instance of type <see cref=\"").Append(@class.ClassName)
				.Append("\" />").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic ").Append(@class.ClassName).Append(" ObjectFor").Append(@class.ClassName.Replace('.','_'))
				.AppendLine();
			sb.Append("\t\t\t=> (").Append(@class.ClassName).Append(")mock.Object;").AppendLine();
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

			sb.Append("\t\t/// <inheritdoc cref=\"").Append(@class.ClassName).Append('.').Append(@event.Name)
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

			sb.Append("\t\t/// <inheritdoc cref=\"").Append(@class.ClassName).Append('.').Append(property.Name)
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
					.Append(" ").Append(property.IndexerParameter is not null
						? property.Name.Replace("[]",
							$"[{property.IndexerParameter.Value.Type.GetMinimizedString(namespaces)} {property.IndexerParameter.Value.Name}]")
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
				sb.Append("\t\t\t\treturn _mock.Get<")
					.Append(property.Type.GetMinimizedString(namespaces))
					.Append(">(\"").Append(@class.GetFullName(property.Name)).AppendLine("\");");
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
				sb.Append("\t\t\t\t_mock.Set(\"").Append(@class.GetFullName(property.Name)).AppendLine("\", value);");
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
			if (method.ReturnType != Type.Void)
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


			if (method.ReturnType != Type.Void)
			{
				sb.Append("\t\t\treturn result.Result;").AppendLine();
			}

			sb.AppendLine("\t\t}");
		}

		sb.Append("\t\t# endregion ").Append(@class.ClassName).AppendLine();
	}

	private static bool AppendProtectedMock(StringBuilder sb, Class @class)
	{
		if (@class.Events.All(@event
			    => @event.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)) &&
		    @class.Methods.All(method
			    => method.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)) &&
		    @class.Properties.All(property
			    => property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)))
		{
			return false;
		}

		sb.Append("\textension(Mock<").Append(@class.ClassName).Append(">").Append(" mock)").AppendLine();
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Allows mocking protected methods, events or properties.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic ProtectedMock<").Append(@class.ClassName).Append(", Mock<").Append(@class.ClassName)
			.Append(">> Protected").AppendLine();
		sb.Append("\t\t\t=> new ProtectedMock<").Append(@class.ClassName).Append(", Mock<").Append(@class.ClassName)
			.Append(">>(mock, ((IMock)mock).Interactions, mock);").AppendLine();
		sb.AppendLine("\t}");
		sb.AppendLine();
		return true;
	}

	private static void AppendInvokedExtensions(StringBuilder sb, Class @class, string[] namespaces, string allClasses,
		bool isProtected = false)
	{
		Func<Method, bool> predicate = isProtected
			? new Func<Method, bool>(e
				=> e.ExplicitImplementation is null &&
				   e.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Method, bool>(e
				=> e.ExplicitImplementation is null &&
				   e.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		if (!@class.Methods.Any(predicate))
		{
			return;
		}

		sb.Append("\textension(MockInvoked<").Append(@class.ClassName).Append(", Mock<").Append(allClasses).Append(">>")
			.Append(isProtected ? ".Protected" : "").Append(" mock)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Method method in @class.Methods.Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the method <see cref=\"").Append(@class.ClassName)
				.Append(".").Append(method.Name).Append("(")
				.Append(string.Join(", ",
					method.Parameters.Select(p => p.RefKind.GetString() + p.Type.GetMinimizedString(namespaces))))
				.Append(")\"/> with the given ")
				.Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>"))).Append(".")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic CheckResult<Mock<").Append(allClasses).Append(">> ").Append(method.Name).Append("(");
			int i = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (i++ > 0)
				{
					sb.Append(", ");
				}

				sb.Append(parameter.RefKind switch
					{
						RefKind.Ref => "With.InvokedRefParameter<",
						RefKind.Out => "With.InvokedOutParameter<",
						_ => "With.Parameter<",
					}).Append(parameter.Type.GetMinimizedString(namespaces))
					.Append("> ").Append(parameter.Name);
			}

			sb.Append(")").AppendLine();
			sb.Append("\t\t\t=> ((IMockInvoked<Mock<").Append(allClasses).Append(">>)mock).Method(\"")
				.Append(@class.GetFullName(method.Name))
				.Append("\"");

			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ");
				sb.Append(parameter.Name);
			}

			sb.AppendLine(");");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendAccessedExtensions(StringBuilder sb, Class @class, string[] namespaces, string allClasses,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(e
				=> !e.IsIndexer && e.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Property, bool>(e
				=> !e.IsIndexer &&
				   e.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		if (!@class.Properties.Any(predicate))
		{
			return;
		}

		sb.Append("\textension(MockAccessed<").Append(@class.ClassName).Append(", Mock<").Append(allClasses)
			.Append(">>")
			.Append(isProtected ? ".Protected" : "").Append(" mock)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Property property in @class.Properties.Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the property <see cref=\"").Append(@class.ClassName)
				.Append(".").Append(property.Name).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic CheckResult.Property<Mock<").Append(allClasses).Append(">, ")
				.Append(property.Type.GetMinimizedString(namespaces))
				.Append("> ").Append(property.Name).AppendLine();
			sb.Append("\t\t\t=> new CheckResult.Property<Mock<").Append(allClasses).Append(">, ")
				.Append(property.Type.GetMinimizedString(namespaces))
				.Append(">(mock, \"").Append(@class.GetFullName(property.Name)).Append("\");");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendEventExtensions(StringBuilder sb, Class @class, string[] namespaces, string allClasses,
		bool isProtected = false)
	{
		Func<Event, bool> predicate = isProtected
			? new Func<Event, bool>(e
				=> e.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Event, bool>(e
				=> e.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		if (!@class.Events.Any(predicate))
		{
			return;
		}

		sb.Append("\textension(MockEvent<").Append(@class.ClassName).Append(", Mock<").Append(allClasses).Append(">>")
			.Append(isProtected ? ".Protected" : "")
			.Append(" mock)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Event @event in @class.Events.Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the subscriptions or unsubscription for the event <see cref=\"")
				.Append(@class.ClassName).Append(".").Append(@event.Name).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic CheckResult.Event<Mock<").Append(allClasses).Append(">, ")
				.Append(@event.Type.GetMinimizedString(namespaces)).Append("> ")
				.Append(@event.Name).AppendLine();
			sb.Append("\t\t\t=> new CheckResult.Event<Mock<").Append(allClasses).Append(">, ")
				.Append(@event.Type.GetMinimizedString(namespaces))
				.Append(">(mock, \"").Append(@class.GetFullName(@event.Name)).Append("\");").AppendLine();
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
