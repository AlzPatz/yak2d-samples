# CLAUDE.md

Guidance for Claude Code (and smith runs) working in this repo.

About 35 standalone demo projects for yak2D, one per feature (bloom, blur, distortion, mesh, input
and so on). It's a .NET (net10.0) solution: `src/yak2d-samples.sln`.

## Commits

Commit as the user only: never add `Co-Authored-By: Claude` trailers, "Generated with Claude Code"
lines or any other AI attribution to commit messages or PR descriptions.

## Commands

Run from `src/`:

```shell
dotnet restore
dotnet build -c Release --no-restore
dotnet run --project AllDemoLauncher
```

There are no tests: building the solution is the check.

## Layout

- Each demo (e.g. `Bloom_Example`, `Draw_FontExample`) is its own console app referencing
  `SampleBase`.
- `AllDemoLauncher` aggregates every demo (`AllDemoLauncher/Demos/*.cs`) behind a shared
  `ApplicationBase`.
- A new yak2d feature gets a demo here, and the demo is also folded into `AllDemoLauncher`.

## How yak2d is referenced

`SampleBase` and `AllDemoLauncher` use the same switch:

- When the yak2d repo is checked out beside this one (`../../../yak2d/`), they reference its
  source (`ProjectReference`), so the samples always build against the latest yak2d code.
- Otherwise, or with `-p:UsePackages=true`, they use the `Yak2D` NuGet package (`1.3.*`).

So an isolated clone (CI, or a smith run, which has no sibling folders and no network) builds
against the published package. When yak2d's minor version moves on (e.g. to 1.4), update `1.3.*`
in both project files. The solution lists only this repo's projects, never yak2d's; keep it that
way, or the repo stops building on its own.

`CustomVeldrid_ComputeShaderExample` has a similar switch for `veldrid-spirv`, which is normally
not checked out, so it uses the NeoVeldrid packages.
