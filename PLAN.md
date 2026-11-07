# Divergence: Full Build Plan (version 2)

Project name: **Divergence** (chosen October 30, 2026). Working description: a natural selection simulation.
Project: L4 Developing and Using Models Extension (science)
Version 1 written: Thursday, October 29, 2026
Version 2 written: Friday, October 30, 2026, after a second planning interview (24 questions) that fixed the architecture and design in detail
Status: Design agreed. Nothing has been built yet.
Project folder on the user's Mac: `/Users/Yumei/Desktop/science L4`
Companion files in the same folder: `CONTEXT.md` (glossary of project terms; use those words exactly) and `DESIGN.md` (the design system: colours, type, icons, components, charts, screens, interface copy; it wins over this file on appearance)

---

## 0. How to use this file

This file holds everything needed to build the project: the original assignment, every decision and the alternatives that were turned down, the full simulation specification, the technical architecture, the hosting plan, the journal plan and a work plan.

Rules for whoever builds from this file:

1. Section 4 (decision log) is the source of truth for what the user chose. Do not reopen those decisions without asking the user.
2. Anything marked **PROPOSED** is an implementation detail that was not put to the user. It is a sensible default and can be changed. Section 22 lists the proposed defaults the user can veto. Anything marked **TUNE** is a starting number that will need adjusting by testing.
3. Use the words in `CONTEXT.md` exactly. The main ones: **Cell** (organism), **Trait** (inherited number, never "gene"), **Genome** (all Traits plus Brain weights), **Brain**, **Species**, **Generation**, **Founder**, **Food**, **Toxin**, **Temperature**, **Pressure**, **Event**, **Preset**, **Run**, **World**, **Seed**, **Player**. The World is open space with things in it. It is not a grid, so the words "tile" and "square" must not be used for anything in the World. Internal lookup helpers such as `SpatialHash` (Section 8.2) are code details and may use buckets; they do not hold World state.
4. The Journal entries are written by the user in their own words (decision Q24 of interview 1). An assistant may supply the template and prompts, but should not write the entries for them.
5. A single Claude Code agent will build the project over several work sessions, using the Unity command line and optionally an MCP server (the user said "two agents" was only a figure of speech). The plan is detailed so that work done in different sessions, with no memory between them, stays consistent. If a detail seems missing, look in Sections 6 to 17 before choosing.
6. The teacher allows coding with AI (confirmed October 30, 2026).

---

## 1. The assignment (verbatim from the user)

**L4 Developing and Using Models Extension**

1. Create a game or simulation to model Natural Selection. This needs to be a physical game or an online simulation that demonstrate how traits in a population can change over time due to environmental pressures.
2. You need to show evidence of multiple work sessions. Each time you work on your project, record your progress on your Word Document with a few sentences explaining what you completed and a screenshot or picture of any physical work.

**Project Format:** You may choose either:
- A physical game (e.g., board game, card game, role-play activity)
- An online simulation (using coding platforms, game builders, or simulation software)

**Modeling Natural Selection:** Your game/simulation must include these key elements:
- Variation in traits within a population
- Environmental factors or pressures that affect survival
- Differential survival or reproduction based on traits
- Changes in trait frequencies over multiple generations

**Work Documentation:** Keep a Word Document as a project journal. Each time you work on your project:
- Write a few sentences describing what you accomplished during that session
- Include a screenshot (for digital work) or a photo (for physical work) showing your progress
- Reflect briefly on any challenges you faced or next steps you plan

**Game/Simulation Requirements:**
- Clear rules or instructions explaining how to play or run the simulation
- A way to track or show changes in the population traits over time
- Evidence that the game/simulation can be repeated with different outcomes based on player choices or random events
- Creativity and engagement

**Final Submission:** Submit your Word Document with the project journal and any materials needed to play or run your game or simulation, such as:
- Photos or scans of physical game components
- Links or files for digital simulations
- Written instructions or rulebook

---

## 2. Project in one paragraph

A 2D top-down artificial-life sandbox, built in Unity 6.3 LTS and published as a WebGL build on GitHub Pages. The Player picks a Preset or sets a Seed and five world settings, then watches Founder Cells (white blobs with two eyes) move through open space that contains Food pellets, drifting Toxin particles and a smooth Temperature field. Each Cell has a Genome made of Traits (size, speed, sense range, preferred Temperature, Temperature tolerance, Toxin resistance, diet, reproduction threshold, and two reserved mating Traits) plus a small neural-network Brain whose weights evolve. Cells reproduce asexually with mutation, compete for Food and can eat smaller Cells. Over many Generations, lineages drift apart in Genome space and split into Species that the program detects, colours and tracks. The Player can place Events (heat wave, ice age, famine, food bloom, toxin spill) anywhere in the World with a radius. Charts of Trait frequencies, population per Species, a family tree and live statistics show what happens. Extras: Presets, CSV export, a screenshot button, and saving and loading a Run as a file. A late-stage feature lets a mating Trait evolve so that Cells can discover sexual reproduction.

### At a glance

| Item | Decision |
|---|---|
| Format | Online simulation (browser) |
| Engine | Unity 6.3 LTS (latest 6000.3.x patch), WebGL build, Universal 2D template (URP) |
| Hosting | GitHub Pages, source on `main`, built game on `gh-pages` branch |
| Devices | Desktop browsers, mouse and keyboard |
| Genre | Pure sandbox, no scoring or win condition |
| World | Seeded, 160 by 90 units, open space with walls, no grid |
| Population | Up to 600 Cells (about 300 to 500 expected) |
| Heredity | Ten Traits plus a fixed-structure neural-network Brain with evolving weights |
| Reproduction | Asexual with mutation; evolved sexual reproduction is a late feature |
| Interaction | Food competition plus emergent predation |
| Pressures | Temperature (smooth field), Food (pellets), Toxin (drifting attracting particles) |
| Events | Heat wave, ice age, famine, food bloom, toxin spill, all click-placed with a radius |
| Tracking | Trait chart, Species chart, family tree, live stats |
| Extras | Presets, CSV export, screenshot button, save and load a Run as a file |
| Look | Flat, dark, scientific, no effects. Cells start white, colour follows the Genome, two eyes show heading |
| Architecture | Pure C# simulation core assembly plus a Unity layer; everything built in code; command line is the required test loop |
| Dates | Due November 16 to 17, 2026; target finish November 14 to 15, 2026 |
| Journal | Word (.docx), written by the user, template supplied |

---

## 3. Context, constraints and verified facts

### The user and the task
- A student at Shanghai American School doing an L4 science extension. The user said the project is "going to be real complex".
- **Due November 16 to 17, 2026. The user wants to be done by November 14 or 15.** Information given on October 30, 2026.
- The teacher allows coding with AI. The user has a GitHub account (username not yet recorded).
- Planning was done as two "grill-me" interviews with multiple-choice questions. A Claude Code agent builds; this chat is for planning.

### The user's computer (checked October 29 and 30)
- A Mac with the project folder `/Users/Yumei/Desktop/science L4`. On October 30 the folder holds `PLAN.md` and `CONTEXT.md`.
- Installed on October 30: **Unity Hub**, **Unity editor 6000.6.7f1** (standard release, not LTS), and a `Unity` folder in /Applications. Not found: Godot, Microsoft Word.
- Also installed: Python 3.12 (for a local web server), LibreOffice, Pages, Keynote, Obsidian, Ghostty, Blender, Inkscape and several browsers.
- The user chose **Unity 6.3 LTS** instead (decision B5). It must be installed with the **Web Build Support** module. Editor 6000.6.7f1 is not used by this project.
- The editor architecture (Apple Silicon or Intel) was not checked; the agent should run `uname -m` and install the matching build.

