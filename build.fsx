#!/usr/bin/env -S dotnet fsi

#r "nuget: Fake.Core.Target"
#r "nuget: Fake.Core.Environment"
#r "nuget: Fake.Core.Xml"
#r "nuget: Fake.Core.Process"
#r "nuget: Fake.Core.ReleaseNotes"
#r "nuget: Fake.IO.FileSystem"
#r "nuget: Fake.IO.Zip"
#r "nuget: Fake.Net.Http"
#r "nuget: Fake.DotNet.Cli"
#r "nuget: Fake.DotNet.AssemblyInfoFile"
#r "nuget: Fake.BuildServer.AppVeyor"
#r "nuget: Fake.DotNet.Nuget"
#r "nuget: Fake.DotNet.MSBuild"
#r "nuget: Newtonsoft.Json"

open System
open System.IO
open System.Text.RegularExpressions
open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.Net
open Fake.BuildServer
open Fake.DotNet.NuGet.NuGet

// Boilerplate
Environment.GetCommandLineArgs()
|> Array.skip 2 // skip fsi.exe; build.fsx
|> Array.toList
|> Context.FakeExecutionContext.Create false __SOURCE_FILE__
|> Context.RuntimeContext.Fake
|> Context.setExecutionContext

BuildServer.install [
  AppVeyor.Installer
]

// *** Common stuff for the build ***
module Common =
  let private scriptDirectory = __SOURCE_DIRECTORY__

  let baseDirectory = DirectoryInfo(scriptDirectory).FullName

  let buildTempDirectory = Path.Combine(baseDirectory, ".build")

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

// *** Build helpers ***
module Build =
  let customBuildProps =
    sprintf "/p:Version=%s /p:AssemblyVersion=%s" VersionLogic.version.Version VersionLogic.version.Version

  let setDotnetCommon (defaults: DotNet.Options) =
    { defaults with CustomParams = customBuildProps |> Some }

  let setDotnetCommonWithExtraArgs extraArgs (defaults: DotNet.Options) =
    { defaults with CustomParams = sprintf "%s %s" customBuildProps extraArgs |> Some }

  let setMsBuildProps (defaults: Fake.DotNet.MSBuild.CliArguments) =
    {
      defaults with
        DisableInternalBinLog = true
        Verbosity = MSBuildVerbosity.Quiet |> Some
        MaxCpuCount = None |> Some
    }

Target.create "RestorePackages" (fun _ ->
  Trace.log "--- Restore packages starting ---"

  (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Storage.sln")
  |> DotNet.restore (fun p -> { p with MSBuildParams = p.MSBuildParams |> Build.setMsBuildProps })
)

Target.create "Build" (fun _ ->
  Trace.log " --- Building the app --- "

  let project = (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Storage.sln")

  let build conf =
    project
    |> DotNet.build (fun p ->
      {
        p with
          Configuration = conf |> DotNet.BuildConfiguration.fromString
          MSBuildParams = p.MSBuildParams |> Build.setMsBuildProps
          Common = p.Common |> Build.setDotnetCommon
      }
    )

  build "Debug"
  build "Release"
)

Target.create "CreateTempFolder" (fun _ ->
  Trace.log("--- Creating temporary build folder ---")

  let di = DirectoryInfo(Common.buildTempDirectory)
  di.Create()
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
          MSBuildParams = p.MSBuildParams |> Build.setMsBuildProps
          Common = p.Common |> Build.setDotnetCommonWithExtraArgs ("--no-restore --include-source --include-symbols /p:PackageVersion=" + version)
      }
    )

  | None -> Trace.log "Skipping nuget packaging"
)

Target.create "PrepareDatabase" (fun _ ->
  match Common.branch with
  | "local" -> Trace.log "Skipping database preparation "
  | _ ->
    Trace.log " --- Preparing database --- "
    let sqlInstance = "(local)\\SQL2017";
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
  (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Storage.sln")
  |> DotNet.test (fun p ->
    { p with
        NoBuild = true
        Configuration = DotNet.BuildConfiguration.Debug
        MSBuildParams = p.MSBuildParams |> Build.setMsBuildProps
        Common = p.Common |> Build.setDotnetCommon
    }
  )
)

Target.create "RunBenchmarks" (fun _ ->
  Trace.log " --- Starting benchmarking --- "
  let startProc =
    DotNet.exec
      id
      "run"
      (sprintf "-p .\\src\\Slp.Evi.Storage\\Slp.Evi.Benchmark -c Release")

  if not startProc.OK then failwithf "Process failed with %A" startProc
)

Target.create "PublishArtifacts" (fun _ ->
  match VersionLogic.version.NugetVersion with
  | Some version ->
    Trace.log " --- Publishing artifacts --- "

    [
      !! (Common.baseDirectory + @"\src\Slp.Evi.Storage\Slp.Evi.Storage\bin\**\*")
        |> GlobbingPattern.setBaseDir (Common.baseDirectory + @"\src\Slp.Evi.Storage\Slp.Evi.Storage\bin")
        |> Zip.filesAsSpecs ""
        |> Zip.moveToFolder "Library"
      !! (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Test.System/bin/**/*")
        |> GlobbingPattern.setBaseDir (Common.baseDirectory + @"\src\Slp.Evi.Storage\Slp.Evi.Test.System\bin")
        |> Zip.filesAsSpecs ""
        |> Zip.moveToFolder "Tests/System"
      !! (Common.baseDirectory + "/src/Slp.Evi.Storage/Slp.Evi.Test.Unit/bin/**/*")
        |> GlobbingPattern.setBaseDir (Common.baseDirectory + @"\src\Slp.Evi.Storage\Slp.Evi.Test.Unit\bin")
        |> Zip.filesAsSpecs ""
        |> Zip.moveToFolder "Tests/Unit"
    ]
    |> Seq.concat
    |> Zip.zipSpec (sprintf "%s/Binaries-%s.zip" Common.buildTempDirectory version)

    !! (Common.buildTempDirectory + "/Binaries-*.zip") |> Seq.iter (fun f -> Trace.publish ImportData.BuildArtifact f)
    !! (Common.baseDirectory + "/nuget/*.nupkg") |> Seq.iter (fun f -> Trace.publish ImportData.BuildArtifact f)

    match Common.branch with
    | "develop" | "master" ->
      [
        "slp.evi"
        "slp.evi.core"
        "slp.evi.mssql"
      ]
      |> List.iter (
        fun project ->
          NuGetPublish (fun p ->
            { p with
                AccessKey = Environment.GetEnvironmentVariable("NUGET_TOKEN")
                Project = project
                Version = version
                OutputPath = Common.baseDirectory + "/nuget"
                WorkingDir = Common.baseDirectory + "/nuget"
            })
      )
    | _ -> ()
  | None -> Trace.log "Skipping artifacts publishing"
)

open Fake.Core.TargetOperators

// *** Define Dependencies ***
"CreateTempFolder"
 ==> "RestorePackages"
 ==> "Build"
 ==> "PrepareDatabase"
 ==> "RunTests"
 ==> "Package"
 ==> "PublishArtifacts"

"RestorePackages"
 ==> "RunBenchmarks"

// *** Start Build ***
Target.runOrDefault "PublishArtifacts"