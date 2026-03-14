using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Mockolate.SourceGenerators.Internals;
using GenericParameter = Mockolate.SourceGenerators.Entities.GenericParameter;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string MockClass(string name, Class @class)
	{
		EquatableArray<Method>? constructors = (@class as MockClass)?.Constructors;
		string escapedClassName = @class.ClassFullName.EscapeForXmlDoc();
		bool hasEvents = @class.AllEvents().Any();
		StringBuilder sb = InitializeBuilder();

		sb.Append("#nullable enable annotations").AppendLine();
		sb.Append("namespace Mockolate;").AppendLine();
		sb.AppendLine();

		#region MockForXXXExtensions

		sb.AppendXmlSummary($"Mock extensions for <see cref=\"{escapedClassName}\" />.", "");
		sb.Append("internal static partial class MockExtensionsFor").Append(name).AppendLine();
		sb.Append("{").AppendLine();

		#region Type extensions

		sb.Append("\t/// <inheritdoc cref=\"MockExtensionsFor").Append(name).Append("\" />").AppendLine();
		sb.Append("\textension(").Append(@class.ClassFullName).Append(" mock)").AppendLine();
		sb.Append("\t{").AppendLine();

		#region Mock Property

		string mockPropertyName = CreateUniquePropertyName(@class, "Mock");

		sb.AppendXmlSummary($"Get access to the mock for <see cref=\"{escapedClassName}\" />.");
		sb.Append("\t\tpublic global::Mockolate.Mock.IMockFor").Append(name).Append(' ').Append(mockPropertyName).AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tget").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tif (mock is global::Mockolate.Mock.IMockFor").Append(name).Append(" mockInterface)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\treturn mockInterface;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"The subject is no mock.\");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		#endregion Mock Property

		#region CreateMock

		sb.AppendXmlSummary($"Create a new mock for <see cref=\"{escapedClassName}\" /> with the default <see cref=\"global::Mockolate.MockBehavior\" />.");
		sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
		sb.Append("\t\tpublic static global::Mockolate.Mock.").Append(name).Append(" CreateMock()").AppendLine();
		sb.Append("\t\t\t=> CreateMock(null, null, []);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Create a new mock for <see cref=\"{escapedClassName}\" /> with the default <see cref=\"global::Mockolate.MockBehavior\" />.");
		sb.AppendXmlRemarks("All provided <paramref name=\"setups\" /> are immediately applied to the mock.");
		sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
		sb.Append("\t\tpublic static global::Mockolate.Mock.").Append(name).Append(" CreateMock(params global::System.Action<global::Mockolate.Mock.IMockSetupFor").Append(name).Append(">[] setups)").AppendLine();
		sb.Append("\t\t\t=> CreateMock(null, null, setups);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Create a new mock for <see cref=\"{escapedClassName}\" /> with the given <paramref name=\"mockBehavior\" />.");
		sb.AppendXmlRemarks("All provided <paramref name=\"setups\" /> are immediately applied to the mock.");
		sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
		sb.Append("\t\tpublic static global::Mockolate.Mock.").Append(name).Append(" CreateMock(global::Mockolate.MockBehavior mockBehavior, params global::System.Action<global::Mockolate.Mock.IMockSetupFor").Append(name).Append(">[] setups)").AppendLine();
		sb.Append("\t\t\t=> CreateMock(null, mockBehavior, setups);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Create a new mock for <see cref=\"{escapedClassName}\" /> using the <paramref name=\"constructorParameters\" /> with the given <paramref name=\"mockBehavior\" />.");
		sb.AppendXmlRemarks("All provided <paramref name=\"setups\" /> are immediately applied to the mock.");
		sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
		sb.Append("\t\tpublic static global::Mockolate.Mock.").Append(name).Append(" CreateMock(object?[]? constructorParameters, global::Mockolate.MockBehavior? mockBehavior = null, params global::System.Action<global::Mockolate.Mock.IMockSetupFor").Append(name).Append(">[] setups)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tmockBehavior ??= global::Mockolate.MockBehavior.Default;").AppendLine();
		sb.Append("\t\t\tIMockBehaviorAccess mockBehaviorAccess = (global::Mockolate.IMockBehaviorAccess)mockBehavior;").AppendLine();
		sb.Append("\t\t\tif (mockBehaviorAccess.TryGet<global::System.Action<global::Mockolate.Mock.IMockSetupFor").Append(name).Append(">[]?>(\"Setup[").Append(name).Append("]\", out var additionalSetups))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tif (setups.Length > 0)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tvar concatenatedSetups = new global::System.Action<global::Mockolate.Mock.IMockSetupFor").Append(name).Append(">[additionalSetups.Length + setups.Length];").AppendLine();
		sb.Append("\t\t\t\t\tadditionalSetups.CopyTo(concatenatedSetups, 0);").AppendLine();
		sb.Append("\t\t\t\t\tsetups.CopyTo(concatenatedSetups, additionalSetups.Length);").AppendLine();
		sb.Append("\t\t\t\t\tsetups = concatenatedSetups;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\telse").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tsetups = additionalSetups;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.MockRegistration registrations = new global::Mockolate.MockRegistration(mockBehavior, \"").Append(@class.ClassFullName).Append("\");").AppendLine();
		sb.Append("\t\t\tif (constructorParameters is null && mockBehaviorAccess.TryGetConstructorParameters<").Append(@class.ClassFullName).Append(">(out object?[]? parameters))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tconstructorParameters = parameters;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		if (@class is { ClassFullName: "global::System.Net.Http.HttpClient", })
		{
			sb.Append("\t\t\tif (constructorParameters is null)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tconstructorParameters = [new global::Mockolate.Mock.HttpMessageHandler(registrations),];").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
		}

		sb.AppendLine();

		if (!@class.IsInterface && constructors?.Count > 0)
		{
			sb.Append("\t\t\tif (constructorParameters is null || constructorParameters.Length == 0)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			if (constructors.Value.Any(m => m.Parameters.Count == 0))
			{
				sb.Append("\t\t\t\tglobal::Mockolate.Mock.").Append(name).Append(".MockRegistrationsProvider.Value = registrations;").AppendLine();
				sb.Append("\t\t\t\tif (setups.Length > 0)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\tvar setupTarget = new global::Mockolate.MockExtensionsFor").Append(name).Append(".MockSetup(registrations);").AppendLine();
				sb.Append("\t\t\t\t\tforeach (var setup in setups)").AppendLine();
				sb.Append("\t\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
				sb.Append("\t\t\t\t\t}").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
				sb.Append("\t\t\t\treturn new global::Mockolate.Mock.").Append(name).Append("(registrations);").AppendLine();
			}
			else
			{
				sb.Append("\t\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"No parameterless constructor found for '").Append(@class.DisplayString).Append("'. Please provide constructor parameters.\");").AppendLine();
			}

			sb.Append("\t\t\t}").AppendLine();
			int constructorIndex = 0;
			bool useTryCast = false;
			bool useTryCastWithDefaultValue = false;
			foreach (EquatableArray<MethodParameter> constructorParameters in constructors.Value
				         .Select(constructor => constructor.Parameters))
			{
				constructorIndex++;
				int requiredParameters = constructorParameters.Count(c => !c.HasExplicitDefaultValue);
				if (requiredParameters < constructorParameters.Count)
				{
					sb.Append("\t\t\telse if (constructorParameters.Length >= ")
						.Append(requiredParameters).Append(" && constructorParameters.Length <= ")
						.Append(constructorParameters.Count);
				}
				else
				{
					sb.Append("\t\t\telse if (constructorParameters.Length == ")
						.Append(constructorParameters.Count);
				}

				int constructorParameterIndex = 0;
				foreach (MethodParameter parameter in constructorParameters)
				{
					useTryCast = useTryCast || !parameter.HasExplicitDefaultValue;
					useTryCastWithDefaultValue = useTryCastWithDefaultValue || parameter.HasExplicitDefaultValue;
					sb.AppendLine().Append("\t\t\t    && ")
						.Append(parameter.HasExplicitDefaultValue ? "TryCastWithDefaultValue" : "TryCast")
						.Append("(constructorParameters, ")
						.Append(constructorParameterIndex++)
						.Append(parameter.HasExplicitDefaultValue ? $", {parameter.ExplicitDefaultValue}" : "")
						.Append(", mockBehavior, out ").Append(parameter.Type.Fullname).Append(" c")
						.Append(constructorIndex)
						.Append('p')
						.Append(constructorParameterIndex).Append(")");
				}

				sb.Append(")").AppendLine();
				sb.Append("\t\t\t{").AppendLine();
				sb.Append("\t\t\t\tglobal::Mockolate.Mock.").Append(name).Append(".MockRegistrationsProvider.Value = registrations;").AppendLine();
				sb.Append("\t\t\t\tif (setups.Length > 0)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\tvar setupTarget = new global::Mockolate.MockExtensionsFor").Append(name).Append(".MockSetup(registrations);").AppendLine();
				sb.Append("\t\t\t\t\tforeach (var setup in setups)").AppendLine();
				sb.Append("\t\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
				sb.Append("\t\t\t\t\t}").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
				sb.Append("\t\t\t\treturn new global::Mockolate.Mock.").Append(name)
					.Append("(registrations");
				for (int i = 1; i <= constructorParameters.Count; i++)
				{
					sb.Append(", ").Append('c').Append(constructorIndex).Append('p').Append(i);
				}

				sb.Append(");").AppendLine();
				sb.Append("\t\t\t}").AppendLine();
			}

			sb.Append("\t\t\telse").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append(
					"\t\t\t\tthrow new global::Mockolate.Exceptions.MockException($\"Could not find any constructor for '")
				.Append(@class.DisplayString)
				.Append(
					"' that matches the {constructorParameters.Length} given parameters ({string.Join(\", \", constructorParameters)}).\");")
				.AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			if (useTryCast)
			{
				sb.Append("""
				          			static bool TryCast<TValue>(object?[] values, int index, global::Mockolate.MockBehavior behavior, out TValue result)
				          			{
				          			    var value = values[index];
				          				if (value is TValue typedValue)
				          				{
				          					result = typedValue;
				          					return true;
				          				}
				          				
				          				result = default!;
				          				return value is null;
				          			}
				          """).AppendLine();
			}

			if (useTryCastWithDefaultValue)
			{
				sb.Append("""
				          			static bool TryCastWithDefaultValue<TValue>(object?[] values, int index, TValue defaultValue, global::Mockolate.MockBehavior behavior, out TValue result)
				          			{
				          				if (values.Length > index && values[index] is TValue typedValue)
				          				{
				          					result = typedValue;
				          					return true;
				          				}
				          				
				          				result = defaultValue;
				          				return true;
				          			}
				          """).AppendLine();
			}
		}
		else
		{
			sb.Append("\t\t\tvar value = new global::Mockolate.Mock.").Append(name).Append("(registrations);")
				.AppendLine();
			sb.Append("\t\t\tif (setups.Length > 0)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tforeach (var setup in setups)").AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\tsetup.Invoke(value);").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\treturn value;").AppendLine();
		}

		sb.Append("\t\t}").AppendLine();

		#endregion CreateMock

		if (@class.IsInterface)
		{
			sb.AppendXmlSummary("Create a mock that wraps the given <paramref name=\"instance\" />.");
			sb.AppendXmlRemarks("All interactions are forwarded to the <paramref name=\"instance\" />.");
			sb.Append("\t\tpublic global::Mockolate.Mock.").Append(name).Append(" Wrapping(").Append(@class.ClassFullName).Append(" instance)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tif (mock is global::Mockolate.IMock mockInterface)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn new global::Mockolate.Mock.").Append(name).Append("(mockInterface.Registrations, instance);").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"The subject is no mock.\");").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t}").AppendLine();

		#endregion Type extensions

		sb.AppendLine();

		#region MockBehavior extensions

		sb.Append("\t/// <inheritdoc cref=\"MockExtensionsFor").Append(name).Append("\" />").AppendLine();
		sb.Append("\textension(global::Mockolate.MockBehavior behavior)").AppendLine();
		sb.Append("\t{").AppendLine();

		sb.AppendXmlSummary("Initialize mocks of type <typeparamref name=\"T\" /> with the given <paramref name=\"setups\" />.");
		sb.AppendXmlRemarks("The <paramref name=\"setups\" /> are applied to the mock before the constructor is executed.");
		sb.Append("\t\tpublic global::Mockolate.MockBehavior Initialize<T>(params global::System.Action<global::Mockolate.Mock.IMockSetupFor").Append(name).Append(">[] setups)").AppendLine();
		sb.Append("\t\t\twhere T : ").Append(@class.ClassFullName).AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar behaviorAccess = (global::Mockolate.IMockBehaviorAccess)behavior;").AppendLine();
		sb.Append("\t\t\treturn behaviorAccess.Set(\"Setup[").Append(name).Append("]\", setups);").AppendLine();
		sb.Append("\t\t}").AppendLine();

		sb.Append("\t}").AppendLine();

		#endregion MockBehavior extensions

		#region Setup helpers

		if (!@class.IsInterface && constructors?.Count > 0)
		{
			sb.AppendLine();
			sb.Append("\tinternal sealed class MockSetup(global::Mockolate.MockRegistration registrations) : global::Mockolate.Mock.IMockSetupFor").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			sb.Append("\t\tprivate global::Mockolate.MockRegistration Registrations { get; } = registrations;").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t#region IMockSetupFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementSetupInterface(sb, @class, name);
			sb.Append("\t\t#endregion IMockSetupFor").Append(name).AppendLine();
			sb.Append("\t}").AppendLine();
		}

		#endregion Setup helpers

		sb.Append("}").AppendLine();

		#endregion MockForXXXExtensions

		sb.AppendLine();

		#region MockForXXX

		sb.Append("internal static partial class Mock").AppendLine();
		sb.Append("{").AppendLine();
		sb.AppendXmlSummary($"A mock implementation for <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal class ").Append(name).Append(" :").AppendLine();
		sb.Append("\t\t").Append(@class.ClassFullName);
		sb.Append(", IMockFor").Append(name).Append(", IMockSetupFor").Append(name);
		if (hasEvents)
		{
			sb.Append(", IMockRaiseOn").Append(name);
		}

		sb.Append(", IMockVerifyFor").Append(name).Append(',').AppendLine();
		sb.Append("\t\tglobal::Mockolate.IMock").AppendLine();
		sb.Append("\t{").AppendLine();

		if (@class.IsInterface)
		{
			sb.AppendXmlSummary("The wrapped instance (if any) of the mocked interface to call the base implementation on.");
			sb.Append("\t\tprivate ").Append(@class.ClassFullName).Append("? Wraps { get; }").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tobject?[] global::Mockolate.IMock.ConstructorParameters => this.ConstructorParameters;").AppendLine();
		sb.Append("\t\tprivate object?[] ConstructorParameters { get; }").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.MockRegistration global::Mockolate.IMock.Registrations => this.Registrations;").AppendLine();
		if (constructors?.Count > 0)
		{
			sb.Append("\t\tprivate global::Mockolate.MockRegistration Registrations").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget => field ?? MockRegistrationsProvider.Value;").AppendLine();
			sb.Append("\t\t\tset;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tinternal static readonly global::System.Threading.AsyncLocal<global::Mockolate.MockRegistration> MockRegistrationsProvider = new global::System.Threading.AsyncLocal<global::Mockolate.MockRegistration>();").AppendLine();
		}
		else
		{
			sb.Append("\t\tprivate global::Mockolate.MockRegistration Registrations { get; }").AppendLine();
		}

		sb.AppendLine();

		ImplementMockForInterface(sb, name, hasEvents);
		sb.AppendLine();

		if (@class.IsInterface)
		{
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(name).Append("\" />").AppendLine();
			sb.Append("\t\tpublic ").Append(name).Append("(global::Mockolate.MockRegistration registrations, ")
				.Append(@class.ClassFullName).Append("? wraps = null)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tthis.ConstructorParameters = [];").AppendLine();
			sb.Append("\t\t\tthis.Registrations = registrations;").AppendLine();
			sb.Append("\t\t\tthis.Wraps = wraps;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}
		else if (constructors is not null)
		{
			foreach (Method constructor in constructors)
			{
				AppendMockSubject_BaseClassConstructor(sb, name, constructor);
			}
		}
		else
		{
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(name).Append("\" />").AppendLine();
			sb.Append("\t\tpublic ").Append(name).Append("(global::Mockolate.MockRegistration registrations)")
				.AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tthis.ConstructorParameters = new object?[0];").AppendLine();
			sb.Append("\t\t\tthis.Registrations = registrations;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		AppendMockSubject_ImplementClass(sb, @class, null);
		sb.AppendLine();

		#region IMockSetupForXXX

		sb.Append("\t\t#region IMockSetupFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementSetupInterface(sb, @class, name);
		sb.Append("\t\t#endregion IMockSetupFor").Append(name).AppendLine();

		#endregion IMockSetupForXXX

		if (hasEvents)
		{
			#region IMockRaiseOnXXX

			sb.AppendLine();
			sb.Append("\t\t#region IMockRaiseOn").Append(name).AppendLine();
			sb.AppendLine();
			ImplementRaiseInterface(sb, @class, name);
			sb.Append("\t\t#endregion IMockRaiseOn").Append(name).AppendLine();

			#endregion IMockRaiseOnXXX
		}

		#region IMockVerifyForXXX

		sb.AppendLine();
		sb.Append("\t\t#region IMockVerifyFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementVerifyInterface(sb, @class, name);
		sb.Append("\t\t#endregion IMockVerifyFor").Append(name).AppendLine();

		#endregion IMockVerifyForXXX

		sb.AppendLine("\t}");

		sb.AppendLine();
		sb.Append("\tprivate sealed class VerifyMonitor").Append(name).Append("(global::Mockolate.MockRegistration registrations) : global::Mockolate.Mock.IMockVerifyFor").Append(name).AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.MockRegistration Registrations { get; } = registrations;").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t#region IMockVerifyFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementVerifyInterface(sb, @class, name);
		sb.Append("\t\t#endregion IMockVerifyFor").Append(name).AppendLine();
		sb.Append("\t}").AppendLine();

		#endregion MockForXXX

		sb.AppendLine();

		#region IMockForXXX

		sb.AppendXmlSummary($"Accesses the mock for <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal interface IMockFor").Append(name).AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary($"Set up the mock for <see cref=\"{escapedClassName}\" />.");
		sb.Append("\t\tIMockSetupFor").Append(name).Append(" Setup { get; }").AppendLine();
		sb.AppendLine();
		if (hasEvents)
		{
			sb.AppendXmlSummary($"Raise events on the mock for <see cref=\"{escapedClassName}\" />.");
			sb.Append("\t\tIMockRaiseOn").Append(name).Append(" Raise { get; }").AppendLine();
			sb.AppendLine();
		}

		sb.AppendXmlSummary($"Verify interactions with the mock for <see cref=\"{escapedClassName}\" />.");
		sb.Append("\t\tIMockVerifyFor").Append(name).Append(" Verify { get; }").AppendLine();
		sb.AppendLine();
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

		sb.AppendXmlSummary($"Set up the mock for <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal interface IMockSetupFor").Append(name).Append(" : global::Mockolate.Setup.IMockSetup<").Append(@class.ClassFullName).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		DefineSetupInterface(sb, @class);
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		#endregion IMockSetupForXXX

		if (hasEvents)
		{
			#region IMockRaiseOnXXX

			sb.AppendXmlSummary($"Raise events on the mock for <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockRaiseOn").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineRaiseInterface(sb, @class);
			sb.Append("\t}").AppendLine();
			sb.AppendLine();

			#endregion IMockRaiseOnXXX
		}

		#region IMockVerifyForXXX

		sb.AppendXmlSummary($"Verify interactions with the mock for <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal interface IMockVerifyFor").Append(name).Append(" : global::Mockolate.Verify.IMockVerify<").Append(@class.ClassFullName).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		DefineVerifyInterface(sb, @class, name);
		sb.Append("\t}").AppendLine();

		#endregion IMockVerifyForXXX

		sb.Append("}").AppendLine();
		sb.AppendLine();
		sb.AppendLine("#nullable disable annotations");
		return sb.ToString();
	}

	private static string CreateUniquePropertyName(Class @class, string initialValue)
	{
		string propertyName = initialValue;
		if (@class.Properties.Any(m => m.Name == propertyName))
		{
			propertyName = $"Mockolate_{initialValue}";
		}

		if (@class.Properties.Any(m => m.Name == propertyName))
		{
			int index = 1;
			do
			{
				propertyName = $"Mockolate_{initialValue}__" + index++;
			} while (@class.Properties.Any(m => m.Name == propertyName));
		}

		return propertyName;
	}

	private static void ImplementMockForInterface(StringBuilder sb, string name, bool hasEvents)
	{
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tIMockSetupFor").Append(name).Append(" IMockFor").Append(name).Append(".Setup").AppendLine();
		sb.Append("\t\t\t=> this;").AppendLine();

		if (hasEvents)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\tIMockRaiseOn").Append(name).Append(" IMockFor").Append(name).Append(".Raise").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tIMockVerifyFor").Append(name).Append(" IMockFor").Append(name).Append(".Verify").AppendLine();
		sb.Append("\t\t\t=> this;").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<IMockVerifyFor").Append(name).Append("> IMockFor").Append(name).Append(".VerifySetup(global::Mockolate.Setup.IMethodSetup setup)").AppendLine();
		sb.Append("\t\t\t=> this.Registrations.Method<IMockVerifyFor").Append(name).Append(">(this, setup);").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tbool IMockFor").Append(name).Append(".VerifyThatAllInteractionsAreVerified()").AppendLine();
		sb.Append("\t\t\t=> this.Registrations.Interactions.GetUnverifiedInteractions().Count == 0;").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tbool IMockFor").Append(name).Append(".VerifyThatAllSetupsAreUsed()").AppendLine();
		sb.Append("\t\t\t=> this.Registrations.GetUnusedSetups(this.Registrations.Interactions).Count == 0;").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tvoid IMockFor").Append(name).Append(".ClearAllInteractions()").AppendLine();
		sb.Append("\t\t\t=> this.Registrations.ClearAllInteractions();").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Monitor.MockMonitor<IMockVerifyFor").Append(name).Append("> IMockFor").Append(name).Append(".Monitor()").AppendLine();
		sb.Append("\t\t\t=> new global::Mockolate.Monitor.MockMonitor<IMockVerifyFor").Append(name).Append(">(this.Registrations.Interactions, interactions => new VerifyMonitor").Append(name).Append("(new global::Mockolate.MockRegistration(this.Registrations.Behavior, this.Registrations.Prefix, interactions)));").AppendLine();
		sb.AppendLine();
	}

	#region Mock Helpers

	private static void AppendMockSubject_BaseClassConstructor(StringBuilder sb, string name, Method constructor)
	{
		string registrations = CreateUniqueParameterName(constructor.Parameters, "registrations");
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(name).Append("\" />").AppendLine();
		sb.Append(constructor.Attributes, "\t\t");
		sb.Append("\t\tpublic ").Append(name).Append("(global::Mockolate.MockRegistration ").Append(registrations);
		foreach (MethodParameter parameter in constructor.Parameters)
		{
			sb.Append(", ");
			if (parameter.IsParams)
			{
				sb.Append("params ");
			}

			sb.Append(parameter.Type.Fullname).Append(' ').Append(parameter.Name);
			if (parameter.HasExplicitDefaultValue)
			{
				sb.Append(" = ").Append(parameter.ExplicitDefaultValue);
			}
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t\t: base(");
		int index = 0;
		foreach (MethodParameter parameter in constructor.Parameters)
		{
			if (index++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append(parameter.Name);
		}

		sb.Append(')').AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tthis.ConstructorParameters = new object?[] { ")
			.Append(string.Join(", ", constructor.Parameters.Select(parameter => parameter.Name))).Append(" };")
			.AppendLine();
		sb.Append("\t\t\tthis.Registrations = ").Append(registrations).Append(';').AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
	}

	private static void AppendMockSubject_ImplementClass(StringBuilder sb, Class @class,
		MockClass? mockClass)
	{
		string className = @class.ClassFullName;
		sb.Append("\t\t#region ").Append(@class.DisplayString).AppendLine();
		sb.AppendLine();
		int count = 0;
		List<Event>? mockEvents = mockClass?.AllEvents().ToList();
		foreach (Event @event in @class.AllEvents())
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			if (mockEvents?.All(e => !Event.EqualityComparer.Equals(@event, e)) != false)
			{
				AppendMockSubject_ImplementClass_AddEvent(sb, @event, className, mockClass is not null,
					@class.IsInterface);
			}
		}

		List<Property>? mockProperties = mockClass?.AllProperties().ToList();
		foreach (Property property in @class.AllProperties())
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}

			if (mockProperties?.All(p => !Property.EqualityComparer.Equals(property, p)) != false)
			{
				AppendMockSubject_ImplementClass_AddProperty(sb, property, className, mockClass is not null,
					@class.IsInterface);
			}
		}

		List<Method>? mockMethods = mockClass?.AllMethods().ToList();
		foreach (Method method in @class.AllMethods())
		{
			if (count++ > 0)
			{
				sb.AppendLine();
			}


			if (mockMethods?.All(m => !Method.EqualityComparer.Equals(method, m)) != false)
			{
				AppendMockSubject_ImplementClass_AddMethod(sb, method, className, mockClass is not null,
					@class.IsInterface, @class);
			}
		}

		sb.AppendLine();
		sb.Append("\t\t#endregion ").Append(@class.DisplayString).AppendLine();
	}

	private static void AppendMockSubject_ImplementClass_AddEvent(StringBuilder sb, Event @event, string className,
		bool explicitInterfaceImplementation, bool isClassInterface)
	{
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(@event.ContainingType.EscapeForXmlDoc()).Append('.')
			.Append(@event.Name.EscapeForXmlDoc()).AppendLine("\" />");
		sb.Append(@event.Attributes, "\t\t");
		if (explicitInterfaceImplementation)
		{
			sb.Append("\t\tevent ").Append(@event.Type.Fullname.TrimEnd('?'))
				.Append("? ").Append(className).Append('.').Append(@event.Name).AppendLine();
		}
		else
		{
			if (@event.ExplicitImplementation is null)
			{
				sb.Append("\t\t").Append(@event.Accessibility.ToVisibilityString()).Append(' ');
				if (!isClassInterface && @event.UseOverride)
				{
					sb.Append("override ");
				}

				sb.Append("event ").Append(@event.Type.Fullname.TrimEnd('?')).Append("? ");
			}
			else
			{
				sb.Append("\t\t").Append("event ").Append(@event.Type.Fullname.TrimEnd('?')).Append("? ")
					.Append(@event.ExplicitImplementation).Append('.');
			}

			sb.Append(@event.Name).AppendLine();
		}

		sb.AppendLine("\t\t{");
		if (isClassInterface && !explicitInterfaceImplementation && @event.ExplicitImplementation is null)
		{
			sb.Append("\t\t\tadd").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tthis.Registrations.AddEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
			sb.Append("\t\t\t\tif (this.Wraps is not null)").AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\tthis.Wraps.").Append(@event.Name).Append(" += value;").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\tremove").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tthis.Registrations.RemoveEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
			sb.Append("\t\t\t\tif (this.Wraps is not null)").AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\tthis.Wraps.").Append(@event.Name).Append(" -= value;").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
		}
		else
		{
			sb.Append("\t\t\tadd => this.Registrations.AddEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
			sb.Append("\t\t\tremove => this.Registrations.RemoveEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
		}

		sb.AppendLine("\t\t}");
	}

	private static void AppendMockSubject_ImplementClass_AddProperty(StringBuilder sb, Property property,
		string className, bool explicitInterfaceImplementation, bool isClassInterface)
	{
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(property.ContainingType.EscapeForXmlDoc()).Append('.').Append(
				property.IndexerParameters is not null
					? property.Name.Replace("[]",
							$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname}"))}]")
						.EscapeForXmlDoc()
					: property.Name.EscapeForXmlDoc())
			.AppendLine("\" />");
		sb.Append(property.Attributes, "\t\t");
		if (explicitInterfaceImplementation)
		{
			sb.Append("\t\t").Append(property.Type.Fullname)
				.Append(" ").Append(className).Append('.').Append(property.IndexerParameters is not null
					? property.Name.Replace("[]",
						$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname} {p.Name}"))}]")
					: property.Name).AppendLine();
		}
		else
		{
			if (property.ExplicitImplementation is null)
			{
				sb.Append("\t\t").Append(property.Accessibility.ToVisibilityString()).Append(' ');
				if (!isClassInterface && property.UseOverride)
				{
					sb.Append("override ");
				}

				sb.Append(property.Type.Fullname).Append(" ");
			}
			else
			{
				sb.Append("\t\t").Append(property.Type.Fullname).Append(" ").Append(property.ExplicitImplementation)
					.Append('.');
			}

			sb.Append(property.IndexerParameters is not null
				? property.Name.Replace("[]",
					$"[{string.Join(", ", property.IndexerParameters.Value.Select(p => $"{p.Type.Fullname} {p.Name}"))}]")
				: property.Name).AppendLine();
		}

		sb.AppendLine("\t\t{");
		if (property.Getter != null)
		{
			sb.Append("\t\t\t");
			if (property.Getter.Accessibility != property.Accessibility)
			{
				sb.Append(property.Getter.Accessibility.ToVisibilityString()).Append(' ');
			}

			sb.AppendLine("get");
			sb.AppendLine("\t\t\t{");
			if (isClassInterface && !explicitInterfaceImplementation && property.ExplicitImplementation is null)
			{
				if (property is { IsIndexer: true, IndexerParameters: not null, })
				{
					string indexerResultVarName =
						Helpers.GetUniqueLocalVariableName("indexerResult", property.IndexerParameters.Value);
					string baseResultVarName =
						Helpers.GetUniqueLocalVariableName("baseResult", property.IndexerParameters.Value);

					sb.Append("\t\t\t\tif (this.Wraps is null)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\treturn this.Registrations.GetIndexer<").AppendTypeOrWrapper(property.Type)
						.Append(">(").Append(FormatIndexerParametersAsNameOrWrapper(property.IndexerParameters.Value))
						.Append(").GetResult(() => ")
						.AppendDefaultValueGeneratorFor(property.Type, "this.Registrations.Behavior.DefaultValue")
						.Append(");").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\tvar ").Append(indexerResultVarName).Append(" = this.Registrations.GetIndexer<")
						.AppendTypeOrWrapper(property.Type).Append(">(")
						.Append(FormatIndexerParametersAsNameOrWrapper(property.IndexerParameters.Value))
						.AppendLine(");");
					sb.Append("\t\t\t\tvar ").Append(baseResultVarName).Append(" = this.Wraps[")
						.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value)).Append("];")
						.AppendLine();
					sb.Append("\t\t\t\treturn ").Append(indexerResultVarName).Append(".GetResult(")
						.Append(baseResultVarName).Append(");").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\treturn this.Registrations.GetProperty<")
						.AppendTypeOrWrapper(property.Type).Append(">(")
						.Append(property.GetUniqueNameString()).Append(", () => ")
						.AppendDefaultValueGeneratorFor(property.Type, "this.Registrations.Behavior.DefaultValue")
						.Append(", this.Wraps is null ? null : () => this.Wraps.").Append(property.Name)
						.Append(");").AppendLine();
				}
			}
			else if (!isClassInterface && !property.IsAbstract)
			{
				if (property is { IsIndexer: true, IndexerParameters: not null, })
				{
					string indexerResultVarName =
						Helpers.GetUniqueLocalVariableName("indexerResult", property.IndexerParameters.Value);
					string baseResultVarName =
						Helpers.GetUniqueLocalVariableName("baseResult", property.IndexerParameters.Value);
					sb.Append("\t\t\t\tvar ").Append(indexerResultVarName).Append(" = this.Registrations.GetIndexer<")
						.AppendTypeOrWrapper(property.Type).Append(">(")
						.Append(FormatIndexerParametersAsNameOrWrapper(property.IndexerParameters.Value))
						.AppendLine(");");
					sb.Append("\t\t\t\tif (!").Append(indexerResultVarName).Append(".SkipBaseClass)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tvar ").Append(baseResultVarName).Append(" = base[")
						.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value)).Append("];")
						.AppendLine();
					sb.Append("\t\t\t\t\treturn ").Append(indexerResultVarName).Append(".GetResult(")
						.Append(baseResultVarName).Append(");").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\treturn ").Append(indexerResultVarName)
						.Append(".GetResult(() => ")
						.AppendDefaultValueGeneratorFor(property.Type, "this.Registrations.Behavior.DefaultValue")
						.Append(");").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\treturn this.Registrations.GetProperty<")
						.AppendTypeOrWrapper(property.Type).Append(">(")
						.Append(property.GetUniqueNameString()).Append(", () => ")
						.AppendDefaultValueGeneratorFor(property.Type, "this.Registrations.Behavior.DefaultValue")
						.Append(", () => base.").Append(property.Name).Append(");").AppendLine();
				}
			}
			else if (property is { IsIndexer: true, IndexerParameters: not null, })
			{
				sb.Append("\t\t\t\treturn this.Registrations.GetIndexer<")
					.AppendTypeOrWrapper(property.Type).Append(">(")
					.Append(FormatIndexerParametersAsNameOrWrapper(property.IndexerParameters.Value))
					.Append(").GetResult(() => ")
					.AppendDefaultValueGeneratorFor(property.Type, "this.Registrations.Behavior.DefaultValue")
					.Append(");").AppendLine();
			}
			else
			{
				sb.Append("\t\t\t\treturn this.Registrations.GetProperty<")
					.AppendTypeOrWrapper(property.Type).Append(">(").Append(property.GetUniqueNameString())
					.Append(", () => ")
					.AppendDefaultValueGeneratorFor(property.Type, "this.Registrations.Behavior.DefaultValue")
					.Append(", null);").AppendLine();
			}

			sb.AppendLine("\t\t\t}");
		}

		if (property.Setter != null)
		{
			sb.Append("\t\t\t");
			if (property.Setter.Accessibility != property.Accessibility)
			{
				sb.Append(property.Setter.Accessibility.ToVisibilityString()).Append(' ');
			}

			sb.AppendLine("set");
			sb.AppendLine("\t\t\t{");

			if (isClassInterface && !explicitInterfaceImplementation && property.ExplicitImplementation is null)
			{
				if (property is { IsIndexer: true, IndexerParameters: not null, })
				{
					sb.Append("\t\t\t\tthis.Registrations.SetIndexer<")
						.Append(property.Type.Fullname)
						.Append(">(value, ")
						.Append(FormatIndexerParametersAsNameOrWrapper(property.IndexerParameters.Value))
						.Append(");").AppendLine();

					sb.Append("\t\t\t\tif (this.Wraps is not null)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tthis.Wraps[")
						.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value))
						.AppendLine("] = value;");
					sb.Append("\t\t\t\t}").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\tthis.Registrations.SetProperty(").Append(property.GetUniqueNameString())
						.Append(", value);").AppendLine();
					sb.Append("\t\t\t\tif (this.Wraps is not null)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tthis.Wraps.").Append(property.Name).Append(" = value;").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
				}
			}
			else if (property is { IsIndexer: true, IndexerParameters: not null, })
			{
				if (!isClassInterface && !property.IsAbstract)
				{
					sb.Append("\t\t\t\tif (!this.Registrations.SetIndexer<").Append(property.Type.Fullname)
						.Append(">(value, ")
						.Append(FormatIndexerParametersAsNameOrWrapper(property.IndexerParameters.Value)).Append("))")
						.AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tbase[")
						.Append(FormatIndexerParametersAsNames(property.IndexerParameters.Value))
						.AppendLine("] = value;");
					sb.Append("\t\t\t\t}").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\tthis.Registrations.SetIndexer<")
						.Append(property.Type.Fullname)
						.Append(">(value, ")
						.Append(FormatIndexerParametersAsNameOrWrapper(property.IndexerParameters.Value))
						.AppendLine(");");
				}
			}
			else
			{
				if (!isClassInterface && !property.IsAbstract)
				{
					sb.Append("\t\t\t\tif (!this.Registrations.SetProperty(").Append(property.GetUniqueNameString())
						.Append(", value))").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tbase.").Append(property.Name).Append(" = value;").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\tthis.Registrations.SetProperty(").Append(property.GetUniqueNameString())
						.AppendLine(", value);");
				}
			}

			sb.AppendLine("\t\t\t}");
		}

		sb.AppendLine("\t\t}");
	}

	private static void AppendMockSubject_ImplementClass_AddMethod(StringBuilder sb, Method method, string className,
		bool explicitInterfaceImplementation, bool isClassInterface, Class @class)
	{
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(method.ContainingType.EscapeForXmlDoc()).Append('.')
			.Append(method.Name.EscapeForXmlDoc()).Append('(')
			.Append(string.Join(", ", method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname))
				.EscapeForXmlDoc()).AppendLine(")\" />");
		sb.Append(method.Attributes, "\t\t");
		if (explicitInterfaceImplementation)
		{
			sb.Append("\t\t");
			sb.Append(method.ReturnType.Fullname).Append(' ').Append(className).Append('.').Append(method.Name)
				.Append('(');
		}
		else
		{
			sb.Append("\t\t");
			if (method.ExplicitImplementation is null)
			{
				sb.Append(method.Accessibility.ToVisibilityString()).Append(' ');
				if ((!isClassInterface && method.UseOverride) || method.IsEquals() || method.IsGetHashCode() ||
				    method.IsToString())
				{
					sb.Append("override ");
				}

				sb.Append(method.ReturnType.Fullname).Append(' ')
					.Append(method.Name).Append('(');
			}
			else
			{
				sb.Append(method.ReturnType.Fullname).Append(' ')
					.Append(method.ExplicitImplementation).Append('.').Append(method.Name).Append('(');
			}
		}

		int index = 0;
		foreach (MethodParameter parameter in method.Parameters)
		{
			if (index++ > 0)
			{
				sb.Append(", ");
			}

			sb.Append(parameter.RefKind.GetString());
			if (parameter.IsParams)
			{
				sb.Append("params ");
			}

			sb.Append(parameter.Type.Fullname).Append(' ').Append(parameter.Name);
			if (!explicitInterfaceImplementation && parameter.HasExplicitDefaultValue)
			{
				sb.Append(" = ").Append(parameter.ExplicitDefaultValue);
			}
		}

		sb.Append(')');
		if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
		{
			foreach (GenericParameter gp in method.GenericParameters.Value)
			{
				gp.AppendWhereConstraint(sb, "\t\t\t");
			}
		}

		sb.AppendLine();
		sb.AppendLine("\t\t{");
		string methodExecutionVarName = Helpers.GetUniqueLocalVariableName("methodExecution", method.Parameters);
		if (method.ReturnType != Type.Void)
		{
			string parameterVarName = Helpers.GetUniqueLocalVariableName("p", method.Parameters);
			sb.Append("\t\t\tglobal::Mockolate.Setup.MethodSetupResult<")
				.AppendTypeOrWrapper(method.ReturnType).Append("> ").Append(methodExecutionVarName)
				.Append(" = this.Registrations.InvokeMethod<")
				.AppendTypeOrWrapper(method.ReturnType).Append(">(").Append(method.GetUniqueNameString())
				.Append(", ").Append(parameterVarName).Append(" => ")
				.AppendDefaultValueGeneratorFor(method.ReturnType, "this.Registrations.Behavior.DefaultValue",
					$", {parameterVarName}");
		}
		else
		{
			sb.Append("\t\t\tglobal::Mockolate.Setup.MethodSetupResult ").Append(methodExecutionVarName)
				.Append(" = this.Registrations.InvokeMethod(")
				.Append(method.GetUniqueNameString());
		}

		foreach (MethodParameter p in method.Parameters)
		{
			sb.Append(", new global::Mockolate.Parameters.NamedParameterValue(\"").Append(p.Name).Append("\", ").Append(
				p.RefKind switch
				{
					RefKind.Out => "null",
					_ => p.ToNameOrWrapper(),
				}).Append(')');
		}

		sb.AppendLine(");");

		if (isClassInterface || method.IsAbstract)
		{
			if (!explicitInterfaceImplementation && isClassInterface)
			{
				string baseResultVarName = Helpers.GetUniqueLocalVariableName("baseResult", method.Parameters);
				if (method.ReturnType != Type.Void)
				{
					sb.Append("\t\t\tif (this.Wraps is not null)").AppendLine();
					sb.Append("\t\t\t{").AppendLine();
					sb.Append("\t\t\t\tvar ").Append(baseResultVarName).Append(" = this.Wraps").Append(".")
						.Append(method.Name).Append('(')
						.Append(FormatMethodParametersWithRefKind(method.Parameters))
						.Append(");").AppendLine();
				}
				else
				{
					sb.Append("\t\t\tif (this.Wraps is not null)").AppendLine();
					sb.Append("\t\t\t{").AppendLine();
					sb.Append("\t\t\t\tthis.Wraps").Append(".")
						.Append(method.Name).Append('(')
						.Append(FormatMethodParametersWithRefKind(method.Parameters))
						.Append(");").AppendLine();
				}

				AppendConditionalOutRefParameterHandling(sb, "\t\t\t\t", method.Parameters, methodExecutionVarName,
					"this.Registrations.Behavior.DefaultValue");

				if (method.ReturnType != Type.Void)
				{
					sb.Append("\t\t\t\tif (!").Append(methodExecutionVarName).Append(".HasSetupResult)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					AppendTriggerCallbacks(sb, "\t\t\t\t\t", methodExecutionVarName, method.Parameters);
					sb.Append("\t\t\t\t\treturn ").Append(baseResultVarName).Append(";").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
				}

				sb.Append("\t\t\t}").AppendLine();
			}

			AppendOutRefParameterHandling(sb, "\t\t\t", method.Parameters, methodExecutionVarName,
				"this.Registrations.Behavior.DefaultValue");

			AppendTriggerCallbacks(sb, "\t\t\t", methodExecutionVarName, method.Parameters);
			if (method.ReturnType != Type.Void)
			{
				sb.Append("\t\t\treturn ").Append(methodExecutionVarName).Append(".Result;").AppendLine();
			}
		}
		else
		{
			sb.Append("\t\t\tif (!").Append(methodExecutionVarName).Append(".SkipBaseClass)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			if (method.ReturnType != Type.Void)
			{
				string baseResultVarName = Helpers.GetUniqueLocalVariableName("baseResult", method.Parameters);

				if (method.Name.StartsWith("Send", StringComparison.Ordinal) &&
				    @class is { ClassFullName: "global::System.Net.Http.HttpClient", })
				{
					sb.Append("\t\t\t\t#if NETFRAMEWORK").AppendLine();
					sb.Append(
							"\t\t\t\t// Persist the HttpContent, because it gets automatically disposed on .NET Framework")
						.AppendLine();
					sb.Append("\t\t\t\tif (request.Content != null)").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append(
							"\t\t\t\t\tvar stream = request.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();")
						.AppendLine();
					sb.Append("\t\t\t\t\tusing global::System.IO.MemoryStream ms = new();").AppendLine();
					sb.Append("\t\t\t\t\tstream.CopyTo(ms);").AppendLine();
					sb.Append("\t\t\t\t\tbyte[] bytes = ms.ToArray();").AppendLine();
					sb.Append("\t\t\t\t\tstream.Position = 0L;").AppendLine();
					sb.Append("\t\t\t\t\trequest.Properties.Add(\"Mockolate:HttpContent\", bytes);").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\t#endif").AppendLine();
				}

				sb.Append("\t\t\t\tvar ").Append(baseResultVarName).Append(" = base.").Append(method.Name).Append('(')
					.Append(FormatMethodParametersWithRefKind(method.Parameters))
					.Append(");").AppendLine();
				AppendConditionalOutRefParameterHandling(sb, "\t\t\t\t", method.Parameters, methodExecutionVarName,
					"this.Registrations.Behavior.DefaultValue");

				sb.Append("\t\t\t\tif (!").Append(methodExecutionVarName).Append(".HasSetupResult)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				AppendTriggerCallbacks(sb, "\t\t\t\t\t", methodExecutionVarName, method.Parameters);
				sb.Append("\t\t\t\t\treturn ").Append(baseResultVarName).Append(";").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
				sb.Append("\t\t\t}").AppendLine();
				if (method.Parameters.Any(p => p.RefKind == RefKind.Ref || p.RefKind == RefKind.Out))
				{
					sb.Append("\t\t\telse").AppendLine();
					sb.Append("\t\t\t{").AppendLine();
					AppendOutRefParameterHandling(sb, "\t\t\t\t", method.Parameters, methodExecutionVarName,
						"this.Registrations.Behavior.DefaultValue");
					sb.Append("\t\t\t}").AppendLine();
				}

				sb.AppendLine();
				AppendTriggerCallbacks(sb, "\t\t\t", methodExecutionVarName, method.Parameters);
				sb.Append("\t\t\treturn ").Append(methodExecutionVarName).Append(".Result;").AppendLine();
			}
			else
			{
				sb.Append("\t\t\t\tbase.").Append(method.Name).Append('(')
					.Append(FormatMethodParametersWithRefKind(method.Parameters)).Append(");").AppendLine();
				sb.Append("\t\t\t}").AppendLine();
				foreach (MethodParameter parameter in method.Parameters)
				{
					if (parameter.RefKind == RefKind.Out)
					{
						sb.AppendLine();
						sb.Append("\t\t\t").Append(parameter.Name).Append(" = ").Append(methodExecutionVarName)
							.Append(".SetOutParameter<")
							.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name)
							.Append("\", () => ")
							.AppendDefaultValueGeneratorFor(parameter.Type, "this.Registrations.Behavior.DefaultValue")
							.Append(");").AppendLine();
					}
					else if (parameter.RefKind == RefKind.Ref)
					{
						sb.AppendLine();
						sb.Append("\t\t\t").Append(parameter.Name).Append(" = ").Append(methodExecutionVarName)
							.Append(".SetRefParameter<")
							.Append(parameter.Type.Fullname).Append(">(\"").Append(parameter.Name).Append("\", ")
							.Append(parameter.Name).Append(");").AppendLine();
					}

					AppendTriggerCallbacks(sb, "\t\t\t", methodExecutionVarName, method.Parameters);
				}
			}
		}

		sb.AppendLine("\t\t}");
	}

	#endregion Mock Helpers

	#region Setup Helpers

	private static void DefineSetupInterface(StringBuilder sb, Class @class)
	{
		#region Properties

		Func<Property, bool> propertyPredicate = property
			=> property.ExplicitImplementation is null && property is { IsIndexer: false, };
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.AppendXmlSummary($"Setup for the {property.Type.Fullname.EscapeForXmlDoc()} property <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{property.Name.EscapeForXmlDoc()}\" />.");
			sb.Append("\t\tglobal::Mockolate.Setup.PropertySetup<").Append(property.Type.Fullname).Append("> ").Append(property.Name).Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate =
			indexer => indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, };
		foreach (Property indexer in @class.AllProperties().Where(indexerPredicate))
		{
			sb.AppendXmlSummary(
				$"Setup for the {indexer.Type.Fullname.EscapeForXmlDoc()} indexer <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.this[{string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.RefKind.GetString() + p.Type.Fullname.EscapeForXmlDoc()))}]\" />.");
			sb.Append("\t\tglobal::Mockolate.Setup.IndexerSetup<").AppendTypeOrWrapper(indexer.Type);
			foreach (MethodParameter parameter in indexer.IndexerParameters!)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append("> this[").Append(string.Join(", ",
					indexer.IndexerParameters.Value.Select((p, i)
						=> p.ToParameter() + (p.CanBeNullable() ? "?" : "") + $" parameter{i + 1}")))
				.Append("] { get; }")
				.AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Methods

		Func<Method, bool> methodPredicate = method => method.ExplicitImplementation is null;
		foreach (Method method in @class.AllMethods().Where(methodPredicate))
		{
			AppendMethodSetupDefinition(sb, @class, method, false);
		}

		foreach (Method method in @class.AllMethods()
			         .Where(methodPredicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Parameters.Count > 0))
		{
			AppendMethodSetupDefinition(sb, @class, method, true);
		}

		#endregion
	}

	private static void AppendMethodSetupDefinition(StringBuilder sb, Class @class, Method method,
		bool useParameters, string? methodNameOverride = null)
	{
		string methodName = methodNameOverride ?? method.Name;
		sb.Append("\t\t/// <summary>").AppendLine();
		if (methodNameOverride is null)
		{
			sb.Append("\t\t///     Setup for the method <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc()).Append(".")
				.Append(method.Name.EscapeForXmlDoc()).Append("(")
				.Append(string.Join(", ",
					method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname.EscapeForXmlDoc())))
				.Append(")\"/>");
		}
		else
		{
			sb.Append("\t\t///     Setup for the delegate <see cref=\"")
				.Append(@class.ClassFullName.EscapeForXmlDoc())
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
			sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<")
				.AppendTypeOrWrapper(method.ReturnType);
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append("> ");
			sb.Append(methodName).Append("(");
		}
		else
		{
			sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup");
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
			sb.Append("global::Mockolate.Parameters.IParameters parameters)");
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
				if (parameter.CanBeNullable())
				{
					sb.Append('?');
				}

				sb.Append(' ').Append(parameter.Name);
				if (parameter.HasExplicitDefaultValue)
				{
					sb.Append(" = null");
				}
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

		sb.Append(";").AppendLine();
		sb.AppendLine();
	}

	private static void ImplementSetupInterface(StringBuilder sb, Class @class, string name)
	{
		#region Properties

		Func<Property, bool> propertyPredicate = property
			=> property.ExplicitImplementation is null && property is { IsIndexer: false, };
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\tglobal::Mockolate.Setup.PropertySetup<").Append(property.Type.Fullname)
				.Append("> global::Mockolate.Mock.IMockSetupFor").Append(name).Append('.').Append(property.Name).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tvar propertySetup = new global::Mockolate.Setup.PropertySetup<")
				.Append(property.Type.Fullname).Append(">(").Append(property.GetUniqueNameString()).Append(");")
				.AppendLine();
			sb.Append("\t\t\t\tthis.Registrations.SetupProperty(propertySetup);").AppendLine();
			sb.Append("\t\t\t\treturn propertySetup;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate =
			indexer => indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, };
		foreach (Property indexer in @class.AllProperties().Where(indexerPredicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\tglobal::Mockolate.Setup.IndexerSetup<").AppendTypeOrWrapper(indexer.Type);

			foreach (MethodParameter parameter in indexer.IndexerParameters!)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append("> global::Mockolate.Mock.IMockSetupFor").Append(name).Append(".this[").Append(string.Join(", ",
					indexer.IndexerParameters.Value.Select((p, i)
						=> p.ToParameter() + (p.CanBeNullable() ? "?" : "") + $" parameter{i + 1}"))).Append("]")
				.AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tvar indexerSetup = new global::Mockolate.Setup.IndexerSetup<")
				.AppendTypeOrWrapper(indexer.Type);

			foreach (MethodParameter parameter in indexer.IndexerParameters!)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append(">(").Append(string.Join(", ",
					indexer.IndexerParameters.Value.Select((p, i)
						=> $"new global::Mockolate.Parameters.NamedParameter(\"{p.Name}\", (global::Mockolate.Parameters.IParameter)(parameter{i + 1}{
							(p.CanBeNullable() ? $" ?? global::Mockolate.It.IsNull<{p.ToNullableType()}>()" : "")}))")))
				.Append(");").AppendLine();
			sb.Append("\t\t\t\tthis.Registrations.SetupIndexer(indexerSetup);").AppendLine();
			sb.Append("\t\t\t\treturn indexerSetup;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Methods

		Func<Method, bool> methodPredicate = method => method.ExplicitImplementation is null;
		foreach (Method method in @class.AllMethods().Where(methodPredicate))
		{
			AppendMethodSetupImplementation(sb, method, name, false);
		}

		foreach (Method method in @class.AllMethods()
			         .Where(methodPredicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Parameters.Count > 0))
		{
			AppendMethodSetupImplementation(sb, method, name, true);
		}

		#endregion
	}

	private static void AppendMethodSetupImplementation(StringBuilder sb, Method method, string name,
		bool useParameters, string? methodNameOverride = null)
	{
		string methodName = methodNameOverride ?? method.Name;
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		if (method.ReturnType != Type.Void)
		{
			sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<").AppendTypeOrWrapper(method.ReturnType);
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append("> global::Mockolate.Mock.IMockSetupFor").Append(name).Append('.');
			sb.Append(methodName).Append("(");
		}
		else
		{
			sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup");
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

			sb.Append(" global::Mockolate.Mock.IMockSetupFor").Append(name).Append('.').Append(methodName).Append("(");
		}

		if (useParameters)
		{
			sb.Append("global::Mockolate.Parameters.IParameters parameters)");
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
				if (parameter.CanBeNullable())
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
			sb.Append("\t\t\tvar methodSetup = new global::Mockolate.Setup.ReturnMethodSetup<")
				.AppendTypeOrWrapper(method.ReturnType);

			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append(">");
		}
		else
		{
			sb.Append("\t\t\tvar methodSetup = new global::Mockolate.Setup.VoidMethodSetup");

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
			sb.Append("(").Append(method.GetUniqueNameString()).Append(", parameters);")
				.AppendLine();
			sb.Append("\t\t\tthis.Registrations.SetupMethod(methodSetup);").AppendLine();
			sb.Append("\t\t\treturn methodSetup;").AppendLine();
		}
		else
		{
			sb.Append("(").Append(method.GetUniqueNameString());
			foreach (MethodParameter parameter in method.Parameters)
			{
				sb.Append(", ");
				AppendNamedParameter(sb, parameter);
			}

			sb.Append(");").AppendLine();
			sb.Append("\t\t\tthis.Registrations.SetupMethod(methodSetup);").AppendLine();
			sb.Append("\t\t\treturn methodSetup;").AppendLine();
		}

		sb.AppendLine("\t\t}");
		sb.AppendLine();
	}

	#endregion Setup Helpers

	#region Raise Helpers

	private static void DefineRaiseInterface(StringBuilder sb, Class @class)
	{
		Func<Event, bool> predicate = @event => @event.ExplicitImplementation is null;
		foreach (Event @event in @class.AllEvents().Where(predicate))
		{
			sb.AppendXmlSummary($"Raise the <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{@event.Name.EscapeForXmlDoc()}\"/> event.");
			sb.Append("\t\tvoid ").Append(@event.Name).Append("(").Append(FormatParametersWithTypeAndName(@event.Delegate.Parameters)).Append(");").AppendLine();
			sb.AppendLine();
		}

		foreach (Event @event in @class.AllEvents()
			         .Where(predicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Delegate.Parameters.Count > 0))
		{
			sb.AppendXmlSummary($"Raise the <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{@event.Name.EscapeForXmlDoc()}\"/> event.");
			sb.Append("\t\tvoid ").Append(@event.Name).Append("(global::Mockolate.Parameters.IDefaultEventParameters parameters);").AppendLine();
			sb.AppendLine();
		}
	}

	private static void ImplementRaiseInterface(StringBuilder sb, Class @class, string name)
	{
		Func<Event, bool> predicate = @event => @event.ExplicitImplementation is null;
		foreach (Event @event in @class.AllEvents().Where(predicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\tvoid IMockRaiseOn").Append(name).Append('.').Append(@event.Name).Append("(")
				.Append(FormatParametersWithTypeAndName(@event.Delegate.Parameters))
				.Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tthis.Registrations.Raise(").Append(@event.GetUniqueNameString())
				.Append(", ")
				.Append(FormatParametersAsNames(@event.Delegate.Parameters)).Append(");").AppendLine();
			sb.AppendLine("\t\t}");
			sb.AppendLine();
		}

		foreach (Event @event in @class.AllEvents()
			         .Where(predicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Delegate.Parameters.Count > 0))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\tvoid IMockRaiseOn").Append(name).Append('.').Append(@event.Name).Append("(global::Mockolate.Parameters.IDefaultEventParameters parameters)")
				.AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tglobal::Mockolate.MockBehavior mockBehavior = this.Registrations.Behavior;").AppendLine();
			sb.Append("\t\t\tthis.Registrations.Raise(").Append(@event.GetUniqueNameString()).Append(", ")
				.Append(string.Join(", ",
					@event.Delegate.Parameters.Select(p
						=> $"mockBehavior.DefaultValue.Generate(default({p.Type.Fullname.TrimEnd('?')}))")))
				.Append(");").AppendLine();
			sb.AppendLine("\t\t}");
			sb.AppendLine();
		}
	}

	#endregion Raise Helpers

	#region Verify Helpers

	private static void DefineVerifyInterface(StringBuilder sb, Class @class, string name)
	{
		#region Properties

		Func<Property, bool> propertyPredicate = property
			=> property.ExplicitImplementation is null && property is { IsIndexer: false, };
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.AppendXmlSummary($"Verify interactions with the {property.Type.Fullname.EscapeForXmlDoc()} property <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{property.Name.EscapeForXmlDoc()}\" />.");
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationPropertyResult<IMockVerifyFor").Append(name).Append(", ").Append(property.Type.Fullname).Append("> ").Append(property.Name).Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate =
			indexer => indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, };
		foreach (Property indexer in @class.AllProperties().Where(indexerPredicate))
		{
			sb.AppendXmlSummary(
				$"Verify interactions with the {indexer.Type.Fullname.EscapeForXmlDoc()} indexer <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.this[{string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.RefKind.GetString() + p.Type.Fullname.EscapeForXmlDoc()))}]\" />.");
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationIndexerResult<IMockVerifyFor").Append(name).Append(", ").AppendTypeOrWrapper(indexer.Type).Append("> this[")
				.Append(string.Join(", ", indexer.IndexerParameters.Value.Select(p => p.ToParameter() + (p.CanBeNullable() ? "? " : " ") + p.Name)))
				.Append("] { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Methods

		Func<Method, bool> methodPredicate = method => method.ExplicitImplementation is null;
		foreach (Method method in @class.AllMethods().Where(methodPredicate))
		{
			AppendMethodVerifyDefinition(sb, @class, method, name, false);
		}

		foreach (Method method in @class.AllMethods()
			         .Where(methodPredicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Parameters.Count > 0))
		{
			AppendMethodVerifyDefinition(sb, @class, method, name, true);
		}

		#endregion

		#region Events

		Func<Event, bool> eventPredicate =
			indexer => indexer.ExplicitImplementation is null;
		foreach (Event @event in @class.AllEvents().Where(eventPredicate))
		{
			sb.AppendXmlSummary(
				$"Verify subscription on the {@event.Name} event <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{@event.Name}\" />.");
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationEventResult<IMockVerifyFor").Append(name).Append("> ").Append(@event.Name)
				.Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion
	}

	private static void AppendMethodVerifyDefinition(StringBuilder sb, Class @class, Method method, string name,
		bool useParameters, string? methodNameOverride = null)
	{
		string methodName = methodNameOverride ?? method.Name;
		sb.Append("\t\t/// <summary>").AppendLine();
		sb.Append("\t\t///     Validates the invocations for the method <see cref=\"")
			.Append(@class.ClassFullName.EscapeForXmlDoc())
			.Append(".").Append(methodName.EscapeForXmlDoc()).Append("(");
		sb.Append(string.Join(", ",
			method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname.EscapeForXmlDoc())));
		sb.Append(")\"/>").Append(method.Parameters.Count > 0 ? " with the given " : "");
		if (useParameters)
		{
			sb.Append("<paramref name=\"parameters\"/>");
		}
		else
		{
			sb.Append(string.Join(", ", method.Parameters.Select(p => $"<paramref name=\"{p.Name}\"/>")));
		}

		sb.Append(".").AppendLine();
		sb.Append("\t\t/// </summary>").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<IMockVerifyFor").Append(name).Append("> ").Append(methodName)
			.Append("(");
		if (useParameters)
		{
			sb.Append("global::Mockolate.Parameters.IParameters parameters");
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

				sb.AppendVerifyParameter(parameter);
				if (parameter.CanBeNullable())
				{
					sb.Append('?');
				}

				sb.Append(' ').Append(parameter.Name);
				if (parameter.HasExplicitDefaultValue)
				{
					sb.Append(" = null");
				}
			}
		}

		sb.Append(")");
		if (method.GenericParameters is not null && method.GenericParameters.Value.Count > 0)
		{
			foreach (GenericParameter gp in method.GenericParameters.Value)
			{
				gp.AppendWhereConstraint(sb, "\t\t\t");
			}
		}

		sb.Append(";").AppendLine();
		sb.AppendLine();
	}

	private static void ImplementVerifyInterface(StringBuilder sb, Class @class, string name)
	{
		#region Properties

		Func<Property, bool> propertyPredicate = property
			=> property.ExplicitImplementation is null && property is { IsIndexer: false, };
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationPropertyResult<IMockVerifyFor").Append(name).Append(", ").Append(property.Type.Fullname).Append("> IMockVerifyFor").Append(name).Append('.').Append(property.Name).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn new global::Mockolate.Verify.VerificationPropertyResult<IMockVerifyFor").Append(name).Append(", ").Append(property.Type.Fullname).Append(">(this, this.Registrations, ").Append(property.GetUniqueNameString()).Append(");").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate =
			indexer => indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, IndexerParameters: not null, };
		foreach (Property indexer in @class.AllProperties().Where(indexerPredicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationIndexerResult<IMockVerifyFor").Append(name).Append(", ").AppendTypeOrWrapper(indexer.Type).Append("> IMockVerifyFor").Append(name).Append(".this[").Append(string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.ToParameter() + (p.CanBeNullable() ? "? " : " ") + p.Name))).Append("]").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn new global::Mockolate.Verify.VerificationIndexerResult<IMockVerifyFor").Append(name).Append(", ").AppendTypeOrWrapper(indexer.Type).Append(">(this, this.Registrations, [ ");
			foreach (MethodParameter parameter in indexer.IndexerParameters)
			{
				AppendNamedParameter(sb, parameter);
				sb.Append(", ");
			}

			sb.Append("]);").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Methods

		Func<Method, bool> methodPredicate = method => method.ExplicitImplementation is null;
		foreach (Method method in @class.AllMethods().Where(methodPredicate))
		{
			AppendMethodVerifyImplementation(sb, method, name, false);
		}

		foreach (Method method in @class.AllMethods()
			         .Where(methodPredicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Parameters.Count > 0))
		{
			AppendMethodVerifyImplementation(sb, method, name, true);
		}

		#endregion

		#region Events

		Func<Event, bool> eventPredicate =
			indexer => indexer.ExplicitImplementation is null;
		foreach (Event @event in @class.AllEvents().Where(eventPredicate))
		{
			sb.AppendXmlSummary(
				$"Verify subscription on the {@event.Name} event <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{@event.Name}\" />.");
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationEventResult<IMockVerifyFor").Append(name).Append("> IMockVerifyFor").Append(name).Append('.').Append(@event.Name).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn new global::Mockolate.Verify.VerificationEventResult<IMockVerifyFor").Append(name).Append(">(this, this.Registrations, ").Append(@event.GetUniqueNameString()).Append(");").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion
	}

	private static void AppendMethodVerifyImplementation(StringBuilder sb, Method method, string name,
		bool useParameters, string? methodNameOverride = null)
	{
		string methodName = methodNameOverride ?? method.Name;
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<IMockVerifyFor").Append(name).Append("> IMockVerifyFor").Append(name).Append('.').Append(methodName).Append("(");
		if (useParameters)
		{
			sb.Append("global::Mockolate.Parameters.IParameters parameters");
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

				sb.AppendVerifyParameter(parameter);
				if (parameter.CanBeNullable())
				{
					sb.Append('?');
				}

				sb.Append(' ').Append(parameter.Name);
			}
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

		sb.Append("\t\t\t=> this.Registrations.Method<IMockVerifyFor").Append(name).Append(">(this, ");

		if (useParameters)
		{
			sb.Append("new global::Mockolate.Setup.MethodParametersMatch(").Append(method.GetUniqueNameString()).Append(", parameters");
		}
		else
		{
			sb.Append("new global::Mockolate.Setup.MethodParameterMatch(").Append(method.GetUniqueNameString()).Append(", [ ");
			foreach (MethodParameter parameter in method.Parameters)
			{
				AppendNamedParameter(sb, parameter);
				sb.Append(", ");
			}

			sb.Append(']');
		}

		sb.AppendLine("));");
		sb.AppendLine();
	}

	#endregion Verify Helpers
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
