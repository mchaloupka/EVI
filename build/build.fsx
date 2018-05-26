#r "paket:
    nuget Fake.Core.Target
    nuget Fake.Core.Environment
    nuget Fake.Core.Xml
    nuget Fake.Core.Process
    nuget Fake.Core.ReleaseNotes
    nuget Fake.IO.FileSystem
    nuget Fake.IO.Zip
    nuget Fake.Net.Http
    nuget Fake.DotNet.Cli
    nuget Fake.DotNet.AssemblyInfoFile
    nuget Fake.BuildServer.AppVeyor
    nuget Fake.DotNet.Nuget
    nuget Fake.DotNet.MSBuild
    nuget Fake.Windows.Chocolatey
    nuget Fake.Testing.SonarQube
    nuget System.Reactive 3.1.1
    nuget Newtonsoft.Json //"

#if !FAKE
#load ".fake/build.fsx/intellisense.fsx"
#endif

open System
open System.IO
open System.Xml
open System.Text.RegularExpressions
open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.Net
open Fake.BuildServer
open Fake.DotNet.NuGet.Restore
open Fake.DotNet.NuGet.NuGet
open Fake.Windows
open Fake.Testing

BuildServer.install [
  AppVeyor.Installer
]

// *** Common stuff for the build ***
module Common =
  let private scriptDirectory = __SOURCE_DIRECTORY__

  let baseDirectory = (Directory.GetParent scriptDirectory).FullName

  let branch =
    let b = AppVeyor.Environment.RepoBranch
    if String.IsNullOrEmpty b then "local"
    else b

  let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
    else None

// *** Logic related to build version retrieval ***
module VersionLogic =
  type VersionInformation = { Version: string; InformationalVersion: string; NugetVersion: string option }

  let private buildNumber =
    let b = AppVeyor.Environment.BuildNumber
    if String.IsNullOrEmpty b then "1"
    else b

  let private tagVersion =
    if AppVeyor.Environment.RepoTag then
      let tag = AppVeyor.Environment.RepoTagName

      Trace.log (sprintf "This is a tag build (tag: %s, branch: %s, build: %s)" tag Common.branch buildNumber)

      match tag with
      | Common.Regex @"v([0-9]*)\.([0-9]*)\.([0-9]*)" [ major; minor; patch ] -> Some (major, minor, patch, None)
      | Common.Regex @"v([0-9]*)\.([0-9]*)\.([0-9]*)(-[a-z]+)" [ major; minor; patch; suffix ] -> Some (major, minor, patch, Some suffix)
      | _ -> None
    else None

  let private releaseNotesVersion =
    ReleaseNotes.load (Common.baseDirectory + "/RELEASE_NOTES.md")
    |> (fun x ->
      match x.AssemblyVersion with
      | Common.Regex @"([0-9]*)\.([0-9]*)\.([0-9]*)" [ major; minor; patch ] -> (major, minor, patch)
      | _ -> failwith "Failed to read release notes"
    )

  let version =
    let (rnMajor, rnMinor, rnPatch) = releaseNotesVersion

    match tagVersion with
    | Some (major, minor, patch, _) ->
      if (major <> rnMajor || minor <> rnMinor || patch <> rnPatch) then
        failwith "The tag version differs from the version in release notes"
    | None -> ()

    match tagVersion with
    | Some (major, minor, patch, Some suffix) ->
      let version = sprintf "%s.%s.%s.%s" major minor patch buildNumber
      let informational = sprintf "%s-%s" version suffix
      let nuget = sprintf "%s.%s.%s-%s" major minor patch suffix
      { Version = version; InformationalVersion = informational; NugetVersion = Some nuget }
    | Some (major, minor, patch, None) ->
      let version = sprintf "%s.%s.%s.%s" major minor patch buildNumber
      let nuget = sprintf "%s.%s.%s" major minor patch
      { Version = version; InformationalVersion = version; NugetVersion = Some nuget }
    | None ->
      let suffix =
        match Common.branch with
        | "develop" -> "alpha"
        | "master" -> "beta"
        | "local" -> "local"
        | _ -> 
          let date = DateTime.Now
          date.ToString "yyyyMMdd" |> sprintf "ci-%s"
      let (version, informational, nuget) =
        let rnVersion = sprintf "%s.%s.%s" rnMajor rnMinor rnPatch
        ((sprintf "%s.%s" rnVersion buildNumber), (sprintf "%s.%s-%s" rnVersion buildNumber suffix), Some (sprintf "%s-%s%s" rnVersion suffix buildNumber))
      { Version = version; InformationalVersion = informational; NugetVersion = nuget}

