using Mockolate.Setup;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public abstract class IndexerAccess : IInteraction
{
	/// <summary>
	///     The number of indexer parameters.
	/// </summary>
	public abstract int ParameterCount { get; }

	/// <summary>
	///     Returns the indexer parameter value at the given <paramref name="index" />.
	/// </summary>
	public abstract object? GetParameterValueAt(int index);

	internal ValueStorage? Storage { get; set; }

	/// <summary>
	///     Attempts to find a previously stored value for this access.
	/// </summary>
	public bool TryFindStoredValue<T>(out T value)
	{
		ValueStorage? current = Storage;
		if (current is null)
		{
			value = default!;
			return false;
		}

		int count = ParameterCount;
		for (int i = 0; i < count; i++)
		{
			current = current.GetChild(GetParameterValueAt(i));
			if (current is null)
			{
				value = default!;
				return false;
			}
		}

		if (current.Value is T typedValue)
		{
			value = typedValue;
			return true;
		}

		value = default!;
		return false;
	}

	/// <summary>
	///     Stores the given <paramref name="value" /> for this access.
	/// </summary>
	public void StoreValue<T>(T value)
	{
		ValueStorage? current = Storage;
		if (current is null)
		{
			return;
		}

		int count = ParameterCount;
		for (int i = 0; i < count; i++)
		{
			current = current.GetOrAddChild(GetParameterValueAt(i)!);
		}

		current.Value = value;
	}
}
