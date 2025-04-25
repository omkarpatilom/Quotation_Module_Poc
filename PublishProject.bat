@echo off
setlocal enabledelayedexpansion

rem Set the base output folder path
set BaseOutputFolder=D:\STESPL_WorkPlace\PUC\Quotation_Module_puc

rem Set the log directory and log file name
set LogDirectory=D:\Skillmatrix-Workplace\Published\log
set LogFile=!LogDirectory!\publish_log.txt

rem Create the log directory if it doesn't exist
if not exist "!LogDirectory!" (
    mkdir "!LogDirectory!"
    echo Created log directory at: !LogDirectory! >> "!LogFile!"
)

rem Log the start of the publishing process
echo Starting publishing process... >> "!LogFile!"
echo Base Output Folder: !BaseOutputFolder! >> "!LogFile!"

rem Create base output folder if it doesn't exist
if not exist "!BaseOutputFolder!" (
    mkdir "!BaseOutputFolder!"
    echo Created base output folder at: !BaseOutputFolder! >> "!LogFile!"
) else (
    echo Using existing base output folder at: !BaseOutputFolder! >> "!LogFile!"
)

rem Loop through each project in the solution
for /f "delims=" %%i in ('dotnet sln list ^| findstr ".csproj"') do (
    rem Extract the project name from the path
    for %%j in ("%%i") do set ProjectName=%%~nj

    rem Create a project-specific output folder
    set ProjectOutputFolder=!BaseOutputFolder!\!ProjectName!
    mkdir "!ProjectOutputFolder!"

    rem Log publishing of the project
    echo Publishing project: %%i >> "!LogFile!"
    echo Project Output Folder: !ProjectOutputFolder! >> "!LogFile!"

    echo Publishing %%i to !ProjectOutputFolder!...
    dotnet publish "%%i" -c Release -o "!ProjectOutputFolder!" >> "!LogFile!" 2>&1

    rem Log completion of project publishing
    echo Finished publishing project: %%i >> "!LogFile!"
)

rem Log the end of the publishing process
echo All projects have been published. >> "!LogFile!"
echo Publishing process completed at: %date% %time% >> "!LogFile!"

endlocal
