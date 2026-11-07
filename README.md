# Divergence

A natural selection simulation. Cells with evolving Traits and neural-network Brains live in a World with Food, Toxin and varying Temperature. Cells that suit the World survive and reproduce, so their Traits shift over generations, and the population can split into Species. Place Events (heat wave, ice age, famine, food bloom, toxin spill) and watch the Traits respond.

Made for the L4 science natural selection project, built with Unity 6.3 LTS (WebGL).

## Play it

- Hosted: https://starharbor2491.github.io/divergence/
- Needs a recent Chrome, Edge, Firefox or Safari, and a window of at least 1280 by 720.
- Rules: press **How it works** in the simulation, or read `Rulebook/Divergence-Rulebook.pdf`.

## Run the zipped build

A WebGL build cannot run by opening `index.html` from disk. Unzip it, then serve the folder:

```
cd divergence-webgl
python3 -m http.server 8000
```

Open http://localhost:8000 in a browser.

## Controls

Drag to pan, scroll to zoom, click a Cell to inspect it. Space pauses, 1 to 4 change speed, F fits the World, T shows Temperature, Tab hides the side panel, Escape cancels or closes.

## Project layout

- `EvoSim/` Unity project. `Scripts/Core` is the simulation with no Unity types. `Scripts/UnityLayer` is rendering, input and UI. `Scripts/Editor` holds the build scripts.
- `tools/` shell scripts: `compile.sh`, `test.sh`, `build-webgl.sh`, `serve.sh`, `deploy-gh-pages.sh`, `experiment.sh` (headless runs), `shots.sh`.
- `Rulebook/` the one-page PDF rulebook.
- `PLAN.md`, `DESIGN.md`, `CONTEXT.md` the plan, design system and vocabulary.

## Build from source

Needs Unity 6000.3 (6.3 LTS) with WebGL Build Support. Then:

```
tools/test.sh          # EditMode tests
tools/build-webgl.sh   # writes Builds/WebGL
tools/serve.sh         # http://localhost:8000
```

Licence: MIT (see `LICENSE`). JetBrains Mono is under the SIL Open Font Licence 1.1.
