using System;

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
        public static readonly Uri XsdInteger = new Uri($"{BaseXsdNamespace}integer");

        /// <summary>
        /// The XSD boolean type
        /// </summary>
        public static readonly Uri XsdBoolean = new Uri($"{BaseXsdNamespace}boolean");

        /// <summary>
        /// The XSD decimal type
        /// </summary>
        public static readonly Uri XsdDecimal = new Uri($"{BaseXsdNamespace}decimal");

        /// <summary>
        /// The XSD double type
        /// </summary>
        public static readonly Uri XsdDouble = new Uri($"{BaseXsdNamespace}double");

        /// <summary>
        /// The XSD date type
        /// </summary>
        public static readonly Uri XsdDate = new Uri($"{BaseXsdNamespace}date");

        /// <summary>
        /// The XSD time type
        /// </summary>
        public static readonly Uri XsdTime = new Uri($"{BaseXsdNamespace}time");

        /// <summary>
        /// The XSD date time type
        /// </summary>
        public static readonly Uri XsdDateTime = new Uri($"{BaseXsdNamespace}dateTime");

        /// <summary>
        /// The XSD hexadecimal binary type
        /// </summary>
        public static readonly Uri XsdHexBinary = new Uri($"{BaseXsdNamespace}hexBinary");
    }
}
