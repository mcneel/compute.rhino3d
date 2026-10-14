# Configure the firewall
#Requires -RunAsAdministrator

#Region funcs
. "$PSScriptRoot\common.ps1"
#EndRegion funcs

Write-Step 'Allowing ping'
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
