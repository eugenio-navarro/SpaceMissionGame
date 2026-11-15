# Local gameplay and visual polish increment

## Goal

Give the existing three-contract mission-design game more meaningful equipment choices and readable screens. Keep the hangar, automatic flight, results, and redesign loop. Unity 6000.6.9f1 with URP and the existing uGUI implementation. Deliver a local reviewable build.

## First playable

Add two mission-appropriate event types and three catalog options. Decisions happen during assembly, supported by a sliding forecast tablet with probability charts and equipment-specific mitigation. Several eligible events can occur in one flight. Resolve their combined effects through the existing flight and results flow. Each result explains the affected subsystem, identifies useful equipment, and shows the reward for achieving the mission within budget.

## Forecast tablet and combined events

Add a visible `MISSION FORECAST` button in the hangar that opens a sliding tablet. Build it from existing uGUI elements, with simple bars, labels, and restrained animation. The panel has a reachable close control, responds to Escape, and scrolls at smaller sizes. Refresh it when equipment changes or the panel opens. Avoid continuous simulation or new assets.

Show only events eligible for the selected contract and orbit. Each row contains event name, probability bar with a printed percent, effect, current protection, and a concrete mitigation. Include a persistent `Simulation probabilities — simplified game model` label. The probability and effect values must come from the same shared definitions used by mission resolution.

Initial balancing probabilities are 15 percent for solar storm, 15 percent for ground-station interference, 30 percent for Earth cloud cover, and 25 percent for extended lunar eclipse. Roll each eligible event independently once. Calm means none occurred. Show the computed probability of at least one event as one minus the product of the no-event probabilities. These are gameplay probabilities, with no claim of NASA forecasting accuracy.

Compute compound effects exactly once. Start from each instrument's original contribution, remove a storm-disabled instrument, reduce surviving camera contributions for clouds, apply eclipse retention to remaining generated data, and apply station interference to downlink. Then evaluate the receiving bottleneck and existing launch-risk effect once. Feedback distinguishes triggered events from events that caused an actual loss. Present all triggered events in flight and results without losing the existing single-event compatibility path.

## Science choices and efficiency reward

Camera descriptions explain mission purpose and the modeled tradeoff. The basic camera offers broad, low-cost optical observations. The advanced camera offers more detailed optical observations at higher mass, power, cost, and data volume. Both depend on a clear view of the target. Radar supplies surface observations through clouds at greater power demand. Treat these as simplified instrument roles and display concrete existing statistics alongside the explanations.

Show a science score breakdown and a budget-efficiency bonus on results. The base remains science multiplied by 10. Award one bonus point per unspent budget million only when the science goal is achieved. A partial or failed mission receives zero efficiency bonus. Free mode receives zero budget-efficiency bonus because its spending is unrestricted. Show `Goal achieved` and the savings calculation when eligible, or explain that reaching the goal unlocks the savings bonus. Preview the potential bonus in the tablet as conditional on mission success.

Existing SaveData.Score already adds unspent budget to science times 10 for every outcome. Introduce a success-aware scoring entry point and update actual result callers while preserving old saved scores and their storage keys. Label prior leaderboard entries as historical if necessary because their stored integer values cannot be reconstructed under the new rule.

### Cloud cover

Eligible for the Earth observation contract. Thick cloud reduces useful observations from the two optical cameras by 50 percent for this simulated interval. Radar retains its contribution. Preserve the spectrometer contribution until a specific atmospheric band model exists. Apply the effect per instrument before the downlink bottleneck. Feedback names the camera loss and the radar benefit when present. Suggested card title is `CLOUD COVER`.

NASA documented NISAR imaging flooding in Mozambique through clouds. The game's numerical penalty is a balancing assumption. Source https://svs.gsfc.nasa.gov/5661

### Extended lunar eclipse

Eligible for the lunar contract. Model extra science downtime during a prolonged eclipse beyond the routine shadow factor already in the calculator. A design with no battery retains 60 percent of generated science data during this simulated interval, the existing battery retains 85 percent, and the new extended battery retains all of it. Keep baseline battery behavior in ordinary flight. Explain conservation shutdowns and battery endurance in the event card and redesign advice. Suggested card title is `EXTENDED ECLIPSE`.

LRO conserved battery power by shutting down most instruments during eclipses and could keep Diviner operating for special science observations. These game fractions are deliberately simplified balance values. Source https://www.nasa.gov/missions/nasas-lunar-reconnaissance-orbiters-dance-with-eclipses/

## Hangar additions

| ID | Name | Category | Proposed game values | Tradeoff |
|---|---|---|---|---|
| panel_m | Medium solar panel | SolarPanel | 30 kg, 180 W generation, $15M | Intermediate mass, price, and power |
| ant_m | Medium antenna | Antenna | 18 kg, 25 W draw, 70 GB/day, $10M | More downlink than small antenna at lower mass and draw than large |
| bat_extended | Extended battery | Battery | 55 kg, $18M | Extra mass and cost for extended-eclipse endurance |

Keep existing IDs and save representation. Reuse catalog selection, mass, cost, power, and downlink calculations. Battery endurance can be one narrowly scoped data field with an explicit legacy/default treatment. Display the extended-eclipse benefit in both battery descriptions. Describe all numerical equipment values as simplified game values. Add a NASA NISAR example to radar text.

## Scope and sequencing

Use existing contract progression, budgets, scenes, and equipment categories. Keep the current three events and append enum values so existing numeric values remain stable. Event selection filters by contract and orbit. A manually forced incompatible event must resolve safely. A data-only test result without a full design must have explicit fallback behavior.

Cloud cover and eclipse add prelaunch planning decisions. Propulsion, orbital decay, gyroscope redundancy, onboard storage, new contracts, and operational choice dialogs belong to a future increment. This cut requires no additional textures, models, packages, network services, or per-frame simulation.

The solar-storm card's current power rule is a gameplay abstraction. Its historical Starlink citation must explain atmospheric drag accurately and avoid implying that power margin caused the real outcome. Prefer a concise NASA-backed storm explanation and label modeled thresholds as game rules. Source https://svs.gsfc.nasa.gov/5193/

## Visual acceptance

Review menu, hub, contract selection, every hangar tab, preflight review, launch event card, results, redesign/trade study, pause, help, settings, and mission archive where present. Use screenshots at 1280x720, 1920x1080, and a narrow desktop aspect such as 1024x768. Preserve readable padding, allow long text to wrap or scroll, keep actionable buttons reachable, and prevent neighboring cards from overlapping. Growing the parts catalog must retain access to its final row. Fix runtime-generated and editor-generated UI at their source so opening existing scenes receives the correction.

## Verification targets

Verify one complete Earth and lunar run with the new events, radar/camera cloud behavior, all three battery states, ordinary calm results, invalid-event filtering, save/load and redesign of new parts, and small-window navigation. Force simultaneous storm/cloud/interference and eclipse/interference combinations and confirm each effect applies once. Verify forecast probabilities match resolution definitions, its at-least-one calculation, all tablet controls, and readable probability bars. Compare equally successful expensive and economical designs, plus failed and free-mode cases, to verify the visible savings bonus. Confirm known valid starter/reference designs can still launch. Capture visual evidence and build locally with compile errors treated as failures. All player-facing text is English.

## Originality and evidence

Mechanics extend this project's existing educational mission-design loop. Scientific claims in this increment use the NASA sources above. Part values, event weights, and loss fractions are game design choices and need play balancing. Visual review is complete only after rendered screens have been inspected.
