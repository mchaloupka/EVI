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

  let private baseDirectory = DirectoryInfo(scriptDirectory).FullName

  let buildTempDirectory = Path.Combine(baseDirectory, ".build")

  let nugetDirectory = Path.Combine(buildTempDirectory, "nuget")

  let solutionFile = Path.Combine(baseDirectory, "src", "Slp.Evi.Storage", "Slp.Evi.Storage.sln")

  let databaseConnectionsFile = Path.Combine(baseDirectory, "src", "Slp.Evi.Storage", "Slp.Evi.Test.System", "database.json")
  
  let releaseNotesFile = Path.Combine(baseDirectory, "RELEASE_NOTES.md")

  let branch =
    let b = AppVeyor.Environment.RepoBranch
    if String.IsNullOrEmpty b then "local"
    else b

  let isLocalBuild = branch = "local"

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
    Common.releaseNotesFile
    |> ReleaseNotes.load
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

module Docker =
  let removeDockerContainer containerName =
    [ "stop"; containerName ]
    |> CreateProcess.fromRawCommand "docker"
    |> Proc.run
    |> ignore

    [ "rm"; "-vf"; containerName ]
    |> CreateProcess.fromRawCommand "docker"
    |> Proc.run
    |> ignore

  let isDockerRunning containerName =
    let result =
      [ "ps"; "-a"; "--filter"; sprintf "name=%s" containerName; "--format"; "{{.Names}} {{.Status}}"]
      |> CreateProcess.fromRawCommand "docker"
      |> CreateProcess.redirectOutput
      |> Proc.run

    if result.ExitCode <> 0 then
      failwithf "Failed to check whether docker is running for %s" containerName
    else
      if result.Result.Output.Contains containerName then
        if result.Result.Output.Contains "Exited" then
          removeDockerContainer containerName
          false
        else
          true
      else
        false

  let startDockerDetached containerName imageName portMapping envVariables =
    let result =
      [
        yield "run"
        yield "-d"
        yield "--name"
        yield containerName

        for k, v in portMapping do
          yield "-p"
          yield sprintf "%d:%d" k v

        for k, v in envVariables do
          yield "-e"
          yield sprintf "%s=%s" k v

        yield imageName
      ]
      |> CreateProcess.fromRawCommand "docker"
      |> Proc.run

    if result.ExitCode <> 0 then
      failwithf "Failed to start %s" containerName

  let execInContainer containerName args =
    let result =
      [
        yield "exec"
        yield containerName
        yield! args
      ]
      |> CreateProcess.fromRawCommand "docker"
      |> Proc.run

    if result.ExitCode <> 0 then
      failwithf "Failed to exec %A in %s" args containerName

module MSSQLDatabase =
  let private containerName = "evi-tests-mssqldb"
  let private imageName = "mcr.microsoft.com/mssql/server:2022-latest"
  let private dockerPort = 1453
  let private password = "Password12!" // AppVeyor default password to MS SQL
  let private dbName = "R2RMLTestStore"
  let private localSqlInstance = "(local)\\SQL2017"

  let connectionString =
    if Common.isLocalBuild then
      sprintf "Server=127.0.0.1,%d;Database=%s;User Id=sa;Password=%s" dockerPort dbName password
    else
      sprintf "Server=%s;Database=%s;User ID=sa;Password=%s" localSqlInstance dbName password

  let startDatabase () =
    if Common.isLocalBuild then
      Trace.log "--- Preparing MS SQL database (docker) ---"
      if Docker.isDockerRunning containerName |> not then
        Docker.startDockerDetached containerName imageName [dockerPort, 1433] [
          "ACCEPT_EULA", "Y"
          "SA_PASSWORD", password
        ]

        Trace.log " ... Waiting for MS SQL Server to start up"
        Threading.Thread.Sleep(60000) // Enough time to start MSSQL server

        Trace.log " ... Creating database in MS SQL"
        Docker.execInContainer containerName [
          "/opt/mssql-tools/bin/sqlcmd"
          "-S"
          "localhost"
          "-U"
          "sa"
          "-P"
          password
          "-Q"
          sprintf "USE [master]; CREATE DATABASE [%s]" dbName
        ]
      else
        Trace.log "--- Skipping MS SQL creation as it already exists ---"
    else
      Trace.log "--- Preparing MS SQL database (appveyor) ---"
      
      Trace.log " ... Creating database in MS SQL"
      let result =
        [
          "-S"
          localSqlInstance
          "-Q"
          sprintf "USE [master]; CREATE DATABASE [%s]" dbName
        ]
        |> CreateProcess.fromRawCommand "sqlcmd"
        |> Proc.run

      if result.ExitCode <> 0 then
        failwith "MS SQL Database instantiation failed"

  let tearDown () =
    if Common.isLocalBuild then
      Trace.log "--- Removing MS SQL container ---"
      Docker.removeDockerContainer containerName

