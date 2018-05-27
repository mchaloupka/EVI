using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace Slp.Evi.Test.System.SPARQL.SPARQL_TestSuite
{
    public abstract class SparqlTestSuite
        : BaseTestSuite
    {
        private readonly SparqlFixture _fixture;

        protected SparqlTestSuite(SparqlFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void TestQuery(string storagePath, string folder, string testName, QueryVerificationType verificationType)
        {
            var storage = _fixture.GetStorage(storagePath);
            var query = GetQuery($"Data\\{folder}\\{testName}.rq");
            var expected = GetExpected($"Data\\{folder}\\{testName}.srx");
            var result = storage.Query(query);

            switch (verificationType)
            {
                case QueryVerificationType.BagEqual:
                    AssertBagEqual(expected, result);
                    break;
                case QueryVerificationType.Equal:
                    AssertEqual(expected, result);
                    break;
                case QueryVerificationType.None:
                    Assert.NotNull(result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(verificationType), verificationType, null);
            }

        }

        private static XDocument GetExpected(string resultFile)
        {
            if (resultFile == null)
                return null;

            var doc = XDocument.Load(SparqlTestHelpers.GetPath(resultFile));
            return doc;
        }

        private static string GetQuery(string queryFile)
        {
            var query = string.Empty;

            using (var sr = new StreamReader(new FileStream(SparqlTestHelpers.GetPath(queryFile), FileMode.Open, FileAccess.Read)))
            {
                query = sr.ReadToEnd();
            }

            return query;
        }

        public static IEnumerable<object[]> TestData => new List<object[]>
        {
            new object[] {"simple.xml", "Simple", "single", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", "Simple", "single", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", "Simple", "join", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", "Simple", "union", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", "Simple", "empty", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", "Simple", "null", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", "Simple", "optional", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", "Simple", "bind", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", "Simple", "nested_optional", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", "Simple", "nested_filter", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", "Simple", "distinct", QueryVerificationType.BagEqual},

            new object[] {"simple.xml", @"Simple\Filter", "bound", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Filter", "not_bound", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Filter", "comparison_gt", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Filter", "comparison_ge", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Filter", "comparison_lt", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Filter", "comparison_le", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Filter", "comparison_eq", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Filter", "comparison_neq", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Filter", "disjunction", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Filter", "conjunction", QueryVerificationType.BagEqual},

            new object[] {"simple.xml", @"Simple\Type", "int", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Type", "double", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Type", "type_equal", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Type", "type_comp_eq", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Type", "type_comp_eq2", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Type", "type_comp_gt", QueryVerificationType.BagEqual},
            new object[] {"simple.xml", @"Simple\Type", "type_join_different", QueryVerificationType.BagEqual},

            new object[] {"students.xml", "Students", "no_result", QueryVerificationType.BagEqual},
            new object[] {"students.xml", "Students", "student_names", QueryVerificationType.BagEqual},
            new object[] {"students.xml", "Students", "student_names_order", QueryVerificationType.Equal},
            new object[] {"students.xml", "Students", "student_names_order_desc", QueryVerificationType.Equal},
            new object[] {"students.xml", "Students", "student_names_order_limit", QueryVerificationType.Equal},
            new object[] {"students.xml", "Students", "student_names_order_offset", QueryVerificationType.Equal},
            new object[] {"students.xml", "Students", "student_names_order_offset_limit", QueryVerificationType.Equal},

            new object[] {"bsbm.xml", "Bsbm", "ProductType_OrderBy", QueryVerificationType.Equal},
            new object[] {"bsbm.xml", "Bsbm", "Query_01", QueryVerificationType.Equal},
            new object[] {"bsbm.xml", "Bsbm", "Query_02", QueryVerificationType.BagEqual},
            new object[] {"bsbm.xml", "Bsbm", "Query_03", QueryVerificationType.Equal},
            new object[] {"bsbm.xml", "Bsbm", "Query_04", QueryVerificationType.Equal},
            new object[] {"bsbm.xml", "Bsbm", "Query_05", QueryVerificationType.Equal},
            new object[] {"bsbm.xml", "Bsbm", "Query_06", QueryVerificationType.BagEqual},
            new object[] {"bsbm.xml", "Bsbm", "Query_07", QueryVerificationType.BagEqual},
            new object[] {"bsbm.xml", "Bsbm", "Query_08", QueryVerificationType.Equal},
            new object[] {"bsbm.xml", "Bsbm", "Query_09", QueryVerificationType.None},
            new object[] {"bsbm.xml", "Bsbm", "Query_10", QueryVerificationType.Equal},
            new object[] {"bsbm.xml", "Bsbm", "Query_11", QueryVerificationType.BagEqual},
            new object[] {"bsbm.xml", "Bsbm", "Query_12", QueryVerificationType.None}
        };
    }

    public enum QueryVerificationType
    {
        BagEqual,
        Equal,
        None
    }
}
