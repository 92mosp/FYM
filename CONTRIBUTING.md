# Contributing

1. Branch from `main` using `feature/<short-name>` or `fix/<short-name>`.
2. Keep changes focused and include tests for behaviour changes.
3. Run `dotnet format FYM.sln`, `dotnet test FYM.sln`, `npm run lint`, `npm test`, and `npm run build` before opening a pull request.
4. Use a pull request and retain at least one approving review.
5. Resolve all CI, CodeQL, dependency review, secret scan, and Trivy findings before merge.

Commits should be small, use an imperative subject, and avoid credentials or generated build output.
