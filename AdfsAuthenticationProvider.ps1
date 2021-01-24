[CmdletBinding()]
param (
    [parameter(,Mandatory=$true)][ValidateNotNullOrEmpty()][ValidateSet('Register','Unregister','Check')]
    [string]$Action,
    [parameter(Position=1,Mandatory=$true)][ValidateNotNullOrEmpty()][ValidateSet('Debug','Release')]
    [string]$Environment
)

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    throw "User is not an Admin."
}

$location = "C:\ADFSAdapters"
$ProjectFolderName = "SMSAuthADFS"
$AdfsAuthenticationProviderName = "SMSAuthADFS"
$DllFileName = "SMSAuthADFS.dll"
$ConfigurationFileName = "SmsAdapter.json"

$DllFilePath = Join-Path -ChildPath $DllFileName -Path $location

New-Item -Path $location -ItemType "directory" -Force | Out-Null
Copy-Item -Path (".\" + $ProjectFolderName + "\bin\" + $Environment + "\Microsoft.IdentityServer.Web.dll") -Destination $location
Copy-Item -Path (".\" + $ProjectFolderName + "\bin\" + $Environment + "\" + $DllFileName) -Destination $location
Copy-Item -Path (".\" + $ProjectFolderName + "\bin\" + $Environment + "\" + $ConfigurationFileName) -Destination $location

$assembly_display_name = [System.Reflection.AssemblyName]::GetAssemblyName($DllFilePath).FullName
$typeName = "SMSAuthADFS.SmsAdapter, " + $assembly_display_name
$ConfigurationFilePath = Join-Path -ChildPath $ConfigurationFileName -Path $location

switch ( $Action )
{
    Register
    { 
        .\gacutil\gacutil.exe /if $DllFilePath
        Register-AdfsAuthenticationProvider -Name $AdfsAuthenticationProviderName -TypeName $typeName -ConfigurationFilePath $ConfigurationFilePath 
        Get-Service -Name adfssrv | Restart-Service
    }
    Unregister
    {
        Unregister-AdfsAuthenticationProvider -Name $AdfsAuthenticationProviderName -Confirm:$false
        Get-Service -Name adfssrv | Restart-Service
        .\gacutil\gacutil.exe /u $assembly_display_name
    }
    Check
    {
        .\gacutil\gacutil.exe /l $assembly_display_name
    }
}