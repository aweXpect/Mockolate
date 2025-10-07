using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Internals;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class SourceGeneration
{
	public static string GetExtensionClass(string name, Class @class)
	{
		string[] namespaces =
		[
			.. @class.GetClassNamespaces(),
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
		sb.AppendLine();
		sb.Append("public static class SetupExtensionsFor").Append(name).AppendLine();
		sb.AppendLine("{");

		AppendRaisesExtensions(sb, @class, namespaces);
		AppendSetupExtensions(sb, @class, namespaces);

		if (@class.Events.Any(@event
			    => @event.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)) ||
		    @class.Methods.Any(method
			    => method.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)) ||
		    @class.Properties.Any(property
			    => property.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal)))
		{
			AppendRaisesExtensions(sb, @class, namespaces, true);
			AppendSetupExtensions(sb, @class, namespaces, true);
		}

		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendRaisesExtensions(StringBuilder sb, Class @class, string[] namespaces,
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

		sb.Append("\textension(MockRaises<").Append(@class.ClassName).Append(">")
			.Append(isProtected ? ".Protected" : "").Append(" mock)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Event @event in @class.Events.Where(predicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raise the <see cref=\"").Append(@class.ClassName).Append(".").Append(@event.Name)
				.Append("\"/> event.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic void ").Append(@event.Name).Append("(").Append(string.Join(", ",
					@event.Delegate.Parameters.Select(p => p.Type.GetMinimizedString(namespaces) + " " + p.Name)))
				.Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\t((IMockRaises)mock).Raise(\"").Append(@class.GetFullName(@event.Name)).Append("\", ")
				.Append(string.Join(", ", @event.Delegate.Parameters.Select(p => p.Name))).Append(");").AppendLine();
			sb.AppendLine("\t\t}");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void AppendSetupExtensions(StringBuilder sb, Class @class, string[] namespaces,
		bool isProtected = false)
	{
		Func<Method, bool> methodPredicate = isProtected
			? new Func<Method, bool>(e
				=> e.ExplicitImplementation is null &&
				   e.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Method, bool>(e
				=> e.ExplicitImplementation is null &&
				   e.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		Func<Property, bool> propertyPredicate = isProtected
			? new Func<Property, bool>(e
				=> !e.IsIndexer && e.Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			: new Func<Property, bool>(e
				=> !e.IsIndexer &&
				   e.Accessibility is not (Accessibility.Protected or Accessibility.ProtectedOrInternal));
		if (!@class.Properties.Any(propertyPredicate) &&
		    !@class.Methods.Any(methodPredicate))
		{
			return;
		}

		sb.Append("\textension(MockSetups<").Append(@class.ClassName).Append(">")
			.Append(isProtected ? ".Protected" : "").Append(" mock)").AppendLine();
		sb.AppendLine("\t{");
		int count = 0;
		foreach (Property property in @class.Properties.Where(propertyPredicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Setup for the property <see cref=\"").Append(@class.ClassName).Append(".")
				.Append(property.Name).Append("\"/>.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic PropertySetup<").Append(property.Type.GetMinimizedString(namespaces)).Append("> ")
				.Append(property.IndexerParameter is not null
					? property.Name.Replace("[]",
						$"[With.Parameter<{property.IndexerParameter.Value.Type.GetMinimizedString(namespaces)}> {property.IndexerParameter.Value.Name}]")
					: property.Name).AppendLine();

			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\tget");
			sb.AppendLine("\t\t\t{");
			sb.Append("\t\t\t\tvar setup = new PropertySetup<").Append(property.Type.GetMinimizedString(namespaces))
				.Append(">();").AppendLine();
			sb.AppendLine("\t\t\t\tif (mock is IMockSetup mockSetup)");
			sb.AppendLine("\t\t\t\t{");
			sb.Append("\t\t\t\t\tmockSetup.RegisterProperty(\"").Append(@class.GetFullName(property.Name))
				.Append("\", setup);").AppendLine();
			sb.AppendLine("\t\t\t\t}");
			sb.AppendLine("\t\t\t\treturn setup;");
			sb.AppendLine("\t\t\t}");
			sb.AppendLine("\t\t}");
			sb.AppendLine();
		}

		foreach (Method method in @class.Methods.Where(methodPredicate))
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Setup for the method <see cref=\"").Append(@class.ClassName).Append(".")
				.Append(method.Name).Append("(")
				.Append(string.Join(", ",
					method.Parameters.Select(p => p.RefKind.GetString() + p.Type.GetMinimizedString(namespaces))))
				.Append(")\"/> with the given ")
				.Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>"))).Append(".")
				.AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			if (method.ReturnType != Type.Void)
			{
				sb.Append("\t\tpublic ReturnMethodSetup<")
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

				sb.Append(parameter.RefKind switch
					{
						RefKind.Ref => "With.RefParameter<",
						RefKind.Out => "With.OutParameter<",
						_ => "With.Parameter<",
					}).Append(parameter.Type.GetMinimizedString(namespaces))
					.Append("> ").Append(parameter.Name);
			}


			sb.Append(")").AppendLine();
			sb.AppendLine("\t\t{");

			if (method.ReturnType != Type.Void)
			{
				sb.Append("\t\t\tvar setup = new ReturnMethodSetup<")
					.Append(method.ReturnType.GetMinimizedString(namespaces));
				foreach (MethodParameter parameter in method.Parameters)
				{
					sb.Append(", ").Append(parameter.Type.GetMinimizedString(namespaces));
				}

				sb.Append(">");
			}
			else
			{
				sb.Append("\t\t\tvar setup = new VoidMethodSetup");

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
		sb.AppendLine();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
