$nugetversion = $env:DOTNETR2RMLSTORE_NUGETVERSION

function Update-NuspecVersion
{
  Param ([string]$Version)
  
  foreach ($o in $input) 
  {
    Write-output ('Updating nuspec file ' + $o.FullName)
    $TmpFile = $o.FullName + ".tmp"

     get-content $o.FullName | 
        %{$_ -replace '\$version\$', $Version } > $TmpFile

     move-item $TmpFile $o.FullName -force
  }
}

function Pack-Nuget
{
  foreach ($o in $input) 
  {
    Write-output ('Packing ' + $o.FullName)
    
	nuget pack $o.FullName -Symbols
  }
}

if($nugetversion)
{
	Add-AppveyorMessage -Message ('Nuget version ' + $nugetversion)

	$env:APPVEYOR_BUILD_FOLDER | get-childitem -recurse |? { $_.Name -like '*.nuspec' } | Update-NuspecVersion $nugetversion;
	$env:APPVEYOR_BUILD_FOLDER | get-childitem -recurse |? { $_.Name -like '*.nuspec' } | Pack-Nuget
	
	Get-ChildItem .\*.nupkg | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
	
	Add-AppveyorMessage -Message 'Publish bin folder of the build where nuget was created'	
	Push-AppveyorArtifact 'src\Slp.r2rml4net.Storage\Slp.r2rml4net.Storage\bin' -FileName 'Slp.r2rml4net.Storage_bin'
}

