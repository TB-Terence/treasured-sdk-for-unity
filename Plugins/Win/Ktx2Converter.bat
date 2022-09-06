:: Loop through all the .png files from the given directory folder
:: Encode to KTX2 format and Delete png files
@ECHO OFF
TITLE Treasured - KTX2 Converter
ECHO --------------------------------------
ECHO Please wait... Encoding to KTX2 format.
ECHO --------------------------------------
set arg1=%1
set arg2=%2
setlocal enabledelayedexpansion
for /R "%arg2%" %%G in (*.png) do (
	"%arg1%" --bcmp "%%~dpnG" "%%G"
	del "%%G"
)
exit