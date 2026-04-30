using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Verifier = Mockolate.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<Mockolate.Analyzers.MockabilityAnalyzer>;

namespace Mockolate.Analyzers.Tests;

/// <summary>
///     Coverage for the <c>Mockolate0003</c> ref-struct mockability diagnostic emitted by
///     <see cref="MockabilityAnalyzer" />.
/// </summary>
/// <remarks>
///     The analyzer-test host targets net10.0 with Mockolate's ref-struct types referenced, so
///     the "target framework lacks the pipeline" branch of
///     <c>GetRefStructPipelineUnsupportedReason</c> cannot be exercised here. The C# language
///     version branch (<c>LangVersion &lt; 13</c>) is reachable via the LanguageVersion-aware
///     verifier overload. All other failure modes (out/ref ref-struct params, non-span ref-struct
///     returns, ref-struct-keyed indexers, delegates, inheritance) are covered below.
/// </remarks>
public class MockabilityAnalyzerRefStructTests
{
	[Test]
	public async Task WhenLanguageVersionBelowCSharp13_RefStructKeyedIndexer_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IRefStructLookup")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Key(int id) { public int Id { get; } = id; }

			  	public interface IRefStructLookup
			  	{
			  		string this[Key key] { get; }
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			{|#0:IRefStructLookup|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			LanguageVersion.CSharp12,
			new DiagnosticResult("Mockolate0003", DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IRefStructLookup", "this[]",
					"ref-struct-keyed indexers require C# 13 or later (uses the 'allows ref struct' anti-constraint; current LangVersion is 12.0)")
		);

	[Test]
	public async Task WhenLanguageVersionBelowCSharp13_RefStructParameterMethod_ShouldBeFlagged() => await Verifier
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
			  			{|#0:IPacketSink|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			LanguageVersion.CSharp12,
			new DiagnosticResult("Mockolate0003", DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IPacketSink", "Consume",
					"ref-struct parameter mocking requires C# 13 or later (uses the 'allows ref struct' anti-constraint; current LangVersion is 12.0)")
		);

	[Test]
	public async Task WhenMockingAbstractClassWithInheritedRefStructViolation_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.DerivedProducer")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Packet(int id) { public int Id { get; } = id; }

			  	public abstract class BaseProducer
			  	{
			  		// Virtual members on a base class are also walked and must trip the analyzer.
			  		public abstract void Produce(out Packet packet);
			  	}

			  	public abstract class DerivedProducer : BaseProducer
			  	{
			  		public abstract void Extra();
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			{|#0:DerivedProducer|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			new DiagnosticResult("Mockolate0003", DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.DerivedProducer", "Produce",
					"out/ref ref-struct parameters are not supported")
		);

	[Test]
	public async Task WhenMockingDelegateReturningNonSpanRefStruct_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.PacketFactoryDelegate")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Packet(int id) { public int Id { get; } = id; }

			  	// Delegate returning a non-span ref struct — same rejection as interface
			  	// methods with that return shape.
			  	public delegate Packet PacketFactoryDelegate();

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			{|#0:PacketFactoryDelegate|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			new DiagnosticResult("Mockolate0003", DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.PacketFactoryDelegate", "Invoke",
					"methods returning a non-span ref struct are not supported")
		);

	[Test]
	public async Task WhenMockingDelegateWithOutRefStructParameter_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.PacketProducerDelegate")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Packet(int id) { public int Id { get; } = id; }

