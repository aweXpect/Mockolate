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

		sb.Append("\textension(").Append(mockClass.ClassFullName).AppendLine(" subject)");
		sb.AppendLine("\t{");

		foreach (Class @class in distinctAdditionalImplementations)
		{
			AppendAdditionalMockExtensions(sb, @class, usedNames);
		}

		sb.AppendLine("\t}");

		sb.AppendLine("}");
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendAdditionalMockExtensions(StringBuilder sb, Class @class, HashSet<string> usedNames)
	{
		sb.AppendLine();
		int nameSuffix = 1;
		string sanitizedClassName = InvalidIdentifierChars.Replace(@class.ClassName, "_");
		string name = sanitizedClassName;
		while (!usedNames.Add(name))
		{
			name = $"{sanitizedClassName}__{++nameSuffix}";
		}

		string mockExpression =
			$"new Mock<{@class.ClassFullName}>(({@class.ClassFullName})subject, GetMockOrThrow(subject).Registrations);";

		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Sets up the mock for <see cref=\"").Append(@class.ClassFullName.EscapeForXmlDoc())
			.Append("\" />")
			.AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IMockSetup<").Append(@class.ClassFullName).Append("> Setup").Append(name)
			.Append("Mock")
			.AppendLine();
		sb.Append("\t\t\t=> ").Append(mockExpression).AppendLine();

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
			sb.Append("\t\t\t=> ").Append(mockExpression).AppendLine();
		}

		sb.AppendLine();
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Verifies the interactions with the mocked subject of <see cref=\"")
			.Append(@class.ClassFullName.EscapeForXmlDoc()).Append("\" /> on the mock.").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tpublic IMockVerify<").Append(@class.ClassFullName).Append("> VerifyOn").Append(name)
			.Append("Mock")
			.AppendLine();
		sb.Append("\t\t\t=> ").Append(mockExpression).AppendLine();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
