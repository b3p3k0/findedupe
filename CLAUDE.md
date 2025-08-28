# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Building
```bash
dotnet build FinDeDupe.sln
dotnet publish FinDeDupe.sln  # For release builds
```

### Testing
Check the project structure for test projects - none currently exist but should be added following xUnit conventions.

### Code Quality
The project uses StyleCop analyzers with custom rules defined in `jellyfin.ruleset`. Key suppressions include:
- SA1633, SA1636, SA1637, SA1640 (file header requirements suppressed via NoWarn)
- Various formatting and documentation rules customized for Jellyfin development

## Project Architecture

**Target Framework**: .NET 8.0  
**Type**: Jellyfin Plugin for duplicate media management  
**Primary Focus**: Series AND movie duplicate detection and removal (v0 scope includes both media types)

### Core Architecture Components

Based on the comprehensive technical blueprint in `concept.txt`, the plugin implements:

1. **TitleNormalizer** - Pure functions for title normalization, handling English/Roman numerals, edition tags, and punctuation for both series and movies
2. **FuzzyMatcher** - Token-based similarity matching with configurable thresholds (≥90 exact, 85-89 with year/provider ID match)
3. **DuplicateScanService** - Media enumeration with exclusion rules, fingerprinting, and duplicate grouping for series and movies
4. **DeletionService** - Safe deletion with Jellyfin API calls plus filesystem cleanup (sidecars, empty folders)
5. **ExclusionEngine** - Library ID, path prefix, and glob pattern exclusions
6. **Admin Controller** - REST API endpoints for scan/plan/execute operations supporting both media types
7. **Embedded Admin UI** - Vanilla HTML/JS interface for admin users with media type selection

### Key Data Structures
- `MediaFingerprint` - Media metadata with title normalization, provider IDs, file paths (supports both Series and Movie)
- `DuplicateGroup` - Grouped candidates with suggested keeper, differentiated by media type
- `DeletePlan` - Execution plan with byte calculations and folder removal preview
- `DeleteResult` - Per-run results with CSV logging

### Security & Safety Features
- **Admin-only access** - Server-side authorization enforcement
- **Typed confirmations** - Exact phrase required for destructive operations ("DELETE N ITEMS")
- **Path safety** - Ancestry checks, library root validation, no directory traversal
- **Atomic operations** - Single active execution, idempotent per item
- **Comprehensive logging** - CSV logs with configurable retention (default 45 days)

### Plugin Structure
```
Jellyfin.Plugin.FinDeDupe/
├── Configuration/
│   ├── PluginConfiguration.cs     # Settings model with exclusions
│   └── configPage.html           # Admin configuration UI
├── Controllers/
│   └── FinDeDupeController.cs    # REST API endpoints
└── FinDeDupePlugin.cs           # Main plugin entry point
```

## Critical Implementation Guidelines

1. **Dual media type support**: v0 supports both Series and Movies - all code must handle both media types appropriately
2. **No shortcuts**: Full safety validation at every step, no "best guess" operations
3. **Exclusion precedence**: Exclusions always win over inclusion heuristics
4. **Cancellation support**: Long operations must accept `CancellationToken`
5. **Structured logging**: Use RunId correlation and standard error codes
6. **Path validation**: All file operations must validate paths are under library roots
7. **Quality gates**: All unit tests must pass, no critical analyzer errors

## API Specification

Base route: `/Plugins/FinDeDupe`

- `GET /scan?cursor=&pageSize=50&kind={Series|Movie}` - Scan for duplicates by media type
- `POST /plan` - Build deletion plan from keeper/delete selections
- `POST /execute` - Execute deletion with typed confirmation
- `GET /log/{runId}` - Retrieve execution results and CSV
- `DELETE /logs/purge` - Purge old logs per retention policy

## Dependencies

- **Jellyfin.Controller** 10.9.11 - Core Jellyfin API access
- **Jellyfin.Model** 10.9.11 - Data models and interfaces  
- **StyleCop.Analyzers** - Code style enforcement
- **SerilogAnalyzer** - Logging best practices
- **MultithreadingAnalyzer** - Thread safety validation

The plugin follows the Jellyfin plugin template pattern with embedded web pages and admin-only access controls.

## GUID & Identity
Plugin GUID: `AA3B5C55-4B15-4D3F-8078-D664C80F7D89` (stable, recorded in scaffolding)