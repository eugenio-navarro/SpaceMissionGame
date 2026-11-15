# Space Apps submission package

Prepared from the current source code, project settings, REFERENCES.md and the existing game screenshots. Public form copy below is in English. This file prepares the submission; it is not a confirmation that Space Apps has saved or accepted it.

## Project Name

Mission Control: Build to Launch

## Summary

Mission Control: Build to Launch is a free 3D browser game where players design and launch satellite missions. Players accept contracts for Earth observation, a space telescope or lunar mapping, then choose instruments, power, communications, a rocket and an orbit. Live indicators show how each choice affects mass, energy, data, cost and risk. A preflight equipment list and orbit map help players review the design. During launch, players time stage separation and encounter a random event. Results explain why the mission succeeded or failed, suggest a change and compare the design with a real mission. A guided tutorial, three difficulty levels and Free Mode support different experience levels. A local Mission Archive records the last 30 flights, including what went well, what went wrong and the lesson. The game teaches mission design through choices whose consequences are visible.

## Project Demonstration

Official project video supplied by the team: [MissionControl_Demo.mp4](MissionControl_Demo.mp4).

Duration: **29.4 seconds**. Resolution: **1920 x 1080**. The video includes an audio track. The edit preserves the original audio and most footage, replacing menu/title shots and adding Mission Forecast footage captured with Unity Recorder.

Use this public URL in Project Demonstration after pushing the files to GitHub:

https://github.com/eugenio-navarro/SpaceMissionGame/raw/refs/heads/main/docs/submission/MissionControl_Demo.mp4

Verify that this URL plays or downloads without signing in. Check the spoken language and add English subtitles if needed before submission. Metadata verification alone does not establish the video's language or AI content.

## Project

https://eugenio-navarro.itch.io/mission-control-build-to-launch

Source repository: https://github.com/eugenio-navarro/SpaceMissionGame

## Project Details

WHAT THE PLAYER DOES
The player accepts a mission at Mission Control, enters the Assembly Hangar, chooses equipment in Science, Power, Rocket and Comms tabs, reviews a Master Equipment List and orbit map, and launches. Contracts cover Landsat: Earth Watch, Hubble: Deep Field and LRO: Lunar Mapping. Successful campaign flights unlock the next contract.

ENGINEERING DECISIONS
The design must fit the rocket payload capacity, generate enough power and remain within the campaign budget. The player can select up to three instruments and one to four ground stations. Science depends on data received on Earth and the selected orbit's detail factor. An instrument upgrade can be wasted if communications cannot transmit its data. Eclipse power penalties and limited margins encourage batteries, spare power and appropriate rocket capacity.

LAUNCH AND FEEDBACK
Launch is an animated sequence with a stage-separation timing challenge. Flights encounter solar storms, ground station interference or calm conditions, plus a separate component-failure risk roll. Results classify the outcome as success, partial or failed, explain the causes, suggest what to try next and compare the mission with Landsat 9, Hubble, LRO or SAOCOM. A trade study checks launchable single-change alternatives. Redesign restores the previous spacecraft for another attempt.

ACCESSIBILITY AND REPLAY
An action-based tutorial and Mission Guide explain the concepts. Cadet provides a starter design, more budget and lower risk. Engineer uses standard rules. Flight Director reduces budget and raises risk. Free Mode opens all contracts and removes the budget limit, while retaining mass, power and contract restrictions. Free flights appear in the log but do not count toward campaign progression or ranking. The local archive retains 30 flights, and the campaign ranking retains five scores per contract.

NASA RESOURCES AND EDUCATIONAL SCOPE
NASA's mission life cycle and four mission segments inform the playable workflow. Goddard's Mission Design Lab informs the equipment review, margins and trade studies. CubeSat 101 informs the explanations of power, eclipses, batteries and ground contact. The game includes factual comparisons with real missions. Equipment values and event effects are simplified learning rules, not a high-fidelity orbital or spacecraft simulation.

TECHNOLOGY AND ASSETS
Unity 6.6 (6000.6.9f1), C#, Universal Render Pipeline, uGUI and Input System; WebGL release on itch.io; Git and GitHub for source control. Models and particles are by Kenney (CC0). Planet and star textures are by Solar System Scope (CC BY 4.0). Chakra Petch and Share Tech Mono use the SIL Open Font License. Music and effects are synthesized in code. PlayerPrefs stores progress locally. The game does not use a cloud account, online multiplayer or an online leaderboard.

AI ASSISTANCE
Claude assisted with research, game design, C# code, scene construction and English game text. ChatGPT assisted with publication, the responsive web player, documentation and submission materials. Music and effects come from AI-assisted code. Full sources, credits and AI disclosure: https://github.com/eugenio-navarro/SpaceMissionGame/blob/main/REFERENCES.md

## Use of Super Intelligence (SI)

Select the site's options that correspond to AI-assisted code, written content and research/design. Exact option labels must be checked in the live form; they are not stored in this repository. Do not select 'I did not use SI'.

Disclosure text:

Claude (Anthropic), through Claude Code and the Unity integration, assisted with research, game design, C# code, scene builders and English game text. The project's existing records state that the team made the design decisions, reviewed changes and tested the game. ChatGPT (OpenAI) assisted with itch.io publication, the responsive HTML/CSS web player, documentation and submission materials. The official video must be reviewed separately before making claims about its content or AI use. Music and sound effects are synthesized at runtime by code written with AI assistance. Third-party models, textures and fonts are credited separately in REFERENCES.md.

## NASA Data

