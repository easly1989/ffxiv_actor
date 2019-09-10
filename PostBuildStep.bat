@echo off
set from=%1
set libs=%2libs
set pdb=%2pdb
echo %from%
echo %libs%
echo %pdb%
echo "Creating folders if necessary..."
if not exist %libs% mkdir %libs%
if not exist %pdb% mkdir %pdb%
echo "Moving all dll files..."
move /Y %from%*.dll %libs%
move /Y %from%*.dll.config %libs%
echo "Moving all xml files..."
move /Y %from%*.xml %libs%
echo "Moving pdb files..."
move /Y %from%*.pdb %pdb%
echo "Moving 7z folders..."
if exist %libs%\x64 rmdir /S /Q %libs%\x64
if exist %from%x64 move /Y %from%x64 %libs%
if exist %libs%\x86 rmdir /S /Q %libs%\x86
if exist %from%x86 move /Y %from%x86 %libs%