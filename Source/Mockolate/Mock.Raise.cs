using Mockolate.Raise;
// ReSharper disable RedundantExtendsListEntry

namespace Mockolate;

public partial class Mock<T> : IMockRaises<T>, IProtectedMockRaises<T>
{
}
