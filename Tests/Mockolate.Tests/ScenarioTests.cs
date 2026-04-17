using Mockolate.Tests.Protected;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed class ScenarioTests
{
	public sealed class InScenarioTests
	{
		[Fact]
		public async Task InScenario_IndexerArity2_ShouldScopeTheSetup()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup[1, 2].Returns(42);
			((IMock)sut).MockRegistry.Scenario = "a";

			int result = sut[1, 2];

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task InScenario_IndexerArity3_ShouldScopeTheSetup()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup[1, 2, 3].Returns(42);
			((IMock)sut).MockRegistry.Scenario = "a";

			int result = sut[1, 2, 3];

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task InScenario_IndexerArity4_ShouldScopeTheSetup()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup[1, 2, 3, 4].Returns(42);
			((IMock)sut).MockRegistry.Scenario = "a";

			int result = sut[1, 2, 3, 4];

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task InScenario_IndexerGetter_ShouldScopeTheSetup()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup[1].Returns(42);
			sut.Mock.Setup[1].Returns(7);

			int resultDefault = sut[1];
			((IMock)sut).MockRegistry.Scenario = "a";
			int resultScoped = sut[1];

			await That(resultDefault).IsEqualTo(7);
			await That(resultScoped).IsEqualTo(42);
		}

		[Fact]
		public async Task InScenario_LambdaOverload_ShouldRegisterAllSetupsInTheScope()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("scoped", scope =>
			{
				scope.Setup.ReturnMethod1(1).Returns(42);
				scope.Setup.ReturnMethod2(1, 2).Returns(99);
			});
			((IMock)sut).MockRegistry.Scenario = "scoped";

			int r1 = sut.ReturnMethod1(1);
			int r2 = sut.ReturnMethod2(1, 2);

			await That(r1).IsEqualTo(42);
			await That(r2).IsEqualTo(99);
		}

		[Fact]
		public async Task InScenario_LambdaOverload_ShouldReturnMockForToAllowFurtherChaining()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock
				.InScenario("a", s => s.Setup.ReturnMethod0().Returns(1))
				.InScenario("b", s => s.Setup.ReturnMethod0().Returns(2));

			((IMock)sut).MockRegistry.Scenario = "a";
			int ra = sut.ReturnMethod0();
			((IMock)sut).MockRegistry.Scenario = "b";
			int rb = sut.ReturnMethod0();

			await That(ra).IsEqualTo(1);
			await That(rb).IsEqualTo(2);
		}

		[Fact]
		public async Task InScenario_SetupsShouldNotLeakIntoDefaultScope()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("scoped").Setup.ReturnMethod1(1).Returns(42);
			sut.Mock.Setup.ReturnMethod1(1).Returns(7);

			int result = sut.ReturnMethod1(1);

			await That(result).IsEqualTo(7);
		}

		[Fact]
		public async Task InScenario_ShouldActivateWhenScenarioMatches()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("scoped").Setup.ReturnMethod1(1).Returns(42);
			sut.Mock.Setup.ReturnMethod1(1).Returns(7);
			((IMock)sut).MockRegistry.Scenario = "scoped";

			int result = sut.ReturnMethod1(1);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task InScenario_ShouldFallBackToDefaultScopeWhenNoMatchingSetup()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.Setup.ReturnMethod1(1).Returns(7);
			((IMock)sut).MockRegistry.Scenario = "scoped";

			int result = sut.ReturnMethod1(1);

			await That(result).IsEqualTo(7);
		}
	}

	public sealed class TransitionToTests
	{
		[Fact]
		public async Task Method_ReturnArity0_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup.ReturnMethod0().Returns(1).TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			int result = sut.ReturnMethod0();

			await That(result).IsEqualTo(1);
			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Method_ReturnArity1_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup.ReturnMethod1(It.IsAny<int>()).Returns(1).TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			int result = sut.ReturnMethod1(42);

			await That(result).IsEqualTo(1);
			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Method_ReturnArity2_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup.ReturnMethod2(It.IsAny<int>(), It.IsAny<int>()).Returns(1).TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			int result = sut.ReturnMethod2(1, 2);

			await That(result).IsEqualTo(1);
			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Method_ReturnArity3_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup
				.ReturnMethod3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.Returns(1).TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			int result = sut.ReturnMethod3(1, 2, 3);

			await That(result).IsEqualTo(1);
			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Method_ReturnArity4_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup
				.ReturnMethod4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.Returns(1).TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			int result = sut.ReturnMethod4(1, 2, 3, 4);

			await That(result).IsEqualTo(1);
			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Method_ReturnArity5_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup
				.ReturnMethod5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.Returns(1).TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			int result = sut.ReturnMethod5(1, 2, 3, 4, 5);

			await That(result).IsEqualTo(1);
			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Method_VoidArity0_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup.VoidMethod0().TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			sut.VoidMethod0();

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Method_VoidArity1_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup.VoidMethod1(It.IsAny<int>()).TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			sut.VoidMethod1(42);

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Method_VoidArity2_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup.VoidMethod2(It.IsAny<int>(), It.IsAny<int>()).TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			sut.VoidMethod2(1, 2);

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Method_VoidArity3_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup
				.VoidMethod3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			sut.VoidMethod3(1, 2, 3);

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Method_VoidArity4_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup
				.VoidMethod4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			sut.VoidMethod4(1, 2, 3, 4);

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Method_VoidArity5_ShouldSwitchScenarioWhenInvoked()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup
				.VoidMethod5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			sut.VoidMethod5(1, 2, 3, 4, 5);

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Property_Getter_ShouldNotFireOnWrite()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup.Property.OnGet.TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			sut.Property = 42;

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("a");
		}

		[Fact]
		public async Task Property_Getter_ShouldSwitchScenarioOnRead()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup.Property.OnGet.TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			_ = sut.Property;

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task Property_Setter_ShouldNotFireOnRead()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup.Property.OnSet.TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			_ = sut.Property;

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("a");
		}

		[Fact]
		public async Task Property_Setter_ShouldSwitchScenarioOnWrite()
		{
			IScenarioService sut = IScenarioService.CreateMock();

			sut.Mock.InScenario("a").Setup.Property.OnSet.TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			sut.Property = 42;

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}
	}

	public sealed class ProtectedTests
	{
		[Fact]
		public async Task InScenario_LambdaOverload_ShouldSupportSetupProtected()
		{
			ProtectedMockTests.MyProtectedClass sut = ProtectedMockTests.MyProtectedClass.CreateMock();

			sut.Mock.InScenario("a", s =>
			{
				s.SetupProtected.MyProtectedMethod(It.IsAny<string>()).Returns("scoped");
			});
			((IMock)sut).MockRegistry.Scenario = "a";

			string result = sut.InvokeMyProtectedMethod("anything");

			await That(result).IsEqualTo("scoped");
		}

		[Fact]
		public async Task InScenario_SetupProtectedMethod_ShouldScopeTheSetup()
		{
			ProtectedMockTests.MyProtectedClass sut = ProtectedMockTests.MyProtectedClass.CreateMock();

			sut.Mock.InScenario("a").SetupProtected.MyProtectedMethod(It.IsAny<string>()).Returns("scoped");
			sut.Mock.SetupProtected.MyProtectedMethod(It.IsAny<string>()).Returns("default");

			string defaultResult = sut.InvokeMyProtectedMethod("x");
			((IMock)sut).MockRegistry.Scenario = "a";
			string scopedResult = sut.InvokeMyProtectedMethod("x");

			await That(defaultResult).IsEqualTo("default");
			await That(scopedResult).IsEqualTo("scoped");
		}

		[Fact]
		public async Task InScenario_SetupProtectedProperty_ShouldScopeTheSetup()
		{
			ProtectedMockTests.MyProtectedClass sut = ProtectedMockTests.MyProtectedClass.CreateMock();

			sut.Mock.InScenario("a").SetupProtected.MyProtectedProperty.InitializeWith(99);
			sut.Mock.SetupProtected.MyProtectedProperty.InitializeWith(1);
			((IMock)sut).MockRegistry.Scenario = "a";

			int result = sut.GetMyProtectedProperty();

			await That(result).IsEqualTo(99);
		}

		[Fact]
		public async Task TransitionTo_FromProtectedMethod_ShouldSwitchScenario()
		{
			ProtectedMockTests.MyProtectedClass sut = ProtectedMockTests.MyProtectedClass.CreateMock();

			sut.Mock.InScenario("a").SetupProtected.MyProtectedMethod(It.IsAny<string>())
				.Returns("scoped").TransitionTo("b");
			((IMock)sut).MockRegistry.Scenario = "a";

			_ = sut.InvokeMyProtectedMethod("anything");

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}
	}
}
