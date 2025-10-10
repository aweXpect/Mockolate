using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;

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
			..GlobalUsings,
			.. mockClass.GetAllNamespaces(),
			"Mockolate.Checks",
			"Mockolate.Events",
			"Mockolate.Protected",
			"Mockolate.Setup",
			"Mockolate.Verify",
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
		sb.Append("internal static class ExtensionsFor").Append(name).AppendLine();
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
			sb.Append("\t\t///     Sets up the mock for <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc()).Append("\" />")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockSetup<").Append(@class.ClassName).Append("> Setup").Append(@class.ClassName.Replace('.', '_'))
				.AppendLine();
			sb.Append("\t\t\t=> new MockSetup<").Append(@class.ClassName).Append(">.Proxy(mock.Setup);").AppendLine();
			if (@class.Events.Any())
			{
				sb.AppendLine();
				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Raise events on the mock for <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc())
					.Append("\" />").AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic MockRaises<").Append(@class.ClassName).Append("> RaiseOn")
					.Append(@class.ClassName.Replace('.', '_')).AppendLine();
				sb.Append("\t\t\t=> new MockRaises<").Append(@class.ClassName)
					.Append(">(mock.Setup, ((IMock)mock).Interactions);").AppendLine();
			}

			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the interactions with the mocked subject of <see cref=\"")
				.Append(@class.ClassName.EscapeForXmlDoc()).Append("\" /> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockVerify<").Append(@class.ClassName).Append(", Mock<").Append(allClasses)
				.Append(">> VerifyOn").Append(@class.ClassName.Replace('.', '_'))
				.AppendLine();
			sb.Append("\t\t\t=> new MockVerify<").Append(@class.ClassName).Append(", Mock<").Append(allClasses)
				.Append(">>(((IMock)mock).Interactions, mock);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Exposes the mocked subject of type <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc())
				.Append("\" />").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic ").Append(@class.ClassName).Append(" SubjectFor").Append(@class.ClassName.Replace('.', '_'))
				.AppendLine();
			sb.Append("\t\t\t=> (").Append(@class.ClassName).Append(")mock.Subject;").AppendLine();
		}

		sb.AppendLine("\t}");
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

		sb.Append("\textension(MockVerify<").Append(@class.ClassName).Append(", Mock<").Append(allClasses).Append(">>")
			.Append(isProtected ? ".Protected" : "").Append(" verify)").AppendLine();
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Verifies the method invocations for <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic MockInvoked<").Append(@class.ClassName).Append(", Mock<").Append(allClasses).Append(">>")
			.Append(isProtected ? ".Protected" : "").Append(" Invoked").AppendLine();
		sb.Append("\t\t\t=> new MockInvoked<").Append(@class.ClassName).Append(", Mock<").Append(allClasses).Append(">>")
			.Append(isProtected ? ".Protected" : "").Append("(verify);").AppendLine();
		sb.AppendLine("\t}");
		sb.AppendLine();

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
			sb.Append("\t\t///     Validates the invocations for the method <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc())
				.Append(".").Append(method.Name.EscapeForXmlDoc()).Append("(")
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
					.Append('>');
				if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
				{
					sb.Append('?');
				}
				sb.Append(' ').Append(parameter.Name);
			}

			sb.Append(")").AppendLine();
			sb.Append("\t\t\t=> ((IMockInvoked<Mock<").Append(allClasses).Append(">>)mock).Method(\"")
				.Append(@class.GetFullName(method.Name))
				.Append("\"");

			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ");
				sb.Append(parameter.Name);
				if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
				{
					sb.Append(" ?? With.Null<").Append(parameter.Type.GetMinimizedString(namespaces))
					.Append(">()");
				}
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

		sb.Append("\textension(MockVerify<").Append(@class.ClassName).Append(", Mock<").Append(allClasses).Append(">>")
			.Append(isProtected ? ".Protected" : "").Append(" verify)").AppendLine();
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Verifies the method invocations for <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic MockAccessed<").Append(@class.ClassName).Append(", Mock<").Append(allClasses).Append(">>")
			.Append(isProtected ? ".Protected" : "").Append(" Accessed").AppendLine();
		sb.Append("\t\t\t=> new MockAccessed<").Append(@class.ClassName).Append(", Mock<").Append(allClasses).Append(">>")
			.Append(isProtected ? ".Protected" : "").Append("(verify);").AppendLine();
		sb.AppendLine("\t}");
		sb.AppendLine();
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
			sb.Append("\t\t///     Validates the invocations for the property <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc())
				.Append(".").Append(property.Name.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic CheckResult.Property<Mock<").Append(allClasses).Append(">, ")
				.Append(property.Type.GetMinimizedString(namespaces))
				.Append("> ").Append(property.Name).AppendLine();
			sb.Append("\t\t\t=> new CheckResult.Property<Mock<").Append(allClasses).Append(">, ")
				.Append(property.Type.GetMinimizedString(namespaces))
				.Append(">(mock, \"").Append(@class.GetFullName(property.Name)).Append("\");").AppendLine();
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

		sb.Append("\textension(MockVerify<").Append(@class.ClassName).Append(", Mock<").Append(allClasses).Append(">>")
			.Append(isProtected ? ".Protected" : "").Append(" verify)").AppendLine();
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Verifies the method invocations for <see cref=\"").Append(@class.ClassName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic MockEvent<").Append(@class.ClassName).Append(", Mock<").Append(allClasses).Append(">>")
			.Append(isProtected ? ".Protected" : "").Append(" Event").AppendLine();
		sb.Append("\t\t\t=> new MockEvent<").Append(@class.ClassName).Append(", Mock<").Append(allClasses).Append(">>")
			.Append(isProtected ? ".Protected" : "").Append("(verify);").AppendLine();
		sb.AppendLine("\t}");
		sb.AppendLine();
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
				.Append(@class.ClassName.EscapeForXmlDoc()).Append(".").Append(@event.Name.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
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
