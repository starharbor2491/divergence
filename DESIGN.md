# Divergence Design System

Version 2, written Thursday, November 12, 2026. This file defines the visual identity, Liquid Glass material system, typography, World rendering, layout, and classroom UX of **Divergence**. `PLAN.md` defines core simulation rules; `CONTEXT.md` fixes the scientific vocabulary; `PRODUCT.md` records product truth.

---

## 1. Design Direction — *Living Tide-Pool & Apple Liquid Glass*

- **Subject & Scene**: A living microscopic aquatic ecosystem observed on classroom laptops and projectors through weightless, refractive **Liquid Glass** instruments.
- **Character**: Luminous, organic, tactile, crystal-clear.
- **The Bright Moment**: The full-bleed bioluminescent World itself — translucent multi-layered Cells with membranes, nuclei, undulating flagella, and sense/birth ripples swimming through thermal currents, glowing emerald phytoplankton Food, and drifting rose-magenta Toxin clouds.
- **Quiet, Refractive Chrome**: Every UI surface floats over the live World with real downsampled backdrop blur and curved-edge refraction (`GlassBackdrop`), tinted marine-glass fills, a directional top-left specular meniscus rim, and subtle bottom-right bioluminescent bounce light.
- **Signature Detail**: Live **Field Notes** pill + curved-edge optical glass refraction over swimming Cells, translating raw population shifts into plain-English natural selection observations as they happen.

---

## 2. Foundations & Tokens

All colours, spacing, radii, and type sizes live in `EvoSim/Assets/Scripts/UnityLayer/UI/Theme/DesignTokens.cs` and generate `Assets/UI/Theme/tokens.uss` (`--evo-<kebab-case>`). Components never hardcode literals outside `DesignTokens` and procedural shaders/textures derived from it.

### 2.1 Reference Size & Responsive Floor
- **Reference resolution**: `1920 × 1080`, `PanelScaleMode.ScaleWithScreenSize`, `match = 0.5`.
- **Minimum supported window**: `1280 × 720` CSS pixels (`Screen / DevicePixelRatio`). Below that, the full-window Liquid Glass size notice takes priority over all popups.

### 2.2 Colour System

#### Deep Aquatic Ground & Liquid Glass Surfaces
| Token | Value | Role |
|---|---|---|
| `bg-0` | `#061019` | Window clear colour & abyssal outer water |
| `bg-1` | `rgba(12, 25, 38, 0.58)` | Primary floating Liquid Glass panels (TopBar, SidePanel, EventDock, SetupCard) |
| `bg-2` | `rgba(24, 45, 66, 0.56)` | Recessed/nested glass wells, secondary pills, inputs, chart surfaces |
| `bg-3` | `rgba(42, 74, 104, 0.68)` | Hover & active segment glass fill |
| `bg-4` | `rgba(58, 98, 134, 0.78)` | Pressed control glass fill |
| `glass-modal` | `rgba(10, 22, 34, 0.84)` | Elevated modal dialogs, dropdown menus, tooltips, Help card (high-legibility frost) |
| `glass-rim-top` | `rgba(255, 255, 255, 0.42)` | Top-left specular meniscus highlight on Liquid Glass |
| `glass-rim-side` | `rgba(186, 230, 253, 0.14)` | Mid-perimeter refractive glass hairline |
| `glass-rim-bottom` | `rgba(94, 234, 212, 0.22)` | Bottom-right bioluminescent water bounce rim |
| `glass-sheen` | `rgba(255, 255, 255, 0.06)` | Soft upper-curvature specular sheen inside glass panels |
| `glass-shadow` | `rgba(2, 8, 15, 0.42)` | Soft offset elevation shadow beneath floating glass |
| `scrim` | `rgba(4, 10, 16, 0.62)` | Dimming veil behind modal dialogs and Help |
| `border` | `rgba(165, 210, 240, 0.14)` | Quiet internal hairlines & dividers |
| `border-strong` | `rgba(205, 235, 255, 0.28)` | Interactive control boundaries |

