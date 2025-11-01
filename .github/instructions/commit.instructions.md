---
# applyTo: "**/*"
---

# Commit Message Policy (Required)

- **Commit messages must be written in English** (short, imperative mood).

## Commit Rules (Conventional Commits)

- Prefix must be one of: `feat|fix|refactor|chore`
- Format: `<type>(<optional scope>): <summary (English, imperative)>`
- Body (optional): describe **why it is needed** / **impact** in bullet points. Keep it concise and in English.
- Breaking changes must be noted with a footer: **`BREAKING CHANGE:`** followed by an explanation.

### Type Definitions

- **feat:** Add a new feature
- **fix:** Bug fix or correction
- **refactor:** Code refactoring without changing external behavior
- **chore:** Other tasks not affecting main code (e.g. `.gitignore`, VSCode settings)

### Examples

- `feat(timer): Add manual break input`
- `fix(api): Fix issue with date crossing in rounding of break time`
- `refactor(worktime): Consolidate duplicate TimeSpan calculations into utility`
- `chore: Add Japanese locale to VSCode settings`

### Instructions for Copilot (when generating commits)

- Always follow the above format and write concisely in English
- **If a body is included**, add 1–3 bullet points for "motivation" and "impact"
- **Tests** or **documentation** should be noted in the body (you may also use `scope` such as `docs|test` if appropriate)
