﻿#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL ILIKE expression.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class ILikeExpression : Expression, IEquatable<ILikeExpression>
    {
        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type => typeof(bool);

        /// <summary>
        /// The match expression.
        /// </summary>
        [NotNull]
        public virtual Expression Match { get; }

        /// <summary>
        /// The pattern to match.
        /// </summary>
        [NotNull]
        public virtual Expression Pattern { get; }

        /// <summary>
        /// The escape character to use in <see cref="Pattern"/>.
        /// </summary>
        [CanBeNull]
        public virtual Expression EscapeChar { get; }

        /// <summary>
        /// Constructs a <see cref="ILikeExpression"/>.
        /// </summary>
        /// <param name="match">The expression to match.</param>
        /// <param name="pattern">The pattern to match.</param>
        /// <exception cref="ArgumentNullException" />
        public ILikeExpression([NotNull] Expression match, [NotNull] Expression pattern)
        {
            Match = Check.NotNull(match, nameof(match));
            Pattern = Check.NotNull(pattern, nameof(pattern));
        }

        /// <summary>
        /// Constructs a <see cref="ILikeExpression"/>.
        /// </summary>
        /// <param name="match">The expression to match.</param>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="escapeChar">The escape character to use in <paramref name="pattern"/>.</param>
        /// <exception cref="ArgumentNullException" />
        public ILikeExpression([NotNull] Expression match, [NotNull] Expression pattern, [CanBeNull] Expression escapeChar)
        {
            Match = Check.NotNull(match, nameof(match));
            Pattern = Check.NotNull(pattern, nameof(pattern));
            EscapeChar = escapeChar;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitILike(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var match = visitor.Visit(Match) ?? Match;
            var pattern = visitor.Visit(Pattern) ?? Pattern;
            var escapeChar = visitor.Visit(EscapeChar);

            return
                match != Match || pattern != Pattern || escapeChar != EscapeChar
                    ? new ILikeExpression(match, pattern, escapeChar)
                    : this;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is ILikeExpression other && Equals(other);

        /// <inheritdoc />
        public bool Equals(ILikeExpression other)
            => other != null &&
               Equals(Match, other.Match) &&
               Equals(Pattern, other.Pattern) &&
               Equals(EscapeChar, other.EscapeChar);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Match.GetHashCode();
                hashCode = (hashCode * 397) ^ Pattern.GetHashCode();
                hashCode = (hashCode * 397) ^ (EscapeChar?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"{Match} ILIKE {Pattern}{(EscapeChar == null ? "" : $" ESCAPE {EscapeChar}")}";
    }
}
