# References

Proyecto: **Mission Control: Build to Launch** (challenge "Space Mission Design Game") · NASA Space Apps Challenge 2026
Motor: Unity 6.6 (6000.6.9f1) · URP

> Regla del equipo: cada asset que se baja se anota acá **en el momento**, con link y licencia.
> Marcar con `[x]` los que efectivamente quedaron en el juego.

---

## Datos y recursos de NASA (contenido del juego)

| Usado | Recurso | Uso en el juego | Link |
|---|---|---|---|
| [x] | Design and Develop Science Missions (NASA) | Etapas de la misión y 4 segmentos (lanzamiento, espacial, terrestre, usuario) | https://www.nasa.gov/wp-content/uploads/2025/04/250323-design-and-develop-science-mission.pdf |
| [x] | Mission Design Lab (NASA Goddard) | Tabla de equipos, trade studies, margen y riesgo | https://etd.gsfc.nasa.gov/capabilities/integrated-design-center/mission-design-lab/ |
| [x] | Build It Yourself: Satellite! (NASA Webb) | Referencia de juego educativo: opciones bloqueadas, niveles, comparación con misiones reales | https://science.nasa.gov/mission/webb/game-build-it-yourself/ |
| [x] | CubeSat 101 (NASA CSLI) | Energía, baterías en eclipse y contacto con estaciones terrenas | https://science.nasa.gov/wp-content/uploads/2023/06/nasa_csli_cubesat_101_508.pdf |
| [x] | Landsat 9 (NASA/USGS) | Contrato "Landsat: Earth Watch" y ficha en Resultados: 2021, 705 km, 16 días, OLI 9 bandas, +700 escenas/día | https://science.nasa.gov/mission/landsat-9 |
| [x] | Hubble (NASA/ESA) | Contrato "Hubble: Deep Field" y ficha: 1990, 547 km, hasta 5.500 W y ~2.100 W de uso, 6 baterías | https://science.nasa.gov/centers-and-facilities/goddard/a-nasa-refresher-on-hubble-facts-for-the-25th-anniversary |
| [x] | Lunar Reconnaissance Orbiter (NASA) | Contrato "LRO: Lunar Mapping" y ficha: 2009, ~50 km, ~900 Gb/día, instrumento LAMP | https://science.nasa.gov/mission/lro/spacecraft-and-instruments/ |
| [x] | SAOCOM 1A y 1B (CONAE, Argentina) | Ficha en Resultados si el satélite de Tierra lleva radar: 2018/2020, ~620 km, radar banda L | https://www.eoportal.org/satellite-missions/saocom |
| [x] | SAC-D / Aquarius (CONAE + NASA) | Créditos: satélite argentino con el instrumento de NASA que midió la salinidad del mar (2011-2015) | https://www.jpl.nasa.gov/missions/aquarius/ |
| [x] | Pérdida de Starlink, febrero 2022 (NOAA/CIRES) | Caso real en el evento de tormenta solar: 38 de 49 satélites cayeron por el roce | https://cires.colorado.edu/news/minor-geomagnetic-storm-big-impact-february-2022-starlink-satellite-loss |
| [x] | Vega y Vega-C (ESA) | "Similar class" de los cohetes mediano y grande: ~1.500 kg a 700 km polar; ~3.300 kg a órbita baja | https://www.esa.int/Enabling_Support/Space_Transportation/Launch_vehicles/Vega-C |
| [x] | Electron (Rocket Lab) | "Similar class" del cohete chico (unos cientos de kg; no se cita la cifra exacta) | https://rocketlabcorp.com/launch/electron/ |
| [x] | GOLD Rules, GSFC-STD-1000 (NASA Goddard) | Consejo de CAPCOM sobre márgenes de energía y masa (sin citar porcentajes) | https://standards.nasa.gov/sites/default/files/standards/GSFC/G/0/GSFC-STD-1000Revg_signature.pdf |
| [ ] | GMAT (NASA) | "Learn more" en créditos (no se usa en el cálculo) | https://software.nasa.gov/software/GSC-19468-1 |
| [ ] | NASA Mission Viz | "Learn more" en créditos | https://nasa.github.io/mission-viz/ |
| [ ] | Concept Maturity Levels (JPL) | "Learn more" en créditos | https://dataverse.jpl.nasa.gov/dataset.xhtml?persistentId=hdl:2014/44299 |
| [ ] | Digital Games and Activities (CSA) | "Learn more" en créditos | https://www.asc-csa.gc.ca/eng/youth-educators/digital-games-and-activities.asp |

