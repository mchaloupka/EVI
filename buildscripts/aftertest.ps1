if($env:APPVEYOR_REPO_BRANCH -eq "master")
{
    MSBuild.SonarQube.Runner.exe end /d:"sonar.login=$env:SONARQUBE_TOKEN"
}

pip install codecov
codecov -f "coverage.xml"