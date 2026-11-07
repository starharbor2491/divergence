# Divergence Design System

Version 1, written Friday, October 30, 2026. This file defines how Divergence looks, reads and behaves. `PLAN.md` says what the program does; this file says what it looks like. `CONTEXT.md` fixes the vocabulary. When this file and `PLAN.md` disagree about appearance, this file wins; for behaviour, `PLAN.md` wins.

## 0. How to use this file

- The design is flat, dark, monospaced and plain. The user asked for "a simulation, not a product demo" with "no flashy graphics or other over the top vfx". Nothing in the interface glows, casts shadows, animates or decorates.
- Every value here is a token. Build components from tokens only. Do not write a colour, size or font into a component.
- The UI is built with Unity UI Toolkit, written in code, drawn at a 1920 by 1080 reference size (Section 2.1). All pixel values below are at that reference size.
- Items marked **PROPOSED** were not put to the user. Section 11 lists them. Everything else follows a decision in Section 12.
- Use the words in `CONTEXT.md` in all interface text: Cell, Trait, Genome, Species, Generation, Food, Toxin, Temperature, Event, Preset, Run, World, Seed.

---

## 1. Principles

1. **Instrument, not product.** The interface should look like lab software: precise, quiet, every element earns its place.
2. **Readable at a glance.** Numbers use a monospaced face, labels sit next to values, and the World stays visually dominant.
3. **Colour carries data, not decoration.** The only saturated colours on screen are Cells, Species in charts, Toxin, and the Temperature tint. The interface chrome is neutral.
4. **Everything explains itself.** Every control and chart has a hover explanation (Section 8). The same text feeds the Help screen and the rulebook.
5. **One word per thing.** Use the CONTEXT.md terms. Never "gene", "tile", "square", "organism" or "creature".
6. **No motion.** State changes are instant (Section 2.7).

---

## 2. Foundations

### 2.1 Reference size and scaling
- Design size: 1920 by 1080. Panel scale mode: scale with screen size, reference resolution 1920 by 1080, match 0.5 (width and height blended).
- Minimum supported window: 1280 by 720. Below that, show a plain full-window message "Window too small. Enlarge the window to at least 1280 by 720." with the warning icon.
- At the minimum window size the text renders at about two thirds of its design size. The comfortable density (Section 12, D7) is chosen so that text stays readable there.

### 2.2 Colour tokens

All colour values are hex. Names are the token names; the C# constant is `DesignTokens.<PascalCase>`; the USS custom property is `--evo-<kebab-case>`.

#### Neutrals (surfaces, lines, text)

| Token | Value | Use |
|---|---|---|
| `bg-0` | `#0E1116` | Window background, World background |
| `bg-1` | `#171B22` | Side panel, top bar, setup card, dialogs, chart surface |
| `bg-2` | `#1F2530` | Inputs, buttons (secondary), cards inside panels, tooltips |
| `bg-3` | `#2A313D` | Hover state of controls |
| `bg-4` | `#343C4A` | Pressed state of controls |
| `scrim` | `#0E1116` at 70% opacity | Behind dialogs and the Help screen |
| `border` | `#2C3440` | Decorative hairlines between regions |
| `border-strong` | `#6B7789` | Boundary of any control (inputs, buttons, sliders, dropdowns) |
| `text-1` | `#E6EAF0` | Primary text, values |
| `text-2` | `#A9B2C0` | Secondary text, labels, icons at rest |
| `text-3` | `#949FB0` | Muted text, axis labels, hints |
| `text-disabled` | `#5F6A7A` | Disabled text and icons (exempt from contrast rules) |

#### Accent and focus

| Token | Value | Use |
|---|---|---|
| `accent` | `#5E9CF0` | Primary button fill, active toggle, selected tab underline, slider fill, focus-adjacent highlights |
| `accent-pressed` | `#4A86DA` | Pressed state of the primary button |
| `accent-text` | `#8DB9F5` | Accent-coloured text and icons on dark surfaces |
| `on-accent` | `#0E1116` | Text and icons on an `accent` fill |
| `focus` | `#8DB9F5` | 2 px outline on a focused input, dropdown or slider |

#### Status (fixed meaning, always an icon plus a label)

| Token | Value | Use |
|---|---|---|
| `status-good` | `#0CA30C` | Success (for example "Run loaded") |
| `status-warning` | `#FAB219` | Caution (for example window too small) |
| `status-serious` | `#EC835A` | Recoverable error (for example a save file that cannot be loaded) |
| `status-critical` | `#D03B3B` | Terminal state (for example "Population extinct") |

Status colours are used only for status icons and status accents. Status text uses `text-1`.

#### World colours

| Token | Value | Use |
|---|---|---|
| `world-bg` | `#0E1116` (same as `bg-0`) | World background |
| `world-wall` | `#3A4352` | 1 px outline of the World edges |
| `world-food` | `#6FD6A8` | Food pellets |
| `world-toxin` | `#D55181` | Toxin particles; alpha = `0.2 + 0.6 * strength` (strength 0..1) |
| `world-selection` | `#FFFFFF` | 1.5 px ring around the selected Cell |
| `world-eye` | `#14181F` | The two eyes on each Cell |
| `world-preview` | `#5E9CF0` | Dashed 2 px circle that previews an Event's radius |

Cells are coloured by their Genome (Section 7), not by a token.

#### Temperature tint
A quad over the World, drawn at 30% opacity. Colour stops (interpolate in OKLab):

| Temperature | Colour |
|---|---|
| 0.0 (cold) | `#3987E5` |
| 0.5 | `#4A5565` |
| 1.0 (hot) | `#E66767` |

This is the diverging pair blue and red with a neutral midpoint. When the tint is on, a small legend (a 160 by 8 px gradient bar with the labels "cold" and "hot" in `text-3`, 13 px) sits at the bottom right of the World view.

#### Chart colours

