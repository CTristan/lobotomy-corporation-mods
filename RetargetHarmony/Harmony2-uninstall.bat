@echo off
setlocal enabledelayedexpansion

:: RetargetHarmony / BepInEx Uninstaller for Lobotomy Corporation
:: This script removes BepInEx, RetargetHarmony, macOS resource fork files,
:: and any BepInEx mods found in BaseMods.

title RetargetHarmony Uninstaller

echo ============================================================
echo   RetargetHarmony / BepInEx Uninstaller
echo ============================================================
echo.

:: Verify we're in the game directory
if not exist "LobotomyCorp.exe" (
    echo WARNING: LobotomyCorp.exe was not found in this directory.
    echo This script should be run from the Lobotomy Corporation game
    echo directory ^(where LobotomyCorp.exe is located^).
    echo.
    set /p "continueAnyway=Continue anyway? (Y/N): "
    if /i not "!continueAnyway!"=="Y" (
        echo.
        echo Exiting. Please move this script to your game folder and try again.
        echo.
        pause
        exit /b 1
    )
    echo.
)

:: Initialize counters
set "totalFiles=0"
set "bepinexFiles="
set "baseModsFiles="
set "hasBepInExDir=0"

echo Scanning for files to remove...
echo.

:: ============================================================
:: Phase 1: Detect BepInEx core files
:: ============================================================

if exist "winhttp.dll" (
    set /a totalFiles+=1
    set "bepinexFiles=!bepinexFiles! winhttp.dll"
)

if exist "doorstop_config.ini" (
    set /a totalFiles+=1
    set "bepinexFiles=!bepinexFiles! doorstop_config.ini"
)

if exist ".doorstop_version" (
    set /a totalFiles+=1
    set "bepinexFiles=!bepinexFiles! .doorstop_version"
)

if exist "BepInEx" (
    set "hasBepInExDir=1"
    set /a totalFiles+=1
)

:: ============================================================
:: Phase 2: Detect macOS artifact files (._* and .DS_Store)
:: ============================================================

set "macosCount=0"
for /f "delims=" %%F in ('dir /b /s /a-d "._*" 2^>nul') do (
    set /a macosCount+=1
    set /a totalFiles+=1
)
for /f "delims=" %%F in ('dir /b /s /a-d ".DS_Store" 2^>nul') do (
    set /a macosCount+=1
    set /a totalFiles+=1
)

:: ============================================================
:: Phase 3: Detect BepInEx_Shim_Backup (original BaseMods DLLs)
:: ============================================================

set "hasShimBackup=0"
set "shimBackupCount=0"

if exist "BepInEx_Shim_Backup" (
    set "hasShimBackup=1"
    for /f "delims=" %%F in ('dir /b /s /a-d "BepInEx_Shim_Backup\*.dll" 2^>nul') do (
        set /a shimBackupCount+=1
    )
)

:: ============================================================
:: Phase 4: Detect BepInEx mods in BaseMods (from audit log)
:: ============================================================

set "auditLogPath=BepInEx\patchers\RetargetHarmony\logs\patched_mods.log"
set "baseModsCount=0"

if exist "!auditLogPath!" (
    for /f "usebackq tokens=* delims=" %%L in ("!auditLogPath!") do (
        set "modPath=%%L"
        if not "!modPath!"=="" (
            if exist "!modPath!" (
                set /a baseModsCount+=1
                set /a totalFiles+=1
                set "baseModsFiles=!baseModsFiles!    !modPath!!LF!"
            )
        )
    )
)

:: ============================================================
:: Phase 4: Display results
:: ============================================================

if !totalFiles! equ 0 (
    echo No BepInEx or RetargetHarmony files were found.
    echo The game directory appears to be clean.
    echo.
    pause
    exit /b 0
)

echo Found !totalFiles! item^(s^) to remove:
echo.

:: Display BepInEx core files
if not "!bepinexFiles!"=="" (
    echo --- BepInEx Core Files ---
    for %%F in (!bepinexFiles!) do (
        echo    %%F
    )
    echo.
)

if !hasBepInExDir! equ 1 (
    echo --- BepInEx Directory ---
    echo    BepInEx\ ^(entire directory^)
    echo.
)

:: Display shim backup files (will be restored, not deleted)
if !hasShimBackup! equ 1 (
    echo --- BepInEx Shim Backup ^(!shimBackupCount! DLL^(s^) to restore^) ---
    echo    Original BaseMods DLLs will be restored from BepInEx_Shim_Backup\
    for /f "delims=" %%F in ('dir /b "BepInEx_Shim_Backup\*.dll" 2^>nul') do (
        echo      %%F
    )
    echo.
)

:: Display macOS artifact files
if !macosCount! gtr 0 (
    echo --- macOS Artifact Files ^(!macosCount! found^) ---
    for /f "delims=" %%F in ('dir /b /s /a-d "._*" 2^>nul') do (
        echo    %%F
    )
    for /f "delims=" %%F in ('dir /b /s /a-d ".DS_Store" 2^>nul') do (
        echo    %%F
    )
    echo.
)

:: Display BepInEx mods in BaseMods
if !baseModsCount! gtr 0 (
    echo --- BepInEx Mods in BaseMods ^(!baseModsCount! found^) ---
    for /f "usebackq tokens=* delims=" %%L in ("!auditLogPath!") do (
        set "modPath=%%L"
        if not "!modPath!"=="" (
            if exist "!modPath!" (
                echo    !modPath!
            )
        )
    )
    echo.
)

