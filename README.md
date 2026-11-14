# Space Mission Design Game

Juego 3D en Unity para el **NASA Space Apps Challenge 2026**, challenge **"Space Mission Design Game"**.

El jugador planifica una misión espacial en un centro espacial, arma un satélite pieza por pieza con recursos limitados, lo lanza y ve por qué la misión funcionó o falló. Se inspira en *Kerbal Space Program*, pero **sin pilotaje ni física real**: lo central son las decisiones de diseño y sus consecuencias.

> Este README es la fuente única de verdad del proyecto. Está escrito para personas **y** para asistentes de IA (Claude Code, etc.). Si algo cambia, se actualiza acá.

---

## Índice

1. [Datos clave y fechas](#1-datos-clave-y-fechas)
2. [El challenge de la NASA](#2-el-challenge-de-la-nasa)
3. [Diseño del juego](#3-diseño-del-juego)
4. [Modelo de datos y fórmulas](#4-modelo-de-datos-y-fórmulas)
5. [Valor educativo](#5-valor-educativo)
6. [Alcance y prioridades](#6-alcance-y-prioridades)
7. [Estilo visual](#7-estilo-visual)
8. [Stack técnico y arquitectura](#8-stack-técnico-y-arquitectura)
9. [Forma de trabajo y Git](#9-forma-de-trabajo-y-git)
10. [Assets y licencias](#10-assets-y-licencias)
11. [Build y entrega](#11-build-y-entrega)
12. [Contexto para asistentes de IA](#12-contexto-para-asistentes-de-ia)
13. [Pendientes y decisiones abiertas](#13-pendientes-y-decisiones-abiertas)

---

## 1. Datos clave y fechas

| Qué | Dato |
|---|---|
| Evento | NASA Space Apps Challenge 2026 — hackathon de 48 h |
| Fechas | Sábado 14 y domingo 15 de noviembre de 2026 |
| **Cierre de entrega en la plataforma** | **Domingo 15/11, 23:59 hora argentina** |
| Local Event | Buenos Aires (Universidad de Palermo, híbrido) |
| Presentaciones al jurado (evento BA) | Domingo 15/11 desde las **15:00** (confirmar con emprende@palermo.edu) |
| Premiación local | Jueves 19/11, 17:00, presencial |
| Equipo | 2 personas |
| Motor | **Unity 6.6 — 6000.6.9f1** (los dos con la misma versión exacta) |
| Repo | GitHub privado `eugenio-navarro/SpaceMissionGame` (Git + LFS) |

**Regla de oro de tiempos:** el juego tiene que estar jugable y subido **antes de las 15:00 del domingo**. Lo que se agregue después es bonus.

---

## 2. El challenge de la NASA

### Qué pide (resumen)

Crear un **juego interactivo** donde el jugador **diseñe, gestione y simule una misión espacial completa**, tome decisiones de ingeniería, administre **recursos limitados** y vea cómo cada decisión afecta el éxito de la misión.

Idea central: **no hay una solución perfecta**. Un instrumento mejor da más ciencia, pero también suma masa, energía, costo y datos.

### Requisitos centrales (los cubrimos sí o sí)

- Definir el **objetivo** de la misión y sus **requisitos**.
- Diseñar la **nave** y elegir **instrumentos**.
- Elegir **lanzador** y **órbita**, con chequeo de compatibilidad masa/órbita/cohete.
- Administrar recursos: **presupuesto, masa, energía, datos, comunicaciones, vida útil**.
- **Simular** la misión y **evaluarla**.
- Dar **feedback que explique por qué** salió bien o mal, no solo "correcto o incorrecto".

### Opcionales que menciona la NASA ("may", "could")

Modo equipo de hasta 4, roles, dificultades, escenarios múltiples, eventos aleatorios, ranking, reporte final, sandbox, guardar y comparar diseños, análisis post-misión.

### Base real usada (fuentes NASA)

- **Design and Develop Science Missions (NASA):** fases de una misión (*necesidad → objetivos → requisitos → diseño → construcción y verificación → operación*) y los **4 segmentos**:
  - **Lanzamiento:** el cohete.
  - **Espacial:** la nave y los instrumentos.
  - **Terrestre:** las antenas y el control en Tierra.
  - **Usuario:** los científicos que usan los datos.
  - Principio clave: **"empezar con el final en mente"**.
- **Mission Design Lab (NASA Goddard):** *trade studies* (comparar alternativas), **MEL** (lista de equipos con masa, potencia y costo), **margen y riesgo**.
- **Build It Yourself: Satellite! (juego NASA/Webb):** opciones en gris cuando son incompatibles, niveles de dificultad y comparación del resultado con **misiones reales**.

### Requisitos de la entrega

- Todo en **inglés** (texto de la entrega y materiales). Si el video está en español, necesita **subtítulos en inglés**.
- Todos los miembros registrados en el equipo **antes** del cierre.
- Listar **todos los recursos externos** en *References* (ver `REFERENCES.md`).
- Declarar que el trabajo es original.

---

## 3. Diseño del juego

### Concepto

Juego **3D** con estética y estructura inspiradas en *Kerbal Space Program*: centro espacial → hangar → lanzamiento → resultados. El lanzamiento es **automático y animado**: el jugador no pilotea.

### Flujo general

```
CENTRO ESPACIAL (menú principal, vista aérea 3D)
 ├─ [1] Control de misión ──► elegir contrato (nivel)
 ├─ [2] Hangar ─────────────► ARMADO del satélite + cohete + órbita
 ├─ [3] Antenas ────────────► estaciones en Tierra + vista de mapa
 └─ [4] Cúpula ─────────────► archivo de misiones, resultados, desbloqueos
            │
            ▼  (botón LANZAR en el hangar)
      LANZAMIENTO (automático) ──► RESULTADOS ──► vuelve al CENTRO ESPACIAL
```

### Las 4 etapas

| # | Etapa | Escena | Qué hace el jugador | Base NASA |
|---|---|---|---|---|
| 1 | **Misión** | `CentroEspacial` | Elige un contrato: objetivo, meta de ciencia, presupuesto y recompensa | Necesidad, objetivos y requisitos |
| 2 | **Armado** | `Hangar` | Elige piezas en 4 pestañas y ve el HUD de recursos en vivo. El botón Lanzar se bloquea si algo no cierra | Segmentos espacial, de lanzamiento y terrestre; verificación (MEL, riesgo) |
| 3 | **Lanzamiento** | `Lanzamiento` | Mira el despegue, la separación de etapas, la llegada a órbita, la vista de mapa y un evento aleatorio | Operación |
| 4 | **Resultados** | `Resultados` | Ve el puntaje, la explicación de por qué salió así, una sugerencia de qué probar y una misión real parecida | Segmento de usuario y trade study |

### Pestañas del hangar (= roles en modo equipo)

| Pestaña | Rol | Qué se elige |
|---|---|---|
| Instrumentos | Científico | Hasta 3 instrumentos |
| Energía | Ingeniero | 1 panel solar y batería opcional |
| Cohete y órbita | Lanzamiento | 1 cohete y 1 órbita |
| Comunicaciones | Comunicaciones | 1 antena y de 1 a 4 estaciones en Tierra |

> Las estaciones en Tierra se eligen en la pestaña Comunicaciones. El edificio de antenas del centro espacial abre ese mismo panel más la vista de mapa (opcional si sobra tiempo).

### Modalidades

- **Campaña:** 3 contratos/niveles en orden (Tierra → Telescopio → Luna). Al pasar uno se desbloquean piezas.
- **Equipo:** de 2 a 4 jugadores por turnos en la misma PC. Cada pestaña del hangar es un rol y todos comparten presupuesto. **No es online.**
- **Libre:** sin límite de presupuesto, para experimentar.

### Contratos (escenarios)

| Nivel | Contrato | Objetivo | Meta de ciencia | Presupuesto | Órbitas permitidas | Instrumentos bloqueados | Misión real parecida |
|---|---|---|---|---|---|---|---|
| 1 | Observar la Tierra | Medir cambios en bosques y hielo | 60 | $300M | Baja, Geoestacionaria | — | Landsat |
| 2 | Telescopio espacial | Estudiar galaxias lejanas | 70 | $400M | Baja, Geoestacionaria | Radar | Hubble |
| 3 | Explorar la Luna | Mapear el polo sur lunar | 40 | $500M | Lunar | — | Lunar Reconnaissance Orbiter |

### Eventos de la simulación

Se sortea uno por lanzamiento:

- **Tormenta solar:** si el margen de energía es menor al 20%, se apaga el instrumento que más consume. Si no, el margen lo protege.
- **Interferencia en estación:** si hay menos de 3 estaciones, se baja un 20% menos de datos. Si hay 3 o más, las otras cubren.
- **Calma:** sin imprevistos.

Después se tira el **dado de riesgo** (ver fórmulas). Si sale falla, se pierde el 50% de la ciencia.

---

## 4. Modelo de datos y fórmulas

> Los valores son **inventados pero coherentes**, pensados para que haya trade-offs reales. Se cargan como **ScriptableObjects** y se pueden ajustar sin tocar código. Ya están probados en un prototipo web.

### Base fija de la nave (estructura + computadora)

Masa 150 kg · consumo 40 W · costo $30M

### Instrumentos (máx. 3)

| ID | Nombre | Masa (kg) | Consumo (W) | Costo ($M) | Datos (GB/día) | Descripción corta |
|---|---|---|---|---|---|---|
| `cam_b` | Cámara básica | 60 | 30 | 20 | 20 | Fotos simples. Barata y liviana. |
| `cam_a` | Cámara avanzada | 120 | 80 | 50 | 60 | Mucho detalle, pesada y gastadora. |
| `espec` | Espectrómetro | 90 | 60 | 40 | 40 | Analiza la luz para saber de qué están hechas las cosas. |
| `radar` | Radar | 150 | 120 | 60 | 50 | Ve a través de nubes y de noche. Muy pesado. |

### Energía

| ID | Nombre | Masa (kg) | Genera (W) | Costo ($M) |
|---|---|---|---|---|
| `panel_s` | Panel solar chico | 20 | 120 | 10 |
| `panel_l` | Panel solar grande | 40 | 250 | 20 |
| `bat` | Batería | 30 | — | 8 |

### Antenas

| ID | Nombre | Masa (kg) | Consumo (W) | Costo ($M) | Bajada máx. (GB/día) |
|---|---|---|---|---|---|
| `ant_s` | Antena chica | 10 | 15 | 5 | 40 |
| `ant_l` | Antena grande | 25 | 40 | 15 | 100 |

Estaciones en Tierra: de 1 a 4, a **$10M cada una**. Factor de contacto por cantidad: `[0.5, 0.8, 1.0, 1.15]`.

### Cohetes

| ID | Nombre | Capacidad a órbita baja (kg) | Costo ($M) |
|---|---|---|---|
| `chico` | Cohete chico | 500 | 60 |
| `mediano` | Cohete mediano | 1200 | 110 |
| `grande` | Cohete grande | 3000 | 180 |

### Órbitas

| ID | Nombre | Altura | Contacto | Vida útil | Multiplicador de capacidad del cohete | Sombra (necesita batería) |
|---|---|---|---|---|---|---|
| `leo` | Órbita baja | ~500 km | 0.5 | 3 años | ×1.0 | Sí |
| `geo` | Geoestacionaria | ~36.000 km | 1.0 | 8 años | ×0.4 | No |
| `lunar` | Órbita lunar | ~384.000 km | 0.6 | 3 años | ×0.3 | Sí |

**Factor de detalle por contrato y órbita:** Tierra `{leo:1.5, geo:0.8}` · Telescopio `{leo:0.9, geo:1.0}` · Luna `{lunar:1.2}`.

### Fórmulas

```
masa        = 150 + Σ masa de piezas
consumo     = 40 + Σ consumo (instrumentos + antena)
costo       = 30 + Σ costo de piezas + costo cohete + 10 × estaciones
generación  = panel.gen × (órbita con sombra y sin batería ? 0.65 : 1)
capacidad   = cohete.cap × órbita.capMul
datosGen    = Σ datos de instrumentos
bajada      = antena.dl × órbita.contact × SF[estaciones-1]
recibidos   = min(datosGen, bajada)
ciencia     = round(recibidos × detalle[contrato][órbita])

margenEnergía = (generación - consumo) / generación
margenMasa    = (capacidad - masa) / capacidad
riesgo (%)    = 5 + (margenEnergía < 0.10 ? 25 : margenEnergía < 0.30 ? 10 : 0)
                  + (margenMasa < 0.10 ? 10 : 0)
```

**Bloquean el lanzamiento:** falta instrumento, panel, antena, cohete u órbita; `masa > capacidad`; `consumo > generación`; `costo > presupuesto` (salvo modo Libre).

**Advertencias, sin bloquear:** sin batería en una órbita con sombra; `datosGen > bajada`; márgenes menores al 10%; ciencia estimada por debajo de la meta.

**Resultado:** Éxito si `ciencia ≥ meta`; Parcial si es ≥ 60% de la meta; Fallida si es menos.

### Soluciones de referencia (para testear el balance)

- **Tierra:** cámara avanzada + panel grande + batería + antena grande + 2 estaciones + cohete chico + órbita baja → 365 kg / 500 kg, margen de energía 36%, $203M, **ciencia 60** ✅
- **Telescopio:** cámara avanzada + espectrómetro + panel grande + antena grande + 2 estaciones + cohete mediano + GEO → 425 kg / 480 kg, margen de energía 12% (riesgo alto), $285M, **ciencia 80** ✅
- **Luna:** cámara avanzada + panel grande + batería + antena grande + 2 estaciones + cohete grande + órbita lunar → 365 kg / 900 kg, **ciencia 58** ✅. Con el cohete mediano **no despega** (capacidad 360 kg): es una buena lección.

---

## 5. Valor educativo

El challenge pide explicar en simple, presentar los conceptos jugando y enseñar **por qué** una decisión afecta a la misión.

| Concepto | Explicación para el jugador | Dónde | Indicador visual |
|---|---|---|---|
| Altitud orbital | Qué tan alto vuela. Más bajo ve más detalle, pero dura menos y la antena lo ve menos tiempo. | Cohete y órbita | Dibujo Tierra–satélite |
| Capacidad de carga | Cuánto peso sube el cohete. Si la nave pesa más, no despega. | Cohete y órbita | Línea roja de límite en la barra de masa |
| Consumo de energía | Los instrumentos gastan y los paneles generan. Si gastás más, algo se apaga. | Energía | Barra de energía y +/− en cada pieza |
| Tasa de datos | Cuánta información genera la nave por día. | Instrumentos | Contador GB/día |
| Comunicaciones | Cuánto puede bajar la antena. Lo que no baja, se pierde. | Comunicaciones | Barras de datos generados vs. enviados |
| Margen y riesgo | Dejar de sobra protege de imprevistos. | Revisión y lanzamiento | Barra en verde, amarillo o rojo, y riesgo en % |

**Herramientas didácticas:**

- **CAPCOM:** personaje propio de control de misión que explica cada concepto en una línea.
- **Tooltips "?"** en cada pieza.
- **Tutorial guiado** en la primera partida.
- **Vista previa del impacto:** al pasar el mouse sobre una pieza, las barras muestran cuánto cambiarían.

**Feedback:** nunca "Misión fallida" a secas. Ejemplo correcto:

> "Misión parcial: tus instrumentos generaban 120 GB/día, pero la antena solo bajaba 80. Se perdió un 33% de la ciencia. Probá agregar una estación terrestre o un instrumento que genere menos datos."

---

## 6. Alcance y prioridades

### Imprescindible (MVP)

- [ ] 4 escenas conectadas: CentroEspacial → Hangar → Lanzamiento → Resultados → CentroEspacial
- [ ] Al menos 1 contrato completo (Observar la Tierra)
- [ ] Hangar con las 4 pestañas y el HUD de recursos en vivo
- [ ] Validaciones y botón Lanzar bloqueado con mensajes claros
- [ ] Lanzamiento animado (Timeline + Cinemachine) y 1 evento
- [ ] Resultados con puntaje y explicación del porqué
- [ ] Textos CAPCOM y tooltips simples
- [ ] Build WebGL subida y funcionando

### Si llegamos

- [ ] Los 3 contratos y el desbloqueo de piezas
- [ ] Riesgo con dado y los 3 eventos
- [ ] Vista de mapa con la órbita dibujada
- [ ] Tabla de equipos (MEL) antes de lanzar
- [ ] Trade study ("con otra elección habrías…")
- [ ] Vista previa del impacto al pasar el mouse

### Bonus

- [ ] Modo equipo por turnos (de 2 a 4)
- [ ] Modo Libre
- [ ] Ranking local (PlayerPrefs)
- [ ] Misión real parecida en los resultados
- [ ] Tutorial guiado
- [ ] Mini desafío de separación de etapas (apretar espacio en el momento justo)

### Fuera de alcance

Pilotaje, física orbital real, cálculo de combustible, aterrizajes, acoplamientos, caminatas espaciales, rovers, multiplayer online, guardado en la nube.

---

## 7. Estilo visual

Referencia: `Moodboard.pdf` (en la raíz del repo), basado en capturas de KSP. Resumen:

- **Centro espacial:** vista aérea 3D, edificios clickeables con nombre al pasar el mouse.
- **Hangar:** catálogo a la izquierda (nombre, masa, costo), nave en el centro con cámara orbital, HUD a la derecha y presupuesto abajo a la izquierda.
- **Lanzamiento:** humo, cámara que sigue al cohete, altímetro tipo odómetro arriba, reloj de misión (T+), lista de etapas que se apagan, llegada a órbita con horizonte curvo y vista de mapa con la órbita dibujada.
- **Resultados:** toma cinematográfica de la nave sobre el planeta, con el panel de resultados semitransparente encima.
- **Paleta:** negro espacial `#0B0F18`, metal `#8F98A3`, naranja de etapas y alertas `#F0782A`, verde fósforo de UI `#7BD88F`, azul planeta `#3F77C9`.
- **UI:** paneles grises biselados con texto verde fósforo. El naranja se reserva para lo importante.
- **Tono:** con humor, mediante un personaje propio.

⚠️ **No usar modelos, texturas, personajes (Kerbals) ni logos de KSP.** Solo es inspiración.

---

## 8. Stack técnico y arquitectura

### Stack

| Qué | Elección |
|---|---|
| Motor | Unity **6000.6.9f1** |
| Render | **URP** (plantilla Universal 3D) |
| Plataforma | **WebGL**, que es la principal para jueces; Windows como respaldo |
| Cámaras | **Cinemachine** |
| Secuencias | **Timeline** |
| UI | **uGUI + TextMeshPro** |
| Datos | **ScriptableObjects** |
| Tweens (opcional) | DOTween (gratis, Asset Store) |
| IA de desarrollo | Claude Code + plugin oficial de Unity (Unity CLI activada) |

### Estructura de carpetas

```
Assets/
  Scenes/        CentroEspacial, Hangar, Lanzamiento, Resultados
  Scripts/
    Core/        MissionState, SceneLoader, MissionCalculator
    Data/        clases ScriptableObject (PartData, RocketData, OrbitData, ContractData)
    Hangar/      UI de pestañas, HUD, validaciones
    Launch/      control de la secuencia y eventos
    Results/     pantalla de resultados
    Hub/         interacción con edificios
  Data/          assets .asset de piezas, cohetes, órbitas y contratos
  Prefabs/
  Art/           modelos, materiales y texturas (Git LFS)
  UI/
README.md
REFERENCES.md
```

### Arquitectura

- **`MissionState`** (singleton, `DontDestroyOnLoad`): guarda el contrato, la modalidad, las piezas elegidas, el cohete, la órbita, las estaciones y el resultado. Lo leen todas las escenas.
- **`MissionCalculator`** (clase C# pura, sin MonoBehaviour): implementa las fórmulas de la sección 4. Es fácil de testear y no depende de la escena.
- **ScriptableObjects:**
  - `PartData`: id, nombre, categoría, masa, consumo, generación, costo, datos, bajada, descripción, tooltip CAPCOM, prefab.
  - `RocketData`, `OrbitData` y `ContractData`, con meta, presupuesto, órbitas permitidas, instrumentos bloqueados, factores de detalle y misión real parecida.
- **`SceneLoader`**: cambio de escenas por nombre.
- **Comunicación**: eventos C# (`event Action`) desde `MissionState` para que el HUD se actualice cuando cambia algo. Nada de `Update()` recalculando todo.

---

## 9. Forma de trabajo y Git

### Enfoque iterativo

El contenido detallado de cada escena **no está cerrado de antemano**: se define y ajusta **a medida que se crean las escenas**, partiendo del diseño de este README y del `Moodboard.pdf`. Las dudas se resuelven en el proceso.

- Cada uno avanza en paralelo con sus escenas.
- `MissionState` y los datos compartidos **crecen a medida que hacen falta**: quien necesita un campo nuevo lo agrega, avisa y pushea enseguida.
- Cuando algo del diseño cambia, se actualiza este README.

### Reparto por escenas (regla anti-conflictos)

**Nunca editen los dos la misma escena.**

| Persona A (Eugenio) | Persona B (compañero) |
|---|---|
| `CentroEspacial` + contratos | `Lanzamiento` (Timeline, cámaras, eventos) |
| `Hangar` (armado + HUD) | `Resultados` + vista de mapa |
| `MissionState`, `MissionCalculator`, ScriptableObjects | Búsqueda e importación de assets 3D |

- Lo compartido va en **prefabs**, no dentro de las escenas.
- Cada script o asset compartido tiene **un solo dueño** a la vez. Si el otro lo necesita tocar, avisa antes.

### Git

- Repo: GitHub privado `eugenio-navarro/SpaceMissionGame`, con **Git LFS** para modelos, texturas y audio. GitHub Free incluye 10 GiB de almacenamiento LFS.
- `.gitignore` oficial de Unity. **Siempre subir los `.meta`.**
- Configuración del editor: **Visible Meta Files** + **Asset Serialization: Force Text**.
- Ramas: `main` siempre funcionando, más ramas cortas por tarea (`feature/hangar-hud`, `feature/launch-timeline`).
- Commits chicos y frecuentes, en español o inglés pero claros.
- Integrar a `main` **cada 2 o 3 horas**.
- Antes de empezar: `git pull`. Al terminar algo que anda: commit y push.

### Setup para quien clona

```bash
git lfs install
git clone https://github.com/eugenio-navarro/SpaceMissionGame.git
```

Después: Unity Hub → *Add project from disk*, con la versión **6000.6.9f1**.

---

## 10. Assets y licencias

Detalle completo en **`REFERENCES.md`**: hay que anotar cada asset al bajarlo.

| Asset | Fuente | Licencia | Uso |
|---|---|---|---|
| Space Kit | Kenney | CC0 | Cohete y props |
| City Kit Industrial | Kenney | CC0 | Edificios del centro espacial |
| Texturas de Tierra, Luna y estrellas (2k) | Solar System Scope | CC BY 4.0 (atribución) | Planetas y skybox |
| Particle Pack | Kenney | CC0 | Humo y fuego |

**Reglas:**

- Modelos livianos (low poly) y texturas de 2k como máximo, por WebGL.
- Formatos FBX u OBJ.
- Nada de KSP.

---

## 11. Build y entrega

### Build WebGL

1. *File → Build Profiles → Web → Switch Platform*.
2. *Player Settings → Publishing Settings → Compression Format: Disabled*. Evita errores en itch.io.
3. *Build* → comprimir la carpeta en zip → subir a **itch.io** como HTML, marcando "played in the browser".
4. Hacer el primer build **temprano** y repetirlo seguido.

### Checklist de entrega (en inglés)

- [ ] Proyecto en la pestaña *Project* del equipo en Space Apps
- [ ] Todos los campos obligatorios completos
- [ ] Link a la build jugable (itch.io)
- [ ] Link al repo de GitHub (público o con acceso para los jueces)
- [ ] Video demo corto, con subtítulos en inglés si está en español
- [ ] *References* completo (copiar de `REFERENCES.md`)
- [ ] Uso de IA declarado
- [ ] Casillas de confirmación marcadas
- [ ] Entregado **antes de las 15:00 del domingo** (presentación al jurado del evento BA) y siempre antes de las 23:59

---

## 12. Contexto para asistentes de IA

> Si sos un asistente de IA trabajando en este repo, leé esto primero.

- **Objetivo:** juego educativo de diseño de misiones espaciales para NASA Space Apps 2026. Hay que entregar en menos de 48 h: **priorizá que funcione sobre que sea perfecto**.
- **Unity 6000.6.9f1, URP, WebGL.** Usá APIs de Unity 6:
  - `FindFirstObjectByType` en vez de `FindObjectOfType`.
  - El Input System nuevo es el default, así que la UI va con `InputSystemUIInputModule` en el EventSystem.
- **WebGL:** nada de hilos, nada de `System.IO` para guardar (usar `PlayerPrefs`) y cuidado con el peso de texturas y modelos.
- **Datos en ScriptableObjects** (sección 4). No hardcodees valores de piezas en scripts.
- **Lógica de cálculo en `MissionCalculator`**, una clase pura. La UI solo muestra.
- **Respetá la sección 9:** no modifiques escenas que no son de la persona que te está usando. Proponé cambios a scripts compartidos antes de hacerlos.
- **Textos del juego en inglés**, porque la entrega es en inglés. Los comentarios de código pueden ir en español.
- **Feedback siempre explicativo** (sección 5).
- **No uses assets, nombres, personajes ni logos de Kerbal Space Program.**
- Antes de agregar una dependencia nueva, verificá que funcione en WebGL y anotala en `REFERENCES.md` si es externa.
- Commits chicos con mensajes claros.

---

## 13. Pendientes y decisiones abiertas

- [ ] Nombre del equipo (opciones: *Binary Orbit*, *Austral Outpost*, *Space Compiler*…)
- [ ] Nombre del juego
- [ ] Modalidad: presencial el sábado en Palermo o 100% virtual
- [ ] Confirmar con el organizador de BA el horario exacto de entrega y presentación
- [ ] Diseño y nombre del personaje CAPCOM
- [ ] Verificar el contenido real de los packs de Kenney al descargarlos
- [ ] Subir la primera build WebGL a itch.io
