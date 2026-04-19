using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Mockolate.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<Mockolate.Analyzers.MockabilityAnalyzer>;

namespace Mockolate.Analyzers.Tests;

/// <summary>
///     Coverage for the <c>Mockolate0004</c> ref-struct mockability diagnostic emitted by
///     <see cref="MockabilityAnalyzer" />.
/// </summary>
/// <remarks>
///     The analyzer-test host targets net10.0, so
///     <c>MockabilityAnalyzer.CompilationSupportsRefStructPipeline</c> is satisfied — the
///     "requires net9.0+" branch cannot be exercised from this test project. The in-scope
///     failure modes (out/ref ref-struct params, non-span ref-struct returns, ref-struct-keyed
///     indexers) are all covered below.
/// </remarks>
public class MockabilityAnalyzerRefStructTests
{
	[Fact]
	public async Task WhenMockingInterfaceWithPlainRefStructParameter_ShouldNotFlag() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IPacketSink")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Packet(int id) { public int Id { get; } = id; }

			  	public interface IPacketSink
			  	{
			  		void Consume(Packet packet);
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			IPacketSink.CreateMock();
			  		}
			  	}
			  }
			  """
		);

	[Fact]
	public async Task WhenMockingInterfaceWithOutRefStructParameter_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IPacketProducer")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Packet(int id) { public int Id { get; } = id; }

			  	public interface IPacketProducer
			  	{
			  		void Produce(out Packet packet);
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			{|#0:IPacketProducer|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			new DiagnosticResult("Mockolate0004", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IPacketProducer", "Produce",
					"out/ref ref-struct parameters are not supported")
		);

	[Fact]
	public async Task WhenMockingInterfaceWithRefRefStructParameter_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IPacketMutator")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Packet(int id) { public int Id { get; } = id; }

			  	public interface IPacketMutator
			  	{
			  		void Mutate(ref Packet packet);
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			{|#0:IPacketMutator|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			new DiagnosticResult("Mockolate0004", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IPacketMutator", "Mutate",
					"out/ref ref-struct parameters are not supported")
		);

	[Fact]
	public async Task WhenMockingInterfaceReturningNonSpanRefStruct_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IPacketFactory")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Packet(int id) { public int Id { get; } = id; }

			  	public interface IPacketFactory
			  	{
			  		Packet Produce();
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			{|#0:IPacketFactory|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			new DiagnosticResult("Mockolate0004", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IPacketFactory", "Produce",
					"methods returning a non-span ref struct are not supported")
		);

	[Fact]
	public async Task WhenMockingInterfaceWithSpanReturn_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IPayloadSource")}}

			  namespace MyNamespace
			  {
			  	public interface IPayloadSource
			  	{
			  		// Span/ReadOnlySpan returns go through the existing wrapper pipeline; no
			  		// ref-struct diagnostic is warranted.
			  		System.ReadOnlySpan<byte> GetPayload();
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			IPayloadSource.CreateMock();
			  		}
			  	}
			  }
			  """
		);

	[Fact]
	public async Task WhenMockingInterfaceWithRefStructIndexerKey_GetterOnly_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IRefStructLookup")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Key(int id) { public int Id { get; } = id; }

			  	public interface IRefStructLookup
			  	{
			  		// Getter-only: fully supported via the ref-struct indexer pipeline.
			  		string this[Key key] { get; }
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			IRefStructLookup.CreateMock();
			  		}
			  	}
			  }
			  """
		);

	[Fact]
	public async Task WhenMockingInterfaceWithRefStructIndexerKeyAndSetter_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IRefStructStore")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Key(int id) { public int Id { get; } = id; }

			  	public interface IRefStructStore
			  	{
			  		// Getter-only indexers are supported; the setter side is not yet wired.
			  		string this[Key key] { get; set; }
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			{|#0:IRefStructStore|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			new DiagnosticResult("Mockolate0004", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IRefStructStore", "this[]",
					"indexers with ref-struct keys and a setter are not yet supported by the generator")
		);

	[Fact]
	public async Task WhenMockingInterfaceWithSpanParameter_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.ISpanConsumer")}}

			  namespace MyNamespace
			  {
			  	public interface ISpanConsumer
			  	{
			  		// Span/ReadOnlySpan parameters go through the SpanWrapper/ReadOnlySpanWrapper
			  		// carve-out; they're not routed through the ref-struct pipeline.
			  		void Consume(System.ReadOnlySpan<byte> payload);
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			ISpanConsumer.CreateMock();
			  		}
			  	}
			  }
			  """
		);

	private static string GeneratedPrefix(string fullyQualifiedTypeName)
	{
		string simpleName = fullyQualifiedTypeName.Split('.')[^1];
		return $$"""
		         namespace Mockolate
		         {
		         	internal static partial class MockExtensionsFor{{simpleName}}
		         	{
		         		extension({{fullyQualifiedTypeName}} mock)
		         		{
		         			public static {{fullyQualifiedTypeName}} CreateMock() => default!;
		         		}
		         	}
		         }
		         """;
	}
}
