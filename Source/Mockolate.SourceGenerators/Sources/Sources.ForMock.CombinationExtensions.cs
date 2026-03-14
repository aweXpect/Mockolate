using System.Text;
using System.Text.RegularExpressions;
using Mockolate.SourceGenerators.Entities;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	private static readonly Regex InvalidIdentifierChars = new("[^a-zA-Z0-9_]", RegexOptions.Compiled);

	public static string ForMockCombinationExtensions(
		string name,
		MockClass mockClass,
		IEnumerable<Class> distinctAdditionalImplementations,
		HashSet<string> usedNames)
	{
		StringBuilder sb = InitializeBuilder();
		sb.Append("""
		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.Append("internal static class ExtensionsFor").Append(name).AppendLine();
		sb.AppendLine("{");
		sb.AppendLine("""
		              	private static global::Mockolate.Mock<T> CastToMockOrThrow<T>(global::Mockolate.IInteractiveMock<T> subject)
		              	{
		              		if (subject is global::Mockolate.Mock<T> mock)
		              		{
		              			return mock;
		              		}
		              	
		              		throw new global::Mockolate.Exceptions.MockException("The subject is no mock.");
		              	}
		              """);
		sb.AppendLine();
		sb.AppendLine("""
		              	private static global::Mockolate.Mock<T> GetMockOrThrow<T>(T subject)
		              	{
		              		if (subject is global::Mockolate.IMockSubject<T> mock)
		              		{
		              			return mock.Mock;
		              		}

		              		if (subject is global::Mockolate.IHasMockRegistration hasMockRegistration)
		              		{
		              			return new global::Mockolate.Mock<T>(subject, hasMockRegistration.Registrations);
		              		}
		              	
		              		throw new global::Mockolate.Exceptions.MockException("The subject is no mock subject.");
		              	}
		              """);
		sb.AppendLine();

		sb.Append("\textension(").Append(mockClass.DisplayString).AppendLine(" subject)");
		sb.AppendLine("\t{");

		foreach (Class @class in distinctAdditionalImplementations)
		{
			AppendAdditionalMockExtensions(sb, mockClass, @class, usedNames);
		}

		sb.AppendLine("\t}");

		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendAdditionalMockExtensions(StringBuilder sb, MockClass mockClass, Class @class,
		HashSet<string> usedNames)
	{
		sb.AppendLine();
		int nameSuffix = 1;
		string sanitizedClassName = InvalidIdentifierChars.Replace(
			mockClass.Namespace == @class.Namespace
				? @class.ClassName
				: $"{@class.Namespace}.{@class.ClassName}", "_");
		string name = sanitizedClassName;
		while (!usedNames.Add(name))
		{
			name = $"{sanitizedClassName}_{++nameSuffix}";
		}

		string mockExpression =
			$"new global::Mockolate.Mock<{@class.DisplayString}>(({@class.DisplayString})subject, GetMockOrThrow(subject).Registrations);";

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Sets up the mock for <see cref=\"").Append(@class.DisplayString.EscapeForXmlDoc())
			.Append("\" />")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic global::Mockolate.Setup.IMockSetup<").Append(@class.DisplayString).Append("> Setup_").Append(name)
			.Append("_Mock")
			.AppendLine();
		sb.Append("\t\t\t=> ").Append(mockExpression).AppendLine();

		if (@class.AllEvents().Any())
		{
			sb.AppendLine();
			sb.Append("\t\t/// <summary>").AppendLine();
			sb.Append("\t\t///     Raise events on the mock for <see cref=\"")
				.Append(@class.DisplayString.EscapeForXmlDoc())
				.Append("\" />").AppendLine();
			sb.Append("\t\t/// </summary>").AppendLine();
			sb.Append("\t\tpublic global::Mockolate.Raise.IMockRaises<").Append(@class.DisplayString).Append("> RaiseOn_")
				.Append(name).Append("_Mock").AppendLine();
			sb.Append("\t\t\t=> ").Append(mockExpression).AppendLine();
		}

		sb.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Verifies the interactions with the mocked subject of <see cref=\"")
			.Append(@class.DisplayString.EscapeForXmlDoc()).Append("\" /> on the mock.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic global::Mockolate.Verify.IMockVerify<").Append(@class.DisplayString).Append("> VerifyOn_").Append(name)
			.Append("_Mock")
			.AppendLine();
		sb.Append("\t\t\t=> ").Append(mockExpression).AppendLine();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
