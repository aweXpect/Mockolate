using Mockolate.Raise;

namespace Mockolate;

public partial class Mock<T> : IMockRaises<T>, IProtectedMockRaises<T>;
