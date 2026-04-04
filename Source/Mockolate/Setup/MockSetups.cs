using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Mockolate.Setup;

[DebuggerDisplay("{ToString()}")]
[DebuggerNonUserCode]
internal partial class MockSetups
{
	/// <inheritdoc cref="object.ToString()" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		StringBuilder sb = new();
		int methodCount = Methods.Count;
		if (methodCount > 0)
		{
			sb.Append(methodCount).Append(methodCount == 1 ? " method, " : " methods, ");
		}

		int propertyCount = Properties.Count;
		if (propertyCount > 0)
		{
			sb.Append(propertyCount).Append(propertyCount == 1 ? " property, " : " properties, ");
		}

		int indexerCount = Indexers.Count;
		if (indexerCount > 0)
		{
			sb.Append(indexerCount).Append(indexerCount == 1 ? " indexer, " : " indexers, ");
		}
		
		if (sb.Length == 0)
		{
			return "no setups";
		}

		sb.Length -= 2;
		return sb.ToString();
	}
}
