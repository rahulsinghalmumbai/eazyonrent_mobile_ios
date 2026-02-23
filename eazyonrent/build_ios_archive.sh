#!/bin/bash
# make the script executable
#chmod +x build_ios_archive.sh

# Exit immediately if a command exits with a non-zero status
set -e

echo "🚀 Starting iOS Archive Build..."

# Run the publish command to create the archive
dotnet publish eazyonrent.csproj -f net10.0-ios -c Release -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64

echo "✅ Build complete. Locating the generated .xcarchive..."

# Find the most recently created .xcarchive
ARCHIVE_PATH=$(find ~/Library/Developer/Xcode/Archives -type d -name "*.xcarchive" -print0 | xargs -0 ls -td 2>/dev/null | head -n 1)

if [ -n "$ARCHIVE_PATH" ]; then
    echo "📂 Found archive at: $ARCHIVE_PATH"
    echo "💻 Opening in Xcode Organizer..."
    # Open the archive which triggers Xcode Organizer
    open "$ARCHIVE_PATH"
else
    echo "❌ Error: Could not find any .xcarchive in ~/Library/Developer/Xcode/Archives"
    exit 1
fi
