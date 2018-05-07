#r "paket:
nuget Fake.Core.Target //"
// include Fake modules, see Fake modules section

open Fake.Core

// *** Define Targets ***
Target.create "Clean" (fun _ ->
  Trace.log " --- Cleaning stuff --- "
)

Target.create "BeforeBuild" (fun _ ->
  Trace.log " --- Before build starting --- "
)

Target.create "Build" (fun _ ->
  Trace.log " --- Building the app --- "
)

Target.create "AfterBuild" (fun _ ->
  Trace.log " --- Before build starting --- "
)

Target.create "Deploy" (fun _ ->
  Trace.log " --- Deploying app --- "
)

open Fake.Core.TargetOperators

// *** Define Dependencies ***
"Clean"
  ==> "BeforeBuild"
  ==> "Build"
  ==> "AfterBuild"
  ==> "Deploy"

// *** Start Build ***
Target.runOrDefault "Deploy"