| Token | Value | Use |
|---|---|---|
| `chart-series-1` | `#3987E5` | The single mean line on the Traits tab; the standard-deviation band is the same colour at 20% opacity |
| `chart-grid` | `#232A34` | Horizontal gridlines, 1 px |
| `chart-axis` | `#3A4352` | Baseline and axis lines, 1 px |
| `chart-surface` | `#171B22` (same as `bg-1`) | Chart background and the 2 px gaps between stacked fills |
| `bin-1` to `bin-6` | `#184F95`, `#256ABF`, `#3987E5`, `#6DA7EC`, `#9EC5F4`, `#CDE2FB` | Trait frequency bins, lowest Trait values to highest (an ordinal ramp: one hue, lightness rises with the value) |
| `cause-eaten` | `#3987E5` | Deaths by cause: eaten |
| `cause-temperature` | `#D95926` | Deaths by cause: temperature |
| `cause-old-age` | `#199E70` | Deaths by cause: old age |
| `cause-starved` | `#C98500` | Deaths by cause: starved |
| `cause-poisoned` | `#D55181` | Deaths by cause: poisoned |

The death-cause colours are stacked in exactly the order listed in this table (`eaten`, `temperature`, `old age`, `starved`, `poisoned`), bottom to top, and the legend uses the same order. Species colours in the Species chart and the family tree come from the Genome (Section 7).

Events are drawn on charts as neutral markers (Section 5.9), not coloured, so no extra palette is needed for them.

#### Verified contrast (WCAG 2 ratios, computed October 30, 2026)

| Pair | Ratio | Requirement | Result |
|---|---|---|---|
| `text-1` on `bg-1` | 14.30 | 4.5 | pass |
| `text-2` on `bg-1` | 8.07 | 4.5 | pass |
| `text-3` on `bg-1` | 5.42 (on `bg-2` 5.75, on `bg-3` 4.89) | 4.5 | pass on `bg-0` to `bg-3` |
| `accent-text` on `bg-1` | 8.54 | 4.5 | pass |
| `accent` on `bg-1` | 6.16 | 3 (graphics) | pass |
| `on-accent` on `accent` | 6.74 | 4.5 | pass |
| `border-strong` on `bg-1` | 3.80 (on `bg-2` 3.39) | 3 (control boundary) | pass |
| `focus` on `bg-1` | 8.54 | 3 | pass |
| Brightest-to-dimmest Cell colours on `bg-1` | 8.06 or more | 3 (graphics) | pass |
| Death-cause colours on `bg-1` | all 3 or more | 3 (graphics) | pass (validator) |

Death-cause palette validator result (dark mode, surface `#171B22`, adjacent pairs): lightness band pass, chroma floor pass, colour-blind separation pass (worst adjacent pair 8.4, protan), normal-vision floor pass (worst adjacent pair 19.3), contrast pass. Trait bin ramp validator result (ordinal mode): monotone lightness, adjacent steps of at least 0.06 in lightness, light-end contrast 2.13 to 1 against the surface, single hue, all pass.

### 2.3 Typography

- **One typeface: JetBrains Mono** (SIL Open Font Licence 1.1; **PROPOSED** choice of monospace face, decision D1 only fixed "one monospaced font everywhere"). Bundle `JetBrainsMono-Regular.ttf`, `JetBrainsMono-Medium.ttf` and `JetBrainsMono-Bold.ttf` under `Assets/Fonts/` with the licence text. Use Unity's own default font only as a fallback if the bundled font fails to load.
- Everything is monospaced, including the Help screen. Keep long help text short and in narrow columns (Section 5.12).
- Sentence case everywhere. No all-capitals labels. No letter spacing changes.

| Token | Size / line height | Weight | Use |
|---|---|---|---|
| `type-display` | 48 / 56 | Bold | Wordmark on the setup screen |
| `type-heading` | 28 / 36 | Bold | Help screen title, extinction title |
| `type-title` | 20 / 28 | Bold | Dialog titles, card titles, section titles in the Help screen |
| `type-body` | 15 / 22 | Regular | Body text, values, readouts |
| `type-label` | 15 / 22 | Medium | Control labels, tab labels, button text |
| `type-caption` | 13 / 18 | Regular | Tooltips, axis tick labels, chart legends, hints, the Temperature legend |

Numbers: no thousands separator under 10,000; comma separators from 10,000. Time is `mm:ss`, and `h:mm:ss` from one hour. Mean Generation has one decimal. Trait values have two decimals. Percentages are whole numbers. Temperature values are shown as `0.00` to `1.00`.

### 2.4 Spacing

4 px base unit.

| Token | Value |
|---|---|
| `space-1` | 4 |
| `space-2` | 8 |
| `space-3` | 12 |
| `space-4` | 16 |
| `space-5` | 24 |
| `space-6` | 32 |
| `space-7` | 48 |

Defaults: gap between related controls 8; panel padding 16; card padding 16; dialog padding 24; setup card padding 32; gap between sections 24.

### 2.5 Shape and borders

- Control height: **40 px** for every button, input, dropdown and tab (decision D7, comfortable density).
- Corner radius **4 px** on buttons, inputs, dropdowns, cards, tooltips and dialogs (decision D5). The side panel, the top bar and chart plot areas have square corners. The Cell shapes and icon buttons' round parts are geometry, not radius tokens.
- Borders are 1 px. Controls use `border-strong`; dividers between regions use `border`.
- No shadows, gradients or blurs anywhere except the Temperature legend bar, which is a gradient because it is data.

### 2.6 Layers
Elevation is expressed by background steps, not shadows: window `bg-0`; panels and cards `bg-1`; controls and tooltips `bg-2`; hover `bg-3`; pressed `bg-4`. Dialogs and the Help screen sit on a `bg-1` card over the `scrim`.

### 2.7 Motion
**None.** All state changes (hover, press, tab switch, panel collapse, dialog open) are instant. Tooltips appear after a 400 ms hover delay; a delay is not an animation. The simulation itself is the only moving thing on screen.

### 2.8 Icons (decision D3: icons everywhere, with hover tooltips)

