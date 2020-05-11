set SOURCE_PATH=
set DESTINATION_PATH=

if not exist "%DESTINATION_PATH%\AC_Extension\AC_Extension.dll" goto end
if not exist "%SOURCE_PATH%\AC_Extension\AC_Extension\bin\Debug\AC_Extension.dll" goto end

copy "%SOURCE_PATH%\AC_Extension\AC_Extension\bin\Debug\AC_Extension.dll" "%DESTINATION_PATH%\AC_Extension\AC_Extension.dll"
copy "%SOURCE_PATH%\AC_Extension\AC_Extension\bin\Debug\AC_Extension.pdb" "%DESTINATION_PATH%\AC_Extension\AC_Extension.pdb"

:end
