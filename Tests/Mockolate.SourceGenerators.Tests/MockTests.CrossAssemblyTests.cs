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
			await That(result.Sources["Mock.MyExternalType.g.cs"]).Contains(expectedOverride)
				.Because(
					"the override must repeat the base member's declared accessibility, and the member name alone would also match the setup and verify surfaces");
		}

		[Theory]
		[InlineData("private protected abstract void MyMethod();")]
		[InlineData("private protected abstract event System.EventHandler MyEvent;")]
		public async Task PrivateProtectedMember_WithInternalsVisibleTo_ShouldNotBeWrapped(string member)
		{
			MetadataReference external = CompileMyExternalTypeAssembly("abstract class", member,
				grantsInternalsVisibleTo: true);

			GeneratorResult result = Generator.RunWithReferences(CreateMockForMyExternalType, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources["Mock.MyExternalType.g.cs"]).DoesNotContain(".Wraps is global::Ext.MyExternalType")
				.Because(
					"a `private protected` member cannot be reached through a base-typed qualifier from the derived mock (CS1540)");
		}

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

			await That(result.Diagnostics).IsEmpty()
				.Because(
					"the call binds to the generic `CreateMock` fallback in Mock.g.cs, so the program still compiles and throws MockException at runtime instead");
			await That(result.Sources).DoesNotContainKey("Mock.MyExternalType.g.cs")
				.Because(
					"the generator must refuse to emit a mock whose overrides could not compile; the user-facing guarantee is the Mockolate0002 error");
		}

		[Theory]
		[InlineData("internal abstract void Hidden();", "internal override void Hidden() { }")]
		[InlineData("internal abstract int Hidden { get; set; }", "internal override int Hidden { get; set; }")]
		[InlineData("internal abstract event System.EventHandler Hidden;",
			"internal override event System.EventHandler Hidden;")]
		[InlineData("public abstract string Hidden { get; internal set; }",
			"public override string Hidden { get => null!; internal set { } }")]
		public async Task InaccessibleAbstractMemberAlreadyOverridden_ShouldStillBeMocked(
			string baseMember, string derivedOverride)
		{
			MetadataReference external = ExternalAssembly.Compile($$"""
			                                                       namespace Ext;

			                                                       public abstract class MyBaseType
			                                                       {
			                                                       	{{baseMember}}
			                                                       	public abstract int Visible();
			                                                       }

			                                                       public abstract class MyExternalType : MyBaseType
			                                                       {
			                                                       	{{derivedOverride}}
			                                                       }
			                                                       """);

			GeneratorResult result = Generator.RunWithReferences(CreateMockForMyExternalType, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources).ContainsKey("Mock.MyExternalType.g.cs");
			await That(result.Sources["Mock.MyExternalType.g.cs"])
				.Contains("public override int Visible()").And
				.DoesNotContain("Hidden")
				.Because("the slot is already filled, so neither the unreachable override nor the base declaration may be restated");
		}

		[Theory]
		[InlineData("internal void Hidden();", "void IMyBaseType.Hidden() { }")]
		[InlineData("internal int Hidden { get; set; }",
			"int IMyBaseType.Hidden { get => 0; set { } }")]
		[InlineData("internal event System.EventHandler Hidden;",
			"event System.EventHandler IMyBaseType.Hidden { add { } remove { } }")]
		public async Task InaccessibleInterfaceMemberWithDefaultImplementation_ShouldStillBeMocked(
			string baseMember, string defaultImplementation)
		{
			MetadataReference external = ExternalAssembly.Compile($$"""
			                                                       namespace Ext;

			                                                       public interface IMyBaseType
			                                                       {
			                                                       	{{baseMember}}
			                                                       }

			                                                       public interface MyExternalType : IMyBaseType
			                                                       {
			                                                       	{{defaultImplementation}}
			                                                       	int Visible();
			                                                       }
			                                                       """);

			GeneratorResult result = Generator.RunWithReferences(CreateMockForMyExternalType, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources).ContainsKey("Mock.MyExternalType.g.cs");
			await That(result.Sources["Mock.MyExternalType.g.cs"])
				.Contains("public int Visible()").And
				.DoesNotContain("Hidden")
				.Because("the default implementation fills the slot, so the mock inherits it rather than restating it");
		}

		[Fact]
		public async Task InaccessibleAbstractMemberReDeclaredAsAbstract_ShouldNotBeMocked()
		{
			MetadataReference external = ExternalAssembly.Compile("""
			                                                      namespace Ext;

			                                                      public abstract class MyBaseType
			                                                      {
			                                                      	internal abstract void Hidden();
			                                                      }

			                                                      public abstract class MyExternalType : MyBaseType
			                                                      {
			                                                      	internal abstract override void Hidden();
			                                                      }
			                                                      """);

			GeneratorResult result = Generator.RunWithReferences(CreateMockForMyExternalType, [external,]);

			await That(result.Diagnostics).IsEmpty();
			await That(result.Sources).DoesNotContainKey("Mock.MyExternalType.g.cs")
				.Because(
					"an `abstract override` re-declaration continues the slot without filling it, so the member is still the mock's obligation");
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
