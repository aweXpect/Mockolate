using System.Diagnostics;
using Mockolate.Events;
using Mockolate.Setup;
using Mockolate.V2;
using Mockolate.Verify;
using IMockSetup = Mockolate.V2.IMockSetup;

namespace Mockolate.ExampleTests.Dummy;


/// <summary>
///     A mock implementing <see cref="Mockolate.Tests.Dummy.IExampleRepository" />.
/// </summary>
public class MockForIExampleRepository : Mockolate.Tests.Dummy.IExampleRepository, IMockSubject<Mockolate.Tests.Dummy.IExampleRepository>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly V2.Mock<Mockolate.Tests.Dummy.IExampleRepository> _mock;

	/// <inheritdoc cref="MockForIExampleRepository" />
	public MockForIExampleRepository(MockBehavior mockBehavior)
	{
		_mock = new V2.Mock<Mockolate.Tests.Dummy.IExampleRepository>(this, new MockRegistration(mockBehavior, "Mockolate.Tests.Dummy.IExampleRepository"));
	}
	
	V2.Mock<Mockolate.Tests.Dummy.IExampleRepository> IMockSubject<Mockolate.Tests.Dummy.IExampleRepository>.Mock => _mock;

	#region Mockolate.Tests.Dummy.IExampleRepository
	/// <inheritdoc cref="Mockolate.Tests.Dummy.IExampleRepository.UsersChanged" />
	public event System.EventHandler? UsersChanged
	{
		add => _mock.Registrations.AddEvent("Mockolate.Tests.Dummy.IExampleRepository.UsersChanged", value?.Target, value?.Method);
		remove => _mock.Registrations.RemoveEvent("Mockolate.Tests.Dummy.IExampleRepository.UsersChanged", value?.Target, value?.Method);
	}

	/// <inheritdoc cref="Mockolate.Tests.Dummy.IExampleRepository.MyEvent" />
	public event Mockolate.Tests.Dummy.MyDelegate? MyEvent
	{
		add => _mock.Registrations.AddEvent("Mockolate.Tests.Dummy.IExampleRepository.MyEvent", value?.Target, value?.Method);
		remove => _mock.Registrations.RemoveEvent("Mockolate.Tests.Dummy.IExampleRepository.MyEvent", value?.Target, value?.Method);
	}

	/// <inheritdoc cref="Mockolate.Tests.Dummy.IExampleRepository.AddUser(string)" />
	public Mockolate.Tests.Dummy.User AddUser(string name)
	{
		MethodSetupResult<Mockolate.Tests.Dummy.User>? methodExecution = _mock?.Registrations.Execute<Mockolate.Tests.Dummy.User>("Mockolate.Tests.Dummy.IExampleRepository.AddUser", name);
		if (methodExecution is null)
		{
			return (_mock?.Registrations.Behavior ?? MockBehavior.Default).DefaultValue.Generate<Mockolate.Tests.Dummy.User>();
		}

		return methodExecution.Result;
	}

	/// <inheritdoc cref="Mockolate.Tests.Dummy.IExampleRepository.RemoveUser(System.Guid)" />
	public bool RemoveUser(System.Guid id)
	{
		MethodSetupResult<bool>? methodExecution = _mock?.Registrations.Execute<bool>("Mockolate.Tests.Dummy.IExampleRepository.RemoveUser", id);
		if (methodExecution is null)
		{
			return (_mock?.Registrations.Behavior ?? MockBehavior.Default).DefaultValue.Generate<bool>();
		}

		return methodExecution.Result;
	}

	/// <inheritdoc cref="Mockolate.Tests.Dummy.IExampleRepository.UpdateUser(System.Guid, string)" />
	public void UpdateUser(System.Guid id, string newName)
	{
		MethodSetupResult? methodExecution = _mock?.Registrations.Execute("Mockolate.Tests.Dummy.IExampleRepository.UpdateUser", id, newName);
	}

	/// <inheritdoc cref="Mockolate.Tests.Dummy.IExampleRepository.TryDelete(System.Guid, out Mockolate.Tests.Dummy.User?)" />
	public bool TryDelete(System.Guid id, out Mockolate.Tests.Dummy.User? user)
	{
		MethodSetupResult<bool>? methodExecution = _mock?.Registrations.Execute<bool>("Mockolate.Tests.Dummy.IExampleRepository.TryDelete", id, null);
		if (methodExecution is null)
		{
			user = (_mock?.Registrations.Behavior ?? MockBehavior.Default).DefaultValue.Generate<Mockolate.Tests.Dummy.User?>();
		}
		else
		{
			user = methodExecution.SetOutParameter<Mockolate.Tests.Dummy.User?>("user");
		}

		if (methodExecution is null)
		{
			return (_mock?.Registrations.Behavior ?? MockBehavior.Default).DefaultValue.Generate<bool>();
		}

		return methodExecution.Result;
	}

	/// <inheritdoc cref="Mockolate.Tests.Dummy.IExampleRepository.SaveChanges()" />
	public bool SaveChanges()
	{
		MethodSetupResult<bool>? methodExecution = _mock?.Registrations.Execute<bool>("Mockolate.Tests.Dummy.IExampleRepository.SaveChanges");
		if (methodExecution is null)
		{
			return (_mock?.Registrations.Behavior ?? MockBehavior.Default).DefaultValue.Generate<bool>();
		}

		return methodExecution.Result;
	}
	#endregion Mockolate.Tests.Dummy.IExampleRepository
}

