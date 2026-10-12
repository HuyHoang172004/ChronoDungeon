from pathlib import Path
from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
SOURCE_ROOT = ROOT / "Assets/ArtSource/Characters/Enemies"
OUTPUT_ROOT = ROOT / "Assets/Art/Enemies/SideView/Roster"
RIGHT_FACING_ROW = 2

SOURCES = {
    "RuinGuardian": ("Map01/HeavyWarden", "HeavyWarden", "Attack"),
    "AshHusk": ("Map01/AshHusk", "AshHusk", "Attack"),
    "RustArcher": ("Map02/RustArcher", "RustArcher", "Attack"),
    "ForgeKnight": ("Map03/ForgeKnight", "ForgeKnight", "HeavyHammerAttack"),
    "ThornSpitter": ("Map03/ThornSpitter", "ThornSpitter", "Attack"),
    "MirrorWisp": ("Map04/MirrorWisp", "MirrorWisp", "MirrorBolt"),
    "CrystalSentinel": ("Map04/LibrarySentinel", "LibrarySentinel", "Attack"),
}


def source_path(folder: str, source_name: str, state: str, attack_name: str) -> Path:
    movement = "MirrorBlink" if source_name == "MirrorWisp" else "Walk"
    suffix = {"Idle": "Idle", "Run": movement, "Attack": attack_name,
              "Hit": "Hit", "Death": "Death"}
    return SOURCE_ROOT / folder / f"CD_Enemy_{source_name}_{suffix[state]}_4Dir_4Frame_v01.png"


def slice_state(enemy: str, state: str, source: Path) -> None:
    atlas = Image.open(source).convert("RGBA")
    cell_w, cell_h = atlas.width // 4, atlas.height // 4
    top = RIGHT_FACING_ROW * cell_h
    folder = OUTPUT_ROOT / enemy / "Frames" / state
    folder.mkdir(parents=True, exist_ok=True)
    for old in folder.glob("*.png"):
        old.unlink()
    for column in range(4):
        frame = atlas.crop((column * cell_w, top, (column + 1) * cell_w, top + cell_h))
        frame = remove_small_border_bleed(frame)
        frame.save(folder / f"{enemy}_{state}_{column:02d}.png")


def remove_small_border_bleed(frame: Image.Image) -> Image.Image:
    """Remove only tiny disconnected fragments leaking in from adjacent grid cells."""
    alpha = frame.getchannel("A")
    width, height = frame.size
    pixels = alpha.load()
    visited = set()
    components = []
    for y in range(height):
        for x in range(width):
            if pixels[x, y] < 8 or (x, y) in visited:
                continue
            stack, component, touches_border = [(x, y)], [], False
            visited.add((x, y))
            while stack:
                px, py = stack.pop()
                component.append((px, py))
                touches_border |= px == 0 or py == 0 or px == width - 1 or py == height - 1
                for nx, ny in ((px - 1, py), (px + 1, py), (px, py - 1), (px, py + 1)):
                    if 0 <= nx < width and 0 <= ny < height and pixels[nx, ny] >= 8 and (nx, ny) not in visited:
                        visited.add((nx, ny))
                        stack.append((nx, ny))
            components.append((component, touches_border))
    largest = max((len(c) for c, _ in components), default=0)
    cleaned = frame.copy()
    clean_alpha = cleaned.getchannel("A")
    clean_pixels = clean_alpha.load()
    for component, touches_border in components:
        if touches_border and len(component) < largest * .08:
            for x, y in component:
                clean_pixels[x, y] = 0
    cleaned.putalpha(clean_alpha)
    return cleaned


def main() -> None:
    for enemy, (folder, source_name, attack_name) in SOURCES.items():
        for state in ("Idle", "Run", "Attack", "Hit", "Death"):
            source = source_path(folder, source_name, state, attack_name)
            if not source.exists():
                raise FileNotFoundError(source)
            slice_state(enemy, state, source)
        print(f"Sliced clean side-view frames: {enemy}")


if __name__ == "__main__":
    main()