module MySQLDatabase =
  let private containerName = "evi-tests-mysqldb"
  let private imageName = "mysql:latest"
  let private dockerPort = 1454
  let private password = "Password12!"
  let private dbName = "R2RMLTestStore"
  let private sqlPort = 3306

  let connectionString =
    let port =
      if Common.isLocalBuild then
        dockerPort
      else
        sqlPort

    sprintf "Server=localhost;Port=%d;User ID=root;Password=%s;Database=%s" port password dbName

  let startDatabase () =
    if Common.isLocalBuild then
      Trace.log "--- Preparing MySQL database (docker) ---"

      if Docker.isDockerRunning containerName |> not then
        Docker.startDockerDetached containerName imageName [dockerPort, 3306] [
          "MYSQL_ROOT_PASSWORD", password
        ]

        Trace.log " ... Waiting for MySQL Server to start up"
        Threading.Thread.Sleep(60000) // Enough time to start MSSQL server

        Trace.log " ... Creating database in MySQL"
        Docker.execInContainer containerName [
          "mysql"
          "-u"
          "root"
          sprintf "-p%s" password
          "-e"
          sprintf "create database %s;" dbName
        ]
      else
        Trace.log "--- Skipping MySQL creation as it already exists ---"
    else
      Trace.log "--- Preparing MySQL database (appveyor) ---"
      Trace.log " ... Creating database in MySQL"
      Docker.execInContainer containerName [
        "mysql"
        "u root"
        sprintf "-p%s" password
        "-e"
        sprintf "create database %s;" dbName
      ]

  let tearDown () =
    if Common.isLocalBuild then
      Trace.log "--- Removing MS SQL container ---"
      Docker.removeDockerContainer containerName

Target.create "RestorePackages" (fun _ ->
  Trace.log "--- Restore packages starting ---"

  Common.solutionFile
  |> DotNet.restore (fun p -> { p with MSBuildParams = p.MSBuildParams |> Build.setMsBuildProps })
)

Target.create "Build" (fun _ ->
  Trace.log "--- Building the app --- "

  let build conf =
    Common.solutionFile
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
  Trace.log"--- Creating temporary build folder ---"
  Common.buildTempDirectory
  |> DirectoryInfo.ofPath
  |> DirectoryInfo.ensure
)

Target.create "Clean" (fun _ ->
  Trace.log "--- Cleaning folders ---"
  Shell.cleanDirs [
    Common.buildTempDirectory
  ]

  Common.solutionFile
  |> DotNet.exec (fun p -> 
    {
      p with
        Verbosity = DotNet.Verbosity.Quiet |> Some
    }
  ) "clean"
  |> ignore
)

Target.create "Package" (fun _ ->
  match VersionLogic.version.NugetVersion with
  | Some version ->
    Trace.log "--- Packaging app --- "

    DirectoryInfo.ensure (DirectoryInfo.ofPath Common.nugetDirectory)

    Common.solutionFile
    |> DotNet.pack (fun p ->
      { p with
          NoBuild = true
          OutputPath = Some Common.nugetDirectory
          Configuration = DotNet.BuildConfiguration.Release
          MSBuildParams = p.MSBuildParams |> Build.setMsBuildProps
          Common = p.Common |> Build.setDotnetCommonWithExtraArgs ("--no-restore --include-source --include-symbols /p:PackageVersion=" + version)
      }
    )

  | None -> Trace.log "Skipping nuget packaging"
)

Target.create "PrepareMSSQLDatabase"  (fun _ ->
  MSSQLDatabase.startDatabase ()
)

Target.create "PrepareMySQLDatabase" (fun _ ->
  MySQLDatabase.startDatabase ()
)

Target.create "PrepareDatabases" (fun _ ->
  Trace.log "--- Updating connection string in database file ---"
  let content =
    Common.databaseConnectionsFile
    |> File.ReadAllText
    |> Newtonsoft.Json.JsonConvert.DeserializeObject
    :?> Newtonsoft.Json.Linq.JToken

  content.["ConnectionStrings"].["mssql"] <- MSSQLDatabase.connectionString |> Newtonsoft.Json.Linq.JValue
  content.["ConnectionStrings"].["mysql"] <- MySQLDatabase.connectionString |> Newtonsoft.Json.Linq.JValue
  
  File.WriteAllText(Common.databaseConnectionsFile, content |> Newtonsoft.Json.JsonConvert.SerializeObject)
)

Target.create "TearDownDatabases" (fun _ ->
  MSSQLDatabase.tearDown ()
  MySQLDatabase.tearDown ()
)

Target.create "RunTests" (fun _ ->
  Trace.log "--- Running tests --- "
  Common.solutionFile
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
  Trace.log "--- Starting benchmarking --- "
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
    Trace.log "--- Publishing artifacts --- "
    !! (Common.nugetDirectory + "/*.nupkg") |> Seq.iter (fun f -> Trace.publish ImportData.BuildArtifact f)

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
                OutputPath = Common.nugetDirectory
                WorkingDir = Common.nugetDirectory
            })
      )
    | _ -> ()
  | None -> Trace.log "Skipping artifacts publishing"
)

open Fake.Core.TargetOperators

// *** Define Dependencies ***
"CreateTempFolder"
 ==> "Clean"
 ==> "RestorePackages"
 ==> "Build"
 ==> "RunTests"
 ==> "Package"
 ==> "PublishArtifacts"

"PrepareMSSQLDatabase"
 ==> "PrepareDatabases"

"PrepareMySQLDatabase"
 ==> "PrepareDatabases"

"PrepareDatabases"
 ==> "RunTests"

"RunTests"
 ?=> "TearDownDatabases"
 ==> "PublishArtifacts"

"RestorePackages"
 ==> "RunBenchmarks"

// *** Start Build ***
Target.runOrDefault "PublishArtifacts"
