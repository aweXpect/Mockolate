using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Mockolate.SourceGenerators.Internals;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
#pragma warning disable S3267 // Loops should be simplified with LINQ expressions
internal static partial class Sources
{
	public static string ForMockExtensions(string name, MockClass mockClass)
	{
		StringBuilder sb = InitializeBuilder([
			"Mockolate.Exceptions",
			"Mockolate.Raise",
			"Mockolate.Setup",
			"Mockolate.Verify",
		]);

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.Append("internal static class ExtensionsFor").Append(name).AppendLine();
		sb.AppendLine("{");
		sb.AppendLine("""
		              	private static Mock<T> CastToMockOrThrow<T>(this IInteractiveMock<T> subject)
		              	{
		              		if (subject is Mock<T> mock)
		              		{
		              			return mock;
		              		}
		              	
		              		throw new MockException("The subject is no mock.");
		              	}
		              """);
		sb.AppendLine();
		sb.AppendLine("""
		              	private static Mock<T> GetMockOrThrow<T>(this T subject)
		              	{
		              		if (subject is IMockSubject<T> mock)
		              		{
		              			return mock.Mock;
		              		}
		              
		              		if (subject is IHasMockRegistration hasMockRegistration)
		              		{
		              			return new Mock<T>(subject, hasMockRegistration.Registrations);
		              		}
		              	
		              		throw new MockException("The subject is no mock subject.");
		              	}
		              """);
		sb.AppendLine();

		if (mockClass.Delegate is not null)
		{
			AppendDelegateExtensions(sb, mockClass, mockClass.Delegate);
		}
		else
		{
			if (mockClass.AdditionalImplementations.Any())
			{
				AppendMockExtensions(sb, mockClass);
			}

			sb.AppendLine();

			/*
			foreach (Class? @class in mockClass.GetAllClasses())
			{
				var methods = @class.AllMethods().Where(m => m.ExplicitImplementation is null && m.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)).ToList();
				bool hasToString = methods.Any(m => m.IsToString());
				bool hasGetHashCode = methods.Any(m => m.IsGetHashCode());
				bool hasEquals = methods.Any(m => m.IsEquals());
				AppendInvokedExtensions(sb, @class, hasToString, hasGetHashCode, hasEquals);
				AppendGotExtensions(sb, @class);
				AppendSetExtensions(sb, @class);
				AppendGotIndexerExtensions(sb, @class);
				AppendSetIndexerExtensions(sb, @class);
				AppendEventExtensions(sb, @class);

				if (AppendProtectedMock(sb, @class))
				{
					AppendInvokedExtensions(sb, @class, hasToString, hasGetHashCode, hasEquals, true);
					AppendGotExtensions(sb, @class, true);
					AppendSetExtensions(sb, @class, true);
					AppendGotIndexerExtensions(sb, @class, true);
					AppendSetIndexerExtensions(sb, @class, true);
					AppendEventExtensions(sb, @class, true);
				}
			}
			*/
		}

		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendDelegateExtensions(StringBuilder sb, MockClass mockClass, Method method)
	{
		#region Setup

		sb.Append("\textension(IMockSetup<").Append(mockClass.ClassFullName).AppendLine("> setup)");
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
					RefKind.Ref => "Match.IRefParameter<",
					RefKind.Out => "Match.IOutParameter<",
					_ => "Match.IParameter<",
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
			sb.Append(", new Match.NamedParameter(\"").Append(parameter.Name).Append("\", ").Append(parameter.Name);
			if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
			{
				sb.Append(" ?? Match.Null<").Append(parameter.Type.Fullname)
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

		sb.Append("\textension(IMockVerify<").Append(mockClass.ClassFullName).Append(", Mock<")
			.Append(mockClass.ClassFullName).Append(">>").Append(" verify)").AppendLine();
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Verifies the delegate invocations for <see cref=\"")
			.Append(mockClass.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic VerificationResult<").Append(mockClass.ClassFullName).Append("> Invoked(");
		i = 0;
		foreach (MethodParameter parameter in method.Parameters)
		{
			if (i++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append(parameter.RefKind switch
				{
					RefKind.Ref => "Match.IVerifyRefParameter<",
					RefKind.Out => "Match.IVerifyOutParameter<",
					_ => "Match.IParameter<",
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
		sb.Append("\t\t\tIMockInvoked<IMockVerify<").Append(mockClass.ClassFullName).Append(", Mock<")
			.Append(mockClass.ClassFullName).Append(">>> invoked = (IMockInvoked<IMockVerify<").Append(mockClass.ClassFullName).Append(", Mock<")
			.Append(mockClass.ClassFullName).Append(">>>)verify;").AppendLine();
		sb.Append("\t\t\treturn invoked.Method(\"")
			.Append(mockClass.ClassFullName).Append('.').Append(method.Name)
			.Append("\"");

		foreach (MethodParameter parameter in method.Parameters)
		{
			sb.Append(", ");
			sb.Append(parameter.Name);
			if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
			{
				sb.Append(" ?? Match.Null<").Append(parameter.Type.Fullname)
					.Append(">()");
			}
		}

		sb.AppendLine(");");
		sb.Append("\t\t}").AppendLine();

		sb.AppendLine("\t}");

		#endregion
	}

	private static void AppendMockExtensions(StringBuilder sb, MockClass mockClass)
	{
		sb.Append("\textension(").Append(mockClass.ClassFullName).AppendLine(" subject)");
		sb.AppendLine("\t{");
		
		HashSet<string> usedNames = [];
		foreach (Class? @class in mockClass.DistinctAdditionalImplementations())
		{
			sb.AppendLine();
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
			sb.Append("\t\tpublic IMockSetup<").Append(@class.ClassFullName).Append("> Setup").Append(name).Append("Mock")
				.AppendLine();
			sb.Append("\t\t\t=> new Mock<").Append(@class.ClassFullName).Append(">((").Append(@class.ClassFullName).Append(")subject, subject.GetMockOrThrow().Registrations);")
				.AppendLine();
			if (@class.AllEvents().Any())
			{
				sb.AppendLine();
				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Raise events on the mock for <see cref=\"")
					.Append(@class.ClassFullName.EscapeForXmlDoc())
					.Append("\" />").AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic IMockRaises<").Append(@class.ClassFullName).Append("> RaiseOn")
					.Append(name).Append("Mock").AppendLine();
				sb.Append("\t\t\t=> new Mock<").Append(@class.ClassFullName).Append(">((").Append(@class.ClassFullName).Append(")subject, subject.GetMockOrThrow().Registrations);")
					.AppendLine();
			}

			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the interactions with the mocked subject of <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\" /> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerify<").Append(@class.ClassFullName).Append("> VerifyOn").Append(name).Append("Mock")
				.AppendLine();
			sb.Append("\t\t\t=> new Mock<").Append(@class.ClassFullName).Append(">((").Append(@class.ClassFullName).Append(")subject, subject.GetMockOrThrow().Registrations);")
				.AppendLine();
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

		sb.Append("\textension(IMockSetup<").Append(@class.ClassFullName).Append("> setup)").AppendLine();
		sb.AppendLine("\t{");
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append(
				"\t\t///     Sets up the protected methods or properties of the mock for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\" />.")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IProtectedMockSetup<").Append(@class.ClassFullName).Append("> Protected").AppendLine();
		sb.Append("\t\t\t=> (IProtectedMockSetup<").Append(@class.ClassFullName).Append(">)setup;").AppendLine();
		sb.AppendLine("\t}");
		sb.AppendLine();
		return true;
	}

	private static void AppendInvokedExtensions(StringBuilder sb, Class @class,
		bool hasToString, bool hasGetHashCode, bool hasEquals, bool isProtected = false)
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

		string verifyType = $"IMockVerifyInvoked<{@class.ClassFullName}>";
		if (hasToString || hasGetHashCode || hasEquals)
		{
			verifyType = $"IMockVerifyInvoked{(hasToString ? "WithToString" : "")}{(hasEquals ? "WithEquals" : "")}{(hasGetHashCode ? "WithGetHashCode" : "")}<{@class.ClassFullName}>";
		}
		if (!isProtected)
		{
			sb.Append("\textension(IMockVerify<").Append(@class.ClassFullName).Append("> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the method invocations for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic ").Append(verifyType).Append(" Invoked").AppendLine();
			sb.Append("\t\t\t=> (").Append(verifyType).Append(")verify;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		if (!@class.AllMethods().Any(predicate))
		{
			return;
		}

		if (isProtected)
		{
			sb.Append("\textension(").Append(verifyType).Append(" verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected method invocations for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifyInvokedProtected<").Append(@class.ClassFullName).Append("> Protected").AppendLine();
			sb.Append("\t\t\t=> (IMockVerifyInvokedProtected<").Append(@class.ClassFullName).Append(">)verify;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
			verifyType = $"IMockVerifyInvokedProtected<{@class.ClassFullName}>";
		}

		sb.Append("\textension(").Append(verifyType).Append(" verifyInvoked)").AppendLine();
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
				.Append(")\"/>").Append(method.Parameters.Count > 0 ? " with the given " : "")
				.Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>"))).Append(".")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ").Append(method.Name).Append("(");
			int i = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (i++ > 0)
				{
					sb.Append(", ");
				}

				sb.Append(parameter.RefKind switch
					{
						RefKind.Ref => "Match.IVerifyRefParameter<",
						RefKind.Out => "Match.IVerifyOutParameter<",
						_ => "Match.IParameter<",
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

			sb.Append("\t\t\t=> verifyInvoked.CastToMockOrThrow().Method(").Append(method.GetUniqueNameString());

			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ");
				sb.Append(parameter.Name);
				if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
				{
					sb.Append(" ?? Match.Null<").Append(parameter.Type.Fullname)
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
			         .Where(m => m.Parameters.Count > 0))
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
				.Append(")\"/> with the given <paramref name=\"parameters\"/>..")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ").Append(method.Name).Append("(Match.IParameters parameters)");
			if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
			{
				foreach (GenericParameter gp in method.GenericParameters.Value)
				{
					gp.AppendWhereConstraint(sb, "\t\t\t");
				}
			}
			sb.AppendLine();

			sb.Append("\t\t\t=> verifyInvoked.CastToMockOrThrow().Method(").Append(method.GetUniqueNameString());
			sb.AppendLine(", parameters);");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendGotExtensions(StringBuilder sb, Class @class,
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
			sb.Append("\textension(IMockVerify<").Append(@class.ClassFullName).Append("> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the property read access for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifyGot<").Append(@class.ClassFullName).Append("> Got").AppendLine();
			sb.Append("\t\t\t=> (IMockVerifyGot<").Append(@class.ClassFullName).Append(">)verify;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		if (!@class.AllProperties().Any(predicate))
		{
			return;
		}

		if (isProtected)
		{
			sb.Append("\textension(IMockVerifyGot<").Append(@class.ClassFullName).Append("> verifyGot)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected property read access for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifyGotProtected<").Append(@class.ClassFullName).Append("> Protected").AppendLine();
			sb.Append("\t\t\t=> (IMockVerifyGotProtected<").Append(@class.ClassFullName).Append(">)verifyGot;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		sb.Append("\textension(IMockVerifyGot").Append(isProtected ? "Protected" : "").Append("<").Append(@class.ClassFullName)
			.Append("> verifyGot)").AppendLine();
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ").Append(property.Name).Append("()").AppendLine();
			sb.Append("\t\t\t=> verifyGot.CastToMockOrThrow().Property(").Append(property.GetUniqueNameString()).Append(");")
				.AppendLine();
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendGotIndexerExtensions(StringBuilder sb, Class @class,
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

		sb.Append("\textension(IMockVerify<").Append(@class.ClassFullName).Append("> verify)").AppendLine();
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> Got").Append(isProtected ? "Protected" : "").Append("Indexer")
				.Append("(").Append(string.Join(", ",
					indexerParameters.Value.Select((p, i) => $"Match.IParameter<{p.Type.Fullname}>? parameter{i + 1}")))
				.Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\treturn verify.CastToMockOrThrow().GotIndexer(")
				.Append(string.Join(", ", indexerParameters.Value.Select((p, i) => $"parameter{i + 1}"))).Append(");")
				.AppendLine();
			sb.AppendLine("\t\t}");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendSetExtensions(StringBuilder sb, Class @class,
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
			sb.Append("\textension(IMockVerify<").Append(@class.ClassFullName).Append("> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the property write access for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifySet<").Append(@class.ClassFullName).Append("> Set").AppendLine();
			sb.Append("\t\t\t=> (IMockVerifySet<").Append(@class.ClassFullName).Append(">)verify;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		if (!@class.AllProperties().Any(predicate))
		{
			return;
		}

		if (isProtected)
		{
			sb.Append("\textension(IMockVerifySet<").Append(@class.ClassFullName).Append("> verifySet)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected property write access for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifySetProtected<").Append(@class.ClassFullName).Append("> Protected").AppendLine();
			sb.Append("\t\t\t=> (IMockVerifySetProtected<").Append(@class.ClassFullName).Append(">)verifySet;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		sb.Append("\textension(IMockVerifySet").Append(isProtected ? "Protected" : "").Append("<").Append(@class.ClassFullName)
			.Append("> verifySet)").AppendLine();
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ").Append(property.Name).Append("(Match.IParameter<")
				.Append(property.Type.Fullname).Append("> value)").AppendLine();
			sb.Append("\t\t\t=> verifySet.CastToMockOrThrow().Property(").Append(property.GetUniqueNameString())
				.Append(", value);").AppendLine();
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendSetIndexerExtensions(StringBuilder sb, Class @class,
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

		sb.Append("\textension(IMockVerify<").Append(@class.ClassFullName).Append("> verify)").AppendLine();
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> Set").Append(isProtected ? "Protected" : "").Append("Indexer")
				.Append("(")
				.Append(string.Join(", ",
					indexer.IndexerParameters.Value.Select((p, i)
						=> $"Match.IParameter<{p.Type.Fullname}>? parameter{i + 1}"))).Append(", Match.IParameter<")
				.Append(indexer.Type.Fullname).Append(">? value)").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\treturn verify.CastToMockOrThrow().SetIndexer(value, ")
				.Append(string.Join(", ", indexer.IndexerParameters.Value.Select((p, i) => $"parameter{i + 1}")))
				.Append(");").AppendLine();
			sb.AppendLine("\t\t}");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendEventExtensions(StringBuilder sb, Class @class, bool isProtected = false)
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
			sb.Append("\textension(IMockVerify<").Append(@class.ClassFullName).Append("> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the event subscriptions for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifySubscribedTo<").Append(@class.ClassFullName).Append("> SubscribedTo").AppendLine();
			sb.Append("\t\t\t=> (IMockVerifySubscribedTo<").Append(@class.ClassFullName).Append(">)verify;").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the event unsubscriptions for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifyUnsubscribedFrom<").Append(@class.ClassFullName).Append("> UnsubscribedFrom").AppendLine();
			sb.Append("\t\t\t=> (IMockVerifyUnsubscribedFrom<").Append(@class.ClassFullName).Append(">)verify;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		if (!@class.AllEvents().Any(predicate))
		{
			return;
		}

		if (isProtected)
		{
			sb.Append("\textension(IMockRaises<").Append(@class.ClassFullName).Append("> raises)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raise protected events on the mock for <typeparamref name=\"TMock\" />.")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IProtectedMockRaises<").Append(@class.ClassFullName).Append("> Protected")
				.AppendLine();
			sb.Append("\t\t\t=> (IProtectedMockRaises<").Append(@class.ClassFullName).Append(">)raises;")
				.AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(IMockVerifySubscribedTo<").Append(@class.ClassFullName).Append("> verifySubscribedTo)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected event subscriptions for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifySubscribedToProtected<").Append(@class.ClassFullName).Append("> Protected").AppendLine();
			sb.Append("\t\t\t=> (IMockVerifySubscribedToProtected<").Append(@class.ClassFullName).Append(">)verifySubscribedTo;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(IMockVerifyUnsubscribedFrom<").Append(@class.ClassFullName).Append("> verifyUnsubscribedFrom)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected event unsubscriptions for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifyUnsubscribedFromProtected<").Append(@class.ClassFullName).Append("> Protected").AppendLine();
			sb.Append("\t\t\t=> (IMockVerifyUnsubscribedFromProtected<").Append(@class.ClassFullName).Append(">)verifyUnsubscribedFrom;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		sb.Append("\textension(IMockVerifySubscribedTo").Append(isProtected ? "Protected" : "").Append("<")
			.Append(@class.ClassFullName).Append("> verifyEvent)").AppendLine();
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ")
				.Append(@event.Name).Append("()").AppendLine();
			sb.Append("\t\t\t=> verifyEvent.CastToMockOrThrow().SubscribedTo(").Append(@event.GetUniqueNameString()).Append(");")
				.AppendLine();
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
		sb.Append("\textension(IMockVerifyUnsubscribedFrom").Append(isProtected ? "Protected" : "").Append("<")
			.Append(@class.ClassFullName).Append("> verifyEvent)").AppendLine();
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ")
				.Append(@event.Name).Append("()").AppendLine();
			sb.Append("\t\t\t=> verifyEvent.CastToMockOrThrow().UnsubscribedFrom(").Append(@event.GetUniqueNameString()).Append(");")
				.AppendLine();
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}
}
#pragma warning restore S3267 // Loops should be simplified with LINQ expressions
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
