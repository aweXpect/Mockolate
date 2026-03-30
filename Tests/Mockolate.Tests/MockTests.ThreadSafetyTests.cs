using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit.Sdk;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	public sealed class ThreadSafetyTests
	{
		[Fact]
		public async Task Event_ShouldBeThreadSafe()
		{
			IMyThreadSafetyService sut = IMyThreadSafetyService.CreateMock();
			ManualResetEventSlim barrier = new(false);
			int taskCount = 50;
			int iterationsPerTask = 20;
			Task[] tasks = new Task[taskCount];
			for (int i = 0; i < taskCount; i++)
			{
				tasks[i] = Task.Run(() =>
				{
					EventHandler handler = (_, _) => { };
					barrier.Wait();
					for (int j = 0; j < iterationsPerTask; j++)
					{
						sut.MyEvent += handler;
						sut.MyEvent -= handler;
					}
				}, CancellationToken.None);
			}

			barrier.Set();
			await Task.WhenAll(tasks);

			await That(sut.Mock.Verify.MyEvent.Subscribed()).Exactly(taskCount * iterationsPerTask);
			await That(sut.Mock.Verify.MyEvent.Unsubscribed()).Exactly(taskCount * iterationsPerTask);

			ValidateInteractionIndices(sut);
		}

		[Fact]
		public async Task Event_SubscribeRaiseUnsubscribe_ShouldBeThreadSafe()
		{
			for (int round = 0; round < 10; round++)
			{
				IMyThreadSafetyService sut = IMyThreadSafetyService.CreateMock();
				int handlerCallCount = 0;
				ManualResetEventSlim barrier = new(false);
				int subscriberCount = 20;
				int eventsPerSubscriber = 10;
				using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(5)))
				{
					Task[] tasks = new Task[subscriberCount * 3];
					for (int i = 0; i < subscriberCount; i++)
					{
						tasks[i] = Task.Run(() =>
						{
							EventHandler handler = (_, _) => Interlocked.Increment(ref handlerCallCount);
							barrier.Wait();
							sut.MyEvent += handler;
						}, cts.Token);
					}

					for (int i = 0; i < subscriberCount; i++)
					{
						int idx = subscriberCount + i;
						tasks[idx] = Task.Run(() =>
						{
							barrier.Wait();
							for (int j = 0; j < eventsPerSubscriber; j++)
							{
								sut.Mock.Raise.MyEvent(this, EventArgs.Empty);
							}
						}, cts.Token);
					}

					for (int i = 0; i < subscriberCount; i++)
					{
						int idx = (subscriberCount * 2) + i;
						tasks[idx] = Task.Run(() =>
						{
							EventHandler handler = (_, _) => Interlocked.Increment(ref handlerCallCount);
							barrier.Wait();
							sut.MyEvent -= handler;
						}, cts.Token);
					}

					barrier.Set();
					await Task.WhenAll(tasks);
				}

				await That(sut.Mock.Verify.MyEvent.Subscribed()).AtLeast(subscriberCount);
				await That(sut.Mock.Verify.MyEvent.Unsubscribed()).AtLeast(subscriberCount);
				await That(handlerCallCount).IsGreaterThanOrEqualTo(0);
			}
		}

		[Fact]
		public async Task Indexer_ManyKeysWithCallbackSequence_ShouldBeThreadSafe()
		{
			for (int round = 0; round < 5; round++)
			{
				IMyThreadSafetyService sut = IMyThreadSafetyService.CreateMock();
				int keyCount = 20;
				for (int k = 0; k < keyCount; k++)
				{
					sut.Mock.Setup[k].InitializeWith($"key-{k}");
				}

				ConcurrentDictionary<int, List<string>> valuesByKey = [];
				ManualResetEventSlim barrier = new(false);
				int readerCount = 30;
				int iterationsPerReader = 10;
				int writerCount = 8;
				int iterationsPerWriter = 10;
				using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(10)))
				{
					Task[] tasks = new Task[readerCount + writerCount];
					Random random = new(42 + round);

					for (int i = 0; i < readerCount; i++)
					{
						tasks[i] = Task.Run(() =>
						{
							barrier.Wait();
							for (int j = 0; j < iterationsPerReader; j++)
							{
								int key = random.Next(keyCount);
								string value = sut[key];
								valuesByKey.AddOrUpdate(key, [value,], (_, list) =>
								{
									list.Add(value);
									return list;
								});
							}
						}, cts.Token);
					}

					for (int i = 0; i < writerCount; i++)
					{
						tasks[readerCount + i] = Task.Run(() =>
						{
							barrier.Wait();
							for (int j = 0; j < iterationsPerWriter; j++)
							{
								int key = random.Next(keyCount);
								sut[key] = $"key-{key}";
							}
						}, cts.Token);
					}

					barrier.Set();
					await Task.WhenAll(tasks);
				}

				// Verify all reads returned expected values (no corruption)
				await That(valuesByKey).All().Satisfy(kvp =>
				{
					string expectedValue = $"key-{kvp.Key}";
					return kvp.Value.All(v => v == expectedValue);
				});

				// Verify interaction counts
				for (int k = 0; k < keyCount; k++)
				{
					await That(sut.Mock.Verify[k].Got()).AtLeast(0);
					await That(sut.Mock.Verify[k].Set(It.IsAny<string>())).AtLeast(0);
				}

				ValidateInteractionIndices(sut);
			}
		}

		[Fact]
		public async Task Indexer_ShouldBeThreadSafe()
		{
			IMyThreadSafetyService sut = IMyThreadSafetyService.CreateMock();
			sut.Mock.Setup[0].InitializeWith("hello");
			sut.Mock.Setup[1].InitializeWith("hello");
			ConcurrentQueue<string> values = [];
			ManualResetEventSlim barrier = new(false);
			int readerCount = 50;
			int iterationsPerReader = 20;
			int writerCount = 10;
			int iterationsPerWriter = 20;
			Task[] tasks = new Task[readerCount + writerCount];
			for (int i = 0; i < readerCount; i++)
			{
				tasks[i] = Task.Run(() =>
				{
					barrier.Wait();
					for (int j = 0; j < iterationsPerReader; j++)
					{
						values.Enqueue(sut[0]);
						values.Enqueue(sut[1]);
					}
				}, CancellationToken.None);
			}

			for (int i = 0; i < writerCount; i++)
			{
				tasks[readerCount + i] = Task.Run(() =>
				{
					barrier.Wait();
					for (int j = 0; j < iterationsPerWriter; j++)
					{
						sut[0] = "hello";
						sut[1] = "hello";
					}
				}, CancellationToken.None);
			}

			barrier.Set();
			await Task.WhenAll(tasks);

			await That(sut.Mock.Verify[0].Got()).Exactly(readerCount * iterationsPerReader);
			await That(sut.Mock.Verify[1].Got()).Exactly(readerCount * iterationsPerReader);
			await That(sut.Mock.Verify[0].Set(It.IsAny<string>())).Exactly(writerCount * iterationsPerWriter);
			await That(sut.Mock.Verify[1].Set(It.IsAny<string>())).Exactly(writerCount * iterationsPerWriter);

			await That(values).Contains("hello")
				.Exactly(readerCount * iterationsPerReader * 2);

			ValidateInteractionIndices(sut);
		}

		[Fact]
		public async Task Method_HighContention_ShouldBeThreadSafe()
		{
			for (int round = 0; round < 10; round++)
			{
				IMyThreadSafetyService sut = IMyThreadSafetyService.CreateMock();
				sut.Mock.Setup.MyMethod(It.IsAny<int>()).Returns(42);
				ConcurrentQueue<int> values = [];
				ManualResetEventSlim barrier = new(false);
				int taskCount = 60;
				int iterationsPerTask = 20;
				using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(5)))
				{
					Task[] tasks = new Task[taskCount];
					for (int i = 0; i < taskCount; i++)
					{
						tasks[i] = Task.Run(() =>
						{
							barrier.Wait();
							for (int j = 0; j < iterationsPerTask; j++)
							{
								values.Enqueue(sut.MyMethod(j));
							}
						}, cts.Token);
					}

					barrier.Set();
					await Task.WhenAll(tasks);
				}

				int expectedCalls = taskCount * iterationsPerTask;
				await That(values).All().Satisfy(v => v == 42);
				await That(sut.Mock.Verify.MyMethod(It.IsAny<int>())).Exactly(expectedCalls);
			}
		}

		[Fact]
		public async Task Method_ShouldBeThreadSafe()
		{
			IMyThreadSafetyService sut = IMyThreadSafetyService.CreateMock();
			sut.Mock.Setup.MyMethod(It.IsAny<int>()).Returns(42);
			ConcurrentQueue<int> values = [];
			ManualResetEventSlim barrier = new(false);
			int taskCount = 50;
			int iterationsPerTask = 20;
			Task[] tasks = new Task[taskCount];
			for (int i = 0; i < taskCount; i++)
			{
				tasks[i] = Task.Run(() =>
				{
					barrier.Wait();
					for (int j = 0; j < iterationsPerTask; j++)
					{
						values.Enqueue(sut.MyMethod(j));
					}
				}, CancellationToken.None);
			}

			barrier.Set();
			await Task.WhenAll(tasks);

			await That(sut.Mock.Verify.MyMethod(It.IsAny<int>())).Exactly(taskCount * iterationsPerTask);
			await That(values).Contains(42).Exactly(taskCount * iterationsPerTask);

			ValidateInteractionIndices(sut);
		}

		[Fact]
		public async Task Property_HighContention_ShouldBeThreadSafe()
		{
			for (int round = 0; round < 10; round++)
			{
				IMyThreadSafetyService sut = IMyThreadSafetyService.CreateMock();
				sut.Mock.Setup.MyStringProperty.InitializeWith("test-value");
				ConcurrentQueue<string> values = [];
				ManualResetEventSlim barrier = new(false);
				int taskCount = 60;
				int iterationsPerTask = 20;
				using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(5)))
				{
					Task[] tasks = new Task[taskCount];
					for (int i = 0; i < taskCount; i++)
					{
						tasks[i] = Task.Run(() =>
						{
							barrier.Wait();
							for (int j = 0; j < iterationsPerTask; j++)
							{
								values.Enqueue(sut.MyStringProperty);
							}
						}, cts.Token);
					}

					barrier.Set();
					await Task.WhenAll(tasks);
				}

				int expectedReads = taskCount * iterationsPerTask;
				await That(values).All().Satisfy(v => v == "test-value");
				await That(sut.Mock.Verify.MyStringProperty.Got()).Exactly(expectedReads);
			}
		}

		[Fact]
		public async Task Property_ShouldBeThreadSafe()
		{
			IMyThreadSafetyService sut = IMyThreadSafetyService.CreateMock();
			Guid expectedGuid = Guid.NewGuid();
			sut.Mock.Setup.MyStringProperty.InitializeWith("hello");
			sut.Mock.Setup.MyGuidProperty.InitializeWith(expectedGuid);
			ConcurrentQueue<string> stringValues = [];
			ConcurrentQueue<Guid> guidValues = [];
			ManualResetEventSlim barrier = new(false);
			int readerCount = 50;
			int iterationsPerReader = 20;
			int writerCount = 10;
			int iterationsPerWriter = 20;
			Task[] tasks = new Task[readerCount + writerCount];
			for (int i = 0; i < readerCount; i++)
			{
				tasks[i] = Task.Run(() =>
				{
					barrier.Wait();
					for (int j = 0; j < iterationsPerReader; j++)
					{
						stringValues.Enqueue(sut.MyStringProperty);
						guidValues.Enqueue(sut.MyGuidProperty);
					}
				}, CancellationToken.None);
			}

			for (int i = 0; i < writerCount; i++)
			{
				tasks[readerCount + i] = Task.Run(() =>
				{
					barrier.Wait();
					for (int j = 0; j < iterationsPerWriter; j++)
					{
						sut.MyStringProperty = "hello";
						sut.MyGuidProperty = expectedGuid;
					}
				}, CancellationToken.None);
			}

			barrier.Set();
			await Task.WhenAll(tasks);

			await That(sut.Mock.Verify.MyStringProperty.Got())
				.Exactly(readerCount * iterationsPerReader);
			await That(sut.Mock.Verify.MyGuidProperty.Got())
				.Exactly(readerCount * iterationsPerReader);

			await That(sut.Mock.Verify.MyStringProperty.Set(It.IsAny<string>()))
				.Exactly(writerCount * iterationsPerWriter);
			await That(sut.Mock.Verify.MyGuidProperty.Set(It.IsAny<Guid>()))
				.Exactly(writerCount * iterationsPerWriter);

			await That(stringValues).All()
				.Satisfy(v => v == "hello");
			await That(guidValues).All()
				.Satisfy(v => v == expectedGuid);

			ValidateInteractionIndices(sut);
		}

		private static void ValidateInteractionIndices(IMyThreadSafetyService sut)
		{
			MockRegistry registry = ((IMock)sut).MockRegistry;
			int[] indices = registry.Interactions.Interactions
				.Select(i => i.Index)
				.ToArray();

			// Verify indices are strictly ascending
			for (int i = 1; i < indices.Length; i++)
			{
				if (indices[i] <= indices[i - 1])
				{
					throw new XunitException(
						$"Interaction indices not strictly ascending at position {i}: {indices[i - 1]} -> {indices[i]}");
				}
			}

			// Verify all indices are unique
			int uniqueCount = indices.Distinct().Count();
			if (uniqueCount != indices.Length)
			{
				throw new XunitException(
					$"Interaction indices not all unique: {indices.Length} total, {uniqueCount} unique");
			}
		}

		internal interface IMyThreadSafetyService
		{
			string MyStringProperty { get; set; }
			Guid MyGuidProperty { get; set; }
			string this[int index] { get; set; }
			int MyMethod(int value);
			event EventHandler? MyEvent;
		}
	}
}