> Los recursos de arriba son los que lista la pestaña *Resources* del challenge, más las páginas de las misiones reales. Las masas, costos, potencias y tasas de datos del juego son valores simplificados para enseñar, no especificaciones reales; así se aclara en el juego y en los créditos.

## Modelos 3D

| Usado | Asset | Autor | Licencia | Uso en el juego | Link |
|---|---|---|---|---|---|
| [x] | Space Kit | Kenney | CC0 | Partes de cohete, terreno y props espaciales | https://kenney.nl/assets/space-kit |
| [x] | City Kit Industrial (v2.0) | Kenney | CC0 | Edificios del centro espacial | https://kenney.nl/assets/city-kit-industrial |

## Texturas y cielo

| Usado | Asset | Autor | Licencia | Uso en el juego | Link |
|---|---|---|---|---|---|
| [x] | Earth Day Map / Clouds (2k) | Solar System Scope | CC BY 4.0 | Planeta Tierra | https://www.solarsystemscope.com/textures/ |
| [x] | Moon (2k) | Solar System Scope | CC BY 4.0 | Luna | https://www.solarsystemscope.com/textures/ |
| [x] | Stars + Milky Way (2k) | Solar System Scope | CC BY 4.0 | Skybox del espacio | https://www.solarsystemscope.com/textures/ |

## Efectos

| Usado | Asset | Autor | Licencia | Uso en el juego | Link |
|---|---|---|---|---|---|
| [x] | Particle Pack | Kenney | CC0 | Humo y fuego del lanzamiento | https://kenney.nl/assets/particle-pack |

## Fuentes

| Usado | Asset | Autor | Licencia | Uso en el juego | Link |
|---|---|---|---|---|---|
| [x] | Chakra Petch (SemiBold, Bold) | Cadson Demak | SIL OFL 1.1 | Textos y títulos de toda la interfaz | https://fonts.google.com/specimen/Chakra+Petch |
| [x] | Share Tech Mono | Carrois Apostrophe | SIL OFL 1.1 | Números (reloj, altímetro, barras y puntajes) | https://fonts.google.com/specimen/Share+Tech+Mono |

## Paquetes de Unity

| Paquete | Uso |
|---|---|
| Universal Render Pipeline (URP) | Render del juego |
| uGUI (Text común) | Toda la interfaz |
| Input System | Mouse y teclado |
| Cinemachine / Timeline | Instalados, pero no se usan: las cámaras y el lanzamiento se animan con código propio |

## Música y sonidos

Generados por código en el juego (`Assets/Scripts/Core/SoundBank.cs`): no se usa ningún archivo de audio de terceros.

## Inspiración de diseño (sin assets usados)

- **Kerbal Space Program** (Squad / Private Division): referencia de estructura (centro espacial, hangar, lanzamiento, vista de mapa) y estilo visual. No se usan modelos, texturas, personajes ni logos del juego.

## Herramientas de IA

- **Claude (Anthropic), con Claude Code y el plugin oficial de Unity:** investigación del challenge y de las fuentes de NASA, planificación del diseño, escritura de gran parte del código C# y de los constructores de escenas, textos del juego en inglés, verificación de datos reales contra las fuentes citadas y pruebas automatizadas en el editor.
- **Revisión humana:** el equipo definió el juego, tomó las decisiones de diseño, revisó cada cambio y probó el juego.
- **Imágenes y audio:** no se usaron imágenes ni videos generados por IA. La música y los sonidos se generan con código (escrito con asistencia de IA).
- **Modelos 3D, texturas y fuentes:** son de terceros (ver arriba), no generados por IA.

---

## Atribuciones requeridas (para los créditos del juego)

- Textures by **Solar System Scope** — https://www.solarsystemscope.com/textures/ — licensed under CC BY 4.0.
- 3D assets by **Kenney** (www.kenney.nl) — CC0.
