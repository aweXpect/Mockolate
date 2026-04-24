Looking at Step 1.3, the two options the plan offers (reusable instance buffer vs. ReadOnlySpan<> + stackalloc) both have trade-offs:

- Reusable buffer: thread-unsafe (two threads calling Matches on the same setup would race on shared state).
- ReadOnlySpan<(string, object?)>: changes INamedParametersMatch.Matches signature — breaking public API contradicts the step's "Public API impact: none" requirement.

The plan explicitly marks Step 1.3 as deferrable: "Can slip to v3.1 if schedule pressure hits" and lists it under Out of scope risks. Given the thread-safety concern with the simple
reusable buffer and the API-break with the Span approach, and that this only affects the rare IParameters-matcher path (the hot IParameterMatch<T> collection path doesn't hit this code),
I'll defer Step 1.3 to v3.1 rather than ship a half-baked optimization.
