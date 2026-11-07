# Divergence

Divergence is a browser simulation in which a population of simple cells evolves under environmental pressures and splits into species, built as a school science project to model natural selection.

## Language

### The simulation

**Run**:
One play of the simulation, from setup until the Player resets it or every Cell has died.
_Avoid_: game, session, playthrough

**World**:
The generated 2D environment a Run takes place in, with its Pressures laid out across it.
_Avoid_: map, level, arena

**Seed**:
A number or text that fixes how a World is generated, so the same Seed always gives the same World.
_Avoid_: random seed, world code

**Player**:
The person running the simulation. The Player sets up the World and triggers Events but never controls a Cell directly.
_Avoid_: user, viewer

### Cells and heredity

**Cell**:
One simulated organism that eats, moves, reproduces and dies.
_Avoid_: organism, creature, individual, agent

**Founder**:
A Cell present when a Run begins. Its Generation is 0.
_Avoid_: starter cell, ancestor

**Trait**:
One inherited, measurable property of a Cell, such as size or speed. Traits vary between Cells and change through Mutation.
_Avoid_: gene, attribute, stat

**Brain**:
The small neural network that turns what a Cell senses into what it does. Its weights are inherited and mutate like Traits.
_Avoid_: AI, controller

**Genome**:
The complete set of a Cell's inherited values: every Trait plus every Brain weight.
_Avoid_: DNA, genes

**Mutation**:
A small random change to a Trait or a Brain weight that happens when a Cell is born.
_Avoid_: random change, drift

**Generation**:
A Cell's lineage depth. A Founder is generation 0 and every other Cell is one more than its older parent (the parent with the higher Generation, when there are two).
_Avoid_: round, wave, cycle

**Trait frequency**:
The share of living Cells whose value for a given Trait falls within a stated range.
_Avoid_: trait count, distribution (unless meaning the whole spread)

**Species**:
A group of living Cells whose Genomes are close enough to count as one kind. Species are detected automatically and tracked from first appearance to extinction. When a Species divides, the group nearer the old average keeps the original Species and the other becomes a new Species descended from it.
_Avoid_: breed, race, clade, type

### Environment

The World is open space with things in it. It is not a grid, and it has no squares or tiles.

**Pressure**:
An environmental factor that affects whether Cells survive or reproduce: Temperature, Food or Toxin.
_Avoid_: hazard, stressor, condition

**Temperature**:
How hot or cold it is at a point in the World. It varies smoothly across the World and a Cell is stressed when it differs from the Cell's preferred Temperature by more than its tolerance.
_Avoid_: climate, heat

**Food**:
A small pellet of energy lying in the World that a Cell eats by touching it.
_Avoid_: plant, resource, nutrient

**Toxin**:
Drifting particles in the World that drain energy from Cells near them. Particles attract each other slightly, so a cloud spreads slowly.
_Avoid_: poison, pollution, hazard

**Event**:
A change to the Pressures that the Player places in the World during a Run, with a radius. The Events are heat wave, ice age, famine, food bloom and toxin spill.
_Avoid_: disaster, trigger, power-up

**Preset**:
A named set of starting settings, including a Seed, that the Player can pick instead of entering each setting.
_Avoid_: template, scenario, level

### Schoolwork

**Work session**:
One sitting of building the project, recorded as one entry in the Journal.
_Avoid_: session (alone, because it clashes with Run)

**Journal**:
The Word document that records each Work session with a short description, a screenshot and a reflection.
_Avoid_: log, diary, report
