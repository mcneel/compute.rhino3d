#!/bin/bash
# Signs, and optionally notarizes, the Mac Hops build: src/sign_hops_mac.sh src/dist
# MAC_SIGNING_IDENTITY defaults to the Developer ID Application certificate in the keychain ("-" signs ad hoc for local testing).
# Set NOTARY_PROFILE to a profile saved with `xcrun notarytool store-credentials` to also notarize.
set -euo pipefail

dir="${1:?usage: sign_hops_mac.sh <folder>}"
identity="${MAC_SIGNING_IDENTITY:-Developer ID Application}"
entitlements="$(cd "$(dirname "$0")" && pwd)/hops_mac.entitlements"
log="${NOTARIZATION_LOG:-notarization_log.json}"
timestamp=--timestamp
[[ "$identity" == "-" ]] && timestamp=

sign() {
    codesign --force $timestamp --options runtime --sign "$identity" "$@"
}

# Notarization needs every Mach-O signed with the hardened runtime. Libraries first, then the executables that load them.
libraries=()
executables=()
while IFS= read -r -d '' f; do
    case "$(file -b "$f")" in
        *Mach-O*executable*) executables+=("$f") ;;
        *Mach-O*) libraries+=("$f") ;;
    esac
done < <(find "$dir" -type f -print0)

for f in "${libraries[@]}"; do
    sign "$f"
done
for f in "${executables[@]}"; do
    case "$(basename "$f")" in
        rhino.compute|compute.geometry) sign --entitlements "$entitlements" "$f" ;;
        *) sign "$f" ;;
    esac
    codesign --verify --strict "$f"
done
echo "Signed ${#libraries[@]} libraries and ${#executables[@]} executables in $dir"

if [[ -z "${NOTARY_PROFILE:-}" ]]; then
    echo "NOTARY_PROFILE not set; skipping notarization"
    exit 0
fi

# Bare executables can't be stapled; macOS looks the ticket up online instead.
zip="$(mktemp -d)/hops-mac-notarize.zip"
ditto -c -k --keepParent "$dir" "$zip"
result=$(xcrun notarytool submit "$zip" --keychain-profile "$NOTARY_PROFILE" --wait --no-progress --output-format json)
echo "$result"
id=$(echo "$result" | python3 -c 'import json,sys; print(json.load(sys.stdin)["id"])')
xcrun notarytool log "$id" --keychain-profile "$NOTARY_PROFILE" "$log" || true
status=$(echo "$result" | python3 -c 'import json,sys; print(json.load(sys.stdin)["status"])')
if [[ "$status" != "Accepted" ]]; then
    echo "Notarization failed: $status (see $log)"
    exit 1
fi
echo "Notarized"
