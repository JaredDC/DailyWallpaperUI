REM cd to project dir.
REM FUCK
echo "bin\Release\"
RM /Q /S "..\Output"
md "..\Output"
xcopy "bin\Release\"  "..\Output" /q /e /y /i
del "..\Output\config*.ini" 
del "..\Output\*.log.txt"
echo fuck appveyor.
set errorlevel=0