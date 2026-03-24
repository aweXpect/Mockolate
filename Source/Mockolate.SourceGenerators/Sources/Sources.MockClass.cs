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
		bool hasEvents = @class.AllEvents().Any(x => !x.IsStatic);
		bool hasStaticEvents = @class.IsInterface &&
		                       @class.AllEvents().Any(@event => @event.IsStatic);
		bool hasStaticMembers = @class.IsInterface &&
		                        (@class.AllMethods().Any(method => method.IsStatic) ||
		                         @class.AllProperties().Any(property => property.IsStatic));
		bool hasProtectedEvents = !@class.IsInterface && @class.AllEvents().Any(@event => @event.IsProtected);
		bool hasProtectedMembers = !@class.IsInterface &&
		                           (@class.AllMethods().Any(method => method.IsProtected)
		                            || @class.AllProperties().Any(property => property.IsProtected));
		string setupType = hasProtectedMembers
			? $"global::System.Action<IMockSetupInitializationFor{name}>"
			: $"global::System.Action<global::Mockolate.Mock.IMockSetupFor{name}>";
		string mockRegistryName = @class.GetUniqueName("MockRegistry", "MockolateMockRegistry");
		StringBuilder sb = InitializeBuilder();

		sb.Append("#nullable enable annotations").AppendLine();
		sb.Append("namespace Mockolate;").AppendLine();
		sb.AppendLine();

		#region MockForXXXExtensions

		sb.AppendXmlSummary($"Mock extensions for <see cref=\"{escapedClassName}\" />.", "");
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
		sb.Append("internal static partial class MockExtensionsFor").Append(name).AppendLine();
		sb.Append("{").AppendLine();

		#region Type extensions

		sb.Append("\t/// <inheritdoc cref=\"MockExtensionsFor").Append(name).Append("\" />").AppendLine();
		sb.Append("\textension(").Append(@class.ClassFullName).Append(" mock)").AppendLine();
		sb.Append("\t{").AppendLine();

		#region Mock Property

		string mockPropertyName = CreateUniquePropertyName(@class, "Mock");

		sb.AppendXmlSummary($"Get access to the mock of <see cref=\"{escapedClassName}\" />.");
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

		sb.AppendXmlSummary($"Create a new mock of <see cref=\"{escapedClassName}\" /> with the default <see cref=\"global::Mockolate.MockBehavior\" />.");
		sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock()").AppendLine();
		sb.Append("\t\t\t=> CreateMock(null, null, null);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Create a new mock of <see cref=\"{escapedClassName}\" /> with the default <see cref=\"global::Mockolate.MockBehavior\" />.");
		sb.AppendXmlRemarks("The provided <paramref name=\"setup\" /> is immediately applied to the mock.");
		sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock(").Append(setupType).Append(" setup)").AppendLine();
		sb.Append("\t\t\t=> CreateMock(null, null, setup);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Create a new mock of <see cref=\"{escapedClassName}\" /> with the given <paramref name=\"mockBehavior\" />.");
		sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock(global::Mockolate.MockBehavior mockBehavior)").AppendLine();
		sb.Append("\t\t\t=> CreateMock(null, mockBehavior, null);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary($"Create a new mock of <see cref=\"{escapedClassName}\" /> with the given <paramref name=\"mockBehavior\" />.");
		sb.AppendXmlRemarks("The provided <paramref name=\"setup\" /> is immediately applied to the mock.");
		sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
		sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock(global::Mockolate.MockBehavior mockBehavior, ").Append(setupType).Append(" setup)").AppendLine();
		sb.Append("\t\t\t=> CreateMock(null, mockBehavior, setup);").AppendLine();
		sb.AppendLine();

		if (!@class.IsInterface)
		{
			sb.AppendXmlSummary($"Create a new mock of <see cref=\"{escapedClassName}\" /> using the <paramref name=\"constructorParameters\" />.");
			sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
			sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock(object?[] constructorParameters)").AppendLine();
			sb.Append("\t\t\t=> CreateMock(constructorParameters, null, null);").AppendLine();
			sb.AppendLine();

			sb.AppendXmlSummary($"Create a new mock of <see cref=\"{escapedClassName}\" /> using the <paramref name=\"constructorParameters\" /> with the given <paramref name=\"mockBehavior\" />.");
			sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
			sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock(object?[] constructorParameters, global::Mockolate.MockBehavior mockBehavior)").AppendLine();
			sb.Append("\t\t\t=> CreateMock(constructorParameters, mockBehavior, null);").AppendLine();
			sb.AppendLine();

			sb.AppendXmlSummary($"Create a new mock of <see cref=\"{escapedClassName}\" /> using the <paramref name=\"constructorParameters\" />.");
			sb.AppendXmlRemarks("The provided <paramref name=\"setup\" /> is immediately applied to the mock.");
			sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
			sb.Append("\t\tpublic static ").Append(@class.ClassFullName).Append(" CreateMock(object?[] constructorParameters, ").Append(setupType).Append(" setup)").AppendLine();
			sb.Append("\t\t\t=> CreateMock(constructorParameters, null, setup);").AppendLine();
			sb.AppendLine();
		}

		sb.AppendXmlSummary($"Create a new mock of <see cref=\"{escapedClassName}\" /> using the <paramref name=\"constructorParameters\" /> with the given <paramref name=\"mockBehavior\" />.");
		sb.AppendXmlRemarks("The provided <paramref name=\"setup\" /> is immediately applied to the mock.");
		sb.Append("\t\t[global::Mockolate.MockGenerator]").AppendLine();
		sb.Append("\t\t").Append(@class.IsInterface ? "private" : "public").Append(" static ").Append(@class.ClassFullName).Append(" CreateMock(object?[]? constructorParameters, global::Mockolate.MockBehavior? mockBehavior, ").Append(setupType).Append("? setup)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (mockBehavior is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tIMockBehaviorAccess mockBehaviorAccess = (global::Mockolate.IMockBehaviorAccess)mockBehavior;").AppendLine();
		sb.Append("\t\t\t\tif (mockBehaviorAccess.TryGet<").Append(setupType).Append("?>(out var additionalSetup))").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tif (setup is null)").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\tsetup = additionalSetup;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t\telse").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\tvar originalSetup = setup;").AppendLine();
		sb.Append("\t\t\t\t\t\tsetup = s => { additionalSetup.Invoke(s); originalSetup.Invoke(s); };").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		if (!@class.IsInterface && !hasStaticMembers)
		{
			sb.Append("\t\t\t\tif (constructorParameters is null && mockBehaviorAccess.TryGetConstructorParameters<").Append(@class.ClassFullName).Append(">(out object?[]? parameters))").AppendLine();
			sb.Append("\t\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t\tconstructorParameters = parameters;").AppendLine();
			sb.Append("\t\t\t\t}").AppendLine();
		}

		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\telse").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tmockBehavior = global::Mockolate.MockBehavior.Default;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t\tglobal::Mockolate.MockRegistry mockRegistry = new global::Mockolate.MockRegistry(mockBehavior);").AppendLine();
		if (@class is { ClassFullName: "global::System.Net.Http.HttpClient", })
		{
			sb.Append("\t\t\tif (constructorParameters is null)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tconstructorParameters = [new global::Mockolate.Mock.HttpMessageHandler(mockRegistry),];").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
		}

		sb.AppendLine();

		if (!@class.IsInterface && constructors?.Count > 0)
		{
			sb.Append("\t\t\tif (constructorParameters is null || constructorParameters.Length == 0)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			if (constructors.Value.Any(m => m.Parameters.Count == 0))
			{
				sb.Append("\t\t\t\tglobal::Mockolate.Mock.").Append(name).Append(".MockRegistryProvider.Value = mockRegistry;").AppendLine();
				sb.Append("\t\t\t\tglobal::Mockolate.MockExtensionsFor").Append(name).Append(".MockSetup? setupTarget = null;").AppendLine();
				sb.Append("\t\t\t\tif (setup is not null)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\tsetupTarget ??= new(mockRegistry);").AppendLine();
				sb.Append("\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
				sb.Append("\t\t\t\treturn new global::Mockolate.Mock.").Append(name).Append("(mockRegistry);").AppendLine();
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
				sb.Append("\t\t\t\tglobal::Mockolate.Mock.").Append(name).Append(".MockRegistryProvider.Value = mockRegistry;").AppendLine();
				sb.Append("\t\t\t\tglobal::Mockolate.MockExtensionsFor").Append(name).Append(".MockSetup? setupTarget = null;").AppendLine();
				sb.Append("\t\t\t\tif (setup is not null)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\tsetupTarget ??= new(mockRegistry);").AppendLine();
				sb.Append("\t\t\t\t\tsetup.Invoke(setupTarget);").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
				sb.Append("\t\t\t\treturn new global::Mockolate.Mock.").Append(name)
					.Append("(mockRegistry");
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
			sb.Append("\t\t\tvar value = new global::Mockolate.Mock.").Append(name).Append("(mockRegistry);").AppendLine();
			sb.Append("\t\t\tif (setup is not null)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tsetup.Invoke(value);").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\treturn value;").AppendLine();
		}

		sb.Append("\t\t}").AppendLine();

		#endregion CreateMock

		if (@class.IsInterface)
		{
			sb.AppendXmlSummary("Create a mock that wraps the given <paramref name=\"instance\" />.");
			sb.AppendXmlRemarks("All interactions are forwarded to the <paramref name=\"instance\" />.");
			sb.Append("\t\tpublic ").Append(@class.ClassFullName).Append(" Wrapping(").Append(@class.ClassFullName).Append(" instance)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tif (mock is global::Mockolate.IMock mockInterface)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn new global::Mockolate.Mock.").Append(name).Append("(mockInterface.MockRegistry, instance);").AppendLine();
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

		sb.AppendXmlSummary("Initialize mocks of type <typeparamref name=\"T\" /> with the given <paramref name=\"setup\" />.");
		sb.AppendXmlRemarks("The <paramref name=\"setup\" /> is applied to the mock before the constructor is executed.");
		sb.Append("\t\tpublic global::Mockolate.MockBehavior Initialize<T>(").Append(setupType).Append(" setup)").AppendLine();
		sb.Append("\t\t\twhere T : ").Append(@class.ClassFullName).AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar behaviorAccess = (global::Mockolate.IMockBehaviorAccess)behavior;").AppendLine();
		sb.Append("\t\t\treturn behaviorAccess.Set(setup);").AppendLine();
		sb.Append("\t\t}").AppendLine();

		sb.Append("\t}").AppendLine();

		#endregion MockBehavior extensions

		#region Setup helpers

		if (!@class.IsInterface && constructors?.Count > 0)
		{
			string protectedName = @class.GetUniqueName("Protected", "SetupProtected");
			if (hasProtectedMembers)
			{
				sb.Append("\tinternal interface IMockSetupInitializationFor").Append(name).Append(" : global::Mockolate.Mock.IMockSetupFor").Append(name).AppendLine();
				sb.Append("\t{").AppendLine();
				sb.AppendXmlSummary("Setup protected members");
				sb.Append("\t\tglobal::Mockolate.Mock.IMockProtectedSetupFor").Append(name).Append(' ').Append(protectedName).Append(" { get; }").AppendLine();
				sb.Append("\t}").AppendLine();
			}

			sb.AppendLine();
			sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
			sb.Append("\tinternal sealed class MockSetup(global::Mockolate.MockRegistry mockRegistry) : global::Mockolate.Mock.IMockSetupFor").Append(name);
			if (hasProtectedMembers)
			{
				sb.Append(", global::Mockolate.Mock.IMockProtectedSetupFor").Append(name).Append(", IMockSetupInitializationFor").Append(name);
			}
			sb.AppendLine();
			sb.Append("\t{").AppendLine();
			if (hasProtectedMembers)
			{
				sb.Append("\t\t/// <inheritdoc />").AppendLine();
				sb.Append("\t\tglobal::Mockolate.Mock.IMockProtectedSetupFor").Append(name).Append(" IMockSetupInitializationFor").Append(name).Append('.').Append(protectedName).Append(" => this;").AppendLine();
			}
			sb.Append("\t\tprivate global::Mockolate.MockRegistry ").Append(mockRegistryName).Append(" { get; } = mockRegistry;").AppendLine();
			sb.AppendLine();
			sb.Append("\t\t#region IMockSetupFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockSetupFor{name}", MemberType.Public);
			sb.Append("\t\t#endregion IMockSetupFor").Append(name).AppendLine();
			if (hasProtectedMembers)
			{
				sb.AppendLine();
				sb.Append("\t\t#region IMockProtectedSetupFor").Append(name).AppendLine();
				sb.AppendLine();
				ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockProtectedSetupFor{name}", MemberType.Protected);
				sb.Append("\t\t#endregion IMockProtectedSetupFor").Append(name).AppendLine();
			}
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
		sb.Append("\t[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]").AppendLine();
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
		sb.Append("\tinternal class ").Append(name).Append(" :").AppendLine();
		sb.Append("\t\t").Append(@class.ClassFullName);
		sb.Append(", IMockFor").Append(name).Append(", IMockSetupFor").Append(name);
		if (hasProtectedMembers)
		{
			sb.Append(", IMockProtectedSetupFor").Append(name);
		}

		if (hasStaticMembers)
		{
			sb.Append(", IMockStaticSetupFor").Append(name);
		}

		if (hasEvents)
		{
			sb.Append(", IMockRaiseOn").Append(name);
		}

		if (hasProtectedEvents)
		{
			sb.Append(", IMockProtectedRaiseOn").Append(name);
		}

		if (hasStaticEvents)
		{
			sb.Append(", IMockStaticRaiseOn").Append(name);
		}

		sb.Append(", IMockVerifyFor").Append(name);

		if (hasProtectedMembers || hasProtectedEvents)
		{
			sb.Append(", IMockProtectedVerifyFor").Append(name);
		}

		if (hasStaticMembers || hasStaticEvents)
		{
			sb.Append(", IMockStaticVerifyFor").Append(name);
		}

		sb.Append(',').AppendLine();

		sb.Append("\t\tglobal::Mockolate.IMock").AppendLine();
		sb.Append("\t{").AppendLine();

		if (@class.IsInterface)
		{
			sb.AppendXmlSummary("The wrapped instance (if any) of the mocked interface to call the base implementation on.");
			sb.Append("\t\tprivate ").Append(@class.ClassFullName).Append("? Wraps { get; }").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
		sb.Append("\t\tobject?[] global::Mockolate.IMock.ConstructorParameters => this.ConstructorParameters;").AppendLine();
		sb.Append("\t\tprivate object?[] ConstructorParameters { get; }").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
		sb.Append("\t\tglobal::Mockolate.MockRegistry global::Mockolate.IMock.MockRegistry => this.").Append(mockRegistryName).Append(';').AppendLine();
		if (constructors?.Count > 0)
		{
			sb.Append("\t\tprivate global::Mockolate.MockRegistry ").Append(mockRegistryName).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget => field ?? MockRegistryProvider.Value;").AppendLine();
			sb.Append("\t\t\tset;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tinternal static readonly global::System.Threading.AsyncLocal<global::Mockolate.MockRegistry> MockRegistryProvider = new global::System.Threading.AsyncLocal<global::Mockolate.MockRegistry>();").AppendLine();
		}
		else
		{
			sb.Append("\t\tprivate global::Mockolate.MockRegistry ").Append(mockRegistryName).Append(" { get; }").AppendLine();
			if (hasStaticMembers)
			{
				sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
				sb.Append("\t\tinternal static readonly global::System.Threading.AsyncLocal<global::Mockolate.MockRegistry> MockRegistryProvider = new global::System.Threading.AsyncLocal<global::Mockolate.MockRegistry>();").AppendLine();
			}
		}

		sb.AppendLine();

		ImplementMockForInterface(sb, mockRegistryName, name, hasEvents, hasProtectedMembers, hasProtectedEvents, hasStaticMembers, hasStaticEvents);

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tstring global::Mockolate.IMock.ToString()").AppendLine();
		sb.Append("\t\t\t=> \"").Append(@class.DisplayString).Append(" mock\";").AppendLine();
		sb.AppendLine();

		if (@class.IsInterface)
		{
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(name).Append("\" />").AppendLine();
			sb.Append("\t\tpublic ").Append(name).Append("(global::Mockolate.MockRegistry mockRegistry, ")
				.Append(@class.ClassFullName).Append("? wraps = null)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tthis.ConstructorParameters = [];").AppendLine();
			sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(" = mockRegistry;").AppendLine();
			if (hasStaticMembers)
			{
				sb.Append("\t\t\tMockRegistryProvider.Value = mockRegistry;").AppendLine();
			}

			sb.Append("\t\t\tthis.Wraps = wraps;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}
		else if (constructors is not null)
		{
			foreach (Method constructor in constructors)
			{
				AppendMockSubject_BaseClassConstructor(sb, mockRegistryName, name, constructor);
			}
		}
		else
		{
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(name).Append("\" />").AppendLine();
			sb.Append("\t\tpublic ").Append(name).Append("(global::Mockolate.MockRegistry mockRegistry)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tthis.ConstructorParameters = new object?[0];").AppendLine();
			sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(" = mockRegistry;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		AppendMockSubject_ImplementClass(sb, @class, mockRegistryName, null);
		sb.AppendLine();

		#region IMockSetupForXXX

		sb.Append("\t\t#region IMockSetupFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockSetupFor{name}", MemberType.Public);
		sb.Append("\t\t#endregion IMockSetupFor").Append(name).AppendLine();

		if (hasProtectedMembers)
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockProtectedSetupFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockProtectedSetupFor{name}", MemberType.Protected);
			sb.Append("\t\t#endregion IMockProtectedSetupFor").Append(name).AppendLine();
		}

		if (hasStaticMembers)
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockStaticSetupFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementSetupInterface(sb, @class, mockRegistryName, $"IMockStaticSetupFor{name}", MemberType.Static);
			sb.Append("\t\t#endregion IMockStaticSetupFor").Append(name).AppendLine();
		}

		#endregion IMockSetupForXXX

		if (hasEvents)
		{
			#region IMockRaiseOnXXX

			sb.AppendLine();
			sb.Append("\t\t#region IMockRaiseOn").Append(name).AppendLine();
			sb.AppendLine();
			ImplementRaiseInterface(sb, @class, mockRegistryName, $"IMockRaiseOn{name}", MemberType.Public);
			sb.Append("\t\t#endregion IMockRaiseOn").Append(name).AppendLine();

			#endregion IMockRaiseOnXXX
		}

		if (hasProtectedEvents)
		{
			#region IMockProtectedRaiseOnXXX

			sb.AppendLine();
			sb.Append("\t\t#region IMockProtectedRaiseOn").Append(name).AppendLine();
			sb.AppendLine();
			ImplementRaiseInterface(sb, @class, mockRegistryName, $"IMockProtectedRaiseOn{name}", MemberType.Protected);
			sb.Append("\t\t#endregion IMockProtectedRaiseOn").Append(name).AppendLine();

			#endregion IMockProtectedRaiseOnXXX
		}

		if (hasStaticEvents)
		{
			#region IMockStaticRaiseOnXXX

			sb.AppendLine();
			sb.Append("\t\t#region IMockStaticRaiseOn").Append(name).AppendLine();
			sb.AppendLine();
			ImplementRaiseInterface(sb, @class, mockRegistryName, $"IMockStaticRaiseOn{name}", MemberType.Static);
			sb.Append("\t\t#endregion IMockStaticRaiseOn").Append(name).AppendLine();

			#endregion IMockStaticRaiseOnXXX
		}

		#region IMockVerifyForXXX

		sb.AppendLine();
		sb.Append("\t\t#region IMockVerifyFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementVerifyInterface(sb, @class, mockRegistryName, $"IMockVerifyFor{name}", MemberType.Public);
		sb.Append("\t\t#endregion IMockVerifyFor").Append(name).AppendLine();

		if (hasProtectedMembers || hasProtectedEvents)
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockProtectedVerifyFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementVerifyInterface(sb, @class, mockRegistryName, $"IMockProtectedVerifyFor{name}", MemberType.Protected);
			sb.Append("\t\t#endregion IMockProtectedVerifyFor").Append(name).AppendLine();
		}

		if (hasStaticMembers || hasStaticEvents)
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockStaticVerifyFor").Append(name).AppendLine();
			sb.AppendLine();
			ImplementVerifyInterface(sb, @class, mockRegistryName, $"IMockStaticVerifyFor{name}", MemberType.Static);
			sb.Append("\t\t#endregion IMockStaticVerifyFor").Append(name).AppendLine();
		}

		#endregion IMockVerifyForXXX

		sb.AppendLine("\t}");

		sb.AppendLine();
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
		sb.Append("\tprivate sealed class VerifyMonitor").Append(name).Append("(global::Mockolate.MockRegistry mockRegistry) : global::Mockolate.Mock.IMockVerifyFor").Append(name).AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.MockRegistry ").Append(mockRegistryName).Append(" { get; } = mockRegistry;").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t#region IMockVerifyFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementVerifyInterface(sb, @class, mockRegistryName, $"IMockVerifyFor{name}", MemberType.Public);
		sb.Append("\t\t#endregion IMockVerifyFor").Append(name).AppendLine();
		sb.Append("\t}").AppendLine();

		#endregion MockForXXX

		sb.AppendLine();

		#region IMockForXXX

		sb.AppendXmlSummary($"Accesses the mock of <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal interface IMockFor").Append(name).AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary($"Set up the mock of <see cref=\"{escapedClassName}\" />.");
		sb.Append("\t\tIMockSetupFor").Append(name).Append(" Setup { get; }").AppendLine();
		sb.AppendLine();
		if (hasProtectedMembers)
		{
			sb.AppendXmlSummary($"Set up protected members on the mock of <see cref=\"{escapedClassName}\" />.");
			sb.Append("\t\tIMockProtectedSetupFor").Append(name).Append(" SetupProtected { get; }").AppendLine();
			sb.AppendLine();
		}

		if (hasStaticMembers)
		{
			sb.AppendXmlSummary($"Set up static members on the mock of <see cref=\"{escapedClassName}\" />.");
			sb.Append("\t\tIMockStaticSetupFor").Append(name).Append(" SetupStatic { get; }").AppendLine();
			sb.AppendLine();
		}

		if (hasEvents)
		{
			sb.AppendXmlSummary($"Raise events on the mock of <see cref=\"{escapedClassName}\" />.");
			sb.Append("\t\tIMockRaiseOn").Append(name).Append(" Raise { get; }").AppendLine();
			sb.AppendLine();
		}

		if (hasProtectedEvents)
		{
			sb.AppendXmlSummary($"Raise protected events on the mock of <see cref=\"{escapedClassName}\" />.");
			sb.Append("\t\tIMockProtectedRaiseOn").Append(name).Append(" RaiseProtected { get; }").AppendLine();
			sb.AppendLine();
		}

		if (hasStaticEvents)
		{
			sb.AppendXmlSummary($"Raise static events on the mock of <see cref=\"{escapedClassName}\" />.");
			sb.Append("\t\tIMockStaticRaiseOn").Append(name).Append(" RaiseStatic { get; }").AppendLine();
			sb.AppendLine();
		}

		sb.AppendXmlSummary($"Verify interactions with the mock of <see cref=\"{escapedClassName}\" />.");
		sb.Append("\t\tIMockVerifyFor").Append(name).Append(" Verify { get; }").AppendLine();
		sb.AppendLine();
		if (hasProtectedMembers || hasProtectedEvents)
		{
			sb.AppendXmlSummary($"Verify protected interactions with the mock of <see cref=\"{escapedClassName}\" />.");
			sb.Append("\t\tIMockProtectedVerifyFor").Append(name).Append(" VerifyProtected { get; }").AppendLine();
			sb.AppendLine();
		}

		if (hasStaticMembers || hasStaticEvents)
		{
			sb.AppendXmlSummary($"Verify static interactions with the mock of <see cref=\"{escapedClassName}\" />.");
			sb.Append("\t\tIMockStaticVerifyFor").Append(name).Append(" VerifyStatic { get; }").AppendLine();
			sb.AppendLine();
		}

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
		sb.Append("\tinternal interface IMockSetupFor").Append(name);
		if (!hasStaticMembers)
		{
			sb.Append(" : global::Mockolate.Setup.IMockSetup<").Append(@class.ClassFullName).Append(">").AppendLine();
		}
		else
		{
			sb.AppendLine();
		}

		sb.Append("\t{").AppendLine();
		DefineSetupInterface(sb, @class, MemberType.Public);
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		if (hasProtectedMembers)
		{
			sb.AppendXmlSummary($"Set up protected members for the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockProtectedSetupFor").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineSetupInterface(sb, @class, MemberType.Protected);
			sb.Append("\t}").AppendLine();
			sb.AppendLine();
		}

		if (hasStaticMembers)
		{
			sb.AppendXmlSummary($"Set up static members for the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockStaticSetupFor").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineSetupInterface(sb, @class, MemberType.Static);
			sb.Append("\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion IMockSetupForXXX

		if (hasEvents)
		{
			#region IMockRaiseOnXXX

			sb.AppendXmlSummary($"Raise events on the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockRaiseOn").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineRaiseInterface(sb, @class, MemberType.Public);
			sb.Append("\t}").AppendLine();
			sb.AppendLine();

			#endregion IMockRaiseOnXXX
		}

		if (hasProtectedEvents)
		{
			#region IMockProtectedRaiseOnXXX

			sb.AppendXmlSummary($"Raise protected events on the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockProtectedRaiseOn").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineRaiseInterface(sb, @class, MemberType.Protected);
			sb.Append("\t}").AppendLine();
			sb.AppendLine();

			#endregion IMockProtectedRaiseOnXXX
		}

		if (hasStaticEvents)
		{
			#region IMockStaticRaiseOnXXX

			sb.AppendXmlSummary($"Raise static events on the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockStaticRaiseOn").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineRaiseInterface(sb, @class, MemberType.Static);
			sb.Append("\t}").AppendLine();
			sb.AppendLine();

			#endregion IMockStaticRaiseOnXXX
		}

		#region IMockVerifyForXXX

		sb.AppendXmlSummary($"Verify interactions with the mock of <see cref=\"{escapedClassName}\" />.", "\t");
		sb.Append("\tinternal interface IMockVerifyFor").Append(name);
		if (!hasStaticMembers)
		{
			sb.Append(" : global::Mockolate.Verify.IMockVerify<").Append(@class.ClassFullName).Append(">").AppendLine();
		}
		else
		{
			sb.AppendLine();
		}

		sb.Append("\t{").AppendLine();
		DefineVerifyInterface(sb, @class, $"IMockVerifyFor{name}", MemberType.Public);
		sb.Append("\t}").AppendLine();

		if (hasProtectedMembers || hasProtectedEvents)
		{
			sb.AppendLine();
			sb.AppendXmlSummary($"Verify protected interactions with the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockProtectedVerifyFor").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineVerifyInterface(sb, @class, $"IMockProtectedVerifyFor{name}", MemberType.Protected);
			sb.Append("\t}").AppendLine();
		}

		if (hasStaticMembers || hasStaticEvents)
		{
			sb.AppendLine();
			sb.AppendXmlSummary($"Verify static interactions with the mock of <see cref=\"{escapedClassName}\" />.", "\t");
			sb.Append("\tinternal interface IMockStaticVerifyFor").Append(name).AppendLine();
			sb.Append("\t{").AppendLine();
			DefineVerifyInterface(sb, @class, $"IMockStaticVerifyFor{name}", MemberType.Static);
			sb.Append("\t}").AppendLine();
		}

		#endregion IMockVerifyForXXX

		sb.Append("}").AppendLine();
		sb.AppendLine();
		sb.AppendLine("#nullable disable annotations");
		return sb.ToString();
	}

	#pragma warning disable S107 // Methods should not have too many parameters
	private static void ImplementMockForInterface(StringBuilder sb, string mockRegistryName, string name, bool hasEvents, bool hasProtectedMembers, bool hasProtectedEvents, bool hasStaticMembers, bool hasStaticEvents)
	{
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
		sb.Append("\t\tIMockSetupFor").Append(name).Append(" IMockFor").Append(name).Append(".Setup").AppendLine();
		sb.Append("\t\t\t=> this;").AppendLine();

		if (hasProtectedMembers)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tIMockProtectedSetupFor").Append(name).Append(" IMockFor").Append(name).Append(".SetupProtected").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		if (hasStaticMembers)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tIMockStaticSetupFor").Append(name).Append(" IMockFor").Append(name).Append(".SetupStatic").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		if (hasEvents)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tIMockRaiseOn").Append(name).Append(" IMockFor").Append(name).Append(".Raise").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		if (hasProtectedEvents)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tIMockProtectedRaiseOn").Append(name).Append(" IMockFor").Append(name).Append(".RaiseProtected").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		if (hasStaticEvents)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tIMockStaticRaiseOn").Append(name).Append(" IMockFor").Append(name).Append(".RaiseStatic").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
		sb.Append("\t\tIMockVerifyFor").Append(name).Append(" IMockFor").Append(name).Append(".Verify").AppendLine();
		sb.Append("\t\t\t=> this;").AppendLine();

		if (hasProtectedMembers || hasProtectedEvents)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tIMockProtectedVerifyFor").Append(name).Append(" IMockFor").Append(name).Append(".VerifyProtected").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		if (hasStaticMembers || hasStaticEvents)
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tIMockStaticVerifyFor").Append(name).Append(" IMockFor").Append(name).Append(".VerifyStatic").AppendLine();
			sb.Append("\t\t\t=> this;").AppendLine();
		}

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<IMockVerifyFor").Append(name).Append("> IMockFor").Append(name).Append(".VerifySetup(global::Mockolate.Setup.IMethodSetup setup)").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".Method<IMockVerifyFor").Append(name).Append(">(this, setup);").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tbool IMockFor").Append(name).Append(".VerifyThatAllInteractionsAreVerified()").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".Interactions.GetUnverifiedInteractions().Count == 0;").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tbool IMockFor").Append(name).Append(".VerifyThatAllSetupsAreUsed()").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".GetUnusedSetups(this.").Append(mockRegistryName).Append(".Interactions).Count == 0;").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tvoid IMockFor").Append(name).Append(".ClearAllInteractions()").AppendLine();
		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".ClearAllInteractions();").AppendLine();

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Monitor.MockMonitor<IMockVerifyFor").Append(name).Append("> IMockFor").Append(name).Append(".Monitor()").AppendLine();
		sb.Append("\t\t\t=> new global::Mockolate.Monitor.MockMonitor<IMockVerifyFor").Append(name).Append(">(this.").Append(mockRegistryName).Append(".Interactions, interactions => new VerifyMonitor").Append(name).Append("(new global::Mockolate.MockRegistry(this.").Append(mockRegistryName).Append(".Behavior, interactions)));").AppendLine();
		sb.AppendLine();
	}
	#pragma warning restore S107 // Methods should not have too many parameters

	#region Mock Helpers

	private static void AppendMockSubject_BaseClassConstructor(StringBuilder sb, string mockRegistryName, string name, Method constructor)
	{
		string mockRegistry = CreateUniqueParameterName(constructor.Parameters, "mockRegistry");
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(name).Append("\" />").AppendLine();
		sb.Append(constructor.Attributes, "\t\t");
		sb.Append("\t\tpublic ").Append(name).Append("(global::Mockolate.MockRegistry ").Append(mockRegistry);
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
		sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(" = ").Append(mockRegistry).Append(';').AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
	}

	private static void AppendMockSubject_ImplementClass(StringBuilder sb, Class @class, string mockRegistryName,
		MockClass? mockClass)
	{
		string className = @class.ClassFullName;
		sb.Append("\t\t#region ").Append(@class.DisplayString).AppendLine();
		sb.AppendLine();

		List<Event>? mockEvents = mockClass?.AllEvents().ToList();
		foreach (Event @event in @class.AllEvents())
		{
			if (mockEvents?.All(e => !Event.EqualityComparer.Equals(@event, e)) != false)
			{
				AppendMockSubject_ImplementClass_AddEvent(sb, @event, mockRegistryName, className, mockClass is not null,
					@class.IsInterface);
				sb.AppendLine();
			}
		}

		List<Property>? mockProperties = mockClass?.AllProperties().ToList();
		foreach (Property property in @class.AllProperties())
		{
			if (mockProperties?.All(p => !Property.EqualityComparer.Equals(property, p)) != false)
			{
				AppendMockSubject_ImplementClass_AddProperty(sb, property, mockRegistryName, className, mockClass is not null,
					@class.IsInterface);
				sb.AppendLine();
			}
		}

		List<Method>? mockMethods = mockClass?.AllMethods().ToList();
		foreach (Method method in @class.AllMethods())
		{
			if (mockMethods?.All(m => !Method.EqualityComparer.Equals(method, m)) != false)
			{
				AppendMockSubject_ImplementClass_AddMethod(sb, method, mockRegistryName, className, mockClass is not null,
					@class.IsInterface, @class);
				sb.AppendLine();
			}
		}

		sb.Append("\t\t#endregion ").Append(@class.DisplayString).AppendLine();
	}

	private static void AppendMockSubject_ImplementClass_AddEvent(StringBuilder sb, Event @event, string mockRegistryName, string className,
		bool explicitInterfaceImplementation, bool isClassInterface)
	{
		string mockRegistry = @event.IsStatic ? "MockRegistryProvider.Value" : $"this.{mockRegistryName}";
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(@event.ContainingType.EscapeForXmlDoc()).Append('.')
			.Append(@event.Name.EscapeForXmlDoc()).AppendLine("\" />");
		sb.Append(@event.Attributes, "\t\t");
		if (explicitInterfaceImplementation)
		{
			sb.Append(@event.IsStatic ? "\t\tstatic event " : "\t\tevent ").Append(@event.Type.Fullname.TrimEnd('?'))
				.Append("? ").Append(className).Append('.').Append(@event.Name).AppendLine();
		}
		else
		{
			if (@event.ExplicitImplementation is null)
			{
				sb.Append("\t\t").Append(@event.Accessibility.ToVisibilityString()).Append(' ');
				if (@event.IsStatic)
				{
					sb.Append("static ");
				}

				if (!isClassInterface && @event.UseOverride)
				{
					sb.Append("override ");
				}

				sb.Append("event ").Append(@event.Type.Fullname.TrimEnd('?')).Append("? ");
			}
			else
			{
				sb.Append(@event.IsStatic ? "\t\tstatic event " : "\t\tevent ").Append(@event.Type.Fullname.TrimEnd('?')).Append("? ")
					.Append(@event.ExplicitImplementation).Append('.');
			}

			sb.Append(@event.Name).AppendLine();
		}

		sb.AppendLine("\t\t{");
		if (isClassInterface && !explicitInterfaceImplementation && @event.ExplicitImplementation is null)
		{
			sb.Append("\t\t\tadd").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t").Append(mockRegistry).Append(".AddEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
			if (!@event.IsStatic)
			{
				sb.Append("\t\t\t\tif (this.Wraps is not null)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\tthis.Wraps.").Append(@event.Name).Append(" += value;").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
			}

			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\tremove").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\t").Append(mockRegistry).Append(".RemoveEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
			if (!@event.IsStatic)
			{
				sb.Append("\t\t\t\tif (this.Wraps is not null)").AppendLine();
				sb.Append("\t\t\t\t{").AppendLine();
				sb.Append("\t\t\t\t\tthis.Wraps.").Append(@event.Name).Append(" -= value;").AppendLine();
				sb.Append("\t\t\t\t}").AppendLine();
			}

			sb.Append("\t\t\t}").AppendLine();
		}
		else
		{
			sb.Append("\t\t\tadd => ").Append(mockRegistry).Append(".AddEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
			sb.Append("\t\t\tremove => ").Append(mockRegistry).Append(".RemoveEvent(").Append(@event.GetUniqueNameString())
				.Append(", value?.Target, value?.Method);").AppendLine();
		}

		sb.AppendLine("\t\t}");
	}

	private static void AppendMockSubject_ImplementClass_AddProperty(StringBuilder sb, Property property, string mockRegistryName,
		string className, bool explicitInterfaceImplementation, bool isClassInterface)
	{
		string mockRegistry = property.IsStatic ? "MockRegistryProvider.Value" : $"this.{mockRegistryName}";
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
			sb.Append(property.IsStatic ? "\t\tstatic " : "\t\t").Append(property.Type.Fullname)
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
				if (property.IsStatic)
				{
					sb.Append("static ");
				}

				if (!isClassInterface && property.UseOverride)
				{
					sb.Append("override ");
				}

				sb.Append(property.Type.Fullname).Append(" ");
			}
			else
			{
				sb.Append("\t\t");

				if (property.IsStatic)
				{
					sb.Append("static ");
				}

				sb.Append(property.Type.Fullname).Append(" ").Append(property.ExplicitImplementation)
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
					sb.Append("\t\t\t\t\treturn ").Append(mockRegistry).Append(".GetIndexer<").AppendTypeOrWrapper(property.Type)
						.Append(">(").Append(FormatIndexerParametersAsNameOrWrapper(property.IndexerParameters.Value))
						.Append(").GetResult(() => ")
						.AppendDefaultValueGeneratorFor(property.Type, $"this.{mockRegistryName}.Behavior.DefaultValue")
						.Append(");").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
					sb.Append("\t\t\t\tvar ").Append(indexerResultVarName).Append(" = this.").Append(mockRegistryName).Append(".GetIndexer<")
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
					sb.Append("\t\t\t\treturn ").Append(mockRegistry).Append(".GetProperty<")
						.AppendTypeOrWrapper(property.Type).Append(">(")
						.Append(property.GetUniqueNameString()).Append(", () => ")
						.AppendDefaultValueGeneratorFor(property.Type, $"{mockRegistry}.Behavior.DefaultValue");
					if (!property.IsStatic)
					{
						sb.Append(", this.Wraps is null ? null : () => this.Wraps.").Append(property.Name);
					}
					else
					{
						sb.Append(", null");
					}

					sb.Append(");").AppendLine();
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
					sb.Append("\t\t\t\tvar ").Append(indexerResultVarName).Append(" = this.").Append(mockRegistryName).Append(".GetIndexer<")
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
						.AppendDefaultValueGeneratorFor(property.Type, $"this.{mockRegistryName}.Behavior.DefaultValue")
						.Append(");").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\treturn ").Append(mockRegistry).Append(".GetProperty<")
						.AppendTypeOrWrapper(property.Type).Append(">(")
						.Append(property.GetUniqueNameString()).Append(", () => ")
						.AppendDefaultValueGeneratorFor(property.Type, $"{mockRegistry}.Behavior.DefaultValue")
						.Append(", () => base.").Append(property.Name).Append(");").AppendLine();
				}
			}
			else if (property is { IsIndexer: true, IndexerParameters: not null, })
			{
				sb.Append("\t\t\t\treturn this.").Append(mockRegistryName).Append(".GetIndexer<")
					.AppendTypeOrWrapper(property.Type).Append(">(")
					.Append(FormatIndexerParametersAsNameOrWrapper(property.IndexerParameters.Value))
					.Append(").GetResult(() => ")
					.AppendDefaultValueGeneratorFor(property.Type, $"this.{mockRegistryName}.Behavior.DefaultValue")
					.Append(");").AppendLine();
			}
			else
			{
				sb.Append("\t\t\t\treturn ").Append(mockRegistry).Append(".GetProperty<")
					.AppendTypeOrWrapper(property.Type).Append(">(").Append(property.GetUniqueNameString())
					.Append(", () => ")
					.AppendDefaultValueGeneratorFor(property.Type, $"{mockRegistry}.Behavior.DefaultValue")
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
					sb.Append("\t\t\t\tthis.").Append(mockRegistryName).Append(".SetIndexer<")
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
					sb.Append("\t\t\t\t").Append(mockRegistry).Append(".SetProperty(").Append(property.GetUniqueNameString())
						.Append(", value);").AppendLine();
					if (!property.IsStatic)
					{
						sb.Append("\t\t\t\tif (this.Wraps is not null)").AppendLine();
						sb.Append("\t\t\t\t{").AppendLine();
						sb.Append("\t\t\t\t\tthis.Wraps.").Append(property.Name).Append(" = value;").AppendLine();
						sb.Append("\t\t\t\t}").AppendLine();
					}
				}
			}
			else if (property is { IsIndexer: true, IndexerParameters: not null, })
			{
				if (!isClassInterface && !property.IsAbstract)
				{
					sb.Append("\t\t\t\tif (!this.").Append(mockRegistryName).Append(".SetIndexer<").Append(property.Type.Fullname)
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
					sb.Append("\t\t\t\tthis.").Append(mockRegistryName).Append(".SetIndexer<")
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
					sb.Append("\t\t\t\tif (!").Append(mockRegistry).Append(".SetProperty(").Append(property.GetUniqueNameString())
						.Append(", value))").AppendLine();
					sb.Append("\t\t\t\t{").AppendLine();
					sb.Append("\t\t\t\t\tbase.").Append(property.Name).Append(" = value;").AppendLine();
					sb.Append("\t\t\t\t}").AppendLine();
				}
				else
				{
					sb.Append("\t\t\t\t").Append(mockRegistry).Append(".SetProperty(").Append(property.GetUniqueNameString())
						.AppendLine(", value);");
				}
			}

			sb.AppendLine("\t\t\t}");
		}

		sb.AppendLine("\t\t}");
	}

	private static void AppendMockSubject_ImplementClass_AddMethod(StringBuilder sb, Method method, string mockRegistryName, string className,
		bool explicitInterfaceImplementation, bool isClassInterface, Class @class)
	{
		string mockRegistry = method.IsStatic ? "MockRegistryProvider.Value" : $"this.{mockRegistryName}";
		sb.Append("\t\t/// <inheritdoc cref=\"").Append(method.ContainingType.EscapeForXmlDoc()).Append('.')
			.Append(method.Name.EscapeForXmlDoc()).Append('(')
			.Append(string.Join(", ", method.Parameters.Select(p => p.RefKind.GetString() + p.Type.Fullname))
				.EscapeForXmlDoc()).AppendLine(")\" />");
		sb.Append(method.Attributes, "\t\t");
		if (explicitInterfaceImplementation)
		{
			sb.Append("\t\t");
			if (method.IsStatic)
			{
				sb.Append("static ");
			}

			sb.Append(method.ReturnType.Fullname).Append(' ').Append(className).Append('.').Append(method.Name)
				.Append('(');
		}
		else
		{
			sb.Append("\t\t");
			if (method.ExplicitImplementation is null)
			{
				sb.Append(method.Accessibility.ToVisibilityString()).Append(' ');
				if (method.IsStatic)
				{
					sb.Append("static ");
				}

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
				if (method.IsStatic)
				{
					sb.Append("static ");
				}

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
				.Append(" = ").Append(mockRegistry).Append(".InvokeMethod<")
				.AppendTypeOrWrapper(method.ReturnType).Append(">(").Append(method.GetUniqueNameString())
				.Append(", ").Append(parameterVarName).Append(" => ")
				.AppendDefaultValueGeneratorFor(method.ReturnType, $"{mockRegistry}.Behavior.DefaultValue",
					$", {parameterVarName}");
		}
		else
		{
			sb.Append("\t\t\tglobal::Mockolate.Setup.MethodSetupResult ").Append(methodExecutionVarName)
				.Append(" = ").Append(mockRegistry).Append(".InvokeMethod(")
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
			if (!explicitInterfaceImplementation && isClassInterface && !method.IsStatic)
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
					$"{mockRegistry}.Behavior.DefaultValue");

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
				$"{mockRegistry}.Behavior.DefaultValue");

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
					$"{mockRegistry}.Behavior.DefaultValue");

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
						$"{mockRegistry}.Behavior.DefaultValue");
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
							.AppendDefaultValueGeneratorFor(parameter.Type, $"{mockRegistry}.Behavior.DefaultValue")
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

	private static void DefineSetupInterface(StringBuilder sb, Class @class, MemberType memberType)
	{
		#region Properties

		Func<Property, bool> propertyPredicate = property
			=> property.ExplicitImplementation is null && property is { IsIndexer: false, } &&
			   property.MemberType == memberType;
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.AppendXmlSummary($"Setup for the {property.Type.Fullname.EscapeForXmlDoc()} property <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{property.Name.EscapeForXmlDoc()}\" />.");
			sb.Append("\t\tglobal::Mockolate.Setup.PropertySetup<").Append(property.Type.Fullname).Append("> ").Append(property.Name).Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate =
			indexer => indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, } &&
			           indexer.MemberType == memberType;
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

		Func<Method, bool> methodPredicate = method => method.ExplicitImplementation is null &&
		                                               method.MemberType == memberType;
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

	private static void ImplementSetupInterface(StringBuilder sb, Class @class, string mockRegistryName, string setupName, MemberType memberType)
	{
		#region Properties

		Func<Property, bool> propertyPredicate = property
			=> property.ExplicitImplementation is null && property is { IsIndexer: false, } &&
			   property.MemberType == memberType;
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tglobal::Mockolate.Setup.PropertySetup<").Append(property.Type.Fullname)
				.Append("> global::Mockolate.Mock.").Append(setupName).Append('.').Append(property.Name).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tvar propertySetup = new global::Mockolate.Setup.PropertySetup<")
				.Append(property.Type.Fullname).Append(">(").Append(property.GetUniqueNameString()).Append(");")
				.AppendLine();
			sb.Append("\t\t\t\tthis.").Append(mockRegistryName).Append(".SetupProperty(propertySetup);").AppendLine();
			sb.Append("\t\t\t\treturn propertySetup;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate =
			indexer => indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, } &&
			           indexer.MemberType == memberType;
		foreach (Property indexer in @class.AllProperties().Where(indexerPredicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tglobal::Mockolate.Setup.IndexerSetup<").AppendTypeOrWrapper(indexer.Type);

			foreach (MethodParameter parameter in indexer.IndexerParameters!)
			{
				sb.Append(", ").AppendTypeOrWrapper(parameter.Type);
			}

			sb.Append("> global::Mockolate.Mock.").Append(setupName).Append(".this[").Append(string.Join(", ",
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
			sb.Append("\t\t\t\tthis.").Append(mockRegistryName).Append(".SetupIndexer(indexerSetup);").AppendLine();
			sb.Append("\t\t\t\treturn indexerSetup;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Methods

		Func<Method, bool> methodPredicate = method => method.ExplicitImplementation is null &&
		                                               method.MemberType == memberType;
		foreach (Method method in @class.AllMethods().Where(methodPredicate))
		{
			AppendMethodSetupImplementation(sb, method, mockRegistryName, setupName, false);
		}

		foreach (Method method in @class.AllMethods()
			         .Where(methodPredicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Parameters.Count > 0))
		{
			AppendMethodSetupImplementation(sb, method, mockRegistryName, setupName, true);
		}

		#endregion
	}

	private static void AppendMethodSetupImplementation(StringBuilder sb, Method method, string mockRegistryName, string setupName,
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

			sb.Append("> global::Mockolate.Mock.").Append(setupName).Append('.');
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

			sb.Append(" global::Mockolate.Mock.").Append(setupName).Append('.').Append(methodName).Append("(");
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
			sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(".SetupMethod(methodSetup);").AppendLine();
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
			sb.Append("\t\t\tthis.").Append(mockRegistryName).Append(".SetupMethod(methodSetup);").AppendLine();
			sb.Append("\t\t\treturn methodSetup;").AppendLine();
		}

		sb.AppendLine("\t\t}");
		sb.AppendLine();
	}

	#endregion Setup Helpers

	#region Raise Helpers

	private static void DefineRaiseInterface(StringBuilder sb, Class @class, MemberType memberType)
	{
		Func<Event, bool> predicate = @event => @event.ExplicitImplementation is null &&
		                                        @event.MemberType == memberType;
		foreach (Event @event in @class.AllEvents().Where(predicate))
		{
			sb.AppendXmlSummary($"Raise the <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{@event.Name.EscapeForXmlDoc()}\"/> event.");
			sb.Append("\t\tvoid ").Append(@event.Name).Append("(").Append(FormatParametersWithTypeAndName(@event.Delegate.Parameters)).Append(");").AppendLine();
			sb.AppendLine();
		}

		foreach (string? eventName in @class.AllEvents()
			         .Where(predicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Delegate.Parameters.Count > 0)
			         .Select(x => x.Name))
		{
			sb.AppendXmlSummary($"Raise the <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{eventName.EscapeForXmlDoc()}\"/> event.");
			sb.Append("\t\tvoid ").Append(eventName).Append("(global::Mockolate.Parameters.IDefaultEventParameters parameters);").AppendLine();
			sb.AppendLine();
		}
	}

	private static void ImplementRaiseInterface(StringBuilder sb, Class @class, string mockRegistryName, string raiseOnName, MemberType memberType)
	{
		string mockRegistry = memberType == MemberType.Static ? "MockRegistryProvider.Value" : $"this.{mockRegistryName}";
		Func<Event, bool> predicate = @event => @event.ExplicitImplementation is null &&
		                                        @event.MemberType == memberType;
		foreach (Event @event in @class.AllEvents().Where(predicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\tvoid ").Append(raiseOnName).Append('.').Append(@event.Name).Append("(")
				.Append(FormatParametersWithTypeAndName(@event.Delegate.Parameters))
				.Append(")").AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\t").Append(mockRegistry).Append(".Raise(").Append(@event.GetUniqueNameString());
			if (@event.Delegate.Parameters.Count > 0)
			{
				sb.Append(", ").Append(FormatParametersAsNames(@event.Delegate.Parameters));
			}

			sb.Append(");").AppendLine();
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
			sb.Append("\t\tvoid ").Append(raiseOnName).Append('.').Append(@event.Name).Append("(global::Mockolate.Parameters.IDefaultEventParameters parameters)")
				.AppendLine();
			sb.AppendLine("\t\t{");
			sb.Append("\t\t\tglobal::Mockolate.MockBehavior mockBehavior = ").Append(mockRegistry).Append(".Behavior;").AppendLine();
			sb.Append("\t\t\t").Append(mockRegistry).Append(".Raise(").Append(@event.GetUniqueNameString());

			if (@event.Delegate.Parameters.Count > 0)
			{
				sb.Append(", ").Append(string.Join(", ", @event.Delegate.Parameters.Select(p => $"mockBehavior.DefaultValue.Generate(default({p.Type.Fullname.TrimEnd('?')}))")));
			}

			sb.Append(");").AppendLine();
			sb.AppendLine("\t\t}");
			sb.AppendLine();
		}
	}

	#endregion Raise Helpers

	#region Verify Helpers

	private static void DefineVerifyInterface(StringBuilder sb, Class @class, string verifyName, MemberType memberType)
	{
		#region Properties

		Func<Property, bool> propertyPredicate = property
			=> property.ExplicitImplementation is null && property is { IsIndexer: false, } &&
			   property.MemberType == memberType;
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.AppendXmlSummary($"Verify interactions with the {property.Type.Fullname.EscapeForXmlDoc()} property <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{property.Name.EscapeForXmlDoc()}\" />.");
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationPropertyResult<").Append(verifyName).Append(", ").Append(property.Type.Fullname).Append("> ").Append(property.Name).Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate =
			indexer => indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, } &&
			           indexer.MemberType == memberType;
		foreach (Property indexer in @class.AllProperties().Where(indexerPredicate))
		{
			sb.AppendXmlSummary(
				$"Verify interactions with the {indexer.Type.Fullname.EscapeForXmlDoc()} indexer <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.this[{string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.RefKind.GetString() + p.Type.Fullname.EscapeForXmlDoc()))}]\" />.");
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationIndexerResult<").Append(verifyName).Append(", ").AppendTypeOrWrapper(indexer.Type).Append("> this[")
				.Append(string.Join(", ", indexer.IndexerParameters.Value.Select(p => p.ToParameter() + (p.CanBeNullable() ? "? " : " ") + p.Name)))
				.Append("] { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Methods

		Func<Method, bool> methodPredicate = method => method.ExplicitImplementation is null &&
		                                               method.MemberType == memberType;
		foreach (Method method in @class.AllMethods().Where(methodPredicate))
		{
			AppendMethodVerifyDefinition(sb, @class, method, verifyName, false);
		}

		foreach (Method method in @class.AllMethods()
			         .Where(methodPredicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Parameters.Count > 0))
		{
			AppendMethodVerifyDefinition(sb, @class, method, verifyName, true);
		}

		#endregion

		#region Events

		Func<Event, bool> eventPredicate = @event => @event.ExplicitImplementation is null &&
		                                             @event.MemberType == memberType;
		foreach (string eventName in @class.AllEvents().Where(eventPredicate).Select(e => e.Name))
		{
			sb.AppendXmlSummary($"Verify subscription on the {eventName} event <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{eventName}\" />.");
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationEventResult<").Append(verifyName).Append("> ").Append(eventName).Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		#endregion
	}

	private static void AppendMethodVerifyDefinition(StringBuilder sb, Class @class, Method method, string verifyName,
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
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<").Append(verifyName).Append("> ").Append(methodName)
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

	private static void ImplementVerifyInterface(StringBuilder sb, Class @class, string mockRegistryName, string verifyName, MemberType memberType)
	{
		#region Properties

		Func<Property, bool> propertyPredicate = property => property.ExplicitImplementation is null && property is { IsIndexer: false, } &&
		                                                     property.MemberType == memberType;
		foreach (Property property in @class.AllProperties().Where(propertyPredicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationPropertyResult<").Append(verifyName).Append(", ").Append(property.Type.Fullname).Append("> ").Append(verifyName).Append('.').Append(property.Name).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn new global::Mockolate.Verify.VerificationPropertyResult<").Append(verifyName).Append(", ").Append(property.Type.Fullname).Append(">(this, this.").Append(mockRegistryName).Append(", ").Append(property.GetUniqueNameString()).Append(");").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion

		#region Indexers

		Func<Property, bool> indexerPredicate = indexer => indexer.ExplicitImplementation is null && indexer is { IsIndexer: true, IndexerParameters: not null, } &&
		                                                   indexer.MemberType == memberType;
		foreach (Property indexer in @class.AllProperties().Where(indexerPredicate))
		{
			sb.Append("\t\t/// <inheritdoc />").AppendLine();
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationIndexerResult<").Append(verifyName).Append(", ").AppendTypeOrWrapper(indexer.Type).Append("> ").Append(verifyName).Append(".this[").Append(string.Join(", ", indexer.IndexerParameters!.Value.Select(p => p.ToParameter() + (p.CanBeNullable() ? "? " : " ") + p.Name))).Append("]").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn new global::Mockolate.Verify.VerificationIndexerResult<").Append(verifyName).Append(", ").AppendTypeOrWrapper(indexer.Type).Append(">(this, this.").Append(mockRegistryName).Append(", [ ");
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

		Func<Method, bool> methodPredicate = method => method.ExplicitImplementation is null &&
		                                               method.MemberType == memberType;
		foreach (Method method in @class.AllMethods().Where(methodPredicate))
		{
			AppendMethodVerifyImplementation(sb, method, mockRegistryName, verifyName, false);
		}

		foreach (Method method in @class.AllMethods()
			         .Where(methodPredicate)
			         .GroupBy(m => m.Name)
			         .Where(g => g.Count() == 1)
			         .Select(g => g.Single())
			         .Where(m => m.Parameters.Count > 0))
		{
			AppendMethodVerifyImplementation(sb, method, mockRegistryName, verifyName, true);
		}

		#endregion

		#region Events

		Func<Event, bool> eventPredicate = @event => @event.ExplicitImplementation is null &&
		                                             @event.MemberType == memberType;
		foreach (Event @event in @class.AllEvents().Where(eventPredicate))
		{
			sb.AppendXmlSummary($"Verify subscription on the {@event.Name} event <see cref=\"{@class.ClassFullName.EscapeForXmlDoc()}.{@event.Name}\" />.");
			sb.Append("\t\t[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]").AppendLine();
			sb.Append("\t\tglobal::Mockolate.Verify.VerificationEventResult<").Append(verifyName).Append("> ").Append(verifyName).Append('.').Append(@event.Name).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tget").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn new global::Mockolate.Verify.VerificationEventResult<").Append(verifyName).Append(">(this, this.").Append(mockRegistryName).Append(", ").Append(@event.GetUniqueNameString()).Append(");").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		#endregion
	}

	private static void AppendMethodVerifyImplementation(StringBuilder sb, Method method, string mockRegistryName, string verifyName,
		bool useParameters, string? methodNameOverride = null)
	{
		string methodName = methodNameOverride ?? method.Name;
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Verify.VerificationResult<").Append(verifyName).Append("> ").Append(verifyName).Append('.').Append(methodName).Append("(");
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
		sb.AppendLine();

		sb.Append("\t\t\t=> this.").Append(mockRegistryName).Append(".Method<").Append(verifyName).Append(">(this, ");

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
