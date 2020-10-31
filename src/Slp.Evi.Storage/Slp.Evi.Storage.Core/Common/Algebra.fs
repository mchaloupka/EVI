module Slp.Evi.Storage.Core.Common.Algebra

type ArithmeticOperator =
    | Add
    | Subtract
    | Multiply
    | Divide

type OrderingDirection =
    | Ascending
    | Descending

type Comparisons =
    | GreaterThan
    | GreaterOrEqualThan
    | LessThan
    | LessOrEqualThan
    | EqualTo