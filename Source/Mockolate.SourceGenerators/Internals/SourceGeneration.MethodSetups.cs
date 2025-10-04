using System.Text;

namespace Mockolate.SourceGenerators.Internals;

internal static partial class SourceGeneration
{
	public static string GetMethodSetups(HashSet<(int, bool)> methodSetups)
	{
		StringBuilder sb = new();
		sb.AppendLine(Header);
		sb.Append("""
		          using System;
		          using System.Collections.Generic;
		          using System.Threading;
		          using Mockolate.Checks;
		          using Mockolate.Exceptions;

		          namespace Mockolate.Setup;

		          #nullable enable

		          """);
		foreach ((int, bool) item in methodSetups)
		{
			sb.AppendLine();
			if (item.Item2)
			{
				AppendVoidMethod(sb, item.Item1);
			}
			else
			{
				AppendReturnMethod(sb, item.Item1);
			}
		}

		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}

	private static void AppendVoidMethod(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"));
		string values = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"v{i}"));
		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Setup for a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <see langword=\"void\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("public class VoidMethodSetup<").Append(typeParams).Append(">(string name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(", With.NamedParameter match").Append(i);
		}

		sb.Append(") : MethodSetup").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\tprivate Action<").Append(typeParams).Append(">? _callback;").AppendLine();
		sb.Append("\tprivate List<Action<").Append(typeParams).Append(">> _returnCallbacks = [];").AppendLine();
		sb.Append("\tint _currentReturnCallbackIndex = -1;").AppendLine();
		sb.AppendLine();
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic VoidMethodSetup<").Append(typeParams).Append("> Callback(Action callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_callback = (")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => callback();").AppendLine();
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
		sb.Append("\t\t_callback = callback;").AppendLine();
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
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic VoidMethodSetup<").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((").Append(values).Append(") => throw callback(").Append(values).Append("));").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
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
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.").AppendLine();
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
		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)\" />")
			.AppendLine();
		sb.Append("\tprotected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (");
		for (int i = 1; i < numberOfParameters; i++)
		{
			sb.Append("TryCast<T").Append(i).Append(">(invocation.Parameters[").Append(i - 1).Append("], out var p")
				.Append(i).Append(", behavior) &&");
			sb.AppendLine().Append("\t\t    ");
		}

		sb.Append("TryCast<T").Append(numberOfParameters).Append(">(invocation.Parameters[")
			.Append(numberOfParameters - 1).Append("], out var p").Append(numberOfParameters).Append(", behavior))")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callback?.Invoke(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(");")
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
		sb.Append("\t\t=> invocation.Name.Equals(name) && Matches([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"match{x}")))
			.Append("], invocation.Parameters);").AppendLine();
		sb.AppendLine();
		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetOutParameter{T}(string, MockBehavior)\" />").AppendLine();
		sb.Append("\tprotected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (HasOutParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"match{x}")))
			.Append("], parameterName, out With.OutParameter<T>? outParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn outParameter.GetValue();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn behavior.DefaultValueGenerator.Generate<T>();").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetRefParameter{T}(string, T, MockBehavior)\" />").AppendLine();
		sb.Append("\tprotected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (HasRefParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"match{x}")))
			.Append("], parameterName, out With.RefParameter<T>? refParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn refParameter.GetValue(value);").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn value;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();
	}

	private static void AppendReturnMethod(StringBuilder sb, int numberOfParameters)
	{
		string typeParams = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"T{i}"));
		string values = string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(i => $"v{i}"));
		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Setup for a method with ").Append(numberOfParameters).Append(" parameters ");
		for (int i = 1; i < numberOfParameters - 1; i++)
		{
			sb.Append("<typeparamref name=\"T").Append(i).Append("\" />, ");
		}

		sb.Append("<typeparamref name=\"T").Append(numberOfParameters - 1).Append("\" /> and <typeparamref name=\"T")
			.Append(numberOfParameters).Append("\" /> returning <typeparamref name=\"TReturn\" />.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("public class ReturnMethodSetup<TReturn, ").Append(typeParams).Append(">(string name");
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append(", With.NamedParameter match").Append(i);
		}

		sb.Append(") : MethodSetup").AppendLine();
		sb.Append("{").AppendLine();
		sb.Append("\tprivate Action<").Append(typeParams).Append(">? _callback;").AppendLine();
		sb.Append("\tprivate List<Func<").Append(typeParams).Append(", TReturn>> _returnCallbacks = [];").AppendLine();
		sb.Append("\tint _currentReturnCallbackIndex = -1;").AppendLine();
		sb.AppendLine();
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> to execute when the method is called.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Callback(Action callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_callback = (")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => callback();").AppendLine();
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
		sb.Append("\t\t_callback = callback;").AppendLine();
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
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Throws(Func<")
			.Append(typeParams).Append(", Exception> callback)").AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((").Append(values).Append(") => throw callback(").Append(values).Append("));").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers a <paramref name=\"callback\" /> that will calculate the exception to throw when the method is invoked.")
			.AppendLine();
		sb.Append("\t/// </summary>").AppendLine();
		sb.Append("\tpublic ReturnMethodSetup<TReturn, ").Append(typeParams).Append("> Throws(Func<Exception> callback)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\t_returnCallbacks.Add((")
			.Append(string.Join(", ", Enumerable.Range(0, numberOfParameters).Select(_ => "_")))
			.Append(") => throw callback());").AppendLine();
		sb.Append("\t\treturn this;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t/// <summary>").AppendLine();
		sb.Append("\t///     Registers an <paramref name=\"exception\" /> to throw when the method is invoked.").AppendLine();
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
		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)\" />")
			.AppendLine();
		sb.Append("\tprotected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (");
		for (int i = 1; i < numberOfParameters; i++)
		{
			sb.Append("TryCast<T").Append(i).Append(">(invocation.Parameters[").Append(i - 1).Append("], out var p")
				.Append(i).Append(", behavior) &&");
			sb.AppendLine().Append("\t\t    ");
		}

		sb.Append("TryCast<T").Append(numberOfParameters).Append(">(invocation.Parameters[")
			.Append(numberOfParameters - 1).Append("], out var p").Append(numberOfParameters).Append(", behavior))")
			.AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callback?.Invoke(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(");")
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
		sb.Append("\t\t\treturn behavior.DefaultValueGenerator.Generate<TResult>();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		for (int i = 1; i <= numberOfParameters; i++)
		{
			sb.Append("\t\tif (!TryCast<T").Append(i).Append(">(invocation.Parameters[").Append(i - 1)
				.Append("], out var p").Append(i).Append(", behavior))").AppendLine();
			sb.Append("\t\t{").AppendLine();
			sb.Append("\t\t\tthrow new MockException($\"The input parameter ").Append(i)
				.Append(" only supports '{typeof(T").Append(i).Append(")}', but is '{invocation.Parameters[")
				.Append(i - 1).Append("]?.GetType()}'.\");").AppendLine();
			sb.Append("\t\t}").AppendLine();
			sb.AppendLine();
		}

		sb.Append("\t\tvar index = Interlocked.Increment(ref _currentReturnCallbackIndex);").AppendLine();
		sb.Append("\t\tvar returnCallback = _returnCallbacks[index % _returnCallbacks.Count];").AppendLine();
		sb.Append("\t\tif (returnCallback(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}")))
			.Append(") is TResult result)").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\t_callback?.Invoke(")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"p{x}"))).Append(");")
			.AppendLine();
		sb.Append("\t\t\treturn result;").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append(
				"\t\tthrow new MockException($\"The return callback only supports '{typeof(TReturn)}' and not '{typeof(TResult)}'.\");")
			.AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.IsMatch(MethodInvocation)\" />").AppendLine();
		sb.Append("\tprotected override bool IsMatch(MethodInvocation invocation)").AppendLine();
		sb.Append("\t\t=> invocation.Name.Equals(name) && Matches([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"match{x}")))
			.Append("], invocation.Parameters);").AppendLine();
		sb.AppendLine();
		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetOutParameter{T}(string, MockBehavior)\" />").AppendLine();
		sb.Append("\tprotected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (HasOutParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"match{x}")))
			.Append("], parameterName, out With.OutParameter<T>? outParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn outParameter.GetValue();").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn behavior.DefaultValueGenerator.Generate<T>();").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t/// <inheritdoc cref=\"MethodSetup.SetRefParameter{T}(string, T, MockBehavior)\" />").AppendLine();
		sb.Append("\tprotected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)")
			.AppendLine();
		sb.Append("\t{").AppendLine();
		sb.Append("\t\tif (HasRefParameter([")
			.Append(string.Join(", ", Enumerable.Range(1, numberOfParameters).Select(x => $"match{x}")))
			.Append("], parameterName, out With.RefParameter<T>? refParameter))").AppendLine();
		sb.Append("\t\t{").AppendLine();
		sb.Append("\t\t\treturn refParameter.GetValue(value);").AppendLine();
		sb.Append("\t\t}").AppendLine();
		sb.AppendLine();
		sb.Append("\t\treturn value;").AppendLine();
		sb.Append("\t}").AppendLine();
		sb.Append("}").AppendLine();
		sb.AppendLine();
	}
}