### Research findings (from Unity and GitHub documentation, October 30, 2026)
Items marked "not found" were not stated in the sources reached.
- Unity 6.3 LTS is the latest LTS, released December 4, 2025 (6000.3.0), supported to December 2027. The newest 6000.3.x patch in October 2026 was not found; ask the Hub or CLI (`editors -r`). Unity 6.6 (6000.6) is a standard release published August 31, 2026, latest patch 6000.6.7f1 on October 21, 2026, with no support end announced. (unity.com/releases/unity-6/support, endoflife.ai/unity/6000.6)
- Unity Personal: free below $200K of revenue and funds raised in the trailing 12 months. A free Unity Student plan also exists. (unity.com/products/unity-personal)
- WebGL ("Web") is a documented platform: needs a WebGL 2 and WebAssembly browser (Chrome, Firefox, Safari 15+ on macOS, Edge); no managed threads; garbage collection runs once per frame. (docs.unity.com WebGL pages)
- Command line: editor path example `/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity`. Flags: `-batchmode`, `-nographics`, `-quit`, `-projectPath`, `-executeMethod Class.Method` (static; exit non-zero on failure), `-logFile <path>` (`-` for stdout), `-buildTarget webgl`, and `-accept-apiupdate` in batch mode. Tests: `-runTests -batchmode -projectPath <p> -testPlatform EditMode -testResults <file.xml>`; do **not** pass `-quit` with `-runTests`. A project cannot be opened in batch mode while the editor has it open. Create a project: `Unity -createProject <path> -batchmode -quit`.
- Hub CLI (deprecated from Hub 3.18, experimental): `"/Applications/Unity Hub.app/Contents/MacOS/Unity Hub" -- --headless install --version <v> --changeset <hash> --module webgl -a arm64`. A newer standalone Unity CLI exists (`unity install`, `unity editors -i`, `unity test`, `unity build`, `unity auth login`); its full syntax was not documented, so use `--help`.
- Personal licences are activated by signing in to Unity Hub. Whether batch mode uses that Hub sign-in was **not documented**; session 1 must test it (Section 21).
- WebGL compression: gzip (default), Brotli (needs HTTPS and correct headers), Disabled (for static compression by the host). Decompression Fallback embeds a JavaScript decompressor, renames files `.unityweb` and disables WebAssembly streaming. GitHub Pages behaviour with Unity compressed files was **not documented**, hence **Compression Disabled** (decision B9).
- GitHub Pages limits: published site 1 GB, soft 100 GB per month bandwidth, 10 minute deploy timeout. A file over 100 MiB is blocked in git. (docs.github.com)
- UI Toolkit `Painter2D` is documented for runtime use with `generateVisualContent`. A WebGL-specific support statement was **not found**; an issue tracker entry mentions increased wasm heap with UI Toolkit in WebGL. A smoke test is required (Section 21).
- `Graphics.RenderMeshInstanced` has a limit of 1023 instances per call (511 by default). Per-instance colour on WebGL 2 was not confirmed, so the plan draws Cells with generated meshes instead (decision B7).
- Input System package: Mouse, Keyboard and Touchscreen are supported on WebGL. The default input handler of the Unity 6.3 2D template was not found; check `activeInputHandler` in `ProjectSettings/ProjectSettings.asset` (0 old, 1 new, 2 both) and set it to the new Input System.
- Unity's 2D guide recommends URP (Universal 2D template).

### Working method
- A Claude Code agent writes C# and text files directly into the project folder, compiles, tests and builds with the Unity command line, and may use an MCP server for extras (decision B2, B8).
- Screenshots for the Journal are taken by the user (macOS: Cmd+Shift+4 or Cmd+Shift+5), or with the in-game screenshot button once built.

---

## 4. Decision log

"Rec." means it was the recommended default. Several choices went against the recommendation; those are deliberate.

### Interview 1 (October 29, 2026): Q1 to Q27

| # | Topic | User's decision | Notes |
|---|---|---|---|
| Q1 | Format | Online simulation | Rec. |
| Q2 | Timeline | "1 to 2 weeks, and its going to be real complex" | Superseded by the exact dates in Section 3. |
| Q3 | Play style | Player controls the environment | Rec. |
| Q4 | Theme | "starts as individual cells, will eventually branch and evolve into species by itself" | Custom. |
| Q5 | Stack | "game engine, and im going to host it on github, cloudfare, or vercel" | Custom. |
| Q6 | Reproduction | Start asexual, unlock sexual reproduction later | Clarified by Q19. |
| Q7 | Pressures | Food supply and distribution; temperature and climate zones; toxins, pollution and disasters. Note: "players decide the starting enviroment and evolution decides the rest, they can add events and stuff in the middle" | Predators not a player control. |
| Q8 | Goal | Pure sandbox | Not rec. |
| Q9 | Engine | Unity WebGL | Not rec. (rec. Godot). |
| Q10 | Genome | Evolving neural-network brains | Not rec. |
| Q11 | Interaction | Competition plus emergent predation | Rec. |
| Q12 | Environment setup | Randomly generated worlds with a seed | Not rec. No map painting. |
| Q13 | Population | 300 to 500 Cells | Rec. |
| Q14 | Brain and body | Fixed small network, evolving weights, plus separate body Traits | Rec. |
| Q15 | Events | Ice age or heat wave; famine or food bloom; toxin spill | Meteor not selected. Placement refined by B18. |
| Q16 | Tracking | All four: Trait chart, Species chart, family tree, live stats | |
| Q17 | Start | Founders with small random variation | Rec. |
| Q18 | Generation | Lineage depth | Rec. Refined for two parents by B24. |
| Q19 | Sexual timing | Later in a Run, evolved by a mating Trait | Not rec. |
| Q20 | Start settings | Food abundance; temperature range and zones; toxin level; mutation rate (plus Seed) | |
| Q21 | Mate rule | Genetic-distance limit set by an evolving choosiness Trait | Rec. |
| Q22 | Cut order | Sexual reproduction first, then family tree | **Superseded by B21.** |
| Q23 | Hosting | GitHub Pages | Rec. Compression changed by B9. |
| Q24 | Journal | User writes entries in their own words; template supplied; entries note where AI helped | Rec. |
| Q25 | Rules | In-game help screen plus a one-page PDF rulebook | Rec. |
| Q26 | Look | "cells start white and slowly diverge, the closer the genes are the closer the colors are. this is a simulation, not a product demo, so no flashy graphics or other over the top vfx" | Custom. "Genes" here means Traits. |
| Q27 | Devices | Desktop browsers, mouse and keyboard | Rec. |

### Interview 2 (October 30, 2026): B1 to B24 (these were asked as Q1 to Q24 that day)

| # | Topic | User's decision | Notes |
|---|---|---|---|
| B1 | Sameness target | "im not actually having 2 agents build it that was just a figure of speech" | Question moot. One agent builds; detail still matters across work sessions. |
| B2 | Scene and assets | "code and they can also use unity cli and mcp" | Code-first. CLI and MCP both allowed. |
| B3 | Code structure | Pure C# simulation core plus a Unity layer | Rec. |
| B4 | Naming | Trait everywhere, Genome for the full set | Rec. "Gene" is not used. |
| B5 | Unity version | Install Unity 6.3 LTS | Not rec. (rec. stay on installed 6000.6.7f1). |
| B6 | UI system | UI Toolkit with Painter2D charts | Rec. Smoke test required. |
| B7 | Drawing Cells | One generated mesh with vertex colours | Rec. |
| B8 | Test loop | Command line required, MCP optional | Rec. |
| B9 | Technical bundle | Universal 2D template, Input System package read in code, IL2CPP, compression Disabled, canvas fits window | Rec. |
| B10 | Repo layout | One repo, source on `main`, built game on `gh-pages` | Rec. GitHub username not given. |
| B11 | Name for world squares | "its not made of squares its empty with stuff in it, and cells are just blobs that move in it." | **No grid.** Replaced the old 80 by 45 grid design. |
| B12 | Layout | Collapsible right-hand tabbed panel | Rec. |
| B13 | Food | Discrete food pellets | Rec. |
| B14 | Temperature | Smooth field, plus warm or cold spots that Events can add | Not rec. (rec. smooth field only). |
| B15 | Toxin | Toxin that spreads and drifts | Not rec. |
| B16 | World edges | Rectangle with walls | Rec. |
| B17 | Toxin motion | Wandering, fading particles "but add slight gravity between them so they dont diffuse too easily" | Custom. |
| B18 | Event placement | Everything click-placed with a radius | Not rec. |
| B19 | Cell look | Plain circle sized by the size Trait, "instad of heading line its 2 eyes" | Custom. |
| B20 | Extras | All four: save and load a run, CSV export, Presets, screenshot button | Large scope. |
| B21 | Cut order | Save/load, then sexual reproduction, then family tree, then screenshot, then CSV | Rec. Replaces Q22. Presets are cheap and stay. |
| B22 | Save storage | Download and upload a save file | Not rec. |
| B23 | Species split | The group nearer the old average stays the original Species | Rec. |
| B24 | Generation, two parents | One more than the older parent's Generation | Rec. |

**Interview 3 (October 30, 2026): design decisions D1 to D8 and the project name** are recorded in `DESIGN.md`, Section 12. In short: the project is named **Divergence**; one monospaced font everywhere; Cell colour in OKLCH; icons everywhere with hover tooltips on every control and chart; 4 px corners; text contrast and control borders held to WCAG levels (keyboard operation and a UI scale setting were not chosen); comfortable 40 px controls with 15 px text; a monospace wordmark with a branching glyph.

---

## 5. Rubric coverage map