Copy each resource into a separate Link Text / Link URL row.

| Link Text | Link URL |
| --- | --- |
| Design and Develop Science Missions (NASA) | https://www.nasa.gov/wp-content/uploads/2025/04/250323-design-and-develop-science-mission.pdf |
| Mission Design Lab (NASA Goddard) | https://etd.gsfc.nasa.gov/capabilities/integrated-design-center/mission-design-lab/ |
| Build It Yourself: Satellite! (NASA Webb) | https://science.nasa.gov/mission/webb/game-build-it-yourself/ |
| CubeSat 101 (NASA CSLI) | https://science.nasa.gov/wp-content/uploads/2023/06/nasa_csli_cubesat_101_508.pdf |
| Landsat 9 (NASA/USGS) | https://science.nasa.gov/mission/landsat-9 |
| Hubble (NASA/ESA) | https://science.nasa.gov/centers-and-facilities/goddard/a-nasa-refresher-on-hubble-facts-for-the-25th-anniversary |
| Lunar Reconnaissance Orbiter (NASA) | https://science.nasa.gov/mission/lro/spacecraft-and-instruments/ |
| SAC-D / Aquarius (CONAE + NASA) | https://www.jpl.nasa.gov/missions/aquarius/ |
| GOLD Rules, GSFC-STD-1000 (NASA Goddard) | https://standards.nasa.gov/sites/default/files/standards/GSFC/G/0/GSFC-STD-1000Revg_signature.pdf |

## Space Agency Partner & Other Data

Copy each resource into a separate row. Solar System Scope appears once because its three texture uses share a source and license. Google Fonts pages are the project-recorded source links; author and license details remain in REFERENCES.md.

| Link Text | Link URL |
| --- | --- |
| SAOCOM 1A y 1B (CONAE, Argentina) | https://www.eoportal.org/satellite-missions/saocom |
| Pérdida de Starlink, febrero 2022 (NOAA/CIRES) | https://cires.colorado.edu/news/minor-geomagnetic-storm-big-impact-february-2022-starlink-satellite-loss |
| Vega y Vega-C (ESA) | https://www.esa.int/Enabling_Support/Space_Transportation/Launch_vehicles/Vega-C |
| Electron (Rocket Lab) | https://rocketlabcorp.com/launch/electron/ |
| Space Kit (CC0) | https://kenney.nl/assets/space-kit |
| City Kit Industrial (v2.0) (CC0) | https://kenney.nl/assets/city-kit-industrial |
| Earth Day Map / Clouds (2k) (CC BY 4.0) | https://www.solarsystemscope.com/textures/ |
| Moon (2k) (CC BY 4.0) | https://www.solarsystemscope.com/textures/ |
| Stars + Milky Way (2k) (CC BY 4.0) | https://www.solarsystemscope.com/textures/ |
| Particle Pack (CC0) | https://kenney.nl/assets/particle-pack |
| Chakra Petch (SemiBold, Bold) (SIL OFL 1.1) | https://fonts.google.com/specimen/Chakra+Petch |
| Share Tech Mono (SIL OFL 1.1) | https://fonts.google.com/specimen/Share+Tech+Mono |
| Unity 6.6 | https://unity.com/ |
| Git | https://git-scm.com/ |
| GitHub source code | https://github.com/eugenio-navarro/SpaceMissionGame |
| itch.io playable release | https://eugenio-navarro.itch.io/mission-control-build-to-launch |
| Claude / Anthropic | https://www.anthropic.com/claude |
| ChatGPT / OpenAI | https://chatgpt.com/ |

## Additional learning resources linked in game credits

These appear as 'Learn more' in ShellScreens.cs. They are not inputs to the game's calculations. List the NASA/JPL items in NASA Data and the CSA item in Other Data, with this scope in Link Text if needed.

| Link Text | Link URL |
| --- | --- |
| GMAT (NASA) | https://software.nasa.gov/software/GSC-19468-1 |
| NASA Mission Viz | https://nasa.github.io/mission-viz/ |
| Concept Maturity Levels (JPL) | https://dataverse.jpl.nasa.gov/dataset.xhtml?persistentId=hdl:2014/44299 |
| Digital Games and Activities (CSA) | https://www.asc-csa.gc.ca/eng/youth-educators/digital-games-and-activities.asp |

## Final submission checklist

- [x] Playable WebGL release published on itch.io and menu verified during publication.
- [x] Public project summary, details, sources and AI disclosure prepared here.
- [x] Current player-focused README prepared.
- [x] Updated project video included; duration verified at 29.4 seconds, resolution 1920 x 1080.
- [ ] Review video language, captions and credits before submission.
- [ ] Commit and push these files to the public repository. Confirm the commit is present remotely.
- [ ] Confirm the official video and all project links open without authentication.
- [ ] Copy the demo URL, project details, NASA Data and Other Data into the team's Project form.
- [ ] Select the appropriate SI-use options in the live form.
- [ ] Confirm all intended members are registered on the team and the correct challenge is selected.
- [ ] The team owner reviews and accepts Terms and Conditions and confirms original work and resource permissions.
- [ ] Save Progress and verify the form retains the text and links.
- [ ] Click Submit for Judging and verify the confirmation banner.

The last live form inspection had an empty demo field, no source rows, unselected SI options and unchecked verification boxes. The browser is currently unavailable for a fresh inspection, so those external steps are deliberately not marked complete.

No extra gameplay mechanics are needed to prepare this package. This checklist does not replace the event's live submission requirements or any separate instructions from the local organizers.