- Icons are drawn in code with `Painter2D`. There are no image files.
- Grid: 24 by 24 units, drawn at 20 by 20 px inside a 40 by 40 px button (scale 20 / 24). Stroke 2 units, round caps and joins, no fill unless the row says "fill". The colour is the current text colour: `text-2` at rest, `text-1` on hover, `accent-text` when a toggle is on, `text-disabled` when disabled.
- Every icon-only control has a hover tooltip (Section 8). An icon never appears without one.
- Geometry below uses: `L(x1,y1,x2,y2)` line; `P(x,y; x,y; ...)` polyline; `PC` closed polyline; `C(cx,cy,r)` circle; `R(x,y,w,h,r)` rounded rectangle; `A(cx,cy,r,startDeg,endDeg)` arc, angles measured clockwise from the positive x axis in screen space; `B(...)` Bezier path in SVG notation.

| Icon | Used for | Geometry |
|---|---|---|
| `play` | Resume the Run | fill `PC(8,5; 19,12; 8,19)` |
| `pause` | Pause the Run | fill `R(7,5,3.5,14,1)`, `R(13.5,5,3.5,14,1)` |
| `speed-1` | Speed 1x | `P(9,6; 15,12; 9,18)` |
| `speed-2` | Speed 2x | `P(6,6; 12,12; 6,18)`, `P(12,6; 18,12; 12,18)` |
| `speed-3` | Speed 5x | `P(4,6; 9,12; 4,18)`, `P(9,6; 14,12; 9,18)`, `P(14,6; 19,12; 14,18)` |
| `speed-4` | Speed 10x | `P(3,6; 7,12; 3,18)`, `P(8,6; 12,12; 8,18)`, `P(13,6; 17,12; 13,18)`, `P(18,6; 22,12; 18,18)` |
| `event` | Events menu | `L(6,4,6,20)`, fill `PC(6,5; 18,8.5; 6,12)` |
| `temperature` | Temperature tint toggle | `R(10,3,4,13,2)`, fill `C(12,17.5,3.5)` |
| `save` | Save a Run | `L(12,4,12,14)`, `P(8,10; 12,14; 16,10)`, `P(5,15; 5,20; 19,20; 19,15)` |
| `load` | Load a Run | `L(12,14,12,4)`, `P(8,8; 12,4; 16,8)`, `P(5,15; 5,20; 19,20; 19,15)` |
| `csv` | Export CSV | `R(4,5,16,14,1)`, `L(4,10,20,10)`, `L(4,14.5,20,14.5)`, `L(10,5,10,19)` |
| `screenshot` | Screenshot | `P(4,9; 4,4; 9,4)`, `P(15,4; 20,4; 20,9)`, `P(20,15; 20,20; 15,20)`, `P(9,20; 4,20; 4,15)`, `C(12,12,3)` |
| `help` | Help screen | `C(12,12,9)`, the character "?" in Bold 15 px centred at (12,12) |
| `panel` | Collapse or open the side panel | `R(3,5,18,14,1)`, `L(15,5,15,19)`; fill `R(15,5,6,14,0)` when the panel is open |
| `home` | Back to the setup screen | `P(4,12; 12,4; 20,12)`, `P(6,11; 6,20; 18,20; 18,11)` |
| `close` | Close dialog, close inspector | `L(6,6,18,18)`, `L(18,6,6,18)` |
| `chevron-down` | Dropdown indicator | `P(7,10; 12,15; 17,10)` |
| `check` | Checkbox mark | `P(5,12.5; 10,17.5; 19,7)` |
| `random` | New random Seed | `R(4,4,16,16,3)`, fill `C(9,9,1.4)`, `C(15,15,1.4)`, `C(12,12,1.4)` |
| `restart` | Restart a Run | `A(12,12,7,-60,240)`, `P(17,3; 17.5,8; 12.5,8)` (arrowhead at the arc start) |
| `tab-traits` | Traits tab | `L(4,8,20,8)`, `L(4,16,20,16)`, fill `C(9,8,2.5)`, `C(15,16,2.5)` |
| `tab-species` | Species tab | `C(9,12,5)`, `C(15,12,5)` |
| `tab-tree` | Tree tab | `P(4,12; 10,12; 10,6; 20,6)`, `P(10,12; 10,18; 20,18)` |
| `tab-stats` | Stats tab | fill `R(5,12,3,7,0)`, `R(10.5,6,3,13,0)`, `R(16,9,3,10,0)` |
| `heat-wave` | Heat wave Event | `C(12,12,4)`, eight rays: `L` from radius 7 to radius 10 at 0, 45, 90, 135, 180, 225, 270, 315 degrees |
| `ice-age` | Ice age Event | `L(12,3,12,21)`, `L(4.2,7.5,19.8,16.5)`, `L(19.8,7.5,4.2,16.5)` |
| `famine` | Famine Event | fill `C(12,12,3.5)`, `L(5,19,19,5)` |
| `food-bloom` | Food bloom Event | fill `C(8,14,2.5)`, `C(15,15,2.5)`, `C(12,8,2.5)` |
| `toxin-spill` | Toxin spill Event | `B(M12,3 C12,3 5,11 5,15 A7,7 0 0 0 19,15 C19,11 12,3 12,3 Z)` |
| `status-good` | Success | `C(12,12,9)`, `P(8,12.5; 11,15.5; 16,9)` |
| `status-warning` | Caution | `PC(12,4; 21,19; 3,19)`, `L(12,10,12,14)`, fill `C(12,16.5,1)` |
| `status-critical` | Terminal state, extinction | `C(12,12,9)`, `L(9,9,15,15)`, `L(15,9,9,15)` |
| `status-serious` | Recoverable error | `C(12,12,9)`, `L(12,7,12,13)`, fill `C(12,16.5,1)` |
| `glyph-branch` | Wordmark | stroke 2.5: `B(M4,12 H10 C14,12 15,6 20,6)`, `B(M10,12 C14,12 15,18 20,18)`, fill `C(4,12,2)`, `C(20,6,2)`, `C(20,18,2)` |

The `glyph-branch` is drawn at 40 px beside the wordmark in `accent-text`. It is the only icon that appears outside a control.

---

## 3. Layout grid and regions

