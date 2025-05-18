; Sovereign Engine - Windows Installer
; Copyright (c) 2025 opticfluorine
;
; This program is free software: you can redistribute it and/or modify
; it under the terms of the GNU General Public License as published by
; the Free Software Foundation, either version 3 of the License, or
; (at your option) any later version.
;
; This program is distributed in the hope that it will be useful,
; but WITHOUT ANY WARRANTY; without even the implied warranty of
; MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
; GNU General Public License for more details.
;
; You should have received a copy of the GNU General Public License
; along with this program.  If not, see <https://www.gnu.org/licenses/>.

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;
; This is the NSIS script for the Sovereign Engine installer on Windows.
; The installer supports the optional installation of the client and/or
; the server. It also provides an option to "productionize" the server
; during installation (i.e. apply recommended settings and install as
; a Windows Service).
;
; To build the installer:
;   1. Run `dotnet publish` on both SovereignClient and SovereignServer.
;      Use the Release configuration and the win-x64 target.
;      A single-file self-contained build is recommended.
;      Optionally, redirect the publish output directory to somewhere more
;      convenient than the usual deeply nested location.
;   2. From this directory, build the installer with `makensis sovereign.nsi`.
;      If you changed the publish output directories in step 1, pass the
;      -DCLIENT_PUBLISH_DIR=... and -DSERVER_PUBLISH_DIR=... options to `makensis`
;      with the correct paths.
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

Name "Sovereign Engine"
OutFile "Sovereign_Install.exe"

!define MULTIUSER_EXECUTIONLEVEL Highest
!define MULTIUSER_MUI
!define MULTIUSER_INSTALLMODEPAGE_TEXT_CURRENTUSER "Install just for me (recommended for players and development servers)"
!define MULTIUSER_INSTALLMODEPAGE_TEXT_ALLUSERS "Install for all users (recommended for production servers)"
!define MULTIUSER_USE_PROGRAMFILES64
!define MULTIUSER_INSTALLMODE_DEFAULT_CURRENTUSER
!define MULTIUSER_INSTALLMODE_INSTDIR "$(^Name)"

!include "MultiUser.nsh"
!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "x64.nsh"
!include "Contrib\ReplaceUtils.nsh"

!ifndef CLIENT_PUBLISH_DIR
!define CLIENT_PUBLISH_DIR "..\..\Client\SovereignClient\bin\Release\net9.0\publish\win-x64"
!endif

!ifndef SERVER_PUBLISH_DIR
!define SERVER_PUBLISH_DIR "..\..\Server\SovereignServer\bin\Release\net9.0\publish\win-x64"
!endif

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "..\..\..\LICENSE"
!insertmacro MULTIUSER_PAGE_INSTALLMODE
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
Page custom ServerOptions OnServerOptionsNextPage
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

;;;;;;;;;;;;;
;; Globals ;;
;;;;;;;;;;;;;

; Server Options dialog and controls
Var ServerOptionsDialog
Var CheckboxNoGrantAdminRole
Var CheckboxInstallWindowsService
Var CheckboxRestrictPermissions

; Server Options state
Var NoGrantAdminRole
Var InstallWindowsService
Var RestrictPermissions


;;;;;;;;;;;;
;; Client ;;
;;;;;;;;;;;;

Section "Client" SecClient
    SetOutPath "$INSTDIR\Client"
    File /r ${CLIENT_PUBLISH_DIR}\*
SectionEnd


;;;;;;;;;;;;
;; Server ;;
;;;;;;;;;;;;

Section "Server" SecServer
    SetOutPath "$INSTDIR\Server"
    File /r ${SERVER_PUBLISH_DIR}\*

    Call ApplyServerOptions
SectionEnd


;;;;;;;;;;;;;
;; General ;;
;;;;;;;;;;;;;

Section "- General" SecGeneral
    SetOutPath "$INSTDIR"
    WriteUninstaller "$INSTDIR\Uninstall.exe"
SectionEnd


;;;;;;;;;;;;;;;
;; Uninstall ;;
;;;;;;;;;;;;;;;

Section "Uninstall"
    Call un.RemoveServerOptions
    RMDir /r /REBOOTOK $INSTDIR
SectionEnd


;;;;;;;;;;;;;;;;;;;;
;; Initialization ;;
;;;;;;;;;;;;;;;;;;;;

Function .onInit
    ; Verify system requirements.
    ${IfNot} ${RunningX64}
        MessageBox MB_OK "Sovereign Engine does not support 32 bit Windows."
        Abort
    ${EndIf}

    !insertmacro MULTIUSER_INIT

    ; Initialize state.
    StrCpy $NoGrantAdminRole 1
    StrCpy $InstallWindowsService 0
    StrCpy $RestrictPermissions 0
FunctionEnd


;;;;;;;;;;;;;;;;;;;;
;; Uninstall Init ;;
;;;;;;;;;;;;;;;;;;;;

Function un.onInit
    !insertmacro MULTIUSER_UNINIT
FunctionEnd


;;;;;;;;;;;;;;;;;;;;;;;;;
;; Server Options Page ;;
;;;;;;;;;;;;;;;;;;;;;;;;;

