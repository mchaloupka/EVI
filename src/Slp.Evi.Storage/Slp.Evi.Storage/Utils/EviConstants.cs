using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.Evi.Storage.Utils
{
    /// <summary>
    /// Constant used in this project
    /// </summary>
    public static class EviConstants
    {
        /// <summary>
        /// The base xsd: namespace
        /// </summary>
        public const string BaseXsdNamespace = "http://www.w3.org/2001/XMLSchema#";

        /// <summary>
        /// The XSD integer type
        /// </summary>
        public static readonly Uri XsdInteger = new Uri(string.Format("{0}integer", BaseXsdNamespace));

        /// <summary>
        /// The XSD boolean type
        /// </summary>
        public static readonly Uri XsdBoolean = new Uri(string.Format("{0}boolean", BaseXsdNamespace));

        /// <summary>
        /// The XSD decimal type
        /// </summary>
        public static readonly Uri XsdDecimal = new Uri(string.Format("{0}decimal", BaseXsdNamespace));

        /// <summary>
        /// The XSD double type
        /// </summary>
        public static readonly Uri XsdDouble = new Uri(string.Format("{0}double", BaseXsdNamespace));

        /// <summary>
        /// The XSD date type
        /// </summary>
        public static readonly Uri XsdDate = new Uri(string.Format("{0}date", BaseXsdNamespace));

        /// <summary>
        /// The XSD time type
        /// </summary>
        public static readonly Uri XsdTime = new Uri(string.Format("{0}time", BaseXsdNamespace));

        /// <summary>
        /// The XSD date time type
        /// </summary>
        public static readonly Uri XsdDateTime = new Uri(string.Format("{0}dateTime", BaseXsdNamespace));

        /// <summary>
        /// The XSD hexadecimal binary type
        /// </summary>
        public static readonly Uri XsdHexBinary = new Uri(string.Format("{0}hexBinary", BaseXsdNamespace));
    }
}
