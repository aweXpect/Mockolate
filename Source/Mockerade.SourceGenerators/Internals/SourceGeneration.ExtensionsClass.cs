using System.Text;
using Microsoft.CodeAnalysis;
using Mockerade.SourceGenerators.Entities;
using Type = Mockerade.SourceGenerators.Entities.Type;

namespace Mockerade.SourceGenerators.Internals;

#pragma warning disable S3779 // Cognitive Complexity of methods should not be too high
internal static partial class SourceGeneration
{
	public static string GetExtensionClass(Class mockClass)
	{
		string[] namespaces = mockClass.GetClassNamespaces();
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
		sb.Append("public static class ExtensionsFor").Append(mockClass.ClassName).AppendLine();
		sb.AppendLine("{");
		AppendRaisesExtensions(sb, mockClass, namespaces);
		sb.AppendLine();

		AppendSetupExtensions(sb, mockClass, namespaces);
		sb.AppendLine();

		AppendInvokedExtensions(sb, mockClass, namespaces);
		sb.AppendLine();

		AppendAccessedExtensions(sb, mockClass, namespaces);
		sb.AppendLine();

		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}
	
	private static void AppendRaisesExtensions(StringBuilder sb, Class @class, string[] namespaces)
	{
		sb.Append("\textension(MockRaises<").Append(@class.ClassName).AppendLine("> mock)");
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Event @event in @class.Events)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raise the <see cref=\"").Append(@class.ClassName).Append(".").Append(@event.Name).Append("\"/> event.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic void ").Append(@event.Name).Append("(").Append(string.Join(", ", @event.Delegate.Parameters.Select(p => p.Type.GetMinimizedString(namespaces) + " " + p.Name))).Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\t((IMockRaises)mock).Raise(\"").Append(@class.GetFullName(@event.Name)).Append("\", ").Append(string.Join(", ", @event.Delegate.Parameters.Select(p => p.Name))).Append(");").AppendLine();
			sb.AppendLine("\t\t}");
		}
		sb.AppendLine("\t}");
	}

	private static void AppendSetupExtensions(StringBuilder sb, Class @class, string[] namespaces)
	{
		sb.Append("\textension(MockSetups<").Append(@class.ClassName).AppendLine("> mock)");
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Property property in @class.Properties)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Setup for the property <see cref=\"").Append(@class.ClassName).Append(".").Append(property.Name).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic PropertySetup<").Append(property.Type.GetMinimizedString(namespaces)).Append("> ")
				.Append(property.Name).AppendLine();

			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\tget");
			sb.AppendLine("\t\t\t{");
			sb.Append("\t\t\t\tvar setup = new PropertySetup<").Append(property.Type.GetMinimizedString(namespaces)).Append(">();").AppendLine();
			sb.AppendLine("\t\t\t\tif (mock is IMockSetup mockSetup)");
			sb.AppendLine("\t\t\t\t{");
			sb.Append("\t\t\t\t\tmockSetup.RegisterProperty(\"").Append(@class.GetFullName(property.Name)).Append("\", setup);").AppendLine();
			sb.AppendLine("\t\t\t\t}");
			sb.AppendLine("\t\t\t\treturn setup;");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t}");
		}

		foreach (Method method in @class.Methods)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Setup for the method <see cref=\"").Append(@class.ClassName).Append(".").Append(method.Name).Append("(").Append(string.Join(", ", method.Parameters.Select(p => p.RefKind.GetString() + p.Type.GetMinimizedString(namespaces)))).Append(")\"/> with the given ").Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>"))).Append(".").AppendLine();
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

			sb.Append("(\"").Append(@class.GetFullName(method.Name)).Append("\"");
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

	private static void AppendInvokedExtensions(StringBuilder sb, Class @class, string[] namespaces)
	{
		sb.Append("\textension(MockInvoked<").Append(@class.ClassName).AppendLine("> mock)");
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Method method in @class.Methods)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the method <see cref=\"").Append(@class.ClassName).Append(".").Append(method.Name).Append("(").Append(string.Join(", ", method.Parameters.Select(p => p.RefKind.GetString() + p.Type.GetMinimizedString(namespaces)))).Append(")\"/> with the given ").Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>"))).Append(".").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic CheckResult ").Append(method.Name).Append("(");
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
			sb.Append("\t\t\t=> new CheckResult(((IMockInvoked)mock).Method(\"").Append(@class.GetFullName(method.Name)).Append("\"");

			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ");
				sb.Append(parameter.Name);
			}
			sb.AppendLine("));");
		}
		sb.AppendLine("\t}");
	}

	private static void AppendAccessedExtensions(StringBuilder sb, Class @class, string[] namespaces)
	{
		sb.Append("\textension(MockAccessed<").Append(@class.ClassName).AppendLine("> mock)");
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Property property in @class.Properties)
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Validates the invocations for the property <see cref=\"").Append(@class.ClassName).Append(".").Append(property.Name).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic CheckResult.Property<").Append(property.Type.GetMinimizedString(namespaces)).Append("> ").Append(property.Name).AppendLine();

			sb.Append("\t\t\t=> new CheckResult.Property<").Append(property.Type.GetMinimizedString(namespaces)).Append(">(mock, \"").Append(@class.GetFullName(property.Name)).Append("\");");
		}
		sb.AppendLine("\t}");
	}
}
#pragma warning restore S3779 // Cognitive Complexity of methods should not be too high
