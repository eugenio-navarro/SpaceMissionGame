<!-- dev-team plan -->
## Handoff

### Goal

Deliver a local playable increment with readable desktop layouts, simultaneous mission events, a sliding forecast tablet, meaningful equipment explanations, and a visible success-gated efficiency reward. Preserve the existing three-contract assembly, flight, results, and redesign loop.

### Context

- `docs/game/gdd.md` specifies cloud cover, extended lunar eclipse, and the three parts.
- `README.md` and `CLAUDE.md` define project conventions. The latter requests simple implementation, no TDD, and no ADRs.
- `MissionHandoff.Build` snapshots the design into `ResultsInfo.design`; `MissionOutcome.Resolve` applies events before the downlink limit.
- Both `LaunchSequence.Start` and `ResultsController.Start` currently select three event enum values directly. They must roll all eligible hazards independently once per flight.
- `TestCatalog` is the active catalog despite the README's eventual ScriptableObject migration. `SaveData` saves IDs, so additive parts fit existing storage.
- Most canvases use a 1920x1080 reference and geometric width/height matching. Narrower aspect ratios reduce logical width below the fixed column layout's needs.
- Unity reference is `C:/Users/ASUS/.codex/plugins/cache/gaisser-agents/gaisser-agents/4.6.0/skills/game-team/references/engine-unity.md`.

### Constraints

- User authorizes full local implementation and review. No push, publishing, or dependency installation.
- Preserve existing modifications in Packages, .vscode, and docs. Never hand-edit scene YAML or regenerate entire scenes to fix layout.
- English player-facing text. NASA sources for new scientific claims. Loss fractions and part statistics are game balance assumptions.
- No new textures, models, packages, simulation loops, contracts, or in-flight operating dialogs. The user specifically requests an in-hangar sliding tablet.
- Keep enum values and existing part IDs stable. Keep ordinary battery and calm-flight calculation behavior.
- Apply fixes to existing saved scenes at runtime and maintain builder consistency.
- Build stages run sequentially. Programmer finishes before technical artist. QA verifies independently.

### Files to touch

Programmer owns these entries.

1. `Assets/Scripts/Data/Specs.cs` — optional narrowly scoped battery endurance field, with basic-battery default.
2. `Assets/Scripts/Data/TestCatalog.cs` — add panel_m, ant_m, bat_extended, descriptions and NASA radar example.
3. `Assets/Scripts/Launch/LaunchInfo.cs` — append CloudCover and ExtendedEclipse enum values.
4. `Assets/Scripts/Results/MissionOutcome.cs` — eligibility, selection, effects, explanations, storm wording.
5. `Assets/Scripts/Launch/LaunchSequence.cs` — use eligible event selector.
6. `Assets/Scripts/Results/ResultsController.cs` — use same selector for direct scene entry.
7. `Assets/Scripts/Core/MissionCalculator.cs` — short contract-specific preparation advice.
8. `Assets/Scripts/Core/TradeStudy.cs` — enumerate battery replacement alternatives and removal.
9. `Assets/Scripts/Core/FlightLog.cs` — remove factual assertion that power margin guarantees storm protection, if present.
10. `README.md` — concise new-event and new-part updates, mark balancing assumptions.
11. `REFERENCES.md` — NASA sources for this increment.

Additional programmer-owned entries authorized by the user's follow-up.

28. `Assets/Scripts/Core/MissionForecast.cs` — shared hazard probabilities and deterministic tablet forecast data using the same resolution rules as flight.
29. `Assets/Scripts/Results/ResultsInfo.cs` — additive list of occurred events while preserving existing single-event fields for compatibility.
30. `Assets/Scripts/Core/SaveData.cs` — outcome-aware efficiency bonus and score overload, existing historical storage preserved.

Technical artist owns these entries, editing only where visual evidence or source geometry identifies a problem.

