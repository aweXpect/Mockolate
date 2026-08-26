using System.Collections.Generic;

namespace Mockolate.ExampleTests.TestData;

public interface IUserCacheBase
{
	IEnumerable<User> Users { get; set; }
}

public interface IUserCache : IUserCacheBase
{
	new IList<User> Users { get; set; }
}
