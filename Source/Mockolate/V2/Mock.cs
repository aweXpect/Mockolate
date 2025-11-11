using Mockolate.Interactions;
using Mockolate.Setup;
using Mockolate.Verify;

namespace Mockolate.V2;

public partial class Mock<T> : IInteractiveMock, IMock<T>, IMockSetup<T>, IMockVerify<T>
{
	/// <inheritdoc cref="MockBase{T}" />
	public Mock(MockBehavior behavior, string prefix)
	{
		Behavior = behavior;
		Prefix = prefix;
		Interactions = new MockInteractions();
	}

	/// <summary>
	///     Gets the behavior settings used by this mock instance.
	/// </summary>
	public MockBehavior Behavior { get; }

	/// <summary>
	///     Gets the prefix string used to identify or categorize items within the context.
	/// </summary>
	public string Prefix { get; }

	/// <summary>
	///     Gets the collection of interactions recorded by the mock object.
	/// </summary>
	public MockInteractions Interactions { get; }

	/// <summary>
	///     TODO: VAB
	/// </summary>
	public T Subject => Behavior.DefaultValue.Generate<T>();
}
