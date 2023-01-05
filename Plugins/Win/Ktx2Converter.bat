:: Loop through all the .png files from the given directory folder
:: Encode to KTX2 format and Delete png files
@ECHO OFF
TITLE Treasured - KTX2 Converter
ECHO ------------------------------------------------------------------
ECHO Please wait... Encoding to KTX2 format. DO NOT close this window.
ECHO ------------------------------------------------------------------
set arg1=%1
set arg2=%2
set progress=1
setlocal enabledelayedexpansion
for /R "%arg2%" %%G in (*.png) do (
	for %%i in ("%%~dpG\.") do (
		for %%q in ("%%~dpG\..") do (
			set /a a = !progress! %% 7
			echo Encoding[!a! / 6]... %%~nxi[%%~nxq]
			if !a! geq 6 (
				set /a progress = 1
			) else (
				set /a progress = progress + 1
			)
		)
	)
	"%arg1%" --bcmp "%%~dpnG" "%%G"
	del "%%G"
)
exit