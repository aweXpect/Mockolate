using System.Text;
using Mockolate.SourceGenerators.Entities;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string MockCombinationClass(
		string fileName,
		string name,
		Class @class,
		(string Name, Class Class)[] additionalInterfaces,
		HashSet<(string, string)> combinationSet)
	{
		EquatableArray<Method>? constructors = (@class as MockClass)?.Constructors;
		string escapedClassName = @class.ClassFullName.EscapeForXmlDoc();
		bool hasEvents = @class.AllEvents().Any(x => !x.IsStatic);
		bool hasStaticEvents = @class.IsInterface && @class.AllEvents().Any(x => x.IsStatic);
		bool hasStaticMembers = @class.IsInterface && (@class.AllMethods().Any(x => x.IsStatic) || @class.AllProperties().Any(x => x.IsStatic));
		StringBuilder sb = InitializeBuilder();

		sb.Append("#nullable enable annotations").AppendLine();
		sb.Append("namespace Mockolate;").AppendLine();
		sb.AppendLine();

		#region Extensions

		sb.AppendXmlSummary($"Mock extensions for <see cref=\"{escapedClassName}\" /> that also implements<br />\n///      - {string.Join("<br />\n///      - ", additionalInterfaces.Select(x => $"<see cref=\"{x.Class.ClassFullName.EscapeForXmlDoc()}\" />"))}.", "");
		sb.Append("internal static partial class MockExtensionsFor").Append(fileName).AppendLine();
		sb.Append("{").AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MockExtensionsFor").Append(fileName).Append("\" />").AppendLine();
		sb.Append("\textension(").Append(@class.ClassFullName).Append(" mock)").AppendLine();
		sb.Append("\t{").AppendLine();

		#region Implementing

		(string Name, Class Class) lastInterface = additionalInterfaces[additionalInterfaces.Length - 1];
		sb.AppendXmlSummary($"Create a mock that also implements <see cref=\"{lastInterface.Class.ClassFullName.EscapeForXmlDoc()}\" />.");
		sb.Append("\t\t[global::Mockolate.MockGeneratorImplementing]").AppendLine();
		sb.Append("\t\tpublic ").Append(@class.ClassFullName).Append(" Implementing<TInterface>(params global::System.Action<global::Mockolate.Mock.IMockSetupFor").Append(lastInterface.Name).Append(">[] setups)").AppendLine();
		sb.Append("\t\t\twhere TInterface : ").Append(lastInterface.Class.ClassFullName).AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar x = mock as global::Mockolate.IMock;").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Mock.").Append(fileName).Append(" value;").AppendLine();

		bool useTryCast = false;
		bool useTryCastWithDefaultValue = false;
		if (!@class.IsInterface && constructors?.Count > 0)
		{
			sb.Append("\t\t\tif (x.ConstructorParameters.Length == 0)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			if (constructors.Value.Any(m => m.Parameters.Count == 0))
			{
				sb.Append("\t\t\t\tvalue = new global::Mockolate.Mock.").Append(fileName).Append("(x.Registrations);").AppendLine();
			}
			else
			{
				sb.Append("\t\t\t\tthrow new global::Mockolate.Exceptions.MockException(\"No parameterless constructor found for '").Append(@class.DisplayString).Append("'. Please provide constructor parameters.\");").AppendLine();
			}

			sb.Append("\t\t\t}").AppendLine();
			int constructorIndex = 0;
			foreach (EquatableArray<MethodParameter> constructorParameters in constructors.Value
				         .Select(constructor => constructor.Parameters))
			{
				constructorIndex++;
				int requiredParameters = constructorParameters.Count(c => !c.HasExplicitDefaultValue);
				if (requiredParameters < constructorParameters.Count)
				{
					sb.Append("\t\t\telse if (x.ConstructorParameters.Length >= ")
						.Append(requiredParameters).Append(" && x.ConstructorParameters.Length <= ")
						.Append(constructorParameters.Count);
				}
				else
				{
					sb.Append("\t\t\telse if (x.ConstructorParameters.Length == ")
						.Append(constructorParameters.Count);
				}

				int constructorParameterIndex = 0;
				foreach (MethodParameter parameter in constructorParameters)
				{
					useTryCast = useTryCast || !parameter.HasExplicitDefaultValue;
					useTryCastWithDefaultValue = useTryCastWithDefaultValue || parameter.HasExplicitDefaultValue;
					sb.AppendLine().Append("\t\t\t    && ")
						.Append(parameter.HasExplicitDefaultValue ? "TryCastWithDefaultValue" : "TryCast")
						.Append("(x.ConstructorParameters, ")
						.Append(constructorParameterIndex++)
						.Append(parameter.HasExplicitDefaultValue ? $", {parameter.ExplicitDefaultValue}" : "")
						.Append(", x.Registrations.Behavior, out ").Append(parameter.Type.Fullname).Append(" c")
						.Append(constructorIndex)
						.Append('p')
						.Append(constructorParameterIndex).Append(")");
				}

				sb.Append(")").AppendLine();
				sb.Append("\t\t\t{").AppendLine();
				sb.Append("\t\t\t\tvalue = new global::Mockolate.Mock.").Append(fileName)
					.Append("(x.Registrations");
				for (int j = 1; j <= constructorParameters.Count; j++)
				{
					sb.Append(", ").Append('c').Append(constructorIndex).Append('p').Append(j);
				}

				sb.Append(");").AppendLine();
				sb.Append("\t\t\t}").AppendLine();
			}

			sb.Append("\t\t\telse").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append(
					"\t\t\t\tthrow new global::Mockolate.Exceptions.MockException($\"Could not find any constructor for '")
				.Append(@class.DisplayString).Append("' that matches the {x.ConstructorParameters.Length} given parameters ({string.Join(\", \", x.ConstructorParameters)}).\");")
				.AppendLine();
			sb.Append("\t\t\t}").AppendLine();
		}
		else
		{
			sb.Append("\t\t\tvalue = new global::Mockolate.Mock.").Append(fileName).Append("(x.Registrations);").AppendLine();
		}

		sb.Append("\t\t\tIMockBehaviorAccess mockBehaviorAccess = (global::Mockolate.IMockBehaviorAccess)x.Registrations.Behavior;").AppendLine();
		sb.Append("\t\t\tif (mockBehaviorAccess.TryGet<global::System.Action<global::Mockolate.Mock.IMockSetupFor").Append(lastInterface.Name).Append(">[]?>(out var additionalSetups))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tif (setups.Length > 0)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tvar concatenatedSetups = new global::System.Action<global::Mockolate.Mock.IMockSetupFor").Append(lastInterface.Name).Append(">[additionalSetups.Length + setups.Length];").AppendLine();
		sb.Append("\t\t\t\t\tadditionalSetups.CopyTo(concatenatedSetups, 0);").AppendLine();
		sb.Append("\t\t\t\t\tsetups.CopyTo(concatenatedSetups, additionalSetups.Length);").AppendLine();
		sb.Append("\t\t\t\t\tsetups = concatenatedSetups;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\telse").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tsetups = additionalSetups;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\tif (setups.Length > 0)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tforeach (var setup in setups)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tsetup.Invoke(value);").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\treturn value;").AppendLine();
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

		sb.Append("\t\t}").AppendLine();

		#endregion Implementing

		sb.Append("\t}").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		#region As<T>

		(string Name, Class Class)[] sources = [(name, @class), ..additionalInterfaces,];
		foreach ((string Name, Class Class) source in sources.Take(sources.Length - 1))
		{
			if (!combinationSet.Add((source.Name, lastInterface.Name)) ||
			    !combinationSet.Add((lastInterface.Name, source.Name)))
			{
				continue;
			}

			sb.Append("internal static partial class MockExtensionsFor").Append(lastInterface.Name).AppendLine();
			sb.Append("{").AppendLine();
			sb.Append("\textension(global::Mockolate.Mock.IMockFor").Append(source.Name).Append(" mock)").AppendLine();
			sb.Append("\t{").AppendLine();
			sb.AppendXmlSummary($"Interprets the mock as a mock for <see cref=\"{lastInterface.Class.ClassFullName.EscapeForXmlDoc()}\" />.");
			sb.AppendXmlRemarks($"Throws a <see cref=\"global::Mockolate.Exceptions.MockException\" /> if the subject does not implement <see cref=\"{lastInterface.Class.ClassFullName.EscapeForXmlDoc()}\" />.");
			sb.Append("\t\tpublic global::Mockolate.Mock.IMockFor").Append(lastInterface.Name).Append(" As<T>() where T : ").Append(lastInterface.Class.ClassFullName).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tif (mock is global::Mockolate.Mock.IMockFor").Append(lastInterface.Name).Append(" typed)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn typed;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\tthrow new global::Mockolate.Exceptions.MockException($\"The subject does not support type {typeof(T)}.\");").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.Append("\t}").AppendLine();
			sb.Append("}").AppendLine();
			sb.AppendLine();
			sb.Append("internal static partial class MockExtensionsFor").Append(source.Name).AppendLine();
			sb.Append("{").AppendLine();
			sb.Append("\textension(global::Mockolate.Mock.IMockFor").Append(lastInterface.Name).Append(" mock)").AppendLine();
			sb.Append("\t{").AppendLine();
			sb.AppendXmlSummary($"Interprets the mock as a mock for <see cref=\"{source.Class.ClassFullName.EscapeForXmlDoc()}\" />.");
			sb.AppendXmlRemarks($"Throws a <see cref=\"global::Mockolate.Exceptions.MockException\" /> if the subject does not implement <see cref=\"{source.Class.ClassFullName.EscapeForXmlDoc()}\" />.");
			sb.Append("\t\tpublic global::Mockolate.Mock.IMockFor").Append(source.Name).Append(" As<T>() where T : ").Append(source.Class.ClassFullName).AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tif (mock is global::Mockolate.Mock.IMockFor").Append(source.Name).Append(" typed)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\treturn typed;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t\tthrow new global::Mockolate.Exceptions.MockException($\"The subject does not support type {typeof(T)}.\");").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.Append("\t}").AppendLine();
			sb.Append("}").AppendLine();
			sb.AppendLine();
		}

		#endregion As<T>

		#endregion Extensions

		#region MockForXXX

		sb.Append("internal static partial class Mock").AppendLine();
		sb.Append("{").AppendLine();
		sb.AppendXmlSummary($"A mock implementation for <see cref=\"{escapedClassName}\" /> that also implements<br />\n\t///      - {string.Join("<br />\n\t///      - ", additionalInterfaces.Select(x => $"<see cref=\"{x.Class.ClassFullName.EscapeForXmlDoc()}\" />"))}.", "\t");
		sb.Append("\tinternal class ").Append(fileName).Append(" :").AppendLine();
		sb.Append("\t\t").Append(@class.ClassFullName).Append(", ").Append("IMockFor").Append(name).Append(", IMockSetupFor").Append(name);
		if (hasEvents)
		{
			sb.Append(", IMockRaiseOn").Append(name);
		}

		sb.Append(", IMockVerifyFor").Append(name).Append(',').AppendLine();
		foreach ((string Name, Class Class) additional in additionalInterfaces)
		{
			sb.Append("\t\t").Append(additional.Class.ClassFullName).Append(", ").Append("IMockFor").Append(additional.Name).Append(", IMockSetupFor").Append(additional.Name);
			if (additional.Class.AllEvents().Any())
			{
				sb.Append(", IMockRaiseOn").Append(additional.Name);
			}

			sb.Append(", IMockVerifyFor").Append(additional.Name);
			sb.Append(",").AppendLine();
		}

		sb.Append("\t\tglobal::Mockolate.IMock").AppendLine();
		sb.Append("\t{").AppendLine();

		if (@class.IsInterface)
		{
			sb.AppendXmlSummary(
				"The wrapped instance (if any) of the mocked interface to call the base implementation on.");
			sb.Append("\t\tprivate ").Append(@class.ClassFullName).Append("? Wraps { get; }").AppendLine();
		}

		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tobject?[] global::Mockolate.IMock.ConstructorParameters => this.ConstructorParameters;").AppendLine();
		sb.Append("\t\tprivate object?[] ConstructorParameters { get; }").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t/// <inheritdoc />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.MockRegistration global::Mockolate.IMock.Registrations => this.Registrations;").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.MockRegistration Registrations { get; }").AppendLine();
		sb.AppendLine();

		ImplementMockForInterface(sb, name, hasEvents, hasStaticMembers, hasStaticEvents);
		foreach ((string additionalInterfaceName, Class additionalInterface) in additionalInterfaces)
		{
			ImplementMockForInterface(sb, additionalInterfaceName, additionalInterface.AllEvents().Any(x => !x.IsStatic), additionalInterface.AllMethods().Any(x => x.IsStatic) || additionalInterface.AllProperties().Any(x => x.IsStatic), additionalInterface.AllEvents().Any(x => x.IsStatic));
		}

		if (@class.IsInterface)
		{
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(fileName).Append("\" />").AppendLine();
			sb.Append("\t\tpublic ").Append(fileName).Append("(global::Mockolate.MockRegistration registrations, ")
				.Append(@class.ClassFullName).Append("? wraps = null)").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tthis.Registrations = registrations;").AppendLine();
			sb.Append("\t\t\tthis.Wraps = wraps;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}
		else if (constructors is not null)
		{
			foreach (Method constructor in constructors)
			{
				AppendMockSubject_BaseClassConstructor(sb, fileName, constructor);
			}
		}
		else
		{
			sb.Append("\t\t/// <inheritdoc cref=\"").Append(fileName).Append("\" />").AppendLine();
			sb.Append("\t\tpublic ").Append(fileName).Append("(global::Mockolate.MockRegistration registrations)")
				.AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tthis.ConstructorParameters = new object?[0];").AppendLine();
			sb.Append("\t\t\tthis.Registrations = registrations;").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		AppendMockSubject_ImplementClass(sb, @class, null);
		foreach ((string Name, Class Class) item in additionalInterfaces)
		{
			sb.AppendLine();
			AppendMockSubject_ImplementClass(sb, item.Class, @class as MockClass);
		}

		sb.AppendLine();

		#region IMockSetupForXXX

		sb.Append("\t\t#region IMockSetupFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementSetupInterface(sb, @class, name);
		sb.Append("\t\t#endregion IMockSetupFor").Append(name).AppendLine();
		foreach ((string Name, Class Class) item in additionalInterfaces)
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockSetupFor").Append(item.Name).AppendLine();
			sb.AppendLine();
			ImplementSetupInterface(sb, item.Class, item.Name);
			sb.Append("\t\t#endregion IMockSetupFor").Append(item.Name).AppendLine();
		}

		#endregion IMockSetupForXXX

		#region IMockRaiseOnXXX

		if (@class.AllEvents().Any())
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockRaiseOn").Append(name).AppendLine();
			sb.AppendLine();
			ImplementRaiseInterface(sb, @class, name);
			sb.Append("\t\t#endregion IMockRaiseOn").Append(name).AppendLine();
		}
#pragma warning disable S3267 // Loops should be simplified using the "Where" LINQ method
		foreach ((string Name, Class Class) item in additionalInterfaces)
		{
			if (item.Class.AllEvents().Any())
			{
				sb.AppendLine();
				sb.Append("\t\t#region IMockRaiseOn").Append(item.Name).AppendLine();
				sb.AppendLine();
				ImplementRaiseInterface(sb, item.Class, item.Name);
				sb.Append("\t\t#endregion IMockRaiseOn").Append(item.Name).AppendLine();
			}
		}
#pragma warning restore S3267 // Loops should be simplified using the "Where" LINQ method

		#endregion IMockRaiseOnXXX

		#region IMockVerifyForXXX

		sb.AppendLine();
		sb.Append("\t\t#region IMockVerifyFor").Append(name).AppendLine();
		sb.AppendLine();
		ImplementVerifyInterface(sb, @class, name);
		sb.Append("\t\t#endregion IMockVerifyFor").Append(name).AppendLine();
		foreach ((string Name, Class Class) item in additionalInterfaces)
		{
			sb.AppendLine();
			sb.Append("\t\t#region IMockVerifyFor").Append(item.Name).AppendLine();
			sb.AppendLine();
			ImplementVerifyInterface(sb, item.Class, item.Name);
			sb.Append("\t\t#endregion IMockVerifyFor").Append(item.Name).AppendLine();
		}

		#endregion IMockVerifyForXXX

		sb.AppendLine("\t}");

		#endregion MockForXXX

		sb.Append("}").AppendLine();
		sb.AppendLine();
		sb.AppendLine("#nullable disable annotations");
		return sb.ToString();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