- Everything aligns to the 4 px unit; major regions to 8 px.
- **Top bar**: 56 px high (40 px controls with 8 px above and below), `bg-1`, `border` hairline on the bottom edge. Present in the Run screen only.
- **World view**: fills everything below the top bar and left of the side panel. Background `bg-0`.
- **Side panel**: `bg-1`, `border` hairline on its left edge, width 30% of the window with a minimum of 420 px and a maximum of 560 px (so 560 px at the reference size). It can be collapsed with the panel toggle or the Tab key; the World view then takes the full width.
- **Setup card**: 720 px wide, centred horizontally and vertically on a `bg-0` window.

---

## 4. Components

For every component: all colours are tokens from Section 2.2; all text styles are tokens from Section 2.3; every interactive component with an explanation has a tooltip from Section 8. States: default, hover, pressed, disabled, and where it makes sense, active or focused.

### 4.1 Button
- Height 40 px, minimum width 40 px, horizontal padding 16 px, radius 4, text `type-label`.
- **Primary** (used once per screen or dialog): fill `accent`, text `on-accent`; hover fill `accent-text`; pressed fill `accent-pressed`; disabled fill `bg-2` and text `text-disabled`.
- **Secondary**: fill `bg-2`, 1 px `border-strong`, text `text-1`; hover fill `bg-3`; pressed fill `bg-4`; disabled text `text-disabled` and border `border`.
- **Icon button**: 40 by 40 px, no label, no border at rest, fill transparent; hover fill `bg-3`; pressed fill `bg-4`; icon colour per Section 2.8. A tooltip is mandatory.
- **Toggle** (icon button with state): on state has icon colour `accent-text` and a 2 px `accent` line along the bottom edge of the button; off is the plain icon button.
- Buttons with an icon and text: 20 px icon, 8 px gap, text `type-label`.

### 4.2 Speed selector
Four icon buttons (`speed-1` to `speed-4`) forming a segmented control: 4 px radius at the outer ends only, 1 px `border-strong` outline around the group, the active speed uses the toggle on-state fill `bg-3` plus `accent-text` icon. Exactly one is active. Keys 1 to 4 select a speed.

### 4.3 Slider with readout
- Row: label (`type-label`, `text-1`) on the left, current value (`type-body`, `text-1`, right aligned) on the right, 8 px below it the track.
- Track 4 px high, `bg-3`; fill from the left to the thumb in `accent`; thumb a 16 px circle in `text-1` with a 2 px `bg-1` ring; hover thumb 18 px; disabled colours use `text-disabled`.
- The row's total height is 64 px. A hover tooltip on the label gives the explanation.
- Double-click on the thumb resets the slider to its default.

### 4.4 Text field
Height 40 px, fill `bg-2`, 1 px `border-strong`, radius 4, text `type-body` `text-1`, padding 12 px; placeholder text `text-3`; focused: 2 px `focus` outline; invalid: 1 px `status-serious` border and a hint below in `type-caption` with the `status-serious` icon.

### 4.5 Dropdown
A text-field-styled button showing the current value with a `chevron-down` on the right. The open list is a `bg-2` card with 1 px `border-strong`, items 40 px high, hover `bg-3`, selected item with a leading `check` icon in `accent-text`. Used for the Preset and the Trait selector.

### 4.6 Checkbox
20 px square, radius 2 (a deliberate exception to the 4 px radius, because of its small size), 1 px `border-strong`; checked: fill `accent` with a `check` icon in `on-accent`. The label is `type-body`, 8 px to the right.

### 4.7 Tabs (side panel)
- A row of four tabs across the top of the side panel, each 40 px high with an icon (`tab-traits`, `tab-species`, `tab-tree`, `tab-stats`), 8 px gap, and the label in `type-label`.
- Selected: text `text-1`, icon `accent-text`, a 2 px `accent` line along the bottom. Unselected: `text-2`. Hover: fill `bg-3`. A 1 px `border` line runs under the whole row.

### 4.8 Panel and card
- **Panel**: a region with `bg-1` and 16 px padding.
- **Card**: `bg-2`, 1 px `border`, radius 4, 16 px padding. Used for the Cell inspector and for grouped content inside panels.

### 4.9 Dialog
`bg-1`, 1 px `border-strong`, radius 4, 24 px padding, over the `scrim`. Title in `type-title`, a `close` icon button at the top right. Buttons are right aligned at the bottom: the secondary action first (for example Cancel), the primary action last (for example Place). Escape closes it.

### 4.10 Tooltip
- `bg-2` fill, 1 px `border-strong`, radius 4, padding 8 by 12 px, maximum width 320 px.
- First line: the control's name in `type-caption` Bold, `text-1`. Then one or two sentences in `type-caption` Regular, `text-2`. Optional last line: the keyboard shortcut in `text-3`, for example "Key: Space".
- Appears after 400 ms of hovering, below and to the right of the pointer, flipped to stay inside the window; disappears when the pointer leaves or any button is pressed. UI Toolkit does not show tooltips in a running player by itself, so a custom `TooltipController` listens for pointer enter and leave events on elements that carry a tooltip key.
- Tooltip text comes from one central copy table (Section 8).

### 4.11 Readout
Used in the top bar and the Stats tab. A pair of text runs on one line: the label in `type-caption` `text-3`, then the value in `type-body` `text-1`, with 8 px between them. Values never change width because the typeface is monospaced; pad numbers to a fixed width when they sit in a column (for example `0312`).

### 4.12 Legend
A vertical list of rows, each 28 px high: a 12 px square swatch (radius 2) in the series colour, 8 px gap, the name in `type-body` `text-1`, and a right-aligned value in `type-body` `text-2`. A hovered row has `bg-3`; for the Species legend clicking a row highlights that Species' Cells in the World (the others are drawn at 35% opacity) and clicking again clears it.

### 4.13 Message
A horizontal block with a 20 px status icon, a message in `type-body` `text-1` and an optional action button. Left border 3 px in the matching status token. Fill `bg-2`. The icon and the text are always both present. Used for load errors and the window-size warning.

### 4.14 Scrollbar
A 8 px wide track (`bg-1`) with a thumb in `bg-4` (hover `border-strong`), radius 4, visible only when content overflows. Used in the Help screen and the side panel.

