using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed class CrossAssemblyTests
	{
		private const string CreateMockForMyBaseClass = """
		                                               using Mockolate;

		                                               namespace MyCode;

		                                               public class Program
		                                               {
		                                                   public static void Main(string[] args) => _ = Ext.MyBaseClass.CreateMock();
		                                               }
		                                               """;

		private const string CreateMockForMyExternalType = """
		                                                   using Mockolate;

		                                                   namespace MyCode;

		                                                   public class Program
		                                                   {
		                                                       public static void Main(string[] args) => _ = Ext.MyExternalType.CreateMock();
		                                                   }
		                                                   """;

		[Fact]
		public async Task InternalVirtualMember_WithInternalsVisibleTo_ShouldBeOverridden()
		{
			MetadataReference external = CompileMyBaseClassAssembly(grantsInternalsVisibleTo: true);

			GeneratorResult result = Generator.RunWithReferences(CreateMockForMyBaseClass, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources).ContainsKey("Mock.MyBaseClass.g.cs");
			await That(result.Sources["Mock.MyBaseClass.g.cs"])
				.Contains("internal override void InternalVirtualMethod()").And
				.Contains("internal override int InternalVirtualProperty");
		}

		[Fact]
		public async Task InternalVirtualMember_WithoutInternalsVisibleTo_ShouldNotBeOverridden()
		{
			MetadataReference external = CompileMyBaseClassAssembly(grantsInternalsVisibleTo: false);

			GeneratorResult result = Generator.RunWithReferences(CreateMockForMyBaseClass, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources).ContainsKey("Mock.MyBaseClass.g.cs");
			await That(result.Sources["Mock.MyBaseClass.g.cs"])
				.Contains("public override void PublicAbstractMethod()").And
				.DoesNotContain("InternalVirtualMethod").And
				.DoesNotContain("InternalVirtualProperty");
		}

		[Fact]
		public async Task ProtectedInternalMember_WithInternalsVisibleTo_ShouldBeOverriddenAsProtectedInternal()
		{
			MetadataReference external = CompileMyBaseClassAssembly(grantsInternalsVisibleTo: true);

			GeneratorResult result = Generator.RunWithReferences(CreateMockForMyBaseClass, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources).ContainsKey("Mock.MyBaseClass.g.cs");
			await That(result.Sources["Mock.MyBaseClass.g.cs"])
				.Contains("protected internal override void ProtectedInternalMethod()").And
				.Contains("protected internal override int ProtectedInternalProperty").And
				.Contains("protected internal override event global::System.EventHandler? ProtectedInternalEvent").And
				.Contains("protected internal set");
		}

		[Fact]
		public async Task ProtectedInternalMember_WithoutInternalsVisibleTo_ShouldBeOverriddenAsProtected()
		{
			MetadataReference external = CompileMyBaseClassAssembly(grantsInternalsVisibleTo: false);

			GeneratorResult result = Generator.RunWithReferences(CreateMockForMyBaseClass, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources).ContainsKey("Mock.MyBaseClass.g.cs");
			await That(result.Sources["Mock.MyBaseClass.g.cs"])
				.Contains("protected override void ProtectedInternalMethod()").And
				.Contains("protected override int ProtectedInternalProperty").And
				.Contains("protected override event global::System.EventHandler? ProtectedInternalEvent").And
				.Contains("protected set").And
				.DoesNotContain("protected internal override").And
				.DoesNotContain("protected internal set");
		}

		[Fact]
		public async Task PublicMembers_ShouldBeMockedAcrossAssemblyBoundary()
		{
			MetadataReference external = ExternalAssembly.Compile("""
			                                                      namespace Ext;

			                                                      public interface IMyService
			                                                      {
			                                                      	int GetValue();
			                                                      }

			                                                      public abstract class MyAbstractService
			                                                      {
			                                                      	public abstract string Name { get; }
			                                                      	public virtual int Compute() => 0;
			                                                      }
			                                                      """);

			GeneratorResult result = Generator.RunWithReferences("""
			                                                     using Mockolate;

			                                                     namespace MyCode;

			                                                     public class Program
			                                                     {
			                                                         public static void Main(string[] args)
			                                                         {
			                                                     		_ = Ext.IMyService.CreateMock();
			                                                     		_ = Ext.MyAbstractService.CreateMock();
			                                                         }
			                                                     }
			                                                     """, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").And
				.ContainsKey("Mock.MyAbstractService.g.cs");
			await That(result.Sources["Mock.IMyService.g.cs"])
				.Contains("public int GetValue()");
			await That(result.Sources["Mock.MyAbstractService.g.cs"])
				.Contains("public override string Name").And
				.Contains("public override int Compute()");
		}

		// The emitted override must repeat the base member's declared accessibility, so assert the
		// full signature rather than just the member name: the name alone also appears in the
		// setup/verify surfaces and would match even if no override were emitted at all.
		[Theory]
		[InlineData("public abstract string MyProperty { get; internal set; }", "public override string MyProperty")]
		[InlineData("internal abstract void MyMethod();", "internal override void MyMethod()")]
		[InlineData("internal abstract int MyProperty { get; set; }", "internal override int MyProperty")]
		[InlineData("internal abstract event System.EventHandler MyEvent;",
			"internal override event global::System.EventHandler MyEvent")]
		[InlineData("private protected abstract void MyMethod();", "private protected override void MyMethod()")]
		[InlineData("private protected abstract int MyProperty { get; set; }",
			"private protected override int MyProperty")]
		[InlineData("private protected abstract event System.EventHandler MyEvent;",
			"private protected override event global::System.EventHandler MyEvent")]
		[InlineData("public abstract string MyProperty { get; private protected set; }",
			"public override string MyProperty")]
		public async Task InaccessibleAbstractClassMember_WithInternalsVisibleTo_ShouldBeMocked(
			string member, string expectedOverride)
		{
			MetadataReference external = CompileMyExternalTypeAssembly("abstract class", member,
				grantsInternalsVisibleTo: true);

			GeneratorResult result = Generator.RunWithReferences(CreateMockForMyExternalType, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources).ContainsKey("Mock.MyExternalType.g.cs");
			await That(result.Sources["Mock.MyExternalType.g.cs"]).Contains(expectedOverride);
		}

		// A `private protected` member cannot be reached through a base-typed qualifier from the
		// derived mock, so it must not take part in the `Wraps` forwarding (CS1540). Non-mixed
		// accessors additionally route to the protected setup surface rather than the public one.
		[Theory]
		[InlineData("private protected abstract void MyMethod();")]
		[InlineData("private protected abstract event System.EventHandler MyEvent;")]
		public async Task PrivateProtectedMember_WithInternalsVisibleTo_ShouldNotBeWrapped(string member)
		{
			MetadataReference external = CompileMyExternalTypeAssembly("abstract class", member,
				grantsInternalsVisibleTo: true);

			GeneratorResult result = Generator.RunWithReferences(CreateMockForMyExternalType, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources["Mock.MyExternalType.g.cs"]).DoesNotContain(".Wraps is global::Ext.MyExternalType");
		}

		// Diagnostics being empty here is not evidence of a graceful outcome: `Mock.g.cs` emits a
		// generic `CreateMock` fallback that the call binds to (and CS1061 is suppressed in
		// Generator.NoWarn anyway), so the program compiles and throws MockException at runtime. The
		// user-facing guarantee is the Mockolate0002 error from MockabilityAnalyzer, covered by
		// MockabilityAnalyzerAccessibilityTests. What this test pins is only that the generator
		// refuses to emit a mock whose overrides could not compile.
		[Theory]
		[InlineData("interface", "string MyProperty { get; internal set; }")]
		[InlineData("interface", "internal void MyMethod();")]
		[InlineData("abstract class", "public abstract string MyProperty { get; internal set; }")]
		[InlineData("abstract class", "internal abstract void MyMethod();")]
		[InlineData("abstract class", "internal abstract int MyProperty { get; set; }")]
		[InlineData("abstract class", "internal abstract event System.EventHandler MyEvent;")]
		[InlineData("abstract class", "private protected abstract void MyMethod();")]
		[InlineData("abstract class", "private protected abstract int MyProperty { get; set; }")]
		[InlineData("abstract class", "private protected abstract event System.EventHandler MyEvent;")]
		public async Task InaccessibleRequiredMember_WithoutInternalsVisibleTo_ShouldNotBeMocked(
			string typeKeyword, string member)
		{
			MetadataReference external = CompileMyExternalTypeAssembly(typeKeyword, member,
				grantsInternalsVisibleTo: false);

			GeneratorResult result = Generator.RunWithReferences(CreateMockForMyExternalType, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources).DoesNotContainKey("Mock.MyExternalType.g.cs");
		}

		private static MetadataReference CompileMyExternalTypeAssembly(string typeKeyword, string member,
			bool grantsInternalsVisibleTo)
		{
			string internalsVisibleTo = grantsInternalsVisibleTo
				? """[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TestAssembly")]"""
				: "";
			return ExternalAssembly.Compile($$"""
			                                 {{internalsVisibleTo}}
			                                 namespace Ext;

			                                 public {{typeKeyword}} MyExternalType
			                                 {
			                                 	{{member}}
			                                 }
			                                 """);
		}

		private static MetadataReference CompileMyBaseClassAssembly(bool grantsInternalsVisibleTo)
		{
			string internalsVisibleTo = grantsInternalsVisibleTo
				? """[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TestAssembly")]"""
				: "";
			return ExternalAssembly.Compile($$"""
			                                 {{internalsVisibleTo}}
			                                 namespace Ext;

			                                 public abstract class MyBaseClass
			                                 {
			                                 	public abstract void PublicAbstractMethod();
			                                 	internal virtual void InternalVirtualMethod() { }
			                                 	internal virtual int InternalVirtualProperty { get; set; }
			                                 	protected internal virtual void ProtectedInternalMethod() { }
			                                 	protected internal virtual int ProtectedInternalProperty { get; set; }
			                                 	protected internal virtual event System.EventHandler? ProtectedInternalEvent;
			                                 	public virtual int MixedAccessorProperty { get; protected internal set; }
			                                 }
			                                 """);
		}
	}
}
