using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mockolate.SourceGenerators.Entities;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Tests.Entities;

public class MethodEqualityComparerTests
{
	[Fact]
	public async Task BothNull_ReturnsTrue()
	{
		IEqualityComparer<Method> comparer = Method.ContainingTypeIndependentEqualityComparer;

		bool result = comparer.Equals(null, null);

		await That(result).IsTrue();
	}

	[Fact]
	public async Task LeftNull_ReturnsFalse()
	{
		IEqualityComparer<Method> comparer = Method.ContainingTypeIndependentEqualityComparer;
		Method right = CreateMethod("public class C { public void Foo() {} }", "Foo");

		bool result = comparer.Equals(null, right);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task RightNull_ReturnsFalse()
	{
		IEqualityComparer<Method> comparer = Method.ContainingTypeIndependentEqualityComparer;
		Method left = CreateMethod("public class C { public void Foo() {} }", "Foo");

		bool result = comparer.Equals(left, null);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task DifferentNames_ReturnsFalse()
	{
		IEqualityComparer<Method> comparer = Method.ContainingTypeIndependentEqualityComparer;
		Method left = CreateMethod("public class C { public void Foo() {} }", "Foo");
		Method right = CreateMethod("public class C { public void Bar() {} }", "Bar");

		bool result = comparer.Equals(left, right);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task DifferentParameterCount_ReturnsFalse()
	{
		IEqualityComparer<Method> comparer = Method.ContainingTypeIndependentEqualityComparer;
		Method left = CreateMethod("public class C { public void Foo() {} }", "Foo");
		Method right = CreateMethod("public class C { public void Foo(int x) {} }", "Foo");

		bool result = comparer.Equals(left, right);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task BothMethodsHaveDefaultParameters_ReturnsTrue()
	{
		IEqualityComparer<Method> comparer = Method.ContainingTypeIndependentEqualityComparer;
		Method left = CreateMethodWithDefaultParameters("public class C { public void Foo() {} }", "Foo");
		Method right = CreateMethodWithDefaultParameters("public class C { public void Foo() {} }", "Foo");

		bool result = comparer.Equals(left, right);

		await That(result).IsTrue();
		await That(left.Parameters.AsArray()).IsNull();
		await That(right.Parameters.AsArray()).IsNull();
	}

	private static Method CreateMethod(string source, string methodName)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
		CSharpCompilation compilation = CSharpCompilation.Create(
			"TestAssembly",
			[tree,],
			[MetadataReference.CreateFromFile(typeof(object).Assembly.Location),],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		SemanticModel model = compilation.GetSemanticModel(tree);
		MethodDeclarationSyntax declaration = tree.GetRoot().DescendantNodes()
			.OfType<MethodDeclarationSyntax>()
			.First(m => m.Identifier.Text == methodName);
		IMethodSymbol symbol = (IMethodSymbol)model.GetDeclaredSymbol(declaration)!;
		return new Method(symbol, null);
	}

	private static Method CreateMethodWithDefaultParameters(string source, string methodName)
	{
		Method method = CreateMethod(source, methodName);
		FieldInfo backingField = typeof(Method)
			.GetField("<Parameters>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
		backingField.SetValue(method, default(EquatableArray<MethodParameter>));
		return method;
	}
}