:: ============================================================
:: Phase 5: Confirm and delete
:: ============================================================

echo ============================================================
echo WARNING: The above files will be permanently deleted.
echo.
set /p "confirm=Are you sure you want to remove all of these files? (Y/N): "

if /i "!confirm!"=="Y" goto :doDelete
if /i "!confirm!"=="yes" goto :doDelete

echo.
echo Uninstall cancelled. No files were removed.
echo.
pause
exit /b 0

:doDelete

echo.
echo Removing files...
echo.

:: Initialize action log
set "logFile=uninstall_log.txt"
echo RetargetHarmony Uninstall Log > "!logFile!"
echo Date: %date% %time% >> "!logFile!"
echo ============================================================ >> "!logFile!"
echo. >> "!logFile!"

set "errors=0"

:: Restore original BaseMods DLLs from BepInEx_Shim_Backup
if !hasShimBackup! equ 1 (
    for /f "delims=" %%F in ('dir /b "BepInEx_Shim_Backup\*.dll" 2^>nul') do (
        copy /y "BepInEx_Shim_Backup\%%F" "LobotomyCorp_Data\BaseMods\%%F" >nul 2>nul
        if errorlevel 1 (
            echo FAILED: Could not restore %%F
            echo FAILED: Restore %%F >> "!logFile!"
            set /a errors+=1
        ) else (
            echo Restored: %%F
            echo Restored: %%F >> "!logFile!"
        )
    )
    rmdir /s /q "BepInEx_Shim_Backup" 2>nul
    if not exist "BepInEx_Shim_Backup" (
        echo Deleted: BepInEx_Shim_Backup\ directory
        echo Deleted: BepInEx_Shim_Backup\ >> "!logFile!"
    )
)

:: Delete BepInEx mods in BaseMods first (before BepInEx directory is removed)
if !baseModsCount! gtr 0 (
    for /f "usebackq tokens=* delims=" %%L in ("!auditLogPath!") do (
        set "modPath=%%L"
        if not "!modPath!"=="" (
            if exist "!modPath!" (
                del /f /q "!modPath!" 2>nul
                if exist "!modPath!" (
                    echo FAILED: Could not delete !modPath!
                    echo FAILED: !modPath! >> "!logFile!"
                    set /a errors+=1
                ) else (
                    echo Deleted: !modPath!
                    echo Deleted: !modPath! >> "!logFile!"
                )
            )
        )
    )
)

:: Delete macOS artifact files (._*)
for /f "delims=" %%F in ('dir /b /s /a-d "._*" 2^>nul') do (
    del /f /q "%%F" 2>nul
    if exist "%%F" (
        echo FAILED: Could not delete %%F
        echo FAILED: %%F >> "!logFile!"
        set /a errors+=1
    ) else (
        echo Deleted: %%F
        echo Deleted: %%F >> "!logFile!"
    )
)
:: Delete macOS artifact files (.DS_Store)
for /f "delims=" %%F in ('dir /b /s /a-d ".DS_Store" 2^>nul') do (
    del /f /q "%%F" 2>nul
    if exist "%%F" (
        echo FAILED: Could not delete %%F
        echo FAILED: %%F >> "!logFile!"
        set /a errors+=1
    ) else (
        echo Deleted: %%F
        echo Deleted: %%F >> "!logFile!"
    )
)

:: Delete BepInEx directory
if !hasBepInExDir! equ 1 (
    rmdir /s /q "BepInEx" 2>nul
    if exist "BepInEx" (
        echo FAILED: Could not delete BepInEx directory
        echo FAILED: BepInEx\ >> "!logFile!"
        set /a errors+=1
    ) else (
        echo Deleted: BepInEx\ directory
        echo Deleted: BepInEx\ >> "!logFile!"
    )
)

:: Delete BepInEx core files
for %%F in (!bepinexFiles!) do (
    del /f /q "%%F" 2>nul
    if exist "%%F" (
        echo FAILED: Could not delete %%F
        echo FAILED: %%F >> "!logFile!"
        set /a errors+=1
    ) else (
        echo Deleted: %%F
        echo Deleted: %%F >> "!logFile!"
    )
)

echo. >> "!logFile!"

if !errors! gtr 0 (
    echo.
    echo WARNING: !errors! file^(s^) could not be deleted.
    echo You may need to run this script as administrator.
    echo !errors! file^(s^) could not be deleted. >> "!logFile!"
) else (
    echo.
    echo All files removed successfully.
    echo All files removed successfully. >> "!logFile!"
)

:: ============================================================
:: Phase 6: Ask about self-removal
:: ============================================================

echo.
set /p "removeSelf=Do you also want to remove this uninstall script? (Y/N): "

if /i "!removeSelf!"=="Y" goto :doSelfRemove
if /i "!removeSelf!"=="yes" goto :doSelfRemove
goto :keepScript

:doSelfRemove
echo Script will be removed. >> "!logFile!"
echo.
echo The uninstall log has been saved to: !logFile!
echo.
echo Press any key to finish and remove this script...
pause >nul
:: Self-delete using a temp batch file
(
    echo @echo off
    echo del /f /q "%~f0"
    echo del /f /q "%%~f0"
) > "%temp%\retarget_cleanup.bat"
start /b "" cmd /c "%temp%\retarget_cleanup.bat"
exit /b 0

:keepScript
echo Script kept. >> "!logFile!"
echo.
echo The uninstall log has been saved to: !logFile!
echo.
pause
