#r "paket: 
    nuget Fake.Core.Target 
    nuget Fake.Core.Environment
    nuget Fake.IO.FileSystem
    nuget Fake.DotNet.AssemblyInfoFile
    nuget Fake.BuildServer.AppVeyor //"

open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.BuildServer
open System

// Add support for AppVeyor
if AppVeyor.detect() then
  Trace.log " --- AppVeyor detected --- "
  AppVeyor.install false

// Common stuff for the build
module Common =
  let baseDirectory = __SOURCE_DIRECTORY__

// Logic related to build version retrieval
module VersionLogic =
  type VersionInformation = { Version: string; InformationalVersion: string; NugetVersion: string }

  let appVeyorVersion =
    let buildVersion = AppVeyor.Environment.BuildVersion
    if String.IsNullOrEmpty buildVersion then
      "0.0.0.1"
    else
      buildVersion
  
  let version = { Version=appVeyorVersion; InformationalVersion=appVeyorVersion; NugetVersion=appVeyorVersion }
  

// *** Define Targets ***
Target.create "UpdateAssemblyInfo" (fun _ ->
  Trace.log " --- Updating assembly info --- "
  Trace.log (sprintf " Version: %s" VersionLogic.version.InformationalVersion)
  
  !! "src/**/AssemblyInfo.cs"
  |> Seq.iter(fun asmInfo ->
    let version = VersionLogic.version
    [
      AssemblyInfo.Version version.Version
      AssemblyInfo.FileVersion version.Version
      AssemblyInfo.InformationalVersion version.InformationalVersion
      AssemblyInfo.Copyright (sprintf "Copyright (c) %d" DateTime.Now.Year)
    ] |> AssemblyInfoFile.updateAttributes asmInfo
  )
)

Target.create "BeforeBuild" (fun _ ->
  Trace.log " --- Before build starting --- "
)

Target.create "Build" (fun _ ->
  Trace.log " --- Building the app --- "
)

Target.create "Package" (fun _ ->
  Trace.log " --- Packaging app --- "
)

open Fake.Core.TargetOperators

// *** Define Dependencies ***
"UpdateAssemblyInfo"
  ==> "BeforeBuild"
  ==> "Build"
  ==> "Package"

// *** Start Build ***
Target.runOrDefault "Package"