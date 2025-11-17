---
applyTo: '**/*.cs'
---

# C# Coding Guidelines (Key Points)

- Write C# code according to the project's LangVersion, while **prioritizing the latest C# 13 syntax and APIs whenever possible**.
  - However, **do not use preview features that fail to build**. If necessary, safely fall back to C# 12 equivalents.
- Use **file-scoped namespaces**, and optimize `using` directives (avoid duplicates and order them properly).
- Assume **nullable is enabled** (`#nullable enable` / project settings).
- For async operations, use `async`/`await`; **use `ConfigureAwait(false)` only within libraries**.
- Make use of **`required` members / `init` accessors** (only when supported by the environment).
- Prefer **`switch` expressions / pattern matching** for concise conditional logic.
- Use **`Span<T>` / `Memory<T>`** and `ReadOnlySpan<char>` only in hot paths, with readability prioritized first.
- Handle exceptions with **guard clauses** for early returns, and keep `try/catch` blocks as granular as possible.
