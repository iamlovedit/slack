# Homebrew Formula for Slack
# To use: 
#   brew tap iamlovedit/tap
#   brew install slack
#
# Or install directly:
#   brew install iamlovedit/tap/slack

class Slack < Formula
  desc "🐟 Fake build output generator - Look busy while slacking off"
  homepage "https://github.com/iamlovedit/slack"
  version "1.0.0"
  license "MIT"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/iamlovedit/slack/releases/download/v#{version}/slack-osx-arm64.tar.gz"
      sha256 "PLACEHOLDER_SHA256_ARM64"
    else
      url "https://github.com/iamlovedit/slack/releases/download/v#{version}/slack-osx-x64.tar.gz"
      sha256 "PLACEHOLDER_SHA256_X64"
    end
  end

  def install
    bin.install "slack"
  end

  test do
    assert_match "slack", shell_output("#{bin}/slack version")
  end
end
