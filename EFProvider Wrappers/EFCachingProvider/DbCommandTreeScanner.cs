// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Data.Common.CommandTrees;

namespace EFCachingProvider
{
    /// <summary>
    /// General purpose command tree visitor that does not return any value.
    /// </summary>
    internal abstract class DbCommandTreeScanner : DbExpressionVisitor
    {
        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbVariableReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbVariableReferenceExpression"/> that is visited.</param>
        public override void Visit(DbVariableReferenceExpression expression)
        {
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbUnionAllExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbUnionAllExpression"/> that is visited.</param>
        public override void Visit(DbUnionAllExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Left.Accept(this);
            expression.Right.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbTreatExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbTreatExpression"/> that is visited.</param>
        public override void Visit(DbTreatExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbSortExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbSortExpression"/> that is visited.</param>
        public override void Visit(DbSortExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Input.Expression.Accept(this);
            foreach (DbSortClause sortClause in expression.SortOrder)
            {
                sortClause.Expression.Accept(this);
            }
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbSkipExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbSkipExpression"/> that is visited.</param>
        public override void Visit(DbSkipExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Count.Accept(this);
            expression.Input.Expression.Accept(this);
            foreach (var sortClause in expression.SortOrder)
            {
                sortClause.Expression.Accept(this);
            }
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbScanExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbScanExpression"/> that is visited.</param>
        public override void Visit(DbScanExpression expression)
        {
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbRelationshipNavigationExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbRelationshipNavigationExpression"/> that is visited.</param>
        public override void Visit(DbRelationshipNavigationExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.NavigationSource.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbRefExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbRefExpression"/> that is visited.</param>
        public override void Visit(DbRefExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbQuantifierExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbQuantifierExpression"/> that is visited.</param>
        public override void Visit(DbQuantifierExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Input.Expression.Accept(this);
            expression.Predicate.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbPropertyExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbPropertyExpression"/> that is visited.</param>
        public override void Visit(DbPropertyExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Instance.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbProjectExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbProjectExpression"/> that is visited.</param>
        public override void Visit(DbProjectExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Projection.Accept(this);
            expression.Input.Expression.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbParameterReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbParameterReferenceExpression"/> that is visited.</param>
        public override void Visit(DbParameterReferenceExpression expression)
        {
            // expression.
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbOrExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbOrExpression"/> that is visited.</param>
        public override void Visit(DbOrExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Left.Accept(this);
            expression.Right.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbOfTypeExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbOfTypeExpression"/> that is visited.</param>
        public override void Visit(DbOfTypeExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbNullExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbNullExpression"/> that is visited.</param>
        public override void Visit(DbNullExpression expression)
        {
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbNotExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbNotExpression"/> that is visited.</param>
        public override void Visit(DbNotExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbNewInstanceExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbNewInstanceExpression"/> that is visited.</param>
        public override void Visit(DbNewInstanceExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            foreach (DbExpression e in expression.Arguments)
            {
                e.Accept(this);
            }
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbLimitExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbLimitExpression"/> that is visited.</param>
        public override void Visit(DbLimitExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
            expression.Limit.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbLikeExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbLikeExpression"/> that is visited.</param>
        public override void Visit(DbLikeExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
            expression.Pattern.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbJoinExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbJoinExpression"/> that is visited.</param>
        public override void Visit(DbJoinExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Left.Expression.Accept(this);
            expression.Right.Expression.Accept(this);
            expression.JoinCondition.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbIsOfExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbIsOfExpression"/> that is visited.</param>
        public override void Visit(DbIsOfExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbIsNullExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbIsNullExpression"/> that is visited.</param>
        public override void Visit(DbIsNullExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbIsEmptyExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbIsEmptyExpression"/> that is visited.</param>
        public override void Visit(DbIsEmptyExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbIntersectExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbIntersectExpression"/> that is visited.</param>
        public override void Visit(DbIntersectExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Left.Accept(this);
            expression.Right.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbGroupByExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbGroupByExpression"/> that is visited.</param>
        public override void Visit(DbGroupByExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Input.Expression.Accept(this);
            foreach (DbExpression ex in expression.Keys)
            {
                ex.Accept(this);
            }

            foreach (DbAggregate agg in expression.Aggregates)
            {
                foreach (DbExpression ex in agg.Arguments)
                {
                    ex.Accept(this);
                }
            }
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbRefKeyExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbRefKeyExpression"/> that is visited.</param>
        public override void Visit(DbRefKeyExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbEntityRefExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbEntityRefExpression"/> that is visited.</param>
        public override void Visit(DbEntityRefExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
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

            foreach (DbExpression ex in expression.Arguments)
            {
                ex.Accept(this);
            }
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbFilterExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbFilterExpression"/> that is visited.</param>
        public override void Visit(DbFilterExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Input.Expression.Accept(this);
            expression.Predicate.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbExceptExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbExceptExpression"/> that is visited.</param>
        public override void Visit(DbExceptExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Left.Accept(this);
            expression.Right.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbElementExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbElementExpression"/> that is visited.</param>
        public override void Visit(DbElementExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbDistinctExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbDistinctExpression"/> that is visited.</param>
        public override void Visit(DbDistinctExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbDerefExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbDerefExpression"/> that is visited.</param>
        public override void Visit(DbDerefExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbCrossJoinExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbCrossJoinExpression"/> that is visited.</param>
        public override void Visit(DbCrossJoinExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            foreach (DbExpressionBinding binding in expression.Inputs)
            {
                binding.Expression.Accept(this);
            }
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbConstantExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbConstantExpression"/> that is visited.</param>
        public override void Visit(DbConstantExpression expression)
        {
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbComparisonExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.Db ComparisonExpression"/> that is visited.</param>
        public override void Visit(DbComparisonExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Left.Accept(this);
            expression.Right.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbCastExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbCastExpression"/> that is visited.</param>
        public override void Visit(DbCastExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Argument.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbCaseExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbCaseExpression"/> that is visited.</param>
        public override void Visit(DbCaseExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            foreach (DbExpression e in expression.When)
            {
                e.Accept(this);
            }

            foreach (DbExpression e in expression.Then)
            {
                e.Accept(this);
            }

            expression.Else.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbArithmeticExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbArithmeticExpression"/> that is visited.</param>
        public override void Visit(DbArithmeticExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            foreach (DbExpression ex in expression.Arguments)
            {
                ex.Accept(this);
            }
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbApplyExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbApplyExpression"/> that is visited.</param>
        public override void Visit(DbApplyExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Apply.Expression.Accept(this);
            expression.Input.Expression.Accept(this);
        }

        /// <summary>
        /// Implements the visitor pattern for <see cref="T:System.Data.Common.CommandTrees.DbAndExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.Data.Common.CommandTrees.DbAndExpression"/> that is visited.</param>
        public override void Visit(DbAndExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            expression.Left.Accept(this);
            expression.Right.Accept(this);
        }

        /// <summary>
        /// Handles any expression of an unrecognized type.
        /// </summary>
        /// <param name="expression">The expression to be handled.</param>
        public override void Visit(DbExpression expression)
        {
            throw new NotSupportedException();
        }
    }
}

