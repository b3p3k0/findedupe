#!/usr/bin/env bash
set -euo pipefail

# ==============================================================================
# FinDeDupe — Jellyfin Plugin Project Bootstrap (Ubuntu 25.04)
# Fully automated: installs deps, clones template, renames project/solution,
# wires up admin config page, fixes solution refs, generates StyleCop-friendly
# stubs (param/returns XML docs included), suppresses header rules, and builds.
# ==============================================================================

PLUGIN_NAME="FinDeDupe"
ORG_NS="Jellyfin.Plugin"
PLUGIN_NS="${ORG_NS}.${PLUGIN_NAME}"
TEMPLATE_REPO="https://github.com/jellyfin/jellyfin-plugin-template.git"
SDK_VERSION="8.0"
PROJECT_DIR="$(pwd)"
PLUGIN_GUID="$(uuidgen | tr '[:lower:]' '[:upper:]')"

log()  { printf "\n\033[1;36m[INFO]\033[0m %s\n" "$*"; }
err()  { printf "\n\033[1;31m[ERR]\033[0m %s\n" "$*" >&2; }

install_pkg() {
  local pkg="$1"
  if ! dpkg -s "$pkg" &>/dev/null; then
    log "Installing missing package: $pkg"
    sudo apt-get update -y
    sudo apt-get install -y "$pkg"
  else
    log "$pkg already installed."
  fi
}

# ------------------------------------------------------------------------------
# Dependencies
# ------------------------------------------------------------------------------
log "Checking dependencies…"
install_pkg git
install_pkg wget
install_pkg curl
install_pkg ca-certificates
if ! command -v dotnet &>/dev/null; then
  log "Installing .NET SDK ${SDK_VERSION}…"
  wget -q https://packages.microsoft.com/config/ubuntu/"$(lsb_release -rs)"/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb >/dev/null
  rm -f packages-microsoft-prod.deb
  sudo apt-get update -y
  sudo apt-get install -y "dotnet-sdk-${SDK_VERSION}"
else
  log "dotnet already installed."
fi
if ! dotnet --list-sdks | grep -q "^${SDK_VERSION}\."; then
  err ".NET SDK ${SDK_VERSION} not detected after install."; exit 1
fi

# ------------------------------------------------------------------------------
# Clean previous attempts
# ------------------------------------------------------------------------------
log "Cleaning previous attempts/cruft…"
rm -rf tmp-template "${ORG_NS}.Template" "${ORG_NS}.${PLUGIN_NAME}"
rm -f  "${PLUGIN_NAME}.sln" jellyfin.ruleset Directory.Build.props build.yaml

# ------------------------------------------------------------------------------
# Clone template
# ------------------------------------------------------------------------------
log "Cloning jellyfin-plugin-template…"
git clone --depth 1 "${TEMPLATE_REPO}" tmp-template

