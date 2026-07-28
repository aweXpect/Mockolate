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
