﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Graph pattern
    /// </summary>
    public class GraphPattern
        : IGraphPattern
    {
        // TODO: Add missing parts

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphPattern"/> class.
        /// </summary>
        /// <param name="innerPattern">The inner pattern.</param>
        public GraphPattern(IGraphPattern innerPattern)
        {
            this.InnerPattern = innerPattern;
        }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables
        {
            get { return InnerPattern.Variables; }
        }

        /// <summary>
        /// Gets the inner pattern.
        /// </summary>
        /// <value>The inner pattern.</value>
        public IGraphPattern InnerPattern { get; private set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IPatternVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