#### Text & Bioluminescent Accent (WCAG AA+ Verified)
| Token | Value | Role |
|---|---|---|
| `text-1` | `#F2F8FD` | Primary text, headings, live figures |
| `text-2` | `#BDD0E2` | Secondary labels, descriptions, resting icons |
| `text-3` | `#8CA7C0` | Captions, axis ticks, keyboard hints |
| `text-disabled` | `#566E84` | Disabled labels and icons |
| `accent` | `#36DFC4` | Bioluminescent seafoam primary action, slider fill, active indicators |
| `accent-pressed` | `#24BFA6` | Pressed primary fill |
| `accent-text` | `#7CF5E0` | Luminous seafoam text/icons on dark glass |
| `on-accent` | `#04201C` | Deep ink on `accent` buttons (11.2:1 contrast) |
| `focus` | `#7CF5E0` | 2 px keyboard/input focus ring |

#### Status & Semantic Colours
| Token | Value | Role |
|---|---|---|
| `status-good` | `#34D399` | Positive confirmation (save/export requested, thriving note) |
| `status-warning` | `#FBBF24` | Caution (capacity limit, window too small) |
| `status-serious` | `#FB923C` | Recoverable file/input error |
| `status-critical` | `#F87171` | Extinction / terminal state |

#### Living World & Chart Data Colours
| Token | Value | Role |
|---|---|---|
| `world-bg` | `#081622` | Tide-pool / petri-dish interior water |
| `world-substrate` | `#10283B` | Subtle caustic & microscopic depth rings |
| `world-wall` | `#4C7A9E` | Luminous containment boundary of the World |
| `world-food` | `#5CE6A0` | Bioluminescent phytoplankton Food pellets |
| `world-toxin` | `#F05A8E` | Drifting Toxin spores & haze |
| `world-selection` | `#FFFFFF` | Crisp selection ring around inspected Cell |
| `world-sense` | `#7CF5E0` | Subtle sonar ring showing inspected Cell's sense range |
| `world-eye` | `#09131C` | Cell pupil |
| `world-preview` | `#7CF5E0` | Dashed Event placement preview ring |
| `temp-cold` / `temp-mid` / `temp-hot` | `#3898F5` / `#173247` / `#F26457` | OKLab-interpolated thermal water currents |
| `chart-series-1` | `#36DFC4` | Trait mean curve & 20% SD band |
| `bin-1` .. `bin-6` | `#114B5F`, `#1A6B7C`, `#228B94`, `#2EB0AC`, `#5FD4C4`, `#B5F5EC` | Monotone lightness seafoam-teal ramp for Trait frequency bins |
| `cause-eaten` .. `cause-poisoned` | `#38BDF8`, `#FB7185`, `#34D399`, `#FBBF24`, `#E879F9` | High-contrast colour-blind-safe death causes |

---

## 3. Typography — Humanist UI + Precision Telemetry

- **UI & Prose Face (`Plus Jakarta Sans`, SIL OFL 1.1)**: Bundled in `Assets/Fonts/` (`PlusJakartaSans-Regular.ttf`, `Medium`, `SemiBold`, `Bold`). Used for wordmark, headings, Preset cards, buttons, tabs, tooltips, Field Notes, and Help prose so non-technical students and teachers read warm, friendly language.
- **Data & Telemetry Face (`JetBrains Mono`, SIL OFL 1.1)**: Bundled in `Assets/Fonts/` (`JetBrainsMono-Regular.ttf`, `Medium`, `Bold`). Reserved exclusively for live counters, timecodes, Generation numbers, slider readouts, Seed codes, and chart axis numerals so numbers never jitter.
- **Rules**: Sentence case everywhere (never tracked ALL-CAPS eyebrows). Line length under 70 characters in Help and tooltips.

