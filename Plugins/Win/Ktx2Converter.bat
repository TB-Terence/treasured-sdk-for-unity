:: Loop through all the .png files from the given directory folder
:: Encode to KTX2 format and Delete png files
@ECHO OFF
TITLE Treasured - KTX2 Converter
set arg1=%1
set arg2=%2
set progress=0
set count=0
setlocal enabledelayedexpansion
for /R "%arg2%" %%G in (*.png) do (set /a count+=1)
for /R "%arg2%" %%G in (*.png) do (
	set /a progress+=1
	set /a percentage=!progress! * 100 / %count%
	echo !percentage!
	%arg1% --bcmp "%%~dpnG" "%%G"
	del "%%G"
	cls
)
exit