### 4.15 Wordmark
The glyph `glyph-branch` at 40 px in `accent-text`, 12 px gap, then the text "divergence" (lowercase) in `type-display`, `text-1`. Under it, in `type-body` `text-2`: "a natural selection simulation". The lowercase wordmark is the only lowercase title in the interface, so it is chosen deliberately as the brand mark. In the rulebook it appears the same way, in black on white.

### 4.16 Cell inspector card
A card at the top left of the World view, 280 px wide, shown when a Cell is selected.
- Header: "Cell" in `type-title` and a `close` icon button. Under it a line with the colour swatch of the Cell's Species (12 px) and the Species name in `type-body`.
- Rows (readout style): energy (as `value / maximum`), age (`mm:ss`), Generation, parent id.
- Trait bars: eight rows for Traits 0 to 7, each 24 px high: name in `type-caption` `text-2` (left, 120 px), a bar 100 px wide and 8 px high (track `bg-3`, fill `accent`, the fill length is the normalised Trait value), the value in `type-caption` `text-1` (right aligned, two decimals). Founder mean is marked by a 1 px `text-3` tick on the track.
- Brain output: four small bars for turn, thrust, bite, mate (the mate bar shown only if the feature exists), each 8 px high with the label in `type-caption`.

### 4.17 Event marker (on charts)
A 1 px dashed vertical line (4 px dash, 4 px gap) in `text-3` through the full height of the plot, with a 20 px circle at its top (fill `bg-2`, 1 px `border-strong`) holding one letter in `type-caption` Bold `text-1`: **H** heat wave, **I** ice age, **F** famine, **B** food bloom, **T** toxin spill. Hovering the circle shows a tooltip with the Event name, time, radius and any settings.

### 4.18 Temperature legend
The small gradient bar described in Section 2.2, with "cold" at the left end and "hot" at the right end in `type-caption` `text-3`. A 8 px margin from the right and bottom edges of the World view.

---

## 5. Charts (follow the data-visualisation rules: thin marks, one axis, recessive grid, hover on every chart)

### 5.1 Anatomy shared by all charts
- Rendered with `Painter2D` on a `VisualElement`; `bg-1` surface; no border around the plot.
- Plot margins: left 56 px (for y labels), right 16 px, top 28 px (room for Event markers), bottom 36 px (x labels).
- Gridlines: horizontal only, 1 px `chart-grid`, 4 to 6 ticks. Axis baseline: 1 px `chart-axis`. Tick labels: `type-caption` `text-3`. No vertical gridlines, no frame, no dual axes.
- Titles sit above the chart, in `type-label` `text-1`, with the unit in `type-caption` `text-3` (for example "Mean size (relative)").
- **Hover layer**: a 1 px `text-3` vertical crosshair that follows the pointer and snaps to the nearest sample, plus a tooltip card (Section 4.10 styling) listing the time, the values and the colour swatch of each series. Marks that carry the hover get a 8 px marker with a 2 px `chart-surface` ring.
- Charts redraw once per sample or once per real second, never every frame.
- Every chart has a short caption line under its title explaining how to read it (the "chart reading" text in Section 8).
- A "Table" toggle is not required (decision D6 committed only to contrast), but the hover read-out always gives exact values.

### 5.2 Line and band chart (Traits tab, top)
Mean line 2 px `chart-series-1`. Standard-deviation band: the same colour at 20% opacity from mean minus one standard deviation to mean plus one. No point markers except under the crosshair.

### 5.3 Frequency chart (Traits tab, bottom)
Stacked area of the six Trait bins, `bin-1` at the bottom to `bin-6` at the top, with a 2 px `chart-surface` gap between fills, normalised to 100% of the living Cells. A six-row legend on the right of the chart header shows each bin's Trait range (for example "0.80 to 1.00"), in the same order.

### 5.4 Species chart
Stacked area, one fill per Species in its Genome colour, 2 px `chart-surface` gap, bottom to top in order of Species birth. The 20 largest Species by peak population are drawn; the rest are summed into "Other" in `text-3`. A legend (Section 4.12) lists living Species, largest first.

### 5.5 Family tree
- Time on the horizontal axis; one row per Species in depth-first order; each Species is a horizontal line from its birth to its extinction time (or to now), 1 to 6 px thick by current or last population, in the Species' Genome colour.
- A parent connects to a child with a vertical 1.5 px `text-3` line at the child's birth time.
- Species names at the right end of each line in `type-caption` `text-2`. An extinct line ends with a small `text-3` cap (a 1 px by 8 px vertical bar).
- Hover shows a tooltip card with the Species name, birth time, birth Generation, peak population and current population; clicking highlights the Species' Cells in the World.

### 5.6 Deaths by cause (Stats tab)
Stacked bars in 10 s bins, stacked in the order in Section 2.2, 2 px `chart-surface` gap between segments, bar width 80% of the bin. Legend rows with the cause colours, in stacking order. Segment hover shows the cause, count, and bin time.

### 5.7 Stats numbers
A grid of readouts (Section 4.11) in two columns: cells alive, births per minute, deaths per minute, highest Generation, living Species, extinct Species.

### 5.8 Axes switch
A two-button segmented control "time" and "generation" above the Traits charts (secondary button style, radius 4 outer ends). The selected one has `bg-3` and `accent-text` text.

### 5.9 Event markers
Section 4.17, drawn on every time-based chart.

---

## 6. Screens (patterns)

### 6.1 Setup screen
A 720 px wide `bg-1` card centred on `bg-0`, padding 32 px.
```
[glyph] divergence                                  (wordmark, Section 4.15)
        a natural selection simulation

Preset                [ Mild world            v ]   (dropdown, 40 px)
Seed                  [ mild______________ ] [random]  (text field + icon button)

Food abundance                               1.00   (slider rows, 64 px each)
Temperature range                            0.60
Zone scale                                   1.00
Toxin level                                  0.30
Mutation rate                                1.00

[ Start ]  [ Load saved run ]  [ How it works ]      (primary, secondary, secondary)
```
- Rows are separated by 16 px; the five sliders sit in a block with 24 px above and below.
- Choosing a Preset fills the Seed and sliders. Changing any control afterwards switches the dropdown to "Custom".
- Start is the only primary button on the screen. All controls have hover tooltips.

