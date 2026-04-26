// Single source of truth for the lock object type used across the runtime library.
// On .NET 10 we use the dedicated `System.Threading.Lock`; on earlier targets we fall
// back to a plain `System.Object` so `lock(...)` keeps working via `Monitor.Enter`.

#if NET10_0_OR_GREATER
global using MockolateLock = System.Threading.Lock;
#else
global using MockolateLock = object;
#endif
