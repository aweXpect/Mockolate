using System.Text;

namespace Mockolate.SourceGenerators.Internals;

internal static partial class Sources
{
	public static string MethodSetups(HashSet<(int, bool)> methodSetups)
	{
		StringBuilder sb = InitializeBuilder([
			"System",
			"System.Collections.Generic",
			"System.Threading",
			"Mockolate.Exceptions",
			"Mockolate.Interactions",
		]);

		sb.Append("""
		          namespace Mockolate.Setup;

		          #nullable enable

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

		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	internal static string MethodSetupsActionFunc(IEnumerable<int> parameterCounts)
	{
		StringBuilder sb = InitializeBuilder([]);

		sb.AppendLine("""
		              namespace System;

		              #nullable enable

		              """);
		foreach (int parameterCount in parameterCounts)
		{
			sb.AppendLine($$"""
			                /// <summary>
			                ///     Encapsulates a method that has {{parameterCount}} parameters and does not return a value.
			                /// </summary>
			                public delegate void Action<{{string.Join(", ", Enumerable.Range(1, parameterCount).Select(i => $"in T{i}"))}}>({{string.Join(", ", Enumerable.Range(1, parameterCount).Select(i => $"T{i} arg{i}"))}});
			                """);
			sb.AppendLine();
			sb.AppendLine($$"""
			                /// <summary>
			                ///     Encapsulates a method that has {{parameterCount}} parameters and returns a value of the type specified by the <typeparamref name="TResult" /> parameter.
			                /// </summary>
			                public delegate TResult Func<{{string.Join(", ", Enumerable.Range(1, parameterCount).Select(i => $"in T{i}"))}}, out TResult>({{string.Join(", ", Enumerable.Range(1, parameterCount).Select(i => $"T{i} arg{i}"))}});
			                """);
			sb.AppendLine();
		}

		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendVoidMethodSetup(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"));
		string values = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"v{i}"));
		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <see langword=\"void\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal class VoidMethodSetup<").Append(typeParams).Append("> : MethodSetup").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\tprivate readonly List<Action<").Append(typeParams).Append(">> _callbacks = [];").AppendLine();
		sb.Append("\tprivate readonly List<Action<").Append(typeParams).Append(">> _returnCallbacks = [];")
			.AppendLine();
		sb.Append("\tprivate readonly string _name;").AppendLine();
		sb.Append("\tprivate readonly Match.IParameters? _matches;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\tprivate readonly Match.NamedParameter? _match").Append(i).Append(";").AppendLine();
		}
		sb.Append("\tint _currentReturnCallbackIndex = -1;").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"VoidMethodSetup{").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />").AppendLine();
		sb.Append("\tpublic VoidMethodSetup(").AppendLine();
		sb.Append("\t\tstring name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(',').AppendLine().Append("\t\tMatch.NamedParameter match").Append(i);
		}
		sb.Append(')').AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_name = name;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t_match").Append(i).Append(" = match").Append(i).Append(";").AppendLine();
		}
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"VoidMethodSetup{").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />").AppendLine();
		sb.Append("\tpublic VoidMethodSetup(string name, Match.IParameters matches)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_name = name;").AppendLine();
		sb.Append("\t\t_matches = matches;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic VoidMethodSetup<").Append(typeParams).Append("> Callback(Action callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_callbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => callback());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic VoidMethodSetup<").Append(typeParams).Append("> Callback(Action<").Append(typeParams)
			.Append("> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_callbacks.Add(callback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an iteration in the sequence of method invocations, that does not throw.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic VoidMethodSetup<").Append(typeParams).Append("> DoesNotThrow()")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => { });").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic VoidMethodSetup<").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\twhere TException : Exception, new()").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw new TException());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic VoidMethodSetup<").Append(typeParams).Append("> Throws(Exception exception)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw exception);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic VoidMethodSetup<").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((").Append(values).Append(") => throw callback(").Append(values)
			.Append("));").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic VoidMethodSetup<").Append(typeParams).Append("> Throws(Func<Exception> callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw callback());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)\" />")
			.AppendLine();
		sb.Append("\tprotected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (");
		for (int i = 1; i < numberOfParameters; i++)
		{
			sb.Append("TryCast(invocation.Parameters[").Append(i - 1).Append("], out T").Append(i).Append(" p")
				.Append(i).Append(", behavior) &&");
			sb.AppendLine().Append("\t\t    ");
		}

		sb.Append("TryCast(invocation.Parameters[")
			.Append(numberOfParameters - 1).Append("], out T").Append(numberOfParameters).Append(" p")
			.Append(numberOfParameters).Append(", behavior))")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks.ForEach(callback => callback.Invoke(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append("));")
			.AppendLine();
		sb.Append("\t\t\tif (_returnCallbacks.Count > 0)").AppendLine();
		sb.Append("\t\t\t{").AppendLine();
		sb.Append("\t\t\t\tvar index = Interlocked.Increment(ref _currentReturnCallbackIndex);").AppendLine();
		sb.Append("\t\t\t\tvar returnCallback = _returnCallbacks[index % _returnCallbacks.Count];").AppendLine();
		sb.Append("\t\t\t\treturnCallback(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(");")
			.AppendLine();
		sb.Append("\t\t\t}").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)\" />")
			.AppendLine();
		sb.Append(
				"\tprotected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\twhere TResult : default").AppendLine();
		sb.Append("\t\t=> throw new MockException(\"The method setup does not support return values.\");").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.IsMatch(MethodInvocation)\" />").AppendLine();
		sb.Append("\tprotected override bool IsMatch(MethodInvocation invocation)").AppendLine();
		sb.Append("\t\t=> invocation.Name.Equals(_name) &&").AppendLine();
		sb.Append("\t\t\t(_matches is not null").AppendLine();
		sb.Append("\t\t\t\t? _matches.Matches(invocation.Parameters)").AppendLine();
		sb.Append("\t\t\t\t: Matches([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}!")))
			.Append("], invocation.Parameters));").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetOutParameter{T}(string, MockBehavior)\" />").AppendLine();
		sb.Append("\tprotected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\tHasOutParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out Match.IOutParameter<T>? outParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn outParameter.GetValue(behavior);").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn behavior.DefaultValue.Generate<T>();").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetRefParameter{T}(string, T, MockBehavior)\" />").AppendLine();
		sb.Append("\tprotected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\tHasRefParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out Match.IRefParameter<T>? refParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn refParameter.GetValue(value);").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn value;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\tpublic override string ToString()").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (_matches is not null)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn $\"void {_name}({_matches})\";").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn $\"void {_name}(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"{{_match{x}}}")))
			.Append(")\";").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();
	}

	private static void AppendReturnMethodSetup(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"));
		string values = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"v{i}"));
		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Sets up a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <typeparamref name=\"TReturn\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("internal class ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> : MethodSetup").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\tprivate readonly List<Action<").Append(typeParams).Append(">> _callbacks = [];").AppendLine();
		sb.Append("\tprivate readonly List<Func<").Append(typeParams).Append(", TReturn>> _returnCallbacks = [];")
			.AppendLine();
		sb.Append("\tprivate readonly string _name;").AppendLine();
		sb.Append("\tprivate readonly Match.IParameters? _matches;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\tprivate readonly Match.NamedParameter? _match").Append(i).Append(";").AppendLine();
		}
		sb.Append("\tprivate int _currentReturnCallbackIndex = -1;").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup(").AppendLine();
		sb.Append("\t\tstring name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(',').AppendLine().Append("\t\tMatch.NamedParameter match").Append(i);
		}
		sb.Append(')').AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_name = name;").AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\t_match").Append(i).Append(" = match").Append(i).Append(";").AppendLine();
		}
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"ReturnMethodSetup{TReturn, ").Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"))).Append("}\" />").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup(string name, Match.IParameters matches)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_name = name;").AppendLine();
		sb.Append("\t\t_matches = matches;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Callback(Action callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_callbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => callback());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Callback(Action<")
			.Append(typeParams).Append("> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_callbacks.Add(callback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this method.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Returns(Func<")
			.Append(typeParams).Append(", TReturn> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add(callback);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to setup the return value for this method.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Returns(Func<TReturn> callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => callback());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers the <paramref name=\"returnValue\" /> for this method.").AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Returns(TReturn returnValue)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => returnValue);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <typeparamref name=\"TException\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Throws<TException>()")
			.AppendLine();
		sb.Append("\t\twhere TException : Exception, new()").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw new TException());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Throws(Exception exception)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw exception);").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((").Append(values).Append(") => throw callback(").Append(values)
			.Append("));").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <summary>").AppendLine();
		sb.Append(
				"\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams)
			.Append("> Throws(Func<Exception> callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw callback());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)\" />")
			.AppendLine();
		sb.Append("\tprotected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (");
		for (int i = 1; i < numberOfParameters; i++)
		{
			sb.Append("TryCast(invocation.Parameters[").Append(i - 1).Append("], out T").Append(i).Append(" p")
				.Append(i).Append(", behavior) &&");
			sb.AppendLine().Append("\t\t    ");
		}

		sb.Append("TryCast(invocation.Parameters[")
			.Append(numberOfParameters - 1).Append("], out T").Append(numberOfParameters).Append(" p")
			.Append(numberOfParameters).Append(", behavior))")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callbacks.ForEach(callback => callback.Invoke(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append("));")
			.AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)\" />")
			.AppendLine();
		sb.Append(
				"\tprotected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t\twhere TResult : default").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (_returnCallbacks.Count == 0)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn behavior.DefaultValue.Generate<TResult>();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\tif (!TryCast(invocation.Parameters[").Append(i - 1)
				.Append("], out T").Append(i).Append(" p").Append(i).Append(", behavior))").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tthrow new MockException($\"The input parameter ").Append(i)
				.Append(" only supports '{FormatType(typeof(T").Append(i)
				.Append("))}', but is '{FormatType(invocation.Parameters[")
				.Append(i - 1).Append("]!.GetType())}'.\");").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t\tvar index = Interlocked.Increment(ref _currentReturnCallbackIndex);").AppendLine();
		sb.Append("\t\tvar returnCallback = _returnCallbacks[index % _returnCallbacks.Count];").AppendLine();
		sb.Append("\t\tTReturn returnValue = returnCallback(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}")))
			.Append(");").AppendLine();
		sb.Append("\t\tif (returnValue is null)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn default!;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\tif (returnValue is TResult result)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn result;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append(
				"\t\tthrow new MockException($\"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.\");")
			.AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.IsMatch(MethodInvocation)\" />").AppendLine();
		sb.Append("\tprotected override bool IsMatch(MethodInvocation invocation)").AppendLine();
		sb.Append("\t\t=> invocation.Name.Equals(_name) &&").AppendLine();
		sb.Append("\t\t\t(_matches is not null").AppendLine();
		sb.Append("\t\t\t\t? _matches.Matches(invocation.Parameters)").AppendLine();
		sb.Append("\t\t\t\t: Matches([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}!")))
			.Append("], invocation.Parameters));").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetOutParameter{T}(string, MockBehavior)\" />").AppendLine();
		sb.Append("\tprotected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\tHasOutParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out Match.IOutParameter<T>? outParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn outParameter.GetValue(behavior);").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn behavior.DefaultValue.Generate<T>();").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetRefParameter{T}(string, T, MockBehavior)\" />").AppendLine();
		sb.Append("\tprotected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (")
			.Append(string.Join(" && ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x} is not null")))
			.Append(" &&").AppendLine();
		sb.Append("\t\t\tHasRefParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"_match{x}")))
			.Append("], parameterName, out Match.IRefParameter<T>? refParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn refParameter.GetValue(value);").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn value;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();

		sb.Append("\t/// <inheritdoc cref=\"object.ToString()\" />").AppendLine();
		sb.Append("\tpublic override string ToString()").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (_matches is not null)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn $\"{FormatType(typeof(TReturn))} {_name}({_matches})\";").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn $\"{FormatType(typeof(TReturn))} {_name}(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"{{_match{x}}}")))
			.Append(")\";").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();
	}
}
