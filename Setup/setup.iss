; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "SignaMath"
#define MyAppVersion "1.0.1"
#define MyAppPublisher "zonetecde"
#define MyAppURL "github.com/zonetecde/SignaMath"
#define MyAppExeName "SignaMath.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{5A9CF922-3F87-4C7F-99D3-1EB5A4F82D37}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
OutputDir=E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\Setup
OutputBaseFilename=SignaMath
SetupIconFile=E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\assets\icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; 

[Files]
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\AngouriMath.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\Antlr4.Runtime.Standard.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\GenericTensor.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\HonkSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\MathNet.Numerics.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\Microsoft.WindowsAPICodePack.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\Microsoft.WindowsAPICodePack.Shell.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\Numbers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\SignaMath.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\SignaMath.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\SignaMath.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\SignaMath.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\SignaMath.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\WpfMath.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\XamlMath.Shared.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\ClassLibrary.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\SignaMath\SignaMath\bin\Debug\net6.0-windows\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

