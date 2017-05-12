MSBuild.SonarQube.Runner.exe end /d:"sonar.login=$env:SONARQUBE_TOKEN"

pip install codecov
codecov -f "coverage.xml" -t $env:CODECOV_TOKEN