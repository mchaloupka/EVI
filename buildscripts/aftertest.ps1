if($env:APPVEYOR_REPO_BRANCH -eq "master")
{
    MSBuild.SonarQube.Runner.exe end /d:"sonar.login=$env:SONARQUBE_TOKEN"
}

$env:PATH = "C:\msys64\usr\bin;" + $env:PATH
Invoke-WebRequest -Uri "https://codecov.io/bash" -OutFile codecov.sh
bash codecov.sh -f "coverage.xml" -t $env:CODECOV_TOKEN