### 6.2 Run screen
```
+----------------------------------------------------------------------------+
| [play][1][2][3][4]  time 12:34 | generation 18.4 | cells 312 | species 4     |  <- top bar, 56 px
|                                [event v][temp]  [save][load][csv][shot] [?][panel][home] |
+----------------------------------------------------------+-----------------+
|                                                          | [Traits][Species][Tree][Stats]
|  World view                                              |  tab content
|  (Cell inspector card top left)                          |  (charts, legend, readouts)
|                                       [cold ==== hot]    |
+----------------------------------------------------------+-----------------+
```
- The top bar groups (left to right): transport (play or pause, then the four speeds), readout, a flexible spacer, Events and Temperature tint, a divider, file actions (Save, Load, Export CSV, Screenshot), a divider, then Help, Panel toggle and Home. Dividers are 1 px `border` lines 24 px high.
- The readout reads `time 12:34   generation 18.4   cells 312   species 4`, using the readout component with 24 px between pairs.

### 6.3 Events menu and placement
- The Events button (`event` icon with a small `chevron-down`) opens a menu card listing the five Events, each with its icon, name in `type-label`, and a one-line description in `type-caption` `text-3`.
- Choosing one opens the Event dialog (Section 4.9): title with the Event icon, sliders (radius 5 to 60; intensity and duration where they apply), the checkbox "Whole World", and the buttons Cancel (secondary) and Place (primary).
- **Place** closes the dialog and arms placement: the pointer becomes a crosshair, a `world-preview` dashed circle of the chosen radius follows the pointer inside the World view, and a one-line message card at the top of the World view reads "Click the World to place [event name]. Press Escape to cancel."
- A click places it. With "Whole World" checked, the button reads **Apply** and the Event applies at once with no placement step.
- Placed Events are added to the chart markers; the placement circle is not left on screen.

### 6.4 Extinction overlay
Centred 480 px wide card on the `scrim`, over the frozen World: `status-critical` icon (32 px) and the title "Population extinct" in `type-heading`. Below, readouts for run time, highest Generation, Species ever (living and extinct). Buttons: Restart (primary, `restart` icon), Export CSV (secondary, `csv` icon), Menu (secondary, `home` icon).

### 6.5 Help screen
An overlay card, width 1100 px (or 80% of the window if smaller), height 80% of the window, on the `scrim`. A left column of 240 px lists the sections as plain text rows (selected row `bg-3`); the right column scrolls the section's text at `type-body`, with a maximum line length of 70 characters. Sections: What this is, Controls, Settings, The World, Traits, Reproduction, Species and colours, Charts, Events, Save and exports. All text comes from the copy table (Section 8) and the same sentences go into the one-page rulebook. A `close` icon button at the top right; Escape also closes it.

### 6.6 Messages and errors
Use the message component (Section 4.13), placed at the top of the World view or the setup card. Messages cover: save file cannot be loaded (`status-serious`: "This file cannot be loaded. It was made by a different version."), window too small (`status-warning`), file exported (`status-good`, shown for 4 seconds), population cap reached (a small `type-caption` note in the top bar readout area, not a message).

---

## 7. Cell colour (decision D2: perceptual colour)

This section replaces the RGB-subtraction method in `PLAN.md` Section 7.4.

Goals: a Cell with no deviation from the Founders is exactly white; near Genomes give near colours; every colour is bright enough to read on the dark background; equal Genome distances look like similar colour differences.

