<#  Install-Module -Name Carbon -RequiredVersion 2.13.0 #>

Import-Module -Name 'Carbon'
[string]$localIp = Get-CIPAddress -V4 | Select -ExpandProperty "IPAddressToString" | Where-Object { $_.StartsWith("192") }

[string] $name = "vocabulary-techno"

[string]$rootFolder = Join-Path $PSScriptRoot  -ChildPath "\..\.."

cd $rootFolder

<# https://www.tutorialspoint.com/how-to-set-environment-variables-using-powershell #>
$env:LOCAL_IP = $localIp
docker-compose -p $name -f docker-compose.yml up --force-recreate --build -d
docker image prune -f
PAUSE
 
cd $PSScriptRoot
