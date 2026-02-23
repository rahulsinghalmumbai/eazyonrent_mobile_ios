#!/usr/bin/env bash
set -euo pipefail

SRC="Platforms/iOS/Assets.xcassets/appicon.appiconset/icon1024.png"
OUTDIR="Platforms/iOS/Assets.xcassets/appicon.appiconset"

if [ ! -f "$SRC" ]; then
  echo "Source not found: $SRC"
  exit 1
fi

mkdir -p "$OUTDIR"

# point size × scale => px size
convert "$SRC" -resize 40x40   "$OUTDIR/icon20@2x.png"
convert "$SRC" -resize 60x60   "$OUTDIR/icon20@3x.png"
convert "$SRC" -resize 58x58   "$OUTDIR/icon29@2x.png"
convert "$SRC" -resize 87x87   "$OUTDIR/icon29@3x.png"
convert "$SRC" -resize 80x80   "$OUTDIR/icon40@2x.png"
convert "$SRC" -resize 120x120 "$OUTDIR/icon40@3x.png"
convert "$SRC" -resize 120x120 "$OUTDIR/icon60@2x.png"   
convert "$SRC" -resize 180x180 "$OUTDIR/icon60@3x.png"
# keep icon1024.png as-is

echo "Generated icons in $OUTDIR"