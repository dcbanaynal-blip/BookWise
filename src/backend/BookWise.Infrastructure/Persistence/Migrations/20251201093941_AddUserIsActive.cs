using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWise.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Users', 'IsActive') IS NULL
                BEGIN
                    ALTER TABLE [Users] ADD [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit);
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Users', 'UpdatedAt') IS NULL
                BEGIN
                    ALTER TABLE [Users] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME();
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Users', 'UpdatedBy') IS NULL
                BEGIN
                    ALTER TABLE [Users] ADD [UpdatedBy] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Users', 'IsActive') IS NOT NULL
                BEGIN
                    ALTER TABLE [Users] DROP COLUMN [IsActive];
                END
                """);

            migrationBuilder.Sql(
                """
                DECLARE @df nvarchar(128);
                SELECT @df = df.name
                FROM sys.default_constraints df
                INNER JOIN sys.columns c ON c.object_id = df.parent_object_id AND c.column_id = df.parent_column_id
                WHERE df.parent_object_id = OBJECT_ID('Users') AND c.name = 'UpdatedAt';
                IF @df IS NOT NULL EXEC('ALTER TABLE [Users] DROP CONSTRAINT ' + QUOTENAME(@df));
                IF COL_LENGTH('Users', 'UpdatedAt') IS NOT NULL
                BEGIN
                    ALTER TABLE [Users] DROP COLUMN [UpdatedAt];
                END
                """);

            migrationBuilder.Sql(
                """
                DECLARE @df nvarchar(128);
                SELECT @df = df.name
                FROM sys.default_constraints df
                INNER JOIN sys.columns c ON c.object_id = df.parent_object_id AND c.column_id = df.parent_column_id
                WHERE df.parent_object_id = OBJECT_ID('Users') AND c.name = 'UpdatedBy';
                IF @df IS NOT NULL EXEC('ALTER TABLE [Users] DROP CONSTRAINT ' + QUOTENAME(@df));
                IF COL_LENGTH('Users', 'UpdatedBy') IS NOT NULL
                BEGIN
                    ALTER TABLE [Users] DROP COLUMN [UpdatedBy];
                END
                """);
        }
    }
}
