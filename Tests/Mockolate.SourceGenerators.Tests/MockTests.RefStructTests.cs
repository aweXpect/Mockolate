namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed class RefStructTests
	{
		[Fact]
		public async Task VoidMethodWithRefStructParameter_ShouldEmitRefStructPipeline()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Packet(int id) { public int Id { get; } = id; }

				     public interface IPacketSink
				     {
				         void Consume(Packet packet);
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IPacketSink.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IPacketSink.g.cs").WhoseValue
				.Contains("#if NET9_0_OR_GREATER").And
				// Method body: records a RefStructMethodInvocation (names only, no value).
				.Contains("RefStructMethodInvocation(\"global::MyCode.IPacketSink.Consume\", \"packet\")").And
				// Dispatch: iterates matching setups via the GetMethodSetups<T>(name) API.
				.Contains("GetMethodSetups<global::Mockolate.Setup.RefStructVoidMethodSetup<global::MyCode.Packet>>").And
				.Contains("RefStructVoidMethodSetup<global::MyCode.Packet>").And
				// Setup facade entry point uses the narrow IRefStructVoidMethodSetup surface.
				.Contains("IRefStructVoidMethodSetup<global::MyCode.Packet>").And
				.Contains("global::Mockolate.Parameters.IParameter<global::MyCode.Packet>? packet");
		}

		[Fact]
		public async Task ReturnMethodWithRefStructParameter_ShouldEmitRefStructReturnSetup()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Packet(int id) { public int Id { get; } = id; }

				     public interface IPacketParser
				     {
				         int TryParse(Packet packet);
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IPacketParser.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IPacketParser.g.cs").WhoseValue
				.Contains("RefStructReturnMethodSetup<int, global::MyCode.Packet>").And
				.Contains("IRefStructReturnMethodSetup<int, global::MyCode.Packet>").And
				// The return-side HasReturnValue gate is there so Throws-only setups still fall
				// through to the framework default.
				.Contains("if (__setup.HasReturnValue)");
		}

		[Fact]
		public async Task MixedParameters_RefStructPlusValueType_ShouldRouteThroughRefStructPipeline()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Packet(int id) { public int Id { get; } = id; }

				     public interface IPacketWriter
				     {
				         void Write(Packet packet, int priority);
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IPacketWriter.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IPacketWriter.g.cs").WhoseValue
				// Two-arg ref-struct setup with the int flowing through as T2 (allows ref struct
				// is satisfied by any type, so int works).
				.Contains("RefStructVoidMethodSetup<global::MyCode.Packet, int>").And
				.Contains("__setup.Matches(packet, priority)");
		}

		[Fact]
		public async Task NonRefStructMethod_ShouldContinueToUseRegularPipeline()
		{
			// Regression guard: our ref-struct switch must not intercept methods that have no
			// ref-struct parameter. The existing VoidMethodSetup / MethodInvocation<T> code path
			// is the entire test surface for thousands of tests.
			GeneratorResult result = Generator
				.Run("""
				     using Mockolate;

				     namespace MyCode;

				     public interface IBoringService
				     {
				         void DoWork(int x, string y);
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IBoringService.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IBoringService.g.cs").WhoseValue
				// Regular pipeline markers.
				.Contains("global::Mockolate.Interactions.MethodInvocation<int, string>").And
				.Contains("global::Mockolate.Setup.VoidMethodSetup<int, string>").And
				// Must NOT use the ref-struct pipeline.
				.DoesNotContain("RefStructMethodInvocation").And
				.DoesNotContain("RefStructVoidMethodSetup");
		}

		[Fact]
		public async Task Arity5VoidMethod_EmitsGeneratedRefStructSetup()
		{
			// Arity 5 exceeds the hand-written ceiling (1-4); the generator must emit
			// RefStructVoidMethodSetup<T1,...,T5> into RefStructMethodSetups.g.cs.
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Packet(int id) { public int Id { get; } = id; }

				     public interface IBigSink
				     {
				         void Consume(Packet p1, Packet p2, Packet p3, Packet p4, Packet p5);
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IBigSink.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IBigSink.g.cs").WhoseValue
				.Contains("RefStructVoidMethodSetup<global::MyCode.Packet, global::MyCode.Packet, global::MyCode.Packet, global::MyCode.Packet, global::MyCode.Packet>").And
				// The method body must not bail out to NotSupportedException for this arity.
				.Contains("__matched = true");

			await That(result.Sources).ContainsKey("RefStructMethodSetups.g.cs").WhoseValue
				.Contains("#if NET9_0_OR_GREATER").And
				.Contains("public interface IRefStructVoidMethodSetup<T1, T2, T3, T4, T5>").And
				.Contains("public sealed class RefStructVoidMethodSetup<T1, T2, T3, T4, T5>").And
				// Every generic parameter carries the allows-ref-struct anti-constraint.
				.Contains("where T5 : allows ref struct");
		}

		[Fact]
		public async Task Arity6ReturnMethod_EmitsGeneratedRefStructReturnSetup()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Packet(int id) { public int Id { get; } = id; }

				     public interface IBigParser
				     {
				         int TryParse(Packet p1, Packet p2, int p3, Packet p4, Packet p5, string p6);
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IBigParser.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("RefStructMethodSetups.g.cs").WhoseValue
				.Contains("public interface IRefStructReturnMethodSetup<TReturn, T1, T2, T3, T4, T5, T6>").And
				.Contains("public sealed class RefStructReturnMethodSetup<TReturn, T1, T2, T3, T4, T5, T6>").And
				.Contains("public bool HasReturnValue");
		}

		[Fact]
		public async Task IndexerGetterWithRefStructKey_EmitsRefStructDispatch()
		{
			// Getter-only ref-struct-keyed indexers route through RefStructIndexerGetterSetup.
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Key(int id) { public int Id { get; } = id; }

				     public interface IRefStructLookup
				     {
				         string this[Key key] { get; }
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IRefStructLookup.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IRefStructLookup.g.cs").WhoseValue
				// Body uses the ref-struct dispatch, not NotSupportedException.
				.Contains("RefStructIndexerGetterSetup<string, global::MyCode.Key>").And
				.Contains("RefStructMethodInvocation(\"global::MyCode.IRefStructLookup.get_Item\", \"key\")").And
				// The setup facade exposes the narrow IRefStructIndexerGetterSetup surface.
				.Contains("IRefStructIndexerGetterSetup<string, global::MyCode.Key>");
		}

		[Fact]
		public async Task IndexerWithRefStructKey_AndSetter_StillEmitsNotSupportedException()
		{
			// Setter side is out of scope until commit J — keep the unsupported-shape guard.
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Key(int id) { public int Id { get; } = id; }

				     public interface IRefStructStore
				     {
				         string this[Key key] { get; set; }
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             #pragma warning disable Mockolate0004
				             _ = IRefStructStore.CreateMock();
				             #pragma warning restore Mockolate0004
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IRefStructStore.g.cs").WhoseValue
				// Setters keep the runtime NSE until commit J wires IRefStructIndexerSetterSetup.
				.Contains("indexer setters with ref-struct keys are not supported");
		}
	}
}
