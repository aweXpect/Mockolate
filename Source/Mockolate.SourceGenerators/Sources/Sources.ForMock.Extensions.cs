using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Mockolate.SourceGenerators.Internals;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string ForMockExtensions(string name, Class @class)
	{
		StringBuilder sb = InitializeBuilder([
			"Mockolate.Exceptions",
			"Mockolate.Monitor",
			"Mockolate.Parameters",
			"Mockolate.Raise",
			"Mockolate.Setup",
			"Mockolate.Verify",
		]);

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable

		          """);
		if (@class is MockClass { Delegate: { } @delegate, })
		{
			sb.Append("internal static class MockFor").Append(name).Append("Extensions").AppendLine();
			sb.AppendLine("{");
			sb.Append("\textension(").Append(@class.ClassFullName).AppendLine(" subject)");
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc())
				.AppendLine("\" />.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockSetup<").Append(@class.ClassFullName).AppendLine("> SetupMock").AppendLine();
			sb.Append("\t\t\t=> GetMockOrThrow(subject);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the interactions with the mock for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).AppendLine("\" />.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerify<").Append(@class.ClassFullName).AppendLine("> VerifyMock").AppendLine();
			sb.Append("\t\t\t=> GetMockOrThrow(subject);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the interactions with the mock for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).AppendLine("\" />.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic System.IDisposable MonitorMock(out MockMonitor<").Append(@class.ClassFullName)
				.AppendLine("> monitor)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tmonitor = new MockMonitor<").Append(@class.ClassFullName)
				.AppendLine(">(GetMockOrThrow(subject));").AppendLine();
			sb.Append("\t\t\treturn monitor.Run();").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
			sb.Append("\textension(IMockSetup<").Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			AppendMethodSetup(@class, sb, @delegate, false, "Delegate");
			sb.AppendLine();

			if (@delegate.Parameters.Count > 0)
			{
				AppendMethodSetup(@class, sb, @delegate, true, "Delegate");
			}

			sb.AppendLine("\t}");
			sb.AppendLine();
			sb.Append("\textension(IMockVerify<").Append(@class.ClassFullName).Append("> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the delegate <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append("\"/>").Append(@delegate.Parameters.Count > 0 ? " with the given " : "")
				.Append(string.Join(", ", @delegate.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>")))
				.Append(".")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> Invoked")
				.Append("(");
			int i = 0;
			foreach (MethodParameter parameter in @delegate.Parameters)
			{
				if (i++ > 0)
				{
					sb.Append(", ");
				}

				sb.AppendVerifyParameter(parameter);
				if (parameter.IsNullable())
				{
					sb.Append('?');
				}

				sb.Append(' ').Append(parameter.Name);
			}

			sb.Append(")").AppendLine();

			sb.Append("\t\t\t=> CastToMockOrThrow(verify).Method(").Append(@delegate.GetUniqueNameString());

			foreach (MethodParameter parameter in @delegate.Parameters)
			{
				sb.Append(", ");
				AppendNamedParameter(sb, parameter);
			}

			sb.AppendLine(");");
			sb.AppendLine();

			if (@delegate.Parameters.Count > 0)
			{
				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Validates the invocations for the method <see cref=\"")
					.Append(@class.ClassFullName.EscapeForXmlDoc())
					.Append(".").Append(@delegate.Name.EscapeForXmlDoc()).Append("(")
					.Append(string.Join(", ",
						@delegate.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname)))
					.Append(")\"/> with the given <paramref name=\"parameters\"/>..")
					.AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> Invoked")
					.Append("(IParameters parameters)").AppendLine();

				sb.Append("\t\t\t=> CastToMockOrThrow(verify).Method(").Append(@delegate.GetUniqueNameString());
				sb.AppendLine(", parameters);");
			}

			sb.AppendLine("\t}");
			sb.AppendLine();
			sb.AppendLine("""
			              	private static Mock<T> CastToMockOrThrow<T>(IInteractiveMock<T> subject)
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
			              	private static MockRegistration CastToMockRegistrationOrThrow<T>(IInteractiveMock<T> subject)
			              	{
			              		if (subject is IHasMockRegistration mock)
			              		{
			              			return mock.Registrations;
			              		}
			              	
			              		throw new MockException("The subject is no mock.");
			              	}
			              """);
			sb.AppendLine();
			sb.AppendLine("""
			              	private static Mock<T> GetMockOrThrow<T>(T subject) where T : System.Delegate
			              	{
			              		var target = subject.Target;
			              		if (target is IMockSubject<T> mock)
			              		{
			              			return mock.Mock;
			              		}
			              	
			              		throw new MockException("The delegate is no mock subject.");
			              	}
			              """);
			sb.AppendLine("}");
		}
		else
		{
			sb.Append("internal static class MockFor").Append(name).Append("Extensions").AppendLine();
			sb.AppendLine("{");
			sb.Append("\textension(").Append(@class.ClassFullName).AppendLine(" subject)");
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc())
				.AppendLine("\" />.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockSetup<").Append(@class.ClassFullName).AppendLine("> SetupMock").AppendLine();
			sb.Append("\t\t\t=> GetMockOrThrow(subject);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raise events on the mock for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).AppendLine("\" />.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockRaises<").Append(@class.ClassFullName).AppendLine("> RaiseOnMock").AppendLine();
			sb.Append("\t\t\t=> GetMockOrThrow(subject);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the interactions with the mock for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).AppendLine("\" />.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerify<").Append(@class.ClassFullName).AppendLine("> VerifyMock").AppendLine();
			sb.Append("\t\t\t=> GetMockOrThrow(subject);").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the interactions with the mock for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).AppendLine("\" />.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic System.IDisposable MonitorMock(out MockMonitor<").Append(@class.ClassFullName)
				.AppendLine("> monitor)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tmonitor = new MockMonitor<").Append(@class.ClassFullName)
				.AppendLine(">(GetMockOrThrow(subject));").AppendLine();
			sb.Append("\t\t\treturn monitor.Run();").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			AppendRaisesExtensions(sb, @class);
			AppendPropertySetupExtensions(sb, @class);
			AppendIndexerSetupExtensions(sb, @class);
			AppendMethodSetupExtensions(sb, @class);

			if (@class.AllEvents().Any(@event
				    => @event.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)) ||
			    @class.AllMethods().Any(method
				    => method.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)) ||
			    @class.AllProperties().Any(property
				    => property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)))
			{
				AppendRaisesExtensions(sb, @class, true);
				AppendPropertySetupExtensions(sb, @class, true);
				AppendIndexerSetupExtensions(sb, @class, true);
				AppendMethodSetupExtensions(sb, @class, true);
			}

			List<Method> methods = @class.AllMethods().Where(method
				=> method.ExplicitImplementation is null &&
				   method.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)).ToList();
			bool hasToString = methods.Any(method => method.IsToString());
			bool hasGetHashCode = methods.Any(method => method.IsGetHashCode());
			bool hasEquals = methods.Any(method => method.IsEquals());
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

			sb.AppendLine("""
			              	private static Mock<T> CastToMockOrThrow<T>(IInteractiveMock<T> subject)
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
			              	private static MockRegistration CastToMockRegistrationOrThrow<T>(IInteractiveMock<T> subject)
			              	{
			              		if (subject is IHasMockRegistration mock)
			              		{
			              			return mock.Registrations;
			              		}
			              	
			              		throw new MockException("The subject is no mock.");
			              	}
			              """);
			sb.AppendLine();
			sb.AppendLine("""
			              	private static Mock<T> GetMockOrThrow<T>(T subject)
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
			sb.AppendLine("}");
		}

		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendMethodSetup(Class @class, StringBuilder sb, Method method, bool useParameters,
		string? methodNameOverride = null)
	{
		string methodName = methodNameOverride ?? method.Name;
		sb.Append("\t\t/// <summary>").AppendLine();
		if (methodNameOverride is null)
		{
			sb.Append("\t\t///     Setup for the method <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append(".")
				.Append(method.Name.EscapeForXmlDoc()).Append("(")
				.Append(string.Join(", ",
					method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname)))
				.Append(")\"/>");
		}
		else
		{
			sb.Append("\t\t///     Setup for the delegate <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append("\"/>");
		}

		if (useParameters)
		{
			sb.Append(" with the given <paramref name=\"parameters\" />");
		}
		else if (method.Parameters.Any())
		{
			sb.Append(" with the given ")
				.Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>")));
		}

		sb.Append(".").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		if (method.ReturnType != Type.Void)
		{
			sb.Append("\t\tpublic IReturnMethodSetup<").AppendTypeOrWrapper(method.ReturnType);

			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append("> ");
			sb.Append(methodName).Append("(");
		}
		else
		{
			sb.Append("\t\tpublic IVoidMethodSetup");
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

					sb.AppendTypeOrWrapper(parameter.Type);
				}

				sb.Append('>');
			}

			sb.Append(' ').Append(methodName).Append("(");
		}

		if (useParameters)
		{
			sb.Append("IParameters parameters)");
		}
		else
		{
			int i = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (i++ > 0)
				{
					sb.Append(", ");
				}

				sb.Append(parameter.ToParameter());
				if (parameter.IsNullable())
				{
					sb.Append('?');
				}

				sb.Append(' ').Append(parameter.Name);
			}

			sb.Append(")");
		}

		if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
		{
			foreach (GenericParameter gp in method.GenericParameters.Value)
			{
				gp.AppendWhereConstraint(sb, "\t\t\t");
			}
		}

		sb.AppendLine();
		sb.AppendLine("\t\t{");

		if (@class is { ClassName: "HttpClient", ClassFullName: "System.Net.Http.HttpClient", } &&
		    method.Name.StartsWith("Send"))
		{
			sb.Append("\t\t\tif (setup is Mock<System.Net.Http.HttpClient> httpClientMock &&").AppendLine();
			sb.Append(
					"\t\t\t    httpClientMock.ConstructorParameters[0] is IMockSubject<System.Net.Http.HttpMessageHandler> httpMessageHandlerMock &&")
				.AppendLine();
			sb.Append(
					"\t\t\t    httpMessageHandlerMock.Mock is IMockMethodSetup<System.Net.Http.HttpMessageHandler> httpMessageHandlerSetup)")
				.AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			AppendMethodSetupBody(sb, method, useParameters,
				"\t\t\t\t",
				method.GetUniqueNameString().Replace("System.Net.Http.HttpMessageInvoker",
					"System.Net.Http.HttpMessageHandler"),
				"httpMessageHandlerSetup");
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\telse").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			AppendMethodSetupBody(sb, method, useParameters, "\t\t\t\t");
			sb.Append("\t\t\t}").AppendLine();
		}
		else
		{
			AppendMethodSetupBody(sb, method, useParameters);
		}

		sb.AppendLine("\t\t}");
	}

	private static void AppendMethodSetupBody(StringBuilder sb, Method method, bool useParameters,
		string indentation = "\t\t\t", string? methodNameOverride = null, string setupVariableName = "setup")
	{
		if (method.ReturnType != Type.Void)
		{
			sb.Append(indentation).Append("var methodSetup = new ReturnMethodSetup<")
				.AppendTypeOrWrapper(method.ReturnType);

			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append(">");
		}
		else
		{
			sb.Append(indentation).Append("var methodSetup = new VoidMethodSetup");

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

					sb.AppendTypeOrWrapper(parameter.Type);
				}

				sb.Append('>');
			}
		}

		if (useParameters)
		{
			sb.Append("(").Append(methodNameOverride ?? method.GetUniqueNameString()).Append(", parameters);")
				.AppendLine();
			sb.Append(indentation).Append("CastToMockRegistrationOrThrow(").Append(setupVariableName)
				.Append(").SetupMethod(methodSetup);").AppendLine();
			sb.Append(indentation).Append("return methodSetup;").AppendLine();
		}
		else
		{
			sb.Append("(").Append(methodNameOverride ?? method.GetUniqueNameString());
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ");
				AppendNamedParameter(sb, parameter);
			}

			sb.Append(");").AppendLine();
			sb.Append(indentation).Append("CastToMockRegistrationOrThrow(").Append(setupVariableName)
				.Append(").SetupMethod(methodSetup);").AppendLine();
			sb.Append(indentation).Append("return methodSetup;").AppendLine();
		}
	}

	private static void AppendRaisesExtensions(StringBuilder sb, Class @class,
		bool isProtected = false)
	{
		Func<Event, bool> predicate = isProtected
			? new Func<Event, bool>(@event
				=> @event.ExplicitImplementation is null &&
				   @event.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Event, bool>(@event
				=> @event.ExplicitImplementation is null &&
				   @event.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		if (!@class.AllEvents().Any(predicate))
		{
			return;
		}

		sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockRaises<")
			.Append(@class.ClassFullName).Append("> mockRaises)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Event @event in @class.AllEvents().Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raise the <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append(".").Append(@event.Name.EscapeForXmlDoc())
				.Append("\"/> event.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic void ").Append(@event.Name).Append("(")
				.Append(FormatParametersWithTypeAndName(@event.Delegate.Parameters))
				.Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tCastToMockRegistrationOrThrow(mockRaises).Raise(").Append(@event.GetUniqueNameString())
				.Append(", ")
				.Append(FormatParametersAsNames(@event.Delegate.Parameters)).Append(");").AppendLine();
			sb.AppendLine("\t\t}");
		}

		foreach (Event @event in @class.AllEvents()
			         .Where(predicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Delegate.Parameters.Count > 0))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raise the <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append(".").Append(@event.Name.EscapeForXmlDoc())
				.Append("\"/> event.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic void ").Append(@event.Name).Append("(IDefaultEventParameters parameters)")
				.AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tMock<").Append(@class.ClassFullName).Append("> mock = CastToMockOrThrow(mockRaises);")
				.AppendLine();
			sb.Append("\t\t\tMockBehavior mockBehavior = mock.Registrations.Behavior;").AppendLine();
			sb.Append("\t\t\tmock.Registrations.Raise(").Append(@event.GetUniqueNameString()).Append(", ")
				.Append(string.Join(", ",
					@event.Delegate.Parameters.Select(p
						=> $"mockBehavior.DefaultValue.Generate(default({p.Type.Fullname.TrimEnd('?')}))")))
				.Append(");").AppendLine();
			sb.AppendLine("\t\t}");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendPropertySetupExtensions(StringBuilder sb, Class @class,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(property
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: false, Accessibility: Accessibility.Protected or Accessibility.ProtectedOrInternal,
				   })
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: false,
					   Accessibility: not (Accessibility.Protected or Accessibility.ProtectedOrInternal),
				   });

		if (@class.AllProperties().Any(predicate))
		{
			sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockSetup<")
				.Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up properties on the mock for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic I").Append(isProtected ? "Protected" : "").Append("MockPropertySetup<")
				.Append(@class.ClassFullName).Append(">")
				.Append("Property").AppendLine();
			sb.Append("\t\t\t=> (I").Append(isProtected ? "Protected" : "").Append("MockPropertySetup<")
				.Append(@class.ClassFullName).Append(">)setup;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockPropertySetup<")
				.Append(@class.ClassFullName).Append(">")
				.Append("setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Property property in @class.AllProperties().Where(predicate))
			{
				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Setup for the property <see cref=\"")
					.Append(@class.ClassFullName.EscapeForXmlDoc()).Append(".")
					.Append(property.Name.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic IPropertySetup<").Append(property.Type.Fullname).Append("> ")
					.Append(property.Name).AppendLine();

				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t\tget");
				sb.AppendLine("\t\t\t{");
				sb.Append("\t\t\t\tvar propertySetup = new PropertySetup<").Append(property.Type.Fullname)
					.Append(">(").Append(property.GetUniqueNameString()).Append(");").AppendLine();
				sb.AppendLine("\t\t\t\tCastToMockRegistrationOrThrow(setup).SetupProperty(propertySetup);")
					.AppendLine();
				sb.AppendLine("\t\t\t\treturn propertySetup;");
				sb.AppendLine("\t\t\t}");
				sb.AppendLine("\t\t}");
				sb.AppendLine();
			}

			sb.AppendLine("\t}");
			sb.AppendLine();
		}
	}

	private static void AppendIndexerSetupExtensions(StringBuilder sb, Class @class,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(property
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: true, Accessibility: Accessibility.Protected or Accessibility.ProtectedOrInternal,
				   })
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: true,
					   Accessibility: not (Accessibility.Protected or Accessibility.ProtectedOrInternal),
				   });

		if (@class.AllProperties().Any(predicate))
		{
			sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockSetup<")
				.Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Property indexer in @class.AllProperties().Where(predicate))
			{
				if (count++ > 0)
				{
					sb.AppendLine();
				}

				sb.Append("\t\t/// <summary>").AppendLine();
				sb.Append("\t\t///     Sets up the ").Append(indexer.Type.Fullname)
					.Append(" indexer on the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc())
					.Append("\" />.")
					.AppendLine();
				sb.Append("\t\t/// </summary>").AppendLine();
				sb.Append("\t\tpublic IIndexerSetup<").AppendTypeOrWrapper(indexer.Type);

				foreach (MethodParameter parameter in indexer.IndexerParameters!)
				{
					sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
				}

				sb.Append("> Indexer").Append("(").Append(string.Join(", ",
						indexer.IndexerParameters.Value.Select((p, i)
							=> p.ToParameter() + (p.IsNullable() ? "?" : "") + $" parameter{i + 1}"))).Append(")")
					.AppendLine();
				sb.Append("\t\t{").AppendLine();
				sb.Append("\t\t\tvar indexerSetup = new IndexerSetup<").AppendTypeOrWrapper(indexer.Type);

				foreach (MethodParameter parameter in indexer.IndexerParameters!)
				{
					sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
				}
				sb.Append(">(").Append(string.Join(", ",
						indexer.IndexerParameters.Value.Select((p, i) => $"new NamedParameter(\"{p.Name}\", (IParameter)(parameter{i + 1}{
							(p.IsNullable() ? $" ?? It.IsNull<{p.Type.Fullname}>()" : "")}))")))
					.Append(");").AppendLine();
				sb.Append("\t\t\tCastToMockRegistrationOrThrow(setup).SetupIndexer(indexerSetup);").AppendLine();
				sb.Append("\t\t\treturn indexerSetup;").AppendLine();
				sb.Append("\t\t}").AppendLine();
			}

			sb.AppendLine("\t}");
			sb.AppendLine();
		}
	}

	private static void AppendMethodSetupExtensions(StringBuilder sb, Class @class,
		bool isProtected = false)
	{
		Func<Method, bool> predicate = isProtected
			? new Func<Method, bool>(method
				=> method.ExplicitImplementation is null &&
				   method.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Method, bool>(method
				=> method.ExplicitImplementation is null &&
				   method.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		List<Method> methods = @class.AllMethods().Where(predicate).ToList();
		if (methods.Any())
		{
			bool hasToString = !isProtected && methods.Any(m => m.IsToString());
			bool hasGetHashCode = !isProtected && methods.Any(m => m.IsGetHashCode());
			bool hasEquals = !isProtected && methods.Any(m => m.IsEquals());
			sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockSetup<")
				.Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Sets up methods on the mock for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic I").Append(isProtected ? "Protected" : "").Append("MockMethodSetup")
				.Append(hasToString ? "WithToString" : "").Append(hasEquals ? "WithEquals" : "")
				.Append(hasGetHashCode ? "WithGetHashCode" : "").Append("<").Append(@class.ClassFullName).Append(">")
				.Append(" Method").AppendLine();
			sb.Append("\t\t\t=> (I").Append(isProtected ? "Protected" : "").Append("MockMethodSetup")
				.Append(hasToString ? "WithToString" : "").Append(hasEquals ? "WithEquals" : "")
				.Append(hasGetHashCode ? "WithGetHashCode" : "").Append("<").Append(@class.ClassFullName)
				.Append(">)setup")
				.Append(";").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(I").Append(isProtected ? "Protected" : "").Append("MockMethodSetup<")
				.Append(@class.ClassFullName).Append("> setup)").AppendLine();
			sb.AppendLine("\t{");
			int count = 0;
			foreach (Method method in methods)
			{
				if (method.IsToString() || method.IsGetHashCode() || method.IsEquals())
				{
					continue;
				}

				if (count++ > 0)
				{
					sb.AppendLine();
				}

				AppendMethodSetup(@class, sb, method, false);
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

				AppendMethodSetup(@class, sb, method, true);
			}

			sb.AppendLine("\t}");
			sb.AppendLine();
		}
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
			verifyType =
				$"IMockVerifyInvoked{(hasToString ? "WithToString" : "")}{(hasEquals ? "WithEquals" : "")}{(hasGetHashCode ? "WithGetHashCode" : "")}<{@class.ClassFullName}>";
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
			sb.Append("\t\tpublic IMockVerifyInvokedProtected<").Append(@class.ClassFullName).Append("> Protected")
				.AppendLine();
			sb.Append("\t\t\t=> (IMockVerifyInvokedProtected<").Append(@class.ClassFullName).Append(">)verify;")
				.AppendLine();
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ").Append(method.Name)
				.Append("(");
			int i = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (i++ > 0)
				{
					sb.Append(", ");
				}

				sb.AppendVerifyParameter(parameter);
				if (parameter.IsNullable())
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

			sb.Append("\t\t\t=> CastToMockOrThrow(verifyInvoked).Method(").Append(method.GetUniqueNameString());

			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ");
				AppendNamedParameter(sb, parameter);
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ").Append(method.Name)
				.Append("(IParameters parameters)");
			if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
			{
				foreach (GenericParameter gp in method.GenericParameters.Value)
				{
					gp.AppendWhereConstraint(sb, "\t\t\t");
				}
			}

			sb.AppendLine();

			sb.Append("\t\t\t=> CastToMockOrThrow(verifyInvoked).Method(").Append(method.GetUniqueNameString());
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
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: false, Accessibility: Accessibility.Protected or Accessibility.ProtectedOrInternal,
					   Getter: not null,
				   } &&
				   property.Getter.Accessibility != Accessibility.Private)
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: false,
					   Accessibility: not (Accessibility.Protected or Accessibility.ProtectedOrInternal),
					   Getter: not null,
				   } &&
				   property.Getter.Accessibility != Accessibility.Private);

		if (@class.AllProperties().All(property => property.IsIndexer))
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
			sb.Append("\t\tpublic IMockVerifyGotProtected<").Append(@class.ClassFullName).Append("> Protected")
				.AppendLine();
			sb.Append("\t\t\t=> (IMockVerifyGotProtected<").Append(@class.ClassFullName).Append(">)verifyGot;")
				.AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		sb.Append("\textension(IMockVerifyGot").Append(isProtected ? "Protected" : "").Append("<")
			.Append(@class.ClassFullName)
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ").Append(property.Name)
				.Append("()").AppendLine();
			sb.Append("\t\t\t=> CastToMockOrThrow(verifyGot).Property(").Append(property.GetUniqueNameString())
				.Append(");")
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
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: true, Accessibility: Accessibility.Protected or Accessibility.ProtectedOrInternal,
					   Getter: not null,
				   } &&
				   property.Getter.Accessibility != Accessibility.Private)
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: true,
					   Accessibility: not (Accessibility.Protected or Accessibility.ProtectedOrInternal),
					   Getter: not null,
				   } &&
				   property.Getter.Accessibility != Accessibility.Private);
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> Got")
				.Append(isProtected ? "Protected" : "").Append("Indexer")
				.Append("(").Append(string.Join(", ",
					indexerParameters.Value.Select((p, i)
						=> p.ToParameter() + (p.IsNullable() ? "?" : "") + $" parameter{i + 1}"))).Append(")")
				.AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\treturn CastToMockOrThrow(verify).GotIndexer(")
				.Append(string.Join(", ", indexerParameters.Value.Select((p, i)
					=> $"new NamedParameter(\"{p.Name}\", (IParameter)(parameter{i + 1}{
						(p.IsNullable() ? $" ?? It.IsNull<{p.Type.Fullname}>()" : "")}))"))).Append(");")
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
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: false, Accessibility: Accessibility.Protected or Accessibility.ProtectedOrInternal,
					   Setter: not null,
				   } &&
				   property.Setter.Accessibility != Accessibility.Private)
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: false,
					   Accessibility: not (Accessibility.Protected or Accessibility.ProtectedOrInternal),
					   Setter: not null,
				   } &&
				   property.Setter.Accessibility != Accessibility.Private);

		if (@class.AllProperties().All(property => property.IsIndexer))
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
			sb.Append("\t\tpublic IMockVerifySetProtected<").Append(@class.ClassFullName).Append("> Protected")
				.AppendLine();
			sb.Append("\t\t\t=> (IMockVerifySetProtected<").Append(@class.ClassFullName).Append(">)verifySet;")
				.AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		sb.Append("\textension(IMockVerifySet").Append(isProtected ? "Protected" : "").Append("<")
			.Append(@class.ClassFullName)
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ").Append(property.Name)
				.Append("(IParameter<")
				.Append(property.Type.Fullname).Append("> value)").AppendLine();
			sb.Append("\t\t\t=> CastToMockOrThrow(verifySet).Property(").Append(property.GetUniqueNameString())
				.Append(", (IParameter)value);").AppendLine();
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendSetIndexerExtensions(StringBuilder sb, Class @class,
		bool isProtected = false)
	{
		Func<Property, bool> predicate = isProtected
			? new Func<Property, bool>(property
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: true, Accessibility: Accessibility.Protected or Accessibility.ProtectedOrInternal,
					   Setter: not null,
				   } &&
				   property.Setter.Accessibility != Accessibility.Private)
			: new Func<Property, bool>(property
				=> property.ExplicitImplementation is null &&
				   property is
				   {
					   IsIndexer: true,
					   Accessibility: not (Accessibility.Protected or Accessibility.ProtectedOrInternal),
					   Setter: not null,
				   } &&
				   property.Setter.Accessibility != Accessibility.Private);
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
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> Set")
				.Append(isProtected ? "Protected" : "").Append("Indexer")
				.Append("(")
				.Append(string.Join(", ",
					indexer.IndexerParameters.Value.Select((p, i)
						=> p.ToParameter() + (p.IsNullable() ? "?" : "") + $" parameter{i + 1}")))
				.Append(", IParameter<")
				.Append(indexer.Type.Fullname).Append(">? value)").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\treturn CastToMockOrThrow(verify).SetIndexer((IParameter?)value, ")
				.Append(string.Join(", ", indexer.IndexerParameters.Value.Select((p, i)
					=> $"new NamedParameter(\"{p.Name}\", (IParameter)(parameter{i + 1}{
						(p.IsNullable() ? $" ?? It.IsNull<{p.Type.Fullname}>()" : "")}))")))
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
			sb.Append("\t\tpublic IMockVerifySubscribedTo<").Append(@class.ClassFullName).Append("> SubscribedTo")
				.AppendLine();
			sb.Append("\t\t\t=> (IMockVerifySubscribedTo<").Append(@class.ClassFullName).Append(">)verify;")
				.AppendLine();
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the event unsubscriptions for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifyUnsubscribedFrom<").Append(@class.ClassFullName)
				.Append("> UnsubscribedFrom").AppendLine();
			sb.Append("\t\t\t=> (IMockVerifyUnsubscribedFrom<").Append(@class.ClassFullName).Append(">)verify;")
				.AppendLine();
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

			sb.Append("\textension(IMockVerifySubscribedTo<").Append(@class.ClassFullName)
				.Append("> verifySubscribedTo)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected event subscriptions for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifySubscribedToProtected<").Append(@class.ClassFullName).Append("> Protected")
				.AppendLine();
			sb.Append("\t\t\t=> (IMockVerifySubscribedToProtected<").Append(@class.ClassFullName)
				.Append(">)verifySubscribedTo;").AppendLine();
			sb.AppendLine("\t}");
			sb.AppendLine();

			sb.Append("\textension(IMockVerifyUnsubscribedFrom<").Append(@class.ClassFullName)
				.Append("> verifyUnsubscribedFrom)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the protected event unsubscriptions for <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\"/> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerifyUnsubscribedFromProtected<").Append(@class.ClassFullName)
				.Append("> Protected").AppendLine();
			sb.Append("\t\t\t=> (IMockVerifyUnsubscribedFromProtected<").Append(@class.ClassFullName)
				.Append(">)verifyUnsubscribedFrom;").AppendLine();
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
			sb.Append("\t\t\t=> CastToMockOrThrow(verifyEvent).SubscribedTo(").Append(@event.GetUniqueNameString())
				.Append(");")
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
			sb.Append("\t\t\t=> CastToMockOrThrow(verifyEvent).UnsubscribedFrom(").Append(@event.GetUniqueNameString())
				.Append(");")
				.AppendLine();
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
