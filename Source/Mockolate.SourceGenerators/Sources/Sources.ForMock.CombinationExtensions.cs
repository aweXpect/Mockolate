using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string ForMockCombinationExtensions(string name, MockClass mockClass)
	{
		StringBuilder sb = InitializeBuilder([
			"Mockolate.Exceptions",
			"Mockolate.Parameters",
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
					RefKind.Ref => "IRefParameter<",
					RefKind.Out => "IOutParameter<",
					_ => "IParameter<",
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
			sb.Append(", new NamedParameter(\"").Append(parameter.Name).Append("\", ").Append(parameter.Name);
			if (parameter.RefKind is not RefKind.Ref and not RefKind.Out)
			{
				sb.Append(" ?? It.IsNull<").Append(parameter.Type.Fullname)
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
					RefKind.Ref => "IVerifyRefParameter<",
					RefKind.Out => "IVerifyOutParameter<",
					_ => "IParameter<",
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
			.Append(mockClass.ClassFullName).Append(">>> invoked = (IMockInvoked<IMockVerify<")
			.Append(mockClass.ClassFullName).Append(", Mock<")
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
				sb.Append(" ?? It.IsNull<").Append(parameter.Type.Fullname)
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
			sb.Append("\t\tpublic IMockSetup<").Append(@class.ClassFullName).Append("> Setup").Append(name)
				.Append("Mock")
				.AppendLine();
			sb.Append("\t\t\t=> new Mock<").Append(@class.ClassFullName).Append(">((").Append(@class.ClassFullName)
				.Append(")subject, GetMockOrThrow(subject).Registrations);")
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
				sb.Append("\t\t\t=> new Mock<").Append(@class.ClassFullName).Append(">((").Append(@class.ClassFullName)
					.Append(")subject, GetMockOrThrow(subject).Registrations);")
					.AppendLine();
			}

			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Verifies the interactions with the mocked subject of <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\" /> on the mock.").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic IMockVerify<").Append(@class.ClassFullName).Append("> VerifyOn").Append(name)
				.Append("Mock")
				.AppendLine();
			sb.Append("\t\t\t=> new Mock<").Append(@class.ClassFullName).Append(">((").Append(@class.ClassFullName)
				.Append(")subject, GetMockOrThrow(subject).Registrations);")
				.AppendLine();
		}

		sb.AppendLine("\t}");
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
