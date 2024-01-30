# Benchmark

To run a proper benchmark of SPARQL endpoint, take a look at https://github.com/mchaloupka/r2rml-benchmark

This repository contains a mechanism to benchmark the available system tests. The intention is to have the ability to easily compare performance of two branches. However, it mainly represents the overhead time to generate the SQL query. It does not reflect whether the generated SQL query is optimal as the underlying datasets are extremely small.

It can be executed using the following commands:

```shell
dotnet fsi build.fsx -t Benchmarks
```

After that, the folder BenchmarkDotNet.Artifacts will contains logs and results of the benchmark.

## Last results on this branch

``` ini
BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3085/23H2/2023Update/SunValley3)
Intel Core i7-1065G7 CPU 1.30GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.101
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
```

| Method  | Argument              | Mean     | Error     | StdDev    | Median   |
|-------- |---------------------- |---------:|----------:|----------:|---------:|
| RunTest | mssql-bsb.Query_01    | 4.284 ms | 0.0807 ms | 0.0829 ms | 4.259 ms |
| RunTest | mssql-bsb.Query_02    | 7.104 ms | 0.1397 ms | 0.2216 ms | 7.050 ms |
| RunTest | mssql-bsb.Query_03    | 3.282 ms | 0.0631 ms | 0.0527 ms | 3.274 ms |
| RunTest | mssql-bsb.Query_04    | 4.545 ms | 0.0876 ms | 0.0900 ms | 4.556 ms |
| RunTest | mssql-bsb.Query_05    | 3.689 ms | 0.0468 ms | 0.0415 ms | 3.687 ms |
| RunTest | mssql-bsb.Query_06    | 2.235 ms | 0.0339 ms | 0.0566 ms | 2.215 ms |
| RunTest | mssql-bsb.Query_07    | 5.170 ms | 0.0723 ms | 0.0676 ms | 5.183 ms |
| RunTest | mssql-bsb.Query_08    | 5.295 ms | 0.1010 ms | 0.0992 ms | 5.300 ms |
| RunTest | mssql-bsb.Query_09    | 2.202 ms | 0.0405 ms | 0.0378 ms | 2.204 ms |
| RunTest | mssql-bsb.Query_10    | 3.243 ms | 0.0439 ms | 0.0410 ms | 3.242 ms |
| RunTest | mssql-bsb.Query_11    | 2.558 ms | 0.0511 ms | 0.0908 ms | 2.535 ms |
| RunTest | mssql-bsb.Query_12    | 3.806 ms | 0.0671 ms | 0.0848 ms | 3.814 ms |
| RunTest | mssql-bsb.pe_OrderBy  | 1.937 ms | 0.0373 ms | 0.0803 ms | 1.912 ms |
| RunTest | mssql-sim.\\not_bound | 1.654 ms | 0.0318 ms | 0.0353 ms | 1.655 ms |
| RunTest | mssql-sim._different  | 1.248 ms | 0.0205 ms | 0.0228 ms | 1.249 ms |
| RunTest | mssql-sim.arison_neq  | 1.698 ms | 0.0253 ms | 0.0197 ms | 1.694 ms |
| RunTest | mssql-sim.bind        | 1.501 ms | 0.0300 ms | 0.0411 ms | 1.509 ms |
| RunTest | mssql-sim.d_optional  | 2.728 ms | 0.0747 ms | 0.2168 ms | 2.858 ms |
| RunTest | mssql-sim.distinct    | 1.705 ms | 0.0312 ms | 0.0292 ms | 1.706 ms |
| RunTest | mssql-sim.e_comp_eq2  | 1.333 ms | 0.0263 ms | 0.0292 ms | 1.337 ms |
| RunTest | mssql-sim.empty       | 1.204 ms | 0.0165 ms | 0.0146 ms | 1.203 ms |
| RunTest | mssql-sim.isjunction  | 1.441 ms | 0.0278 ms | 0.0321 ms | 1.431 ms |
| RunTest | mssql-sim.join        | 2.470 ms | 0.0311 ms | 0.0243 ms | 2.463 ms |
| RunTest | mssql-sim.lter\\bound | 2.215 ms | 0.0432 ms | 0.0633 ms | 2.188 ms |
| RunTest | mssql-sim.null        | 1.646 ms | 0.0271 ms | 0.0212 ms | 1.641 ms |
| RunTest | mssql-sim.onjunction  | 2.106 ms | 0.0191 ms | 0.0179 ms | 2.109 ms |
| RunTest | mssql-sim.optional    | 1.589 ms | 0.0318 ms | 0.0326 ms | 1.580 ms |
| RunTest | mssql-sim.parison_eq  | 1.739 ms | 0.0345 ms | 0.0355 ms | 1.729 ms |
| RunTest | mssql-sim.parison_ge  | 1.724 ms | 0.0294 ms | 0.0275 ms | 1.717 ms |
| RunTest | mssql-sim.parison_gt  | 1.447 ms | 0.0244 ms | 0.0358 ms | 1.441 ms |
| RunTest | mssql-sim.parison_le  | 1.706 ms | 0.0157 ms | 0.0131 ms | 1.708 ms |
| RunTest | mssql-sim.parison_lt  | 1.423 ms | 0.0214 ms | 0.0200 ms | 1.417 ms |
| RunTest | mssql-sim.pe_comp_eq  | 1.910 ms | 0.0271 ms | 0.0226 ms | 1.914 ms |
| RunTest | mssql-sim.pe_comp_gt  | 1.704 ms | 0.1364 ms | 0.4023 ms | 1.445 ms |
| RunTest | mssql-sim.single      | 1.446 ms | 0.0262 ms | 0.0358 ms | 1.439 ms |
| RunTest | mssql-sim.ted_filter  | 2.327 ms | 0.0418 ms | 0.0370 ms | 2.334 ms |
| RunTest | mssql-sim.type\\int   | 1.362 ms | 0.0268 ms | 0.0418 ms | 1.348 ms |
| RunTest | mssql-sim.type_equal  | 1.863 ms | 0.0339 ms | 0.0318 ms | 1.856 ms |
| RunTest | mssql-sim.union       | 1.511 ms | 0.0268 ms | 0.0349 ms | 1.503 ms |
| RunTest | mssql-sim.ype\\double | 1.329 ms | 0.0179 ms | 0.0167 ms | 1.331 ms |
| RunTest | mssql-stu.ames_order  | 1.617 ms | 0.0320 ms | 0.0381 ms | 1.619 ms |
| RunTest | mssql-stu.dent_names  | 1.442 ms | 0.0261 ms | 0.0244 ms | 1.451 ms |
| RunTest | mssql-stu.der_offset  | 1.641 ms | 0.0322 ms | 0.0482 ms | 1.637 ms |
| RunTest | mssql-stu.fset_limit  | 1.684 ms | 0.0335 ms | 0.0568 ms | 1.666 ms |
| RunTest | mssql-stu.no_result   | 1.446 ms | 0.0289 ms | 0.0256 ms | 1.443 ms |
| RunTest | mssql-stu.order_desc  | 1.578 ms | 0.0260 ms | 0.0230 ms | 1.576 ms |
| RunTest | mssql-stu.rder_limit  | 1.885 ms | 0.0359 ms | 0.0352 ms | 1.876 ms |
| RunTest | mysql-bsb.Query_01    | 3.486 ms | 0.0575 ms | 0.0481 ms | 3.470 ms |
| RunTest | mysql-bsb.Query_02    | 7.301 ms | 0.0829 ms | 0.0775 ms | 7.310 ms |
| RunTest | mysql-bsb.Query_03    | 4.016 ms | 0.0306 ms | 0.0286 ms | 4.012 ms |
| RunTest | mysql-bsb.Query_04    | 5.265 ms | 0.0854 ms | 0.0713 ms | 5.259 ms |
| RunTest | mysql-bsb.Query_05    | 4.415 ms | 0.0314 ms | 0.0294 ms | 4.420 ms |
| RunTest | mysql-bsb.Query_06    | 2.347 ms | 0.0260 ms | 0.0203 ms | 2.346 ms |
| RunTest | mysql-bsb.Query_07    | 5.908 ms | 0.0450 ms | 0.0398 ms | 5.906 ms |
| RunTest | mysql-bsb.Query_08    | 4.720 ms | 0.0625 ms | 0.0522 ms | 4.704 ms |
| RunTest | mysql-bsb.Query_09    | 3.254 ms | 0.0247 ms | 0.0219 ms | 3.247 ms |
| RunTest | mysql-bsb.Query_10    | 3.823 ms | 0.0270 ms | 0.0210 ms | 3.829 ms |
| RunTest | mysql-bsb.Query_11    | 3.430 ms | 0.0619 ms | 0.0783 ms | 3.392 ms |
| RunTest | mysql-bsb.Query_12    | 4.326 ms | 0.0237 ms | 0.0210 ms | 4.325 ms |
| RunTest | mysql-bsb.pe_OrderBy  | 2.858 ms | 0.0563 ms | 0.0602 ms | 2.830 ms |
| RunTest | mysql-sim.\\not_bound | 2.334 ms | 0.0255 ms | 0.0213 ms | 2.336 ms |
| RunTest | mysql-sim._different  | 1.829 ms | 0.0616 ms | 0.1777 ms | 1.717 ms |
| RunTest | mysql-sim.arison_neq  | 2.211 ms | 0.0194 ms | 0.0162 ms | 2.211 ms |
| RunTest | mysql-sim.bind        | 1.992 ms | 0.0386 ms | 0.0554 ms | 1.972 ms |
| RunTest | mysql-sim.d_optional  | 2.642 ms | 0.0417 ms | 0.0390 ms | 2.637 ms |
| RunTest | mysql-sim.distinct    | 2.223 ms | 0.0177 ms | 0.0217 ms | 2.220 ms |
| RunTest | mysql-sim.e_comp_eq2  | 1.751 ms | 0.0173 ms | 0.0162 ms | 1.754 ms |
| RunTest | mysql-sim.empty       | 1.622 ms | 0.0159 ms | 0.0148 ms | 1.618 ms |
| RunTest | mysql-sim.isjunction  | 2.003 ms | 0.0209 ms | 0.0196 ms | 1.995 ms |
| RunTest | mysql-sim.join        | 2.662 ms | 0.0524 ms | 0.0700 ms | 2.630 ms |
| RunTest | mysql-sim.lter\\bound | 2.259 ms | 0.0227 ms | 0.0201 ms | 2.256 ms |
| RunTest | mysql-sim.null        | 1.968 ms | 0.0393 ms | 0.0918 ms | 1.932 ms |
| RunTest | mysql-sim.onjunction  | 1.999 ms | 0.0275 ms | 0.0244 ms | 1.992 ms |
| RunTest | mysql-sim.optional    | 2.204 ms | 0.0213 ms | 0.0200 ms | 2.202 ms |
| RunTest | mysql-sim.parison_eq  | 1.952 ms | 0.0266 ms | 0.0249 ms | 1.952 ms |
| RunTest | mysql-sim.parison_ge  | 1.961 ms | 0.0251 ms | 0.0223 ms | 1.961 ms |
| RunTest | mysql-sim.parison_gt  | 1.936 ms | 0.0336 ms | 0.0298 ms | 1.933 ms |
| RunTest | mysql-sim.parison_le  | 1.928 ms | 0.0225 ms | 0.0200 ms | 1.926 ms |
| RunTest | mysql-sim.parison_lt  | 1.996 ms | 0.0350 ms | 0.0524 ms | 2.009 ms |
| RunTest | mysql-sim.pe_comp_eq  | 2.377 ms | 0.0255 ms | 0.0239 ms | 2.377 ms |
| RunTest | mysql-sim.pe_comp_gt  | 2.321 ms | 0.0428 ms | 0.0772 ms | 2.294 ms |
| RunTest | mysql-sim.single      | 2.126 ms | 0.0185 ms | 0.0154 ms | 2.121 ms |
| RunTest | mysql-sim.ted_filter  | 2.451 ms | 0.0209 ms | 0.0185 ms | 2.453 ms |
| RunTest | mysql-sim.type\\int   | 1.886 ms | 0.0269 ms | 0.0251 ms | 1.881 ms |
| RunTest | mysql-sim.type_equal  | 2.295 ms | 0.0272 ms | 0.0241 ms | 2.297 ms |
| RunTest | mysql-sim.union       | 2.362 ms | 0.0548 ms | 0.1590 ms | 2.405 ms |
| RunTest | mysql-sim.ype\\double | 1.866 ms | 0.0352 ms | 0.0330 ms | 1.851 ms |
| RunTest | mysql-stu.ames_order  | 2.294 ms | 0.0458 ms | 0.0596 ms | 2.280 ms |
| RunTest | mysql-stu.dent_names  | 2.155 ms | 0.0291 ms | 0.0272 ms | 2.150 ms |
| RunTest | mysql-stu.der_offset  | 2.314 ms | 0.0458 ms | 0.0685 ms | 2.289 ms |
| RunTest | mysql-stu.fset_limit  | 2.276 ms | 0.0409 ms | 0.0341 ms | 2.270 ms |
| RunTest | mysql-stu.no_result   | 1.746 ms | 0.0339 ms | 0.0497 ms | 1.727 ms |
| RunTest | mysql-stu.order_desc  | 2.052 ms | 0.0263 ms | 0.0233 ms | 2.052 ms |
| RunTest | mysql-stu.rder_limit  | 2.030 ms | 0.0336 ms | 0.0314 ms | 2.030 ms |
