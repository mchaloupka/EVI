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
open System.IO
open System.Text.RegularExpressions

BuildServer.install [
  AppVeyor.Installer
]

// Common stuff for the build
module Common =
  let private scriptDirectory = __SOURCE_DIRECTORY__

  let baseDirectory = Directory.GetParent scriptDirectory

  let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
    else None

// Logic related to build version retrieval
module VersionLogic =
  type VersionInformation = { Version: string; InformationalVersion: string; NugetVersion: string option }

  let buildNumber =
    let b = AppVeyor.Environment.BuildNumber
    if String.IsNullOrEmpty b then "1"
    else b

  let branch =
    let b = AppVeyor.Environment.RepoBranch
    if String.IsNullOrEmpty b then "local"
    else b

  let tagVersion =
    if AppVeyor.Environment.RepoTag then
      let tag = AppVeyor.Environment.RepoTagName

      Trace.log (sprintf "This is a tag build (tag: %s, branch: %s, build: %s)" tag branch buildNumber)

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
        match branch with
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