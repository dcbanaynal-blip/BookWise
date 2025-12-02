using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWise.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountHierarchySafeguards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Accounts', 'SegmentCode') IS NULL
                BEGIN
                    ALTER TABLE [Accounts] ADD [SegmentCode] nvarchar(50) NOT NULL CONSTRAINT [DF_Accounts_SegmentCode] DEFAULT('');
                    UPDATE [Accounts] SET [SegmentCode] = ISNULL([ExternalAccountNumber], [SegmentCode]);
                    ALTER TABLE [Accounts] DROP CONSTRAINT [DF_Accounts_SegmentCode];
                END
                IF COL_LENGTH('Accounts', 'SegmentCode') IS NOT NULL
                BEGIN
                    ALTER TABLE [Accounts] ALTER COLUMN [SegmentCode] nvarchar(50) NOT NULL;
                    UPDATE [Accounts]
                    SET [SegmentCode] = [ExternalAccountNumber]
                    WHERE [SegmentCode] IS NULL OR LTRIM(RTRIM([SegmentCode])) = '';
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Accounts', 'Level') IS NULL
                BEGIN
                    ALTER TABLE [Accounts] ADD [Level] int NOT NULL CONSTRAINT [DF_Accounts_Level] DEFAULT(1);
                    UPDATE a
                    SET [Level] = CASE WHEN a.[ParentAccountId] IS NULL THEN 1 ELSE ISNULL(p.[Level] + 1, 2) END
                    FROM [Accounts] a
                    LEFT JOIN [Accounts] p ON p.[AccountId] = a.[ParentAccountId];
                    ALTER TABLE [Accounts] DROP CONSTRAINT [DF_Accounts_Level];
                END
                IF COL_LENGTH('Accounts', 'Level') IS NOT NULL
                BEGIN
                    UPDATE a
                    SET [Level] = CASE WHEN a.[ParentAccountId] IS NULL THEN 1 ELSE ISNULL(p.[Level] + 1, 2) END
                    FROM [Accounts] a
                    LEFT JOIN [Accounts] p ON p.[AccountId] = a.[ParentAccountId];
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[CK_Accounts_LevelPositive]', N'C') IS NULL
                    ALTER TABLE [Accounts] ADD CONSTRAINT [CK_Accounts_LevelPositive] CHECK ([Level] >= 1);
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_RootSegmentCode')
                    DROP INDEX [IX_Accounts_RootSegmentCode] ON [Accounts];
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_RootSegmentCode')
                    CREATE UNIQUE INDEX [IX_Accounts_RootSegmentCode]
                    ON [Accounts]([SegmentCode])
                    WHERE [ParentAccountId] IS NULL;
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_ParentSegmentCode')
                    DROP INDEX [IX_Accounts_ParentSegmentCode] ON [Accounts];
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_ParentSegmentCode')
                    CREATE UNIQUE INDEX [IX_Accounts_ParentSegmentCode]
                    ON [Accounts]([ParentAccountId], [SegmentCode])
                    WHERE [ParentAccountId] IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[TR_Accounts_ValidateHierarchy]', N'TR') IS NOT NULL
                    DROP TRIGGER [dbo].[TR_Accounts_ValidateHierarchy];
                EXEC(N'
                CREATE TRIGGER [dbo].[TR_Accounts_ValidateHierarchy]
                ON [dbo].[Accounts]
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted i
                        LEFT JOIN Accounts p ON i.ParentAccountId = p.AccountId
                        WHERE (i.ParentAccountId IS NULL AND i.Level <> 1)
                           OR (i.ParentAccountId IS NOT NULL AND (p.AccountId IS NULL OR i.Level <> p.Level + 1))
                           OR (i.ParentAccountId = i.AccountId)
                    )
                    BEGIN
                        THROW 51000, ''Invalid account hierarchy. Ensure parents exist and levels increment by one.'', 1;
                    END;
                END');
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[TR_Accounts_PreventExternalUpdates]', N'TR') IS NOT NULL
                    DROP TRIGGER [dbo].[TR_Accounts_PreventExternalUpdates];
                EXEC(N'
                CREATE TRIGGER [dbo].[TR_Accounts_PreventExternalUpdates]
                ON [dbo].[Accounts]
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF UPDATE(ExternalAccountNumber)
                    BEGIN
                        IF EXISTS (
                            SELECT 1
                            FROM inserted i
                            INNER JOIN deleted d ON i.AccountId = d.AccountId
                            WHERE i.ExternalAccountNumber <> d.ExternalAccountNumber
                        )
                        BEGIN
                            THROW 51001, ''External account numbers are immutable and cannot be updated.'', 1;
                        END;
                    END;
                END');
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[TR_Entries_BlockNonLeafAccounts]', N'TR') IS NOT NULL
                    DROP TRIGGER [dbo].[TR_Entries_BlockNonLeafAccounts];
                EXEC(N'
                CREATE TRIGGER [dbo].[TR_Entries_BlockNonLeafAccounts]
                ON [dbo].[Entries]
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted i
                        WHERE EXISTS (
                            SELECT 1
                            FROM Accounts c
                            WHERE c.ParentAccountId = i.AccountId
                        )
                    )
                    BEGIN
                        THROW 51002, ''Entries can only reference leaf accounts that have no children.'', 1;
                    END;
                END');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[TR_Accounts_ValidateHierarchy]', N'TR') IS NOT NULL
                    DROP TRIGGER [dbo].[TR_Accounts_ValidateHierarchy];
                IF OBJECT_ID(N'[dbo].[TR_Accounts_PreventExternalUpdates]', N'TR') IS NOT NULL
                    DROP TRIGGER [dbo].[TR_Accounts_PreventExternalUpdates];
                IF OBJECT_ID(N'[dbo].[TR_Entries_BlockNonLeafAccounts]', N'TR') IS NOT NULL
                    DROP TRIGGER [dbo].[TR_Entries_BlockNonLeafAccounts];
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[CK_Accounts_LevelPositive]', N'C') IS NOT NULL
                    ALTER TABLE [Accounts] DROP CONSTRAINT [CK_Accounts_LevelPositive];
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_ParentSegmentCode')
                    DROP INDEX [IX_Accounts_ParentSegmentCode] ON [Accounts];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_RootSegmentCode')
                    DROP INDEX [IX_Accounts_RootSegmentCode] ON [Accounts];
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Accounts', 'Level') IS NOT NULL
                    ALTER TABLE [Accounts] DROP COLUMN [Level];
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Accounts', 'SegmentCode') IS NOT NULL
                    ALTER TABLE [Accounts] DROP COLUMN [SegmentCode];
                """);
        }
    }
}
