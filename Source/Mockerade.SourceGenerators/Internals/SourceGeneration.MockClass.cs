using System.Text;
using Mockerade.SourceGenerators.Entities;
using Microsoft.CodeAnalysis;
using Type = Mockerade.SourceGenerators.Entities.Type;

namespace Mockerade.SourceGenerators.Internals;

#pragma warning disable S3779 // Cognitive Complexity of methods should not be too high
internal static partial class SourceGeneration
{
	public static string GetMockClass(MockClass mockClass)
	{
		string[] namespaces = mockClass.GetNamespaces();
		StringBuilder sb = new();
		foreach (string @namespace in namespaces)
		{
			sb.Append("using ").Append(@namespace).AppendLine(";");
		}

		sb.Append("""
		          using Mockerade.Events;
		          using Mockerade.Invocations;
		          using Mockerade.Setup;

		          namespace Mockerade;

		          #nullable enable

		          """);
		sb.Append("public static class For").Append(mockClass.ClassName).AppendLine();
		sb.AppendLine("{");

		AppendMockObject(sb, mockClass, namespaces);
		sb.AppendLine();
		
		AppendMock(sb, mockClass);
		sb.AppendLine();

		AppendRaisesExtensions(sb, mockClass, namespaces);
		sb.AppendLine();

		AppendSetupExtensions(sb, mockClass, namespaces);
		sb.AppendLine();

		AppendInvocationExtensions(sb, mockClass, namespaces);
		sb.AppendLine();

		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendMockObject(StringBuilder sb, MockClass mockClass, string[] namespaces)
	{
		sb.Append("\tpublic partial class MockObject(IMock mock)")
			.Append(" : ").AppendLine(mockClass.ClassName);
		sb.AppendLine("\t{");
		int count = 0;

		foreach (Event @event in mockClass.Events)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(mockClass.ClassName).Append('.').Append(@event.Name).AppendLine("\" />");
			sb.Append("\t\t").Append(@event.Accessibility.ToVisibilityString()).Append(' ');
			if (!mockClass.IsInterface && @event.IsVirtual)
			{
				sb.Append("override ");
			}
			sb.Append("event ").Append(@event.Type.GetMinimizedString(namespaces))
				.Append("? ").Append(@event.Name).AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tadd => mock.Raises.AddEvent(nameof(").Append(@event.Name).Append("), value?.Target, value?.Method);").AppendLine();
			sb.Append("\t\t\tremove => mock.Raises.RemoveEvent(nameof(").Append(@event.Name).Append("), value?.Target, value?.Method);").AppendLine();
			sb.AppendLine("\t\t}");
		}

		foreach (Property property in mockClass.Properties)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(mockClass.ClassName).Append('.').Append(property.Name).AppendLine("\" />");
			sb.Append("\t\t").Append(property.Accessibility.ToVisibilityString()).Append(' ');
			if (!mockClass.IsInterface && property.IsVirtual)
			{
				sb.Append("override ");
			}
			sb.Append(property.Type.GetMinimizedString(namespaces))
				.Append(" ").Append(property.Name).AppendLine();
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
					.Append(">(nameof(").Append(property.Name).AppendLine("));");
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
				sb.Append("\t\t\t\tmock.Set(nameof(").Append(property.Name).AppendLine("), value);");
				sb.AppendLine("\t\t\t}");
			}

