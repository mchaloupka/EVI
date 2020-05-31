module Slp.Evi.Common.Utils

let createOptionFromNullable x = if x = null then None else Some x
