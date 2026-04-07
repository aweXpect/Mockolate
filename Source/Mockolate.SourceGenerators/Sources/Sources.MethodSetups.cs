using System.Text;

namespace Mockolate.SourceGenerators.Sources;

internal static partial class Sources
{
	public static string MethodSetups(HashSet<(int, bool)> methodSetups)
	{
		StringBuilder sb = InitializeBuilder();

		sb.Append("""
		          #nullable enable

		          namespace Mockolate.Setup
		          {
		          """);
		foreach ((int, bool) item in methodSetups)
		{
			sb.AppendLine();
			if (item.Item2)
			{
				AppendVoidMethodSetup(sb, item.Item1);
			}
			else
			{
				AppendReturnMethodSetup(sb, item.Item1);
			}
		}
		sb.Append("""
		          }

		          namespace Mockolate
		          {
		          	[global::System.Diagnostics.DebuggerNonUserCode]
		          	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		          	internal static class MethodSetupExtensions
		          	{
		          """).AppendLine();
		foreach ((int, bool) item in methodSetups)
		{
			string genericParameters = GetGenericTypeParameters(item.Item1);
			if (!item.Item2)
			{
				genericParameters = "TReturn, " + genericParameters;
			}

			string xmlDocSummary = item.Item2 ? "returning void" : "returning <typeparamref name=\"TReturn\"/>";
			string typePrefix = item.Item2
				? "global::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder"
				: "global::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder";
			string setupTypePrefix = item.Item2
				? "global::Mockolate.Setup.IVoidMethodSetup"
				: "global::Mockolate.Setup.IReturnMethodSetup";
			string setupCallbackPrefix = item.Item2
				? "global::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder"
				: "global::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder";
			sb.Append($$"""
			            		/// <summary>
			            		///     Extensions for method callback setup {{xmlDocSummary}} with {{item.Item1}} parameters.
			            		/// </summary>
			            		extension<{{genericParameters}}>({{setupCallbackPrefix}}<{{genericParameters}}> setup)
			            		{
			            			/// <summary>
			            			///     Executes the callback only once.
			            			/// </summary>
			            			public {{setupTypePrefix}}<{{genericParameters}}> OnlyOnce()
			            				=> setup.Only(1);
			            		}
			            		
			            		/// <summary>
			            		///     Extensions for method setup {{xmlDocSummary}} with {{item.Item1}} parameters.
			            		/// </summary>
			            		extension<{{genericParameters}}>({{typePrefix}}<{{genericParameters}}> setup)
			            		{
			            			/// <summary>
			            			///     Returns/throws forever.
			            			/// </summary>
			            			public void Forever()
			            			{
			            				setup.For(int.MaxValue);
			            			}

			            			/// <summary>
			            			///     Uses the return value only once.
			            			/// </summary>
			            			public {{setupTypePrefix}}<{{genericParameters}}> OnlyOnce()
			            				=> setup.Only(1);
			            		}
			            """).AppendLine();
		}

		sb.Append("""
		          	}
		          }
		          """).AppendLine();
		sb.AppendLine();
		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendVoidMethodSetup(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = GetGenericTypeParameters(numberOfParameters);
		string parameters = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"p{i}"));
		string discards = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(_ => "_"));

		sb.AppendXmlSummary(
			$"Sets up a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <see langword=\"void\" />.",
			"\t");
		sb.Append("\tinternal interface IVoidMethodSetup<").Append(typeParams).Append("> : IMethodSetup").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Specifies if calling the base class implementation should be skipped.");
		sb.AppendXmlRemarks("If not specified, use <see cref=\"global::Mockolate.MockBehavior.SkipBaseClass\" />.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append("> SkippingBaseClass(bool skipBaseClass = true);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(global::System.Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(global::System.Action<").Append(typeParams)
			.Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(global::System.Action<int, ")
			.Append(typeParams)
			.Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an iteration in the sequence of method invocations, that does not throw.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> DoesNotThrow();").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : global::System.Exception, new();").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an <paramref name=\"exception\" /> to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(global::System.Exception exception);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			"Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(global::System.Func<")
			.Append(typeParams).Append(", global::System.Exception> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			"Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(global::System.Func<global::System.Exception> callback);")
			.AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <see langword=\"void\" />.",
			"");
		sb.Append("internal interface IVoidMethodSetupCallbackBuilder<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.AppendXmlSummary("Runs the callback in parallel to the other callbacks.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> InParallel();").AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary(
			"Limits the callback to only execute for method invocations where the predicate returns true.");
		sb.AppendXmlRemarks(
			"Provides a zero-based counter indicating how many times the method has been invoked so far.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> When(global::System.Func<int, bool> predicate);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a when callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <see langword=\"void\" />.",
			"");
		sb.Append("internal interface IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.AppendXmlSummary("Repeats the callback for the given number of <paramref name=\"times\" />.");
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder{")
			.Append(typeParams).Append("}.When(global::System.Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams).Append("> For(int times);")
			.AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Deactivates the callback after the given number of <paramref name=\"times\" />.");
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder{")
			.Append(typeParams).Append("}.When(global::System.Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append("> Only(int times);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();


		sb.AppendXmlSummary(
			$"Sets up a return callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <see langword=\"void\" />.",
			"");
		sb.Append("internal interface IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.AppendXmlSummary(
			"Limits the throw to only execute for method invocations where the predicate returns true.");
		sb.AppendXmlRemarks(
			"Provides a zero-based counter indicating how many times the method has been invoked so far.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> When(global::System.Func<int, bool> predicate);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a when return callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <see langword=\"void\" />.",
			"");
		sb.Append("internal interface IVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.AppendXmlSummary("Repeats the throw for the given number of <paramref name=\"times\" />.");
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"global::Mockolate.Setup.IVoidMethodSetupReturnBuilder{")
			.Append(typeParams).Append("}.When(global::System.Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<").Append(typeParams).Append("> For(int times);").AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Deactivates the throw after the given number of <paramref name=\"times\" />.");
		sb.Append("\t\t/// <remarks>").AppendLine();
		sb.Append(
				"\t\t///     The number of times is only counted for actual executions (<see cref=\"global::Mockolate.Setup.IVoidMethodSetupReturnBuilder{")
			.Append(typeParams).Append("}.When(global::System.Func{int, bool})\" /> evaluates to <see langword=\"true\" />).")
			.AppendLine();
		sb.Append("\t\t/// </remarks>").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append("> Only(int times);").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Allows ignoring the provided parameters.", "\t");
		sb.Append("\tinternal interface IVoidMethodSetupParameterIgnorer<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Replaces the explicit parameter matcher with <see cref=\"Match.AnyParameters()\" />.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append("> AnyParameters();").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <see langword=\"void\" />.",
			"");
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
		sb.Append("\t[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]").AppendLine();
		sb.Append("internal class VoidMethodSetup<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.MethodSetup,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupParameterIgnorer<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\tprivate readonly global::System.Collections.Generic.List<global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>> _callbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly global::System.Collections.Generic.List<global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams)
			.Append(">>> _returnCallbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly string _name;").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.Parameters.IParameters? _matches;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\tprivate readonly global::Mockolate.Parameters.NamedParameter? _match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\tprivate bool? _skipBaseClass;").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.Setup.Callback? _currentCallback;").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.Setup.Callback? _currentReturnCallback;").AppendLine();
		sb.Append("\t\tprivate int _currentCallbacksIndex;").AppendLine();
		sb.Append("\t\tprivate int _currentReturnCallbackIndex;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.VoidMethodSetup{")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\t\tpublic VoidMethodSetup(").AppendLine();
		sb.Append("\t\t\tstring name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(',').AppendLine().Append("\t\tglobal::Mockolate.Parameters.NamedParameter match").Append(i);
		}

		sb.Append(')').AppendLine();
		sb.Append("\t\t\t: base(new global::Mockolate.Setup.MethodParameterMatch(name, [")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"match{i}"))).Append("]))")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_name = name;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t_match").Append(i).Append(" = match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.VoidMethodSetup{")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\t\tpublic VoidMethodSetup(string name, global::Mockolate.Parameters.IParameters matches)").AppendLine();
		sb.Append("\t\t\t: base(new global::Mockolate.Setup.MethodParametersMatch(name, matches))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_name = name;").AppendLine();
		sb.Append("\t\t\t_matches = matches;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Specifies if calling the base class implementation should be skipped.");
		sb.AppendXmlRemarks("If not specified, use <see cref=\"global::Mockolate.MockBehavior.SkipBaseClass\" />.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append("> SkippingBaseClass(bool skipBaseClass = true)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_skipBaseClass = skipBaseClass;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(global::System.Action callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(global::System.Action<")
			.Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(parameters).Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> Do(global::System.Action<int, ")
			.Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an iteration in the sequence of method invocations, that does not throw.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> DoesNotThrow()")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => { });").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : global::System.Exception, new()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw new TException());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an <paramref name=\"exception\" /> to throw when the method is invoked.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append("> Throws(global::System.Exception exception)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw exception);").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			"Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> Throws(global::System.Func<")
			.Append(typeParams).Append(", global::System.Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(parameters).Append(") => throw callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			"Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append("> Throws(global::System.Func<global::System.Exception> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw callback());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder{").Append(typeParams)
			.Append("}.InParallel()\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams)
			.Append(">.InParallel()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.InParallel();").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder{").Append(typeParams)
			.Append("}.When(global::System.Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams)
			.Append(">.When(global::System.Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder{").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<")
			.Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder{").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<")
			.Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupReturnBuilder{").Append(typeParams)
			.Append("}.When(global::System.Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append(">.When(global::System.Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder{").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<")
			.Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder{").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<")
			.Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupParameterIgnorer{").Append(typeParams).Append("}.AnyParameters()\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetupParameterIgnorer<")
			.Append(typeParams)
			.Append(">.AnyParameters()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_matches = global::Mockolate.Match.AnyParameters();").AppendLine();
		sb.Append("\t\t\tMethodMatch = new global::Mockolate.Setup.MethodParametersMatch(_name, _matches);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.ExecuteCallback(global::Mockolate.Interactions.MethodInvocation, global::Mockolate.MockBehavior)\" />")
			.AppendLine();
		sb.Append("\t\tprotected override void ExecuteCallback(global::Mockolate.Interactions.MethodInvocation invocation, global::Mockolate.MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (");
		for (int i = 1; i < numberOfParameters; i++)
		{
			sb.Append("invocation.Parameters[").Append(i - 1).Append("].TryGetValue(out T").Append(i).Append(" p")
				.Append(i).Append(") &&");
			sb.AppendLine().Append("\t\t    ");
		}

		sb.Append("invocation.Parameters[")
			.Append(numberOfParameters - 1).Append("].TryGetValue(out T").Append(numberOfParameters).Append(" p")
			.Append(numberOfParameters).Append("))")
			.AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tbool wasInvoked = false;").AppendLine();
		sb.Append("\t\t\t\tint currentCallbacksIndex = _currentCallbacksIndex;").AppendLine();
		sb.Append("\t\t\t\tfor (int i = 0; i < _callbacks.Count; i++)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? callback =").AppendLine();
		sb.Append("\t\t\t\t\t\t_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];").AppendLine();
		sb.Append("\t\t\t\t\tif (callback.Invoke(wasInvoked, ref _currentCallbacksIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(")))")
			.AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\twasInvoked = true;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t\tforeach (var _ in _returnCallbacks)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\t\tvar returnCallback = _returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];")
			.AppendLine();
		sb.Append("\t\t\t\t\tif (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(")))")
			.AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\treturn;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append(
				"\t\t/// <inheritdoc cref=\"MethodSetup.GetReturnValue{TResult}(global::Mockolate.Interactions.MethodInvocation, global::Mockolate.MockBehavior, global::System.Func{TResult})\" />")
			.AppendLine();
		sb.Append(
				"\t\tprotected override TResult GetReturnValue<TResult>(global::Mockolate.Interactions.MethodInvocation invocation, global::Mockolate.MockBehavior behavior, global::System.Func<TResult> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t\t\twhere TResult : default").AppendLine();
		sb.Append("\t\t\t=> throw new global::Mockolate.Exceptions.MockException(\"The method setup does not support return values.\");")
			.AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.TriggerParameterCallbacks(object?[])\" />").AppendLine();
		sb.Append("\t\tprotected override void TriggerParameterCallbacks(object?[] parameters)").AppendLine();
		sb.Append("\t\t\t=> TriggerCallbacks([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameters);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.GetSkipBaseClass()\" />").AppendLine();
		sb.Append("\t\tprotected override bool? GetSkipBaseClass()").AppendLine();
		sb.Append("\t\t\t=> _skipBaseClass;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.HasReturnCalls()\" />").AppendLine();
		sb.Append("\t\tprotected override bool HasReturnCalls()").AppendLine();
		sb.Append("\t\t\t=> _returnCallbacks.Count > 0;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.SetOutParameter{T}(string, global::System.Func{T})\" />").AppendLine();
		sb.Append("\t\tprotected override T SetOutParameter<T>(string parameterName, global::System.Func<T> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (HasOutParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out global::Mockolate.Parameters.IOutParameter<T>? outParameter))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn outParameter.GetValue(defaultValueGenerator);").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn defaultValueGenerator();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.SetRefParameter{T}(string, T, global::Mockolate.MockBehavior)\" />")
			.AppendLine();
		sb.Append("\t\tprotected override T SetRefParameter<T>(string parameterName, T value, global::Mockolate.MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (HasRefParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out global::Mockolate.Parameters.IRefParameter<T>? refParameter))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn refParameter.GetValue(value);").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn value;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\t\tpublic override string ToString()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_matches is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn $\"void {SubstringAfterLast(_name, '.')}({_matches})\";").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn $\"void {SubstringAfterLast(_name, '.')}(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"{{_match{x}}}")))
			.Append(")\";").AppendLine();
		sb.Append("\t\t\tstatic string SubstringAfterLast(string name, char c)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tint index = name.LastIndexOf(c);").AppendLine();
		sb.Append("\t\t\t\tif (index >= 0)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\treturn name.Substring(index + 1);").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\treturn name;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
	}

	private static void AppendReturnMethodSetup(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"));
		string parameters = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"p{i}"));
		string discards = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(_ => "_"));

		sb.AppendXmlSummary(
			$"Sets up a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <typeparamref name=\"TReturn\" />.",
			"\t");
		sb.Append("\tinternal interface IReturnMethodSetup<TReturn, ").Append(typeParams).Append("> : global::Mockolate.Setup.IMethodSetup")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Specifies if calling the base class implementation should be skipped.");
		sb.AppendXmlRemarks("If not specified, use <see cref=\"global::Mockolate.MockBehavior.SkipBaseClass\" />.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> SkippingBaseClass(bool skipBaseClass = true);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(global::System.Action callback);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(global::System.Action<")
			.Append(typeParams).Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(global::System.Action<int, ")
			.Append(typeParams).Append("> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to setup the return value for this method.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Returns(global::System.Func<")
			.Append(typeParams).Append(", TReturn> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to setup the return value for this method.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Returns(global::System.Func<TReturn> callback);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers the <paramref name=\"returnValue\" /> for this method.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Returns(TReturn returnValue);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : global::System.Exception, new();").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an <paramref name=\"exception\" /> to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Throws(global::System.Exception exception);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			"Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws(global::System.Func<")
			.Append(typeParams).Append(", global::System.Exception> callback);").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			"Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Throws(global::System.Func<global::System.Exception> callback);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <typeparamref name=\"TReturn\" />.",
			"\t");
		sb.Append("\tinternal interface IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Runs the callback in parallel to the other callbacks.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> InParallel();")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			"Limits the callback to only execute for method invocations where the predicate returns true.");
		sb.AppendXmlRemarks(
			"Provides a zero-based counter indicating how many times the method has been invoked so far.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> When(global::System.Func<int, bool> predicate);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a when callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <typeparamref name=\"TReturn\" />.",
			"\t");
		sb.Append("\tinternal interface IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("\t{").AppendLine();

		sb.AppendXmlSummary("Repeats the callback for the given number of <paramref name=\"times\" />.");
		sb.AppendXmlRemarks(
			$"The number of times is only counted for actual executions (<see cref=\"global::Mockolate.Setup.IReturnMethodSetupCallbackBuilder{{TReturn, {typeParams}}}.When(global::System.Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams).Append("> For(int times);")
			.AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Deactivates the callback after the given number of <paramref name=\"times\" />.");
		sb.AppendXmlRemarks(
			$"The number of times is only counted for actual executions (<see cref=\"global::Mockolate.Setup.IReturnMethodSetupCallbackBuilder{{TReturn, {typeParams}}}.When(global::System.Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Only(int times);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a return callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <typeparamref name=\"TReturn\" />.",
			"\t");
		sb.Append("\tinternal interface IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("\t{").AppendLine();

		sb.AppendXmlSummary(
			"Limits the return/throw to only execute for method invocations where the predicate returns true.");
		sb.AppendXmlRemarks(
			"Provides a zero-based counter indicating how many times the method has been invoked so far.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> When(global::System.Func<int, bool> predicate);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a when return callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <typeparamref name=\"TReturn\" />.",
			"\t");
		sb.Append("\tinternal interface IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("\t{").AppendLine();

		sb.AppendXmlSummary("Repeats the callback for the given number of <paramref name=\"times\" />.");
		sb.AppendXmlRemarks(
			$"The number of times is only counted for actual executions (<see cref=\"global::Mockolate.Setup.IReturnMethodSetupReturnBuilder{{TReturn, {typeParams}}}.When(global::System.Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams).Append("> For(int times);")
			.AppendLine();
		sb.AppendLine();
		sb.AppendXmlSummary("Deactivates the return/throw after the given number of <paramref name=\"times\" />.");
		sb.AppendXmlRemarks(
			$"The number of times is only counted for actual executions (<see cref=\"global::Mockolate.Setup.IReturnMethodSetupReturnBuilder{{TReturn, {typeParams}}}.When(global::System.Func{{int, bool}})\" /> evaluates to <see langword=\"true\" />).");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Only(int times);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Allows ignoring the provided parameters.", "\t");
		sb.Append("\tinternal interface IReturnMethodSetupParameterIgnorer<TReturn, ").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Replaces the explicit parameter matcher with <see cref=\"Match.AnyParameters()\" />.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams).Append("> AnyParameters();").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <typeparamref name=\"TReturn\" />.",
			"\t");
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
		sb.Append("\t[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]").AppendLine();
		sb.Append("\tinternal class ReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> : global::Mockolate.Setup.MethodSetup,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupParameterIgnorer<TReturn, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tprivate readonly global::System.Collections.Generic.List<global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>> _callbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly global::System.Collections.Generic.List<global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams)
			.Append(", TReturn>>> _returnCallbacks = [];")
			.AppendLine();
		sb.Append("\t\tprivate readonly string _name;").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.Parameters.IParameters? _matches;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\tprivate readonly global::Mockolate.Parameters.NamedParameter? _match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\tprivate bool? _skipBaseClass;").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.Setup.Callback? _currentCallback;").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.Setup.Callback? _currentReturnCallback;").AppendLine();
		sb.Append("\t\tprivate int _currentCallbacksIndex;").AppendLine();
		sb.Append("\t\tprivate int _currentReturnCallbackIndex;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\t\tpublic ReturnMethodSetup(").AppendLine();
		sb.Append("\t\t\t\tstring name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(',').AppendLine().Append("\t\t\tglobal::Mockolate.Parameters.NamedParameter match").Append(i);
		}

		sb.Append(')').AppendLine();
		sb.Append("\t\t\t: base(new global::Mockolate.Setup.MethodParameterMatch(name, [")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"match{i}"))).Append("]))")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_name = name;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t_match").Append(i).Append(" = match").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />")
			.AppendLine();
		sb.Append("\t\tpublic ReturnMethodSetup(string name, global::Mockolate.Parameters.IParameters matches)").AppendLine();
		sb.Append("\t\t\t: base(new global::Mockolate.Setup.MethodParametersMatch(name, matches))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_name = name;").AppendLine();
		sb.Append("\t\t\t_matches = matches;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Specifies if calling the base class implementation should be skipped.");
		sb.AppendXmlRemarks("If not specified, use <see cref=\"global::Mockolate.MockBehavior.SkipBaseClass\" />.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> SkippingBaseClass(bool skipBaseClass = true)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_skipBaseClass = skipBaseClass;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> Do(global::System.Action callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append("> Do(global::System.Action<")
			.Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(parameters).Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> Do(global::System.Action<int, ")
			.Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t\t_currentCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to setup the return value for this method.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Returns(global::System.Func<")
			.Append(typeParams).Append(", TReturn> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(parameters)
			.Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to setup the return value for this method.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Returns(global::System.Func<TReturn> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers the <paramref name=\"returnValue\" /> for this method.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Returns(TReturn returnValue)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => returnValue);").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\t\twhere TException : global::System.Exception, new()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw new TException());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Registers an <paramref name=\"exception\" /> to throw when the method is invoked.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Throws(global::System.Exception exception)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw exception);").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			"Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append("> Throws(global::System.Func<")
			.Append(typeParams).Append(", global::System.Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(parameters)
			.Append(") => throw callback(").Append(parameters)
			.Append("));").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			"Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.");
		sb.Append("\t\tpublic global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> Throws(global::System.Func<global::System.Exception> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw callback());").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupCallbackBuilder{TReturn, ").Append(typeParams)
			.Append("}.InParallel()\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append(">.InParallel()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.InParallel();").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupCallbackBuilder{TReturn, ").Append(typeParams)
			.Append("}.When(global::System.Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append(">.When(global::System.Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentCallback?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupReturnBuilder{TReturn, ").Append(typeParams)
			.Append("}.When(global::System.Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append(">.When(global::System.Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder{TReturn, ").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_currentReturnCallback?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupParameterIgnorer{TReturn, ").Append(typeParams).Append("}.AnyParameters()\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams).Append("> global::Mockolate.Setup.IReturnMethodSetupParameterIgnorer<TReturn, ")
			.Append(typeParams)
			.Append(">.AnyParameters()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_matches = global::Mockolate.Match.AnyParameters();").AppendLine();
		sb.Append("\t\t\tMethodMatch = new global::Mockolate.Setup.MethodParametersMatch(_name, _matches);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.ExecuteCallback(global::Mockolate.Interactions.MethodInvocation, global::Mockolate.MockBehavior)\" />")
			.AppendLine();
		sb.Append("\t\tprotected override void ExecuteCallback(global::Mockolate.Interactions.MethodInvocation invocation, global::Mockolate.MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (");
		for (int i = 1; i < numberOfParameters; i++)
		{
			sb.Append("invocation.Parameters[").Append(i - 1).Append("].TryGetValue(out T").Append(i).Append(" p")
				.Append(i).Append(") &&");
			sb.AppendLine().Append("\t\t    ");
		}

		sb.Append("invocation.Parameters[")
			.Append(numberOfParameters - 1).Append("].TryGetValue(out T").Append(numberOfParameters).Append(" p")
			.Append(numberOfParameters).Append("))")
			.AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tbool wasInvoked = false;").AppendLine();
		sb.Append("\t\t\t\tint currentCallbacksIndex = _currentCallbacksIndex;").AppendLine();
		sb.Append("\t\t\t\tfor (int i = 0; i < _callbacks.Count; i++)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? callback =").AppendLine();
		sb.Append("\t\t\t\t\t\t_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];").AppendLine();
		sb.Append("\t\t\t\t\tif (callback.Invoke(wasInvoked, ref _currentCallbacksIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(")))")
			.AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\twasInvoked = true;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append(
				"\t\t/// <inheritdoc cref=\"MethodSetup.GetReturnValue{TResult}(global::Mockolate.Interactions.MethodInvocation, global::Mockolate.MockBehavior, global::System.Func{TResult})\" />")
			.AppendLine();
		sb.Append(
				"\t\tprotected override TResult GetReturnValue<TResult>(global::Mockolate.Interactions.MethodInvocation invocation, global::Mockolate.MockBehavior behavior, global::System.Func<TResult> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t\t\twhere TResult : default").AppendLine();
		sb.Append("\t\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\tif (!invocation.Parameters[").Append(i - 1)
				.Append("].TryGetValue(out T").Append(i).Append(" p").Append(i).Append("))").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tthrow new global::Mockolate.Exceptions.MockException($\"The input parameter ").Append(i)
				.Append(" only supports '{FormatType(typeof(T").Append(i)
				.Append("))}', but is '{FormatType(invocation.Parameters[")
				.Append(i - 1).Append("].GetValueType())}'.\");").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t\t\tforeach (var _ in _returnCallbacks)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\tvar returnCallback = _returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];")
			.AppendLine();
		sb.Append(
				"\t\t\t\tif (returnCallback.Invoke<TReturn>(ref _currentReturnCallbackIndex, (invocationCount, @delegate)")
			.AppendLine();
		sb.Append("\t\t\t\t\t=> @delegate(invocationCount, ")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}")))
			.Append("), out TReturn? newValue))").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tif (newValue is null)").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\treturn default!;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t\t\tif (!TryCast(newValue, out TResult returnValue, behavior))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append(
				"\t\t\t\t\t\tthrow new global::Mockolate.Exceptions.MockException($\"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.\");")
			.AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t\t\treturn returnValue;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn defaultValueGenerator();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.TriggerParameterCallbacks(object?[])\" />").AppendLine();
		sb.Append("\t\tprotected override void TriggerParameterCallbacks(object?[] parameters)").AppendLine();
		sb.Append("\t\t\t=> TriggerCallbacks([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameters);").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.GetSkipBaseClass()\" />").AppendLine();
		sb.Append("\t\tprotected override bool? GetSkipBaseClass()").AppendLine();
		sb.Append("\t\t\t=> _skipBaseClass;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.HasReturnCalls()\" />").AppendLine();
		sb.Append("\t\tprotected override bool HasReturnCalls()").AppendLine();
		sb.Append("\t\t\t=> _returnCallbacks.Count > 0;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.SetOutParameter{T}(string, global::System.Func{T})\" />").AppendLine();
		sb.Append("\t\tprotected override T SetOutParameter<T>(string parameterName, global::System.Func<T> defaultValueGenerator)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (HasOutParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out global::Mockolate.Parameters.IOutParameter<T>? outParameter))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn outParameter.GetValue(defaultValueGenerator);").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn defaultValueGenerator();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"MethodSetup.SetRefParameter{T}(string, T, global::Mockolate.MockBehavior)\" />")
			.AppendLine();
		sb.Append("\t\tprotected override T SetRefParameter<T>(string parameterName, T value, global::Mockolate.MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (HasRefParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out global::Mockolate.Parameters.IRefParameter<T>? refParameter))").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn refParameter.GetValue(value);").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn value;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\t\tpublic override string ToString()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_matches is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn $\"{FormatType(typeof(TReturn))} {SubstringAfterLast(_name, '.')}({_matches})\";").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\treturn $\"{FormatType(typeof(TReturn))} {SubstringAfterLast(_name, '.')}(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"{{_match{x}}}")))
			.Append(")\";").AppendLine();
		sb.Append("\t\t\tstatic string SubstringAfterLast(string name, char c)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tint index = name.LastIndexOf(c);").AppendLine();
		sb.Append("\t\t\t\tif (index >= 0)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\treturn name.Substring(index + 1);").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\treturn name;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
	}
}
