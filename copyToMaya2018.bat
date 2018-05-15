@echo off
::echo d | xcopy "D:\jianguoyun\mayaTools\InazumaTool\InazumaTool" "C:\Program Files\Autodesk\Maya2018\bin\plug-ins\InazumaTool" /Y /E
echo f | xcopy "%~dp0\InazumaTool\bin\x64\Release\InazumaTool.dll" "C:\Program Files\Autodesk\Maya2018\bin\plug-ins\InazumaTool\InazumaTool.nll.dll" /Y
pause