using System.Collections.Generic;

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
				.OnGet.Do((i, _) => { invocations.Add(i); })
				.When(x => x is > 3 and < 9);

			for (int i = 0; i < 20; i++)
			{
				_ = sut.MyProperty;
			}

			await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
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
