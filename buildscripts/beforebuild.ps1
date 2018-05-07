nuget restore src\Slp.Evi.Storage\Slp.Evi.Storage.sln

if($env:APPVEYOR_REPO_BRANCH -eq "master")
{
    $projectName = "EVI"    
    choco install ""msbuild-sonarqube-runner"" -y
    MSBuild.SonarQube.Runner.exe begin /k:"$projectName" /d:""sonar.host.url=https://sonarcloud.io"" /d:"sonar.login=$env:SONARQUBE_TOKEN" /d:"sonar.organization=mchaloupka-github" /d:"sonar.cs.opencover.reportsPaths=coverage.xml"
}

.\fake.cmd build -t BeforeBuild