| Token | Size / Line | Family & Weight | Role |
|---|---|---|---|
| `type-display` | 44 / 52 | Plus Jakarta Sans Bold | Setup wordmark |
| `type-heading` | 26 / 34 | Plus Jakarta Sans Bold | Dialog/Help/Extinction headings |
| `type-title` | 18 / 25 | Plus Jakarta Sans Bold | Section & card titles |
| `type-body` | 15 / 22 | Plus Jakarta Sans Regular | Explanations, descriptions, prose |
| `type-label` | 14 / 20 | Plus Jakarta Sans Medium/Bold | Button labels, tabs, control names |
| `type-mono` | 15 / 22 | JetBrains Mono Medium | Numeric readouts, Seed text, telemetry |
| `type-caption` | 12 / 17 | Plus Jakarta Sans Regular (Mono for ticks) | Captions, axis ticks, hints |

---

## 4. Liquid Glass Architecture

1. **Real-Time Optical Backdrop (`GlassBackdrop` + `Evo/LiquidGlassBackdrop` shader)**:
   - A secondary orthographic camera (`depth = -10`) renders the `World` layer at 1/3 resolution into a bilinear `RenderTexture`.
   - `GlassBackdrop` renders rounded-rectangle SDF lenses just under `UIDocument` for every active `LiquidGlassSurface`, applying a smooth 13-tap disc blur in the interior and outward normal-vector lens refraction + subtle dispersion along the curved bevel.
2. **Specular Meniscus Rim (`LiquidGlass.Attach`)**:
   - `Painter2D` draws a soft outer drop shadow, an upper curved specular sheen, and a continuous rounded border whose stroke transitions from bright top-left specular (`glass-rim-top`) to soft side refraction (`glass-rim-side`) and seafoam bottom-right water bounce (`glass-rim-bottom`).
3. **Nested Continuous Radii**:
   - Floating panels & Setup card: `22 px` (`RadiusPanel`).
   - Cards, dialogs, dropdowns & Inspector: `16 px` (`RadiusCard`).
   - Controls, buttons, inputs & Preset tiles: `12 px` (`Radius`), or full capsule (`20 px` = `RadiusPill`) on floating transport/Event pills.

---

## 5. Living World Visuals (`WorldView`)

Purely visual enhancements in `Evo.UnityLayer` — `Evo.Core` simulation math, RNG streams, and snapshots remain 100% deterministic and compatible:
- **Full-Bleed Tide-Pool**: The camera renders across 100% of the window (`Rect(0,0,1,1)`), framing the `160 × 90` World cleanly inside the open stage while deep water extends beneath every floating glass panel.
- **Ambient Thermal Currents**: Gentle subtle thermal water tint is visible at low ambient strength even before toggling high-contrast **Thermal Map**, so cold/warm regions are intuitively legible; toggling **Thermal Map** boosts opacity and shows the `cold — hot` legend pill.
- **Multi-Layer Organic Cells**:
  - Soft bioluminescent energy aura when well-fed.
  - Translucent cell membrane + inner cytoplasm (OKLCH Genome colour).
  - Distinct inner nucleus/organelle (emerald tint for grazers, warm amber-coral nucleus + forward mandible cue for high-diet predators).
  - Gentle undulating flagellum tail driven by speed and heading.
  - Crisp directional eyes, mitosis/birth ripple rings, and an inspected Cell **Sense Range** sonar circle.
- **Active Event Zones**: Active Heat waves, Ice ages, Famines, and Food blooms render breathing environmental boundary rings and localized tint in the World until they fade.

---

## 6. Classroom-First Screens & Flows

1. **Setup Screen (Over Live Ambient World)**:
   - Runs a live background preview World (`previewSim`, separate from `AppController.Sim`) that updates immediately when choosing a Preset!
   - Asymmetric two-column Liquid Glass card:
     - **Left**: Wordmark, plain-English 2-sentence overview of natural selection, and clickable **Preset Cards** (*Mild world*, *Cold gradient*, *Toxic basin*, *Rapid evolution*) with clear badges explaining what students will witness.
     - **Right**: Seed field with randomizer button, five environment sliders with intuitive descriptions, and action bar (**Start Run**, **Load Saved Run**, **How It Works**).