log "Copying template into project directory…"
shopt -s dotglob nullglob
cp -r tmp-template/* tmp-template/.[!.]* "${PROJECT_DIR}/" || true
shopt -u dotglob nullglob
rm -rf tmp-template
rm -rf .git .github .vscode || true

# ------------------------------------------------------------------------------
# Detect solution and project
# ------------------------------------------------------------------------------
log "Detecting solution and project…"
SOLUTION_FILE="$(ls *.sln 2>/dev/null | head -n1 || true)"
[[ -z "${SOLUTION_FILE}" ]] && { err "No *.sln found!"; exit 1; }

SRC_PROJECT_DIR="$(find . -maxdepth 1 -type d -name "${ORG_NS}.*" | head -n1 || true)"
[[ -z "${SRC_PROJECT_DIR}" ]] && { err "No ${ORG_NS}.* project dir found!"; exit 1; }

SRC_CSPROJ="$(find "${SRC_PROJECT_DIR}" -maxdepth 1 -name "*.csproj" | head -n1 || true)"
[[ -z "${SRC_CSPROJ}" ]] && { err "No .csproj inside ${SRC_PROJECT_DIR}!"; exit 1; }

# ------------------------------------------------------------------------------
# Rename solution and move project
# ------------------------------------------------------------------------------
NEW_SOLUTION="${PLUGIN_NAME}.sln"
[[ "${SOLUTION_FILE}" != "${NEW_SOLUTION}" ]] && mv "${SOLUTION_FILE}" "${NEW_SOLUTION}" || log "Solution already named ${NEW_SOLUTION}; skipping rename."

NEW_PROJECT_DIR="./${PLUGIN_NS}"
mkdir -p "${NEW_PROJECT_DIR}"

dotnet sln "${NEW_SOLUTION}" remove "${SRC_CSPROJ}" >/dev/null 2>&1 || true

log "Moving template project contents into ${NEW_PROJECT_DIR}…"
shopt -s dotglob nullglob
mv "${SRC_PROJECT_DIR}"/* "${NEW_PROJECT_DIR}/" || true
shopt -u dotglob nullglob
rmdir "${SRC_PROJECT_DIR}" 2>/dev/null || true

FOUND_MOVED_CSPROJ="$(find "${NEW_PROJECT_DIR}" -maxdepth 1 -name "*.csproj" | head -n1 || true)"
[[ -z "${FOUND_MOVED_CSPROJ}" ]] && { err "No .csproj found after moving!"; exit 1; }
TARGET_CSPROJ="${NEW_PROJECT_DIR}/${PLUGIN_NAME}.csproj"
[[ "${FOUND_MOVED_CSPROJ}" != "${TARGET_CSPROJ}" ]] && mv "${FOUND_MOVED_CSPROJ}" "${TARGET_CSPROJ}"

dotnet sln "${NEW_SOLUTION}" add "${TARGET_CSPROJ}"

# ------------------------------------------------------------------------------
# Admin configuration page (embedded)
# ------------------------------------------------------------------------------
log "Scaffolding admin configuration page…"
CONFIG_DIR="${NEW_PROJECT_DIR}/Configuration"
mkdir -p "${CONFIG_DIR}"
cat > "${CONFIG_DIR}/configPage.html" <<HTML
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8"/>
  <title>${PLUGIN_NAME} — Configuration</title>
</head>
<body>
  <div id="${PLUGIN_NAME}ConfigurationPage"
       data-role="page"
       class="page type-interior pluginConfigurationPage"
       data-require="emby-input,emby-button">
    <div data-role="content">
      <div class="content-primary">
        <h2>${PLUGIN_NAME} Settings</h2>
        <form id="${PLUGIN_NAME}ConfigurationForm" style="margin-top:1rem;">
          <div class="inputContainer">
            <label class="inputLabel inputLabelUnpaired">Status</label>
            <div id="${PLUGIN_NAME}Status" class="fieldDescription">Ready.</div>
          </div>
          <div class="formButtons" style="margin-top: 1rem;">
            <button is="emby-button" type="submit" class="raised button-submit block emby-button">
              Save
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>

  <script type="text/javascript">
  (function () {
    'use strict';
    var PLUGIN_ID = '${PLUGIN_GUID}';
    function loadConfig(page) {
      Dashboard.showLoadingMsg();
      ApiClient.getPluginConfiguration(PLUGIN_ID).then(function () {
        page.querySelector('#${PLUGIN_NAME}Status').innerText = 'Loaded.';
        Dashboard.hideLoadingMsg();
      });
    }
    function onSubmit(e) {
      e.preventDefault();
      Dashboard.showLoadingMsg();
      ApiClient.getPluginConfiguration(PLUGIN_ID).then(function (config) {
        return ApiClient.updatePluginConfiguration(PLUGIN_ID, config);
      }).then(function (result) {
        Dashboard.processPluginConfigurationUpdateResult(result);
      });
      return false;
    }
    document.addEventListener('pageshow', function (e) {
      var page = e.target;
      if (page && page.id === '${PLUGIN_NAME}ConfigurationPage') {
        loadConfig(page);
        page.querySelector('#${PLUGIN_NAME}ConfigurationForm').addEventListener('submit', onSubmit);
      }
    });
  })();
  </script>
</body>
</html>
HTML

# ------------------------------------------------------------------------------
# Patch csproj
#   - remove CodeAnalysisRuleSet (if any)
#   - suppress *header* StyleCop rules only (keep param/returns enforced)
# ------------------------------------------------------------------------------
log "Patching ${PLUGIN_NAME}.csproj…"
sed -i '/<CodeAnalysisRuleSet>/d' "${TARGET_CSPROJ}" || true
if grep -q "<NoWarn>" "${TARGET_CSPROJ}"; then
  sed -i 's#<NoWarn>\(.*\)</NoWarn>#<NoWarn>\1;SA1633;SA1636;SA1637;SA1640</NoWarn>#' "${TARGET_CSPROJ}"
else
  awk '
    BEGIN{added=0}
    /<PropertyGroup>/ && added==0 { print; print "    <NoWarn>SA1633;SA1636;SA1637;SA1640</NoWarn>"; added=1; next }
    { print }
  ' "${TARGET_CSPROJ}" > "${TARGET_CSPROJ}.tmp" && mv "${TARGET_CSPROJ}.tmp" "${TARGET_CSPROJ}"
fi

# ------------------------------------------------------------------------------
# Namespace swaps
# ------------------------------------------------------------------------------
log "Renaming namespaces/identifiers…"
find "${NEW_PROJECT_DIR}" -type f -name "*.csproj" -print0 | xargs -0 sed -i "s#${ORG_NS}\.Template#${PLUGIN_NS}#g"
find . -type f \( -name "*.cs" -o -name "*.sln" -o -name "*.json" -o -name "*.html" \) -print0 | xargs -0 sed -i "s#${ORG_NS}\.Template#${PLUGIN_NS}#g"
find "${NEW_PROJECT_DIR}" -type f \( -name "*.cs" -o -name "*.json" -o -name "*.html" \) -print0 | xargs -0 sed -i "s/\bTemplate\b/${PLUGIN_NAME}/g"
find "${NEW_PROJECT_DIR}" -type f \( -name "*.cs" -o -name "*.json" -o -name "*.html" \) -print0 | xargs -0 sed -i "s/PluginTemplate/${PLUGIN_NAME}/g"

# ------------------------------------------------------------------------------
# Generate StyleCop-friendly C# stubs (with param/returns XML docs)
# ------------------------------------------------------------------------------
log "Generating C# stubs…"

cat > "${NEW_PROJECT_DIR}/${PLUGIN_NAME}Plugin.cs" <<CSharp
namespace ${PLUGIN_NS}
{
    using System;
    using System.Collections.Generic;
    using MediaBrowser.Common.Configuration;
    using MediaBrowser.Common.Plugins;
    using MediaBrowser.Model.Plugins;
    using MediaBrowser.Model.Serialization;

    /// <summary>Jellyfin plugin entry point for ${PLUGIN_NAME}.</summary>
    public class ${PLUGIN_NAME}Plugin : BasePlugin<Configuration.PluginConfiguration>, IHasWebPages
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="${PLUGIN_NAME}Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Application paths provider.</param>
        /// <param name="xmlSerializer">XML serializer instance.</param>
        public ${PLUGIN_NAME}Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
        }

        /// <summary>Gets the plugin display name.</summary>
        public override string Name => "${PLUGIN_NAME}";

        /// <summary>Gets the plugin unique identifier.</summary>
        public override Guid Id => Guid.Parse("${PLUGIN_GUID}");

        /// <summary>Gets the plugin description.</summary>
        public override string Description => "Detects and manages duplicate media items.";

        /// <summary>Gets the admin configuration pages exposed by the plugin.</summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PluginPageInfo"/> describing admin pages.</returns>
        public IEnumerable<PluginPageInfo> GetPages()
        {
            yield return new PluginPageInfo
            {
                Name = "Configuration",
                EmbeddedResourcePath = "${PLUGIN_NS}.Configuration.configPage.html",
            };
        }
    }
}
CSharp

mkdir -p "${NEW_PROJECT_DIR}/Configuration"
cat > "${NEW_PROJECT_DIR}/Configuration/PluginConfiguration.cs" <<CSharp
namespace ${PLUGIN_NS}.Configuration
{
    using MediaBrowser.Model.Plugins;

    /// <summary>Configuration root for the ${PLUGIN_NAME} plugin.</summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>Gets or sets a value indicating whether the plugin is enabled.</summary>
        public bool Enabled { get; set; } = true;
    }
}
CSharp

mkdir -p "${NEW_PROJECT_DIR}/Controllers"
cat > "${NEW_PROJECT_DIR}/Controllers/${PLUGIN_NAME}Controller.cs" <<CSharp
namespace ${PLUGIN_NS}.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>Minimal API surface for ${PLUGIN_NAME}.</summary>
    [ApiController]
    [Route("Plugins/${PLUGIN_NAME}")]
    public class ${PLUGIN_NAME}Controller : ControllerBase
    {
        /// <summary>Simple health endpoint to validate plugin wiring.</summary>
        /// <returns>An <see cref="IActionResult"/> indicating success.</returns>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return this.Ok(new { plugin = "${PLUGIN_NAME}", status = "ok" });
        }
    }
}
CSharp

# Remove stray old Plugin.cs, if any
rm -f "${NEW_PROJECT_DIR}/Plugin.cs" || true

# ------------------------------------------------------------------------------
# Init Git (main)
# ------------------------------------------------------------------------------
if [ ! -d .git ]; then
  log "Initializing Git repository…"
  git init -b main >/dev/null 2>&1 || { git init >/dev/null; git branch -m main >/dev/null 2>&1 || true; }
fi
git add .
git commit -m "Scaffold ${PLUGIN_NAME} (GUID=${PLUGIN_GUID}); admin page wired; StyleCop header rules suppressed; XML docs added; solution fixed" >/dev/null || true

# ------------------------------------------------------------------------------
# Restore + Build
# ------------------------------------------------------------------------------
log "Restoring packages…"
dotnet restore "${NEW_SOLUTION}"

log "Building (Debug)…"
dotnet build "${NEW_SOLUTION}" -c Debug /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary

echo ""
echo "=== ${PLUGIN_NAME} setup complete and READY! ==="
echo "Solution: ${NEW_SOLUTION}"
echo "Project : ${NEW_PROJECT_DIR}"
echo "GUID    : ${PLUGIN_GUID}"
echo ""
echo "The plugin should now build cleanly with StyleCop param/returns docs satisfied."