12. `Assets/Scripts/Core/UILayout.cs` — new small shared canvas configuration and existing-scene layout correction helper.
13. `Assets/Scripts/Core/GameShell.cs` — invoke layout correction on loaded scenes and configure persistent canvas.
14. `Assets/Scripts/Core/UIFactory.cs` — reusable bounded text or scrolling support when required.
15. `Assets/Editor/SceneBuilders/BuilderUtil.cs` — shared canvas configuration.
16. `Assets/Editor/SceneBuilders/HangarUIBuilder.cs` — corrected hangar geometry, alerts viewport.
17. `Assets/Editor/SceneBuilders/HubUIBuilder.cs` — correct diagnosed contract-panel text bounds.
18. `Assets/Editor/SceneBuilders/MenuSceneBuilder.cs` — correct diagnosed menu text bounds.
19. `Assets/Scripts/Hangar/HangarUI.cs` — runtime layout application, accessible catalog and alerts.
20. `Assets/Scripts/Hangar/MissionReview.cs` — readable preflight content and footer.
21. `Assets/Scripts/Hub/HubModal.cs` — modal bounds.
22. `Assets/Scripts/Core/ShellScreens.cs` — help/settings/credits bounds as needed.
23. `Assets/Scripts/Results/Editor/ResultsSceneBuilder.cs` — canvas configuration and event/result bounds.
24. `Assets/Scripts/Launch/Editor/LaunchHudBuilder.cs` — canvas configuration and event bounds.
25. `Assets/Editor/LocalReview.cs` — editor-only verification and Windows build entry points, optional render/capture runner.
26. `Assets/Scripts/Core/LocalReviewCapture.cs` — optional development-build-only, explicitly activated screenshot harness (`--visual-review`), omitted from ordinary release compilation.
27. `docs/game/credits.md` — record that this increment reuses existing visual assets.

31. `Assets/Scripts/Hangar/MissionTablet.cs` — technical-artist-owned sliding panel, probability bars, mitigation state and science/score preview. Programmer provides model/API first.
32. `Assets/Scripts/Menu/MainMenu.cs` — technical artist updates displayed product name to `Mission Control: Build to Launch`, including authored title-line sizing.
33. `ProjectSettings/ProjectSettings.asset` — update PlayerSettings.productName through an editor API only if existing PlayerPrefs storage can be preserved. Keep company and application identifiers stable.

QA owns `docs/game/playtest.md` and may extend editor-only checks in `Assets/Editor/LocalReview.cs` after technical artist handoff. Unity generates .meta files for new Assets files. Logs, local builds and screenshots go under ignored `Logs/`, `Builds/`, or `Temp/` directories.

### Out of scope

Propulsion, orbit decay simulation, gyroscope hardware, onboard storage, new contracts, online services, scene art replacement, and catalog architecture migration.

### Acceptance criteria

