using System.Collections.Concurrent;
using System.Collections.Generic;

#if NET8_0_OR_GREATER
namespace Mockolate.Tests;

public sealed partial class MockTests
{
	public sealed class StaticInterfaceMembersTests
	{
		[Fact]
		public async Task WithAbstractStaticEvent()
		{
			List<int> receivedCalls = [];
			IMyServiceWithAbstractStaticMembers sut = IMyServiceWithAbstractStaticMembers.CreateMock();

			SubscribeAbstractStaticEvent<Mock.MockTests_StaticInterfaceMembersTests_IMyServiceWithAbstractStaticMembers>(Callback);

			sut.Mock.RaiseStatic.AbstractStaticEvent(1);
			sut.Mock.RaiseStatic.AbstractStaticEvent(3);

			await That(receivedCalls).IsEqualTo([1, 3,]);
			await That(sut.Mock.VerifyStatic.AbstractStaticEvent.Subscribed()).Once();
			await That(sut.Mock.VerifyStatic.AbstractStaticEvent.Unsubscribed()).Never();
			UnsubscribeAbstractStaticEvent<Mock.MockTests_StaticInterfaceMembersTests_IMyServiceWithAbstractStaticMembers>(Callback);
			await That(sut.Mock.VerifyStatic.AbstractStaticEvent.Unsubscribed()).Once();

			void Callback(int value)
			{
				receivedCalls.Add(value);
			}

			static void SubscribeAbstractStaticEvent<T>(Action<int> callback)
				where T : IMyServiceWithAbstractStaticMembers
			{
				T.AbstractStaticEvent += callback;
			}

			static void UnsubscribeAbstractStaticEvent<T>(Action<int> callback)
				where T : IMyServiceWithAbstractStaticMembers
			{
				T.AbstractStaticEvent -= callback;
			}
		}

		[Fact]
		public async Task WithAbstractStaticEvent_ShouldWorkInParallel()
		{
			ConcurrentDictionary<int, List<int>> receivedCalls = [];
			Task[] tasks = new Task[20];
			for (int i = 0; i < 20; i++)
			{
				int taskId = i;
				tasks[i] = Task.Run(async () =>
				{
					IMyServiceWithAbstractStaticMembers sut = IMyServiceWithAbstractStaticMembers.CreateMock();

					SubscribeAbstractStaticEvent<Mock.MockTests_StaticInterfaceMembersTests_IMyServiceWithAbstractStaticMembers>(v => Callback(taskId, v));

					sut.Mock.RaiseStatic.AbstractStaticEvent(taskId + 100);
					sut.Mock.RaiseStatic.AbstractStaticEvent(taskId + 300);

					await That(sut.Mock.VerifyStatic.AbstractStaticEvent.Subscribed()).Once();
					await That(sut.Mock.VerifyStatic.AbstractStaticEvent.Unsubscribed()).Never();
				});
			}

			await Task.WhenAll(tasks);

			for (int taskId = 0; taskId < 20; taskId++)
			{
				await That(receivedCalls[taskId]).IsEqualTo([100 + taskId, 300 + taskId,]);
			}

			void Callback(int taskId, int value)
			{
				receivedCalls.AddOrUpdate(taskId, _ => [value,], (_, list) =>
				{
					list.Add(value);
					return list;
				});
			}

			static void SubscribeAbstractStaticEvent<T>(Action<int> callback)
				where T : IMyServiceWithAbstractStaticMembers
			{
				T.AbstractStaticEvent += callback;
			}
		}

		[Fact]
		public async Task WithAbstractStaticMethod()
		{
			IMyServiceWithAbstractStaticMembers sut = IMyServiceWithAbstractStaticMembers.CreateMock();

			sut.Mock.SetupStatic.AbstractStaticMethod().Returns(4);

			int? result = CallAbstractStaticMethod<Mock.MockTests_StaticInterfaceMembersTests_IMyServiceWithAbstractStaticMembers>();

			await That(result).IsEqualTo(4);
			await That(sut.Mock.VerifyStatic.AbstractStaticMethod()).Once();

			static int? CallAbstractStaticMethod<T>()
				where T : IMyServiceWithAbstractStaticMembers
			{
				return T.AbstractStaticMethod();
			}
		}

		[Fact]
		public async Task WithAbstractStaticMethod_ShouldWorkInParallel()
		{
			Task[] tasks = new Task[20];
			for (int i = 0; i < 20; i++)
			{
				int j = i;
				tasks[i] = Task.Run(async () =>
				{
					IMyServiceWithAbstractStaticMembers sut1 = IMyServiceWithAbstractStaticMembers.CreateMock();

					sut1.Mock.SetupStatic.AbstractStaticMethod().Returns(j);

					int? result = CallAbstractStaticMethod<Mock.MockTests_StaticInterfaceMembersTests_IMyServiceWithAbstractStaticMembers>();

					await That(result).IsEqualTo(j);
					await That(sut1.Mock.VerifyStatic.AbstractStaticMethod()).Once();
				});
			}

			await Task.WhenAll(tasks);

			static int? CallAbstractStaticMethod<T>()
				where T : IMyServiceWithAbstractStaticMembers
			{
				return T.AbstractStaticMethod();
			}
		}

