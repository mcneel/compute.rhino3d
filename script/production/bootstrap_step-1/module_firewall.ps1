# Configure firewall and URL reservations
#Requires -RunAsAdministrator

#Region funcs
. "$PSScriptRoot\common.ps1"
#EndRegion funcs

# Setup URL reservations with firewall
Write-Step 'Configuring URL reservation (80 and 443)'
Start-Process "netsh" -ArgumentList "http", "add", "urlacl", "url='http://+:80/'", "user='Everyone'"
Start-Process "netsh" -ArgumentList "http", "add", "urlacl", "url='https://+:443/'", "user='Everyone'"

# Add firewall rules for ICMP communication (ping); a second run finds them already there.
$pingRules = @(
    @{ DisplayName = "ICMP Allow incoming V4 echo request"; Protocol = "ICMPv4"; IcmpType = 8 },
    @{ DisplayName = "ICMP Allow incoming V6 echo request"; Protocol = "ICMPv6"; IcmpType = 128 }
)
foreach ($rule in $pingRules) {
    if (-not (Get-NetFirewallRule -DisplayName $rule.DisplayName -ErrorAction SilentlyContinue)) {
        New-NetFirewallRule @rule -Direction Inbound -Action Allow | Out-Null
    }
}
