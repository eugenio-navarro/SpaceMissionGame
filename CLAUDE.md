# Project instructions

Read README.md for the current game behavior, controls, execution and publication workflow. REFERENCES.md lists sources, credits and AI use. docs/submission/SUBMISSION.md contains prepared delivery materials.

The original planning document is preserved in docs/PROJECT_CONTEXT.md as historical context. Its planned architecture and checklists are not proof of implementation. Verify behavior in current source before documenting it.

- Unity 6000.6.9f1, URP, WebGL. Use the Input System and InputSystemUIInputModule.
- Keep game text and public submission materials in English.
- Explain why designs fail and what players can change.
- Current catalog: Assets/Scripts/Data/TestCatalog.cs. Rules: MissionCalculator.cs and MissionOutcome.cs. Do not describe ScriptableObjects or MissionState as implemented without checking source.
- Browser saves use PlayerPrefs. No threads or file-based browser saves.
- Preserve docs/web-player/ when rebuilding WebGL.
- Coordinate before editing another teammate's scenes. Historical ownership is in section 9 of docs/PROJECT_CONTEXT.md.
- Prefer simple hackathon changes. No mandatory TDD or ADRs. Record new external resources in REFERENCES.md.
- Keep commits clear. Do not mark an external submission complete without observing confirmation.
