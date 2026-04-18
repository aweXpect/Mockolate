using System.Text;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
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
		          """);
#if !DEBUG
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.AppendLine("""
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
		sb.Append("namespace Mockolate.Interactions").AppendLine();
		sb.Append("{").AppendLine();
		foreach (int count in methodSetups.GroupBy(x => x.Item1).Select(x => x.Key))
		{
			sb.AppendXmlSummary($"An invocation of a method with {count} parameters {string.Join(", ", Enumerable.Range(1, count - 1).Select(x => $"<paramref name=\"parameter{x}\"/>"))} and <paramref name=\"parameter{count}\"/>.", "\t");
			sb.Append("\t[global::System.Diagnostics.DebuggerDisplay(\"{ToString()}\")]").AppendLine();
#if !DEBUG
			sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
			sb.Append("\tpublic class MethodInvocation<")
				.Append(string.Join(", ", Enumerable.Range(1, count).Select(x => $"T{x}"))).Append(">(string name, ")
				.Append(string.Join(", ", Enumerable.Range(1, count).Select(x => $"string parameterName{x}, T{x} parameter{x}")))
				.Append(") : IMethodInteraction").AppendLine();
			sb.Append("\t{").AppendLine();
			sb.AppendXmlSummary("The name of the method.");
			sb.Append("\t\tpublic string Name { get; } = name;").AppendLine();
			for (int i = 1; i <= count; i++)
			{
				string comment = i switch
				{
					1 => "first",
					2 => "second",
					3 => "third",
					_ => $"{i}th",
				};
				sb.AppendXmlSummary($"The {comment} parameter name of the method.");
				sb.Append("\t\tpublic string ParameterName").Append(i).Append(" { get; } = parameterName").Append(i).Append(";").AppendLine();
				sb.AppendXmlSummary($"The {comment} parameter value of the method.");
				sb.Append("\t\tpublic T").Append(i).Append(" Parameter").Append(i).Append(" { get; } = parameter").Append(i).Append(";").AppendLine();
			}

			sb.Append("\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
			sb.Append("\t\tpublic override string ToString()").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\treturn $\"invoke method {SubstringAfterLast(Name, '.')}(")
				.Append(string.Join(", ", Enumerable.Range(1, count).Select(x => $"{{Parameter{x}?.ToString() ?? \"null\"}}"))).Append(")\";").AppendLine();
			sb.Append("\t\t\tstatic string SubstringAfterLast(string name, char c)").AppendLine();
			sb.Append("\t\t\t{").AppendLine();
			sb.Append("\t\t\t\tint index = name.LastIndexOf(c);").AppendLine();
			sb.Append("\t\t\t\treturn index >= 0 ? name.Substring(index + 1) : name;").AppendLine();
			sb.Append("\t\t\t}").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.Append("\t}").AppendLine();
		}

		sb.Append("}").AppendLine();
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

		sb.AppendXmlSummary("Transitions the scenario to the given <paramref name=\"scenario\" /> when the method is called.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupParallelCallbackBuilder<").Append(typeParams).Append("> TransitionTo(string scenario);")
			.AppendLine();
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
			$"Sets up a callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <typeparamref name=\"TReturn\" />.",
			"\t");
		sb.Append("\tinternal interface IVoidMethodSetupCallbackBuilder<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IVoidMethodSetupParallelCallbackBuilder<").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Runs the callback in parallel to the other callbacks.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupParallelCallbackBuilder<").Append(typeParams).Append("> InParallel();")
			.AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <see langword=\"void\" />.",
			"\t");
		sb.Append("\tinternal interface IVoidMethodSetupParallelCallbackBuilder<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Limits the callback to only execute for method invocations where the predicate returns true.");
		sb.AppendXmlRemarks("Provides a zero-based counter indicating how many times the method has been invoked so far.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams).Append("> When(global::System.Func<int, bool> predicate);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a when callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <see langword=\"void\" />.",
			"\t");
		sb.Append("\tinternal interface IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
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
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a return callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <see langword=\"void\" />.",
			"\t");
		sb.Append("\tinternal interface IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary(
			"Limits the throw to only execute for method invocations where the predicate returns true.");
		sb.AppendXmlRemarks(
			"Provides a zero-based counter indicating how many times the method has been invoked so far.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> When(global::System.Func<int, bool> predicate);").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a when return callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <see langword=\"void\" />.",
			"\t");
		sb.Append("\tinternal interface IVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
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
		sb.Append("\t}").AppendLine();
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
			"\t");