Method (OKLCH, computed in `Evo.Core` or the Unity layer as pure functions; pure C# with no Unity types):
1. Build a deviation vector `u` of 108 numbers: for Traits 0 to 7, `u_i = sqrt(0.6 / 8) * (value_i - founderMean_i) / (max_i - min_i)`; for the 100 Brain weights, `u_(8+j) = sqrt(0.4 / 100) * (w_j - m_j) / 2.0`, where `m_j` is the Founder Brain mean (the innate wiring with no noise).
2. Project to two numbers with two fixed random unit vectors `Px` and `Py` of length 108, generated once from the constant seed `0xC0102026` with `SeededRandom` (not the World Seed): `x = sqrt(108) * (Px . u)` and `y = sqrt(108) * (Py . u)`.
3. Distance from white: `r = sqrt(x^2 + y^2)`; saturation `s = clamp01(r / 0.30)` (**TUNE**: 0.30 is the deviation that gives full colour).
4. Hue: `h = atan2(y, x)` in degrees.
5. OKLCH colour: `L = 1 - 0.22 * s`, `C = 0.16 * s`, `H = h`. Lightness never falls below 0.78.
6. Convert OKLCH to linear sRGB with the standard OKLab matrices. If any channel falls outside 0 to 1, reduce `C` by bisection (30 iterations) until it fits, keeping `L` and `H`. Then apply the sRGB transfer function and clamp.
7. At `s = 0` the result is `(1, 1, 1)`.

Verified: with `L = 1 - 0.22 s` and the gamut-mapped chroma, the dimmest colours at twelve hues have contrast 8.06 to 9.15 against `bg-1`.

A Species' display colour is the colour computed from its centroid Genome by the same method. In charts, Species colours rely on the 2 px gap, the legend, hover read-outs and click-to-highlight, because two Species can end up with similar colours.

Tests (EditMode): zero deviation gives `(1,1,1)` within 1/255; the function is continuous (a small change in the Genome changes the colour by a small amount); the minimum contrast against `#171B22` over 360 hues and `s` from 0.3 to 1.0 is at least 3; outputs are always valid sRGB.

---

## 8. Copy (written once, used for tooltips, the Help screen and the rulebook)

Style: plain sentences, present tense, no exclamation marks, no emoji, units given where they exist. A tooltip is a name plus one or two sentences. The strings below are the specification; keep them in one `UiText` table keyed by id.

### Controls
| Id | Name | Text |
|---|---|---|
| `play` | Resume | Resumes the simulation. Key: Space. |
| `pause` | Pause | Freezes the simulation. You can still pan, zoom and select Cells. Key: Space. |
| `speed-1` | Speed 1x | Runs the simulation at normal speed. Key: 1. |
| `speed-2` | Speed 2x | Runs at twice normal speed. Key: 2. |
| `speed-3` | Speed 5x | Runs at five times normal speed. Key: 3. |
| `speed-4` | Speed 10x | Runs at ten times normal speed. On slow computers the real speed may be lower. Key: 4. |
| `events` | Events | Places a heat wave, ice age, famine, food bloom or toxin spill in the World. |
| `temperature-tint` | Temperature tint | Shows or hides the colour overlay for Temperature: blue is cold, red is hot. Key: T. |
| `save` | Save run | Downloads this Run as a file you can load later, even in another browser. |
| `load` | Load run | Loads a Run from a saved file and replaces the current Run. |
| `csv` | Export CSV | Downloads the recorded Trait and Species data as two CSV files. |
| `screenshot` | Screenshot | Downloads a PNG image of the window. |
| `help` | How it works | Opens the Help screen. |
| `panel` | Side panel | Shows or hides the charts. Key: Tab. |
| `home` | Menu | Returns to the setup screen. The current Run is discarded unless you saved it. |
| `random-seed` | New random seed | Fills the Seed field with a random code. |

### Setup
| Id | Name | Text |
|---|---|---|
| `preset` | Preset | A ready-made set of settings and a Seed. Changing any setting afterwards switches this to Custom. |
| `seed` | Seed | A code that fixes the World. The same Seed and settings always start the same World. |
| `food-abundance` | Food abundance | How fast Food appears. Higher values support more Cells. Range 0.20 to 2.00. |
| `temperature-range` | Temperature range | How different the coldest and hottest parts of the World are. 0 is uniform, 1 is the strongest contrast. |
| `zone-scale` | Zone scale | How large the warm and cold zones are. Lower values make large zones, higher values make small ones. |
| `toxin-level` | Toxin level | How much Toxin the World starts with. 0 means none. |
| `mutation-rate` | Mutation rate | How much Traits and Brains change when a Cell is born. 1.00 is normal. |
| `start` | Start | Starts a new Run with these settings. |

### Tabs and charts
| Id | Name | Text |
|---|---|---|
| `tab-traits` | Traits | Shows how one Trait changes in the living population over time. |
| `tab-species` | Species | Shows how many Cells belong to each Species over time. |
| `tab-tree` | Tree | Shows which Species split from which, and when. |
| `tab-stats` | Stats | Shows live counts and why Cells die. |
| `trait-select` | Trait | Chooses which Trait the charts show. |
| `axis-switch` | X axis | Plots time in seconds, or the mean Generation of the living Cells. |
| `chart-trait-mean` | Mean and spread | The line is the average value of the Trait. The shaded band is one standard deviation either side. |
| `chart-trait-frequency` | Frequency | Shows what share of Cells has each range of the Trait. Darker bins are lower values. |
| `chart-species` | Species over time | Each colour is one Species. The height of a band is its population. |
| `chart-tree` | Family tree | Each line is a Species from when it first appeared to when it died out. Branches show where a Species split off. |
| `chart-deaths` | Deaths by cause | Counts of Cells that died in each 10 second period, by cause. |
| `event-marker` | Event marker | Marks when an Event happened. H heat wave, I ice age, F famine, B food bloom, T toxin spill. |

### Events
| Id | Name | Text |
|---|---|---|
| `event-heat-wave` | Heat wave | Raises Temperature inside the circle, then lets it return to normal. |
| `event-ice-age` | Ice age | Lowers Temperature inside the circle, then lets it return to normal. |
| `event-famine` | Famine | Removes most Food inside the circle and slows new Food there for a while. |
| `event-food-bloom` | Food bloom | Adds extra Food inside the circle for a while. |
| `event-toxin-spill` | Toxin spill | Releases Toxin inside the circle. It spreads slowly and fades. |
| `event-radius` | Radius | The size of the area affected, in World units. |
| `event-intensity` | Intensity | How far Temperature moves from normal at the centre. |
| `event-duration` | Duration | How long the Event lasts at full strength, in seconds. |
| `event-whole-world` | Whole World | Applies the Event everywhere instead of in one place. |
| `event-place` | Place | Closes this window. Then click the World to place the Event. |

### Inspector and legend
| Id | Name | Text |
|---|---|---|
| `inspect-energy` | Energy | The Cell's stored energy and its maximum. It dies at zero. |
| `inspect-age` | Age | Time since the Cell was born. Old Cells lose extra energy. |
| `inspect-generation` | Generation | How many births separate this Cell from a Founder. |
| `inspect-trait` | Trait bar | The Trait value inside its allowed range. The tick marks the Founders' average. |
| `inspect-brain` | Brain output | What the Cell's Brain is asking it to do now: turn, move, bite. |
| `legend-species` | Species | Click to highlight this Species' Cells in the World. Click again to clear. |
| `legend-cause` | Cause of death | The colour used for this cause in the Deaths chart. |

### Help screen section text (short form; the same sentences go into the rulebook)
1. **What this is.** Divergence is a model of natural selection. Cells live in a World with Food, Toxin and varying Temperature. Cells that suit the World survive and reproduce, so their Traits become more common over generations. Over time the population can split into Species.
2. **Controls.** Drag to move the view. Scroll to zoom. Click a Cell to inspect it. Space pauses. Keys 1 to 4 change speed. F fits the World. T shows Temperature. Tab hides the side panel. Escape cancels or closes.
3. **Settings.** The Seed fixes the World. Food abundance, Temperature range, zone scale, Toxin level and mutation rate shape it. Presets fill these in. The same Seed and settings always make the same starting World.
4. **The World.** Temperature varies smoothly from cold to hot. Food appears as pellets, more in some places than others. Toxin is a set of particles that drift, attract each other slightly and fade.
5. **Traits.** Each Cell inherits ten Traits: size, speed, sense range, preferred Temperature, Temperature tolerance, Toxin resistance, diet, reproduction threshold, and two mating Traits. Each Trait helps in some way and costs energy in another, so no Cell is best everywhere.
6. **Reproduction.** A Cell divides when it has stored enough energy. The child gets a copy of the Genome with small random changes called mutations. Mutations are how new Traits and new Brains appear.
7. **Species and colours.** Cells begin white. A Cell's colour comes from its Genome, so similar Cells have similar colours. A Species is a group of Cells whose Genomes are close together. A new Species appears when a group drifts far enough from the rest.
8. **Charts.** Traits shows one Trait over time. Species shows population by Species. Tree shows which Species came from which. Stats shows counts and causes of death.
9. **Events.** Events change the World while a Run is going. Choose an Event, set its size, then click the World to place it.
10. **Save and exports.** Save downloads the Run as a file. Load restores it exactly. Export CSV downloads the recorded data. Screenshot downloads an image of the window.

---

## 9. Implementation in Unity

- **Token source of truth**: `Assets/Scripts/UnityLayer/UI/Theme/DesignTokens.cs` holds every colour, size and type token in Section 2 as constants. An Editor method `Evo.Editor.GenerateTokensUss` writes `Assets/UI/Theme/tokens.uss` from it (custom properties named `--evo-<kebab-case>`), and the build script runs it. An EditMode test checks that the committed `tokens.uss` equals the generated output so the two cannot drift. Charts and World meshes read `DesignTokens` directly; USS components use `var(--evo-...)`.
- **USS files**: `tokens.uss` (generated), `typography.uss`, `components.uss`. Class names use a flat prefix: `evo-button`, `evo-button--primary`, `evo-button--icon`, `evo-slider`, `evo-tab`, `evo-card`, `evo-dialog`, `evo-tooltip`, `evo-readout`, `evo-legend-row`, `evo-message`, `evo-chart`.
- **Font**: the bundled TTFs loaded with `-unity-font-definition` or `-unity-font` in `typography.uss`. Verify in the session 1 smoke test that the font renders in the WebGL build; if the font definition does not work, create the font asset in code with `FontAsset.CreateFontAsset`.
- **Icons**: a static `Icons` class with one method per icon in Section 2.8, taking a `Painter2D`, a colour and a size. An `IconElement` (a `VisualElement` with `generateVisualContent`) draws any icon by name and recolours on hover or state change by reading the element's current style colour.
- **Tooltips**: a `TooltipController` created by the bootstrap. Elements register with `Tooltips.Attach(element, textId)`; the controller shows one `evo-tooltip` element in the top layer after 400 ms of hovering, positions it, and hides it on leave or press. All text lives in `UiText` (a static dictionary keyed by the ids in Section 8; a test checks that every id used in code exists and every id has text).
- **Charts**: a shared `ChartElement` base (axes, grid, crosshair, tooltip, Event markers) with subclasses for the line and band chart, stacked area chart, bar chart and tree. Chart code never contains a colour literal.
- **Scale**: `PanelSettings` with scale mode scale with screen size, reference resolution 1920 by 1080, match 0.5.
- **No animation**: no USS transitions, no `schedule` based animation, no tweening code.

---

## 10. Quality checklist (use when reviewing a screenshot or a build)

- [ ] No colour or size appears in the code outside `DesignTokens`.
- [ ] Every icon-only control has a tooltip, and every tooltip id exists in `UiText`.
- [ ] Every control is 40 px high; every radius is 4 px (checkbox 2 px).
- [ ] Only neutrals appear in the chrome. Saturated colour appears only in Cells, Species in charts, Toxin, the Temperature tint, the death-cause chart and the Trait bin chart.
- [ ] No glow, shadow, blur, gradient decoration or animation.
- [ ] All text is JetBrains Mono, sentence case, at one of the six type tokens.
- [ ] Text contrast of at least 4.5 to 1 and control borders of at least 3 to 1 (Section 2.2 table); recheck after any colour change.
- [ ] Every chart has a title, a caption, a hover read-out and a legend where it has more than one series.
- [ ] The words "gene", "tile", "square", "organism" and "creature" do not appear in any interface text.
- [ ] The Help screen text and the rulebook match Section 8.
- [ ] Screenshots at 1920 by 1080 and 1366 by 768 show no clipped or overlapping text.

---

## 11. Defaults the user has not seen yet (veto any)

- JetBrains Mono as the specific monospaced face (the user chose "one monospaced font everywhere" but not which one).
- The exact neutral palette, accent blue, hover and pressed steps.
- Icon geometry in Section 2.8, including using chevron counts for the four speeds.
- The wordmark in lowercase, and the subtitle "a natural selection simulation".
- The death-cause stacking order and colour assignment, event markers as neutral letter chips, and the temperature tint colours and 30% opacity.
- Tooltip delay of 400 ms, maximum width 320 px, and the 70-character line length in the Help screen.
- All copy in Section 8 (the wording, not the meaning).
- Top bar height 56 px; side panel width rule (30%, minimum 420, maximum 560); setup card width 720 px.
- No animation at all, including panel collapse.
- The colour mapping constants: the 0.30 saturation scale, `L = 1 - 0.22 s`, `C = 0.16 s`.

---

## 12. Decisions from the design interview (October 30, 2026)

| # | Topic | User's decision | Notes |
|---|---|---|---|
| D1 | Typography | One monospaced font everywhere | Not rec. (rec. Inter for text and a mono face for numbers). |
| D2 | Cell colour mapping | Perceptual colour (OKLCH), hue from direction, chroma from distance, lightness kept high | Rec. Replaces the RGB subtraction in `PLAN.md` 7.4. |
| D3 | Icons | Icons everywhere with hover tooltips | Not rec. (rec. icons for transport only). |
| D4 | In-interface explanations | Hover tooltips on every control and chart | Rec. No guided tour. |
| D5 | Shape language | Slightly rounded, 4 px | Rec. |
| D6 | Accessibility scope | Text contrast of at least 4.5 to 1 and control borders of at least 3 to 1. Not selected: colour-blind-safe chart palettes with labels, full keyboard operation with a visible focus ring, a UI scale setting | Palettes were validated anyway because it cost nothing; no keyboard or scale commitment. |
| D7 | Control size | Comfortable, 40 px controls with 15 px text | Not rec. (rec. compact 32 px). |
| D8 | Brand mark | Monospace wordmark with a small branching glyph | Rec. |
| (name) | Project name | Divergence | Chosen from a list of suggestions. |
