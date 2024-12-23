;--------------------------------
; Includes

!include "MUI2.nsh"
!include "logiclib.nsh"

;--------------------------------
; Custom defines
!define NAME "Babble App"
!define APPFILE "Babble.Avalonia.Desktop.exe"
!define VERSION "1.0.0.0"
!define SLUG "${NAME} v${VERSION}"

;--------------------------------
; General

Name "${NAME}"
OutFile "${NAME} Setup.exe"
InstallDir "$PROGRAMFILES\\${NAME}"
InstallDirRegKey HKCU "Software\\${NAME}" ""
RequestExecutionLevel admin

;--------------------------------
; UI

!define MUI_ICON "assets\\IconOpaque_32x32.ico"
!define MUI_HEADERIMAGE
!define MUI_WELCOMEFINISHPAGE_BITMAP "assets\\MUI_WELCOMEFINISHPAGE_BITMAP.bmp"
!define MUI_HEADERIMAGE_BITMAP "assets\\MUI_HEADERIMAGE_BITMAP.bmp"
!define MUI_ABORTWARNING
!define MUI_WELCOMEPAGE_TITLE "${SLUG} Setup"

;--------------------------------
; Pages

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "license.txt"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

;--------------------------------
; Section - Install App

Section "-hidden app"
  SectionIn RO
  SetOutPath "$INSTDIR"
  File /r /x ".gitignore" "app\\*.*"
  WriteRegStr HKCU "Software\\${NAME}" "" $INSTDIR
  WriteUninstaller "$INSTDIR\\Uninstall.exe"
SectionEnd

;--------------------------------
; Section - Install CUDA and cuDNN

Section "Install CUDA and cuDNN"
  SetOutPath "$TEMP"
  
  ; Include CUDA and cuDNN installers
  File "cudnn_9.5.0_windows.exe"
  File "cuda_12.6.3_windows_network.exe"

  ; Install cuDNN
  ExecWait "$TEMP\\cudnn_9.5.0_windows.exe"
  IfErrors +2 0
  MessageBox MB_OK "Error installing cuDNN. Installation will abort."
  Quit

  ; Install CUDA
  ExecWait "$TEMP\\cuda_12.6.3_windows_network.exe"
  IfErrors +2 0
  MessageBox MB_OK "Error installing CUDA. Installation will abort."
  Quit
SectionEnd

;--------------------------------
; Section - Shortcut

Section "Desktop Shortcut" DeskShort
  CreateShortCut "$DESKTOP\\${NAME}.lnk" "$INSTDIR\\${APPFILE}"
SectionEnd

;--------------------------------
; Descriptions

LangString DESC_DeskShort ${LANG_ENGLISH} "Create Shortcut on Desktop."

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${DeskShort} $(DESC_DeskShort)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
; Section - Uninstaller

Section "Uninstall"

  Delete "$DESKTOP\\${NAME}.lnk"
  Delete "$INSTDIR\\Uninstall.exe"
  RMDir /r "$INSTDIR"
  DeleteRegKey /ifempty HKCU "Software\\${NAME}"

SectionEnd