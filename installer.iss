#ifndef MyAppVersion
#define MyAppVersion "1.0.0"
#endif

[Setup]
AppName=AI Study Hub
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\AI Study Hub
DefaultGroupName=AI Study Hub
OutputDir=.\InstallerOutput
OutputBaseFilename=AIStudyHub-Installer-win-x64
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\AIStudyHub.exe

[Files]
Source: ".\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\AI Study Hub"; Filename: "{app}\AIStudyHub.exe"
Name: "{autodesktop}\AI Study Hub"; Filename: "{app}\AIStudyHub.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"
