using System.Threading.Tasks;
using Xunit;
using Verifier = Mockolate.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<Mockolate.Analyzers.MockabilityAnalyzer>;

namespace Mockolate.Analyzers.Tests;

public class MockabilityAnalyzerAccessibilityTests
{
	[Theory]
	[InlineData("internal abstract void MyMember();", "Ext.MyExternalType.MyMember()")]
	[InlineData("internal abstract int MyMember { get; set; }", "Ext.MyExternalType.MyMember.get")]
	[InlineData("internal abstract event System.EventHandler MyMember;", "Ext.MyExternalType.MyMember")]
	[InlineData("public abstract string MyMember { get; internal set; }", "Ext.MyExternalType.MyMember.set")]
	public async Task WhenAbstractMemberOfReferencedAssemblyIsNotAccessible_ShouldBeFlagged(
		string member, string reportedMember) => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			ExternalType("abstract class", member),
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("Ext.MyExternalType",
					$"the member '{reportedMember}' must be implemented, but it is not accessible from this assembly")
		);

	[Fact]
	public async Task WhenInterfaceAccessorOfReferencedAssemblyIsNotAccessible_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			ExternalType("interface", "string MyMember { get; internal set; }"),
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("Ext.MyExternalType",
					"the member 'Ext.MyExternalType.MyMember.set' must be implemented, but it is not accessible from this assembly")
		);

	[Fact]
	public async Task WhenInaccessibleAbstractMemberIsInheritedFromABaseType_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			"""
			namespace Ext
			{
				public abstract class MyBaseType
				{
					internal abstract void MyMember();
				}

				public abstract class MyExternalType : MyBaseType
				{
				}
			}
			""",
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("Ext.MyExternalType",
					"the member 'Ext.MyBaseType.MyMember()' must be implemented, but it is not accessible from this assembly")
		);

	[Theory]
	[InlineData("internal abstract void MyMember();", "internal override void MyMember() { }")]
	[InlineData("internal abstract int MyMember { get; set; }", "internal override int MyMember { get; set; }")]
	[InlineData("internal abstract event System.EventHandler MyMember;",
		"internal override event System.EventHandler MyMember;")]
	[InlineData("public abstract string MyMember { get; internal set; }",
		"public override string MyMember { get => null!; internal set { } }")]
	[InlineData("public abstract string MyMember { internal get; set; }",
		"public override string MyMember { internal get => null!; set { } }")]
	[InlineData("private protected abstract int MyMember { get; set; }",
		"private protected override int MyMember { get; set; }")]
	public async Task WhenInaccessibleAbstractMemberIsAlreadyOverridden_ShouldNotBeFlagged(
		string baseMember, string derivedOverride) => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			$$"""
			  namespace Ext
			  {
			  	public abstract class MyBaseType
			  	{
			  		{{baseMember}}
			  	}

			  	public abstract class MyExternalType : MyBaseType
			  	{
			  		{{derivedOverride}}
			  	}
			  }
			  """);

	[Fact]
	public async Task WhenInaccessibleInterfaceMemberHasADefaultImplementation_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			"""
			namespace Ext
			{
				public interface IMyBaseType
				{
					internal void MyMember();
				}

				public interface MyExternalType : IMyBaseType
				{
					void IMyBaseType.MyMember() { }
				}
			}
			""");

	[Fact]
	public async Task WhenInaccessibleAbstractMemberIsReDeclaredAsAbstract_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			"""
			namespace Ext
			{
				public abstract class MyBaseType
				{
					internal abstract void MyMember();
				}

				public abstract class MyExternalType : MyBaseType
				{
					internal abstract override void MyMember();
				}
			}
			""",
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("Ext.MyExternalType",
					"the member 'Ext.MyExternalType.MyMember()' must be implemented, but it is not accessible from this assembly")
		);

	[Theory]
	[InlineData("internal abstract void MyMember();")]
	[InlineData("public abstract string MyMember { get; internal set; }")]
	public async Task WhenInternalsAreVisibleToTheMockAssembly_ShouldNotBeFlagged(string member) => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			ExternalType("abstract class", member, internalsVisibleToTestProject: true));

	[Theory]
	[InlineData("internal virtual void MyMember() { }")]
	[InlineData("internal virtual int MyMember { get; set; }")]
	public async Task WhenInaccessibleMemberIsOnlyVirtual_ShouldNotBeFlagged(string member) => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			ExternalType("class", member));

	[Fact]
	public async Task WhenProtectedInternalAbstractMember_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			ExternalType("abstract class", "protected internal abstract void MyMember();"));

	[Theory]
	[InlineData("protected abstract void MyMember(MyConfiguration configuration);",
		"Ext.MyExternalType.MyMember(Ext.MyExternalType.MyConfiguration)", "Ext.MyExternalType.MyConfiguration")]
	[InlineData("protected abstract MyConfiguration MyMember();", "Ext.MyExternalType.MyMember()",
		"Ext.MyExternalType.MyConfiguration")]
	[InlineData("protected abstract MyConfiguration MyMember { get; set; }", "Ext.MyExternalType.MyMember",
		"Ext.MyExternalType.MyConfiguration")]
	[InlineData("protected abstract void MyMember<T>() where T : MyConfiguration;",
		"Ext.MyExternalType.MyMember<T>()", "Ext.MyExternalType.MyConfiguration")]
	[InlineData("protected abstract System.Collections.Generic.List<MyConfiguration> MyMember();",
		"Ext.MyExternalType.MyMember()", "System.Collections.Generic.List<Ext.MyExternalType.MyConfiguration>")]
	public async Task WhenAbstractMemberSignatureUsesAnInaccessibleType_ShouldBeFlagged(
		string member, string reportedMember, string reportedType) => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			ExternalType("abstract class", $$"""
			                                 {{member}}
			                                 		protected internal class MyConfiguration { }
			                                 """),
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("Ext.MyExternalType",
					$"the member '{reportedMember}' must be implemented, but its signature uses the type '{reportedType}', which is not accessible from this assembly")
		);

	[Fact]
	public async Task WhenAbstractMemberSignatureUsesATypeVisibleToTheMockAssembly_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			ExternalType("abstract class", """
			                               protected abstract void MyMember(MyConfiguration configuration);
			                               		protected internal class MyConfiguration { }
			                               """, internalsVisibleToTestProject: true));

	[Theory]
	[InlineData("private protected abstract void MyMember();", "Ext.MyExternalType.MyMember()")]
	[InlineData("private protected abstract int MyMember { get; set; }", "Ext.MyExternalType.MyMember.get")]
	[InlineData("private protected abstract event System.EventHandler MyMember;", "Ext.MyExternalType.MyMember")]
	[InlineData("public abstract string MyMember { get; private protected set; }", "Ext.MyExternalType.MyMember.set")]
	public async Task WhenPrivateProtectedAbstractMemberIsNotAccessible_ShouldBeFlagged(
		string member, string reportedMember) => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			MockCreation,
			ExternalType("abstract class", member),
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("Ext.MyExternalType",
					$"the member '{reportedMember}' must be implemented, but it is not accessible from this assembly")
		);

	[Theory]
	[InlineData("private protected abstract void MyMember();")]
	[InlineData("private protected abstract event System.EventHandler MyMember;")]
	[InlineData("public abstract string MyMember { get; private protected set; }")]
	public async Task WhenPrivateProtectedAbstractMemberIsVisibleToTheMockAssembly_ShouldNotBeFlagged(string member)
		=> await Verifier
			.VerifyAnalyzerWithReferencedProjectAsync(
				MockCreation,
				ExternalType("abstract class", member, internalsVisibleToTestProject: true));

	[Fact]
	public async Task WhenAdditionalInterfaceHasAnInaccessibleMember_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			ImplementingMockCreation,
			ExternalType("interface", "internal void MyMember();"),
			Verifier.Diagnostic(Rules.MockabilityRule)
				.WithLocation(0)
				.WithArguments("Ext.MyExternalType",
					"the member 'Ext.MyExternalType.MyMember()' must be implemented, but it is not accessible from this assembly")
		);

	[Fact]
	public async Task WhenAdditionalInterfaceMemberIsVisibleToTheMockAssembly_ShouldNotBeFlagged() => await Verifier
		.VerifyAnalyzerWithReferencedProjectAsync(
			ImplementingMockCreation,
			ExternalType("interface", "internal void MyMember();", internalsVisibleToTestProject: true));

	private const string MockCreation = """
	                                    namespace Mockolate
	                                    {
	                                    	internal static partial class MockExtensionsForMyExternalType
	                                    	{
	                                    		extension(Ext.MyExternalType mock)
	                                    		{
	                                    			public static Ext.MyExternalType CreateMock() => default!;
	                                    		}
	                                    	}
	                                    }

	                                    namespace MyNamespace
	                                    {
	                                    	public class MyClass
	                                    	{
	                                    		public void MyTest()
	                                    		{
	                                    			_ = {|#0:Ext.MyExternalType|}.CreateMock();
	                                    		}
	                                    	}
	                                    }
	                                    """;

	private const string ImplementingMockCreation = """
	                                                namespace Mockolate
	                                                {
	                                                	internal static partial class MockExtensionsForIMyInterface
	                                                	{
	                                                		extension(MyNamespace.IMyInterface mock)
	                                                		{
	                                                			public static MyNamespace.IMyInterface CreateMock() => default!;
	                                                			public MyNamespace.IMyInterface Implementing<TInterface>() => default!;
	                                                		}
	                                                	}
	                                                }

	                                                namespace MyNamespace
	                                                {
	                                                	public interface IMyInterface
	                                                	{
	                                                	}

	                                                	public class MyClass
	                                                	{
	                                                		public void MyTest()
	                                                		{
	                                                			IMyInterface.CreateMock().Implementing<{|#0:Ext.MyExternalType|}>();
	                                                		}
	                                                	}
	                                                }
	                                                """;

	private static string ExternalType(string typeKeyword, string member,
		bool internalsVisibleToTestProject = false)
	{
		string internalsVisibleTo = internalsVisibleToTestProject
			? """[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TestProject")]"""
			: "";
		return $$"""
		         {{internalsVisibleTo}}
		         namespace Ext
		         {
		         	public {{typeKeyword}} MyExternalType
		         	{
		         		{{member}}
		         	}
		         }
		         """;
	}
}