2. **Run HUD (Floating Liquid Glass Capsules)**:
   - **Top Bar**: Labeled **Pause/Resume** pill, clear **`1× 2× 5× 10×`** speed selector, live **Time · Generation · Cells · Species** counters, **Thermal Map** toggle, **Save / Load / CSV / Photo**, **Guide**, **Charts**, **Menu**.
   - **Bottom Quick-Event Dock**: Floating Liquid Glass dock with 1-click buttons for **Heat wave**, **Ice age**, **Famine**, **Food bloom**, and **Toxin spill** so anyone can test environmental Pressures immediately.
   - **Live Field Notes (`LiveNarrator`)**: Top-left floating glass pill reporting real-time evolutionary events (Founder start, placed Events, significant Trait adaptations, Species splits, predation shifts).
   - **Cell Inspector**: Floating glass card with live Cell portrait, plain-English lifestyle summary, Energy progress bar, Lineage metadata, Trait bars with Founder ticks, and live bidirectional Brain activity bars (`Turn`, `Swim`, `Bite`, `Mate`).
   - **Floating Side Panel**: Detached glass panel (`Traits`, `Species`, `Tree`, `Stats`) with luminous charts, interactive Event chips, and click-to-highlight Species legends.

---

## 8. Copy (written once, used for tooltips, the Help screen and the rulebook)

Style: plain sentences, present tense, no exclamation marks, no emoji, units given where they exist. A tooltip is a name plus one or two sentences. The strings below are the specification; keep them in one `UiText` table keyed by id.

### Controls
| Id | Name | Text |
|---|---|---|
| `play` | Resume | Resumes the simulation. Key: Space. |
| `pause` | Pause | Freezes the simulation so you can pan, zoom and inspect Cells. Key: Space. |
| `speed-1` | Speed 1x | Runs the simulation at normal real-time speed. Key: 1. |
| `speed-2` | Speed 2x | Runs at twice normal speed. Key: 2. |
| `speed-3` | Speed 5x | Runs at five times normal speed. Key: 3. |
| `speed-4` | Speed 10x | Runs at ten times normal speed. On slow computers the real speed may be lower. Key: 4. |
| `events` | Events | Places a heat wave, ice age, famine, food bloom or toxin spill in the World. |
| `temperature-tint` | Thermal map | Boosts the colour overlay for Temperature: blue is cold, coral-red is hot. Key: T. |
| `fit-view` | Fit World | Centres and zooms the camera to fit the whole World inside the stage. Key: F. |
| `field-notes` | Field notes | Plain-English observations of natural selection, placed Events and Species splits as they happen. |
| `save` | Save run | Requests a download of this Run for loading later. Check your browser's downloads before discarding it. |
| `load` | Load run | Opens a saved Run file. A valid file replaces the current Run only after confirmation. |
| `csv` | Export CSV | Requests two CSV downloads for recorded Trait and Species data. Your browser may ask permission for multiple downloads. |
| `screenshot` | Screenshot | Requests a PNG download of the window. Check your browser's downloads for the file. |
| `help` | How it works | Opens the illustrated guide to natural selection, Traits and controls. |
| `panel` | Charts panel | Shows or hides the floating charts panel. Key: Tab. |
| `home` | Menu | Opens a confirmation before returning to setup and discarding this Run. |
| `random-seed` | New random seed | Fills the Seed field with a random code. |
| `restart` | Restart | Opens a confirmation before replacing this Run with a new one using the same Seed and settings. |
| `close` | Close | Closes this window. Key: Escape. |
| `inspector-close` | Close | Stops inspecting this Cell. Key: Escape. |
| `confirm-cancel` | Cancel | Keeps the current Run without discarding or replacing it. Key: Escape. |
| `confirm-discard` | Discard | Discards the current Run and continues the chosen action. Check that any requested save is in your browser's downloads first. |
| `confirm-save` | Save | Requests a Run download and keeps the current Run. Check your browser's downloads before choosing Discard; the app cannot confirm a disk write. |