- AC1: Existing catalog IDs and enum values remain stable, with exactly three new parts using the GDD statistics. Check: `LocalReview.Verify` verifies identifiers and numeric values. Pass: all assertions succeed.
- AC2: CloudCover halves cam_b/cam_a data only before downlink; radar and spectrometer contributions persist. Check: isolated camera, radar, mixed payload, and downlink-limited cases with riskRoll=99. Pass: received equals min(adjusted generated data, downlink), with correct rounded score.
- AC3: ExtendedEclipse retains 60%, 85%, and 100% of generated data for absent, basic, and extended batteries. Check: lunar cases with generous downlink and riskRoll=99. Pass: expected fractions and feedback; ordinary shadow calculation remains unchanged.
- AC4: Event eligibility permits CloudCover only for Earth observation and ExtendedEclipse only for lunar mapping in lunar orbit. Check: selector across all three contracts, forced incompatible event, unknown event value, and ResultsInfo without design. Pass: invalid/unsupported events resolve to Calm without exceptions or unintended loss; old events remain available.
- AC5: Existing reference designs retain calm calculations and launch validity. Check: evaluate the README reference designs before/after with default difficulty. Pass: same calm totals and same lunar medium-rocket mass rejection.
- AC6: New parts round-trip through save/load and redesign. Check: temporary unique save key restored/deleted in finally; clone and trade-study alternatives. Pass: IDs and totals retained and all battery choices evaluated.
- AC7: Menu, hub, hangar, launch and results remain within viewport at 1920x1080, 1366x768, 1280x800 and 1024x768. Check: manual rendered screenshot review at every size, with geometry checks for controls. Pass: no overlapping cards, clipped essential text, off-screen primary buttons, or negative-height panels.
- AC8: All hangar tabs expose final rows, alerts remain readable for empty and invalid designs, and preflight footer stays reachable. Check: manual scroll and click through catalog and review, including extended battery and lunar orbit. Pass: all options and actions can be reached.
- AC9: Contract modal, archive, guide, settings, pause, event card, and results/trade study support longest current text. Check: manual screenshots and navigation. Pass: visible padding and accessible close/back controls, scrolling where needed.
- AC10: Real flight from hangar reaches results, redesign preserves choices, and Earth/lunar events explain the affected subsystem. Check: manual complete two runs, force deterministic event only through editor verification tooling. Pass: one result recording per run and no console errors.
- AC11: Windows local build succeeds with all five configured scenes. Check: `unity run C:/dev/SpaceMissionGame --timeout 600 --log-file Logs/local-review-build.log --no-tail -- -executeMethod LocalReview.BuildWindows -nographics`. Pass: BuildReport succeeded and `Builds/LocalReview/SpaceMissionGame.exe` exists. If licensing prevents execution, document exact blocker and keep criterion unverified.
- AC12: New work adds no packages or art payload and performs layout work only on scene load or size change. Check: diff and runtime inspection. Pass: no new per-frame object scans or allocations from this increment. Record observed frame timing/memory if the renderer is available, otherwise explicitly unverified.
- AC13: Independent event rolls permit two or more hazards in the same flight, with Calm representing no occurrence. Check: deterministic all-hit/no-hit/mixed roll inputs and repeated Resolve on the same snapshot. Pass: expected distinct event set, no duplicate damage and no reroll between launch/results.
- AC14: Composed effects are order-independent to the caller. Check: reverse/duplicate an input event list containing solar storm, cloud cover and station interference; repeat for solar storm plus lunar eclipse. Pass: canonical unique event order gives equal final data/science, removed instrument data is never subtracted twice, downlink reduction applies once and failure roll applies once.
- AC15: Tablet displays occurrence probabilities from the flight rules, equipment mitigation, per-event projected science and a combined-event preview. Check: manual open, change each relevant part/station while tablet open, close/Escape and reopen at all four resolutions; compare values against deterministic rule checks. Pass: chart/labels update on design changes, probabilities are labeled as independent simplified game values and never claim a mitigation changes event occurrence.
- AC16: Savings bonus is awarded only when a campaign mission reaches its science goal, capped from zero to budget with nonnegative cost. Check: successful cheap/expensive/overbudget/negative-cost cases, partial/failed missions, and Free mode. Pass: bonus equals max(0, budget-max(0,cost)) only for qualifying success, with bonus zero otherwise; total score and displayed breakdown agree.
- AC17: Both camera descriptions explain purpose, mass/power/cost tradeoffs, and cloud vulnerability. Check: manual hover/tablet inspection and catalog source review. Pass: basic camera communicates cheaper/lighter survey use, advanced camera communicates higher useful data with higher resource demand, and no unsupported resolution or spectral simulation is claimed.
- AC18: The success reward is visible without a record/unlock popup, and historical archive remains readable. Check: results for new success/partial/free runs plus an old archive record. Pass: science contribution, efficiency bonus and total are visible, all new storage/announcements use outcome-aware scoring, and existing saved records are retained.
- AC19: Player-visible branding reads `Mission Control: Build to Launch` in menu, hub, help/credits and documentation. Check: text search plus menu/hub screenshots and build title. Pass: old product title is replaced everywhere player-visible, official quoted challenge title remains accurate, and technical paths/identifiers and existing save access remain stable.

## Design decisions and trade-offs

The active catalog and save format remain in use. Additive entries offer choices without a migration. Scientific source links are listed below; every numerical event reduction and hazard probability is a balancing decision from the GDD. Independent per-hazard rolls add compound outcomes without adding flight interactions. The tablet explains occurrence and mitigation separately and previews consequences using the flight resolver.