// *** Define Targets ***
Target.create "UpdateAssemblyInfo" (fun _ ->
  match Common.branch with
  | "local" -> Trace.log "Skipping assembly info update"
  | _ ->
    Trace.log " --- Updating assembly info --- "
    Trace.log (sprintf " Version: %s" VersionLogic.version.InformationalVersion)
    let date = DateTime.Now
    let copyrightYear = date.Year

    !! (Common.baseDirectory + "/src/**/AssemblyInfo.cs")
    |> Seq.iter(fun asmInfo ->
      let version = VersionLogic.version
      [
        AssemblyInfo.Version version.Version
        AssemblyInfo.FileVersion version.Version
        AssemblyInfo.InformationalVersion version.InformationalVersion
        AssemblyInfo.Copyright (sprintf "Copyright (c) %d" copyrightYear)
      ] |> AssemblyInfoFile.updateAttributes asmInfo
    )
)

Target.create "RestorePackages" (fun _ ->
  Trace.log "--- Restore packages starting ---"

  (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Storage.sln")
  |> DotNet.restore id
)

Target.create "Build" (fun _ ->
  Trace.log " --- Building the app --- "

  let project = (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Storage.sln")
  let build conf =
    project
    |> MSBuild.build (fun p ->
      { p with
          Verbosity = Some MSBuildVerbosity.Quiet
          Targets = ["Build"]
          Properties =
            [
              "Configuration", conf
            ]
          MaxCpuCount = Some None
      }
    )

  build "Debug"
  build "Release"
)

Target.create "InstallDependencies" (fun _ ->
  Trace.log " --- Installing dependencies --- "
  match Common.branch with
  | "local" -> ()
  | _ ->
    let zipLocation = Common.baseDirectory + "/build/opencover.zip"
    Http.downloadFile zipLocation "https://github.com/OpenCover/opencover/releases/download/4.6.519/opencover.4.6.519.zip" |> ignore
    Zip.unzip (Common.baseDirectory + "/build/opencover") zipLocation

  let toInstall =
    match Common.branch with
    | "develop" ->
      [
        "msbuild-sonarqube-runner"
      ]
    | _ -> []

  toInstall
  |> Seq.iter (Choco.install id)
)

Target.create "BeginSonarQube" (fun _ ->
  if Common.branch = "develop" then
    Trace.log " --- Starting SonarQube analyzer --- "
    SonarQube.start (fun p ->
      { p with
          Key = "EVI"
          Settings =
            [
              "sonar.host.url=https://sonarcloud.io"
              ("sonar.login=" + Environment.GetEnvironmentVariable("SONARQUBE_TOKEN"))
              "sonar.organization=mchaloupka-github"
              "sonar.cs.opencover.reportsPaths=..\\coverage.xml"
            ]
          ToolsPath = "C:\\ProgramData\\chocolatey\\lib\\msbuild-sonarqube-runner\\tools\\SonarScanner.MSBuild.exe"
      }
    )
  else Trace.log "SonarQube start skipped (not develop branch)"
)

Target.create "EndSonarQube" (fun _ ->
  if Common.branch = "develop" then
    Trace.log " --- Exiting SonarQube analyzer --- "
    SonarQube.finish (Some (fun p ->
      { p with
          Settings = [ ("sonar.login=" + Environment.GetEnvironmentVariable("SONARQUBE_TOKEN")) ]
          ToolsPath = "C:\\ProgramData\\chocolatey\\lib\\msbuild-sonarqube-runner\\tools\\SonarScanner.MSBuild.exe"
      }
    ))
  else Trace.log "SonarQube end skipped (not develop branch)"
)

Target.create "Package" (fun _ ->
  match VersionLogic.version.NugetVersion with
  | Some version ->
    Trace.log " --- Packaging app --- "

    DirectoryInfo.ensure (DirectoryInfo.ofPath (Common.baseDirectory + "/nuget"))

    (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Storage.sln")
    |> DotNet.pack (fun p ->
      { p with
          NoBuild = true
          OutputPath = Some (Common.baseDirectory + "/nuget")
          Configuration = DotNet.BuildConfiguration.Release
          Common =
            { p.Common with 
                CustomParams = Some ("--no-restore --include-source --include-symbols /p:PackageVersion=" + version)
            }
      }
    )

  | None -> Trace.log "Skipping nuget packaging"
)

Target.create "PrepareDatabase" (fun _ ->
  match Common.branch with
  | "local" -> Trace.log "Skipping database preparation "
  | _ ->
    Trace.log " --- Preparing database --- "
    let sqlInstance = "(local)\\SQL2014";
    let dbName = "R2RMLTestStore";
    let connectionString = sprintf "Server=%s;Database=%s;User ID=sa;Password=Password12!" sqlInstance dbName

    let updateConfig file =
      Trace.log (sprintf "Updating connection string in: %s" file)
      let content = File.ReadAllText file
      let contentObj = Newtonsoft.Json.JsonConvert.DeserializeObject content :?> Newtonsoft.Json.Linq.JToken
      contentObj.["ConnectionStrings"].["mssql"] <- Newtonsoft.Json.Linq.JValue connectionString
      let newContent = Newtonsoft.Json.JsonConvert.SerializeObject contentObj
      File.WriteAllText(file, newContent)

    !! (Common.baseDirectory + "/src/**/database.json")
    |> Seq.iter updateConfig

    Trace.log (sprintf "Creating database in %s" sqlInstance)
    let result = Shell.Exec("sqlcmd", sprintf "-S \"%s\" -Q \"USE [master]; CREATE DATABASE [%s]\"" sqlInstance dbName)
    if result <> 0 then failwith "Database creation failed"
)

Target.create "RunTests" (fun _ ->
  Trace.log " --- Running tests --- "
  let exec proj =
    match Common.branch with
    | "local" ->
      proj 
      |> DotNet.test (fun p ->
        { p with
            NoRestore = true
            NoBuild = true
        }
      )
    | _ ->  
      let result = Shell.Exec(Common.baseDirectory + "/build/opencover/OpenCover.Console.exe", sprintf "-register:user -returntargetcode -target:\"dotnet.exe\" -targetargs:\"test %s --configuration Debug --no-build\" -filter:\"+[Slp.Evi.Storage*]*\" -mergeoutput -output:\"%s/build/coverage.xml\" -oldstyle" proj Common.baseDirectory)
      if result <> 0 then failwithf "Tests failed (exit code %d, project: %s)" result proj

  !! (Common.baseDirectory + "/src/**/*.Test.*.csproj")
  |> Seq.iter exec
)

Target.create "UploadCodeCov" (fun _ ->
  match Common.branch with
  | "local" -> Trace.log "Skipping uploading coverage results"
  | _ ->
    Trace.log " --- Uploading CodeCov --- "
    Http.downloadFile ".\\codecov.sh" "https://codecov.io/bash" |> ignore
    let result = Shell.Exec("bash", sprintf "codecov.sh -f \"%s/build/coverage.xml\" -t %s" Common.baseDirectory (Environment.GetEnvironmentVariable("CODECOV_TOKEN")))
    if result <> 0 then failwithf "Uploading coverage results failed (exit code %d)" result
)

Target.create "PublishArtifacts" (fun _ ->
  match VersionLogic.version.NugetVersion with
  | Some version ->
    Trace.log " --- Publishing artifacts --- "
    [
      "Release", !! (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Storage/bin/Release/*")
      "Tests/System/Release", !! (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Test.System/bin/Release/*")
      "Tests/Unit/Release", !! (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Test.Unit/bin/Release/*")
    ] |> Zip.zipOfIncludes (sprintf "%s/build/Binaries-%s.zip" Common.baseDirectory version)

    !! (Common.baseDirectory + "/build/Binaries-*.zip") |> Seq.iter (fun f -> Trace.publish ImportData.BuildArtifact f)
    !! (Common.baseDirectory + "/nuget/*.nupkg") |> Seq.iter (fun f -> Trace.publish ImportData.BuildArtifact f)


    match Common.branch with
    | "develop" | "master" ->
      NuGetPublish (fun p ->
        { p with
            AccessKey = Environment.GetEnvironmentVariable("NUGET_TOKEN")
            Project = "slp.evi"
            Version = version
            OutputPath = Common.baseDirectory + "/nuget"
            WorkingDir = Common.baseDirectory + "/nuget"
        })
    | _ -> ()
  | None -> Trace.log "Skipping artifacts publishing"
)

open Fake.Core.TargetOperators

// *** Define Dependencies ***
"InstallDependencies"
 ==> "UpdateAssemblyInfo" <=> "RestorePackages"
 ==> "BeginSonarQube"
 ==> "Build"
 ==> "PrepareDatabase"
 ==> "RunTests"
 ==> "EndSonarQube"
 ==> "UploadCodeCov"
 ==> "Package"
 ==> "PublishArtifacts"

// *** Start Build ***
Target.runOrDefault "PublishArtifacts"