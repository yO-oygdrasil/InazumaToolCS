@echo off
::echo d | xcopy "D:\jianguoyun\mayaTools\InazumaTool\InazumaTool" "C:\Program Files\Autodesk\Maya2018\bin\plug-ins\InazumaTool" /Y /E
::echo f | xcopy "%~dp0\InazumaTool\bin\x64\Release\InazumaTool.dll" "C:\Program Files\Autodesk\Maya2018\bin\plug-ins\InazumaTool\InazumaTool.nll.dll" /Y
::pause
if exist "C:\Program Files\Autodesk\Maya2018\bin\plug-ins\InazumaTool\InazumaTool.nll.dll" (goto copyDllWithDate2018) else goto copyDll2018

:copyDll2018
xcopy "%~dp0\InazumaTool\bin\x64\Release\InazumaTool.dll*" "C:\Program Files\Autodesk\Maya2018\bin\plug-ins\InazumaTool\InazumaTool.nll.dll*" /Y
goto copy2018end

:copyDllWithDate2018
xcopy "%~dp0\InazumaTool\bin\x64\Release\InazumaTool.dll*" "C:\Program Files\Autodesk\Maya2018\bin\plug-ins\InazumaTool\InazumaTool_%date:~,4%%date:~5,2%%date:~8,2%_%time:~,2%_%time:~3,2%_%time:~6,2%.nll.dll*" /Y
goto copy2018end

:copy2018end


if exist "C:\Program Files\Autodesk\Maya2020\bin\plug-ins\InazumaTool\InazumaTool.nll.dll" (goto copyDllWithDate2020) else goto copyDll2020

:copyDll2020
xcopy "%~dp0\InazumaTool\bin\x64\Release\InazumaTool.dll*" "C:\Program Files\Autodesk\Maya2020\bin\plug-ins\InazumaTool\InazumaTool.nll.dll*" /Y
goto copy2020end

:copyDllWithDate2020
xcopy "%~dp0\InazumaTool\bin\x64\Release\InazumaTool.dll*" "C:\Program Files\Autodesk\Maya2020\bin\plug-ins\InazumaTool\InazumaTool_%date:~,4%%date:~5,2%%date:~8,2%_%time:~,2%_%time:~3,2%_%time:~6,2%.nll.dll*" /Y
goto copy2020end





:copy2020end

