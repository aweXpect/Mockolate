using System.Collections.Generic;

namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class OnSetTests
	{
		[Fact]
		public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnSet((i, _, _) => { invocations.Add(i); })
				.For(4);

			for (int i = 0; i < 20; i++)
			{
				sut.MyProperty = i * 2;
			}

			await That(invocations).IsEqualTo([0, 1, 2, 3,]);
		}

		[Fact]
		public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty
				.OnSet((i, _, _) => { invocations.Add(i); })
				.When(x => x > 2)
				.For(4);

			for (int i = 0; i < 20; i++)
			{
				sut.MyProperty = i * 2;
			}

			await That(invocations).IsEqualTo([3, 4, 5, 6,]);
		}

		[Fact]
		public async Task MultipleOnSet_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.InitializeWith(2)
				.OnSet(() => { callCount1++; })
				.OnSet((old, @new) => { callCount2 += old * @new; });

			sut.MyProperty = 4; // 2 * 4 = 8
			sut.MyProperty = 6; // 4 * 6 = 24

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(8 + 24);
		}

		[Fact]
		public async Task ShouldExecuteWhenPropertyIsWrittenTo()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnSet(() => { callCount++; });

			sut.MyProperty = 5;

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task ShouldNotExecuteWhenPropertyIsReadOrOtherPropertyIsWrittenTo()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnSet(() => { callCount++; });

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
				.OnSet((i, _, _) => { invocations.Add(i); })
				.When(x => x is > 3 and < 9);

			for (int i = 0; i < 20; i++)
			{
				sut.MyProperty = i * 2;
			}

			await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
		}

		[Fact]
		public async Task WithValue_ShouldExecuteWhenPropertyIsWrittenTo()
		{
			int receivedOldValue = 0;
			int receivedNewValue = 0;
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.InitializeWith(4)
				.OnSet((oldValue, newValue) =>
				{
					callCount++;
					receivedOldValue = oldValue;
					receivedNewValue = newValue;
				});

			sut.MyProperty = 6;

			await That(callCount).IsEqualTo(1);
			await That(receivedOldValue).IsEqualTo(4);
			await That(receivedNewValue).IsEqualTo(6);
		}

		[Fact]
		public async Task WithValue_ShouldNotExecuteWhenPropertyIsReadOrOtherPropertyIsWrittenTo()
		{
			int callCount = 0;
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.OnSet((_, _) => { callCount++; });

			sut.MyOtherProperty = 1;
			_ = sut.MyProperty;

			await That(callCount).IsEqualTo(0);
		}
	}
}
