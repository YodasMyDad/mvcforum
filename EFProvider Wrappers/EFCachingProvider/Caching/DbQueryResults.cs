// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCachingProvider.Caching
{
    /// <summary>
    /// Represents cached query results.
    /// </summary>
    [Serializable]
    public class DbQueryResults
    {
        /// <summary>
        /// Initializes a new instance of the DbQueryResults class.
        /// </summary>
        public DbQueryResults()
        {
            this.Rows = new List<object[]>();
            this.ColumnNames = new List<string>();
        }

        /// <summary>
        /// Gets the collection of column names.
        /// </summary>
        public IList<string> ColumnNames { get; private set; }

        /// <summary>
        /// Gets the collection of rows.
        /// </summary>
        /// <value>The collection of rows.</value>
        public IList<object[]> Rows { get; private set; }
    }
}
