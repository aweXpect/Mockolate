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
			AppendMethodSetup(@class, sb, @delegate, false);
			sb.AppendLine();

			if (@delegate.Parameters.Count > 0)
			{
				AppendMethodSetup(@class, sb, @delegate, true);
			}

			sb.AppendLine("\t}");
			sb.AppendLine();
			sb.Append("\textension(IMockVerify<").Append(@class.ClassFullName).Append("> verify)").AppendLine();
			sb.AppendLine("\t{");
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the method <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc())
				.Append(".").Append(@delegate.Name.EscapeForXmlDoc()).Append("(")
				.Append(string.Join(", ",
					@delegate.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname)))
				.Append(")\"/>").Append(@delegate.Parameters.Count > 0 ? " with the given " : "")
				.Append(string.Join(", ", @delegate.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>")))
				.Append(".")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ").Append(@delegate.Name)
				.Append("(");
			int i = 0;
			foreach (MethodParameter parameter in @delegate.Parameters)
			{
				if (i++ > 0)
				{
					sb.Append(", ");
				}

				sb.Append((parameter.RefKind, parameter.IsSpan, parameter.IsReadOnlySpan) switch
					{
						(RefKind.Ref, _, _) => "Match.IVerifyRefParameter<",
						(RefKind.Out, _, _) => "Match.IVerifyOutParameter<",
						(_, true, false) => "Match.IVerifySpanParameter<",
						(_, false, true) => "Match.IVerifyReadOnlySpanParameter<",
						_ => "Match.IParameter<",
					}).Append(parameter.IsSpan || parameter.IsReadOnlySpan
						? parameter.SpanType!.Fullname
						: parameter.Type.Fullname)
					.Append('>');
				if (IsNullable(parameter))
				{
					sb.Append('?');
				}

				sb.Append(' ').Append(parameter.Name);
			}

			sb.Append(")");
			if (@delegate.GenericParameters is not null && @delegate.GenericParameters.Value.Count > 0)
			{
				foreach (GenericParameter gp in @delegate.GenericParameters.Value)
				{
					gp.AppendWhereConstraint(sb, "\t\t\t");
				}
			}

			sb.AppendLine();

			sb.Append("\t\t\t=> CastToMockOrThrow(verify).Method(").Append(@delegate.GetUniqueNameString());

			foreach (MethodParameter parameter in @delegate.Parameters)
			{
				sb.Append(", ");
				sb.Append("new Match.NamedParameter(\"").Append(parameter.Name).Append("\", (Match.IParameter)(");
				sb.Append(parameter.Name);
				if (IsNullable(parameter))
				{
					sb.Append(" ?? Match.Null<").Append(parameter.Type.Fullname)
						.Append(">()");
				}

				sb.Append("))");
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
				sb.Append("\t\tpublic VerificationResult<").Append(@class.ClassFullName).Append("> ")
					.Append(@delegate.Name).Append("(Match.IParameters parameters)");
				if (@delegate.GenericParameters is not null && @delegate.GenericParameters.Value.Count > 0)
				{
					foreach (GenericParameter gp in @delegate.GenericParameters.Value)
					{
						gp.AppendWhereConstraint(sb, "\t\t\t");
					}
				}

				sb.AppendLine();

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
			              	private static Mock<T> GetMockOrThrow<T>(T subject) where T : Delegate
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

	private static void AppendMethodSetup(Class @class, StringBuilder sb, Method method, bool useParameters)
	{
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Setup for the method <see cref=\"")
			.Append(@class.ClassFullName.EscapeForXmlDoc()).Append(".")
			.Append(method.Name.EscapeForXmlDoc()).Append("(")
			.Append(string.Join(", ",
				method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname)))
			.Append(")\"/>");
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
			sb.Append("\t\tpublic IReturnMethodSetup<")
				.Append(method.ReturnType.Fullname);
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").Append((parameter.IsSpan, parameter.IsReadOnlySpan) switch
				{
					(true, false) => $"SpanWrapper<{parameter.SpanType!.Fullname}>",
					(false, true) => $"ReadOnlySpanWrapper<{parameter.SpanType!.Fullname}>",
					_ => parameter.Type.Fullname,
				});
			}

			sb.Append("> ");
			sb.Append(method.Name).Append("(");
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

					sb.Append((parameter.IsSpan, parameter.IsReadOnlySpan) switch
					{
						(true, false) => $"SpanWrapper<{parameter.SpanType!.Fullname}>",
						(false, true) => $"ReadOnlySpanWrapper<{parameter.SpanType!.Fullname}>",
						_ => parameter.Type.Fullname,
					});
				}

				sb.Append('>');
			}

			sb.Append(' ').Append(method.Name).Append("(");
		}

		if (useParameters)
		{
			sb.Append("Match.IParameters parameters)");
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

				sb.Append((parameter.RefKind, parameter.IsSpan, parameter.IsReadOnlySpan) switch
					{
						(RefKind.Ref, _, _) => "Match.IRefParameter<",
						(RefKind.Out, _, _) => "Match.IOutParameter<",
						(_, true, false) => "Match.ISpanParameter<",
						(_, false, true) => "Match.IReadOnlySpanParameter<",
						_ => "Match.IParameter<",
					}).Append(parameter.IsSpan || parameter.IsReadOnlySpan
						? parameter.SpanType!.Fullname
						: parameter.Type.Fullname)
					.Append('>');
				if (IsNullable(parameter))
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

		if (method.ReturnType != Type.Void)
		{
			sb.Append("\t\t\tvar methodSetup = new ReturnMethodSetup<")
				.Append(method.ReturnType.Fullname);
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").Append((parameter.IsSpan, parameter.IsReadOnlySpan) switch
				{
					(true, false) => $"SpanWrapper<{parameter.SpanType!.Fullname}>",
					(false, true) => $"ReadOnlySpanWrapper<{parameter.SpanType!.Fullname}>",
					(_, _) => parameter.Type.Fullname,
				});
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

					sb.Append((parameter.IsSpan, parameter.IsReadOnlySpan) switch
					{
						(true, false) => $"SpanWrapper<{parameter.SpanType!.Fullname}>",
						(false, true) => $"ReadOnlySpanWrapper<{parameter.SpanType!.Fullname}>",
						(_, _) => parameter.Type.Fullname,
					});
				}

				sb.Append('>');
			}
		}

		if (useParameters)
		{
			sb.Append("(").Append(method.GetUniqueNameString()).Append(", parameters);").AppendLine();
			sb.AppendLine("\t\t\tCastToMockRegistrationOrThrow(setup).SetupMethod(methodSetup);");
			sb.AppendLine("\t\t\treturn methodSetup;");
			sb.AppendLine("\t\t}");
		}
		else
		{
			sb.Append("(").Append(method.GetUniqueNameString());
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", new Match.NamedParameter(\"").Append(parameter.Name).Append("\", (Match.IParameter)(")
					.Append(parameter.Name);
				if (IsNullable(parameter))
				{
					sb.Append(" ?? Match.Null<").Append(parameter.Type.Fullname)
						.Append(">()");
				}

				sb.Append("))");
			}

			sb.Append(");").AppendLine();
			sb.AppendLine("\t\t\tCastToMockRegistrationOrThrow(setup).SetupMethod(methodSetup);");
			sb.AppendLine("\t\t\treturn methodSetup;");
			sb.AppendLine("\t\t}");
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
			sb.Append("\t\tpublic void ").Append(@event.Name).Append("(").Append(string.Join(", ",
					@event.Delegate.Parameters.Select(p => p.Type.Fullname + " " + p.Name)))
				.Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tCastToMockRegistrationOrThrow(mockRaises).Raise(").Append(@event.GetUniqueNameString())
				.Append(", ")
				.Append(string.Join(", ", @event.Delegate.Parameters.Select(p => p.Name))).Append(");").AppendLine();
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
			sb.Append("\t\tpublic void ").Append(@event.Name).Append("(Match.IDefaultEventParameters parameters)")
				.AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tMock<").Append(@class.ClassFullName).Append("> mock = CastToMockOrThrow(mockRaises);")
				.AppendLine();
			sb.Append("\t\t\tMockBehavior mockBehavior = mock.Registrations.Behavior;").AppendLine();
			sb.Append("\t\t\tmock.Registrations.Raise(").Append(@event.GetUniqueNameString()).Append(", ")
				.Append(string.Join(", ",
					@event.Delegate.Parameters.Select(p => $"mockBehavior.DefaultValue.Generate<{p.Type.Fullname}>()")))
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
					.Append(property.IndexerParameters is not null
						? property.Name.Replace("[]",
							$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"Match.IParameter<{p.Type.Fullname}> {p.Name}"))}]")
						: property.Name).AppendLine();

				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t\tget");
				sb.AppendLine("\t\t\t{");
				sb.Append("\t\t\t\tvar propertySetup = new PropertySetup<").Append(property.Type.Fullname)
					.Append(">();").AppendLine();
				sb.AppendLine("\t\t\t\tCastToMockRegistrationOrThrow(setup).SetupProperty(")
					.Append(property.GetUniqueNameString())
					.Append(", propertySetup);").AppendLine();
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
				sb.Append("\t\tpublic IIndexerSetup<").Append(indexer.Type.Fullname);
				foreach (MethodParameter parameter in indexer.IndexerParameters!)
				{
					sb.Append(", ").Append((parameter.IsSpan, parameter.IsReadOnlySpan) switch
					{
						(true, false) => $"SpanWrapper<{parameter.SpanType!.Fullname}>",
						(false, true) => $"ReadOnlySpanWrapper<{parameter.SpanType!.Fullname}>",
						_ => parameter.Type.Fullname,
					});
				}

				sb.Append("> Indexer").Append("(").Append(string.Join(", ",
					indexer.IndexerParameters.Value.Select((p, i)
						=> (p.IsSpan, p.IsReadOnlySpan) switch
						{
							(true, false) => $"Match.ISpanParameter<{p.SpanType!.Fullname}>",
							(false, true) => $"Match.IReadOnlySpanParameter<{p.SpanType!.Fullname}>",
							(_, _) => $"Match.IParameter<{p.Type.Fullname}>",
						} + (IsNullable(p) ? "?" : "") + $" parameter{i + 1}"))).Append(")").AppendLine();
				sb.Append("\t\t{").AppendLine();
				sb.Append("\t\t\tvar indexerSetup = new IndexerSetup<").Append(indexer.Type.Fullname);
				foreach (MethodParameter parameter in indexer.IndexerParameters!)
				{
					sb.Append(", ").Append((parameter.IsSpan, parameter.IsReadOnlySpan) switch
					{
						(true, false) => $"SpanWrapper<{parameter.SpanType!.Fullname}>",
						(false, true) => $"ReadOnlySpanWrapper<{parameter.SpanType!.Fullname}>",
						(_, _) => parameter.Type.Fullname,
					});
				}

				sb.Append(">(").Append(string.Join(", ",
						indexer.IndexerParameters.Value.Select((p, i) => $"(Match.IParameter)(parameter{i + 1}{
							(IsNullable(p) ? $" ?? Match.Null<{p.Type.Fullname}>()" : "")})")))
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

				sb.Append((parameter.RefKind, parameter.IsSpan, parameter.IsReadOnlySpan) switch
					{
						(RefKind.Ref, _, _) => "Match.IVerifyRefParameter<",
						(RefKind.Out, _, _) => "Match.IVerifyOutParameter<",
						(_, true, false) => "Match.IVerifySpanParameter<",
						(_, false, true) => "Match.IVerifyReadOnlySpanParameter<",
						_ => "Match.IParameter<",
					}).Append(parameter.IsSpan || parameter.IsReadOnlySpan
						? parameter.SpanType!.Fullname
						: parameter.Type.Fullname)
					.Append('>');
				if (IsNullable(parameter))
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
				sb.Append("new Match.NamedParameter(\"").Append(parameter.Name).Append("\", (Match.IParameter)(");
				sb.Append(parameter.Name);
				if (IsNullable(parameter))
				{
					sb.Append(" ?? Match.Null<").Append(parameter.Type.Fullname)
						.Append(">()");
				}

				sb.Append("))");
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
				.Append("(Match.IParameters parameters)");
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
						=> (p.IsSpan, p.IsReadOnlySpan) switch
						{
							(true, false) => $"Match.ISpanParameter<{p.SpanType!.Fullname}>",
							(false, true) => $"Match.IReadOnlySpanParameter<{p.SpanType!.Fullname}>",
							(_, _) => $"Match.IParameter<{p.Type.Fullname}>",
						} + (IsNullable(p) ? "?" : "") + $" parameter{i + 1}"))).Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\treturn CastToMockOrThrow(verify).GotIndexer(")
				.Append(string.Join(", ", indexerParameters.Value.Select((p, i)
					=> $"new Match.NamedParameter(\"{p.Name}\", (Match.IParameter)(parameter{i + 1}{
						(IsNullable(p) ? $" ?? Match.Null<{p.Type.Fullname}>()" : "")}))"))).Append(");")
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
				.Append("(Match.IParameter<")
				.Append(property.Type.Fullname).Append("> value)").AppendLine();
			sb.Append("\t\t\t=> CastToMockOrThrow(verifySet).Property(").Append(property.GetUniqueNameString())
				.Append(", (Match.IParameter)value);").AppendLine();
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
						=> (p.IsSpan, p.IsReadOnlySpan) switch
						{
							(true, false) => $"Match.ISpanParameter<{p.SpanType!.Fullname}>",
							(false, true) => $"Match.IReadOnlySpanParameter<{p.SpanType!.Fullname}>",
							(_, _) => $"Match.IParameter<{p.Type.Fullname}>",
						} + (IsNullable(p) ? "?" : "") + $" parameter{i + 1}"))).Append(", Match.IParameter<")
				.Append(indexer.Type.Fullname).Append(">? value)").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\treturn CastToMockOrThrow(verify).SetIndexer((Match.IParameter?)value, ")
				.Append(string.Join(", ", indexer.IndexerParameters.Value.Select((p, i)
					=> $"new Match.NamedParameter(\"{p.Name}\", (Match.IParameter)(parameter{i + 1}{
						(IsNullable(p) ? $" ?? Match.Null<{p.Type.Fullname}>()" : "")}))")))
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

	private static bool IsNullable(MethodParameter parameter)
		=> parameter.RefKind is not RefKind.Ref and not RefKind.Out &&
		   parameter is { IsSpan: false, IsReadOnlySpan: false, };
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
