namespace Slp.Evi.Relational.ValueBinders

open Slp.Evi.R2RML
open Slp.Evi.Sparql.Algebra
open Slp.Evi.Relational.Algebra

type BaseValueBinder = { Source: ISqlSource; TermMap: ObjectMapping }

type ValueBinder =
    | Base of SparqlVariable * BaseValueBinder