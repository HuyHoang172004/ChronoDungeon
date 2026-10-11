# Map 01 — Ruined Entrance (Side-View Blueprint)

## Role

Ruined Entrance is the opening map of the side-view game. It teaches movement,
jumping, basic combat and the first weapon pickup without using Time Loop or
Temporal Ghost. It is one continuous, authored route rather than a group of
teleporting zones.

## Continuous route

`Rampart Breach → Broken Aqueduct → Sunken Mustering Yard → Runic Liftworks → Gatebreaker Keep`

The route spans roughly 192 world units horizontally. The camera tracks the
Player continuously, with a little vertical freedom in the liftworks. Gaps are
short, readable, and have a recovery floor in the opening map.

## Five level beats

1. **Rampart Breach (x 0–28)** — Safe spawn, movement/jump practice and the
   first enemy marker. A collapsed outer wall frames the beginning of the run.
2. **Broken Aqueduct (x 28–65)** — A sequence of low ledges and a short,
   non-lethal gap. It teaches jumping before combat pressure rises.
3. **Sunken Mustering Yard (x 65–106)** — The first broad combat yard. It
   contains three to four standard enemy spawn markers, a reward/chest marker,
   and clear room to dash and attack.
4. **Runic Liftworks (x 106–146)** — A vertical ruined tower route with
   ascending platforms. The first weapon pickup is placed here, after the
   player has experienced unarmed combat.
5. **Gatebreaker Keep (x 146–192)** — A flat, enclosed finale arena. It holds
   the Map 01 boss marker, reward marker, checkpoint and the Map 02 exit.

## Collider rules

- Every walkable floor and platform uses a matching `BoxCollider2D` on
  `SideViewGround`.
- Walls use their own solid colliders; camera bounds are trigger-only.
- The player shell is never allowed outside the left/right solid boundaries.
- The Broken Aqueduct recovery floor prevents an opening-map soft lock.
- Marker objects have no collision until their gameplay system is implemented.

## Presentation contract

The map is rendered as a layered dark-fantasy fortress: far ruins and waterfall
depth in the background, readable stone platforms in the foreground, cyan rune
torches as guidance, and a purple-tempered boss arena. Presentation is separate
from collision, so future visual replacement cannot change platforming physics.

## Current scene hierarchy

`Map01_RuinedEntrance`

- `Presentation`
  - `Background`
  - `Midground`
  - `Foreground`
- `Collision/GameplayGeometry`
- `GameplayMarkers`
- `MapBeats`
- `CameraSystem`
