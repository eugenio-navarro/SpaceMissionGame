# Local verification — 2026-11-15

Mission Control: Build to Launch. Windows development build with Unity 6000.6.9f1.

Build executable `Builds/LocalReview/Mission Control - Build to Launch.exe`.
Build evidence `Logs/local-review-final-build.log` reports 42 deterministic checks passed and a successful 182,807,611-byte build.
Runtime evidence `Logs/local-review-player-final.log` and `Builds/LocalReview/Captures/errors.txt`. The error report is empty and `complete.txt` records 52,438 frames and completion.

The primary agent completed verification after the independent QA agent encountered the account usage limit. Visual review combined contact sheets for all captured states with full-size inspection of menu, hangar, forecast, preflight, events, results, guide, options, contracts and credits. Automated interactions invoke the same game button handlers and controller entry points. Keyboard-only and physical mouse interaction require a human playtest.

| Criteria | Verdict | Evidence |
| --- | --- | --- |
| AC1 | Pass | Unique IDs, stable enum values, three catalog additions in LocalReview.Verify |
| AC2–AC3 | Pass | Camera/radar/mixed payload and all battery retention cases |
| AC4 | Pass | Ineligible and unknown events, designless fallback, independent roll thresholds |
| AC5 | Pass | Existing Earth 60, telescope 80, lunar 58 science reference designs and lunar mass rejection |
| AC6 | Pass for new IDs and redesign | Temporary-key save round trip, cloning and eight runtime redesign returns |
| AC7 | Pass for captured states | Four resolutions, five scenes, screenshots under Captures |
| AC8 | Pass for automated navigation | All tabs and scrolled endpoints, empty-design alerts, preflight footer |
| AC9 | Pass for captured states | Contract, archive, guide, options, event and result panels. Populated historical archive and keyboard pause remain manual |
| AC10 | Pass for automated route | Earth and Moon through hangar, launch, existing Skip action, results and Redesign at each size. Campaign recording suppressed to protect progress |
| AC11 | Pass | Windows development build succeeded |
| AC12 | Pass on measured desktop sample | No new packages/art. Scene-load layout work. Timing below |
| AC13–AC14 | Pass | Independent rolls, duplicate events, reversed ordering, repeated resolution and one hardware-failure reduction |
| AC15 | Pass for automated interaction | Shared forecast definitions, 50.575% no-event and 9.9% simultaneous-event Earth checks, live antenna change screenshots |
| AC16 | Pass | Success, partial/failure, free mode, overbudget and negative-cost bonus checks |
| AC17 | Pass | Both camera descriptions explain mass, power, cost, downlink and cloud tradeoffs; tablet shows both |
| AC18 | Pass for new results | Partial Earth bonus 0 and successful Moon bonus 167 visibly shown. Historical archive rendering with populated old records remains manual |
| AC19 | Pass | Menu, hub, credits and native Windows title use new name. Internal identity preserves PlayerPrefs access |

## Performance sample

Ten seconds in the 1920×1080 hangar after warmup, rendered Windows development player on this computer. 1,969 frames, average 5.080745 ms, 95th percentile 7.118403 ms, allocated memory 161,355,134 bytes (153.9 MiB). This meets the 16.7 ms desktop target in this sample. There is no before/after performance comparison or WebGL browser measurement.

## Fixes from verification

- Added the three catalog entries missing from the initial implementation, caught by deterministic verification.
- Added scrollbars to overflowing content and clarified the calm-flight trade-study comparison.
- Increased modal background opacity to improve text readability over scene content.
- Verified empty hangar alerts, live forecast updates and redesign retention in the final run.
- Reverted build-generated graphics/settings churn in files that were clean before this task.

## Remaining manual checks

- [ ] PT1 (AC8/AC15) Drag scrollbars, select every part with a physical mouse, close/reopen the tablet with Escape.
- [ ] PT2 (AC9) Open a populated historical archive and use the pause menu via keyboard.
- [ ] PT3 (AC10) Complete the full animated launch with the separation challenge and verify campaign score recording once.
- [ ] PT4 (platform) Build WebGL and play in a browser. This run delivered and measured Windows.

All new probabilities, loss fractions and part statistics are game-balance assumptions. NASA evidence for the physical phenomena is in REFERENCES.md. No commits or pushes were made.