Use one small shared layout entry point to configure CanvasScaler with `ScreenMatchMode.Expand` and the existing reference resolution. This retains at least the authored logical dimensions at narrower desktop aspects. Correct crowded text/card interiors independently using bounded text and scrollable long content. A wholesale responsive layout rewrite would broaden the requested change unnecessarily.

Runtime corrections must be idempotent and use known controllers/references. Prefer a scene-load application plus targeted HangarUI initialization. Do not repeatedly search all objects every frame. Preserve world objects, scene GUIDs and serialized controller bindings.

- https://svs.gsfc.nasa.gov/5661 — radar observations through clouds.
- https://www.nasa.gov/missions/nasas-lunar-reconnaissance-orbiters-dance-with-eclipses/ — LRO power conservation during eclipses.
- https://svs.gsfc.nasa.gov/5193/ — atmospheric drag mechanism in the Starlink case.

## Risks and edge cases

- Design-less ResultsInfo is used when scenes open directly. New events need a conservative Calm fallback; avoid inventing payload or battery state.
- For compound events, compute remaining per-instrument production first. Solar shutdown removes its chosen instrument, cloud cover scales only surviving cameras, eclipse then scales surviving total, station interference scales downlink, and risk scales final science once.
- Keep a legacy single-event Resolve overload. Populate old eventKind with the first canonical occurred event or Calm, while eventTitle/eventText/why summarize the full event list.
- Existing top-score records include the older savings formula. Preserve their numbers as historical scores; apply the new reward rule prospectively and explain new breakdowns consistently.
- `Why` currently claims all original generated data arrived before describing a subsequent reduction. Rewrite that sentence around adjusted data for affected events.
- A reduction can be absorbed by excess downlink-limited data. Distinguish instrument downtime from actual received-science loss.
- Existing basic battery remains the default for old saves and tests. If a new endurance field is added, missing/zero values must retain basic behavior.
- Tests and automated play sessions must restore relevant PlayerPrefs and never reset user progression.
- Existing result intro animations may reposition cards after initialization. Apply compatible base geometry and preserve animation endpoints.
- Expanding canvas dimensions can shrink physical text at 4:3. Review readability at 1024x768; adjust crowded interiors rather than globally shrinking fonts.

## Ordered steps