			sb.AppendLine("\t\t}");
		}

		foreach (Method method in mockClass.Methods)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(mockClass.ClassName).Append('.').Append(method.Name)
				.Append('(').Append(string.Join(", ",
					method.Parameters.Select(p => p.RefKind.GetString() + p.Type.GetMinimizedString(namespaces))))
				.AppendLine(")\" />");
			sb.Append("\t\t");
			sb.Append(method.Accessibility.ToVisibilityString()).Append(' ');
			if (!mockClass.IsInterface && method.IsVirtual)
			{
				sb.Append("override ");
			}
			sb.Append(method.ReturnType.GetMinimizedString(namespaces)).Append(' ')
				.Append(method.Name).Append('(');
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
					.Append(">(nameof(").Append(method.Name).Append(")");
				foreach (MethodParameter p in method.Parameters)
				{
					sb.Append(", ").Append(p.RefKind == RefKind.Out ? "null" : p.Name);
				}

				sb.AppendLine(");");
			}
			else
			{
				sb.Append("\t\t\tvar result = mock.Execute(nameof(").Append(method.Name).Append(")");
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

		sb.AppendLine("\t}");
	}

	private static void AppendMock(StringBuilder sb, MockClass mockClass)
	{
		sb.Append("\tpublic class Mock : Mock<").Append(mockClass.ClassName).AppendLine(">");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\tpublic Mock(MockBehavior mockBehavior) : base(mockBehavior)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\tObject = new MockObject(this);");
		sb.AppendLine("\t\t}");
		sb.AppendLine();
		sb.Append("\t\t/// <inheritdoc cref=\"Mock{").Append(mockClass.ClassName).AppendLine("}.Object\" />");
		sb.Append("\t\tpublic override ").Append(mockClass.ClassName).AppendLine(" Object { get; }");
		sb.AppendLine("\t}");
	}

	private static void AppendRaisesExtensions(StringBuilder sb, MockClass mockClass, string[] namespaces)
	{
		sb.Append("\textension(MockRaises<").Append(mockClass.ClassName).AppendLine("> mock)");
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Event @event in mockClass.Events)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raises the <see cref=\"").Append(mockClass.ClassName).Append(".").Append(@event.Name).Append("\"/> event.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic void ").Append(@event.Name).Append("(").Append(string.Join(", ", @event.Delegate.Parameters.Select(p => p.Type.GetMinimizedString(namespaces) + " " + p.Name))).Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\t((IMockRaises)mock).Raises(\"").Append(@event.Name).Append("\", ").Append(string.Join(", ", @event.Delegate.Parameters.Select(p => p.Name))).Append(");").AppendLine();
			sb.AppendLine("\t\t}");
		}
		sb.AppendLine("\t}");
	}

	private static void AppendSetupExtensions(StringBuilder sb, MockClass mockClass, string[] namespaces)
	{
		sb.Append("\textension(MockSetup<").Append(mockClass.ClassName).AppendLine("> mock)");
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Property property in mockClass.Properties)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Setup for the property <see cref=\"").Append(mockClass.ClassName).Append(".").Append(property.Name).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic PropertySetup<").Append(property.Type.GetMinimizedString(namespaces)).Append("> ")
				.Append(property.Name).AppendLine();

			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\tget");
			sb.AppendLine("\t\t\t{");
			sb.Append("\t\t\t\tvar setup = new PropertySetup<").Append(property.Type.GetMinimizedString(namespaces)).Append(">();").AppendLine();
			sb.AppendLine("\t\t\t\tif (mock is IMockSetup mockSetup)");
			sb.AppendLine("\t\t\t\t{");
			sb.Append("\t\t\t\t\tmockSetup.RegisterProperty(\"").Append(property.Name).Append("\", setup);").AppendLine();
			sb.AppendLine("\t\t\t\t}");
			sb.AppendLine("\t\t\t\treturn setup;");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t}");
		}

		foreach (Method method in mockClass.Methods)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Setup for the method <see cref=\"").Append(mockClass.ClassName).Append(".").Append(method.Name).Append("(").Append(string.Join(", ", method.Parameters.Select(p => p.RefKind.GetString() + p.Type.GetMinimizedString(namespaces)))).Append(")\"/> with the given ").Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>"))).Append(".").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			if (method.ReturnType != Type.Void)
			{
				sb.Append("\t\tpublic MethodWithReturnValueSetup<")
					.Append(method.ReturnType.GetMinimizedString(namespaces));
				foreach (MethodParameter parameter in method.Parameters)
				{
					sb.Append(", ").Append(parameter.Type.GetMinimizedString(namespaces));
				}

				sb.Append("> ");
				sb.Append(method.Name).Append("(");
			}
			else
			{
				sb.Append("\t\tpublic MethodWithoutReturnValueSetup");
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

						sb.Append(parameter.Type.GetMinimizedString(namespaces));
					}

					sb.Append('>');
				}

				sb.Append(' ').Append(method.Name).Append("(");
			}
			int i = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (i++ > 0)
				{
					sb.Append(", ");
				}
				sb.Append(parameter.RefKind switch {
					RefKind.Ref => "With.RefParameter<",
					RefKind.Out => "With.OutParameter<",
					_ => "With.Parameter<"
				}).Append(parameter.Type.GetMinimizedString(namespaces))
					.Append("> ").Append(parameter.Name);
			}


			sb.Append(")").AppendLine();
			sb.AppendLine("\t\t{");

			if (method.ReturnType != Type.Void)
			{
				sb.Append("\t\t\tvar setup = new MethodWithReturnValueSetup<")
					.Append(method.ReturnType.GetMinimizedString(namespaces));
				foreach (MethodParameter parameter in method.Parameters)
				{
					sb.Append(", ").Append(parameter.Type.GetMinimizedString(namespaces));
				}

				sb.Append(">");
			}
			else
			{
				sb.Append("\t\t\tvar setup = new MethodWithoutReturnValueSetup");

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

						sb.Append(parameter.Type.GetMinimizedString(namespaces));
					}

					sb.Append('>');
				}

			}

			sb.Append("(nameof(").Append(mockClass.ClassName).Append(".").Append(method.Name).Append(")");
			foreach (string name in method.Parameters.Select(p => p.Name))
			{
				sb.Append(", new With.NamedParameter(\"").Append(name).Append("\", ").Append(name).Append(")");
			}
			sb.Append(");").AppendLine();
			sb.AppendLine("\t\t\tif (mock is IMockSetup mockSetup)");
			sb.AppendLine("\t\t\t{");
			sb.AppendLine("\t\t\t\tmockSetup.RegisterMethod(setup);");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t\treturn setup;");
			sb.AppendLine("\t\t}");
		}
		sb.AppendLine("\t}");
	}

	private static void AppendInvocationExtensions(StringBuilder sb, MockClass mockClass, string[] namespaces)
	{
		sb.Append("\textension(MockInvocations<").Append(mockClass.ClassName).AppendLine("> mock)");
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Property property in mockClass.Properties)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the property <see cref=\"").Append(mockClass.ClassName).Append(".").Append(property.Name).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic InvocationResult.Property<").Append(property.Type.GetMinimizedString(namespaces)).Append("> ").Append(property.Name).AppendLine();

			sb.Append("\t\t\t=> new InvocationResult.Property<").Append(property.Type.GetMinimizedString(namespaces)).Append(">(mock, \"").Append(property.Name).Append("\");");
		}

		foreach (Method method in mockClass.Methods)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the method <see cref=\"").Append(mockClass.ClassName).Append(".").Append(method.Name).Append("(").Append(string.Join(", ", method.Parameters.Select(p => p.RefKind.GetString() + p.Type.GetMinimizedString(namespaces)))).Append(")\"/> with the given ").Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>"))).Append(".").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic InvocationResult ").Append(method.Name).Append("(");
			int i = 0;
			foreach (MethodParameter parameter in method.Parameters)
			{
				if (i++ > 0)
				{
					sb.Append(", ");
				}
				sb.Append(parameter.RefKind switch
				{
					RefKind.Ref => "With.InvocationRefParameter<",
					RefKind.Out => "With.InvocationOutParameter<",
					_ => "With.Parameter<"
				}).Append(parameter.Type.GetMinimizedString(namespaces))
					.Append("> ").Append(parameter.Name);
			}

			sb.Append(")").AppendLine();
			sb.Append("\t\t\t=> new InvocationResult(((IMockInvocations)mock).Method(\"").Append(method.Name).Append("\"");

			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ");
				sb.Append(parameter.Name);
			}
			sb.AppendLine("));");
		}
		sb.AppendLine("\t}");
	}
}
#pragma warning restore S3779 // Cognitive Complexity of methods should not be too high
