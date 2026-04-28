namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed class RefStructTests
	{
		[Fact]
		public async Task Arity16VoidMethod_EmitsGeneratedRefStructSetupAtArity16()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Packet(int id) { public int Id { get; } = id; }

				     public interface IBigSink16
				     {
				         void Consume(Packet p1, Packet p2, Packet p3, Packet p4, Packet p5, Packet p6, Packet p7, Packet p8, Packet p9, Packet p10, Packet p11, Packet p12, Packet p13, Packet p14, Packet p15, Packet p16);
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IBigSink16.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("RefStructMethodSetups.g.cs").WhoseValue
				.Contains("public interface IRefStructVoidMethodSetup<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>").And
				.Contains("public sealed class RefStructVoidMethodSetup<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>").And
				.Contains("where T16 : allows ref struct").And
				.Contains("where T1 : allows ref struct");
		}

		[Fact]
		public async Task Arity5IndexerGetAndSet_WithRefStructKey_EmitsCombinedSetup()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Key(int id) { public int Id { get; } = id; }

				     public interface IRefStructStore5
				     {
				         string this[Key k1, int k2, Key k3, int k4, Key k5] { get; set; }
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IRefStructStore5.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("RefStructMethodSetups.g.cs").WhoseValue
				.Contains("public interface IRefStructIndexerSetup<TValue, T1, T2, T3, T4, T5>").And
				.Contains("public sealed class RefStructIndexerSetup<TValue, T1, T2, T3, T4, T5>").And
				// Combined facade exposes the inner Getter / Setter properties.
				.Contains("Getter { get; }").And
				.Contains("Setter { get; }").And
				// Storage plumbing visible on the getter-only emission.
				.Contains("TryResolveKey").And
				.Contains("GetChildDispatch").And
				.Contains("StoreValue").And
				.Contains("BoundGetter");
		}

		[Fact]
		public async Task Arity5IndexerGetterOnly_WithRefStructKey_EmitsGetterSetupOnly()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Key(int id) { public int Id { get; } = id; }

				     public interface IRefStructLookup5
				     {
				         string this[Key k1, int k2, Key k3, int k4, Key k5] { get; }
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IRefStructLookup5.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("RefStructMethodSetups.g.cs").WhoseValue
				.Contains("public interface IRefStructIndexerGetterSetup<TValue, T1, T2, T3, T4, T5>").And
				.Contains("public sealed class RefStructIndexerGetterSetup<TValue, T1, T2, T3, T4, T5>").And
				// Storage / projection plumbing must be present at arity 5.
				.Contains("TryResolveKey").And
				.Contains("GetChildDispatch").And
				// Setter-only and combined surfaces must NOT be emitted for a getter-only indexer.
				.DoesNotContain("public sealed class RefStructIndexerSetterSetup<TValue, T1, T2, T3, T4, T5>").And
				.DoesNotContain("public sealed class RefStructIndexerSetup<TValue, T1, T2, T3, T4, T5>");
		}

		[Fact]
		public async Task Arity5IndexerSetterOnly_WithRefStructKey_EmitsSetterSetupOnly()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Key(int id) { public int Id { get; } = id; }

				     public interface IRefStructWriter5
				     {
				         string this[Key k1, int k2, Key k3, int k4, Key k5] { set; }
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IRefStructWriter5.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("RefStructMethodSetups.g.cs").WhoseValue
				.Contains("public interface IRefStructIndexerSetterSetup<TValue, T1, T2, T3, T4, T5>").And
				.Contains("public sealed class RefStructIndexerSetterSetup<TValue, T1, T2, T3, T4, T5>").And
				// BoundGetter property is emitted on the setter so combined setups can forward writes.
				.Contains("BoundGetter").And
				// No getter-only or combined surface for a setter-only indexer.
				.DoesNotContain("public sealed class RefStructIndexerGetterSetup<TValue, T1, T2, T3, T4, T5>").And
				.DoesNotContain("public sealed class RefStructIndexerSetup<TValue, T1, T2, T3, T4, T5>");
		}

		[Fact]
		public async Task Arity5ReturnMethod_EmitsGeneratedRefStructReturnSetup()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Packet(int id) { public int Id { get; } = id; }

				     public interface IBigParser5
				     {
				         int TryParse(Packet p1, int p2, Packet p3, Packet p4, string p5);
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IBigParser5.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("RefStructMethodSetups.g.cs").WhoseValue
				.Contains("public interface IRefStructReturnMethodSetup<TReturn, T1, T2, T3, T4, T5>").And
				.Contains("public sealed class RefStructReturnMethodSetup<TReturn, T1, T2, T3, T4, T5>").And
				.Contains("public bool HasReturnValue => _returnFactory is not null;").And
				.Contains(".Returns(TReturn returnValue)").And
				.Contains(".Returns(global::System.Func<TReturn> returnFactory)").And
				// default! fallback when neither return factory nor defaultFactory is present.
				.Contains("return defaultFactory is not null ? defaultFactory() : default!;");
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
				.Contains("matched = true");

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
		public async Task Arity6VoidMethod_EmitsGeneratedRefStructSetupAtArity6()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Packet(int id) { public int Id { get; } = id; }

				     public interface IBigSink6
				     {
				         void Consume(Packet p1, Packet p2, Packet p3, Packet p4, Packet p5, Packet p6);
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IBigSink6.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("RefStructMethodSetups.g.cs").WhoseValue
				.Contains("public interface IRefStructVoidMethodSetup<T1, T2, T3, T4, T5, T6>").And
				.Contains("public sealed class RefStructVoidMethodSetup<T1, T2, T3, T4, T5, T6>").And
				.Contains("where T1 : allows ref struct").And
				.Contains("where T6 : allows ref struct").And
				// _returnAction body present (matches the emitted void-setup throw machinery).
				.Contains("private global::System.Func<global::System.Exception?>? _returnAction;").And
				.Contains("_returnAction = static () => new TException();");
		}

		[Fact]
		public async Task Arity7ReturnMethod_MixedRefStructAndValueTypes_EmitsRefStructReturnSetup()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Packet(int id) { public int Id { get; } = id; }

				     public interface IBigParser7
				     {
				         int TryParse(Packet p1, Packet p2, int p3, Packet p4, Packet p5, string p6, double p7);
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IBigParser7.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("RefStructMethodSetups.g.cs").WhoseValue
				.Contains("public interface IRefStructReturnMethodSetup<TReturn, T1, T2, T3, T4, T5, T6, T7>").And
				.Contains("public sealed class RefStructReturnMethodSetup<TReturn, T1, T2, T3, T4, T5, T6, T7>").And
				.Contains("public bool HasReturnValue => _returnFactory is not null;").And
				.Contains("return defaultFactory is not null ? defaultFactory() : default!;").And
				.Contains("where T7 : allows ref struct");
		}

		[Fact]
		public async Task Arity8VoidMethod_EmitsGeneratedRefStructSetupAtArity8()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Packet(int id) { public int Id { get; } = id; }

				     public interface IBigSink8
				     {
				         void Consume(Packet p1, Packet p2, Packet p3, Packet p4, Packet p5, Packet p6, Packet p7, Packet p8);
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IBigSink8.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("RefStructMethodSetups.g.cs").WhoseValue
				.Contains("public interface IRefStructVoidMethodSetup<T1, T2, T3, T4, T5, T6, T7, T8>").And
				.Contains("public sealed class RefStructVoidMethodSetup<T1, T2, T3, T4, T5, T6, T7, T8>").And
				.Contains("where T8 : allows ref struct").And
				.Contains("_returnAction = exceptionFactory;");
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
		public async Task IndexerWithRefStructKey_AndSetter_EmitsCombinedSetup()
		{
			// Get+set ref-struct-keyed indexer: expose IRefStructIndexerSetup<TValue, T> on the
			// setup facade and dispatch both accessors through the ref-struct pipeline.
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
				             _ = IRefStructStore.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IRefStructStore.g.cs").WhoseValue
				// Combined facade type is the public setup surface.
				.Contains("IRefStructIndexerSetup<string, global::MyCode.Key>").And
				// Both accessors now dispatch through the ref-struct pipeline, not NSE.
				.Contains("RefStructMethodInvocation(\"global::MyCode.IRefStructStore.get_Item\", \"key\")").And
				.Contains("RefStructMethodInvocation(\"global::MyCode.IRefStructStore.set_Item\", \"key\", \"value\")").And
				.Contains("RefStructIndexerSetterSetup<string, global::MyCode.Key>");
		}

		[Fact]
		public async Task IndexerWithRefStructKey_SetterOnly_EmitsSetterSetup()
		{
			GeneratorResult result = Generator
				.Run("""
				     using System;
				     using Mockolate;

				     namespace MyCode;

				     public readonly ref struct Key(int id) { public int Id { get; } = id; }

				     public interface IRefStructWriter
				     {
				         string this[Key key] { set; }
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IRefStructWriter.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IRefStructWriter.g.cs").WhoseValue
				.Contains("IRefStructIndexerSetterSetup<string, global::MyCode.Key>").And
				.Contains("RefStructMethodInvocation(\"global::MyCode.IRefStructWriter.set_Item\", \"key\", \"value\")");
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
				.Contains("setup.Matches(packet, priority)");
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
		public async Task NoRefStructSurface_ShouldNotEmitRefStructMethodSetupsFile()
		{
			GeneratorResult result = Generator
				.Run("""
				     using Mockolate;

				     namespace MyCode;

				     public interface IPlainService
				     {
				         void DoWork(int v1, int v2, int v3, int v4, int v5, int v6);
				         int Compute(int v1, int v2, int v3, int v4, int v5, int v6);
				         int this[int v1, int v2, int v3, int v4, int v5] { get; set; }
				     }

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				             _ = IPlainService.CreateMock();
				         }
				     }
				     """);

			await That(result.Sources).DoesNotContainKey("RefStructMethodSetups.g.cs");
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
				.Contains("if (setup.HasReturnValue)");
		}

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
	}
}