| Rubric requirement | How the project meets it |
|---|---|
| Variation in traits within a population | Founders differ by small random amounts and every birth adds mutation. The Trait chart shows the spread. |
| Environmental factors or pressures | Temperature field, Food pellets, Toxin particles, plus Events the Player places. |
| Differential survival or reproduction | Better-matched Cells gain energy faster, live longer and divide more. Deaths are recorded by cause. |
| Changes in trait frequencies over multiple generations | The Trait tab plots each Trait's mean, spread and binned frequencies against time or mean Generation. |
| Clear rules or instructions | In-game help screen plus a one-page PDF rulebook. |
| A way to track or show changes over time | Four tracking displays (Section 12). |
| Repeatable with different outcomes | Seed fixes the starting World; settings, Presets, Events and random mutation change outcomes; save files can replay from a stored point. |
| Creativity and engagement | Cells with eyes and evolving neural-network Brains, drifting attracting Toxin, emergent predation, species that split by themselves, evolved sexual reproduction. |
| Journal with multiple sessions, text and screenshots | Section 18 and Section 19. |
| Submission materials | Link to the hosted build, zipped build, PDF rulebook, Word Journal. |

---

## 6. The World and its Pressures

All numbers are **TUNE** starting values unless stated. Keep every tunable number in one `SimParameters` class (Section 14) so it can change without editing logic.

### 6.1 Space
- The World is a rectangle, 160 by 90 units, from (0,0) at the bottom left to (160,90) at the top right, y up. Cells, Food and Toxin particles stay inside it. Cells and particles that reach an edge stay at the edge (Cells slide along it; particles reflect).
- There is **no grid** in the model. Positions are continuous. The only grids in the code are internal lookup structures (SpatialHash, Section 8.2) and a texture used to draw the Temperature tint; they do not store World state.

### 6.2 Seed
- A Seed is text or a number typed by the Player. It is converted to a 32-bit value (FNV-1a hash of the UTF-8 text) and used to start two random number streams: the world stream (World layout) and the sim stream (everything that happens during the Run, including mutation, Food spawning and Events).
- The same Seed and settings always create the same starting World and Founders.

### 6.3 Temperature (smooth field)
- Temperature `T(x, y, t)` runs from 0 (cold) to 1 (hot) and is **computed from position on demand**, not stored per square.
- Base field: `Tbase(x,y) = clamp01(0.5 + range * (0.6 * g + 0.4 * (n - 0.5)))`, where
  - `g = 0.5 - y / 90` (warm at the bottom, cold at the top; range from -0.5 to 0.5),
  - `n` is seeded 2D Perlin noise in 0..1 at `(x * zoneScale / 60, y * zoneScale / 60)`,
  - `range` (0 to 1) and `zoneScale` (0.5 to 2) are Player settings.
- Events add local or global offsets (Section 10). `T(x,y,t) = clamp01(Tbase + sum of active offsets at (x,y,t))`.
- The Perlin noise must be a seeded implementation inside the core assembly (a permutation table shuffled by the world stream), not `Mathf.PerlinNoise`, so the core stays free of Unity code and runs identically in tests and in the browser.
- A Cell's comfort: stress `= max(0, abs(T(x,y,t) - preferredTemperature) - temperatureTolerance)`.

### 6.4 Food (discrete pellets)
- A Food pellet has a position and carries 20 energy. Radius 0.15 (for drawing and touching).
- Spawning: a stream of `30 * foodAbundance` pellets per second of simulated time. Each pellet position is chosen by rejection sampling: draw a random point, accept with probability `richness(x,y)`, retry up to 20 times (then accept the last point). `richness(x,y) = clamp01(1.4 * nFood(x,y) - 0.2)`, where `nFood` is a second seeded Perlin noise field (scale 1/40), drawn from the world stream's permutation table. This gives rich and sparse regions without storing anything per square.
- Maximum 1,500 pellets in the World. While at the cap, spawning waits.
- Initial pellets: before the first step, spawn 600 pellets by the same method (so the Seed fixes them).
- A Cell eats a pellet by touching it (Section 8.6). The pellet is removed.

### 6.5 Toxin (drifting, attracting particles)
- Toxin is a set of particles. Each has position, velocity and a strength `s` that fades by half-life (initial clouds 900 s; Event spills 90 s). A particle is removed when `s < 0.05`. At most 1,500 particles in the World.
- Starting Toxin: `toxinLevel` (0 to 1) sets how much. Create `3 + round(toxinLevel * 5)` clouds (none when `toxinLevel = 0`) and `round(600 * toxinLevel)` particles in total, split evenly. Each cloud has a random centre (at least 10 units from the edges) and a cloud radius of 6 to 14 units; particles are placed with a normal distribution of standard deviation `cloudRadius / 2` around the centre, clamped to the World.
- Motion each step (`dt = 0.05`):
  1. `v = v * exp(-1.5 * dt)` (damping)
  2. add random drift: `v += N(0, 0.6) * sqrt(dt)` for each axis
  3. add attraction: for every other particle `j` within `Rc = 3` units, `a += G * (pj - pi) / (d + 0.5)^2` with `G = 2.0`, then cap `|a|` at 3, and `v += a * dt`
  4. `p += v * dt`; reflect at the walls
- The attraction ("slight gravity") keeps clouds together so they spread slowly; the goal is a 10-unit cloud growing by less than about 50% in diameter over 5 simulated minutes (**TUNE**).
- A Cell's Toxin exposure `X` (0 to 1) is `min(1, 0.1 * sum over particles j within Rt of s_j * (1 - d_j / Rt))` with `Rt = 2.5 + cellRadius`.

### 6.6 Presets and Player settings
Player settings (all set before a Run starts):

| Setting | Range | Default |
|---|---|---|
| Seed | any text or number | random 6-character text |
| Food abundance | 0.2 to 2.0 | 1.0 |
| Temperature range | 0 to 1 | 0.6 |
| Zone scale | 0.5 to 2.0 | 1.0 |
| Toxin level | 0 to 1 | 0.3 |
| Mutation rate | 0.25 to 3.0 | 1.0 |

Presets (**PROPOSED**; names and values can change; each fills all six settings including a fixed Seed so the world is identical every time):

| Preset | Seed | Food | Temp range | Zone scale | Toxin | Mutation |
|---|---|---|---|---|---|---|
| Mild world | mild | 1.0 | 0.4 | 1.0 | 0.2 | 1.0 |
| Cold gradient | gradient | 0.9 | 1.0 | 1.0 | 0.1 | 1.0 |
| Toxic basin | basin | 1.2 | 0.5 | 1.5 | 0.9 | 1.0 |

Founders: 120 Cells at the start of every Run (**PROPOSED**).

---

## 7. Cells, Traits and Brains

### 7.1 Traits
Each Cell stores one float per Trait. Ranges are fixed. A "normalised" Trait value is `(value - min) / (max - min)`, used for mutation, Species distance, colour and charts.

| Index | Trait (enum `TraitId`) | Range | Founder mean | Benefit | Cost or trade-off |
|---|---|---|---|---|---|
| 0 | Size | 0.5 to 3.0 | 1.0 | More energy capacity, can eat much smaller Cells, harder to eat | Base cost grows with `size^0.75`; moving costs scale with `size^0.5` |
| 1 | Speed (maximum, units per second) | 0.5 to 4.0 | 1.5 | Reaches Food sooner, escapes predators | Movement cost scales with speed squared |
| 2 | Sense range (units) | 2 to 15 | 7 | Sees Food, Cells and Toxin from farther | Base cost grows with range |
| 3 | Preferred Temperature | 0 to 1 | 0.5 | No stress where Temperature matches | Stress where it does not |
| 4 | Temperature tolerance | 0.05 to 0.5 | 0.25 | Survives a wider range of Temperatures | Base cost grows with tolerance |
| 5 | Toxin resistance | 0 to 1 | 0.1 | Cuts Toxin damage by up to 90% | Base cost grows with resistance |
| 6 | Diet (0 plant eater, 1 meat eater) | 0 to 1 | 0.05 | High diet gains energy from eating Cells | High diet gets much less energy from Food pellets |
| 7 | Reproduction threshold (fraction of maximum energy) | 0.5 to 0.95 | 0.8 | Low threshold divides sooner | Offspring get a share of the parent's energy, so early dividers produce weaker offspring |
| 8 | Sexual tendency | 0 to 1 | 0.0 | Reserved (Section 11) | Reserved |
| 9 | Choosiness | 0 to 1 | 0.5 | Reserved (Section 11) | Reserved |

- Each advantage has a cost so that no single Genome wins everywhere. If testing shows one extreme always wins, adjust costs rather than benefits.
- Founder variation: each Trait is drawn from a normal distribution around the founder mean with standard deviation 5% of the Trait range, then clamped. Sexual tendency starts at exactly 0 for every Founder; choosiness uses the table value with the same spread.
- Traits 8 and 9 are skipped by mutation and by every statistic until the sexual feature exists (Section 11). If the feature is cut, delete them and fix `TraitCount`.