### Setup
| Id | Name | Text |
|---|---|---|
| `preset` | Preset | A ready-made set of settings and a Seed. Changing any setting afterwards switches to Custom. |
| `preset-mild` | Mild world | Gentle Temperature variation and steady Food. Ideal first Run to watch gradual Mutation and colour drift. |
| `preset-gradient` | Cold gradient | Strong contrast between cold and hot regions. Watch Cells adapt different preferred Temperatures by zone. |
| `preset-basin` | Toxic basin | Dense drifting Toxin clouds across the World. Creates strong Pressure for higher Toxin resistance. |
| `preset-rapid` | Rapid evolution | Abundant Food, sharp Temperature zones and double mutation rate for fast Trait shifts and Species splits. |
| `seed` | Seed | A code that fixes the World. The same Seed and settings always start the same World. |
| `food-abundance` | Food abundance | How fast Food appears. Higher values support more Cells. Range 0.20 to 2.00. |
| `temperature-range` | Temperature range | How different the coldest and hottest parts of the World are. 0 is uniform, 1 is the strongest contrast. |
| `zone-scale` | Zone scale | How large the warm and cold zones are. Lower values make large zones, higher values make smaller pockets. |
| `toxin-level` | Toxin level | How much Toxin the World starts with. 0 means none. |
| `mutation-rate` | Mutation rate | How much Traits and Brains change when a Cell is born. 1.00 is normal. |
| `start` | Start run | Starts a new Run in this World. |

### Tabs and charts
| Id | Name | Text |
|---|---|---|
| `tab-traits` | Traits | Shows how one Trait changes in the living population over time or Generations. |
| `tab-species` | Species | Shows how many Cells belong to each Species over time. |
| `tab-tree` | Tree | Shows which Species split from which, and when. |
| `tab-stats` | Stats | Shows live counts and why Cells die. |
| `trait-select` | Trait | Chooses which Trait the charts show. |
| `axis-switch` | X axis | Plots time in seconds, or the actual mean Generation of living Cells. The mean can decrease as the population changes, so the line can turn back. |
| `chart-trait-mean` | Mean and spread | The line is the average value of the Trait; the shaded band is one standard deviation either side. The dashed line is the Founders' starting average. |
| `chart-trait-frequency` | Frequency | Shows what share of Cells has each range of the Trait. Darker shades are lower values. |
| `chart-species` | Species over time | Band height is population for each of the twenty Species with highest historical peaks. Other combines the rest; hover identifies the plotted layers. |
| `chart-tree` | Family tree | Each line is a Species from when it first appeared to when it died out. Curved branches show where a new Species split off. |
| `chart-deaths` | Deaths by cause | Shows deaths by cause in 10-second bins from the latest ten minutes. Legend totals count deaths over the whole Run. |
| `event-marker` | Event marker | Marks when an Event happened. H heat wave, I ice age, F famine, B food bloom, T toxin spill. |

### Events
| Id | Name | Text |
|---|---|---|
| `event-heat-wave` | Heat wave | Raises Temperature inside the circle, then lets it return to normal. |
| `event-ice-age` | Ice age | Lowers Temperature inside the circle, then lets it return to normal. |
| `event-famine` | Famine | Removes most Food inside the circle and slows new Food there for a while. |
| `event-food-bloom` | Food bloom | Adds extra Food inside the circle for a while. |
| `event-toxin-spill` | Toxin spill | Releases Toxin inside the circle. It spreads slowly and fades. |
| `event-radius` | Radius | Radius in World units. For a toxin spill it also sets the requested particle count; Whole World changes placement, not quantity. |
| `event-toxin-amount` | Toxin quantity | By default, radius requests 30 Toxin particles per World unit: 150 to 1800 for radius 5 to 60. Remaining global capacity can reduce the actual count; Whole World changes positions, not quantity. |
| `event-intensity` | Intensity | How far Temperature moves from normal at the centre. |
| `event-duration` | Duration | How long the Event lasts at full strength, in seconds. |
| `event-whole-world` | Whole World | Applies the Event everywhere instead of in one place. |
| `event-place` | Place | Closes this window. Then click the World to place the Event. |
| `event-apply` | Apply | Applies the Event to the whole World at once. |
| `event-cancel` | Cancel | Closes this window without placing an Event. Key: Escape. |

