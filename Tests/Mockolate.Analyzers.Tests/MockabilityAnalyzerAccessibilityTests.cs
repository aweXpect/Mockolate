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