Function ServerOptions
    !insertmacro MUI_HEADER_TEXT "Server Options" "Select options for server installation."

    ; Skip this section if we're not installing the server.
    SectionGetFlags ${SecServer} $0
    IntOp $0 $0 & ${SF_SELECTED}
    IntCmp $0 0 Done

    ; Server options dialog.
    nsDialogs::Create 1018
    Pop $ServerOptionsDialog

    ${NSD_CreateLabel} 20u 0u 250u 24u "Select any options for the server installation. Note that some options are not available unless installing for all users. Production servers are recommended to select all options below."

    ; Create checkboxes
    ${NSD_CreateCheckBox} 20u 30u 200u 24u "Do not grant new players the Admin role by default"
    Pop $CheckboxNoGrantAdminRole
    ${NSD_SetState} $CheckboxNoGrantAdminRole ${BST_CHECKED}

    ${NSD_CreateCheckBox} 20u 50u 200u 24u "Install server as a Windows Service" 
    Pop $CheckboxInstallWindowsService
    ${NSD_OnClick} $CheckboxInstallWindowsService OnCheckboxInstallWindowsServiceChange
    ${NSD_SetState} $CheckboxInstallWindowsService ${BST_UNCHECKED}

    ${NSD_CreateCheckBox} 20u 70u 200u 24u "Restrict permissions to sensitive server folders" 
    Pop $CheckboxRestrictPermissions
    ${NSD_SetState} $CheckboxRestrictPermissions ${BST_UNCHECKED}
    EnableWindow $CheckboxRestrictPermissions 0  ; Disable by default

    ; If installing for the current user only, disable options that don't apply and choose good dev server defaults.
    ${If} $MultiUser.InstallMode == CurrentUser
        ${NSD_SetState} $CheckboxNoGrantAdminRole ${BST_UNCHECKED}
        EnableWindow $CheckboxInstallWindowsService 0
        EnableWindow $CheckboxRestrictPermissions 0
    ${EndIf}

    ; Show dialog
    nsDialogs::Show
Done:
FunctionEnd


;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Options Change Handler ;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;

Function OnCheckboxInstallWindowsServiceChange
    ${NSD_GetState} $CheckboxInstallWindowsService $0
    IntOp $0 $0 & ${BST_CHECKED}
    ${If} $0 == ${BST_CHECKED}
        EnableWindow $CheckboxRestrictPermissions 1
    ${Else}
        EnableWindow $CheckboxRestrictPermissions 0
        ${NSD_SetState} $CheckboxRestrictPermissions ${BST_UNCHECKED}
    ${EndIf}
FunctionEnd


;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Server Options Handler ;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;

Function OnServerOptionsNextPage
    ; Save state.
    ${NSD_GetState} $CheckboxNoGrantAdminRole $0
    ${NSD_GetState} $CheckboxInstallWindowsService $1
    ${NSD_GetState} $CheckboxRestrictPermissions $2
    IntOp $NoGrantAdminRole $0 & ${BST_CHECKED}
    IntOp $InstallWindowsService $1 & ${BST_CHECKED}
    IntOp $RestrictPermissions $2 & ${BST_CHECKED}
FunctionEnd


;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Apply Server Options ;;
;;;;;;;;;;;;;;;;;;;;;;;;;;

Function ApplyServerOptions
    SetOutPath "$INSTDIR\Server"

    ; Remove admin by default?
    ${If} NoGrantAdminRole != 0
        ; Update settings file.
        Push "appsettings.json" 
        Push '"AdminByDefault": true' 
        Push '"AdminByDefault": false'
        Call RIF
    ${EndIf}

    ${If} $InstallWindowsService != 0
        ; Build options string for the productionize script.
        StrCpy $0 ""
        ${If} $RestrictPermissions != 0
            StrCpy $1 "-RestrictPermissions"
        ${EndIf}

        ; Run productionize script.
        nsExec::ExecToLog "powershell -File Deploy\windows\InstallService.ps1 $1"
        Pop $0
        ${If} $0 != 0
            ; Script failed.
            MessageBox MB_OK|MB_ICONEXCLAMATION "Failed to apply server options.$\r$\nPlease use the InstallService.ps1 script to apply these options.$\r$\nRefer to the manual for more information."
        ${EndIf}
    ${EndIf}
FunctionEnd


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Uninstall Server Options ;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

Function un.RemoveServerOptions
    ; Try to remove the service. The script is smart and will only remove it if it exists and
    ; it is using the executable file that is about to be uninstalled. If this doesn't work, just
    ; silently fail because it probably means the Windows Service was never created.
    SetOutPath "$INSTDIR\Server"
    nsExec::ExecToLog "pwsh -File Deploy\windows\UninstallService.ps1"
    Pop $0
FunctionEnd


;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Component Descriptions ;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecClient} "Sovereign Engine client used to play the game."
    !insertmacro MUI_DESCRIPTION_TEXT ${SecServer} "Sovereign Engine server used to host the game."
!insertmacro MUI_FUNCTION_DESCRIPTION_END