			  	// Delegate Invoke methods are analyzed the same as interface methods; out/ref on
			  	// a ref-struct parameter must be rejected.
			  	public delegate void PacketProducerDelegate(out Packet packet);

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			{|#0:PacketProducerDelegate|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			new DiagnosticResult("Mockolate0003", DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.PacketProducerDelegate", "Invoke",
					"out/ref ref-struct parameters are not supported")
		);

	[Test]
	public async Task WhenMockingDelegateWithPlainRefStructParameter_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.PacketHandler")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Packet(int id) { public int Id { get; } = id; }

			  	// Delegate Invoke with a plain ref-struct parameter — routed through the
			  	// generator's ref-struct pipeline, no diagnostic expected.
			  	public delegate void PacketHandler(Packet packet);

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			PacketHandler.CreateMock();
			  		}
			  	}
			  }
			  """
		);

	[Test]
	public async Task WhenMockingInterfaceInheritingFromInterfaceWithBadMethod_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IDerivedSink")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Packet(int id) { public int Id { get; } = id; }

			  	public interface IBaseSink
			  	{
			  		void Produce(out Packet packet);
			  	}

			  	// Inherited members are walked via ITypeSymbol.AllInterfaces — the violation on
			  	// the base must surface against the derived interface being mocked.
			  	public interface IDerivedSink : IBaseSink
			  	{
			  		void Extra();
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			{|#0:IDerivedSink|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			new DiagnosticResult("Mockolate0003", DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IDerivedSink", "Produce",
					"out/ref ref-struct parameters are not supported")
		);

	[Test]
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
			new DiagnosticResult("Mockolate0003", DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IPacketFactory", "Produce",
					"methods returning a non-span ref struct are not supported")
		);

	[Test]
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
			new DiagnosticResult("Mockolate0003", DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IPacketProducer", "Produce",
					"out/ref ref-struct parameters are not supported")
		);

	[Test]
	public async Task WhenMockingInterfaceWithOverloads_OnlyViolatingOverloadIsFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IOverloadedSink")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Packet(int id) { public int Id { get; } = id; }

			  	// Two overloads share a name; GetSignatureKey includes RefKind so de-duplication
			  	// does not collapse them. The plain-parameter overload is fine; the out-parameter
			  	// overload must produce exactly one diagnostic.
			  	public interface IOverloadedSink
			  	{
			  		void Consume(Packet packet);
			  		void Consume(out Packet packet);
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			{|#0:IOverloadedSink|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			new DiagnosticResult("Mockolate0003", DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IOverloadedSink", "Consume",
					"out/ref ref-struct parameters are not supported")
		);

	[Test]
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

	[Test]
	public async Task WhenMockingInterfaceWithRefReadonlyRefStructParameter_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IPacketInspector")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Packet(int id) { public int Id { get; } = id; }

			  	public interface IPacketInspector
			  	{
			  		// `ref readonly` on a parameter produces RefKind.RefReadOnlyParameter, which
			  		// is explicitly listed alongside Out and Ref in MockabilityAnalyzer.
			  		void Inspect(ref readonly Packet packet);
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			{|#0:IPacketInspector|}.CreateMock();
			  		}
			  	}
			  }
			  """,
			new DiagnosticResult("Mockolate0003", DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IPacketInspector", "Inspect",
					"out/ref ref-struct parameters are not supported")
		);

	[Test]
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
			new DiagnosticResult("Mockolate0003", DiagnosticSeverity.Warning)
				.WithLocation(0)
				.WithArguments("MyNamespace.IPacketMutator", "Mutate",
					"out/ref ref-struct parameters are not supported")
		);

	[Test]
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

	[Test]
	public async Task WhenMockingInterfaceWithRefStructIndexerKeyAndSetter_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			$$"""
			  {{GeneratedPrefix("MyNamespace.IRefStructStore")}}

			  namespace MyNamespace
			  {
			  	public readonly ref struct Key(int id) { public int Id { get; } = id; }

			  	public interface IRefStructStore
			  	{
			  		// Get+set ref-struct-keyed indexers route through the combined
			  		// IRefStructIndexerSetup facade.
			  		string this[Key key] { get; set; }
			  	}

			  	public class MyClass
			  	{
			  		public void MyTest()
			  		{
			  			IRefStructStore.CreateMock();
			  		}
			  	}
			  }
			  """
		);

	[Test]
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

	[Test]
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