### Trait definitions
`UiText.TraitIds` lists these ten ids in Trait index order (0 to 9). `UiText.TraitTip(int trait)` returns the title and shared meaning, units and trade-off; an invalid index uses `inspect-trait`.

| Id | Name | Text |
|---|---|---|
| `trait-size` | Size | Relative size, 0.50 to 3.00; radius is half this value in World units. Larger Cells store more energy and can eat smaller Cells, but cost more energy to sustain and move. |
| `trait-speed` | Speed | Maximum movement speed, 0.50 to 4.00 World units per second. Faster movement helps reach Food or escape, but movement energy cost rises with speed squared. |
| `trait-sense` | Sense range | Detection radius for Food, Cells and Toxin, 2.00 to 15.00 World units. A wider range finds things farther away but increases baseline energy cost. |
| `trait-temperature` | Preferred Temperature | Best-suited Temperature on the 0.00 cold to 1.00 hot scale. Matching the local Temperature avoids stress; mismatches beyond tolerance cost energy. |
| `trait-tolerance` | Temperature tolerance | Allowed Temperature mismatch without stress, 0.05 to 0.50 on the Temperature scale. Wider tolerance reduces stress in more places but raises baseline energy cost. |
| `trait-resistance` | Toxin resistance | Unitless resistance, 0.00 to 1.00, reducing Toxin energy drain by up to 90%. Higher resistance protects against Toxin but raises baseline energy cost. |
| `trait-diet` | Diet | Unitless diet, 0.00 to 1.00: low values favour Food pellets, high values favour eating smaller Cells. Higher values gain more energy from prey but less from Food; predation also needs sufficient size and biting. |
| `trait-reproduction` | Reproduction threshold | Energy needed to initiate reproduction, 0.50 to 0.95 of maximum energy. A lower threshold allows earlier reproduction but leaves less energy to pass to offspring. |
| `trait-sexual` | Sexual tendency | Unitless tendency, 0.00 to 1.00: below 0.50 divides alone; at least 0.50 requires a sexual partner. Crossover combines two Genomes, but both parents pay energy and a Cell without a compatible partner waits. |
| `trait-choosiness` | Choosiness | Unitless partner selectivity, 0.00 to 1.00, used by sexual Cells. Higher values accept only closer Genomes, preserving similarity but reducing the number of compatible partners. |

### Inspector and legend
| Id | Name | Text |
|---|---|---|
| `inspect-energy` | Energy | The Cell's stored energy and its maximum. It dies at zero. |
| `inspect-age` | Age | Time since the Cell was born. Old Cells lose extra energy. |
| `inspect-generation` | Generation | Birth depth from a Founder, whose Generation is zero. A sexual child's Generation is one more than the higher parent Generation. |
| `inspect-parent` | Parent ID | The recorded parent's ID, not a Generation. 'none' means a Founder; sexual births record only the initiating parent's ID. |
| `inspect-trait` | Trait bar | The Trait value inside its allowed range. The tick marks the Founders' average. |
| `inspect-brain` | Brain activity | What the Cell's Brain is telling it to do right now: steer left or right, swim forward, bite prey, or court a partner. |
| `legend-species` | Species | Click to spotlight this Species' Cells in the World. Click again to clear. |
| `legend-other` | Other | The chart draws the twenty Species with highest peak populations over the Run; all others form Other. Individual living Species rows still highlight their Cells, including Species inside Other. |
| `legend-cause` | Cause of death | This cause's legend count covers the whole Run, not just the latest ten minutes shown in the chart. |
| `legend-bin` | Trait range | The range of Trait values that this shade covers. |
| `stat-readout` | Live count | A live count taken from the current Run. |
| `stat-sexual-share` | Sexual share | Share of living Cells whose sexual tendency is at least 0.50. This is not the share of births from sexual reproduction. |
| `temperature-legend` | Temperature legend | Blue is cold and coral-red is hot. The thermal map is drawn over the World. |

### Download notifications
A request is not confirmation that a file reached disk. These `UiText` constants are shared message copy.

| Constant | Text |
|---|---|
| `DownloadRequested` | Download requested. Check your browser's downloads. |
| `CsvDownloadRequested` | Two CSV downloads requested. Your browser may ask permission for multiple downloads. |

