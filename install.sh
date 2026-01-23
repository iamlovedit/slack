#!/bin/bash
# Slack - Cross-platform installation script for Linux and macOS
# Usage: curl -fsSL https://raw.githubusercontent.com/iamlovedit/slack/main/install.sh | bash

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
            error "Please use install.ps1 for Windows installation"
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

# Download and install
install_slack() {
    local platform version download_url temp_dir archive_name

    info "Detecting platform..."
    platform=$(detect_platform)
    info "Platform: $platform"

    info "Getting latest version..."
    version=$(get_latest_version)
    info "Latest version: $version"

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

    success "Slack has been installed successfully!"
    echo ""
    echo "Run 'slack --help' to get started."
    echo ""
    
    # Verify installation
    if command -v slack &> /dev/null; then
        info "Installed version:"
        slack version
    fi
}

# Check for Homebrew installation option on macOS
check_homebrew() {
    if [ "$(uname -s)" = "Darwin" ]; then
        if command -v brew &> /dev/null; then
            echo ""
            info "💡 Tip: You can also install via Homebrew on macOS:"
            echo "   brew tap iamlovedit/tap"
            echo "   brew install slack"
            echo ""
        fi
    fi
}

# Main
main() {
    echo ""
    echo "🐟 Slack Installer"
    echo "=================="
    echo ""

    # Check for required tools
    if ! command -v curl &> /dev/null; then
        error "'curl' is required but not installed. Please install curl first."
    fi

    if ! command -v tar &> /dev/null; then
        error "'tar' is required but not installed. Please install tar first."
    fi

    install_slack
    check_homebrew
}

main "$@"
