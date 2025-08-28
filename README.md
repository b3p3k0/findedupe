# FinDeDupe - Jellyfin Duplicate Media Detection Plugin

FinDeDupe is a comprehensive Jellyfin plugin designed to safely detect and manage duplicate media files in your libraries. It uses advanced algorithms to identify duplicates across movies and series while providing multiple safety layers to protect your valuable media collection.

## Features

- **Smart Duplicate Detection**: Advanced title normalization and fuzzy matching algorithms
- **Multi-Media Support**: Handles both movies and TV series with specialized logic for each
- **Safety First**: Dry-run mode by default with comprehensive path validation
- **Flexible Exclusions**: Library, path prefix, and glob pattern exclusion support
- **Admin Control**: Admin-only access with confirmation dialogs for destructive operations
- **Detailed Logging**: Comprehensive CSV logs with configurable retention
- **Provider ID Validation**: Cross-references TVDB, TMDB, and IMDB identifiers

## Installation

### Method 1: Plugin Repository (Recommended)

1. Open your Jellyfin Admin Dashboard
2. Navigate to **Plugins** → **Repositories**
3. Click **Add Repository**
4. Enter the following information:
   - **Repository Name**: `FinDeDupe`
   - **Repository URL**: `https://raw.githubusercontent.com/b3p3k0/findedupe/main/manifest.json`
5. Click **Save**
6. Go to **Plugins** → **Catalog**
7. Find **FinDeDupe** in the catalog and click **Install**
8. Restart your Jellyfin server when prompted

**Important**: Use the raw manifest URL above, not the GitHub repository URL. The manifest URL must point directly to the JSON file.

### Method 2: Manual Installation