### Help screen section text (short form; the same sentences go into the rulebook)
1. **What this is.** Divergence is a model of natural selection. Cells live in a World with Food, Toxin and varying Temperature. Cells that suit the World survive and reproduce, so their Traits become more common over generations. Over time the population can split into Species.
2. **Controls.** Drag to move the view. Scroll to zoom. Click a Cell to inspect it and see its sense range. Space pauses. Keys 1 to 4 change speed. F fits the World. T boosts the thermal map. Tab hides the charts panel. Escape cancels or closes.
3. **Settings.** The Seed fixes the World. Food abundance, Temperature range, zone scale, Toxin level and mutation rate shape it. Presets fill these in. The same Seed and settings always make the same starting World.
4. **The World.** Temperature varies smoothly from cold blue currents to hot coral zones. Food appears as glowing green pellets, more in some places than others. Toxin is a set of pink particles that drift, attract each other slightly and fade.
5. **Traits.** Each Cell inherits ten Traits: size, speed, sense range, preferred Temperature, Temperature tolerance, Toxin resistance, diet, reproduction threshold, sexual tendency and choosiness. Traits have benefits and trade-offs, so no Cell is best everywhere. Hover a Trait for its meaning, units and trade-off.
6. **Reproduction.** Below sexual tendency 0.50, a Cell divides at its energy threshold. At 0.50 or above, it needs a sexual partner within sense range with enough energy and mutual acceptance under both Cells' choosiness. Both pay energy; crossover takes each Trait and Brain weight from either parent, then mutation can make small random changes. Without a partner it waits; it never divides alone.
7. **Species and colours.** Cells begin white. A Cell's colour comes from its Genome, so similar Cells have similar colours. Grazers show a green inner core while high-diet hunters grow warm amber cores and front mandibles. A Species is a group of Cells whose Genomes are close together; a new Species appears when a group drifts far enough apart.
8. **Charts.** Traits shows one Trait over time or mean Generation, with a dashed line marking the Founders' starting average. Species shows the twenty Species with highest historical peaks; the rest form Other. Tree shows which Species split from which. Stats shows deaths in the latest ten minutes; legend totals cover the whole Run. Sexual share counts living Cells with sexual tendency at least 0.50, not births.
9. **Events.** Events change the World while a Run is going. Pick an Event from the bottom dock, set its size, then click the World to place it. Whole World applies without placement; for toxin spills, radius still sets quantity.
10. **Save and exports.** Save requests a Run download. Load restores a valid saved file. Export CSV requests two data files; your browser may ask permission for multiple downloads. Screenshot requests a PNG. Check your browser's downloads; the app cannot confirm files reached disk.

---

## 9. Implementation in Unity

- **Token source of truth**: `Assets/Scripts/UnityLayer/UI/Theme/DesignTokens.cs` holds every colour, spacing, radius, and type token as constants, and `Evo.Editor.GenerateTokensUss` writes `Assets/UI/Theme/tokens.uss`.
- **Fonts**: `PlusJakartaSans` (`Regular`, `Medium`, `SemiBold`, `Bold`) for UI & prose, and `JetBrainsMono` (`Regular`, `Medium`, `Bold`) for live telemetry, numeric readouts, Seed inputs, and chart axes, linked through `RenderAssets`.
- **Liquid Glass**: `LiquidGlass.cs` paints directional specular meniscus rims, upper liquid sheen, and soft shadows via `Painter2D` while `GlassBackdrop.cs` + `Assets/Shaders/LiquidGlassBackdrop.shader` render real-time downsampled backdrop blur and curved-edge normal-vector refraction over the live World.
- **Living World & Field Notes**: `WorldView.cs` draws full-bleed aquatic caustics, ambient/full thermal currents, active Event zones, multi-layer Cells (aura, undulating flagellum, translucent membrane, diet-coded organelle, eyes), birth ripples, and Sense Range sonar rings; `LiveNarrator.cs` translates simulation milestones into plain-English Field Notes.
