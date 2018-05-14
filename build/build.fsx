#r "paket: 
    nuget Fake.Core.Target 
    nuget Fake.Core.Environment
    nuget Fake.Core.Xml
    nuget Fake.Core.Process
    nuget Fake.IO.FileSystem
    nuget Fake.DotNet.AssemblyInfoFile
    nuget Fake.BuildServer.AppVeyor
    nuget Fake.DotNet.Nuget
    nuget Fake.DotNet.MSBuild
    nuget Fake.Windows.Chocolatey
    nuget Fake.Testing.SonarQube //"

open System
open System.IO
open System.Xml
open System.Text.RegularExpressions
open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.DotNet
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
  
  let version =
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
        | "develop" -> Some "alpha"
        | "master" -> Some "beta"
        | _ -> None
      let (version, informational, nuget) =
        match suffix with
        | Some s -> ((sprintf "0.0.1.%s" buildNumber), (sprintf "0.0.1.%s-%s" buildNumber s), Some (sprintf "0.0.1-%s%s" s buildNumber))
        | None ->
          let v = sprintf "0.0.1.%s" buildNumber
          (v, v, None)
      { Version = version; InformationalVersion = informational; NugetVersion = nuget} 
  

// *** Define Targets ***
Target.create "UpdateAssemblyInfo" (fun _ ->
  Trace.log " --- Updating assembly info --- "
  Trace.log (sprintf " Version: %s" VersionLogic.version.InformationalVersion)
  let copyrightYear = DateTime.Now.Year
  
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
  |> RestoreMSSolutionPackages (fun p ->
    { p with OutputPath = (Common.baseDirectory + "/src/Slp.Evi.Storage/packages") }
  )
)

Target.create "Build" (fun _ ->
  Trace.log " --- Building the app --- "

  (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Storage.sln")
  |> MSBuild.build (fun p ->
    { p with
        Verbosity = Some MSBuildVerbosity.Quiet
        Targets = ["Build"]
        Properties = 
          [
            "Configuration", "Release"
          ]
        MaxCpuCount = Some None
    }
  )
)

Target.create "InstallDependencies" (fun _ ->
  Trace.log " --- Installing dependencies --- "
  let toInstall =
    match Common.branch with
    | "local" -> []
    | "develop" -> 
      [ 
        "msbuild-sonarqube-runner"
        "opencover"
      ]
    | _ -> [ "opencover" ]
  
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
              "sonar.cs.opencover.reportsPaths=coverage.xml"
            ]
      }
    )
  else Trace.log "SonarQube start skipped (not develop branch)"
)

Target.create "RunTests" (fun _ ->
  Trace.log " --- Running tests --- "
)

Target.create "EndSonarQube" (fun _ ->
  if Common.branch = "develop" then
    Trace.log " --- Exiting SonarQube analyzer --- "
    SonarQube.finish (Some (fun p ->
      { p with
          Settings = [ ("sonar.login=" + Environment.GetEnvironmentVariable("SONARQUBE_TOKEN")) ]
      }
    ))
  else Trace.log "SonarQube end skipped (not develop branch)"
)

Target.create "Package" (fun _ ->
  match VersionLogic.version.NugetVersion with
  | Some version ->
    Trace.log " --- Packaging app --- "
    !! (Common.baseDirectory + "/src/**/*.nuspec")
    |> Seq.iter(fun x ->
      x
      |> NuGetPack (fun p ->
          { p with
              Version = version
          }
        ) 
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
      let doc = new XmlDocument()
      doc.LoadXml file
      Xml.replaceXPath "//connectionStrings/add[@name=\"mssql_connection\"]/@connectionString" "" doc
      doc.Save file

    !! (Common.baseDirectory + "/src/**/Slp.Evi.Test.System.dll.config")
    |> Seq.iter updateConfig

    Trace.log (sprintf "Creating database in %s" sqlInstance)
    let result = Shell.Exec("sqlcmd", sprintf "-S \"%s\" -Q \"USE [master]; CREATE DATABASE [%s]\"" sqlInstance dbName)
    if result <> 0 then failwith "Database creation failed"
)

Target.create "PublishArtifacts" (fun _ ->
  Trace.log " --- Publishing artifacts --- "
)

open Fake.Core.TargetOperators

// *** Define Dependencies ***
"InstallDependencies"
 ==> "UpdateAssemblyInfo" <=> "RestorePackages"
 ==> "BeginSonarQube"
 ==> "Build"
 ==> "PrepareDatabase"
 ==> "EndSonarQube"
 ==> "Package"
 ==> "PublishArtifacts"

// *** Start Build ***
Target.runOrDefault "Package"