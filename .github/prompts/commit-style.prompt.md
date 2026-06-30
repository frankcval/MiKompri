---
description: Suggest commit messages that match this repository's Conventional Commits style based on the current git changes.
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding.

## Goal

Generate high-signal commit message suggestions for the current repository changes.

This repository follows **Conventional Commits** and the examples in the project README use
Spanish descriptions after the commit type, for example:

- `feat: agregar nueva funcionalidad`
- `fix: corregir validación de lista`
- `docs: actualizar instrucciones de instalación`

## Instructions

1. Inspect the current git state before proposing any message:
   - Read `git status --short`
   - Inspect the relevant staged and unstaged diffs
   - If the changes are too broad or unrelated, recommend splitting them into multiple commits

2. Use this style by default:
   - Format: `<type>: <descripción breve>`
   - Types should prefer: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`
   - Keep the subject concise, specific, and in lowercase
   - Write the description in Spanish unless the user explicitly asks for another language
   - Avoid vague subjects like `update files`, `misc changes`, or `fix stuff`

3. Match the commit type to the dominant intent:
   - `feat` for visible functionality
   - `fix` for bug fixes or behavior corrections
   - `docs` for documentation-only changes
   - `refactor` for structural changes without behavior change
   - `test` for test-only changes
   - `chore` for maintenance, tooling, or repository setup

4. When useful, provide:
   - one **recommended** commit message
   - up to two **alternatives**
   - a short explanation of why that type fits

5. If the work should be split into multiple commits:
   - say that clearly
   - propose one commit message per logical chunk

6. If there are no meaningful tracked changes, say so instead of inventing a commit message.

## Output Format

Use this structure:

### Recomendado

```text
<type>: <descripción>
```

### Alternativas

```text
<type>: <descripción>
```

### Criterio

One short paragraph explaining the recommendation, or explain why the changes should be split.
