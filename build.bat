@echo off

rmdir /s /q "Machine.UI\bin"
rmdir /s /q "Machine.UI\obj"

taskkill /F /IM Machine.UI.exe 2>nul

MSBuild "Machine.Solution.sln" /t:Rebuild /p:Configuration=Release /p:Platform=x64

del "%USERPROFILE%\Desktop\Machine.UI.lnk" 2>nul

powershell "$target='%~dp0Machine.UI\bin\x64\Release\Machine.UI';$s=(New-Object -COM WScript.Shell).CreateShortcut('%USERPROFILE%\Desktop\Machine.UI.lnk');$s.TargetPath=$target;$s.WorkingDirectory='%~dp0Machine.UI\bin\x64\Release';$s.Save()"

pause