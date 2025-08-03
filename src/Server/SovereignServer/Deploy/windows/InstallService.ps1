# Sovereign Enginne
# Copyright (c) 2025 opticfluorine
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see <https://www.gnu.org/licenses/>.

#
# ------
#

# This script is used to productionize a Sovereign Engine server on Windows.
# It installs Sovereign.Server.exe as a Windows service and optionally restricts 
# permissions on sensitive folders.
#
# Usage:
# .\InstallService.ps1 [-RestrictPermissions] [-ServiceName name] [-ServiceDisplayName displayName]
#
# Options:
# -RestrictPermissions: Restrict permissions on the Data directory. 
# -ServiceName: Specify the name of the service (default is "SovereignServer").
# -ServiceDisplayName: Specify the display name of the service (default is "Sovereign Server").
#

#
# -----
#

param (
    [switch]$RestrictPermissions,
    [string]$ServiceName = "SovereignServer",
    [string]$ServiceDisplayName = "Sovereign Server"
)

#####################
## 1. Create Service
#####################

# Install Sovereign.Server.exe as a Windows service
$exePath = Join-Path -Path (Get-Location) -ChildPath "Sovereign.Server.exe"
$serviceExists = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if (-not $serviceExists) {
    Write-Host "Installing Sovereign.Server.exe as a Windows service."
    sc.exe create $ServiceName binpath= "`"$exePath`"" DisplayName= "$ServiceDisplayName" obj= "NT AUTHORITY\LocalService" type= own start= auto
    sc.exe description $ServiceName "Server for Sovereign Engine."
    sc.exe sidtype $ServiceName restricted
} else {
    Write-Host "Service $ServiceName already exists."
}

Write-Host "Configuring permissions for production."
$sid = "NT SERVICE\$ServiceName"

####
## Default to ReadAndExecute permissions for the service user.
####

$rootPath = Get-Location
$rootAcl = Get-Acl -Path $rootPath
$rootServiceRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $sid, 
    "ReadAndExecute", 
    "ContainerInherit,ObjectInherit", 
    "None", 
    "Allow")
$rootAcl.AddAccessRule($rootServiceRule)
Set-Acl -Path $rootPath -AclObject $rootAcl

####
## Set inheritance on appsettings.json as the installer sometimes disables it.
####

$appsettingPath = Join-Path -Path $rootPath -ChildPath "appsettings.json"
$appsettingAcl = Get-Acl -Path $appsettingPath
$appsettingAcl.SetAccessRuleProtection($false, $true) | Out-Null # Enable inheritance
Set-Acl -Path $appsettingPath -AclObject $appsettingAcl

####
## Restrict permissions on the Data directory.
####

$dataDir = Join-Path -Path (Get-Location) -ChildPath "Data"
if (-not (Test-Path -Path $dataDir)) {
    Write-Host "Data directory does not exist: $dataDir"
    exit 1
}

$dataAcl = Get-Acl -Path $dataDir
if ($RestrictPermissions) {
    $dataAcl.SetAccessRuleProtection($true, $false) | Out-Null # Disable inheritance
    $dataAcl.Access | ForEach-Object { $dataAcl.RemoveAccessRule($_) } | Out-Null # Remove existing rules

    # Administrators: full control
    $adminRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        "Administrators", 
        "FullControl", 
        "ContainerInherit,ObjectInherit", 
        "None", 
        "Allow")
    $dataAcl.AddAccessRule($adminRule)
}

# Service: full control
$serviceUserRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $sid, 
    "FullControl", 
    "ContainerInherit,ObjectInherit", 
    "None", 
    "Allow")
$dataAcl.AddAccessRule($serviceUserRule)

Set-Acl -Path $dataDir -AclObject $dataAcl

####
## Restrict permissions on the Data\Scripts directory
####

$scriptsDir = Join-Path -Path $dataDir -ChildPath "Scripts"
if (-not (Test-Path -Path $scriptsDir)) {
    Write-Host "Scripts directory does not exist: $scriptsDir"
    exit 1
}

$scriptsAcl = Get-Acl -Path $scriptsDir
if ($RestrictPermissions) {
    $scriptsAcl.SetAccessRuleProtection($true, $false) | Out-Null # Disable inheritance
    $scriptsAcl.Access | ForEach-Object { $scriptsAcl.RemoveAccessRule($_) } | Out-Null # Remove existing rules

    # Administrators: full control
    $adminScriptsRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        "Administrators",
        "FullControl",
        "ContainerInherit,ObjectInherit",
        "None",
        "Allow"
    )
    $scriptsAcl.AddAccessRule($adminScriptsRule)
}

# Service user: read-only
# (Mitigate any vulnerabilities that would allow writing and executing malicious scripts)
$serviceUserReadRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $sid,
    "ReadAndExecute",
    "ContainerInherit,ObjectInherit",
    "None",
    "Allow"
)
$scriptsAcl.AddAccessRule($serviceUserReadRule)

Set-Acl -Path $scriptsDir -AclObject $scriptsAcl

####
## Restrict permissions on the Data\Packages directory
####

$packagesDir = Join-Path -Path $dataDir -ChildPath "Packages"
if (-not (Test-Path -Path $packagesDir)) {
    Write-Host "Packages directory does not exist: $packagesDir"
    exit 1
}

$packagesAcl = Get-Acl -Path $packagesDir
if ($RestrictPermissions) {
    $packagesAcl.SetAccessRuleProtection($true, $false) | Out-Null # Disable inheritance
    $packagesAcl.Access | ForEach-Object { $packagesAcl.RemoveAccessRule($_) } | Out-Null # Remove existing rules

    # Administrators: full control
    $adminPackagesRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        "Administrators",
        "FullControl",
        "ContainerInherit,ObjectInherit",
        "None",
        "Allow"
    )
    $packagesAcl.AddAccessRule($adminPackagesRule)
}

# Service user: read-only
# (Mitigate any vulnerabilities that would allow writing and executing malicious packages)
$serviceUserReadRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $sid,
    "ReadAndExecute",
    "ContainerInherit,ObjectInherit",
    "None",
    "Allow"
)
$packagesAcl.AddAccessRule($serviceUserReadRule)

Set-Acl -Path $packagesDir -AclObject $packagesAcl

####
## Restrict permissions on the Logs directory.
####

$logsDir = Join-Path -Path (Get-Location) -ChildPath "Logs"
if (-not (Test-Path -Path $logsDir)) {
    Write-Host "Logs directory does not exist: $logsDir"
    exit 1
}

$logsAcl = Get-Acl -Path $logsDir
if ($RestrictPermissions) {
    $logsAcl.SetAccessRuleProtection($true, $false) | Out-Null # Disable inheritance
    $logsAcl.Access | ForEach-Object { $logsAcl.RemoveAccessRule($_) } | Out-Null # Remove existing rules

    # Administrators: full control
    $adminLogsRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        "Administrators", 
        "FullControl", 
        "ContainerInherit,ObjectInherit", 
        "None", 
        "Allow")
    $logsAcl.AddAccessRule($adminLogsRule)
}

# Service user: full control
$serviceLogsRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $sid,
    "FullControl", 
    "ContainerInherit,ObjectInherit", 
    "None", 
    "Allow")
$logsAcl.AddAccessRule($serviceLogsRule)

Set-Acl -Path $logsDir -AclObject $logsAcl

Write-Host "Directory permissions configured successfully."
