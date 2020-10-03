module Slp.Evi.Storage.Core.Common.Utils

let createOptionFromNullable x = if x = null then None else Some x