1. Download the latest release from the [Releases](../../releases) page
2. Extract the ZIP file to get the `FinDeDupe.dll` file
3. Copy the DLL to your Jellyfin plugins directory:
   - **Linux**: `~/.local/share/jellyfin/plugins/FinDeDupe/`
   - **Windows**: `%LOCALAPPDATA%\jellyfin\plugins\FinDeDupe\`
   - **Docker**: `/config/plugins/FinDeDupe/` (inside container)
4. Restart your Jellyfin server
5. The plugin will appear in your Admin Dashboard under Plugins

### Method 3: Build from Source

Requirements:
- .NET 8.0 SDK
- Git

```bash
git clone https://github.com/b3p3k0/findedupe.git
cd findedupe
dotnet build
```

Copy the built `FinDeDupe.dll` from `Jellyfin.Plugin.FinDeDupe/bin/Debug/net8.0/` to your plugins directory.

## Configuration

Access the plugin configuration through:
**Admin Dashboard → Plugins → FinDeDupe → Settings**

### Core Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Operation Mode** | `DryRun` | Safe mode that only logs potential actions without deleting files |
| **Exact Match Threshold** | `90` | Similarity percentage for high-confidence matches (0-100) |
| **Conditional Match Threshold** | `85` | Similarity percentage requiring additional validation (0-100) |

### Exclusion Rules

#### Library Exclusions
Exclude entire libraries by ID:
```
Library-1234-5678-90ab
Library-abcd-efgh-ijkl
```

#### Path Prefix Exclusions
Exclude specific directory trees:
```
/media/Archive
/storage/Backup
/mnt/old-collection
```

#### Glob Pattern Exclusions
Use wildcards for flexible matching:
```
**/Archive/**        # Any folder named "Archive"
**.sample.**         # Files containing "sample"
**/Season*/**/*.mkv  # MKV files in season folders
```

### Safety Features

- **Dry-Run Default**: All operations are logged but no files are deleted until explicitly changed
- **Path Validation**: All file operations are restricted to configured library roots
- **Admin-Only Access**: Only administrator accounts can access the plugin
- **Confirmation Dialogs**: Multiple confirmation steps for destructive operations
- **Comprehensive Logging**: All operations logged with timestamps and details

## Usage

### Basic Workflow

1. **Initial Setup**
   - Install the plugin and restart Jellyfin
   - Configure exclusion rules for any content you want to protect
   - Leave in DryRun mode for initial testing

2. **Scan for Duplicates**
   - Navigate to Admin Dashboard → Scheduled Tasks
   - Find "Scan for Duplicate Media" task
   - Run manually or schedule for automatic operation

3. **Review Results**
   - Check the generated CSV log files in your Jellyfin data directory
   - Review identified duplicates and their similarity scores
   - Verify the exclusion rules are working correctly

4. **Enable Deletion** (Optional)
   - Change Operation Mode from `DryRun` to `DeleteFiles`
   - **Warning**: This will permanently delete files - ensure you have backups
   - Re-run the scan to perform actual deletions

### Understanding Duplicate Detection

#### Title Normalization
The plugin normalizes titles by:
- Removing quality tags (1080p, BluRay, etc.)
- Removing bracketed content [1999], (Director's Cut)
- Normalizing sequel numbers (II → 2, "Part Two" → "Part 2")
- Standardizing punctuation and spacing

#### Similarity Matching
Three algorithms work together:
- **Token Set Ratio**: Compares unique words between titles
- **Token Sort Ratio**: Compares alphabetically sorted words
- **Levenshtein Distance**: Character-level similarity

#### Match Types
- **Exact Match** (≥90% similarity): High confidence, immediate match
- **Conditional Match** (≥85% similarity): Requires year match or shared provider IDs
- **Provider ID Match**: Exact TVDB/TMDB/IMDB ID matches override similarity scores

### Log Files

Logs are stored in CSV format in your Jellyfin data directory:
- `findedupe-scan-YYYY-MM-DD.csv`: Scan results and actions taken
- `findedupe-error-YYYY-MM-DD.csv`: Error logs and warnings

Log retention is automatically managed (default: 30 days).

## Troubleshooting

### Common Issues

**Plugin doesn't appear in dashboard**
- Verify the DLL is in the correct plugins directory
- Check that you restarted Jellyfin after installation
- Ensure you're logged in as an administrator

**No duplicates detected**
- Check that your libraries are properly configured in Jellyfin
- Verify exclusion rules aren't too broad
- Review the scan logs for any errors

**False positives/negatives**
- Adjust similarity thresholds in configuration
- Use exclusion rules to protect specific content
- Report persistent issues on GitHub

**Permission errors**
- Ensure Jellyfin has read/write access to media directories
- Check that library paths are correctly configured
- Verify file system permissions

### Getting Help

1. **Check the logs** - Most issues are explained in the CSV log files
2. **Review exclusions** - Ensure your rules aren't blocking intended scans
3. **Test in dry-run** - Always test configuration changes in dry-run mode first
4. **GitHub Issues** - Report bugs and request features at [GitHub Issues](../../issues)

### Performance Considerations

- **Large Libraries**: For 20k+ items, consider running scans during off-peak hours
- **Storage**: Ensure adequate free space for log files and temporary operations
- **Network**: For network storage, ensure stable connections during scans

## Safety & Legal

### Data Protection
- **Always backup** your media collection before enabling deletion mode
- Test thoroughly with dry-run mode before making changes
- Use exclusion rules to protect irreplaceable content

### Legal Compliance
- This plugin only manages files you already own
- Ensure compliance with local copyright laws
- The plugin does not download or distribute copyrighted content

## Development

### Contributing
Contributions are welcome! Please see:
- [COLLAB.md](COLLAB.md) for development guidelines
- [concept.txt](concept.txt) for technical specifications
- [CLAUDE.md](CLAUDE.md) for AI development guidance

### Building
```bash
dotnet build                    # Build the plugin
dotnet test                     # Run unit tests
dotnet publish -c Release       # Build release version
```

### Testing
The plugin includes comprehensive unit tests covering:
- Title normalization algorithms
- Fuzzy matching logic
- Exclusion rule processing
- Configuration validation

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.

Due to linking with Jellyfin's GPLv3 libraries, the compiled plugin is also distributed under GPLv3.

## Changelog

### Version 1.0.0 (Phase 1)
- Initial release with core duplicate detection
- Advanced title normalization and fuzzy matching
- Comprehensive safety features and exclusion system
- Dry-run mode with detailed CSV logging
- Support for both movies and TV series