### 7.2 Brain
A feedforward network with fixed structure.
- **11 inputs** (index order is part of the specification, constants in `BrainLayout`):
  0. food direction: sine of the angle from heading to the nearest Food pellet in sense range (0 if none)
  1. food proximity: `1 - distance / senseRange` (0 if none)
  2. prey direction: sine of the relative angle to the nearest Cell at least 1.2 times smaller in radius, in sense range (0 if none)
  3. prey proximity
  4. threat direction: nearest Cell at least 1.2 times larger
  5. threat proximity
  6. Toxin direction: nearest Toxin particle with strength 0.3 or more in sense range
  7. Toxin proximity
  8. Temperature mismatch: `(T(x,y,t) - preferredTemperature)`, clamped to -1..1
  9. energy fraction: `energy / maxEnergy`
  10. bias: constant 1
- **6 hidden neurons**, activation `tanh`.
- **4 outputs**, activation `tanh`: 0 turn (-1 to 1), 1 thrust (use `(o + 1) / 2`), 2 bite (active when above 0.3), 3 mate-seek (inert until Section 11).
- **Weights: 100** in one array in this order: input-to-hidden `W1[i * 6 + h]` for 11 x 6 = 66; hidden biases `b1[h]` for 6; hidden-to-output `W2[h * 4 + o]` for 24; output biases `b2[o]` for 4. Keep the count in a constant (`BrainLayout.WeightCount = 100`).
- **Founder Brains**: all weights drawn from `N(0, 0.3)`, then add a small innate wiring so that first-generation Cells can find Food at all: `W1[foodDirection -> hidden 0] += 1.5`, `W2[hidden 0 -> turn] += 1.0`, `W1[foodProximity -> hidden 1] += 1.5`, `W2[hidden 1 -> thrust] += 1.0`, `b2[thrust] += 0.3`. Multiply the added amounts by a setting `innateWiring` (default 1.0; set 0 to test fully random Brains). Nothing else is wired; fleeing, avoiding Toxin and hunting have to evolve.
- All array sizes are allocated once; the forward pass allocates nothing.

### 7.3 Cell body and state
- Radius `r = 0.5 * size` units.
- Maximum energy `Emax = 100 * size`.
- Founders start at `0.6 * Emax`, a random heading and age 0.
- Stored per Cell: id, position, heading, energy, age, Generation, parent id, Species id, Traits (10 floats), Brain weights (100 floats), rolling drain averages (for death cause), bite timer, alive flag.
- Cells do not collide or push each other; they may overlap. Vision is 360 degrees; the eyes on the drawing are cosmetic (they show the heading).
- Motion is direct: `heading += turn * 3.0 * dt`, `speed = thrust * speedTrait`, `position += speed * (cos heading, sin heading) * dt`.

### 7.4 Colour
- Founders are white. Colour is a continuous function of the Genome, so near Genomes have near colours.
- Method (decision D2, October 30, 2026): perceptual colour in OKLCH. Hue comes from the direction of the Genome's deviation from the Founders, chroma from its size, and lightness stays at 0.78 or above, so every Cell is bright on the dark background and zero deviation is exactly white. The full algorithm, constants and tests are in `DESIGN.md` Section 7 and replace the earlier RGB-subtraction idea.
- The colour is recomputed when a Cell is born, and a Species' display colour is the colour of its centroid.
- The background is dark so white Cells are visible.

---

## 8. Simulation step

### 8.1 Time
- Fixed step `dt = 0.05` s (20 steps per simulated second). Speed buttons: 1x, 2x, 5x and 10x run that many steps per second of real time.
- Each rendered frame accumulates real time times speed and runs steps while the accumulator allows, but stops at a budget of 10 ms of real time per frame; the effective speed may then fall below the selected speed. Rendering and UI use the latest state.
- Pause freezes the accumulator. The Player can still pan, zoom and inspect.

### 8.2 Spatial lookup
Three `SpatialHash` structures, rebuilt each step by counting sort into preallocated arrays (no allocation): one for Cells, one for Food pellets, one for Toxin particles. Bucket size 5 units (32 by 18 buckets). A query returns items within a radius by scanning the covered buckets. These buckets are lookup helpers, not World state.

### 8.3 Order of work per step
1. Update Events (time left, ramp envelopes); remove expired ones.
2. Spawn Food (stream plus any bloom streams); apply famine rejection.
3. Move Toxin particles (Section 6.5) and fade them.
4. Rebuild the three hashes.
5. For each living Cell in index order: sense, think, move, pay costs, eat Food, bite, check death, check reproduction (details below). Newborns are added to a pending list and join at the end of the step so that iteration is stable.
6. Remove dead Cells (swap-remove from the dense alive list), add newborns, update counters and death causes.
7. Every 2 s of simulated time: Species clustering (Section 9). Every 1 s: statistics sample (Section 9.4).

### 8.4 Sensing and thinking
For each Cell, using its sense range, find the nearest Food pellet, nearest prey Cell, nearest threat Cell and nearest Toxin particle with strength 0.3 or more. Build the 11 inputs, run the forward pass and read the four outputs.

### 8.5 Energy costs per second
- Base: `0.4 * size^0.75 + 0.05 * senseRange + 0.3 * toxinResistance + 0.4 * temperatureTolerance`. For a Founder this is about 0.9.
- Movement: `0.15 * (thrust * speedTrait)^2 * size^0.5`.
- Ageing: `0.01 * (age - 300)` once age exceeds 300 s.
- Temperature stress: `8 * stress`.
- Toxin: `6 * X * (1 - 0.9 * toxinResistance)`.
- Keep a rolling average (time constant 5 s) of each of four groups: baseline (base plus movement), temperature, Toxin, ageing. The death cause is the largest group at death.

### 8.6 Eating
- Food: a pellet within `cellRadius + 0.15` of a Cell is eaten; the Cell gains `20 * (1 - 0.9 * diet)` energy (capped at Emax; the excess is lost).
- Predation: if output 2 is above 0.3, diet is at least 0.3, and another Cell is touching (distance below `r1 + r2 + 0.1`) with radius at least 1.2 times smaller, accumulate bite time. After 0.5 s of continuous contact the prey dies with cause "eaten" and the predator gains `diet * (0.5 * prey.energy + 0.3 * prey.Emax)`, capped at its Emax.

### 8.7 Death
A Cell dies when energy reaches 0 (cause: the largest drain group) or when eaten.

