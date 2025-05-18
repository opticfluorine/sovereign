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
# This script is used to remove an existing Sovereign Server service.
#
# -----
#

param(
    [string]$ServiceName = "SovereignServer",
    [string]$User = "SovereignServiceUser"
)

$expectedExe = Join-Path -Path (Get-Location) -ChildPath "Sovereign.Server.exe"

# Get the service
$service = Get-WmiObject -Class Win32_Service -Filter "Name='$ServiceName'" -ErrorAction SilentlyContinue

if ($null -eq $service) {
    Write-Host "Service '$ServiceName' not found."
    exit 0
}

# Check the executable path
if ($service.PathName -replace '"','' -ieq $expectedExe) {
    # Stop the service if running
    if ($service.State -eq "Running") {
        Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    }
    # Remove the service
    sc.exe remove $ServiceName
    Write-Host "Service '$ServiceName' removed."
    
    # Remove the user if it matches
    if ($service.StartName -eq $User -or $service.StartName -eq ".\$User") {
        $userObj = Get-LocalUser -Name $User -ErrorAction SilentlyContinue
        if ($null -ne $userObj) {
            Remove-LocalUser -Name $User
            Write-Host "User '$User' removed."
        }
    }
} else {
    Write-Host "Service '$ServiceName' does not use the expected executable path. No action taken."
}
