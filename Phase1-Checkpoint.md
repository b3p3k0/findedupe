# Human Testing Checkpoint #1 - Phase 1 Foundation Complete

## What Has Been Implemented

### ✅ Core Architecture
- **Plugin Structure**: Verified compliance with Jellyfin plugin template requirements
- **Data Models**: Complete set of models for MediaFingerprint, DuplicateGroup, DeletePlan, etc.
- **Configuration**: Comprehensive plugin configuration with dry-run defaults and exclusion settings
- **Service Layer**: Three core services implemented:
  - **TitleNormalizer**: Handles movie/series title normalization with sequel detection, foreign titles, quality tags
  - **FuzzyMatcher**: Token-based similarity matching with configurable thresholds and provider ID matching
  - **ExclusionEngine**: Library ID, path prefix, and glob pattern exclusions with security validation

### ✅ Test Framework
- **Unit Tests**: Comprehensive test coverage for all pure functions
- **Test Data Generator**: Realistic movie and series data with known duplicates for testing
- **xUnit Framework**: Standard testing setup with Moq for mocking

### ✅ Safety & Security Features
- **Path Validation**: All file operations validate paths are under library roots
- **Dry-Run Default**: Safe operation mode as default setting
- **Exclusion System**: Multiple layers of content filtering
- **Structured Error Handling**: Standard error codes and structured logging

## What Needs Testing

### Configuration Testing
1. **Plugin Installation**:
   - Does the plugin appear in Jellyfin admin plugins list?
   - Can you access the configuration page?
   - Are default settings sensible?

2. **Configuration Interface**:
   - Test exclusion settings (library IDs, path prefixes, glob patterns)
   - Verify dry-run mode is default
   - Check threshold settings are clear

### Core Service Testing
1. **Title Normalization**:
   - Test with real movie/series titles from your library
   - Verify sequel detection works (The Matrix vs The Matrix Reloaded)
   - Check foreign title handling
   - Test quality tag removal (1080p, BluRay, etc.)

2. **Fuzzy Matching**:
   - Test similarity scoring with actual duplicate titles
   - Verify provider ID matching works
   - Check false positive rate with different titles

3. **Exclusion Logic**:
   - Test library exclusion
   - Test path prefix exclusion
   - Test glob pattern exclusion (e.g., `**/Archive/**`)

## Test Scenarios

### Scenario 1: Basic Title Normalization
Test these title pairs - they should normalize to the same base:
- "The Matrix [1999] [1080p]" vs "The Matrix (BluRay)"
- "Spider-Man: No Way Home" vs "Spider-Man No Way Home"
- "The Lord of the Rings: Part II" vs "The Lord of the Rings 2"

### Scenario 2: Similarity Matching
Test these pairs for correct similarity scoring:
- Should match (>90%): "Inception" vs "Inception [2160p]"
- Should conditionally match (85-89%): "The Dark Knight" vs "Dark Knight" (if same year)
- Should NOT match: "The Matrix" vs "The Avengers"

### Scenario 3: Exclusions
Test exclusion rules work correctly:
- Exclude a test library and verify items are filtered
- Set path prefix exclusion like `/media/Archive` and test
- Use glob pattern `**.sample.**` and verify sample files are excluded

## Known Issues

### StyleCop Compliance
- Multiple formatting issues (missing newlines, using directives order)
- Documentation requirements for boolean properties
- Collection type preferences (List vs ReadOnlyCollection)

**Status**: Non-blocking for functionality testing, will be fixed in Phase 2

### Missing Integration
- No Jellyfin API integration yet (scanning, deletion services)
- No web UI implementation 
- No background processing

**Status**: Planned for Phases 2-3

## Test Questionnaire

Please test the above scenarios and provide feedback using this format:

### 1. Installation & Configuration (1-5 scale)
- Plugin installation difficulty: ___
- Configuration page usability: ___
- Default settings appropriateness: ___
- Any installation errors: ___

### 2. Title Normalization Accuracy (Test with your real titles)
- Provide 5 movie titles from your library: ___
- How many normalized correctly: ___
- Any incorrect normalizations: ___
- Overall accuracy (1-5): ___

### 3. Fuzzy Matching Testing
- Test similarity with 5 duplicate pairs: ___
- False positives found: ___
- False negatives found: ___
- Matching accuracy (1-5): ___

### 4. Exclusion Testing
- Library exclusion works: Y/N
- Path prefix exclusion works: Y/N  
- Glob pattern exclusion works: Y/N
- Any unexpected exclusions: ___

### 5. Overall Assessment
- Code quality feels production-ready: Y/N
- Confident in core algorithms: Y/N
- Major concerns: ___
- Suggested improvements: ___

## Next Steps After Testing
- Address any critical feedback from testing
- Fix StyleCop compliance issues
- Begin Phase 2: Scanning Engine & Background Processing
- Start Jellyfin API integration work

---
**Estimated Testing Time**: 2-3 hours
**Focus Areas**: Core algorithm accuracy, configuration usability, safety features