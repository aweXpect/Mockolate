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
		public async Task IndexerWithRefStructKey_EmitsNotSupportedException()
		{
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
				.Contains("indexer getters with ref-struct keys are not yet supported").And
				.Contains("throw new global::System.NotSupportedException(");
		}
	}
}
