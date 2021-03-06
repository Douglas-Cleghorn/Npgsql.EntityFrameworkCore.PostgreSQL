﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public partial class SimpleQueryNpgsqlTest
    {
        public override async Task String_Contains_Literal(bool isAsync)
        {
            await base.String_Contains_Literal(isAsync);
            AssertContainsSqlFragment("WHERE STRPOS(c.\"ContactName\", 'M') > 0");
        }

        public override async Task String_StartsWith_Literal(bool isAsync)
        {
            await base.String_StartsWith_Literal(isAsync);
            AssertContainsSqlFragment("WHERE c.\"ContactName\" LIKE 'M%'");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task String_StartsWith_Literal_with_escaping(bool isAsync)
        {
            await AssertQuery<Customer>(isAsync, cs => cs.Where(c => c.ContactName.StartsWith(@"_a%b\c")));
            AssertContainsSqlFragment(@"WHERE c.""ContactName"" LIKE '\_a\%b\\c%'");
        }

        public override async Task String_StartsWith_Column(bool isAsync)
        {
            await AssertQuery<Customer>(isAsync, cs => cs.Where(c => c.ContactName.StartsWith(c.City)));
            AssertContainsSqlFragment(@"WHERE c.""ContactName"" LIKE (c.""City"" || '%') AND (LEFT(c.""ContactName"", LENGTH(c.""City"")) = c.""City"")");
        }

        public override async Task String_EndsWith_Literal(bool isAsync)
        {
            await base.String_EndsWith_Literal(isAsync);
            AssertContainsSqlFragment("WHERE RIGHT(c.\"ContactName\", LENGTH('b')) = 'b'");
        }

        public override async Task Trim_without_argument_in_predicate(bool isAsync)
        {
            await base.Trim_without_argument_in_predicate(isAsync);
            AssertContainsSqlFragment(@"WHERE REGEXP_REPLACE(c.""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");
        }

        public override async Task Trim_with_char_argument_in_predicate(bool isAsync)
        {
            await base.Trim_with_char_argument_in_predicate(isAsync);
            AssertContainsSqlFragment("WHERE BTRIM(c.\"ContactTitle\", 'O')");
        }

        public override async Task Trim_with_char_array_argument_in_predicate(bool isAsync)
        {
            await base.Trim_with_char_array_argument_in_predicate(isAsync);
            AssertContainsSqlFragment("WHERE BTRIM(c.\"ContactTitle\", 'Or')");
        }

        public override async Task TrimStart_without_arguments_in_predicate(bool isAsync)
        {
            await base.TrimStart_without_arguments_in_predicate(isAsync);
            AssertContainsSqlFragment("WHERE REGEXP_REPLACE(c.\"ContactTitle\", '^\\s*', '') = 'Owner'");
        }

        public override async Task TrimStart_with_char_argument_in_predicate(bool isAsync)
        {
            await base.TrimStart_with_char_argument_in_predicate(isAsync);
            AssertContainsSqlFragment("WHERE LTRIM(c.\"ContactTitle\", 'O')");
        }

        public override async Task TrimStart_with_char_array_argument_in_predicate(bool isAsync)
        {
            await base.TrimStart_with_char_array_argument_in_predicate(isAsync);
            AssertContainsSqlFragment("WHERE LTRIM(c.\"ContactTitle\", 'Ow')");
        }

        public override async Task TrimEnd_without_arguments_in_predicate(bool isAsync)
        {
            await base.TrimEnd_without_arguments_in_predicate(isAsync);
            AssertContainsSqlFragment("WHERE REGEXP_REPLACE(c.\"ContactTitle\", '\\s*$', '') = 'Owner'");
        }

        public override async Task TrimEnd_with_char_argument_in_predicate(bool isAsync)
        {
            await base.TrimEnd_with_char_argument_in_predicate(isAsync);
            AssertContainsSqlFragment("WHERE RTRIM(c.\"ContactTitle\", 'r')");
        }

        public override async Task TrimEnd_with_char_array_argument_in_predicate(bool isAsync)
        {
            await base.TrimEnd_with_char_array_argument_in_predicate(isAsync);
            AssertContainsSqlFragment("WHERE RTRIM(c.\"ContactTitle\", 'er')");
        }

        public override async Task IsNullOrWhiteSpace_in_predicate(bool isAsync)
        {
            await base.IsNullOrWhiteSpace_in_predicate(isAsync);
            AssertContainsSqlFragment("WHERE c.\"Region\" IS NULL OR (c.\"Region\" ~ '^\\s*$' = TRUE)");
        }

        public override async Task Query_expression_with_to_string_and_contains(bool isAsync)
        {
            await base.Query_expression_with_to_string_and_contains(isAsync);
            AssertContainsSqlFragment("STRPOS(CAST(o.\"EmployeeID\" AS text), '10') > 0");
        }

        #region Regex

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatch(bool isAsync)
        {
            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A")),
                entryCount: 4);
            AssertContainsSqlFragment("WHERE c.\"CompanyName\" ~ ('(?p)' || '^A')");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatchOptionsNone(bool isAsync)
        {
            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.None)),
                entryCount: 4);
            AssertContainsSqlFragment("WHERE c.\"CompanyName\" ~ ('(?p)' || '^A')");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatchOptionsIgnoreCase(bool isAsync)
        {
            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^a", RegexOptions.IgnoreCase)),
                entryCount: 4);
            AssertContainsSqlFragment("WHERE c.\"CompanyName\" ~ ('(?ip)' || '^a')");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatchOptionsMultiline(bool isAsync)
        {
            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.Multiline)),
                entryCount: 4);
            AssertContainsSqlFragment("WHERE c.\"CompanyName\" ~ ('(?n)' || '^A')");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatchOptionsSingleline(bool isAsync)
        {
            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.Singleline)),
                entryCount: 4);
            AssertContainsSqlFragment("WHERE c.\"CompanyName\" ~ '^A'");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatchOptionsIgnorePatternWhitespace(bool isAsync)
        {
            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^ A", RegexOptions.IgnorePatternWhitespace)),
                entryCount: 4);
            AssertContainsSqlFragment("WHERE c.\"CompanyName\" ~ ('(?px)' || '^ A')");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatchOptionsUnsupported(bool isAsync)
        {
            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.RightToLeft)),
                entryCount: 4);
            Assert.DoesNotContain("WHERE c.\"CompanyName\" ~ ", Fixture.TestSqlLoggerFactory.Sql);
        }

        #endregion Regex

        void AssertContainsSqlFragment(string expectedFragment)
            => Assert.True(Fixture.TestSqlLoggerFactory.SqlStatements.Any(s => s.Contains(expectedFragment)));
    }
}
