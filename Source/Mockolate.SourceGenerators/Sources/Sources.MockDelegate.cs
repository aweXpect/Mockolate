using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string MockDelegate(string name, MockClass @class, Method delegateMethod)
	{
		string mockRegistryName = @class.GetUniqueName("MockRegistry", "MockolateMockRegistry");
		string escapedClassName = @class.ClassFullName.EscapeForXmlDoc();
		StringBuilder sb = InitializeBuilder();
		sb.Append("#nullable enable annotations").AppendLine();
		sb.Append("namespace Mockolate;").AppendLine();
		sb.AppendLine();

		#region MockForXXXExtensions

		sb.AppendXmlSummary($"Mock extensions for <see cref=\"{escapedClassName}\" />.", "");
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
		sb.Append("internal static partial class MockExtensionsFor").Append(name).AppendLine();
		sb.Append("{").AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MockExtensionsFor").Append(name).Append("\" />").AppendLine();
		sb.Append("\textension(").Append(@class.ClassFullName).Append(" mock)").AppendLine();
		sb.Append("\t{").AppendLine();

		#region Mock Property

		sb.AppendXmlSummary($"Get access to the mock of <see cref=\"{escapedClassName}\" />.");
		sb.Append("\t\tpublic global::Mockolate.Mock.IMockFor").Append(name).Append(" Mock").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tget").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tif (mock.Target is global::Mockolate.Mock.IMockFor").Append(name).Append(" mockInterface)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\treturn mockInterface;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"The subject is no mock.\");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		#endregion Mock Property

		#region CreateMock

		sb.AppendXmlSummary($"Create a new mock of <see cref=\"{escapedClassName}\" /> with the default <see cref=\"global::Mockolate.MockBehavior\" />.");
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock()").AppendLine();
		sb.Append("\t\t\t=> CreateMock(null, []);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Create a new mock of <see cref=\"{escapedClassName}\" /> with the default <see cref=\"global::Mockolate.MockBehavior\" />.");
		sb.AppendXmlRemarks("All provided <paramref name=\"setups\" /> are immediately applied to the mock.");
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock(params global::System.Action<global::Mockolate.Mock.IMockSetupFor").Append(name).Append(">[] setups)").AppendLine();
		sb.Append("\t\t\t=> CreateMock(null, setups);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Create a new mock of <see cref=\"{escapedClassName}\" /> with the given <paramref name=\"mockBehavior\" />.");
		sb.AppendXmlRemarks("All provided <paramref name=\"setups\" /> are immediately applied to the mock.");
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName)
			.Append(
				" CreateMock(global::Mockolate.MockBehavior? mockBehavior = null, params global::System.Action<global::Mockolate.Mock.IMockSetupFor")
			.Append(name).Append(">[] setups)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tmockBehavior ??= global::Mockolate.MockBehavior.Default;").AppendLine();
		sb.Append("\t\t\tvar mockRegistry = new global::Mockolate.MockRegistry(mockBehavior);").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Mock.").Append(name).Append(" mockTarget = new global::Mockolate.Mock.")
			.Append(name).Append("(mockRegistry);")
			.AppendLine();
		sb.Append("\t\t\tif (setups.Length > 0)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tforeach (var setup in setups)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tsetup.Invoke(mockTarget);").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\treturn mockTarget.Object;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.Append("}").AppendLine();

		#endregion MockForXXXExtensions

		#endregion MockForXXXExtensions

		sb.AppendLine();

		#region MockForXXX

		sb.Append("internal static partial class Mock").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     A mock implementation for <see cref=\"").Append(escapedClassName).Append("\" />.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\t[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]").AppendLine();
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
		sb.Append("\tinternal class ").Append(name).Append(" :").AppendLine();
		sb.Append("\t\tIMockFor").Append(name).Append(',').AppendLine();
		sb.Append("\t\tglobal::Mockolate.IMock").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
		sb.Append("\t\tglobal::Mockolate.MockRegistry global::Mockolate.IMock.MockRegistry => this.").Append(mockRegistryName).Append(';').AppendLine();
		sb.Append("\t\tprivate global::Mockolate.MockRegistry ").Append(mockRegistryName).Append(" { get; }").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"").Append(name).Append("\" />").AppendLine();
		sb.Append("\t\tpublic ").Append(name).Append("(global::Mockolate.MockRegistry mockRegistry)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(" = mockRegistry;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Returns the actual delegate with the mock as target.");
		sb.Append("\t\tpublic ").Append(@class.ClassFullName).Append(" Object => new(Invoke);").AppendLine();
		sb.Append("\t\tprivate ")
			.Append(delegateMethod.ReturnType == Type.Void
				? "void"
				: delegateMethod.ReturnType.Fullname)
			.Append(" Invoke(")
			.Append(string.Join(", ",
				delegateMethod.Parameters.Select(p => $"{p.RefKind.GetString()}{p.Type.Fullname} {p.Name}")))
			.Append(')').AppendLine();
		sb.Append("\t\t{").AppendLine();
		string resultVarName = Helpers.GetUniqueLocalVariableName("result", delegateMethod.Parameters);
		if (delegateMethod.ReturnType != Type.Void)
		{
			string parameterVarName = Helpers.GetUniqueLocalVariableName("p", delegateMethod.Parameters);
			sb.Append("\t\t\tvar ").Append(resultVarName).Append(" = this.").Append(mockRegistryName).Append(".InvokeMethod<")
				.Append(delegateMethod.ReturnType.Fullname)
				.Append(">(").Append(delegateMethod.GetUniqueNameString());
			sb.Append(", ").Append(parameterVarName).Append(" => ")
				.AppendDefaultValueGeneratorFor(delegateMethod.ReturnType,
					$"this.{mockRegistryName}.Behavior.DefaultValue", $", {parameterVarName}");
		}
		else
		{
			sb.Append("\t\t\tvar ").Append(resultVarName).Append(" = this.").Append(mockRegistryName).Append(".InvokeMethod(")
				.Append(delegateMethod.GetUniqueNameString());
		}

		foreach (MethodParameter p in delegateMethod.Parameters)
		{
			sb.Append(", new global::Mockolate.Parameters.NamedParameterValue(\"").Append(p.Name).Append("\", ")
				.Append(p.RefKind switch
				{
					RefKind.Out => "null",
					_ => p.ToNameOrWrapper(),
				}).Append(')');
		}

		sb.AppendLine(");");

		AppendOutRefParameterHandling(sb, "\t\t\t", delegateMethod.Parameters, resultVarName,
			$"this.{mockRegistryName}.Behavior.DefaultValue");

		sb.Append("\t\t\t").Append(resultVarName).Append(".TriggerCallbacks(")
			.Append(string.Join(", ", delegateMethod.Parameters.Select(p => p.Name))).Append(");").AppendLine();
		if (delegateMethod.ReturnType != Type.Void)
		{
			sb.Append("\t\t\treturn ").Append(resultVarName).Append(".Result;").AppendLine();
		}

		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tstring global::Mockolate.IMock.ToString()").AppendLine();
		sb.Append("\t\t\t=> \"").Append(@class.DisplayString).Append(" mock\";").AppendLine();
		sb.AppendLine();
		
		AppendMethodSetupImplementation(sb, delegateMethod, mockRegistryName, $"IMockSetupFor{name}", false, "Setup");
		if (delegateMethod.Parameters.Count > 0)
		{
			AppendMethodSetupImplementation(sb, delegateMethod, mockRegistryName, $"IMockSetupFor{name}", true, "Setup");
		}

		AppendMethodVerifyImplementation(sb, delegateMethod, mockRegistryName, $"IMockVerifyFor{name}", false, "Verify");
		if (delegateMethod.Parameters.Count > 0)
		{
			AppendMethodVerifyImplementation(sb, delegateMethod, mockRegistryName, $"IMockVerifyFor{name}", true, "Verify");
		}

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<IMockVerifyFor").Append(name).Append("> IMockFor").Append(name).Append(".VerifySetup(global::Mockolate.Setup.IMethodSetup setup)").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".Method<IMockVerifyFor").Append(name).Append(">(this, setup);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tbool IMockFor").Append(name).Append(".VerifyThatAllInteractionsAreVerified()").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".Interactions.GetUnverifiedInteractions().Count == 0;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tbool IMockFor").Append(name).Append(".VerifyThatAllSetupsAreUsed()").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".GetUnusedSetups(this.").Append(mockRegistryName).Append(".Interactions).Count == 0;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tvoid IMockFor").Append(name).Append(".ClearAllInteractions()").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".ClearAllInteractions();").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Monitor.MockMonitor<IMockVerifyFor").Append(name).Append("> IMockFor").Append(name).Append(".Monitor()").AppendLine();
		sb.Append("\t\t\t=> new global::Mockolate.Monitor.MockMonitor<IMockVerifyFor").Append(name).Append(">(this.").Append(mockRegistryName).Append(".Interactions, interactions => new VerifyMonitor").Append(name).Append("(new global::Mockolate.MockRegistry(this.").Append(mockRegistryName).Append(", interactions)));").AppendLine();
		sb.AppendLine("\t}");

		sb.AppendLine();
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
		sb.Append("\tprivate sealed class VerifyMonitor").Append(name).Append("(global::Mockolate.MockRegistry mockRegistry) : global::Mockolate.Mock.IMockVerifyFor").Append(name).AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.MockRegistry ").Append(mockRegistryName).Append(" { get; } = mockRegistry;").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t#region IMockVerifyFor").Append(name).AppendLine();
		sb.AppendLine();
		AppendMethodVerifyImplementation(sb, delegateMethod, mockRegistryName, $"IMockVerifyFor{name}", false, "Verify");
		if (delegateMethod.Parameters.Count > 0)
		{
			AppendMethodVerifyImplementation(sb, delegateMethod, mockRegistryName, $"IMockVerifyFor{name}", true, "Verify");
		}

		sb.Append("\t\t#endregion IMockVerifyFor").Append(name).AppendLine();
		sb.Append("\t}").AppendLine();

		#endregion MockForXXX

		sb.AppendLine();

		#region IMockForXXX

		sb.AppendXmlSummary($"Accesses the mock of <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal interface IMockFor").Append(name).Append(" :").AppendLine();
		sb.Append("\t\t IMockSetupFor").Append(name).Append(", IMockVerifyFor").Append(name).AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Verifies the method invocations for the <paramref name=\"setup\" /> on the mock.");
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<IMockVerifyFor").Append(name).Append("> VerifySetup(global::Mockolate.Setup.IMethodSetup setup);").AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Gets a value indicating whether all expected interactions have been verified.");
		sb.Append("\t\tbool VerifyThatAllInteractionsAreVerified();").AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Gets a value indicating whether all registered setups were used.");
		sb.Append("\t\tbool VerifyThatAllSetupsAreUsed();").AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Clears all interactions recorded by the mock object.");
		sb.Append("\t\tvoid ClearAllInteractions();").AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Provides monitoring capabilities for a mocked instance of the specified type, allowing inspection of accessed properties, invoked methods, and event subscriptions.");
		sb.Append("\t\tglobal::Mockolate.Monitor.MockMonitor<IMockVerifyFor").Append(name).Append("> Monitor();").AppendLine();
		sb.Append("\t}").AppendLine();

		#endregion IMockForXXX

		sb.AppendLine();

		#region IMockSetupForXXX

		sb.AppendXmlSummary($"Set up the mock of <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal interface IMockSetupFor").Append(name).Append(" : global::Mockolate.Setup.IMockSetup<").Append(@class.ClassFullName).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		AppendMethodSetupDefinition(sb, @class, delegateMethod, false, "Setup");
		if (delegateMethod.Parameters.Count > 0)
		{
			AppendMethodSetupDefinition(sb, @class, delegateMethod, true, "Setup");
		}

		sb.Append("\t}").AppendLine();

		#endregion IMockSetupForXXX

		sb.AppendLine();

		#region IMockVerifyForXXX

		sb.AppendXmlSummary($"Verify interactions with the mock of <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal interface IMockVerifyFor").Append(name).Append(" : global::Mockolate.Verify.IMockVerify<").Append(@class.ClassFullName).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		AppendMethodVerifyDefinition(sb, @class, delegateMethod, $"IMockVerifyFor{name}", false, "Verify");
		if (delegateMethod.Parameters.Count > 0)
		{
			AppendMethodVerifyDefinition(sb, @class, delegateMethod, $"IMockVerifyFor{name}", true, "Verify");
		}

		sb.Append("\t}").AppendLine();

		#endregion IMockVerifyForXXX

		sb.Append("}").AppendLine();
		sb.AppendLine("#nullable disable annotations");
		return sb.ToString();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
