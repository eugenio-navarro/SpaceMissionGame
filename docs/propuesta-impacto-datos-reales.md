# Propuesta: más impacto con datos reales de NASA y CONAE

Documento de trabajo para iterar entre los dos (y con nuestros asistentes de IA).
Fecha: 15/11/2026. Entrega: hoy 23:59 hora argentina.

**Cómo leerlo si sos un asistente de IA:** este archivo propone cambios; no los des por decididos. Cada tarea dice qué archivo toca y cuánto riesgo tiene. Antes de escribir un número real en el juego, abrí la fuente citada y confirmalo: la columna "Confianza" dice cuáles están verificados y cuáles no. No rebalancees las fórmulas del juego (sección 2). La fuente de verdad del proyecto sigue siendo `README.md`.

---

## 1. Qué exige la entrega

Sale de la *Project Submission Guide* oficial (https://www.spaceappschallenge.org/resources/project-submission-guide/).

| Requisito | Detalle | Estado nuestro |
|---|---|---|
| Plazo | Apretar "Submit for Judging" antes del domingo 15/11 a las 23:59. Se puede reenviar; vale el último guardado. | Pendiente |
| Idioma | Toda la página del proyecto y los materiales enlazados, en inglés. | El juego ya está en inglés |
| Demo | Video de **30 segundos como máximo** o hasta 7 slides. Si se pasa, queda fuera del juzgamiento global. Video en otro idioma: subtítulos en inglés. | Pendiente |
| Links | Públicos, sin login ni permisos. Probarlos en ventana privada. | El repo es privado: hay que abrirlo |
| Datos de NASA | Usar al menos un dato o recurso de NASA para aspirar a premio global. | Tenemos documentos; faltan datos dentro del juego |
| Agencias socias | Recomendado. La CONAE argentina figura en la lista. | No usamos ninguna |
| IA | Declarar dónde y cómo se usó. Imágenes y videos generados con IA: marca de agua visible. Código y audio: reconocerlo en texto y metadatos. | Pendiente |
| Otros | Sin logos de NASA en contenido generado. Ninguna persona menor de 18 en el material. Todos los miembros en el mismo evento local. | Revisar |
| Campos | Nombre, resumen, link al demo, link al proyecto, descripción detallada, uso de IA, datos de NASA, otras referencias. | Pendiente |

Los jueces puntúan cinco criterios: **Impact, Creativity, Validity, Relevance y Presentation**. La mayor parte de la nota sale de la página del proyecto, no del video.

---

## 2. Diagnóstico: qué es real y qué es inventado hoy

**Real (estructura):** las fases de una misión, los cuatro segmentos, la tabla de equipos, los márgenes y el riesgo vienen de documentos de NASA (ver `REFERENCES.md`).

**Inventado (números):** masa, consumo y costo de cada pieza, capacidad de los cohetes, presupuestos, factores de contacto, la fórmula de ciencia y los porcentajes de riesgo. Están en `Assets/Scripts/Data/TestCatalog.cs` y `Assets/Scripts/Core/MissionCalculator.cs`.

**Decisión propuesta:** no tocar esos números. Están balanceados y las tres soluciones de referencia del README dependen de ellos. Los datos reales se suman **alrededor**: fichas de misiones, textos de tooltip, nombres, imágenes y créditos. Así sube Validity y Relevance sin riesgo de romper el juego a horas de entregar.

**Un hallazgo a favor:** varios números inventados caen cerca de la realidad, y eso se puede decir en la página del proyecto.

| En el juego | En la realidad | Fuente |
|---|---|---|
| Instrumentos que generan 20 a 60 GB/día | LRO baja unos 460 gigabits por día, es decir unos 57 GB/día | NASA, ver 4.3 |
| Capacidad a GEO = ×0,4 de la de órbita baja | Falcon 9: unos 22.800 kg a órbita baja contra unos 8.300 kg a transferencia geoestacionaria, es decir ×0,36 | Estimación de terceros, verificar en spacex.com |
| Margen de energía menor al 30% sube el riesgo | NASA Goddard pide 25% de margen de potencia y masa al final de la fase A | GOLD Rules, ver 4.6 |

---

## 3. Tareas propuestas, por prioridad

Riesgo: **bajo** = solo textos o datos nuevos; **medio** = toca UI o escenas; **alto** = toca fórmulas.

### P1. Fichas de misiones reales en Resultados (riesgo bajo-medio)

Hoy Resultados muestra el nombre de la misión parecida y una línea (`MissionOutcome.SimilarNote`). La propuesta es una ficha con datos reales al lado de los del jugador.

- Crear un catálogo `RealMissions` (clase estática nueva en `Assets/Scripts/Data/`) con: nombre, agencia, año, masa, órbita, instrumento principal, dato curioso y fuente.
- En Resultados, mostrar "Your satellite: 365 kg, low orbit" contra "Landsat 9: 2,711 kg, 705 km".
- Elegir la misión según el diseño, no solo según el contrato (ver P2).
- Archivos: `Assets/Scripts/Results/MissionOutcome.cs`, `ResultsInfo.cs`, `ResultsController.cs`, `Editor/ResultsSceneBuilder.cs`, y `Assets/Scripts/Hangar/MissionHandoff.cs` (pasa `similarMission`).

### P2. Misión argentina según lo que arme el jugador (riesgo bajo)

- Contrato Tierra **con radar** → SAOCOM 1A/1B (CONAE).
- Contrato Tierra sin radar → Landsat 9 (NASA/USGS).
- Mención de SAC-D/Aquarius como ejemplo de cooperación NASA–CONAE (en créditos o en la ficha).
- Es el gancho más fuerte para un jurado en Buenos Aires y suma agencia socia.

### P3. Texturas de NASA (riesgo bajo)

- Tierra: reemplazar `2k_earth_daymap.jpg` (Solar System Scope) por **Blue Marble Next Generation** de NASA.
- Luna: mapa de color del **CGI Moon Kit** de NASA SVS, hecho con datos de LRO.
- Mantener 2k por el peso en WebGL. Mismo nombre de archivo o actualizar la referencia en `ResultsSceneBuilder` y donde el Hub las use.
- Actualizar `REFERENCES.md`: con esto las imágenes de planetas pasan a ser recurso de NASA.

### P4. Estaciones terrenas con nombre real (riesgo bajo-medio)

Hoy son "1 a 4 estaciones". Propuesta: darles nombre e ícono en `Assets/Scripts/Hub/StationsPanel.cs`, sin cambiar el costo ni el factor.

1. Córdoba, Argentina (CONAE, Centro Espacial Teófilo Tabanera).
2. Tolhuin, Tierra del Fuego (CONAE).
3. White Sands, Estados Unidos (NASA, la estación que usa LRO; verificar).
4. Malargüe, Mendoza (ESA, antena de espacio profundo de 35 m; CONAE usa el 10% del tiempo).

### P5. "Real-world equivalent" en los tooltips (riesgo bajo)

Una línea extra en la descripción de cada pieza en `TestCatalog.cs`. No cambia números.

| Pieza | Texto propuesto (en inglés, verificar antes) |
|---|---|
| Advanced camera | "Like Landsat 9's OLI-2, which images Earth in 9 colors of light." |
| Radar | "Like the L-band radar on Argentina's SAOCOM satellites." |
| Spectrometer | "Like the instruments that found water ice signs on the Moon." (buscar instrumento concreto) |
| Low orbit | "Landsat 9 flies at 705 km. The ISS at about 400 km." |
| Geostationary | "35,786 km up: one lap takes exactly one day." |
| Lunar orbit | "The Moon is about 384,400 km away. LRO orbits it at about 50 km." |
| Small rocket | "Similar class: Rocket Lab Electron (about 300 kg to low orbit)." |
| Large rocket | "Similar class: Falcon 9 (about 22,800 kg to low orbit)." |

### P6. Margen según la regla real de NASA (riesgo bajo si es solo texto)

- Texto: cuando el margen de energía o masa es bajo, CAPCOM dice "NASA Goddard asks for 25% spare power and mass this early in a design."
- Opcional y con riesgo alto: cambiar el umbral de 30% a 25% en `MissionCalculator`. No recomendado hoy.

### P7. Evento real como referencia (riesgo bajo)

En la tarjeta de tormenta solar, una línea: "In February 2022 a geomagnetic storm brought down 38 of 49 newly launched Starlink satellites."
Aclaración honesta: en ese caso real el daño fue por mayor fricción con la atmósfera, no por apagar un instrumento. Sirve como ejemplo de que el clima espacial afecta misiones, no como copia del mecanismo del juego.

### P8. Entrega y presentación (sin código)

- Definir **nombre del juego y del equipo** (siguen pendientes en el README).
- Abrir el repo o dar acceso a los jueces.
- Subir la build WebGL a itch.io y probarla en ventana privada.
- Sección "Use of AI" en el README y en los créditos del juego (`Assets/Scripts/Core/ShellScreens.cs`, línea "BASED ON").
- Actualizar `REFERENCES.md` con todo lo nuevo.

---

## 4. Datos investigados

Confianza: **Alta** = dato de una página de NASA o de la agencia, confirmado en esta búsqueda. **Media** = coinciden dos fuentes secundarias. **Baja** = hay fuentes que no coinciden o no se pudo confirmar; verificar antes de usar.

### 4.1 Landsat 9 (NASA / USGS) — contrato "Observe the Earth"

| Dato | Valor | Confianza |
|---|---|---|
| Lanzamiento | 27 de septiembre de 2021 | Media (verificar fecha exacta) |
| Masa al lanzar | 2.711 kg (2.710 según la ficha de NASA) | Alta |
| Órbita | 705 km, casi polar, heliosincrónica, inclinación 98,2° | Alta |
| Vuelta a la Tierra | cada 99 minutos | Media |
| Repite cobertura | cada 16 días | Alta |
| Instrumentos | OLI-2 (9 bandas) y TIRS-2 (2 bandas térmicas) | Alta |
| Imágenes | más de 700 escenas por día (hasta 740) | Alta |
| Potencia | 4.300 W (no queda claro si es generación) | Baja |

Fuentes: https://science.nasa.gov/mission/landsat-9/landsat-9-mission-details · https://www.nasa.gov/wp-content/uploads/2015/04/landsat_9_fast_facts.pdf · https://landsat.gsfc.nasa.gov/satellites/landsat-9/landsat-9-instruments

### 4.2 Hubble (NASA / ESA) — contrato "Space Telescope"

| Dato | Valor | Confianza |
|---|---|---|
| Lanzamiento | abril de 1990 | Alta |
| Masa al lanzar | unos 10.800 kg según NASA; 11.110 kg según ESA | Media (dar "about 11,000 kg") |
| Altitud | unos 540 km (órbita baja circular) | Alta |
| Paneles | unos 5.500 W al sol; uso promedio de unos 2.100 W | Alta |
| Datos | unos 140 gigabits de ciencia por semana (unos 17,5 GB) | Alta |
| Espejo | 2,4 m | Alta (verificar en la fuente) |

Dato útil para el juego: Hubble genera mucha energía de sobra (5.500 W contra 2.100 W de uso), un ejemplo real de margen.

Fuentes: https://science.nasa.gov/centers-and-facilities/goddard/a-nasa-refresher-on-hubble-facts-for-the-25th-anniversary · https://esahubble.org/about/general/fact_sheet/

### 4.3 Lunar Reconnaissance Orbiter (NASA) — contrato "Explore the Moon"

| Dato | Valor | Confianza |
|---|---|---|
| Lanzamiento | junio de 2009 | Alta |
| Masa al lanzar | 1.916 kg es la cifra más citada; documentos previos al lanzamiento dan 1.823 kg | Baja (dar "about 1,900 kg" y verificar) |
| Combustible | cerca de la mitad de la masa (898 kg de 1.823) | Media |
| Órbita | polar, unos 50 km sobre la Luna | Alta |
| Potencia | unos 680 a 820 W de promedio según el documento | Baja |
| Datos | unos 460 gigabits por día (unos 57 GB/día), a 100 Mbps en banda Ka | Media |
| Estación en Tierra | White Sands, Nuevo México | Baja (no confirmado en esta búsqueda) |

Dato útil para el juego: casi la mitad del peso de LRO era combustible. Explica por qué en el juego el cohete levanta mucho menos hasta la Luna.

Fuentes: https://science.nasa.gov/mission/lro/spacecraft-and-instruments/ · https://eoportal.org/satellite-missions/lro

### 4.4 SAOCOM 1A y 1B (CONAE, Argentina)

| Dato | Valor | Confianza |
|---|---|---|
| Lanzamientos | 1A: octubre de 2018. 1B: 30 de agosto de 2020. Los dos en Falcon 9 | Alta |
| Masa al lanzar | unos 3.000 kg (3.050 kg en una fuente; otra da 1.600 kg, que sería masa seca) | Media |
| Órbita | 620 km, heliosincrónica, inclinación 97,9° | Alta |
| Instrumento | radar de apertura sintética en banda L (1.275 MHz) | Alta |
| Revisita | 16 días con un satélite, 8 con los dos | Alta |
| Para qué sirve | mapas de humedad del suelo, agricultura, inundaciones y emergencias | Alta |

Fuentes: https://www.eoportal.org/satellite-missions/saocom · https://en.wikipedia.org/wiki/SAOCOM

### 4.5 SAC-D / Aquarius (CONAE + NASA)

| Dato | Valor | Confianza |
|---|---|---|
| Lanzamiento | 10 de junio de 2011, desde Vandenberg | Alta |
| Masa al lanzar | 1.515 kg (dos fuentes); otra da 1.350 kg | Media |
| Órbita | 657 km, heliosincrónica | Alta |
| Qué hizo | satélite argentino con el instrumento Aquarius de NASA: midió la salinidad del mar desde el espacio | Alta |
| Fin de misión | junio de 2015 | Alta |

Fuentes: https://eoportal.org/satellite-missions/sac-d · https://www.jpl.nasa.gov/news/press_kits/aquariusLaunch.pdf

### 4.6 Márgenes: la regla real de NASA Goddard

- Las *GOLD Rules* (GSFC-STD-1000), sección 1.06, fijan márgenes mínimos de recursos.
- Una presentación de un ingeniero de sistemas de Goddard resume: margen de potencia y masa seca de **25% o más al final de la fase A, 20% al final de la fase B y 15% al final de la fase C**.
- Fórmula que da NASA: `margen = (recurso disponible − uso estimado) / recurso disponible`. Es la misma que usa `MissionCalculator`.
- Confianza: Media. Los valores exactos están en las tablas 1.06-1 y 1.06-2 del estándar; conviene abrir el PDF.

Fuentes: https://standards.nasa.gov/sites/default/files/standards/GSFC/G/0/GSFC-STD-1000Revg_signature.pdf · https://ndia.dtic.mil/wp-content/uploads/2014/system/16863WedTrack3DiPietro.pdf

### 4.7 Estaciones terrenas

| Estación | Datos | Confianza |
|---|---|---|
| Córdoba (CONAE) | Estación Terrena Córdoba, en el Centro Espacial Teófilo Tabanera, Falda del Cañete. Cubre Sudamérica. Ahí está el control de misión del SAOCOM | Media |
| Tolhuin, Tierra del Fuego (CONAE) | Dos antenas, de 13,5 m y 7,3 m, operadas a distancia desde Córdoba | Media |
| Malargüe, Mendoza (ESA, DSA 3) | Antena de 35 m para espacio profundo, inaugurada el 18/12/2012. CONAE coordina el 10% del tiempo de antena | Alta |
| White Sands (NASA) | Estación de LRO | Baja |

Fuentes: https://www.esa.int/Enabling_Support/Operations/ESA_Ground_Stations/Malarguee_-_DSA_3 · https://www.argentina.gob.ar/sites/default/files/2018/07/triptico_conae.pdf (los datos de CONAE salen de comunicados oficiales; verificar en conae.gov.ar)

### 4.8 Cohetes de referencia

Son estimaciones de terceros, no precios oficiales. Usarlos solo como "similar class", nunca como dato duro.

| Cohete | A órbita baja | Otros | Confianza |
|---|---|---|---|
| Rocket Lab Electron | unos 300 kg | unos 225 kg a heliosincrónica; unos 6 a 7,5 millones de dólares | Baja |
| Vega-C | unos 2.300 kg | unos 2.350 kg a heliosincrónica | Baja |
| Falcon 9 | unos 22.800 kg | unos 8.300 kg a transferencia geoestacionaria (no confirmado en esta búsqueda) | Baja |

### 4.9 Clima espacial

- El 3 de febrero de 2022 se lanzaron 49 satélites Starlink; una tormenta geomagnética "menor" calentó la atmósfera alta, aumentó la fricción hasta un 50% y 38 reentraron en pocos días.
- Confianza: Alta (hay una página de NASA SVS y un estudio de NOAA/CIRES).

Fuentes: https://svs.gsfc.nasa.gov/hyperwall/index/data/a000000/a005100/a005193/cap5193.txt.html · https://cires.colorado.edu/news/minor-geomagnetic-storm-big-impact-february-2022-starlink-satellite-loss

### 4.10 Imágenes de NASA

| Recurso | Uso | Licencia | Link |
|---|---|---|---|
| Blue Marble Next Generation | Textura de la Tierra | Material de NASA, sin copyright salvo indicación. Piden el crédito "NASA Earth Observatory" | https://science.nasa.gov/earth/earth-observatory/blue-marble-next-generation/ |
| CGI Moon Kit (NASA SVS) | Textura de la Luna, con datos de LRO | Material de NASA; verificar la página | https://svs.gsfc.nasa.gov/4720 (link no confirmado en esta búsqueda) |

Política de uso: https://www.visibleearth.nasa.gov/image-use-policy

### 4.11 Lo que no se pudo confirmar

- Masa y consumo de los instrumentos por separado (OLI-2, TIRS-2, cámara de LRO).
- Costo total de cada misión. Por eso los presupuestos del juego quedan como ficticios.
- Volumen diario de datos de Landsat 9 en GB.
- Un caso de una nave de NASA que apagó instrumentos por una tormenta solar.
- Capacidad de un cohete real hasta la Luna, para comparar con el ×0,3 del juego.

---

## 5. Textos listos para el juego (en inglés)

Borradores para iterar. Cada número sale de la sección 4.

**Fichas de misión real**

- **Landsat 9** (NASA / USGS, 2021): "2,711 kg, 705 km up. It photographs all of Earth's land every 16 days, more than 700 scenes a day."
- **SAOCOM 1A & 1B** (CONAE, Argentina, 2018 and 2020): "Two radar satellites at 620 km. They map soil moisture to help farmers and warn about floods."
- **Hubble** (NASA / ESA, 1990): "About 11,000 kg at 540 km. Its panels make 5,500 W and it uses about 2,100: that spare power is margin."
- **Lunar Reconnaissance Orbiter** (NASA, 2009): "About 1,900 kg, and almost half of that was fuel. It maps the Moon from just 50 km up."
- **SAC-D / Aquarius** (CONAE + NASA, 2011): "An Argentine satellite carrying a NASA instrument that measured how salty the oceans are."

**CAPCOM**

- Margen bajo: "NASA Goddard asks for 25% spare power and mass this early. You have {x}%."
- Datos que no bajan: "LRO sends home about 57 GB a day. Your antenna can't keep up with your instruments."
- Radar en la Tierra: "Radar sees through clouds and at night. Argentina's SAOCOM does exactly that."

---

## 6. Video demo de 30 segundos

Grabar una partida real, acelerar los tramos lentos y poner texto grande en pantalla (muchos jueces lo ven sin sonido). Audio o subtítulos en inglés.

| Tiempo | Qué se ve | Qué se dice |
|---|---|---|
| 0:00–0:03 | Título del juego sobre el centro espacial | "We're [team]. This is [game]." |
| 0:03–0:07 | Tarjeta de contrato | "Design a real space mission with limited money, mass and power." |
| 0:07–0:15 | Hangar: se agrega una pieza y las barras pasan a rojo | "Every part is a trade-off. A better camera means more weight and more power." |
| 0:15–0:22 | Lanzamiento: despegue, separación, evento | "Then you launch, and space fights back." |
| 0:22–0:27 | Resultados: falla, el porqué y la alternativa | "You don't just fail. You learn why, and what to change." |
| 0:27–0:30 | Ficha de la misión real (SAOCOM o Landsat) | "Built on how NASA designs missions. Play it in your browser." |

Alternativa si no llegamos con el video: 7 slides con el mismo recorrido, una captura por slide.

---

## 7. Cómo venderlo

- **La frase:** "It's not a flight simulator. It's the real job of a mission engineer, in five minutes."
- **Impact:** corre en el navegador, sin instalar nada; sirve para un aula.
- **Creativity:** el juego no dice solo "fallaste": explica por qué y muestra qué habría pasado con otra elección.
- **Validity:** el ciclo sale de documentos de NASA y la fórmula de margen es la misma que da NASA. Con P1 a P6, los resultados se comparan con misiones reales.
- **Relevance:** cubre todo lo que pide el challenge (objetivo, nave, instrumentos, cohete, órbita, recursos, simulación, feedback).
- **Gancho local:** terminar una partida con "your satellite looks like Argentina's SAOCOM".

---

## 8. Orden sugerido para hoy

1. P8: nombre del juego y del equipo, repo público, build en itch.io. Sin esto no hay entrega.
2. P1 + P2: fichas de misiones reales con SAOCOM. Es lo que más sube la nota por hora de trabajo.
3. P3: texturas de NASA.
4. Video de 30 segundos (después de P1 y P3, así salen en la grabación).
5. P5 y P6: textos de tooltip y de CAPCOM.
6. P4 y P7 solo si sobra tiempo.
7. Completar la página del proyecto y `REFERENCES.md`. Enviar antes de las 23:59.

---

## 9. Preguntas abiertas para decidir entre los dos

1. ¿Nombre del juego y del equipo?
2. ¿Video o slides?
3. ¿La misión real se elige solo por contrato, o también por las piezas (radar → SAOCOM)?
4. ¿Mostramos los números reales como comparación directa con el diseño del jugador, o solo como ficha informativa? La comparación directa puede confundir, porque nuestros satélites pesan 300 a 500 kg y Landsat 9 pesa 2.711 kg.
5. ¿Nombramos las estaciones terrenas o lo dejamos como está?
6. ¿Quién arma el video y quién completa la página del proyecto?

---

## 10. Reglas para no romper nada

- No cambiar valores ni fórmulas en `TestCatalog.cs` y `MissionCalculator.cs`, salvo decisión explícita de los dos.
- Textos del juego en inglés, simples, sin jerga sin explicar.
- Nada de logos ni insignias de NASA, ni de KSP.
- WebGL: texturas de 2k como máximo, sin hilos, sin `System.IO` en runtime.
- Cada recurso nuevo se anota en `REFERENCES.md` con link y licencia.
- Respetar el reparto de escenas del README (sección 9): avisar antes de tocar un archivo del otro.
