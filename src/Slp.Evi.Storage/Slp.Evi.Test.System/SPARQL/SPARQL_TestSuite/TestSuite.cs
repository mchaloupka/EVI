using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.Evi.Storage;

namespace Slp.Evi.Test.System.SPARQL.SPARQL_TestSuite
{
    public abstract class TestSuite
        : BaseSPARQLTestSuite
    {
        [TestMethod]
        public void Simple_single()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\single.rq";
            var resultFile = @"Data\Simple\single.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_join()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\join.rq";
            var resultFile = @"Data\Simple\join.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_union()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\union.rq";
            var resultFile = @"Data\Simple\union.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_empty()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\empty.rq";
            var resultFile = @"Data\Simple\empty.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_null()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\null.rq";
            var resultFile = @"Data\Simple\null.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_optional()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\optional.rq";
            var resultFile = @"Data\Simple\optional.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_bind()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\bind.rq";
            var resultFile = @"Data\Simple\bind.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_nested_optional()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\nested_optional.rq";
            var resultFile = @"Data\Simple\nested_optional.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_nested_filter()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\nested_filter.rq";
            var resultFile = @"Data\Simple\nested_filter.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_distinct()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\distinct.rq";
            var resultFile = @"Data\Simple\distinct.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Filter_bound()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Filter\bound.rq";
            var resultFile = @"Data\Simple\Filter\bound.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Filter_not_bound()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Filter\not_bound.rq";
            var resultFile = @"Data\Simple\Filter\not_bound.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Filter_comparison_gt()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Filter\comparison_gt.rq";
            var resultFile = @"Data\Simple\Filter\comparison_gt.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Filter_comparison_ge()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Filter\comparison_ge.rq";
            var resultFile = @"Data\Simple\Filter\comparison_ge.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Filter_comparison_lt()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Filter\comparison_lt.rq";
            var resultFile = @"Data\Simple\Filter\comparison_lt.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Filter_comparison_le()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Filter\comparison_le.rq";
            var resultFile = @"Data\Simple\Filter\comparison_le.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Filter_comparison_eq()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Filter\comparison_eq.rq";
            var resultFile = @"Data\Simple\Filter\comparison_eq.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Filter_comparison_neq()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Filter\comparison_neq.rq";
            var resultFile = @"Data\Simple\Filter\comparison_neq.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Filter_disjunction()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Filter\disjunction.rq";
            var resultFile = @"Data\Simple\Filter\disjunction.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Filter_conjunction()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Filter\conjunction.rq";
            var resultFile = @"Data\Simple\Filter\conjunction.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }


        [TestMethod]
        public void Simple_Type_int()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Type\int.rq";
            var resultFile = @"Data\Simple\Type\int.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Type_double()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Type\double.rq";
            var resultFile = @"Data\Simple\Type\double.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Type_type_equal()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Type\type_equal.rq";
            var resultFile = @"Data\Simple\Type\type_equal.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Type_type_comp_eq()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Type\type_comp_eq.rq";
            var resultFile = @"Data\Simple\Type\type_comp_eq.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Type_type_comp_eq2()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Type\type_comp_eq2.rq";
            var resultFile = @"Data\Simple\Type\type_comp_eq2.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Type_type_comp_gt()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Type\type_comp_gt.rq";
            var resultFile = @"Data\Simple\Type\type_comp_gt.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Simple_Type_type_join_different()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\Type\type_join_different.rq";
            var resultFile = @"Data\Simple\Type\type_join_different.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }


        [TestMethod]
        public void Students_no_result()
        {
            var storage = GetStorage("students.xml");
            var queryFile = @"Data\Students\no_result.rq";
            var resultFile = @"Data\Students\no_result.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void Students_student_names()
        {
            var storage = GetStorage("students.xml");
            var queryFile = @"Data\Students\student_names.rq";
            var resultFile = @"Data\Students\student_names.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }


        [TestMethod]
        public void Students_student_names_order()
        {
            var storage = GetStorage("students.xml");
            var queryFile = @"Data\Students\student_names_order.rq";
            var resultFile = @"Data\Students\student_names_order.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertEqual(expected, result);
        }

        [TestMethod]
        public void Students_student_names_order_desc()
        {
            var storage = GetStorage("students.xml");
            var queryFile = @"Data\Students\student_names_order_desc.rq";
            var resultFile = @"Data\Students\student_names_order_desc.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertEqual(expected, result);
        }

        [TestMethod]
        public void Students_student_names_order_limit()
        {
            var storage = GetStorage("students.xml");
            var queryFile = @"Data\Students\student_names_order_limit.rq";
            var resultFile = @"Data\Students\student_names_order_limit.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertEqual(expected, result);
        }

        [TestMethod]
        public void Students_student_names_order_offset()
        {
            var storage = GetStorage("students.xml");
            var queryFile = @"Data\Students\student_names_order_offset.rq";
            var resultFile = @"Data\Students\student_names_order_offset.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertEqual(expected, result);
        }

        [TestMethod]
        public void Students_student_names_order_offset_limit()
        {
            var storage = GetStorage("students.xml");
            var queryFile = @"Data\Students\student_names_order_offset_limit.rq";
            var resultFile = @"Data\Students\student_names_order_offset_limit.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertEqual(expected, result);
        }


        [TestMethod]
        public void Bsbm_ProductType_OrderBy()
        {
            var storage = GetStorage("bsbm.xml");
            var queryFile = @"Data\Bsbm\ProductType_OrderBy.rq";
            var resultFile = @"Data\Bsbm\ProductType_OrderBy.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertEqual(expected, result);
        }


        protected abstract EviQueryableStorage GetStorage(string storageName);

        private XDocument GetExpected(string resultFile)
        {
            var doc = XDocument.Load(GetPath(resultFile));
            return doc;
        }

        private static string GetQuery(string queryFile)
        {
            var query = string.Empty;

            using (var fsr = new FileStream(GetPath(queryFile), FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(fsr))
            {
                query = sr.ReadToEnd();
            }

            return query;
        }

        protected static readonly string[] StorageNames = { "simple.xml", "students.xml", "bsbm.xml" };
    }
}
