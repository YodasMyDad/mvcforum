// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Linq;
using EFProviderWrapperToolkit;

namespace EFCachingProvider
{
    /// <summary>
    /// Represents a command definitio
    /// </summary>
    public class EFCachingCommandDefinition : DbCommandDefinitionWrapper
    {
        private List<EntitySetBase> affectedEntitySets = new List<EntitySetBase>();
        private List<EdmFunction> functionsUsed = new List<EdmFunction>();

        /// <summary>
        /// Initializes static members of the EFCachingCommandDefinition class.
        /// </summary>
        static EFCachingCommandDefinition()
        {
            NonCacheableFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Edm.CurrentDateTime",
                "Edm.CurrentUtcDateTime",
                "Edm.CurrentDateTimeOffsets",
                "Edm.NewGuid",

                "SqlServer.NEWID",
                "SqlServer.GETDATE",
                "SqlServer.GETUTCDATE",
                "SqlServer.SYSDATETIME",
                "SqlServer.SYSUTCDATETIME",
                "SqlServer.SYSDATETIMEOFFSET",
                "SqlServer.CURRENT_USER",
                "SqlServer.CURRENT_TIMESTAMP",
                "SqlServer.HOST_NAME",
                "SqlServer.USER_NAME",
            };
        }

        internal EFCachingCommandDefinition(DbCommandDefinition wrappedCommandDefinition, DbCommandTree commandTree)
            : base(wrappedCommandDefinition, commandTree, (cmd, def) => new EFCachingCommand(cmd, def))
        {
            this.GetAffectedEntitySets(commandTree);
        }

        /// <summary>
        /// Gets the list of non-cacheable functions (by default includes canonical and SqlServer functions).
        /// </summary>
        /// <value>The non-cacheable functions.</value>
        public static ICollection<string> NonCacheableFunctions { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a modification command (INSERT, UPDATE or DELETE).
        /// </summary>
        /// <value>
        /// Returns <c>true</c> if this instance is modification command (INSERT, UPDATE, DELETE); otherwise, <c>false</c>.
        /// </value>
        public bool IsModification { get; set; }

        /// <summary>
        /// Gets the list of entity sets affected by this command.
        /// </summary>
        /// <value>The affected entity sets.</value>
        public IList<EntitySetBase> AffectedEntitySets
        {
            get { return this.affectedEntitySets; }
        }

        /// <summary>
        /// Creates and returns a <see cref="T:System.Data.Common.DbCommandDefinition"/> object associated with the current connection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Common.DbCommandDefinition"/>.
        /// </returns>
        public override DbCommand CreateCommand()
        {
            return new EFCachingCommand(WrappedCommandDefinition.CreateCommand(), this);
        }

        /// <summary>
        /// Determines whether this command definition is cacheable.
        /// </summary>
        /// <returns>
        /// A value of <c>true</c> if this command definition is cacheable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCacheable()
        {
            if (!(this.CommandTree is DbQueryCommandTree))
            {
                // we can only cache queries
                return false;
            }

            if (this.functionsUsed.Any(f => IsNonDeterministicFunction(f)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified function is non-deterministic.
        /// </summary>
        /// <param name="function">The function object.</param>
        /// <returns>
        /// A value of <c>true</c> if the function is non-deterministic; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsNonDeterministicFunction(EdmFunction function)
        {
            return NonCacheableFunctions.Contains(function.NamespaceName + "." + function.Name);
        }

        private void GetAffectedEntitySets(DbCommandTree commandTree)
        {
            FindAffectedEntitySetsVisitor visitor = new FindAffectedEntitySetsVisitor(this.affectedEntitySets, this.functionsUsed);
            DbQueryCommandTree queryTree = commandTree as DbQueryCommandTree;
            if (queryTree != null)
            {
                queryTree.Query.Accept(visitor);
                return;
            }

            DbUpdateCommandTree updateTree = commandTree as DbUpdateCommandTree;
            if (updateTree != null)
            {
                this.IsModification = true;
                updateTree.Target.Expression.Accept(visitor);
                updateTree.Predicate.Accept(visitor);
                if (updateTree.Returning != null)
                {
                    updateTree.Returning.Accept(visitor);
                }

                return;
            }

            DbInsertCommandTree insertTree = commandTree as DbInsertCommandTree;
            if (insertTree != null)
            {
                this.IsModification = true;
                insertTree.Target.Expression.Accept(visitor);
                if (insertTree.Returning != null)
                {
                    insertTree.Returning.Accept(visitor);
                }

                return;
            }

            DbDeleteCommandTree deleteTree = commandTree as DbDeleteCommandTree;
            if (deleteTree != null)
            {
                this.IsModification = true;
                deleteTree.Target.Expression.Accept(visitor);
                if (deleteTree.Predicate != null)
                {
                    deleteTree.Predicate.Accept(visitor);
                }

                return;
            }

            throw new NotSupportedException("Command tree type " + commandTree.GetType() + " is not supported.");
        }

        /// <summary>
        /// Scans the command tree for occurences of entity sets and functions.
        /// </summary>
        private class FindAffectedEntitySetsVisitor : DbCommandTreeScanner
        {
            private ICollection<EntitySetBase> affectedEntitySets;
            private ICollection<EdmFunction> functionsUsed;

            /// <summary>
            /// Initializes a new instance of the FindAffectedEntitySetsVisitor class.
            /// </summary>
            /// <param name="affectedEntitySets">The affected entity sets.</param>
            /// <param name="functionsUsed">The functions used.</param>
            public FindAffectedEntitySetsVisitor(ICollection<EntitySetBase> affectedEntitySets, ICollection<EdmFunction> functionsUsed)
            {
                this.affectedEntitySets = affectedEntitySets;
                this.functionsUsed = functionsUsed;
            }

            /// <summary>
            /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbScanExpression"/>.
            /// </summary>
            /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbScanExpression"/> that is visited.</param>
            public override void Visit(DbScanExpression expression)
            {
                if (expression == null)
                {
                    throw new ArgumentNullException("expression");
                }

                base.Visit(expression);
                this.affectedEntitySets.Add(expression.Target);
            }

            /// <summary>
            /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbFunctionExpression"/>.
            /// </summary>
            /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbFunctionExpression"/> that is visited.</param>
            public override void Visit(DbFunctionExpression expression)
            {
                if (expression == null)
                {
                    throw new ArgumentNullException("expression");
                }

                base.Visit(expression);
                this.functionsUsed.Add(expression.Function);
            }
        }
    }
}
