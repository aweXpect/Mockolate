#if NET8_0_OR_GREATER
using System.Collections.Generic;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	public sealed class StaticInterfaceCombinationTests
	{
		[Fact]
		public async Task StaticBase_WithExtraInterface_ShouldCompile()
		{
			IServiceWithAllStaticAbstracts sut = IServiceWithAllStaticAbstracts.CreateMock()
				.Implementing<IExtraServiceForStaticCombination>();

			await That(sut is null).IsFalse();
		}

		[Fact]
		public async Task StaticBase_WithExtraInterface_ShouldDispatchStaticEvent()
		{
			List<int> receivedCalls = [];
			IServiceWithAllStaticAbstracts sut = IServiceWithAllStaticAbstracts.CreateMock()
				.Implementing<IExtraServiceForStaticCombination>();

			Subscribe<Mock.MockTests_StaticInterfaceCombinationTests_IServiceWithAllStaticAbstracts__MockTests_StaticInterfaceCombinationTests_IExtraServiceForStaticCombination>(Callback);

			sut.Mock.RaiseStatic.StaticEvent(2);
			sut.Mock.RaiseStatic.StaticEvent(5);

			await That(receivedCalls).IsEqualTo([2, 5,]);
			await That(sut.Mock.VerifyStatic.StaticEvent.Subscribed()).Once();

			Unsubscribe<Mock.MockTests_StaticInterfaceCombinationTests_IServiceWithAllStaticAbstracts__MockTests_StaticInterfaceCombinationTests_IExtraServiceForStaticCombination>(Callback);

			void Callback(int value)
			{
				receivedCalls.Add(value);
			}

			static void Subscribe<T>(Action<int> callback)
				where T : IServiceWithAllStaticAbstracts
			{
				T.StaticEvent += callback;
			}

			static void Unsubscribe<T>(Action<int> callback)
				where T : IServiceWithAllStaticAbstracts
			{
				T.StaticEvent -= callback;
			}
		}

		[Fact]
		public async Task StaticBase_WithExtraInterface_ShouldDispatchStaticMethod()
		{
			IServiceWithAllStaticAbstracts sut = IServiceWithAllStaticAbstracts.CreateMock()
				.Implementing<IExtraServiceForStaticCombination>();
			sut.Mock.SetupStatic.StaticMethod().Returns(7);

			int result = CallStaticMethod<Mock.MockTests_StaticInterfaceCombinationTests_IServiceWithAllStaticAbstracts>();

			await That(result).IsEqualTo(7);
			await That(sut.Mock.VerifyStatic.StaticMethod()).Once();

			static int CallStaticMethod<T>()
				where T : IServiceWithAllStaticAbstracts
			{
				return T.StaticMethod();
			}
		}

		[Fact]
		public async Task StaticBase_WithExtraInterface_ShouldDispatchStaticProperty()
		{
			IServiceWithAllStaticAbstracts sut = IServiceWithAllStaticAbstracts.CreateMock()
				.Implementing<IExtraServiceForStaticCombination>();
			sut.Mock.SetupStatic.StaticProperty.Returns(11);

			int result = CallStaticProperty<Mock.MockTests_StaticInterfaceCombinationTests_IServiceWithAllStaticAbstracts>();

			await That(result).IsEqualTo(11);
			await That(sut.Mock.VerifyStatic.StaticProperty.Got()).Once();

			static int CallStaticProperty<T>()
				where T : IServiceWithAllStaticAbstracts
			{
				return T.StaticProperty;
			}
		}

		public interface IServiceWithAllStaticAbstracts
		{
			bool InstanceFlag { get; set; }
			static abstract int StaticProperty { get; set; }
			static abstract int StaticMethod();
			static abstract event Action<int> StaticEvent;
		}

		public interface IExtraServiceForStaticCombination
		{
			void Run();
		}
	}
}
#endif
