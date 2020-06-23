namespace Slp.Evi.Relational.ValueBinders

open Slp.Evi.Common.Types
open Slp.Evi.R2RML
open Slp.Evi.Sparql.Algebra
open Slp.Evi.Relational.Algebra

type ValueBinder =
    | EmptyValueBinder
    | BaseValueBinder of ObjectMapping * Map<string, Variable>
    | CoalesceValueBinder of ValueBinder list
    | CaseValueBinder of Variable * Map<int, ValueBinder>
    | ExpressionValueBinder of Expression * NodeType

type ValueBinding = { Variable: SparqlVariable; Binder: ValueBinder }