#if !DEBUG
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.Append("\t[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]").AppendLine();
		sb.Append("\tinternal abstract class VoidMethodSetup<").Append(typeParams)
			.Append("> : global::Mockolate.Setup.MethodSetup,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append(">").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\t\tprivate readonly global::Mockolate.MockRegistry _mockRegistry;").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.Setup.Callbacks<global::System.Action<int, ").Append(typeParams).Append(">>? _callbacks = [];").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.Setup.Callbacks<global::System.Action<int, ").Append(typeParams).Append(">>? _returnCallbacks = [];").AppendLine();
		sb.Append("\t\tprivate bool? _skipBaseClass;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\tprotected VoidMethodSetup(global::Mockolate.MockRegistry mockRegistry, string name)").AppendLine();
		sb.Append("\t\t\t: base(name)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_mockRegistry = mockRegistry;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// SkippingBaseClass
		sb.AppendXmlSummary("Specifies if calling the base class implementation should be skipped.");
		sb.AppendXmlRemarks("If not specified, use <see cref=\"global::Mockolate.MockBehavior.SkipBaseClass\" />.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append(">.SkippingBaseClass(bool skipBaseClass)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_skipBaseClass = skipBaseClass;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Do(Action)
		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append(">.Do(global::System.Action callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_callbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_callbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Do(Action<T1,...>)
		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append(">.Do(global::System.Action<").Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ")
			.Append(parameters).Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_callbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_callbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Do(Action<int, T1,...>)
		sb.AppendXmlSummary("Registers a <paramref name=\"callback\" /> to execute when the method is called.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append(">.Do(global::System.Action<int, ").Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new(callback);")
			.AppendLine();
		sb.Append("\t\t\t_callbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_callbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// TransitionTo
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetup{").Append(typeParams).Append("}.TransitionTo(string)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupParallelCallbackBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append(">.TransitionTo(string scenario)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ").Append(discards).Append(") => _mockRegistry.TransitionTo(scenario));").AppendLine();
		sb.Append("\t\t\tcurrentCallback.InParallel();").AppendLine();
		sb.Append("\t\t\t_callbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_callbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Returns(Action<int, T1,...>)
		sb.AppendXmlSummary("Registers an iteration in the sequence of method invocations, that does not throw.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append(">.DoesNotThrow()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => { });").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Throws<TException>()
		sb.AppendXmlSummary("Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append(">.Throws<TException>()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw new TException());").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Throws(Exception)
		sb.AppendXmlSummary("Registers an <paramref name=\"exception\" /> to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append(">.Throws(global::System.Exception exception)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw exception);").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Throws(Func<T1,...,Exception>)
		sb.AppendXmlSummary(
			"Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append(">.Throws(global::System.Func<").Append(typeParams).Append(", global::System.Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(parameters).Append(") => throw callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Throws(Func<Exception>)
		sb.AppendXmlSummary(
			"Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.");
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append(">.Throws(global::System.Func<global::System.Exception> callback)")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>((_, ")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw callback());").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// InParallel
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder{").Append(typeParams)
			.Append("}.InParallel()\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupParallelCallbackBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder<").Append(typeParams)
			.Append(">.InParallel()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks?.Active?.InParallel();").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// When (callback)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupCallbackBuilder{").Append(typeParams)
			.Append("}.When(global::System.Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetupParallelCallbackBuilder<").Append(typeParams)
			.Append(">.When(global::System.Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks?.Active?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// For (callback)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder{").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<")
			.Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks?.Active?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Only (callback)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder{").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetupCallbackWhenBuilder<")
			.Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks?.Active?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// When (return)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupReturnBuilder{").Append(typeParams)
			.Append("}.When(global::System.Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetupReturnBuilder<").Append(typeParams)
			.Append(">.When(global::System.Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_returnCallbacks?.Active?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// For (return)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder{").Append(typeParams)
			.Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<")
			.Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_returnCallbacks?.Active?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Only (return)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder{").Append(typeParams)
			.Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams).Append("> global::Mockolate.Setup.IVoidMethodSetupReturnWhenBuilder<")
			.Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_returnCallbacks?.Active?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.MethodSetup.MatchesInteraction(global::Mockolate.Interactions.IMethodInteraction)\" />").AppendLine();
		sb.Append("\t\tprotected override bool MatchesInteraction(global::Mockolate.Interactions.IMethodInteraction interaction)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (interaction is global::Mockolate.Interactions.MethodInvocation<").Append(typeParams).Append("> invocation)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn Matches(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"invocation.ParameterName{i}, invocation.Parameter{i}"))).Append(");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\treturn false;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Gets the flag indicating if the base class implementation should be skipped.");
		sb.Append("\t\tpublic bool SkipBaseClass(global::Mockolate.MockBehavior behavior)").AppendLine();
		sb.Append("\t\t\t=> _skipBaseClass ?? behavior.SkipBaseClass;").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Checks if the given parameters match the setup.");
		sb.Append("\t\tpublic abstract bool Matches(");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			if (i > 1)
			{
				sb.Append(", ");
			}

			sb.Append("string p").Append(i).Append("Name, T").Append(i).Append(" p").Append(i).Append("Value");
		}

		sb.Append(");").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Triggers any configured parameter callbacks for the method setup with the specified parameters.");
		sb.Append("\t\tpublic virtual void TriggerCallbacks(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i} parameter{i}"))).Append(")").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_callbacks is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tbool wasInvoked = false;").AppendLine();
		sb.Append("\t\t\t\tint currentCallbacksIndex = _callbacks.CurrentIndex;").AppendLine();
		sb.Append("\t\t\t\tfor (int i = 0; i < _callbacks.Count; i++)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tvar callback = _callbacks[(currentCallbacksIndex + i) % _callbacks.Count];").AppendLine();
		sb.Append("\t\t\t\t\tif (callback.Invoke(wasInvoked, ref _callbacks.CurrentIndex, Callback))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\twasInvoked = true;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\tif (_returnCallbacks is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tforeach (var _ in _returnCallbacks)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tvar returnCallback = _returnCallbacks[_returnCallbacks.CurrentIndex % _returnCallbacks.Count];").AppendLine();
		sb.Append("\t\t\t\t\tif (returnCallback.Invoke(ref _returnCallbacks.CurrentIndex, Callback))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\treturn;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
		sb.Append("\t\t\tvoid Callback(int invocationCount, Action<int, ").Append(typeParams).Append("> @delegate)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t@delegate(invocationCount, ").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"parameter{x}"))).Append(");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// WithParameters inner class
		sb.Append("\t\t/// <summary>Setup for a method with ").Append(numberOfParameters).Append(" parameter").Append(numberOfParameters > 1 ? "s" : "").Append(" matching against <see cref=\"global::Mockolate.Parameters.IParameters\" />.</summary>").AppendLine();
		sb.Append("\t\tpublic class WithParameters : VoidMethodSetup<").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"VoidMethodSetup{").Append(typeParams).Append("}\" />").AppendLine();
		sb.Append("\t\t\tpublic WithParameters(global::Mockolate.MockRegistry mockRegistry, string name, global::Mockolate.Parameters.IParameters parameters)").AppendLine();
		sb.Append("\t\t\t\t: base(mockRegistry, name)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tParameters = parameters;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\tprivate global::Mockolate.Parameters.IParameters Parameters { get; }").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"VoidMethodSetup{").Append(typeParams).Append("}.Matches(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"string, T{i}"))).Append(")\" />").AppendLine();
		sb.Append("\t\t\tpublic override bool Matches(");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			if (i > 1)
			{
				sb.Append(", ");
			}

			sb.Append("string p").Append(i).Append("Name, T").Append(i).Append(" p").Append(i).Append("Value");
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t\t\t=> Parameters switch").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\tglobal::Mockolate.Parameters.IParametersMatch m => m.Matches([").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"p{i}Value"))).Append("]),").AppendLine();
		sb.Append("\t\t\t\t\t\tglobal::Mockolate.Parameters.INamedParametersMatch m => m.Matches([").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"(p{i}Name, p{i}Value)"))).Append("]),").AppendLine();
		sb.Append("\t\t\t\t\t\t_ => true,").AppendLine();
		sb.Append("\t\t\t\t\t};").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"VoidMethodSetup{").Append(typeParams).Append("}.TriggerCallbacks(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append(")\" />").AppendLine();
		sb.Append("\t\t\tpublic override void TriggerCallbacks(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i} parameter{i}"))).Append(")").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t// No callbacks get triggered for IParameters").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\t\t\tpublic override string ToString()").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn $\"void {SubstringAfterLast(Name, '.')}({Parameters})\";").AppendLine();
		sb.Append("\t\t\t\tstatic string SubstringAfterLast(string name, char c)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tint index = name.LastIndexOf(c);").AppendLine();
		sb.Append("\t\t\t\t\treturn index >= 0 ? name.Substring(index + 1) : name;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine(); // close WithParameters
		sb.AppendLine();

		// WithParameterCollection inner class
		sb.Append("\t\t/// <summary>Setup for a method with ").Append(numberOfParameters).Append(" parameter").Append(numberOfParameters > 1 ? "s" : "").Append(" matching against individual <see cref=\"global::Mockolate.Parameters.IParameterMatch{T}\" />.</summary>").AppendLine();
		sb.Append("\t\tpublic class WithParameterCollection : VoidMethodSetup<").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.IVoidMethodSetupParameterIgnorer<").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tprivate bool _matchAnyParameters;").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"VoidMethodSetup{").Append(typeParams).Append("}\" />").AppendLine();
		sb.Append("\t\t\tpublic WithParameterCollection(").AppendLine();
		sb.Append("\t\t\t\tglobal::Mockolate.MockRegistry mockRegistry,").AppendLine();
		sb.Append("\t\t\t\tstring name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(',').AppendLine().Append("\t\t\t\tglobal::Mockolate.Parameters.IParameterMatch<T").Append(i).Append("> parameter").Append(i);
		}

		sb.Append(')').AppendLine();
		sb.Append("\t\t\t\t: base(mockRegistry, name)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t\tParameter").Append(i).Append(" = parameter").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			string ordinal = i switch { 1 => "first", 2 => "second", 3 => "third", _ => $"{i}th", };
			sb.Append("\t\t\t/// <summary>The ").Append(ordinal).Append(" parameter of the method.</summary>").AppendLine();
			sb.Append("\t\t\tpublic global::Mockolate.Parameters.IParameterMatch<T").Append(i).Append("> Parameter").Append(i).Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IVoidMethodSetupParameterIgnorer{").Append(typeParams).Append("}.AnyParameters()\" />").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.IVoidMethodSetup<").Append(typeParams)
			.Append("> global::Mockolate.Setup.IVoidMethodSetupParameterIgnorer<").Append(typeParams)
			.Append(">.AnyParameters()").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t_matchAnyParameters = true;").AppendLine();
		sb.Append("\t\t\t\treturn this;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"VoidMethodSetup{").Append(typeParams).Append("}.Matches\" />").AppendLine();
		sb.Append("\t\t\tpublic override bool Matches(");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			if (i > 1)
			{
				sb.Append(", ");
			}

			sb.Append("string p").Append(i).Append("Name, T").Append(i).Append(" p").Append(i).Append("Value");
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t\t\t=> _matchAnyParameters || (");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			if (i > 1)
			{
				sb.Append(" && ");
			}

			sb.Append("Parameter").Append(i).Append(".Matches(p").Append(i).Append("Value)");
		}

		sb.Append(");").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"VoidMethodSetup{").Append(typeParams).Append("}.TriggerCallbacks\" />").AppendLine();
		sb.Append("\t\t\tpublic override void TriggerCallbacks(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i} parameter{i}"))).Append(")").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t\tParameter").Append(i).Append("?.InvokeCallbacks(parameter").Append(i).Append(");").AppendLine();
		}

		sb.Append("\t\t\t\tbase.TriggerCallbacks(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"parameter{i}"))).Append(");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\t\t\tpublic override string ToString()").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn $\"void {SubstringAfterLast(Name, '.')}(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"{{Parameter{x}}}")))
			.Append(")\";").AppendLine();
		sb.Append("\t\t\t\tstatic string SubstringAfterLast(string name, char c)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tint index = name.LastIndexOf(c);").AppendLine();
		sb.Append("\t\t\t\t\treturn index >= 0 ? name.Substring(index + 1) : name;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine(); // close WithParameterCollection
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

		sb.AppendXmlSummary("Transitions the scenario to the given <paramref name=\"scenario\" /> when the method is called.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupParallelCallbackBuilder<TReturn, ").Append(typeParams).Append("> TransitionTo(string scenario);")
			.AppendLine();
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
			.Append("> : global::Mockolate.Setup.IReturnMethodSetupParallelCallbackBuilder<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.AppendXmlSummary("Runs the callback in parallel to the other callbacks.");
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupParallelCallbackBuilder<TReturn, ").Append(typeParams).Append("> InParallel();")
			.AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary(
			$"Sets up a parallel callback for a method with {numberOfParameters} parameters {GetTypeParametersDescription(numberOfParameters)} returning <typeparamref name=\"TReturn\" />.",
			"\t");
		sb.Append("\tinternal interface IReturnMethodSetupParallelCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> : global::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams).Append(">")
			.AppendLine();
		sb.Append("\t{").AppendLine();
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
#if !DEBUG
		sb.Append("\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.Append("\t[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]").AppendLine();
		sb.Append("\tinternal abstract class ReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> : global::Mockolate.Setup.MethodSetup,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tprivate readonly global::Mockolate.MockRegistry _mockRegistry;").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.Setup.Callbacks<global::System.Action<int, ").Append(typeParams).Append(">>? _callbacks = [];").AppendLine();
		sb.Append("\t\tprivate global::Mockolate.Setup.Callbacks<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>? _returnCallbacks = [];").AppendLine();
		sb.Append("\t\tprivate bool? _skipBaseClass;").AppendLine();
		sb.AppendLine();

		sb.Append("\t\tprotected ReturnMethodSetup(global::Mockolate.MockRegistry mockRegistry, string name)").AppendLine();
		sb.Append("\t\t\t: base(name)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_mockRegistry = mockRegistry;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// SkippingBaseClass
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.SkippingBaseClass(bool)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.SkippingBaseClass(bool skipBaseClass)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_skipBaseClass = skipBaseClass;").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Do(Action)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Do(global::System.Action)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.Do(global::System.Action callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ").Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_callbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_callbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Do(Action<T1,...>)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Do(global::System.Action{").Append(typeParams).Append("})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.Do(global::System.Action<").Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ").Append(parameters).Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_callbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_callbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Do(Action<int, T1,...>)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Do(global::System.Action{int, ").Append(typeParams).Append("})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.Do(global::System.Action<int, ").Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new(callback);").AppendLine();
		sb.Append("\t\t\t_callbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_callbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// TransitionTo
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.TransitionTo(string)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupParallelCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.TransitionTo(string scenario)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.Callback<global::System.Action<int, ").Append(typeParams).Append(">>? currentCallback = new((_, ").Append(discards).Append(") => _mockRegistry.TransitionTo(scenario));").AppendLine();
		sb.Append("\t\t\tcurrentCallback.InParallel();").AppendLine();
		sb.Append("\t\t\t_callbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_callbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_callbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Returns(Func<T1,...,TReturn>)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Returns(global::System.Func{").Append(typeParams).Append(", TReturn})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.Returns(global::System.Func<").Append(typeParams).Append(", TReturn> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ").Append(parameters).Append(") => callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Returns(Func<TReturn>)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Returns(global::System.Func{TReturn})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.Returns(global::System.Func<TReturn> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ").Append(discards).Append(") => callback());").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Returns(TReturn)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Returns(TReturn)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.Returns(TReturn returnValue)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ").Append(discards).Append(") => returnValue);").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Throws<TException>()
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Throws{TException}()\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.Throws<TException>()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ").Append(discards).Append(") => throw new TException());").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Throws(Exception)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Throws(global::System.Exception)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.Throws(global::System.Exception exception)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ").Append(discards).Append(") => throw exception);").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Throws(Func<T1,...,Exception>)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Throws(global::System.Func{").Append(typeParams).Append(", global::System.Exception})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.Throws(global::System.Func<").Append(typeParams).Append(", global::System.Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ").Append(parameters).Append(") => throw callback(").Append(parameters).Append("));").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Throws(Func<Exception>)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Throws(global::System.Func{global::System.Exception})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append(">.Throws(global::System.Func<global::System.Exception> callback)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tvar currentCallback = new global::Mockolate.Setup.Callback<global::System.Func<int, ").Append(typeParams).Append(", TReturn>>((_, ").Append(discards).Append(") => throw callback());").AppendLine();
		sb.Append("\t\t\t_returnCallbacks ??= [];").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Active = currentCallback;").AppendLine();
		sb.Append("\t\t\t_returnCallbacks.Add(currentCallback);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// InParallel
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupCallbackBuilder{TReturn, ").Append(typeParams).Append("}.InParallel()\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupParallelCallbackBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupCallbackBuilder<TReturn, ").Append(typeParams)
			.Append(">.InParallel()").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks?.Active?.InParallel();").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// When (callback)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupParallelCallbackBuilder{TReturn, ").Append(typeParams).Append("}.When(global::System.Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupParallelCallbackBuilder<TReturn, ").Append(typeParams)
			.Append(">.When(global::System.Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks?.Active?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// For (callback)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder{TReturn, ").Append(typeParams).Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks?.Active?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Only (callback)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder{TReturn, ").Append(typeParams).Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupCallbackWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks?.Active?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// When (return)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupReturnBuilder{TReturn, ").Append(typeParams).Append("}.When(global::System.Func{int, bool})\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupReturnBuilder<TReturn, ").Append(typeParams)
			.Append(">.When(global::System.Func<int, bool> predicate)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_returnCallbacks?.Active?.When(predicate);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// For (return)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder{TReturn, ").Append(typeParams).Append("}.For(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.For(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_returnCallbacks?.Active?.For(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// Only (return)
		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder{TReturn, ").Append(typeParams).Append("}.Only(int)\" />").AppendLine();
		sb.Append("\t\tglobal::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupReturnWhenBuilder<TReturn, ").Append(typeParams)
			.Append(">.Only(int times)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_returnCallbacks?.Active?.Only(times);").AppendLine();
		sb.Append("\t\t\treturn this;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.MethodSetup.MatchesInteraction(global::Mockolate.Interactions.IMethodInteraction)\" />").AppendLine();
		sb.Append("\t\tprotected override bool MatchesInteraction(global::Mockolate.Interactions.IMethodInteraction interaction)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (interaction is global::Mockolate.Interactions.MethodInvocation<").Append(typeParams).Append("> invocation)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn Matches(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"invocation.ParameterName{i}, invocation.Parameter{i}"))).Append(");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\treturn false;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Flag indicating, if any return callbacks have been registered on this setup.");
		sb.Append("\t\tpublic bool HasReturnCallbacks").AppendLine();
		sb.Append("\t\t\t=> _returnCallbacks is { Count: > 0, };").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Gets the flag indicating if the base class implementation should be skipped.");
		sb.Append("\t\tpublic bool SkipBaseClass(global::Mockolate.MockBehavior behavior)").AppendLine();
		sb.Append("\t\t\t=> _skipBaseClass ?? behavior.SkipBaseClass;").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Gets the registered return value.");
		sb.Append("\t\tpublic bool TryGetReturnValue(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i} p{i}"))).Append(", out TReturn returnValue)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_returnCallbacks != null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tforeach (var _ in _returnCallbacks)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tvar returnCallback = _returnCallbacks[_returnCallbacks.CurrentIndex % _returnCallbacks.Count];").AppendLine();
		sb.Append("\t\t\t\t\tif (returnCallback.Invoke<TReturn>(ref _returnCallbacks.CurrentIndex, (invocationCount, @delegate)").AppendLine();
		sb.Append("\t\t\t\t\t\t=> @delegate(invocationCount, ").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append("), out TReturn? newValue))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\treturnValue = newValue;").AppendLine();
		sb.Append("\t\t\t\t\t\treturn true;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\treturnValue = default!;").AppendLine();
		sb.Append("\t\t\treturn false;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Checks if the given parameters match the setup.");
		sb.Append("\t\tpublic abstract bool Matches(");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			if (i > 1)
			{
				sb.Append(", ");
			}

			sb.Append("string p").Append(i).Append("Name, T").Append(i).Append(" p").Append(i).Append("Value");
		}

		sb.Append(");").AppendLine();
		sb.AppendLine();

		sb.AppendXmlSummary("Triggers any configured parameter callbacks for the method setup with the specified parameters.");
		sb.Append("\t\tpublic virtual void TriggerCallbacks(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i} parameter{i}"))).Append(")").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tif (_callbacks is not null)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tbool wasInvoked = false;").AppendLine();
		sb.Append("\t\t\t\tint currentCallbacksIndex = _callbacks.CurrentIndex;").AppendLine();
		sb.Append("\t\t\t\tfor (int i = 0; i < _callbacks.Count; i++)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tvar callback = _callbacks[(currentCallbacksIndex + i) % _callbacks.Count];").AppendLine();
		sb.Append("\t\t\t\t\tif (callback.Invoke(wasInvoked, ref _callbacks.CurrentIndex, Callback))").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\twasInvoked = true;").AppendLine();
		sb.Append("\t\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t\t[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
		sb.Append("\t\t\tvoid Callback(int invocationCount, global::System.Action<int, ").Append(typeParams).Append("> @delegate)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t@delegate(invocationCount, ").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"parameter{x}"))).Append(");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();

		// WithParameters inner class
		sb.Append("\t\t/// <summary>Setup for a method with ").Append(numberOfParameters).Append(" parameter").Append(numberOfParameters > 1 ? "s" : "").Append(" matching against <see cref=\"global::Mockolate.Parameters.IParameters\" />.</summary>").AppendLine();
		sb.Append("\t\tpublic class WithParameters : ReturnMethodSetup<TReturn, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ").Append(typeParams).Append("}\" />").AppendLine();
		sb.Append("\t\t\tpublic WithParameters(global::Mockolate.MockRegistry mockRegistry, string name, global::Mockolate.Parameters.IParameters parameters)").AppendLine();
		sb.Append("\t\t\t\t: base(mockRegistry, name)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tParameters = parameters;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\tprivate global::Mockolate.Parameters.IParameters Parameters { get; }").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Matches(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"string, T{i}"))).Append(")\" />").AppendLine();
		sb.Append("\t\t\tpublic override bool Matches(");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			if (i > 1)
			{
				sb.Append(", ");
			}

			sb.Append("string p").Append(i).Append("Name, T").Append(i).Append(" p").Append(i).Append("Value");
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t\t\t=> Parameters switch").AppendLine();
		sb.Append("\t\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\t\tglobal::Mockolate.Parameters.IParametersMatch m => m.Matches([").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"p{i}Value"))).Append("]),").AppendLine();
		sb.Append("\t\t\t\t\t\tglobal::Mockolate.Parameters.INamedParametersMatch m => m.Matches([").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"(p{i}Name, p{i}Value)"))).Append("]),").AppendLine();
		sb.Append("\t\t\t\t\t\t_ => true,").AppendLine();
		sb.Append("\t\t\t\t\t};").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\t\t\tpublic override string ToString()").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn $\"{FormatType(typeof(TReturn))} {SubstringAfterLast(Name, '.')}({Parameters})\";").AppendLine();
		sb.Append("\t\t\t\tstatic string SubstringAfterLast(string name, char c)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tint index = name.LastIndexOf(c);").AppendLine();
		sb.Append("\t\t\t\t\treturn index >= 0 ? name.Substring(index + 1) : name;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine(); // close WithParameters
		sb.AppendLine();

		// WithParameterCollection inner class
		sb.Append("\t\t/// <summary>Setup for a method with ").Append(numberOfParameters).Append(" parameter").Append(numberOfParameters > 1 ? "s" : "").Append(" matching against individual <see cref=\"global::Mockolate.Parameters.IParameterMatch{T}\" />.</summary>").AppendLine();
		sb.Append("\t\tpublic class WithParameterCollection : ReturnMethodSetup<TReturn, ").Append(typeParams).Append(">,").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.IReturnMethodSetupParameterIgnorer<TReturn, ").Append(typeParams).Append(">").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\tprivate bool _matchAnyParameters;").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ").Append(typeParams).Append("}\" />").AppendLine();
		sb.Append("\t\t\tpublic WithParameterCollection(").AppendLine();
		sb.Append("\t\t\t\tglobal::Mockolate.MockRegistry mockRegistry,").AppendLine();
		sb.Append("\t\t\t\tstring name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(',').AppendLine().Append("\t\t\t\tglobal::Mockolate.Parameters.IParameterMatch<T").Append(i).Append("> parameter").Append(i);
		}

		sb.Append(')').AppendLine();
		sb.Append("\t\t\t\t: base(mockRegistry, name)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t\tParameter").Append(i).Append(" = parameter").Append(i).Append(";").AppendLine();
		}

		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			string ordinal = i switch { 1 => "first", 2 => "second", 3 => "third", _ => $"{i}th", };
			sb.Append("\t\t\t/// <summary>The ").Append(ordinal).Append(" parameter of the method.</summary>").AppendLine();
			sb.Append("\t\t\tpublic global::Mockolate.Parameters.IParameterMatch<T").Append(i).Append("> Parameter").Append(i).Append(" { get; }").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t\t\t/// <inheritdoc cref=\"global::Mockolate.Setup.IReturnMethodSetupParameterIgnorer{TReturn, ").Append(typeParams).Append("}.AnyParameters()\" />").AppendLine();
		sb.Append("\t\t\tglobal::Mockolate.Setup.IReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> global::Mockolate.Setup.IReturnMethodSetupParameterIgnorer<TReturn, ").Append(typeParams)
			.Append(">.AnyParameters()").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t_matchAnyParameters = true;").AppendLine();
		sb.Append("\t\t\t\treturn this;").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.Matches\" />").AppendLine();
		sb.Append("\t\t\tpublic override bool Matches(");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			if (i > 1)
			{
				sb.Append(", ");
			}

			sb.Append("string p").Append(i).Append("Name, T").Append(i).Append(" p").Append(i).Append("Value");
		}

		sb.Append(")").AppendLine();
		sb.Append("\t\t\t\t=> _matchAnyParameters || (");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			if (i > 1)
			{
				sb.Append(" && ");
			}

			sb.Append("Parameter").Append(i).Append(".Matches(p").Append(i).Append("Value)");
		}

		sb.Append(");").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ").Append(typeParams).Append("}.TriggerCallbacks\" />").AppendLine();
		sb.Append("\t\t\tpublic override void TriggerCallbacks(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i} parameter{i}"))).Append(")").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t\t\tParameter").Append(i).Append("?.InvokeCallbacks(parameter").Append(i).Append(");").AppendLine();
		}

		sb.Append("\t\t\t\tbase.TriggerCallbacks(").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"parameter{i}"))).Append(");").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\t\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\t\t\tpublic override string ToString()").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\treturn $\"{FormatType(typeof(TReturn))} {SubstringAfterLast(Name, '.')}(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"{{Parameter{x}}}")))
			.Append(")\";").AppendLine();
		sb.Append("\t\t\t\tstatic string SubstringAfterLast(string name, char c)").AppendLine();
		sb.Append("\t\t\t\t{").AppendLine();
		sb.Append("\t\t\t\t\tint index = name.LastIndexOf(c);").AppendLine();
		sb.Append("\t\t\t\t\treturn index >= 0 ? name.Substring(index + 1) : name;").AppendLine();
		sb.Append("\t\t\t\t}").AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine(); // close WithParameterCollection
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
