using System.Text;

namespace Mockolate.SourceGenerators.Internals;

internal static partial class SourceGeneration
{
	public static string GetReturnsAsyncExtensions(int[] numberOfParameters)
	{
		StringBuilder sb = new();
		sb.AppendLine(Header);
		sb.Append("""
		          using System;
		          using Mockolate.Setup;

		          namespace Mockolate;

		          #nullable enable

		          """);
		sb.AppendLine();
		sb.Append("/// <summary>").AppendLine();
		sb.Append("///     Extensions for setting up asynchronous return values.").AppendLine();
		sb.Append("/// </summary>").AppendLine();
		sb.Append("public static class ReturnsAsyncExtensions2").AppendLine();
		sb.Append("{").AppendLine();
		foreach (var number in numberOfParameters)
		{
			var types = string.Join(", ", Enumerable.Range(1, number).Select(i => $"T{i}"));
			var variables = string.Join(", ", Enumerable.Range(1, number).Select(i => $"v{i}"));
			sb.Append("\t/// <summary>").AppendLine();
			sb.Append("\t///     Registers the <see langword=\"async\" /> <paramref name=\"returnValue\" /> for this method.").AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static ReturnMethodSetup<Task<TReturn>, ").Append(types).Append("> ReturnsAsync<TReturn, ").Append(types).Append(">(this ReturnMethodSetup<Task<TReturn>, ").Append(types).Append("> setup, TReturn returnValue)").AppendLine();
			sb.Append("\t\t=> setup.Returns(Task.FromResult(returnValue));");
			sb.AppendLine();
			sb.Append("\t/// <summary>").AppendLine();
			sb.Append("\t///     Registers an <see langword=\"async\" /> <paramref name=\"callback\" /> to setup the return value for this method.").AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static ReturnMethodSetup<Task<TReturn>, ").Append(types).Append("> ReturnsAsync<TReturn, ").Append(types).Append(">(this ReturnMethodSetup<Task<TReturn>, ").Append(types).Append("> setup, Func<TReturn> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns(() => Task.FromResult(callback()));");
			sb.AppendLine();
			sb.Append("\t/// <summary>").AppendLine();
			sb.Append("\t///     Registers an <see langword=\"async\" /> <paramref name=\"callback\" /> to setup the return value for this method.").AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static ReturnMethodSetup<Task<TReturn>, ").Append(types).Append("> ReturnsAsync<TReturn, ").Append(types).Append(">(this ReturnMethodSetup<Task<TReturn>, ").Append(types).Append("> setup, Func<").Append(types).Append(", TReturn> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns((").Append(variables).Append(") => Task.FromResult(callback(").Append(variables).Append(")));");
			sb.AppendLine();
		}

		sb.AppendLine("#if NET8_0_OR_GREATER");
		foreach (var number in numberOfParameters)
		{
			var types = string.Join(", ", Enumerable.Range(1, number).Select(i => $"T{i}"));
			var variables = string.Join(", ", Enumerable.Range(1, number).Select(i => $"v{i}"));
			sb.Append("\t/// <summary>").AppendLine();
			sb.Append("\t///     Registers the <see langword=\"async\" /> <paramref name=\"returnValue\" /> for this method.").AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static ReturnMethodSetup<ValueTask<TReturn>, ").Append(types).Append("> ReturnsAsync<TReturn, ").Append(types).Append(">(this ReturnMethodSetup<ValueTask<TReturn>, ").Append(types).Append("> setup, TReturn returnValue)").AppendLine();
			sb.Append("\t\t=> setup.Returns(ValueTask.FromResult(returnValue));");
			sb.AppendLine();
			sb.Append("\t/// <summary>").AppendLine();
			sb.Append("\t///     Registers an <see langword=\"async\" /> <paramref name=\"callback\" /> to setup the return value for this method.").AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static ReturnMethodSetup<ValueTask<TReturn>, ").Append(types).Append("> ReturnsAsync<TReturn, ").Append(types).Append(">(this ReturnMethodSetup<ValueTask<TReturn>, ").Append(types).Append("> setup, Func<TReturn> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns(() => ValueTask.FromResult(callback()));");
			sb.AppendLine();
			sb.Append("\t/// <summary>").AppendLine();
			sb.Append("\t///     Registers an <see langword=\"async\" /> <paramref name=\"callback\" /> to setup the return value for this method.").AppendLine();
			sb.Append("\t/// </summary>").AppendLine();
			sb.Append("\tpublic static ReturnMethodSetup<ValueTask<TReturn>, ").Append(types).Append("> ReturnsAsync<TReturn, ").Append(types).Append(">(this ReturnMethodSetup<ValueTask<TReturn>, ").Append(types).Append("> setup, Func<").Append(types).Append(", TReturn> callback)").AppendLine();
			sb.Append("\t\t=> setup.Returns((").Append(variables).Append(") => ValueTask.FromResult(callback(").Append(variables).Append(")));");
			sb.AppendLine();
		}
		sb.AppendLine("#endif");
		sb.Append("}").AppendLine();

		sb.AppendLine("#nullable disable");
		return sb.ToString();
	}
}
