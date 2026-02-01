using System.Collections.Generic;

namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class OnSetTests
	{
		[Fact]
		public async Task MultipleOnSet_InParallel_ShouldInvokeCallbacksInSequence()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.InitializeWith(2)
				.OnSet.Do(() => { callCount1++; })
				.OnSet.Do((_, v) => { callCount2 += v; }).InParallel()
				.OnSet.Do((_, v) => { callCount3 += v; });

			sut.MyProperty = 4;
			sut.MyProperty = 6;
			sut.MyProperty = 8;
			sut.MyProperty = 10;

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(4 + 6 + 8 + 10);
			await That(callCount3).IsEqualTo(6 + 10);
		}

		[Theory]
		[InlineData(1, 1)]
		[InlineData(2, 3)]
		[InlineData(3, 6)]
		public async Task MultipleOnSet_Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times,
			int expectedValue)
		{
			int sum = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnSet.Do((_, v) => { sum += v; }).Only(times);

			sut.MyProperty = 1;
			sut.MyProperty = 2;
			sut.MyProperty = 3;
			sut.MyProperty = 4;
			sut.MyProperty = 5;

			await That(sum).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task MultipleOnSet_OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnSet.Do(() => { callCount1++; })
				.OnSet.Do((_, v) => { callCount2 += v; }).OnlyOnce();

			sut.MyProperty = 1;
			sut.MyProperty = 2;
			sut.MyProperty = 3;
			sut.MyProperty = 4;

			await That(callCount1).IsEqualTo(3);
			await That(callCount2).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleOnSet_ShouldInvokeCallbacksInSequence()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.InitializeWith(2)
				.OnSet.Do(() => { callCount1++; })
				.OnSet.Do((_, v) => { callCount2 += v; });

			sut.MyProperty = 4;
			sut.MyProperty = 6;
			sut.MyProperty = 8;
			sut.MyProperty = 10;

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(6 + 10);
		}

		[Fact]
		public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnSet.Do((i, _) => { invocations.Add(i); })
				.Only(4);

			for (int i = 0; i < 20; i++)
			{
				sut.MyProperty = i * 2;
			}

			await That(invocations).IsEqualTo([0, 1, 2, 3,]);
		}

		[Fact]
		public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnSet.Do((i, _) => { invocations.Add(i); })
				.When(x => x > 2)
				.Only(4);

			for (int i = 0; i < 20; i++)
			{
				sut.MyProperty = i * 2;
			}

			await That(invocations).IsEqualTo([3, 4, 5, 6,]);
		}

		[Fact]
		public async Task ShouldExecuteWhenPropertyIsWrittenTo()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnSet.Do(() => { callCount++; });

			sut.MyProperty = 5;

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task ShouldNotExecuteWhenPropertyIsReadOrOtherPropertyIsWrittenTo()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnSet.Do(() => { callCount++; });

			sut.MyOtherProperty = 1;
			_ = sut.MyProperty;

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
		{
			List<int> invocations = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnSet.Do((i, _) => { invocations.Add(i); })
				.When(x => x is > 3 and < 9);

			for (int i = 0; i < 20; i++)
			{
				sut.MyProperty = i * 2;
			}

			await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
		}

		[Fact]
		public async Task WithIndexAndValue_ShouldExecuteWhenPropertyIsWrittenTo()
		{
			List<int> receivedIndexes = [];
			List<int> receivedValues = [];
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.InitializeWith(4)
				.OnSet.Do((i, v) =>
				{
					receivedIndexes.Add(i);
					receivedValues.Add(v);
				});

			sut.MyProperty = 6;
			sut.MyProperty = 8;
			sut.MyProperty = 10;

			await That(receivedIndexes).IsEqualTo([0, 1, 2,]);
			await That(receivedValues).IsEqualTo([6, 8, 10,]);
		}

		[Fact]
		public async Task WithValue_ShouldExecuteWhenPropertyIsWrittenTo()
		{
			int receivedNewValue = 0;
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.InitializeWith(4)
				.OnSet.Do(v =>
				{
					callCount++;
					receivedNewValue = v;
				});

			sut.MyProperty = 6;

			await That(callCount).IsEqualTo(1);
			await That(receivedNewValue).IsEqualTo(6);
		}

		[Fact]
		public async Task WithValue_ShouldNotExecuteWhenPropertyIsReadOrOtherPropertyIsWrittenTo()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnSet.Do((_, _) => { callCount++; });

			sut.MyOtherProperty = 1;
			_ = sut.MyProperty;

			await That(callCount).IsEqualTo(0);
		}
	}
}
