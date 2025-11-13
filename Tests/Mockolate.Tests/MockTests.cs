using Mockolate.Exceptions;
using Mockolate.Generated;
using Mockolate.Tests.TestHelpers;
using static Mockolate.BaseClass;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task Create_BaseClassWithMultipleConstructors()
	{
		void Act()
			=> _ = Mock.Create<MyServiceBaseWithMultipleConstructors>(WithConstructorParameters(5));

		await That(Act).DoesNotThrow();
	}

	[Fact]
	public async Task Create_BaseClassWithoutConstructor_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MyBaseClassWithoutConstructor>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Could not find any constructor at all for the base type 'Mockolate.Tests.MockTests.MyBaseClassWithoutConstructor'. Therefore mocking is not supported!");
	}

	[Fact]
	public async Task Create_WithUseBaseClassAsDefaultValue_ShouldUseBaseClassValuesInConstructor()
	{
		MyServiceBaseWithVirtualCallsInConstructor mock =
			Mock.Create<MyServiceBaseWithVirtualCallsInConstructor>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue
			});

		int value = mock.VirtualProperty;

		await That(mock.VerifyMock.Invoked.VirtualMethod()).Once();
		await That(value).IsEqualTo(1);
	}

	[Fact]
	public async Task Create_SealedClass_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With1AdditionalInterface_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With2AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With3AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With4AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With5AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With6AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With7AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock
				.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
					IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With8AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock
				.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
					IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_WithConstructorParameters_SealedClass_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass>(WithConstructorParameters());

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_WithMatchingParameters_ShouldCreateMock()
	{
		MyBaseClassWithConstructor Act()
			=> _ = Mock.Create<MyBaseClassWithConstructor>(WithConstructorParameters("foo"));

		await That(Act).DoesNotThrow().AndWhoseResult.IsNotNull();
	}

	[Fact]
	public async Task Create_WithMockBehavior_SealedClass_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass>(MockBehavior.Default);

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_WithRequiredParameters_WithEmptyParameters_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MyBaseClassWithConstructor>(WithConstructorParameters());

		await That(Act).Throws<MockException>()
			.WithMessage(
				"No parameterless constructor found for 'Mockolate.Tests.MockTests.MyBaseClassWithConstructor'. Please provide constructor parameters.");
	}

	[Fact]
	public async Task Create_WithRequiredParameters_WithoutParameters_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MyBaseClassWithConstructor>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"No parameterless constructor found for 'Mockolate.Tests.MockTests.MyBaseClassWithConstructor'. Please provide constructor parameters.");
	}

	[Fact]
	public async Task Create_WithTooManyParameters_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MyBaseClassWithConstructor>(WithConstructorParameters("foo", 1, 2));

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Could not find any constructor for 'Mockolate.Tests.MockTests.MyBaseClassWithConstructor' that matches the 3 given parameters (foo, 1, 2).");
	}

	[Fact]
	public async Task DoubleNestedInterfaces_ShouldStillWork()
	{
		Nested.Nested2.IMyDoubleNestedService mock = Mock.Create<Nested.Nested2.IMyDoubleNestedService>();
		mock.SetupMock.Property.IsValid.InitializeWith(true);

		bool result = mock.IsValid;

		await That(result).IsTrue();
	}

	public class MyServiceBaseWithVirtualCallsInConstructor
	{
		// ReSharper disable VirtualMemberCallInConstructor
		public MyServiceBaseWithVirtualCallsInConstructor()
		{
			int[] values = VirtualMethod();
			VirtualProperty = values[0];
		}
		// ReSharper restore VirtualMemberCallInConstructor

		public virtual int VirtualProperty { get; set; }

		public virtual int[] VirtualMethod() => [1, 2,];
	}

	public class MyServiceBaseWithMultipleConstructors
	{
		public MyServiceBaseWithMultipleConstructors(int initialValue)
		{
		}

		public MyServiceBaseWithMultipleConstructors(DateTime initialValue)
		{
		}
	}

	public interface IMyService
	{
		public bool? IsValid { get; set; }
		public int Counter { get; set; }

		public int Multiply(int value, int? multiplier);

		public void SetIsValid(bool isValid, Func<bool>? predicate);
	}

	public class MyBaseClassWithConstructor
	{
		public MyBaseClassWithConstructor(string text)
		{
			Text = text;
		}

		public int Number { get; }
		public string Text { get; }
		public virtual string VirtualMethod() => Text;
	}

	public class MyBaseClassWithoutConstructor
	{
		private MyBaseClassWithoutConstructor()
		{
		}

		public int Number { get; }
	}

	public sealed class Nested
	{
		public sealed class Nested2
		{
			public interface IMyDoubleNestedService
			{
				bool IsValid { get; }
			}
		}
	}

	public sealed class MySealedClass
	{
	}
}