### 8.8 Reproduction (asexual)
When `energy >= reproductionThreshold * Emax` and the living count is below 600, the Cell divides: the parent keeps 50% of its energy, the child receives 40% (capped at the child's Emax), and 10% is lost. The child appears 1 radius away at a random angle with the parent's heading, `Generation = parent.Generation + 1`, and a mutated copy of the Genome.

### 8.9 Mutation (mutation rate `m`, default 1.0)
- Each Trait in indices 0 to 7 mutates with probability 0.35: add `N(0, 0.04 * range * m)`, then clamp. With probability 0.02 per mutation the step is five times larger.
- Each Brain weight mutates with probability 0.08: add `N(0, 0.25 * m)`, same large-step rule, clamp to -4..4.

### 8.10 Calibration targets (what "tuned" means)
The constants above are starting values. Tune until, on the Mild world Preset with no Events:
- The population settles between 250 and 450 Cells within 5 simulated minutes and does not go extinct on at least 9 of 10 different Seeds.
- Cell generation time (birth to division) averages 60 to 120 s.
- At least two Species appear by themselves within 10 to 25 simulated minutes on most Seeds.
- At least one Trait shifts visibly in response to each Event type.
- 500 Cells run at 30 frames per second or more at 1x speed in Chrome on a typical laptop.

---

## 9. Species, Generation and tracking

### 9.1 Generation
A Founder is Generation 0. Every other Cell is one more than its older parent (the parent with the higher Generation when there are two). Charts can use mean Generation of the living population as an x-axis.

### 9.2 Genome distance
```
D = 0.6 * Dtraits + 0.4 * Dbrain
Dtraits = sqrt(mean over Traits 0..7 of (aNormalised - bNormalised)^2)
Dbrain  = sqrt(mean over the 100 weights of (wa - wb)^2) / 2.0
```
The weights (0.6, 0.4), the divisor 2.0 and the threshold `theta` (default 0.15) are **TUNE**. Traits 8 and 9 never count towards a Species.

### 9.3 Species detection
Run every 2 s of simulated time:
1. Each living Species has a stored centroid Genome.
2. Assign each Cell to the nearest Species centroid if `D <= theta`.
3. Unassigned Cells go through greedy leader clustering with the same threshold. A group with at least 10 Cells that is still present at two consecutive checks becomes a **new Species**.
4. The new Species' parent is the existing Species whose centroid is nearest to the group. The group nearer the old average therefore stays the original (decision B23).
5. Leftover Cells in groups below 10 are assigned to the nearest Species for display only.
6. Update each Species' centroid: `0.8 * old + 0.2 * new mean`.
7. A Species with no members at two consecutive checks is extinct, with the time recorded.
8. Species never merge (simplification, stated in the rulebook).
9. The first Species is created from the Founders at the first check.

Species record: id, parent id, name ("Species 7"), birth time, birth Generation, extinction time (or none), peak population, current population, centroid, display colour.

### 9.4 Statistics sampling
Once per simulated second record: time, living count, mean and maximum Generation, per-Trait mean and standard deviation (Traits 0 to 7), per-Trait binned counts, per-Species population, cumulative births, cumulative deaths by cause, and Event markers. When a series reaches 3,600 samples, keep every second sample and double the sampling interval from then on (deterministic decimation).

Trait bins: 6 bins per Trait spanning the window `founderMean +/- 0.35 * (max - min)`, with the two outer bins open-ended. This keeps changes visible instead of dumping everything in one bin (**TUNE**).

---

## 10. Events

Every Event is placed by clicking the World with a radius, or applied to the whole World.

Placement flow: choose the Event from the top bar menu, set its sliders in a small dialog, press Place; the pointer shows a circle preview of the radius; click the World to place it, or press Escape to cancel. A "Whole World" checkbox skips placement and applies it everywhere.

Shared: radius 5 to 60 units (default 20). Influence weight at distance `d` is `w = 1 - smoothstep(0.6 * r, r, d)`; with Whole World, `w = 1`.

| Event | Effect (**TUNE**) | Sliders |
|---|---|---|
| Heat wave | Adds `+intensity * w` to Temperature. Envelope: ramp up 10 s, hold for the duration, ramp down 30 s. | Intensity 0.1 to 0.5 (0.3), duration 30 to 180 s (90) |
| Ice age | Same with a negative offset. | Same |
| Famine | At placement, remove 80% of Food pellets inside the area. For the duration, the Food stream rejects candidate positions inside the area with probability `0.9 * w`. | Duration 30 to 120 s (60) |
| Food bloom | For the duration, an extra stream of 40 pellets per second is placed uniformly inside the area (respecting the 1,500 cap). | Duration 30 to 120 s (60) |
| Toxin spill | At placement, add `round(300 * radius / 10)` particles uniformly inside the area (respecting the 1,500 cap), half-life 90 s. | Radius only |

Each Event is logged with its time and shown as a marker on every chart. Meteor or mass extinction was not selected and is out of scope. Up to 8 active Events may overlap.

---

## 11. Sexual reproduction (late feature; cut early, see Section 19)

Applies only if this feature is built. Traits 8 and 9 and the "mate-seek" output exist from the start so that the Genome layout never changes.

- A Cell whose sexual tendency is at least 0.5 does not divide alone. When its energy reaches its threshold it looks within sense range for a partner who also has sexual tendency at least 0.5, has energy at least 60% of its own threshold, and passes the compatibility rule.
- Compatibility: `D(a,b) <= maxMateDistance(a)` and `D(a,b) <= maxMateDistance(b)`, with `maxMateDistance(c) = lerp(Dmax, Dmin, choosiness(c))`, `Dmax = 2 * theta`, `Dmin = 0.25 * theta` (**TUNE**). Higher choosiness means only close relatives are acceptable, which leads to reproductive isolation over time (decision Q21).
- Costs: each parent pays 30% of its energy. The child receives 40% of each parent's payment (so 12% of the combined energy is lost). A sexual Cell with no partner waits and keeps paying costs.
- The child's Genome is a uniform crossover of both parents' Traits and Brain weights (each value from either parent with probability 0.5), then mutation. `Generation = max(parent Generations) + 1` (decision B24).
- Sexual reproduction must be advantageous only in some conditions, or it will either dominate or never spread. The cost numbers are the main balance lever.
- Add a statistic: fraction of the living population with sexual tendency of at least 0.5.
- When the feature is not built, mutation skips Traits 8 and 9 and nothing reads them.

---

## 12. User interface

Built with UI Toolkit, written in code (UXML and USS text files are allowed). Panel scaling: scale with screen size, reference 1920 by 1080, match 0.5.

### 12.1 Screens and layout
- **Setup screen** (shown at start and when returning from a Run): a centred card 720 px wide with the title, a Preset dropdown (Custom, Mild world, Cold gradient, Toxic basin), Seed text field with a "Random" button, five sliders with numeric readouts (Section 6.6), and buttons: Start, Load saved Run, How it works.
- **Run screen**:
  - Top bar, 56 px high (40 px controls; see `DESIGN.md` Sections 3 and 6.2 for the exact layout): left, Play/Pause and speed buttons 1x 2x 5x 10x; centre, a readout `time mm:ss | mean generation | population | species`; right, Events menu, temperature tint toggle, Save, Load, Export CSV, Screenshot, Help, panel toggle, Menu (back to Setup).
  - World view fills the rest of the window on the left. A right-hand panel, about 30% of the width (minimum 420 px, maximum 560 px), holds four tabs: Traits, Species, Tree, Stats. The panel can be collapsed; the World view then takes the full width.
  - Cell inspector: clicking a Cell selects it (thin white ring) and shows a small card at the top left of the World view with the Trait values as bars, energy, age, Generation, Species and the four Brain outputs.
- **Help screen**: an overlay with sections: what this is, controls, the settings, how the World works (Temperature, Food, Toxin), what Traits do, how Cells reproduce, Species and colours, how to read each chart, Events, saving and exports.
- **Extinction message**: when no Cell is alive, show "Population extinct" with run time, maximum Generation, Species counts, and buttons Restart, Menu, Export CSV.

### 12.2 Tabs
- **Traits**: a dropdown chooses one Trait. Top chart: mean line with a shaded standard-deviation band. Bottom chart: stacked area of binned frequencies (Section 9.4). An x-axis switch chooses simulated time or mean Generation. Event markers appear as dashed vertical lines with a one-letter label.
- **Species**: stacked area of population per Species in each Species' colour (the 20 largest, the rest grouped as "Other"). A legend lists living Species with population; clicking one highlights its Cells in the World.
- **Tree**: family tree with time on the horizontal axis; each Species is a horizontal line from birth to extinction (or to now), joined to its parent by a vertical connector at its birth time; line thickness reflects population (1 to 6 px); order of rows is a depth-first walk of the tree; clicking a branch shows its summary. Drawn only if the feature is built (cut order, Section 19).
- **Stats**: current population, births and deaths per minute, maximum Generation, living and extinct Species counts, and a stacked bar chart of deaths by cause (starved, eaten, poisoned, temperature, old age) in 10 s bins.

### 12.3 Charts
Drawn with `Painter2D` in a `generateVisualContent` callback on a `VisualElement`. Refresh once per sample or once per real second, not every frame. Axes with 4 to 6 labelled ticks, light gridlines, a legend where needed, and a hover read-out of the value under the pointer. Nothing animates.

### 12.4 Controls
- Left mouse button drag pans the World. A click without a drag (movement under 4 px) selects the nearest Cell within `max(radius, 1.0)` units, or clears the selection.
- Mouse wheel zooms around the pointer (a factor of 1.1 per notch; trackpad pinch arrives as wheel events). Zoom limits: orthographic size from 8 units up to 1.2 times the fitting size.
- Keys: Space pause or resume; 1, 2, 3, 4 select speed 1x, 2x, 5x, 10x; F fits the whole World in the view; T toggles the Temperature tint; Tab collapses the panel; Escape cancels Event placement, closes dialogs, or clears the selection.

### 12.5 Visual design
- The full design system is in `DESIGN.md`: dark neutral theme, one monospaced font (decision D1), icons drawn in code with hover tooltips on every control and chart (D3, D4), 4 px corners (D5), 40 px controls with 15 px text (D7), a lowercase "divergence" wordmark with a branching glyph (D8), and no animation. Use its tokens; do not write colours or sizes into components.
- The earlier palette listed here is superseded by `DESIGN.md` Section 2.2.

---

## 13. Rendering

- Orthographic camera looking at the 2D World. Camera viewport fits the area left of the panel; the World is fitted with a margin of 2 units.
- Dark rectangle for the World background and a thin lighter outline for the walls.
- **Temperature tint**: a quad over the World with a procedurally generated 160 by 90 texture (one texel per unit, bilinear), coloured as in `DESIGN.md` Section 2.2 (blue for cold, neutral in the middle, red for hot, 30% opacity). Regenerated when the Seed or any Event is active (at most twice a second), otherwise only on Start. This texture is presentation only, not World state.
- **Cells**: a single dynamic `Mesh` rebuilt each frame. Each Cell contributes a body quad textured with a procedurally generated circle (64 by 64, white with alpha edge) and vertex colour from the Genome, plus two eye quads (a small dark circle) at angles +/- 0.6 radians from the heading, distance `0.55 * r` from the centre, eye radius `0.2 * r`. Vertices per Cell: 12. Indices are 16-bit (fewer than 65,535 vertices). A selected Cell gets a ring quad.
- **Food**: one dynamic mesh of small quads using the same circle texture, in `world-food` (`DESIGN.md` Section 2.2). **Toxin**: one dynamic mesh of quads in `world-toxin`, alpha `0.2 + 0.6 * strength`.
- One custom unlit shader `Evo/UnlitVertexColorTexture` (text `.shader` file under `Assets/Shaders`) with vertex colour and a texture. Materials and the shader are referenced from a `RenderAssets` ScriptableObject in `Assets/Resources` so the build does not strip them (`Shader.Find` can be stripped; avoid it). The shader pass carries no `LightMode` tag so the Universal 2D renderer draws it as an unlit pass; verify in the session 1 smoke test.
- Draw order: background, Temperature tint, Toxin, Food, Cells, selection ring, event preview circle.

---

## 14. Architecture

### 14.1 Assemblies (decision B3)
| Assembly (asmdef) | Contents | Rules |
|---|---|---|
| `Evo.Core` | All simulation rules and data. Namespaces `Evo.Core`, `.World`, `.Cells`, `.Species`, `.Stats`, `.Persistence`. | `noEngineReferences: true`. No `UnityEngine` or `UnityEditor` references. Pure C#. |
| `Evo.UnityLayer` | MonoBehaviours, rendering, input, UI Toolkit, file bridge, app controller. | References `Evo.Core`. |
| `Evo.Editor` | Editor-only scripts: project setup, build, headless experiment runner. | Editor platform only. |
| `Evo.Tests.EditMode` | Unit tests of `Evo.Core` (and the Editor scripts where useful). | References `Evo.Core`, Unity Test Framework. |

### 14.2 Folder layout (inside the repository root)
```
science L4/                 (repository root, also the user's project folder)
  PLAN.md  CONTEXT.md
  docs/adr/                 decision records (created only if the user accepts any)
  Journal/                  the Word journal and screenshots (kept out of git)
  Rulebook/                 one-page PDF rulebook and its source
  tools/                    shell scripts (Section 16)
  EvoSim/                   Unity project
    Assets/
      Scripts/Core/         World, Cells, Species, Stats, Persistence (Evo.Core)
      Scripts/UnityLayer/   App, Rendering, UI, Input, Interop (Evo.UnityLayer)
      Scripts/Editor/       (Evo.Editor)
      Tests/EditMode/       (Evo.Tests.EditMode)
      Fonts/                JetBrains Mono TTFs and licence (DESIGN.md 2.3)
      UI/Theme/             tokens.uss (generated), typography.uss, components.uss
      Shaders/              Evo/UnlitVertexColorTexture.shader
      Resources/            RenderAssets.asset
      Plugins/WebGL/        FileBridge.jslib
      Scenes/Main.unity
    Packages/  ProjectSettings/
  Builds/                   build output (git-ignored on main)
```

### 14.3 Core classes (public surface, **PROPOSED** names)
- `SimParameters`: every constant from Sections 6 to 11 with its default (public fields), plus `Validate()`.
- `SetupSettings`: seed text and the five Player settings.
- `SeededRandom`: xoshiro128** with 32-bit state words (four `uint`), seeded from a 32-bit value expanded by splitmix32. Methods: `NextUInt()`, `NextFloat01()` (24 random bits), `NextRange(min, max)`, `NextInt(maxExclusive)`, `NextGaussian()` (Marsaglia polar method; the cached second value is part of the saved state). Never use `System.Random` or `UnityEngine.Random` in `Evo.Core`.
- `PerlinNoise2D`: seeded permutation table, `Sample(x, y)` returning 0..1.
- `TraitId` enum and `TraitInfo` table (name, min, max, founder mean, founder spread); `Genome` (Trait array, weight array, copy, mutate, crossover, normalise, distance).
- `BrainLayout` (constants) and `Brain` (forward pass over preallocated arrays).
- `Cell` (class, preallocated and pooled), `CellPool` (capacity 700, free list, dense alive list with swap-remove), `Food` and `ToxinParticle` structs in preallocated arrays.
- `SpatialHash` (counting sort rebuild, `Query(x, y, radius, results)` writing into a caller-supplied buffer).
- `World` (dimensions, `TemperatureAt(x, y)`, `FoodRichnessAt(x, y)`, Event offsets).
- `EventSystem` (list of active Events with envelopes) and `EventSpec`.
- `Simulation` (`Create(SetupSettings, SimParameters)`, `Step()`, state access, subscription callbacks for births, deaths, species changes).
- `SpeciesTracker`, `SpeciesRecord`, `GeneticDistance`.
- `StatsRecorder`, `TimeSeries` (with decimation).
- `SimSnapshot` (serialisable data transfer objects with `[System.Serializable]`) and `SnapshotMapper` (Simulation to and from snapshot).
- `StateHash` (a 64-bit hash of the whole simulation state, used in tests).

### 14.4 Cell data layout
Pooled class instances. `CellPool` preallocates 700 `Cell` objects, each owning a Trait array of length `TraitCount` and a weight array of length 100 allocated once. A birth takes a free instance and copies arrays in place; a death returns it. The alive list is a dense array of indices with swap-remove. No allocation in the step loop.

### 14.5 Unity layer
- `Bootstrap` (a `[RuntimeInitializeOnLoadMethod]`) builds everything from code: the camera, a `UIDocument` with a runtime `PanelSettings`, the render objects, the input handling and the `AppController`. `Main.unity` is nearly empty (created by an Editor setup script).
- `AppController` is a small state machine: Setup, Running, Paused, Extinct. It owns the `Simulation`, the `SimulationRunner` (the accumulator from Section 8.1) and the views.
- Rendering: `WorldView` (background, outline, Temperature tint), `CellMeshRenderer`, `FoodMeshRenderer`, `ToxinMeshRenderer`, `CameraController`, `RenderAssets`.
- Input: `InputRouter` reads `Mouse.current` and `Keyboard.current` from the Input System package; UI Toolkit receives pointer and keyboard events through an `EventSystem` with an `InputSystemUIInputModule` (assign the default actions in code). Set the project input handler to "Input System Package (New)".
- UI: `UiRoot`, `SetupScreen`, `TopBar`, `SidePanel`, `TraitsTab`, `SpeciesTab`, `TreeTab`, `StatsTab`, `CellInspector`, `EventDialog`, `HelpScreen`, `ExtinctionOverlay`, `ChartElement` (shared chart base using `Painter2D`), plus the design-system pieces in `DESIGN.md` Section 9: `DesignTokens`, `Icons`/`IconElement`, `TooltipController`, `UiText` (the copy table).
- Interop: `FileBridge` wrapping `Assets/Plugins/WebGL/FileBridge.jslib` (functions `DownloadBytes(name, mime, dataPtr, length)` and `PickFile(callbackObject, callbackMethod, accept)` using `FileReader`). In the Editor, the wrapper writes downloads to a folder under `Application.persistentDataPath/exports`, and the file picker is replaced by a path from a text field, so everything is testable without the browser.

### 14.6 Performance rules (Unity WebGL is single-threaded, GC once per frame)
- Preallocate everything; no LINQ, no `new` in per-step or per-frame code, no string building in the hot loop, no boxing.
- Spatial hash for all neighbour queries; never all-pairs.
- Brain forward pass is about 100 multiply-adds per Cell. Species clustering about 1.5 million operations every 2 s. Mesh rebuild each frame touches about 7,200 vertices for 600 Cells.
- If too slow: lower the Cell cap first (interview 1, Q13), then reduce hidden neurons to 4, then sense every second step.

---

## 15. Persistence and exports

### 15.1 Save and load a Run (decision B20, B22)
- Save: a button downloads `evo-run-<seed>-<mm>m<ss>s.json` through `FileBridge`. Load: a button opens the browser file picker and the chosen file replaces the current Run (the Setup screen also has a Load button).
- The file contains a `formatVersion`, the setup settings, the `SimParameters` used, the time and step count, both random number streams' full state (including the cached Gaussian), every Cell (id, position, heading, energy, age, Generation, parent id, Species id, Traits, weights, drain averages, bite timer), all Food pellets, all Toxin particles, active Events with their remaining time, all Species records with centroids, the statistics time series (decimated state included) and counters.
- Serialisation: `[System.Serializable]` data classes in `Evo.Core`, converted to JSON by the Unity layer with `JsonUtility` (arrays and plain fields only; no dictionaries, no polymorphism).
- Rule for correctness: load a file, run N steps, and the state hash must equal the state hash of the original Run continued for the same N steps. This is a required test (Section 17).
- Loading a file with a different `formatVersion` shows a plain-language error and keeps the current Run.

### 15.2 CSV export
One click downloads two files:
- `traits-over-time.csv`: one row per statistics sample with columns `time_s, mean_generation, max_generation, population, species_count, births_total, deaths_starved, deaths_eaten, deaths_poisoned, deaths_temperature, deaths_old_age`, then for each of Traits 0 to 7 the columns `<trait>_mean, <trait>_sd`.
- `species.csv`: one row per Species with `id, parent_id, name, birth_time_s, birth_generation, extinct_time_s, peak_population, current_population`.
Comma separated, `.` decimal point, header row, UTF-8, `\n` line endings.

### 15.3 Screenshot
The button captures the whole window at the end of the frame (`ScreenCapture.CaptureScreenshotAsTexture`), encodes PNG and downloads `evo-<seed>-<mm>m<ss>s.png` through `FileBridge`.

---

## 16. Build tooling and hosting

### 16.1 Unity setup
- Install Unity 6.3 LTS (the newest 6000.3.x patch) with **Web Build Support** through Unity Hub (graphical install, or the Hub or Unity command line). Sign in to Unity Hub with the Personal licence first. Pin the exact version in `ProjectSettings/ProjectVersion.txt` once the project exists.
- Create the project with the Universal 2D template (`Unity -createProject` with the URP 2D template, or the Hub), at `EvoSim/`.
- Player settings (decision B9): scripting backend IL2CPP; WebGL compression format Disabled; Decompression Fallback off; data caching on; canvas fits the window; input handler set to the new Input System; Allow downloads of unsafe code off; run in background on. Switch to Gzip with Decompression Fallback only if the build is too large for the host.

### 16.2 Scripts in `tools/` (**PROPOSED**; each exits non-zero on failure and writes a log to `Builds/logs/`)
- `unity-env.sh`: finds the editor path (`/Applications/Unity/Hub/Editor/<pinned version>/Unity.app/Contents/MacOS/Unity`) and exports it.
- `compile.sh`: `-batchmode -nographics -quit -projectPath EvoSim -logFile ... -accept-apiupdate`; fails on compile errors.
- `test.sh`: `-runTests -batchmode -nographics -projectPath EvoSim -testPlatform EditMode -testResults Builds/test-results.xml` (no `-quit`); prints a pass/fail summary from the XML.
- `build-webgl.sh`: `-batchmode -nographics -quit -projectPath EvoSim -buildTarget webgl -executeMethod Evo.Editor.BuildScript.BuildWebGL -logFile ...`; output to `Builds/WebGL/`.
- `serve.sh`: `python3 -m http.server 8000` inside `Builds/WebGL/`.
- `experiment.sh`: runs `Evo.Editor.Experiments.Run` headless (Section 17.3).
- `deploy-gh-pages.sh`: creates or updates a `git worktree` for the `gh-pages` branch, copies `Builds/WebGL/*`, adds `.nojekyll`, commits and pushes; prints the Pages URL.
If `-nographics` causes problems with a WebGL build (texture or shader import), remove it from that script.

### 16.3 Hosting on GitHub Pages (decision B10)
1. One repository, for example `natural-selection-sim`, source on `main`, built game on `gh-pages`.
2. In the repository settings, publish Pages from the `gh-pages` branch.
3. The game is served at `https://<username>.github.io/<repository>/`. The username and repository name are open items (Section 22).
4. A Unity build cannot be run by opening `index.html` from disk, so the hosted link is the main way to run it; also zip `Builds/WebGL` with a README for the submission.
5. Check the published page in Chrome and Safari after each deploy.

### 16.4 Version control
Standard Unity `.gitignore` (ignore `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`, `Builds/`), plus `Journal/` (a school submission document, kept out of the repository). Commit at the end of each work session; the commit history is extra evidence of multiple sessions.

---

## 17. Testing and tuning

### 17.1 EditMode tests (required; they run headless through `tools/test.sh`)
- `SeededRandom`: same seed gives the same sequence; different seeds differ; saving and restoring state (including the cached Gaussian) continues the same sequence; `NextFloat01` stays in [0,1).
- `PerlinNoise2D`: same seed gives the same values; outputs in 0..1; different seeds differ.
- `Genome`: mutation keeps every Trait in range and every weight in -4..4; with mutation rate 0 the copy is identical; crossover takes each value from a parent; normalise and un-normalise round-trip.
- `GeneticDistance`: distance of a Genome to itself is 0; symmetric; increases when a Trait changes.
- Colour: zero deviation gives white; the function is continuous (small deviation gives a small colour change).
- `Brain`: the forward pass is deterministic, outputs stay in -1..1 and nothing allocates.
- `SpatialHash`: query results equal a brute-force search on random data.
- Energy: a Cell with no inputs and no Food loses exactly the specified drain over a known time; temperature and Toxin drains match the formulas.
- Eating: Food gives `20 * (1 - 0.9 * diet)`; a predator needs the 1.2 size ratio and diet of at least 0.3; bite time of 0.5 s is required.
- Reproduction: parent energy plus child energy plus the 10% loss equals the energy before division; Generation is parent plus one.
- Toxin: attraction keeps two nearby particles from separating faster than the same pair without attraction; strength fades by half-life; the cap is respected.
- Events: envelope timing; weight function; Food removal and spawn rules.
- Species: two well-separated synthetic clusters give two Species with the right parent; a single cluster gives one; a group below 10 never becomes a Species; extinction is recorded.
- Statistics: sampling, decimation keeps the series length bounded and deterministic.
- Persistence: snapshot round trip gives an equal state hash; **load then run N steps equals continuing the original for N steps** (state hash equal); CSV column headers match Section 15.2.
- World generation: same Seed and settings give the same Founders, the same initial Food and Toxin and the same state hash after 200 steps.

### 17.2 Manual checks each work session
No console errors in the editor or the WebGL build; same Seed gives the same starting World; the population neither explodes nor dies out immediately; frame rate acceptable at 1x; screenshot saved; Journal entry written; commit made.

### 17.3 Headless experiment runner
`Evo.Editor.Experiments.Run(preset or seed and settings, simulatedMinutes, eventScript)` runs the core with no rendering at maximum speed and writes the statistics CSV and a Species summary to `Builds/experiments/`. Used to tune the constants quickly against the calibration targets (Section 8.10) and to produce the evidence runs in Section 20 without waiting in real time. An event script is a small text file with lines like `at 480 heatwave 0.3 90 x=80 y=45 r=30`.

---

## 18. Deliverables and Journal

Final submission: the Word document plus everything needed to run the simulation.

- [ ] Word Journal (.docx) with all entries, screenshots, experiments, final reflection and links (hosted game, repository).
- [ ] Hosted WebGL link working in Chrome and Safari.
- [ ] Zipped WebGL build with a README that gives the `python3 -m http.server` command.
- [ ] One-page PDF rulebook (same content as the in-game help).
- [ ] Optional: repository link for the source.

Journal template (a .docx in `Journal/`; Word is not installed, so create it in a tool that writes .docx and edit it in Pages or LibreOffice, then check the formatting):
- Front matter: title, name, class, dates, short project description, links.
- One entry per work session: number and date, approximate time spent; what I did (a few sentences in the user's own words); screenshots with captions; challenges; next steps; where AI helped this session (one line, decision Q24).
- Experiments section (Section 20), final reflection (what the model shows, what it simplifies, what surprised me, what I would change), and the rubric checklist (Section 5).

The user writes the entries. An assistant may offer prompts and the template.

---

## 19. Work plan

Today is Friday, October 30, 2026. Due November 16 to 17; the target finish is November 14 to 15, which leaves two spare days. Each work session ends with a screenshot for the Journal, a Journal entry written by the user, and a commit. The dates below are targets (**PROPOSED**).

| # | Target date | Goal | Done when |
|---|---|---|---|
| 1 | Fri Oct 30 | Install Unity 6.3 LTS with Web Build Support and sign in. Create the repo and project (Universal 2D). Write the `tools/` scripts. **Smoke test:** a code-built scene with a UI Toolkit button using the bundled monospaced font, a drawn icon with a hover tooltip, a `Painter2D` line chart, a generated-mesh circle and the custom shader, built to WebGL and deployed to `gh-pages`. Test the headless licence. Create the Journal. | The smoke test page loads from the GitHub Pages link in Chrome and Safari; `compile.sh`, `test.sh` and `build-webgl.sh` all work; fallbacks chosen for anything that failed. |
| 2 | Sat Oct 31 | `Evo.Core` foundations: `SeededRandom`, `PerlinNoise2D`, `SimParameters`, `Genome`, `World`, Food pellets, Toxin particles, `SpatialHash`, `EventSystem`, tests. | Tests pass; headless run produces stable Food and Toxin counts. |
| 3 | Sun Nov 1 | Cells: pool, Traits, energy, eating Food, death, asexual reproduction, mutation, a temporary rule-based steering. Unity layer: bootstrap, camera, World view, Cell, Food and Toxin meshes with eyes. | 100 to 500 Cells run for minutes on screen without exploding or dying out. |
| 4 | Mon Nov 2 | Brains, sensing, predation, innate wiring, headless experiment runner. Compare against the temporary rule. | Cells steer towards Food; predation occurs; 500 Cells hold 30 FPS. |
| 5 | Tue Nov 3 | Statistics, Trait tab charts, Generation counter, colour mapping, inspector, Stats tab, death causes. | Trait charts show change after an Event run headless; colours drift from white. |
| 6 | Wed Nov 4 | Species detection and Species tab. Setup screen, Presets, top bar, Event dialogs and placement, Temperature tint. | Setup to Run flow complete; Events place and show markers. |
| 7 | Thu Nov 5 | Tuning with the headless runner to the calibration targets (Section 8.10) on several Seeds. | Targets met on most Seeds; Toxin clouds behave. |
| 8 | Fri Nov 6 | Family tree tab, help screen, extinction message, rulebook PDF, performance check, first full deploy. | Four tabs work; hosted build works in two browsers. |
| 9 | Sat Nov 7 | CSV export, screenshot button, `FileBridge`. | Files download in Chrome and Safari. |
| 10 | Sun Nov 8 | Save and load a Run (and state-hash round-trip test). | The load-then-continue test passes. |
| 11 | Mon Nov 9 | Sexual reproduction (late feature) if time allows. | A mating Trait can evolve in and spread or fail. |
| 12 | Tue Nov 10 and Wed Nov 11 | Experiments (Section 20), Journal evidence, polish. | At least six experiments recorded with screenshots. |
| 13 | Thu Nov 12 | Final build, deploy, zip, rulebook, submission checks. | Section 18 checklist ticked. |
| Buffer | Fri Nov 13 to Sun Nov 15 | Fixes and final checks. Target finish Nov 14 to 15. | |
| Due | Mon Nov 16 and Tue Nov 17 | Submit. | |

Cut order (decision B21): if behind schedule, drop save and load first, then sexual reproduction, then the family tree, then the screenshot button, then CSV export. Presets stay. The required rubric elements (variation, pressures, differential survival, a Trait chart) are never cut. Suggested checkpoint: if the core (sessions 1 to 7) is not done by Sunday November 8, apply the cut order from the top.

---

## 20. Experiments to run and document

Run them with the headless runner (Section 17.3) or live, and put the settings, a screenshot of the relevant chart and two or three sentences using natural selection vocabulary in the Journal.
1. **Control**: a Preset with no Events for 20 simulated minutes; screenshots every 5 minutes.
2. **Heat wave**: same Seed, heat wave at minute 8; compare preferred Temperature and tolerance with the control.
3. **Ice age**: the mirror case.
4. **Toxin spill**: spill in a crowded area; watch Toxin resistance.
5. **Famine versus food bloom**: compare size, speed, sense range and reproduction threshold.
6. **Mutation rate**: the same Seed at 0.5, 1.0 and 2.0; compare how fast Species split.
7. **Different Seeds**: three Seeds with the same settings; the outcomes differ.
8. **Same Seed, different Events**: shows the Player's choices change the result.
9. **Predation emerging**: when a high-diet lineage first appears and what happens to prey Traits.
10. **If built**: whether sexual tendency spreads, and whether choosiness produces isolated groups.

---

## 21. Risks and fallbacks

| Risk | Likelihood | Mitigation |
|---|---|---|
| Species do not split by themselves | Medium | Tune mutation rate, threshold and environment variety; use the headless runner; Temperature and Toxin variety isolate regions. As a last resort, raise mutation for a demonstration and say so honestly. |
| Founders starve before evolution acts | High at first | Innate Food-seeking wiring, generous start energy, lower costs while tuning. |
| One strategy wins everywhere | Medium | Adjust Trait costs (Section 7.1). |
| Population explodes or collapses | Medium | Tune Food spawn and costs together; hard cap 600; extinction message. |
| WebGL performance | Medium | Lower the cap, shrink the hidden layer, sense every second step. |
| Unity licence does not work in batch mode (undocumented for Personal) | Medium | Test in work session 1. If it fails: run the editor-based loop with MCP, or have the user run the build from the editor, and keep the scripts for the machine where it works. |
| Editor and command line both open the same project | Medium | Close the editor before running `tools/` scripts; the agent should check that the editor is not running. |
| UI Toolkit or `Painter2D` fails or is heavy in WebGL (no explicit support statement found) | Medium | Smoke test in session 1. Fallback: uGUI with generated meshes for charts. |
| Custom shader stripped or not drawn in URP 2D | Medium | Reference through `RenderAssets` in `Resources`; no `LightMode` tag; smoke test. Fallback: Sprites/Default through an Always Included Shaders entry. |
| Input System with UI Toolkit needs the right module and actions | Medium | Create the `EventSystem` and assign default actions in code; smoke test mouse, wheel and keys in the browser. |
| GitHub Pages and Unity files | Medium | Compression Disabled; test in session 1. Fallback Gzip with Decompression Fallback. |
| Scope too large for the dates | High | Cut order (Section 19). |
| Journal is thin though the game works | Medium | Write each entry the same day. |
| Teacher cannot open the game | Low | Link, zipped build, screenshots. |

---

## 22. Open items and proposed defaults to review

### Open items
1. GitHub username and repository name (default repository name `natural-selection-sim`).
2. Whether the teacher wants a specific way of crediting AI help. Decision Q24 stands: entries are in the user's words and note where AI helped.
3. Which Unity MCP server, if any, will be used (optional by decision B8).
4. The exact latest 6000.3.x patch (read from Unity Hub when installing).
5. Whether the Personal licence works headless (tested in session 1).

### Proposed defaults the user has not seen yet (veto any of these)
- World size 160 by 90 units; walls on all sides; 120 Founders; 600 Cell cap.
- The ten Traits and their ranges; Cell radius `0.5 * size`; no collisions between Cells; 360 degree vision; direct (inertia-free) movement.
- 11 Brain inputs, 6 hidden neurons, 4 outputs; innate wiring for Food only.
- All energy, Food and Toxin numbers; ageing after 300 s.
- Presets: Mild world, Cold gradient, Toxic basin and their values.
- The design-system defaults listed in `DESIGN.md` Section 11; the controls in Section 12.4.
- Eyes: two small dark eyes at the front, fixed in proportion to the body, cosmetic only.
- Trait bins (6 bins over the window around the founder mean); Species chart shows the 20 largest.
- Species distance weights and threshold; no merging of Species.
- Event slider ranges; up to 8 active Events; a Whole World checkbox.
- Journal kept out of the repository.
- Session dates in Section 19.

---

## 23. Out of scope (explicitly not chosen)

- A grid, tiles or squares anywhere in the model (decision B11).
- Physical game or printable version; map painting; slider-only worlds.
- Player-controlled predators or breeding.
- Scored levels, challenges, win or lose conditions.
- Real species (moths, finches) and hand-designed creatures.
- NEAT-style evolving network structure; brain-only Genomes.
- Multicellular clumping or colonies.
- Meteor or mass-extinction Event.
- Mobile phones and tablets.
- Godot, JavaScript game engines, Vercel and Cloudflare Pages hosting.
- Flashy visual effects of any kind.
- Several thousand Cells.
- DOTS, ECS or Burst jobs.

---

## 24. Notes for the building agent

- Read Sections 0, 4 and 6 to 17 before writing code. Use the `CONTEXT.md` words in code names, UI text and documentation.
- Start with work session 1: prove the toolchain (install, headless compile, headless test, headless WebGL build, GitHub Pages deploy, UI Toolkit with `Painter2D`, the custom shader in the URP 2D renderer, the input module) before building features on top of it. Record what you find in the Journal prompts for the user.
- Keep every tunable in `SimParameters`. Keep `TraitCount`, `BrainLayout` sizes and the statistics layout as constants used everywhere.
- Never use `UnityEngine.Random`, `System.Random`, `DateTime` or any other nondeterministic input in `Evo.Core`. The same Seed and the same sequence of Player inputs must give the same state hash.
- Never allocate in the per-step or per-frame loops.
- Close the Unity editor before running command-line builds or tests on the same project; check that it is not running.
- Do not write the user's Journal entries. Offer prompts and the template; the user writes the text.
- Keep the visual style flat and plain (Q26).
- When a decision is missing, ask the user a short multiple-choice question instead of guessing, unless it is a small implementation detail already marked PROPOSED.
- At the end of each work session remind the user to take a screenshot, write the Journal entry and commit.
