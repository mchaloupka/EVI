Set-AppveyorBuildVariable -Name 'DOTNETR2RMLSTORE_NUGETVERSION' -Value ''

$version = $env:APPVEYOR_BUILD_VERSION
$iversion = $version

if($env:APPVEYOR_REPO_TAG -eq 'True')
{
	$branch = $env:APPVEYOR_REPO_BRANCH
	$tag = $env:APPVEYOR_REPO_TAG_NAME
	$build = $env:APPVEYOR_BUILD_NUMBER
	
	Add-AppveyorMessage -Message "This is a tag build (tag: $tag, branch: $branch, build: $build)"
	
	if($tag -match 'v([0-9]*)\.([0-9]*)\.([0-9]*)(-[a-z]+)?')
	{
		$version = $matches[1] + "." + $matches[2] + "." + $matches[3];
		$nversion = $version;
		
		$version = $version + "." + $build;
		$iversion = $version;
		
		if($matches.count -eq 5)
		{
			$nversion = $nversion + $matches[4]
			$iversion = $iversion + $matches[4]
		}
		
		Set-AppveyorBuildVariable -Name 'DOTNETR2RMLSTORE_NUGETVERSION' -Value $nversion
		Add-AppveyorMessage -Message "Stored nuget version"
	}
}

Add-AppveyorMessage -Message "Informational version $iversion"

function Update-SourceVersion
{
  Param ([string]$Version, [string]$InformationalVersion)
  
  $NewVersion = 'AssemblyVersion("' + $Version + '")';
  $NewFileVersion = 'AssemblyFileVersion("' + $Version + '")';
  $NewInformationalVersion = 'AssemblyInformationalVersion("' + $InformationalVersion + '")';

  foreach ($o in $input) 
  {
	Write-output ('Updating ' + $o.FullName)
    $TmpFile = $o.FullName + ".tmp"

     get-content $o.FullName | 
        %{$_ -replace 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewVersion } |
        %{$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewFileVersion } | 
		%{$_ -replace 'AssemblyInformationalVersion\("[0-9]+(\.([0-9]+|\*)){1,3}(-[a-z]+)?"\)', $NewInformationalVersion } > $TmpFile

     move-item $TmpFile $o.FullName -force
  }
}

foreach ($file in "AssemblyInfo.cs", "AssemblyInfo.vb" )
{
	$env:APPVEYOR_BUILD_FOLDER | get-childitem -recurse |? {$_.Name -eq $file} | Update-SourceVersion $version $iversion;
}
$projectName = "EVI"
nuget restore src\Slp.Evi.Storage\Slp.Evi.Storage.sln
choco install ""msbuild-sonarqube-runner"" -y
MSBuild.SonarQube.Runner.exe begin /k:""$projectName"" /d:""sonar.host.url=https://sonarqube.com"" /d:""sonar.login=$env:SONARQUBE_TOKEN""