1. Programmer implements additive catalog changes and event enum entries. Use existing fields for panels and antennas. Prefer a battery `extendedEclipseRetention` field defaulting to 0.85f only if it improves clarity; extended sets 1f and absent is 0.6f in event rules.
2. Add `MissionOutcome.IsEligible(ResultsInfo run, LaunchEventKind kind)`, `MissionForecast.EventProbability(ResultsInfo run, LaunchEventKind kind)`, and `MissionForecast.RollEvents(ResultsInfo run, System.Func<float> nextRoll)` returning an event array. Rolls are percentages in [0,100), each eligible non-Calm hazard gets its own roll. GDD probabilities are Storm 15%, StationInterference 15%, Earth CloudCover 30%, Lunar ExtendedEclipse 25%; define once for both flight and tablet. Empty array represents Calm. New events require a non-null design, and lunar additionally requires orbit `lunar`. Preserve enum values and ignore unknown/ineligible forced events.
3. Retain `Resolve(ResultsInfo, LaunchEventKind, float)` as a wrapper around `Resolve(ResultsInfo, IReadOnlyList<LaunchEventKind>, float)`. Normalize duplicates and canonical ordering. Work from the cloned design's per-instrument production, remove the storm-disabled instrument once, scale surviving cameras for clouds, apply eclipse retention, then apply station downlink reduction, existing min bottleneck, rounding, and one failure roll. Keep old no-design behavior for old events. Populate additive occurred-event list plus aggregate existing title/text/why fields. Resolve always starts from original snapshot totals, so repeating it does not compound prior state.
4. Update both normal selection entry points to independently roll eligible events once and preserve `resolved`. Extend camera descriptions with mass/power/cost and purpose explanations grounded in existing behavior. Add concise preparation advice through calculator warnings. Enumerate all batteries in TradeStudy alongside no battery. Add `SaveData.EfficiencyBonus(int budget, int cost, bool goalReached, bool freeMode)` and outcome-aware `Score(ResultsInfo run, bool freeMode)`, retaining the legacy three-argument method for source compatibility. Update every new-run scoring call, including FlightLog.Record and result announcement, to the outcome-aware method. Store historical scores unchanged.
4a. Programmer adds forecast rows with kind, probability, projected science, mitigation description and mitigation status; obtain consequence previews by resolving isolated fresh snapshots with riskRoll=100, never touching ResultsContext or saves. Add baseline and all-eligible combined scenario previews. Forecast includes current separate hardware failure probability from MissionReport, clearly labeled separately. Output proposed score breakdown using the same success-gated reward method. Display P(2 or more events) = 1 - P(none) - P(exactly one), computed from eligible independent probabilities. Unit-check this against enumeration of event subsets. Label the all-hazards consequence preview explicitly so its severity is not confused with P(2+).
4b. Technical artist adds `MissionTablet.Create(Transform root, HangarController controller)` with Open/Close/IsOpen. Hook into HangarUI initialization and controller.Changed, unsubscribe on destroy. Slide with unscaled time only while animating; no per-frame prediction recalculation. Use existing fonts/cards, horizontal filled bars and numerical probability labels. Keep a visible toggle, reachable close and Escape handling before general pause. Provide scrollable body for every row and combined preview at small sizes. Add a persistent results score breakdown using the supplied scoring API, including zero bonus reason on partial/free results. Coordinate ResultsController edits after programmer handoff.
5. Technical artist captures current screens where feasible, then applies shared canvas sizing and targeted layout fixes. Ensure existing scenes benefit without regeneration. Maintain builder defaults. Make long alerts scrollable and retain catalog scroll access.
6. Technical artist adds `LocalReview.Verify` and `LocalReview.BuildWindows` in editor-only code. Verify deterministic rules and save round-trip using temporary keys and try/finally cleanup. Build with configured enabled scenes to Builds/LocalReview. Capture tools may enter Play Mode and set resolution, but must restore preferences and selected scene. An opt-in DEVELOPMENT_BUILD capture harness is permitted for a review build when headless editor capture is unreliable. It runs only with `--visual-review`, writes PNG evidence, and is omitted from release compilation. Root coordinates Unity processes; do not start concurrent editor instances. Elevated Unity baseline compilation succeeded after sandbox licensing restrictions were removed.
7. QA runs Verify, local build, renderer/screenshots and two complete mission loops. Inspect all resolutions and modals, record each AC verdict and screenshot path in docs/game/playtest.md. A green compile alone does not pass visual criteria. Return concrete defects to the owning build role, with at most two fix cycles.
8. Apply the requested branding across visible strings. Programmer updates README and documentation product mentions. Technical artist updates MainMenu, ShellScreens.GameTitle, HubUIBuilder and runtime saved hub labels. Keep the official challenge title in quotations unchanged. Product-name metadata can change only while preserving the existing PlayerPrefs namespace; because productName changes Windows standalone preference location, retain the existing internal productName if safe migration is outside scope and disclose that decision, while using the requested visible window/game title.

## Open questions and assumptions

None blocking. The user requested a finished local solution, so implementation proceeds under that authorization. The follow-up adds simultaneous hazards, tablet forecasts, richer camera explanations, and visible reward for fulfilling the goal economically. Windows executable is the immediate local preview; WebGL compatibility is preserved by avoiding new runtime dependencies and filesystem operations.

## Memory

None. CLAUDE.md explicitly excludes ADRs for this project.

## Revisions

- 2026-11-15 — Added AC13–AC18 and four scoped files for the user's follow-up. Independent event composition replaces single random selection; the tablet shares rules with flight, and new-run savings rewards require objective completion. Existing AC IDs and historical data remain stable.
- 2026-11-15 — Added requested branding and AC19. Locked GDD probabilities at 15/15/30/25 and added simultaneous-event probability P(2+) to the forecast.
