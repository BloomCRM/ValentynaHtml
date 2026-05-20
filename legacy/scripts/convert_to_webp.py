"""
convert_to_webp.py — конвертує топ-N найважчих картинок з wwwroot/img/
у 4 WebP-варіанти (480, 768, 1280, 1920px) у wwwroot/img/webp/.

Пробіли у назвах файлів замінюються на дефіс.
Запуск: python legacy/scripts/convert_to_webp.py
"""

import sys
import io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

from pathlib import Path
from PIL import Image

WIDTHS = [480, 768, 1280, 1920]
QUALITY = 82
TOP_N = 15

ROOT = Path(__file__).resolve().parent.parent.parent
IMG_DIR = ROOT / "src" / "ValentynaWeb" / "wwwroot" / "img"
OUT_DIR = IMG_DIR / "webp"

EXTENSIONS = {".jpg", ".jpeg", ".png", ".JPG", ".JPEG", ".PNG"}


def find_images():
    return [
        p for p in IMG_DIR.iterdir()
        if p.is_file() and p.suffix in EXTENSIONS
    ]


def convert(src_path: Path):
    stem = src_path.stem.replace(" ", "-")
    try:
        with Image.open(src_path) as img:
            if img.mode in ("RGBA", "P", "LA"):
                img = img.convert("RGBA").convert("RGB")
            elif img.mode != "RGB":
                img = img.convert("RGB")

            orig_w, orig_h = img.size
            saved = []

            for target_w in WIDTHS:
                if orig_w >= target_w:
                    h = round(orig_h * target_w / orig_w)
                    resized = img.resize((target_w, h), Image.LANCZOS)
                else:
                    resized = img.copy()  # зберігаємо оригінальний розмір

                out = OUT_DIR / f"{stem}-{target_w}.webp"  # завжди цільова ширина
                resized.save(out, "webp", quality=QUALITY)
                saved.append(f"{out.name} ({out.stat().st_size // 1024} KB)")

            return saved
    except Exception as e:
        return [f"ERROR: {e}"]


def main():
    OUT_DIR.mkdir(exist_ok=True)

    images = find_images()
    images.sort(key=lambda p: p.stat().st_size, reverse=True)
    top = images[:TOP_N]

    total_before = sum(p.stat().st_size for p in top)
    total_after = 0

    print(f"Конвертую топ-{TOP_N} файлів → {OUT_DIR}\n")
    print(f"{'Оригінал':<35} {'Розмір':>8}   Результат")
    print("-" * 80)

    for src in top:
        size_kb = src.stat().st_size // 1024
        print(f"{src.name:<35} {size_kb:>6} KB")
        results = convert(src)
        for r in results:
            print(f"    → {r}")
            if "ERROR" not in r:
                name = r.split(" ")[0]
                out_path = OUT_DIR / name
                if out_path.exists():
                    total_after += out_path.stat().st_size
        print()

    print("-" * 80)
    print(f"До:    {total_before // 1024:>7} KB")
    print(f"Після: {total_after // 1024:>7} KB  (усі варіанти разом)")
    print("Готово!")


if __name__ == "__main__":
    main()
