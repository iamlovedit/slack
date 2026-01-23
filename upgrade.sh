#!/bin/bash
# Slack - Cross-platform upgrade script for Linux and macOS
# Usage: curl -fsSL https://raw.githubusercontent.com/iamlovedit/slack/master/upgrade.sh | bash

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
REPO="iamlovedit/slack"
BINARY_NAME="slack"
INSTALL_DIR="${INSTALL_DIR:-/usr/local/bin}"

# Print colored message
info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
    exit 1
}

# Detect OS and architecture
detect_platform() {
    local os arch

    case "$(uname -s)" in
        Linux*)
            os="linux"
            # Check if musl (Alpine Linux, etc.)
            if ldd --version 2>&1 | grep -qi musl; then
                os="linux-musl"
            fi
            ;;
        Darwin*)
            os="osx"
            ;;
        MINGW*|MSYS*|CYGWIN*)
            error "Please use upgrade.ps1 for Windows"
            ;;
        *)
            error "Unsupported operating system: $(uname -s)"
            ;;
    esac

    case "$(uname -m)" in
        x86_64|amd64)
            arch="x64"
            ;;
        arm64|aarch64)
            arch="arm64"
            ;;
        *)
            error "Unsupported architecture: $(uname -m)"
            ;;
    esac

    echo "${os}-${arch}"
}

# Get the latest release version from GitHub
get_latest_version() {
    local version
    version=$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" 2>/dev/null | grep '"tag_name":' | sed -E 's/.*"([^"]+)".*/\1/')
    
    if [ -z "$version" ]; then
        error "Failed to get latest version. Please check your internet connection."
    fi
    
    echo "$version"
}

# Get current installed version
get_current_version() {
    if command -v slack &> /dev/null; then
        # Try to get version, remove 'v' prefix if present
        local ver
        ver=$(slack version 2>/dev/null | grep -oE '[0-9]+\.[0-9]+\.[0-9]+' | head -1)
        if [ -n "$ver" ]; then
            echo "v$ver"
        else
            echo ""
        fi
    else
        echo ""
    fi
}

# Compare versions (returns 0 if v1 < v2, 1 otherwise)
version_lt() {
    local v1="${1#v}"
    local v2="${2#v}"
    
    # Use sort -V for version comparison
    [ "$(printf '%s\n' "$v1" "$v2" | sort -V | head -n1)" = "$v1" ] && [ "$v1" != "$v2" ]
}

# Download and upgrade
upgrade_slack() {
    local platform version current_version download_url temp_dir archive_name

    info "Checking current installation..."
    current_version=$(get_current_version)
    
    if [ -z "$current_version" ]; then
        warn "Slack is not installed. Running installation instead..."
        echo ""
        # Download and run install script
        curl -fsSL "https://raw.githubusercontent.com/${REPO}/master/install.sh" | bash
        exit 0
    fi
    
    info "Current version: $current_version"

    info "Detecting platform..."
    platform=$(detect_platform)
    info "Platform: $platform"

    info "Getting latest version..."
    version=$(get_latest_version)
    info "Latest version: $version"

    # Compare versions
    if ! version_lt "$current_version" "$version"; then
        success "You already have the latest version ($current_version). No upgrade needed."
        exit 0
    fi

    info "Upgrading from $current_version to $version..."

    # Construct download URL
    archive_name="slack-${platform}.tar.gz"
    download_url="https://github.com/${REPO}/releases/download/${version}/${archive_name}"

    info "Downloading from: $download_url"

    # Create temp directory
    temp_dir=$(mktemp -d)
    trap "rm -rf $temp_dir" EXIT

    # Download archive
    if ! curl -fsSL "$download_url" -o "${temp_dir}/${archive_name}"; then
        error "Failed to download ${archive_name}. Please check if the release exists for your platform."
    fi

    info "Extracting..."
    tar -xzf "${temp_dir}/${archive_name}" -C "$temp_dir"

    # Find the binary
    if [ ! -f "${temp_dir}/${BINARY_NAME}" ]; then
        error "Binary not found in archive"
    fi

    # Make executable
    chmod +x "${temp_dir}/${BINARY_NAME}"

    # Install to destination
    info "Installing to ${INSTALL_DIR}..."
    
    if [ -w "$INSTALL_DIR" ]; then
        mv "${temp_dir}/${BINARY_NAME}" "${INSTALL_DIR}/${BINARY_NAME}"
    else
        warn "Need sudo permission to install to ${INSTALL_DIR}"
        sudo mv "${temp_dir}/${BINARY_NAME}" "${INSTALL_DIR}/${BINARY_NAME}"
    fi

    success "Slack has been upgraded successfully from $current_version to $version!"
    echo ""
    
    # Verify installation
    if command -v slack &> /dev/null; then
        info "New version:"
        slack version
    fi
}

# Check for Homebrew upgrade option on macOS
check_homebrew() {
    if [ "$(uname -s)" = "Darwin" ]; then
        if command -v brew &> /dev/null; then
            echo ""
            info "💡 Tip: If you installed via Homebrew, use:"
            echo "   brew upgrade slack"
            echo ""
        fi
    fi
}

# Main
main() {
    echo ""
    echo "🐟 Slack Upgrader"
    echo "================="
    echo ""

    # Check for required tools
    if ! command -v curl &> /dev/null; then
        error "'curl' is required but not installed. Please install curl first."
    fi

    if ! command -v tar &> /dev/null; then
        error "'tar' is required but not installed. Please install tar first."
    fi

    upgrade_slack
    check_homebrew
}

main "$@"
