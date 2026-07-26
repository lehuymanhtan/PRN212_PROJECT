#ifndef MyAppVersion
#define MyAppVersion "1.0.0"
#endif

#ifndef AppArch
#define AppArch "x64"
#endif

#ifndef BuildRid
#define BuildRid "win-x64"
#endif

[Setup]
AppName=AI Study Hub
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\AI Study Hub
DefaultGroupName=AI Study Hub
OutputDir=.\InstallerOutput
OutputBaseFilename=AIStudyHub-Installer-{#BuildRid}
Compression=lzma2
SolidCompression=yes

#if AppArch == "x64"
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
#elif AppArch == "arm64"
ArchitecturesAllowed=arm64
ArchitecturesInstallIn64BitMode=arm64
#else
; For x86 (win32), no specific architecture limits
#endif

UninstallDisplayIcon={app}\AIStudyHub.exe

[Files]
Source: ".\publish\{#BuildRid}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\AI Study Hub"; Filename: "{app}\AIStudyHub.exe"
Name: "{autodesktop}\AI Study Hub"; Filename: "{app}\AIStudyHub.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\AIStudyHub.exe"; Description: "{cm:LaunchProgram,AI Study Hub}"; Flags: nowait postinstall skipifsilent