		[Fact]
		public async Task WithAbstractStaticProperty()
		{
			IMyServiceWithAbstractStaticMembers sut = IMyServiceWithAbstractStaticMembers.CreateMock();

			sut.Mock.SetupStatic.AbstractStaticProperty.Returns(4);

			int? result = CallAbstractStaticProperty<Mock.MockTests_StaticInterfaceMembersTests_IMyServiceWithAbstractStaticMembers>();

			await That(result).IsEqualTo(4);
			await That(sut.Mock.VerifyStatic.AbstractStaticProperty.Got()).Once();

			static int? CallAbstractStaticProperty<T>()
				where T : IMyServiceWithAbstractStaticMembers
			{
				return T.AbstractStaticProperty;
			}
		}

		[Fact]
		public async Task WithAbstractStaticProperty_ShouldWorkInParallel()
		{
			Task[] tasks = new Task[20];
			for (int i = 0; i < 20; i++)
			{
				int j = i;
				tasks[i] = Task.Run(async () =>
				{
					IMyServiceWithAbstractStaticMembers sut1 = IMyServiceWithAbstractStaticMembers.CreateMock();

					sut1.Mock.SetupStatic.AbstractStaticProperty.Returns(j);

					int? result = CallAbstractStaticProperty<Mock.MockTests_StaticInterfaceMembersTests_IMyServiceWithAbstractStaticMembers>();

					await That(result).IsEqualTo(j);
					await That(sut1.Mock.VerifyStatic.AbstractStaticProperty.Got()).Once();
				});
			}

			await Task.WhenAll(tasks);

			static int? CallAbstractStaticProperty<T>()
				where T : IMyServiceWithAbstractStaticMembers
			{
				return T.AbstractStaticProperty;
			}
		}

		[Fact]
		public async Task WithVirtualStaticMethod()
		{
			IMyServiceWithAbstractStaticMembers sut = IMyServiceWithAbstractStaticMembers.CreateMock();

			sut.Mock.SetupStatic.VirtualStaticMethod().Returns(4);

			int? result = CallVirtualStaticMethod<Mock.MockTests_StaticInterfaceMembersTests_IMyServiceWithAbstractStaticMembers>();

			await That(result).IsEqualTo(4);
			await That(sut.Mock.VerifyStatic.VirtualStaticMethod()).Once();

			static int? CallVirtualStaticMethod<T>()
				where T : IMyServiceWithAbstractStaticMembers
			{
				return T.VirtualStaticMethod();
			}
		}

		[Fact]
		public async Task WithVirtualStaticMethod_ShouldWorkInParallel()
		{
			Task[] tasks = new Task[20];
			for (int i = 0; i < 20; i++)
			{
				int j = i;
				tasks[i] = Task.Run(async () =>
				{
					IMyServiceWithAbstractStaticMembers sut1 = IMyServiceWithAbstractStaticMembers.CreateMock();

					sut1.Mock.SetupStatic.VirtualStaticMethod().Returns(j);

					int? result = CallVirtualStaticMethod<Mock.MockTests_StaticInterfaceMembersTests_IMyServiceWithAbstractStaticMembers>();

					await That(result).IsEqualTo(j);
					await That(sut1.Mock.VerifyStatic.VirtualStaticMethod()).Once();
				});
			}

			await Task.WhenAll(tasks);

			static int? CallVirtualStaticMethod<T>()
				where T : IMyServiceWithAbstractStaticMembers
			{
				return T.VirtualStaticMethod();
			}
		}

		[Fact]
		public async Task WithVirtualStaticProperty()
		{
			IMyServiceWithAbstractStaticMembers sut = IMyServiceWithAbstractStaticMembers.CreateMock();

			sut.Mock.SetupStatic.VirtualStaticProperty.Returns(4);

			int? result = CallVirtualStaticProperty<Mock.MockTests_StaticInterfaceMembersTests_IMyServiceWithAbstractStaticMembers>();

			await That(result).IsEqualTo(4);
			await That(sut.Mock.VerifyStatic.VirtualStaticProperty.Got()).Once();

			static int? CallVirtualStaticProperty<T>()
				where T : IMyServiceWithAbstractStaticMembers
			{
				return T.VirtualStaticProperty;
			}
		}

		[Fact]
		public async Task WithVirtualStaticProperty_ShouldWorkInParallel()
		{
			Task[] tasks = new Task[20];
			for (int i = 0; i < 20; i++)
			{
				int j = i;
				tasks[i] = Task.Run(async () =>
				{
					IMyServiceWithAbstractStaticMembers sut1 = IMyServiceWithAbstractStaticMembers.CreateMock();

					sut1.Mock.SetupStatic.VirtualStaticProperty.Returns(j);

					int? result = CallVirtualStaticProperty<Mock.MockTests_StaticInterfaceMembersTests_IMyServiceWithAbstractStaticMembers>();

					await That(result).IsEqualTo(j);
					await That(sut1.Mock.VerifyStatic.VirtualStaticProperty.Got()).Once();
				});
			}

			await Task.WhenAll(tasks);

			static int? CallVirtualStaticProperty<T>()
				where T : IMyServiceWithAbstractStaticMembers
			{
				return T.VirtualStaticProperty;
			}
		}

		public interface IMyServiceWithAbstractStaticMembers
		{
			bool SomeNormalProperty { get; set; }
			static virtual int VirtualStaticProperty { get; } = 0;
			static abstract int AbstractStaticProperty { get; set; }
			static abstract int AbstractStaticMethod();
			static virtual int VirtualStaticMethod() => 0;
			static abstract event Action<int> AbstractStaticEvent;
		}
	}
}
#endif
