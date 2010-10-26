@ECHO OFF
ECHO Installing Web Part...

IF EXIST "C:\Program Files\Common Files\Microsoft Shared\web server extensions\60\BIN\stsadm.exe" "C:\Program Files\Common Files\Microsoft Shared\web server extensions\60\BIN\stsadm" -o deletewppack -name "SiteNavigationTree_Deploy.CAB"

IF EXIST "C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\BIN\stsadm.exe" "C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\BIN\stsadm" -o deletewppack -name "SiteNavigationTree_Deploy.CAB"

IF EXIST "C:\Program Files\Common Files\Microsoft Shared\web server extensions\14\BIN\stsadm.exe" "C:\Program Files\Common Files\Microsoft Shared\web server extensions\60\BIN\stsadm" -o deletewppack -name "SiteNavigationTree_Deploy.CAB"

recycle.js

pause