internal static class ExtensionsForIExampleRepository
{
	extension(Mockolate.Tests.Dummy.IExampleRepository subject)
	{
		/// <summary>
		///     TODO
		/// </summary>
		public V2.IMockSetup<Mockolate.Tests.Dummy.IExampleRepository> SetupMock
			=> (V2.IMockSetup<Mockolate.Tests.Dummy.IExampleRepository>)subject.GetMockOrThrow();
		
		/// <summary>
		///     TODO
		/// </summary>
		public V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository> VerifyMock
			=> (V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>)subject.GetMockOrThrow();
	}

	private static V2.Mock<T> GetMockOrThrow<T>(this T subject)
	{
		if (subject is IMockSubject<T> mock)
		{
			return mock.Mock;
		}

		throw new NotSupportedException("The subject is no mock!");
	}

	extension(V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository> verify)
	{
		/// <summary>
		///     Verifies the method invocations for <see cref="Mockolate.Tests.Dummy.IExampleRepository"/> on the mock.
		/// </summary>
		public IMockVerifyInvoked<Mockolate.Tests.Dummy.IExampleRepository> Invoked
			=> (IMockVerifyInvoked<Mockolate.Tests.Dummy.IExampleRepository>)verify;
	}

	extension(IMockVerifyInvoked<Mockolate.Tests.Dummy.IExampleRepository> mock)
	{
		/// <summary>
		///     Validates the invocations for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.AddUser(string)"/> with the given <paramref name="name"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> AddUser(Match.IParameter<string>? name)
			=> ((IMockInvoked<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Method("Mockolate.Tests.Dummy.IExampleRepository.AddUser", name ?? Match.Null<string>());

		/// <summary>
		///     Validates the invocations for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.RemoveUser(System.Guid)"/> with the given <paramref name="id"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> RemoveUser(Match.IParameter<System.Guid>? id)
			=> ((IMockInvoked<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Method("Mockolate.Tests.Dummy.IExampleRepository.RemoveUser", id ?? Match.Null<System.Guid>());

		/// <summary>
		///     Validates the invocations for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.UpdateUser(System.Guid, string)"/> with the given <paramref name="id"/>, <paramref name="newName"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> UpdateUser(Match.IParameter<System.Guid>? id, Match.IParameter<string>? newName)
			=> ((IMockInvoked<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Method("Mockolate.Tests.Dummy.IExampleRepository.UpdateUser", id ?? Match.Null<System.Guid>(), newName ?? Match.Null<string>());

		/// <summary>
		///     Validates the invocations for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.TryDelete(System.Guid, out Mockolate.Tests.Dummy.User?)"/> with the given <paramref name="id"/>, <paramref name="user"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> TryDelete(Match.IParameter<System.Guid>? id, Match.IVerifyOutParameter<Mockolate.Tests.Dummy.User?> user)
			=> ((IMockInvoked<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Method("Mockolate.Tests.Dummy.IExampleRepository.TryDelete", id ?? Match.Null<System.Guid>(), user);

		/// <summary>
		///     Validates the invocations for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.SaveChanges()"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> SaveChanges()
			=> ((IMockInvoked<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Method("Mockolate.Tests.Dummy.IExampleRepository.SaveChanges");

		/// <summary>
		///     Validates the invocations for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.AddUser(string)"/> with the given <paramref name="name"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> AddUser(Match.IParameters parameters)
			=> ((IMockInvoked<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Method("Mockolate.Tests.Dummy.IExampleRepository.AddUser", parameters);

		/// <summary>
		///     Validates the invocations for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.RemoveUser(System.Guid)"/> with the given <paramref name="id"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> RemoveUser(Match.IParameters parameters)
			=> ((IMockInvoked<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Method("Mockolate.Tests.Dummy.IExampleRepository.RemoveUser", parameters);

		/// <summary>
		///     Validates the invocations for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.UpdateUser(System.Guid, string)"/> with the given <paramref name="id"/>, <paramref name="newName"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> UpdateUser(Match.IParameters parameters)
			=> ((IMockInvoked<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Method("Mockolate.Tests.Dummy.IExampleRepository.UpdateUser", parameters);

		/// <summary>
		///     Validates the invocations for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.TryDelete(System.Guid, out Mockolate.Tests.Dummy.User?)"/> with the given <paramref name="id"/>, <paramref name="user"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> TryDelete(Match.IParameters parameters)
			=> ((IMockInvoked<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Method("Mockolate.Tests.Dummy.IExampleRepository.TryDelete", parameters);
	}

	extension(V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository> verify)
	{
		/// <summary>
		///     Verifies the event subscriptions for <see cref="Mockolate.Tests.Dummy.IExampleRepository"/> on the mock.
		/// </summary>
		public IMockVerifySubscribedTo<Mockolate.Tests.Dummy.IExampleRepository> SubscribedTo
			=> (IMockVerifySubscribedTo<Mockolate.Tests.Dummy.IExampleRepository>)verify;

		/// <summary>
		///     Verifies the event unsubscriptions for <see cref="Mockolate.Tests.Dummy.IExampleRepository"/> on the mock.
		/// </summary>
		public IMockVerifyUnsubscribedFrom<Mockolate.Tests.Dummy.IExampleRepository> UnsubscribedFrom
			=> (IMockVerifyUnsubscribedFrom<Mockolate.Tests.Dummy.IExampleRepository>)verify;
	}

	extension(IMockVerifySubscribedTo<Mockolate.Tests.Dummy.IExampleRepository> mock)
	{
		/// <summary>
		///     Validates the subscriptions for the event <see cref="Mockolate.Tests.Dummy.IExampleRepository.UsersChanged"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> UsersChanged()
			=> ((IMockSubscribedTo<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Event("Mockolate.Tests.Dummy.IExampleRepository.UsersChanged");

		/// <summary>
		///     Validates the subscriptions for the event <see cref="Mockolate.Tests.Dummy.IExampleRepository.MyEvent"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> MyEvent()
			=> ((IMockSubscribedTo<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Event("Mockolate.Tests.Dummy.IExampleRepository.MyEvent");
	}

	extension(IMockVerifyUnsubscribedFrom<Mockolate.Tests.Dummy.IExampleRepository> mock)
	{
		/// <summary>
		///     Validates the unsubscription for the event <see cref="Mockolate.Tests.Dummy.IExampleRepository.UsersChanged"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> UsersChanged()
			=> ((IMockUnsubscribedFrom<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Event("Mockolate.Tests.Dummy.IExampleRepository.UsersChanged");

		/// <summary>
		///     Validates the unsubscription for the event <see cref="Mockolate.Tests.Dummy.IExampleRepository.MyEvent"/>.
		/// </summary>
		public VerificationResult<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>> MyEvent()
			=> ((IMockUnsubscribedFrom<V2.IMockVerify<Mockolate.Tests.Dummy.IExampleRepository>>)mock).Event("Mockolate.Tests.Dummy.IExampleRepository.MyEvent");
	}

}

internal static class SetupExtensionsForIExampleRepository
{
	extension(IMockRaises<Mockolate.Tests.Dummy.IExampleRepository> mock)
	{
		/// <summary>
		///     Raise the <see cref="Mockolate.Tests.Dummy.IExampleRepository.UsersChanged"/> event.
		/// </summary>
		public void UsersChanged(object? sender, System.EventArgs e)
		{
			((V2.Mock<Mockolate.Tests.Dummy.IExampleRepository>)mock).Registrations.Raise("Mockolate.Tests.Dummy.IExampleRepository.UsersChanged", sender, e);
		}

		/// <summary>
		///     Raise the <see cref="Mockolate.Tests.Dummy.IExampleRepository.MyEvent"/> event.
		/// </summary>
		public void MyEvent(int x, int y)
		{
			((V2.Mock<Mockolate.Tests.Dummy.IExampleRepository>)mock).Registrations.Raise("Mockolate.Tests.Dummy.IExampleRepository.MyEvent", x, y);
		}

		/// <summary>
		///     Raise the <see cref="Mockolate.Tests.Dummy.IExampleRepository.UsersChanged"/> event.
		/// </summary>
		public void UsersChanged(Match.IDefaultEventParameters parameters)
		{
			MockBehavior mockBehavior = ((V2.Mock<Mockolate.Tests.Dummy.IExampleRepository>)mock).Registrations.Behavior;
			((V2.Mock<Mockolate.Tests.Dummy.IExampleRepository>)mock).Registrations.Raise("Mockolate.Tests.Dummy.IExampleRepository.UsersChanged", mockBehavior.DefaultValue.Generate<object?>(), mockBehavior.DefaultValue.Generate<System.EventArgs>());
		}

		/// <summary>
		///     Raise the <see cref="Mockolate.Tests.Dummy.IExampleRepository.MyEvent"/> event.
		/// </summary>
		public void MyEvent(Match.IDefaultEventParameters parameters)
		{
			MockBehavior mockBehavior = ((V2.Mock<Mockolate.Tests.Dummy.IExampleRepository>)mock).Registrations.Behavior;
			((V2.Mock<Mockolate.Tests.Dummy.IExampleRepository>)mock).Registrations.Raise("Mockolate.Tests.Dummy.IExampleRepository.MyEvent", mockBehavior.DefaultValue.Generate<int>(), mockBehavior.DefaultValue.Generate<int>());
		}
	}

	extension(V2.IMockSetup<Mockolate.Tests.Dummy.IExampleRepository> setup)
	{
		/// <summary>
		///     Sets up methods on the mock for <see cref="Mockolate.Tests.Dummy.IExampleRepository"/>.
		/// </summary>
		public IMockMethodSetup<Mockolate.Tests.Dummy.IExampleRepository> Method
			=> (IMockMethodSetup<Mockolate.Tests.Dummy.IExampleRepository>)setup;
	}

	extension(IMockMethodSetup<Mockolate.Tests.Dummy.IExampleRepository> setup)
	{
		/// <summary>
		///     Setup for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.AddUser(string)"/> with the given <paramref name="name"/>.
		/// </summary>
		public ReturnMethodSetup<Mockolate.Tests.Dummy.User, string> AddUser(Match.IParameter<string>? name)
		{
			var methodSetup = new ReturnMethodSetup<Mockolate.Tests.Dummy.User, string>("Mockolate.Tests.Dummy.IExampleRepository.AddUser", new Match.NamedParameter("name", name ?? Match.Null<string>()));
			if (setup is IHasMockRegistration hasMockRegistration)
			{
				hasMockRegistration.Registrations.SetupMethod(methodSetup);
			}
			return methodSetup;
		}

		/// <summary>
		///     Setup for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.RemoveUser(System.Guid)"/> with the given <paramref name="id"/>.
		/// </summary>
		public ReturnMethodSetup<bool, System.Guid> RemoveUser(Match.IParameter<System.Guid>? id)
		{
			var methodSetup = new ReturnMethodSetup<bool, System.Guid>("Mockolate.Tests.Dummy.IExampleRepository.RemoveUser", new Match.NamedParameter("id", id ?? Match.Null<System.Guid>()));
			if (setup is IHasMockRegistration hasMockRegistration)
			{
				hasMockRegistration.Registrations.SetupMethod(methodSetup);
			}
			return methodSetup;
		}

		/// <summary>
		///     Setup for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.UpdateUser(System.Guid, string)"/> with the given <paramref name="id"/>, <paramref name="newName"/>.
		/// </summary>
		public VoidMethodSetup<System.Guid, string> UpdateUser(Match.IParameter<System.Guid>? id, Match.IParameter<string>? newName)
		{
			var methodSetup = new VoidMethodSetup<System.Guid, string>("Mockolate.Tests.Dummy.IExampleRepository.UpdateUser", new Match.NamedParameter("id", id ?? Match.Null<System.Guid>()), new Match.NamedParameter("newName", newName ?? Match.Null<string>()));
			if (setup is IHasMockRegistration hasMockRegistration)
			{
				hasMockRegistration.Registrations.SetupMethod(methodSetup);
			}
			return methodSetup;
		}

		/// <summary>
		///     Setup for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.TryDelete(System.Guid, out Mockolate.Tests.Dummy.User?)"/> with the given <paramref name="id"/>, <paramref name="user"/>.
		/// </summary>
		public ReturnMethodSetup<bool, System.Guid, Mockolate.Tests.Dummy.User?> TryDelete(Match.IParameter<System.Guid>? id, Match.IOutParameter<Mockolate.Tests.Dummy.User?> user)
		{
			var methodSetup = new ReturnMethodSetup<bool, System.Guid, Mockolate.Tests.Dummy.User?>("Mockolate.Tests.Dummy.IExampleRepository.TryDelete", new Match.NamedParameter("id", id ?? Match.Null<System.Guid>()), new Match.NamedParameter("user", user));
			if (setup is IHasMockRegistration hasMockRegistration)
			{
				hasMockRegistration.Registrations.SetupMethod(methodSetup);
			}
			return methodSetup;
		}

		/// <summary>
		///     Setup for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.SaveChanges()"/>.
		/// </summary>
		public ReturnMethodSetup<bool> SaveChanges()
		{
			var methodSetup = new ReturnMethodSetup<bool>("Mockolate.Tests.Dummy.IExampleRepository.SaveChanges");
			if (setup is IHasMockRegistration hasMockRegistration)
			{
				hasMockRegistration.Registrations.SetupMethod(methodSetup);
			}
			return methodSetup;
		}

		/// <summary>
		///     Setup for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.AddUser(string)"/> with the given <paramref name="name"/>.
		/// </summary>
		public ReturnMethodSetup<Mockolate.Tests.Dummy.User, string> AddUser(Match.IParameters parameters)
		{
			var methodSetup = new ReturnMethodSetup<Mockolate.Tests.Dummy.User, string>("Mockolate.Tests.Dummy.IExampleRepository.AddUser", parameters);
			if (setup is IHasMockRegistration hasMockRegistration)
			{
				hasMockRegistration.Registrations.SetupMethod(methodSetup);
			}
			return methodSetup;
		}

		/// <summary>
		///     Setup for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.RemoveUser(System.Guid)"/> with the given <paramref name="id"/>.
		/// </summary>
		public ReturnMethodSetup<bool, System.Guid> RemoveUser(Match.IParameters parameters)
		{
			var methodSetup = new ReturnMethodSetup<bool, System.Guid>("Mockolate.Tests.Dummy.IExampleRepository.RemoveUser", parameters);
			if (setup is IHasMockRegistration hasMockRegistration)
			{
				hasMockRegistration.Registrations.SetupMethod(methodSetup);
			}
			return methodSetup;
		}

		/// <summary>
		///     Setup for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.UpdateUser(System.Guid, string)"/> with the given <paramref name="id"/>, <paramref name="newName"/>.
		/// </summary>
		public VoidMethodSetup<System.Guid, string> UpdateUser(Match.IParameters parameters)
		{
			var methodSetup = new VoidMethodSetup<System.Guid, string>("Mockolate.Tests.Dummy.IExampleRepository.UpdateUser", parameters);
			if (setup is IHasMockRegistration hasMockRegistration)
			{
				hasMockRegistration.Registrations.SetupMethod(methodSetup);
			}
			return methodSetup;
		}

		/// <summary>
		///     Setup for the method <see cref="Mockolate.Tests.Dummy.IExampleRepository.TryDelete(System.Guid, out Mockolate.Tests.Dummy.User?)"/> with the given <paramref name="id"/>, <paramref name="user"/>.
		/// </summary>
		public ReturnMethodSetup<bool, System.Guid, Mockolate.Tests.Dummy.User?> TryDelete(Match.IParameters parameters)
		{
			var methodSetup = new ReturnMethodSetup<bool, System.Guid, Mockolate.Tests.Dummy.User?>("Mockolate.Tests.Dummy.IExampleRepository.TryDelete", parameters);
			if (setup is IHasMockRegistration hasMockRegistration)
			{
				hasMockRegistration.Registrations.SetupMethod(methodSetup);
			}
			return methodSetup;
		}
	}

}
