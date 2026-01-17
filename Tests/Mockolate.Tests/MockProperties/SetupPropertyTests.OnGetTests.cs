using System.Collections.Generic;
using Mockolate.Setup;

namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class OnGetTests
	{
		[Fact]
		public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnGet.Do((i, _) => { invocations.Add(i); })
				.For(4);

			for (int i = 0; i < 20; i++)
			{
				_ = sut.MyProperty;
			}

			await That(invocations).IsEqualTo([0, 1, 2, 3,]);
		}

		[Theory]
		[InlineData(-2)]
		[InlineData(0)]
		public async Task For_LessThanOne_ShouldThrowArgumentOutOfRangeException(int times)
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			void Act()
			{
				sut.SetupMock.Property.MyProperty
					.OnGet.Do(() => { }).For(times);
			}

			await That(Act).Throws<ArgumentOutOfRangeException>()
				.WithMessage("Times must be greater than zero.").AsPrefix();
		}

		[Fact]
		public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnGet.Do((i, _) => { invocations.Add(i); })
				.When(x => x > 2)
				.For(4);

			for (int i = 0; i < 20; i++)
			{
				_ = sut.MyProperty;
			}

			await That(invocations).IsEqualTo([3, 4, 5, 6,]);
		}

		[Fact]
		public async Task MultipleOnGet_InParallel_ShouldInvokeParallelCallbacksAlways()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnGet.Do(() => { callCount1++; })
				.OnGet.Do(v => { callCount2 += v; }).InParallel()
				.OnGet.Do(v => { callCount3 += v; });

			sut.MyProperty = 1;
			_ = sut.MyProperty;
			sut.MyProperty = 2;
			_ = sut.MyProperty;
			sut.MyProperty = 3;
			_ = sut.MyProperty;
			sut.MyProperty = 4;
			_ = sut.MyProperty;

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(10);
			await That(callCount3).IsEqualTo(6);
		}

		[Theory]
		[InlineData(1, 1)]
		[InlineData(2, 3)]
		[InlineData(3, 6)]
		public async Task MultipleOnGet_Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times,
			int expectedValue)
		{
			int sum = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnGet.Do(v => { sum += v; }).Only(times);

			sut.MyProperty = 1;
			_ = sut.MyProperty;
			sut.MyProperty = 2;
			_ = sut.MyProperty;
			sut.MyProperty = 3;
			_ = sut.MyProperty;
			sut.MyProperty = 4;
			_ = sut.MyProperty;
			sut.MyProperty = 5;
			_ = sut.MyProperty;

			await That(sum).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task MultipleOnGet_OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnGet.Do(() => { callCount1++; })
				.OnGet.Do(v => { callCount2 += v; }).OnlyOnce();

			sut.MyProperty = 1;
			_ = sut.MyProperty;
			sut.MyProperty = 2;
			_ = sut.MyProperty;
			sut.MyProperty = 3;
			_ = sut.MyProperty;
			sut.MyProperty = 4;
			_ = sut.MyProperty;

			await That(callCount1).IsEqualTo(3);
			await That(callCount2).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleOnGet_ShouldInvokeCallbacksInSequence()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnGet.Do(() => { callCount1++; })
				.OnGet.Do(v => { callCount2 += v; });

			sut.MyProperty = 1;
			_ = sut.MyProperty;
			sut.MyProperty = 2;
			_ = sut.MyProperty;
			sut.MyProperty = 3;
			_ = sut.MyProperty;
			sut.MyProperty = 4;
			_ = sut.MyProperty;

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(6);
		}

		[Theory]
		[InlineData(-2)]
		[InlineData(0)]
		public async Task Only_LessThanOne_ShouldThrowArgumentOutOfRangeException(int times)
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			void Act()
			{
				sut.SetupMock.Property.MyProperty
					.OnGet.Do(() => { }).Only(times);
			}

			await That(Act).Throws<ArgumentOutOfRangeException>()
				.WithMessage("Times must be greater than zero.").AsPrefix();
		}

		[Fact]
		public async Task ShouldExecuteWhenPropertyIsRead()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnGet.Do(() => { callCount++; });

			_ = sut.MyProperty;

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task ShouldIncludeIncrementingAccessCounter()
		{
			List<int> invocations = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnGet.Do((i, _) => { invocations.Add(i); });

			for (int i = 0; i < 10; i++)
			{
				_ = sut.MyProperty;
			}

			await That(invocations).IsEqualTo([0, 1, 2, 3, 4, 5, 6, 7, 8, 9,]);
		}

		[Fact]
		public async Task ShouldNotExecuteWhenPropertyIsWrittenToOrOtherPropertyIsRead()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnGet.Do(() => { callCount++; });

			_ = sut.MyOtherProperty;
			sut.MyProperty = 1;

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
		{
			List<int> invocations = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnGet.Do(p => { invocations.Add(p); })
				.When(x => x is > 3 and < 9);

			for (int i = 0; i < 20; i++)
			{
				sut.MyProperty = 10 * i;
				_ = sut.MyProperty;
			}

			await That(invocations).IsEqualTo([40, 50, 60, 70, 80,]);
		}

		[Fact]
		public async Task WithoutCallback_IPropertySetupCallbackBuilder_ShouldNotThrow()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();
			IPropertySetupCallbackBuilder<int> setup =
				(IPropertySetupCallbackBuilder<int>)sut.SetupMock.Property.MyProperty;

			void ActWhen()
			{
				setup.When(_ => true);
			}

			void ActFor()
			{
				setup.For(2);
			}

			void ActInParallel()
			{
				setup.InParallel();
			}

			await That(ActWhen).DoesNotThrow();
			await That(ActFor).DoesNotThrow();
			await That(ActInParallel).DoesNotThrow();
		}

		[Fact]
		public async Task WithoutCallback_IPropertySetupWhenBuilder_ShouldNotThrow()
		{
			IPropertyService mock = Mock.Create<IPropertyService>();
			IPropertySetupCallbackWhenBuilder<int> setup =
				(IPropertySetupCallbackWhenBuilder<int>)mock.SetupMock.Property.MyProperty;

			void ActFor()
			{
				setup.For(2);
			}

			void ActOnly()
			{
				setup.Only(2);
			}

			await That(ActFor).DoesNotThrow();
			await That(ActOnly).DoesNotThrow();
		}

		[Fact]
		public async Task WithValue_ShouldExecuteWhenPropertyIsRead()
		{
			int callCount = 0;
			int receivedValue = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.InitializeWith(4)
				.OnGet.Do(v =>
				{
					callCount++;
					receivedValue = v;
				});

			_ = sut.MyProperty;

			await That(callCount).IsEqualTo(1);
			await That(receivedValue).IsEqualTo(4);
		}

		[Fact]
		public async Task WithValue_ShouldNotExecuteWhenPropertyIsWrittenToOrOtherPropertyIsRead()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnGet.Do(_ => { callCount++; });

			_ = sut.MyOtherProperty;
			sut.MyProperty = 1;

			await That(callCount).IsEqualTo(0);
		}
	}
}
