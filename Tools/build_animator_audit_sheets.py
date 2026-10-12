from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageOps

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets/Screenshots/AnimatorAudit"
PLAYER = ROOT / "Assets/Art/Characters/Player/SideView/Calibrated"
ENEMIES = ROOT / "Assets/Art/Enemies/SideView/Roster"


def font(size: int):
    candidates = [Path("C:/Windows/Fonts/segoeui.ttf"), Path("C:/Windows/Fonts/arial.ttf")]
    for path in candidates:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


TITLE, LABEL, SMALL = font(34), font(24), font(16)


def checker(size):
    im = Image.new("RGB", size, "#172033")
    draw = ImageDraw.Draw(im)
    step = 16
    for y in range(0, size[1], step):
        for x in range(0, size[0], step):
            if (x // step + y // step) % 2 == 0:
                draw.rectangle((x, y, x + step - 1, y + step - 1), fill="#202c43")
    return im


def render_frame(path: Path, mirrored: bool, size=(190, 145)):
    source = Image.open(path).convert("RGBA")
    if mirrored:
        source = ImageOps.mirror(source)
    box = source.getbbox()
    if box:
        source = source.crop(box)
    source.thumbnail((size[0] - 18, size[1] - 20), Image.Resampling.NEAREST)
    cell = checker(size)
    x = (size[0] - source.width) // 2
    y = size[1] - source.height - 8
    cell.paste(source, (x, y), source)
    draw = ImageDraw.Draw(cell)
    draw.line((0, size[1] - 7, size[0], size[1] - 7), fill="#32d8df", width=2)
    return cell


def sheet(name: str, rows):
    label_w, cell_w, cell_h = 260, 190, 145
    frame_count = max(len(paths) for _, paths, _ in rows)
    width = label_w + frame_count * cell_w + 28
    height = 78 + len(rows) * (cell_h + 16) + 24
    canvas = Image.new("RGB", (width, height), "#0b1020")
    draw = ImageDraw.Draw(canvas)
    draw.text((20, 16), name + " — Animator states", font=TITLE, fill="#ffffff")
    y = 72
    for label, paths, mirrored in rows:
        draw.rounded_rectangle((12, y, width - 12, y + cell_h), radius=10, fill="#111a2c", outline="#344665")
        draw.text((25, y + 42), label, font=LABEL, fill="#9debf0")
        draw.text((25, y + 79), f"{len(paths)} frame" + ("s" if len(paths) != 1 else ""), font=SMALL, fill="#aab7cd")
        for index, path in enumerate(paths):
            frame = render_frame(path, mirrored, (cell_w, cell_h))
            canvas.paste(frame, (label_w + index * cell_w, y))
            draw.text((label_w + index * cell_w + 8, y + 6), f"{index:02}", font=SMALL, fill="#ffffff")
        y += cell_h + 16
    OUT.mkdir(parents=True, exist_ok=True)
    canvas.save(OUT / f"{name}_AnimatorAudit.png")


def files(folder: Path):
    return sorted(folder.glob("*.png"))


def main():
    idle, run = files(PLAYER / "Idle"), files(PLAYER / "Run")
    player_states = [("IdleRight", idle, False), ("IdleLeft", idle, True),
                     ("RunRight", run, False), ("RunLeft", run, True),
                     ("JumpRight", [run[1]], False), ("JumpLeft", [run[1]], True)]
    for state in ("Attack", "Hit", "Death"):
        frames = files(PLAYER / state)
        player_states += [(state + "Right", frames, False), (state + "Left", frames, True)]
    sheet("Player", player_states)

    for enemy in ("RuinGuardian", "AshHusk", "RustArcher", "ForgeKnight",
                  "ThornSpitter", "MirrorWisp", "CrystalSentinel"):
        rows = []
        for state in ("Idle", "Run", "Attack", "Hit", "Death"):
            frames = files(ENEMIES / enemy / "Frames" / state)
            rows += [(state + "Right", frames, False), (state + "Left", frames, True)]
        sheet(enemy, rows)


if __name__ == "__main__":
    main()
