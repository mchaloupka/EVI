using System;
using System.Collections.Generic;
using System.IO;

namespace Slp.Evi.Test.System.Sparql;

public static class SparqlTestData
{
    public static string GetPath(string dataFile)
    {
        var root = AppDomain.CurrentDomain.BaseDirectory;
        var path = Path.Combine(root, $".\\Sparql\\{dataFile}");
        return path;
    }

    public static string GetQuery(string queryFile)
    {
        using var sr = new StreamReader(new FileStream(GetPath(queryFile), FileMode.Open, FileAccess.Read));
        return sr.ReadToEnd();
    }

    public static IEnumerable<object[]> TestData => new List<object[]>
    {
        new object[] {"simple", "single", QueryVerificationType.BagEqual},
        new object[] {"simple", "join", QueryVerificationType.BagEqual},
        new object[] {"simple", "union", QueryVerificationType.BagEqual},
        new object[] {"simple", "empty", QueryVerificationType.BagEqual},
        new object[] {"simple", "null", QueryVerificationType.BagEqual},
        new object[] {"simple", "optional", QueryVerificationType.BagEqual},
        new object[] {"simple", "bind", QueryVerificationType.BagEqual},
        new object[] {"simple", "nested_optional", QueryVerificationType.BagEqual},
        new object[] {"simple", "nested_filter", QueryVerificationType.BagEqual},
        new object[] {"simple", "distinct", QueryVerificationType.BagEqual},

        new object[] {"simple", @"filter\bound", QueryVerificationType.BagEqual},
        new object[] {"simple", @"filter\not_bound", QueryVerificationType.BagEqual},
        new object[] {"simple", @"filter\comparison_gt", QueryVerificationType.BagEqual},
        new object[] {"simple", @"filter\comparison_ge", QueryVerificationType.BagEqual},
        new object[] {"simple", @"filter\comparison_lt", QueryVerificationType.BagEqual},
        new object[] {"simple", @"filter\comparison_le", QueryVerificationType.BagEqual},
        new object[] {"simple", @"filter\comparison_eq", QueryVerificationType.BagEqual},
        new object[] {"simple", @"filter\comparison_neq", QueryVerificationType.BagEqual},
        new object[] {"simple", @"filter\disjunction", QueryVerificationType.BagEqual},
        new object[] {"simple", @"filter\conjunction", QueryVerificationType.BagEqual},

        new object[] {"simple", @"type\int", QueryVerificationType.BagEqual},
        new object[] {"simple", @"type\double", QueryVerificationType.BagEqual},
        new object[] {"simple", @"type\type_equal", QueryVerificationType.BagEqual},
        new object[] {"simple", @"type\type_comp_eq", QueryVerificationType.BagEqual},
        new object[] {"simple", @"type\type_comp_eq2", QueryVerificationType.BagEqual},
        new object[] {"simple", @"type\type_comp_gt", QueryVerificationType.BagEqual},
        new object[] {"simple", @"type\type_join_different", QueryVerificationType.BagEqual},

        new object[] {"students", "no_result", QueryVerificationType.BagEqual},
        new object[] {"students", "student_names", QueryVerificationType.BagEqual},
        new object[] {"students", "student_names_order", QueryVerificationType.Equal},
        new object[] {"students", "student_names_order_desc", QueryVerificationType.Equal},
        new object[] {"students", "student_names_order_limit", QueryVerificationType.Equal},
        new object[] {"students", "student_names_order_offset", QueryVerificationType.Equal},
        new object[] {"students", "student_names_order_offset_limit", QueryVerificationType.Equal},

        new object[] {"bsbm", "ProductType_OrderBy", QueryVerificationType.Equal},
        new object[] {"bsbm", "Query_01", QueryVerificationType.Equal},
        new object[] {"bsbm", "Query_02", QueryVerificationType.BagEqual},
        new object[] {"bsbm", "Query_03", QueryVerificationType.Equal},
        new object[] {"bsbm", "Query_04", QueryVerificationType.Equal},
        new object[] {"bsbm", "Query_05", QueryVerificationType.Equal},
        new object[] {"bsbm", "Query_06", QueryVerificationType.BagEqual},
        new object[] {"bsbm", "Query_07", QueryVerificationType.BagEqual},
        new object[] {"bsbm", "Query_08", QueryVerificationType.Equal},
        new object[] {"bsbm", "Query_09", QueryVerificationType.None},
        new object[] {"bsbm", "Query_10", QueryVerificationType.Equal},
        new object[] {"bsbm", "Query_11", QueryVerificationType.BagEqual},
        new object[] {"bsbm", "Query_12", QueryVerificationType.None}
    };
}