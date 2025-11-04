using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Internals;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
#pragma warning disable S3267 // Loops should be simplified with LINQ expressions
internal static partial class Sources
{
	public static string ForMockExtensions(string name, MockClass mockClass)
	{
		StringBuilder sb = InitializeBuilder([
			"Mockolate.Events",
			"Mockolate.Setup",
			"Mockolate.Verify",
		]);

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.Append("internal static class ExtensionsFor").Append(name).AppendLine();
		sb.AppendLine("{");

		if (mockClass.Delegate is not null)
		{
			AppendDelegateExtensions(sb, mockClass, mockClass.Delegate);
		}
		else
		{
			string allClasses = mockClass.ClassFullName;
			if (mockClass.AdditionalImplementations.Any())
			{
				allClasses += ", " + string.Join(",", mockClass.AdditionalImplementations.Select(c => c.ClassFullName));
			}

			if (mockClass.AdditionalImplementations.Any())
			{
				AppendMockExtensions(sb, mockClass, allClasses);
				sb.AppendLine();
			}

			foreach (Class? @class in mockClass.GetAllClasses())
			{
				AppendInvokedExtensions(sb, @class, allClasses);
				AppendGotExtensions(sb, @class, allClasses);
				AppendSetExtensions(sb, @class, allClasses);
				AppendGotIndexerExtensions(sb, @class, allClasses);
				AppendSetIndexerExtensions(sb, @class, allClasses);
				AppendEventExtensions(sb, @class, allClasses);

				if (AppendProtectedMock(sb, @class))
				{
					AppendInvokedExtensions(sb, @class, allClasses, true);
					AppendGotExtensions(sb, @class, allClasses, true);
					AppendSetExtensions(sb, @class, allClasses, true);
					AppendGotIndexerExtensions(sb, @class, allClasses, true);
					AppendSetIndexerExtensions(sb, @class, allClasses, true);
					AppendEventExtensions(sb, @class, allClasses, true);
				}
			}
		}

		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendDelegateExtensions(StringBuilder sb, MockClass mockClass, Method method)
	{
		#region Setup

		sb.Append("\textension(MockSetup<").Append(mockClass.ClassFullName).AppendLine("> setup)");
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Sets up the delegate <see cref=\"").Append(mockClass.ClassFullName.EscapeForXmlDoc())
			.Append("\" /> on the mock.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		if (method.ReturnType != Type.Void)
		{
			sb.Append("\t\tpublic ReturnMethodSetup<")
				.Append(method.ReturnType.Fullname);
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").Append(parameter.Type.Fullname);
			}

			sb.Append("> Delegate(");
		}
		else
		{
			sb.Append("\t\tpublic VoidMethodSetup");
			if (method.Parameters.Count > 0)
			{
				sb.Append('<');
				int index = 0;
				foreach (MethodParameter parameter in method.Parameters)
				{
					if (index++ > 0)
					{
						sb.Append(", ");
					}

					sb.Append(parameter.Type.Fullname);
				}

				sb.Append('>');
			}

			sb.Append(" Delegate(");
		}

		int i = 0;
		foreach (MethodParameter parameter in method.Parameters)
		{
			if (i++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append(parameter.RefKind switch
				{
					RefKind.Ref => "With.RefParameter<",
					RefKind.Out => "With.OutParameter<",
					_ => "With.Parameter<",
				}).Append(parameter.Type.Fullname)
				.Append('>');
			if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
			{
				sb.Append('?');
			}

			sb.Append(' ').Append(parameter.Name);
		}

		sb.Append(")");
		if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
		{
			foreach (GenericParameter gp in method.GenericParameters.Value)
			{
				gp.AppendWhereConstraint(sb, "\t\t\t");
			}
		}
		sb.AppendLine();

		sb.AppendLine("\t\t{");

		if (method.ReturnType != Type.Void)
		{
			sb.Append("\t\t\tvar methodSetup = new ReturnMethodSetup<")
				.Append(method.ReturnType.Fullname);
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").Append(parameter.Type.Fullname);
			}

			sb.Append(">");
		}
		else
		{
			sb.Append("\t\t\tvar methodSetup = new VoidMethodSetup");

			if (method.Parameters.Count > 0)
			{
				sb.Append('<');
				int index = 0;
				foreach (MethodParameter parameter in method.Parameters)
				{
					if (index++ > 0)
					{
						sb.Append(", ");
					}

					sb.Append(parameter.Type.Fullname);
				}

				sb.Append('>');
			}
		}

		sb.Append("(\"").Append(mockClass.ClassFullName).Append('.').Append(method.Name).Append("\"");
		foreach (MethodParameter parameter in method.Parameters)
		{
			sb.Append(", new With.NamedParameter(\"").Append(parameter.Name).Append("\", ").Append(parameter.Name);
			if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
			{
				sb.Append(" ?? With.Null<").Append(parameter.Type.Fullname)
					.Append(">()");
			}

			sb.Append(")");
		}

		sb.Append(");").AppendLine();
		sb.AppendLine("\t\t\tif (setup is IMockSetup mockSetup)");
		sb.AppendLine("\t\t\t{");
		sb.AppendLine("\t\t\t\tmockSetup.RegisterMethod(methodSetup);");
		sb.AppendLine("\t\t\t}");
		sb.AppendLine("\t\t\treturn methodSetup;");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");

		#endregion

		sb.AppendLine();

		#region Verify

		sb.Append("\textension(MockVerify<").Append(mockClass.ClassFullName).Append(", Mock<")
			.Append(mockClass.ClassFullName).Append(">>").Append(" verify)").AppendLine();
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Verifies the delegate invocations for <see cref=\"")
			.Append(mockClass.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic VerificationResult<MockVerify<").Append(mockClass.ClassFullName).Append(", Mock<")
			.Append(mockClass.ClassFullName).Append(">>> Invoked(");
		i = 0;
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
				}).Append(parameter.Type.Fullname)
				.Append('>');
			if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
			{
				sb.Append('?');
			}

			sb.Append(' ').Append(parameter.Name);
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tIMockInvoked<MockVerify<").Append(mockClass.ClassFullName).Append(", Mock<")
			.Append(mockClass.ClassFullName).Append(">>> invoked = new MockInvoked<").Append(mockClass.ClassFullName)
			.Append(", Mock<").Append(mockClass.ClassFullName).Append(">>").Append("(verify);").AppendLine();
		sb.Append("\t\t\treturn invoked.Method(\"")
			.Append(mockClass.ClassFullName).Append('.').Append(method.Name)
			.Append("\"");

		foreach (MethodParameter parameter in method.Parameters)
		{
			sb.Append(", ");
			sb.Append(parameter.Name);
			if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
			{
				sb.Append(" ?? With.Null<").Append(parameter.Type.Fullname)
					.Append(">()");
			}
		}

		sb.AppendLine(");");
		sb.Append("\t\t}").AppendLine();

		sb.AppendLine("\t}");

		#endregion
	}

	private static void AppendMockExtensions(StringBuilder sb, MockClass mockClass, string allClasses)
	{
		sb.Append("\textension(Mock<").Append(allClasses).AppendLine("> mock)");
		sb.AppendLine("\t{");
		int count = 0;
		HashSet<string> usedNames = [];
		foreach (Class? @class in mockClass.DistinctAdditionalImplementations())
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			int nameSuffix = 1;
			string name = @class.ClassName.Replace('.', '_');
			while (!usedNames.Add(name))
			{
				name = $"{@class.ClassName.Replace('.', '_')}__{++nameSuffix}";
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append("\" />")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockSetup<").Append(@class.ClassFullName).Append("> Setup").Append(name)
				.AppendLine();
			sb.Append("\t\t\t=> new MockSetup<").Append(@class.ClassFullName).Append(">.Proxy(mock.Setup);")
				.AppendLine();
			if (@class.AllEvents().Any())
			{
				sb.AppendLine();
				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Raise events on the mock for <see cref=\"")
					.Append(@class.ClassFullName.EscapeForXmlDoc())
					.Append("\" />").AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic MockRaises<").Append(@class.ClassFullName).Append("> RaiseOn")
					.Append(name).AppendLine();
				sb.Append("\t\t\t=> new MockRaises<").Append(@class.ClassFullName)
					.Append(">(mock.Setup, ((IMock)mock).Interactions);").AppendLine();
			}

			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the interactions with the mocked subject of <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\" /> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockVerify<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> VerifyOn").Append(name)
				.AppendLine();
			sb.Append("\t\t\t=> new MockVerify<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">>(((IMock)mock).Interactions, mock);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Exposes the mocked subject of type <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append("\" />").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic ").Append(@class.ClassFullName).Append(" SubjectFor").Append(name)
				.AppendLine();
			sb.Append("\t\t\t=> (").Append(@class.ClassFullName).Append(")mock.Subject;").AppendLine();
		}

		sb.AppendLine("\t}");
	}

	private static bool AppendProtectedMock(StringBuilder sb, Class @class)
	{
		bool hasProtectedEvents = @class.AllEvents().Any(@event
			=> @event.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal);
		bool hasProtectedMethods = @class.AllMethods().Any(method
			=> method.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal);
		bool hasProtectedProperties = @class.AllProperties().Any(property
			=> property.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal);
		if (!hasProtectedEvents && !hasProtectedMethods && !hasProtectedProperties)
		{
			return false;
		}

		sb.Append("\textension(MockSetup<").Append(@class.ClassFullName).Append("> setup)").AppendLine();
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Sets up the protected methods or properties of the mock for <typeparamref name=\"TMock\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic ProtectedMockSetup<").Append(@class.ClassFullName).Append("> Protected").AppendLine();
		sb.Append("\t\t\t=> new ProtectedMockSetup<").Append(@class.ClassFullName).Append(">(setup);").AppendLine();
		sb.AppendLine("\t}");
		sb.AppendLine();
		return true;
	}

	private static void AppendInvokedExtensions(StringBuilder sb, Class @class, string allClasses,
		bool isProtected = false)
	{
		Func<Method, bool> predicate = isProtected
			? new Func<Method, bool>(method
				=> method.ExplicitImplementation is null &&
				   method.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Method, bool>(method
				=> method.ExplicitImplementation is null &&
				   method.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		if (!@class.AllMethods().Any(method => method.ExplicitImplementation is null))
		{
			return;
		}

		if (!isProtected)
		{
			sb.Append("\textension(MockVerify<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the method invocations for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockInvoked<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> Invoked").AppendLine();
			sb.Append("\t\t\t=> new MockInvoked<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">>(verify);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		if (!@class.AllMethods().Any(predicate))
		{
			return;
		}

		if (isProtected)
		{
			sb.Append("\textension(MockInvoked<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected method invocations for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic ProtectedMockInvoked<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">> Protected").AppendLine();
			sb.Append("\t\t\t=> new ProtectedMockInvoked<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>(verify);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		sb.Append("\textension(").Append(isProtected ? "Protected" : "").Append("MockInvoked<")
			.Append(@class.ClassFullName).Append(", Mock<").Append(allClasses).Append(">> mock)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Method method in @class.AllMethods().Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the method <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append(".").Append(method.Name.EscapeForXmlDoc()).Append("(")
				.Append(string.Join(", ",
					method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname)))
				.Append(")\"/> with the given ")
				.Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>"))).Append(".")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>> ").Append(method.Name).Append("(");
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
					}).Append(parameter.Type.Fullname)
					.Append('>');
				if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
				{
					sb.Append('?');
				}

				sb.Append(' ').Append(parameter.Name);
			}

			sb.Append(")");
			if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
			{
				foreach (GenericParameter gp in method.GenericParameters.Value)
				{
					gp.AppendWhereConstraint(sb, "\t\t\t");
				}
			}
			sb.AppendLine();

			sb.Append("\t\t\t=> ((IMockInvoked<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>>)mock).Method(").Append(method.GetUniqueNameString());

			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ");
				sb.Append(parameter.Name);
				if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
				{
					sb.Append(" ?? With.Null<").Append(parameter.Type.Fullname)
						.Append(">()");
				}
			}

			sb.AppendLine(");");
		}

		foreach (Method method in @class.AllMethods()
			         .Where(predicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Parameters.Count > 1 && m.Parameters.All(x => x.RefKind == RefKind.None)))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the method <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append(".").Append(method.Name.EscapeForXmlDoc()).Append("(")
				.Append(string.Join(", ",
					method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname)))
				.Append(")\"/> with the given ")
				.Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>"))).Append(".")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>> ").Append(method.Name).Append("(With.Parameters parameters)");
			if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
			{
				foreach (GenericParameter gp in method.GenericParameters.Value)
				{
					gp.AppendWhereConstraint(sb, "\t\t\t");
				}
			}
			sb.AppendLine();

			sb.Append("\t\t\t=> ((IMockInvoked<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>>)mock).Method(").Append(method.GetUniqueNameString());
			sb.AppendLine(", parameters);");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendGotExtensions(StringBuilder sb, Class @class, string allClasses,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && !property.IsIndexer &&
				   property.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal &&
				   property.Getter != null && property.Getter.Accessibility != Accessibility.Private)
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && !property.IsIndexer &&
				   property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal) &&
				   property.Getter != null && property.Getter.Accessibility != Accessibility.Private);

		if (!@class.AllProperties().Any(property => !property.IsIndexer))
		{
			return;
		}

		if (!isProtected)
		{
			sb.Append("\textension(MockVerify<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the property read access for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockGot<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> Got").AppendLine();
			sb.Append("\t\t\t=> new MockGot<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">>(verify);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		if (!@class.AllProperties().Any(predicate))
		{
			return;
		}

		if (isProtected)
		{
			sb.Append("\textension(MockGot<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected property read access for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic ProtectedMockGot<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> Protected").AppendLine();
			sb.Append("\t\t\t=> new ProtectedMockGot<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>(verify);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		sb.Append("\textension(").Append(isProtected ? "Protected" : "").Append("MockGot<").Append(@class.ClassFullName)
			.Append(", Mock<").Append(allClasses)
			.Append(">> mock)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Property property in @class.AllProperties().Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the property <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append(".").Append(property.Name.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>> ").Append(property.Name).Append("()").AppendLine();
			sb.Append("\t\t\t=> ((IMockGot<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>>)mock).Property(").Append(property.GetUniqueNameString()).Append(");")
				.AppendLine();
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendGotIndexerExtensions(StringBuilder sb, Class @class, string allClasses,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && property.IsIndexer &&
				   property.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal &&
				   property.Getter != null && property.Getter.Accessibility != Accessibility.Private)
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && property.IsIndexer &&
				   property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal) &&
				   property.Getter != null && property.Getter.Accessibility != Accessibility.Private);
		if (!@class.AllProperties().Any(predicate))
		{
			return;
		}

		sb.Append("\textension(MockVerify<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
			.Append(">> verify)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (EquatableArray<MethodParameter>? indexerParameters in @class.AllProperties().Where(predicate)
			         .Select(x => x.IndexerParameters))
		{
			if (indexerParameters is null || indexerParameters.Value.Count == 0)
			{
				continue;
			}

			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the indexer read access for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>> Got").Append(isProtected ? "Protected" : "").Append("Indexer")
				.Append("(").Append(string.Join(", ",
					indexerParameters.Value.Select((p, i) => $"With.Parameter<{p.Type.Fullname}>? parameter{i + 1}")))
				.Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tMockGotIndexer<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> indexer = new(verify);").AppendLine();
			sb.Append("\t\t\treturn ((IMockGotIndexer<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>>)indexer).Got(")
				.Append(string.Join(", ", indexerParameters.Value.Select((p, i) => $"parameter{i + 1}"))).Append(");")
				.AppendLine();
			sb.AppendLine("\t\t}");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendSetExtensions(StringBuilder sb, Class @class, string allClasses,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && !property.IsIndexer &&
				   property.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal &&
				   property.Setter != null && property.Setter.Accessibility != Accessibility.Private)
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && !property.IsIndexer &&
				   property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal) &&
				   property.Setter != null && property.Setter.Accessibility != Accessibility.Private);

		if (!@class.AllProperties().Any(property => !property.IsIndexer))
		{
			return;
		}

		if (!isProtected)
		{
			sb.Append("\textension(MockVerify<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the property write access for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockSet<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> Set").AppendLine();
			sb.Append("\t\t\t=> new MockSet<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">>(verify);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		if (!@class.AllProperties().Any(predicate))
		{
			return;
		}

		if (isProtected)
		{
			sb.Append("\textension(MockSet<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected property write access for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic ProtectedMockSet<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> Protected").AppendLine();
			sb.Append("\t\t\t=> new ProtectedMockSet<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>(verify);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		sb.Append("\textension(").Append(isProtected ? "Protected" : "").Append("MockSet<").Append(@class.ClassFullName)
			.Append(", Mock<").Append(allClasses)
			.Append(">> mock)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Property property in @class.AllProperties().Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the property <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append(".").Append(property.Name.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>> ").Append(property.Name).Append("(With.Parameter<")
				.Append(property.Type.Fullname).Append("> value)").AppendLine();
			sb.Append("\t\t\t=> ((IMockSet<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>>)mock).Property(").Append(property.GetUniqueNameString())
				.Append(", value);").AppendLine();
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendSetIndexerExtensions(StringBuilder sb, Class @class, string allClasses,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && property.IsIndexer &&
				   property.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal &&
				   property.Setter != null && property.Setter.Accessibility != Accessibility.Private)
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null && property.IsIndexer &&
				   property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal) &&
				   property.Setter != null && property.Setter.Accessibility != Accessibility.Private);
		if (!@class.AllProperties().Any(predicate))
		{
			return;
		}

		sb.Append("\textension(MockVerify<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
			.Append(">> verify)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Property indexer in @class.AllProperties().Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			if (indexer.IndexerParameters is null || indexer.IndexerParameters.Value.Count == 0)
			{
				continue;
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the indexer write access for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>> Set").Append(isProtected ? "Protected" : "").Append("Indexer")
				.Append("(")
				.Append(string.Join(", ",
					indexer.IndexerParameters.Value.Select((p, i)
						=> $"With.Parameter<{p.Type.Fullname}>? parameter{i + 1}"))).Append(", With.Parameter<")
				.Append(indexer.Type.Fullname).Append(">? value)").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tMockSetIndexer<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> indexer = new(verify);").AppendLine();
			sb.Append("\t\t\treturn ((IMockSetIndexer<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>>)indexer).Set(value, ")
				.Append(string.Join(", ", indexer.IndexerParameters.Value.Select((p, i) => $"parameter{i + 1}")))
				.Append(");").AppendLine();
			sb.AppendLine("\t\t}");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendEventExtensions(StringBuilder sb, Class @class, string allClasses,
		bool isProtected = false)
	{
		Func<Event, bool> predicate = isProtected
			? new Func<Event, bool>(@event
				=> @event.ExplicitImplementation is null &&
				   @event.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Event, bool>(@event
				=> @event.ExplicitImplementation is null &&
				   @event.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));

		if (!@class.AllEvents().Any())
		{
			return;
		}

		if (!isProtected)
		{
			sb.Append("\textension(MockVerify<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the event subscriptions for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockSubscribedTo<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> SubscribedTo").AppendLine();
			sb.Append("\t\t\t=> new MockSubscribedTo<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>(verify);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the event unsubscriptions for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic MockUnsubscribedFrom<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">> UnsubscribedFrom").AppendLine();
			sb.Append("\t\t\t=> new MockUnsubscribedFrom<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>(verify);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		if (!@class.AllEvents().Any(predicate))
		{
			return;
		}

		if (isProtected)
		{
			sb.Append("\textension(MockRaises<").Append(@class.ClassFullName).Append("> raises)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raise protected events on the mock for <typeparamref name=\"TMock\" />.")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic ProtectedMockRaises<").Append(@class.ClassFullName).Append("> Protected")
				.AppendLine();
			sb.Append("\t\t\t=> new ProtectedMockRaises<").Append(@class.ClassFullName).Append(">(raises);")
				.AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(MockSubscribedTo<").Append(@class.ClassFullName).Append(", Mock<").Append(allClasses)
				.Append(">> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected event subscriptions for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic ProtectedMockSubscribedTo<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">> Protected").AppendLine();
			sb.Append("\t\t\t=> new ProtectedMockSubscribedTo<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>(verify);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(MockUnsubscribedFrom<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected event unsubscriptions for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic ProtectedMockUnsubscribedFrom<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">> Protected").AppendLine();
			sb.Append("\t\t\t=> new ProtectedMockUnsubscribedFrom<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>(verify);").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		sb.Append("\textension(").Append(isProtected ? "Protected" : "").Append("MockSubscribedTo<")
			.Append(@class.ClassFullName).Append(", Mock<").Append(allClasses).Append(">> mock)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Event @event in @class.AllEvents().Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the subscriptions for the event <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append(".").Append(@event.Name.EscapeForXmlDoc())
				.Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>> ")
				.Append(@event.Name).Append("()").AppendLine();
			sb.Append("\t\t\t=> ((IMockSubscribedTo<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>>)mock).Event(").Append(@event.GetUniqueNameString()).Append(");")
				.AppendLine();
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
		sb.Append("\textension(").Append(isProtected ? "Protected" : "").Append("MockUnsubscribedFrom<")
			.Append(@class.ClassFullName).Append(", Mock<").Append(allClasses).Append(">> mock)").AppendLine();
		sb.AppendLine("\t{");
		count = 0;
		foreach (Event @event in @class.AllEvents().Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the unsubscription for the event <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append(".").Append(@event.Name.EscapeForXmlDoc())
				.Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>> ")
				.Append(@event.Name).Append("()").AppendLine();
			sb.Append("\t\t\t=> ((IMockUnsubscribedFrom<MockVerify<").Append(@class.ClassFullName).Append(", Mock<")
				.Append(allClasses).Append(">>>)mock).Event(").Append(@event.GetUniqueNameString()).Append(");")
				.AppendLine();
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}
}
#pragma warning restore S3267 // Loops should be simplified with LINQ expressions
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
