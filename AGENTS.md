# Repository Guidelines

## Project Structure & Module Organization
- Unity project. Source lives in `Assets/_Project/1. Scripts` by feature: `UI/`, `Scene/`, `Sound/`, `Localization/`, `DataTable/`, `DesignPattern/`, `Tool/`.
- Scenes in `Assets/_Project/0. Scene` (e.g., `Title.unity`, `Lobby.unity`).
- Prefabs in `Assets/_Project/Prefab`; UI prefabs under `Prefab/UI/` are named after the component type (e.g., `UITitle.prefab`) to match `UIManager` lookup.
- Third‑party/plugins in `Assets/GUPS` (AntiCheat), `Assets/Plugins`, `Assets/LevelPlay`.
- Settings in `ProjectSettings/`; packages in `Packages/manifest.json`. Do not modify or commit generated caches (`Library/`, `Temp/`, `Logs/`).

## Build, Test, and Development Commands
- Open project: launch Unity (matching the repo’s Unity version) and open this folder. For code, open `SCG001-MTL.sln` in Rider/VS.
- Run EditMode tests (CLI): `"<UNITY_PATH>" -batchmode -projectPath "." -runTests -testPlatform EditMode -testResults "TestResults.xml" -quit`
- Run PlayMode tests (CLI): same as above with `-testPlatform PlayMode`.
- Build Windows player (CLI): `"<UNITY_PATH>" -batchmode -projectPath "." -buildWindows64Player "Builds/Windows/SCG001.exe" -nographics -quit`
- Addressables: after asset/key changes, rebuild via Unity: Window → Asset Management → Addressables → Build → New Build.

## Coding Style & Naming Conventions
- C# (Unity) style; 4‑space indentation; UTF‑8.
- PascalCase for public types/methods/properties; camelCase for fields/locals.
- Editor scripts must reside in an `Editor/` subfolder.
- Prefer `Async` suffix for async methods; coroutines may use a `Co_` prefix.
- Keep UI prefab names equal to component type names used by `UIManager` and keep addressable keys stable (e.g., `SoundBox`).

## Testing Guidelines
- Framework: Unity Test Framework (NUnit). Tests live in `Assets/GUPS/AntiCheat/Tests` and follow `*Tests.cs` (e.g., `StorageItem_Tests.cs`).
- Favor deterministic EditMode tests; add PlayMode tests when scene/runtime behavior is required.
- No strict coverage threshold; cover core managers/utilities and any bug fixes.

## Commit & Pull Request Guidelines
- Commits: short, imperative messages (Korean or English). Example: `UI: 버튼 사운드 구조 수정` or `Sound: Fix BGM loop`.
- Use optional module prefixes (`UI:`, `Sound:`, `Data:`, `Tool:`) to clarify scope.
- PRs: include summary, rationale, impacted scenes/assets, test notes, and screenshots/GIFs for UI changes; link issues if applicable.

## Security & Configuration Tips
- Never commit secrets/keystores. Review changes under `ProjectSettings/` before committing.
- Do not edit generated caches. Validate Addressables groups and keys when adding assets.

