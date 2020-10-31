# Benchmark

To run a proper benchmark of SPARQL endpoint, take a look at https://github.com/mchaloupka/r2rml-benchmark

This repository contains a mechanism to benchmark the available system tests. The intention is to have the ability to easily compare performance of two branches. However, it mainly represents the overhead time to generate the SQL query. It does not reflect whether the generated SQL query is optimal as the underlying datasets are extremely small.

It can be executed using the following commands:
```
dotnet tool restore
dotnet fake build -t RunBenchmarks
```

After that, the folder BenchmarkDotNet.Artifacts will contains logs and results of the benchmark.

## Last results on this branch
``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.264 (2004/?/20H1)
Intel Core i7-1065G7 CPU 1.30GHz, 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.300
  [Host]     : .NET Core 3.1.4 (CoreCLR 4.700.20.20201, CoreFX 4.700.20.22101), X64 RyuJIT
  DefaultJob : .NET Core 3.1.4 (CoreCLR 4.700.20.20201, CoreFX 4.700.20.22101), X64 RyuJIT


```
|  Method |             Argument |       Mean |     Error |    StdDev |     Median |
|-------- |--------------------- |-----------:|----------:|----------:|-----------:|
| **RunTest** | **bsb.ductType_OrderBy** | **1,207.6 μs** |  **17.39 μs** |  **15.42 μs** | **1,206.4 μs** |
| **RunTest** |        **bsbm-Query_01** | **2,982.4 μs** |  **59.20 μs** | **149.61 μs** | **2,925.1 μs** |
| **RunTest** |        **bsbm-Query_02** | **7,363.7 μs** | **145.91 μs** | **304.57 μs** | **7,382.0 μs** |
| **RunTest** |        **bsbm-Query_03** | **4,031.7 μs** |  **92.69 μs** | **270.38 μs** | **4,020.2 μs** |
| **RunTest** |        **bsbm-Query_04** | **7,204.9 μs** | **201.51 μs** | **574.92 μs** | **6,963.4 μs** |
| **RunTest** |        **bsbm-Query_05** | **4,032.4 μs** |  **80.53 μs** | **166.31 μs** | **3,966.3 μs** |
| **RunTest** |        **bsbm-Query_06** | **1,245.5 μs** |  **23.50 μs** |  **33.70 μs** | **1,244.8 μs** |
| **RunTest** |        **bsbm-Query_07** | **7,641.8 μs** | **115.22 μs** |  **96.22 μs** | **7,645.6 μs** |
| **RunTest** |        **bsbm-Query_08** | **5,821.9 μs** | **115.71 μs** | **233.74 μs** | **5,733.4 μs** |
| **RunTest** |        **bsbm-Query_09** | **2,894.2 μs** |  **57.46 μs** | **126.12 μs** | **2,829.0 μs** |
| **RunTest** |        **bsbm-Query_10** | **4,014.0 μs** | **134.91 μs** | **397.78 μs** | **3,876.2 μs** |
| **RunTest** |        **bsbm-Query_11** | **3,173.2 μs** | **101.68 μs** | **293.36 μs** | **3,072.7 μs** |
| **RunTest** |        **bsbm-Query_12** | **4,131.3 μs** | **145.02 μs** | **427.61 μs** | **3,986.2 μs** |
| **RunTest** | **sim.e_join_different** |   **309.2 μs** |   **9.82 μs** |  **28.95 μs** |   **310.4 μs** |
| **RunTest** | **sim.er\comparison_eq** |   **533.0 μs** |  **44.95 μs** | **132.54 μs** |   **492.5 μs** |
| **RunTest** | **sim.er\comparison_ge** |   **907.3 μs** |  **28.38 μs** |  **83.68 μs** |   **913.4 μs** |
| **RunTest** | **sim.er\comparison_gt** |   **577.3 μs** |  **58.20 μs** | **171.60 μs** |   **499.3 μs** |
| **RunTest** | **sim.er\comparison_le** |   **465.6 μs** |  **16.49 μs** |  **48.35 μs** |   **466.6 μs** |
| **RunTest** | **sim.er\comparison_lt** |   **464.8 μs** |  **17.61 μs** |  **51.93 μs** |   **456.3 μs** |
| **RunTest** | **sim.filter\not_bound** |   **742.3 μs** |  **26.15 μs** |  **73.75 μs** |   **729.3 μs** |
| **RunTest** | **sim.lter\conjunction** |   **520.4 μs** |  **18.34 μs** |  **53.79 μs** |   **515.7 μs** |
| **RunTest** | **sim.lter\disjunction** |   **524.2 μs** |  **15.76 μs** |  **45.97 μs** |   **517.4 μs** |
| **RunTest** |  **sim.nested_optional** | **1,546.5 μs** |  **56.27 μs** | **165.03 μs** | **1,493.2 μs** |
| **RunTest** | **sim.pe\type_comp_eq2** |   **531.2 μs** |  **18.78 μs** |  **54.78 μs** |   **520.7 μs** |
| **RunTest** | **sim.r\comparison_neq** |   **448.4 μs** |  **13.77 μs** |  **39.06 μs** |   **446.7 μs** |
| **RunTest** |  **sim.type\type_equal** |   **299.5 μs** |   **8.43 μs** |  **24.72 μs** |   **297.2 μs** |
| **RunTest** | **sim.ype\type_comp_eq** |   **771.8 μs** |  **26.06 μs** |  **76.85 μs** |   **749.9 μs** |
| **RunTest** | **sim.ype\type_comp_gt** |   **488.0 μs** |  **17.95 μs** |  **52.65 μs** |   **474.6 μs** |
| **RunTest** |          **simple-bind** |   **546.8 μs** |  **20.42 μs** |  **60.22 μs** |   **529.9 μs** |
| **RunTest** |      **simple-distinct** |   **827.9 μs** |  **22.09 μs** |  **63.74 μs** |   **813.9 μs** |
| **RunTest** |         **simple-empty** |   **265.5 μs** |   **8.00 μs** |  **23.45 μs** |   **263.8 μs** |
| **RunTest** |  **simple-filter\bound** |   **774.9 μs** |  **38.39 μs** | **112.59 μs** |   **740.5 μs** |
| **RunTest** |          **simple-join** | **1,413.1 μs** |  **48.21 μs** | **141.40 μs** | **1,381.0 μs** |
| **RunTest** | **simple-nested_filter** | **1,329.4 μs** |  **61.98 μs** | **181.78 μs** | **1,282.5 μs** |
| **RunTest** |          **simple-null** |   **423.3 μs** |  **12.61 μs** |  **37.18 μs** |   **420.8 μs** |
| **RunTest** |      **simple-optional** |   **712.3 μs** |  **28.78 μs** |  **84.41 μs** |   **684.0 μs** |
| **RunTest** |        **simple-single** |   **472.5 μs** |  **16.69 μs** |  **49.21 μs** |   **458.9 μs** |
| **RunTest** |   **simple-type\double** |   **373.8 μs** |  **12.71 μs** |  **37.47 μs** |   **369.8 μs** |
| **RunTest** |      **simple-type\int** |   **367.1 μs** |  **11.58 μs** |  **34.14 μs** |   **361.9 μs** |
| **RunTest** |         **simple-union** |   **687.4 μs** |  **24.16 μs** |  **70.10 μs** |   **672.4 μs** |
| **RunTest** | **stu.ames_order_limit** |   **718.5 μs** |  **24.11 μs** |  **71.08 μs** |   **695.8 μs** |
| **RunTest** | **stu.dent_names_order** |   **702.8 μs** |  **33.20 μs** |  **96.85 μs** |   **669.6 μs** |
| **RunTest** | **stu.der_offset_limit** |   **735.6 μs** |  **25.94 μs** |  **76.09 μs** |   **716.0 μs** |
| **RunTest** | **stu.mes_order_offset** |   **733.8 μs** |  **29.56 μs** |  **85.28 μs** |   **711.1 μs** |
| **RunTest** | **stu.names_order_desc** |   **728.3 μs** |  **25.67 μs** |  **75.27 μs** |   **722.0 μs** |
| **RunTest** |    **stu.student_names** |   **627.5 μs** |  **20.98 μs** |  **61.87 μs** |   **609.5 μs** |
| **RunTest** |   **students-no_result** |   **308.5 μs** |   **9.15 μs** |  **26.54 μs** |   **307.9 μs** |
