namespace paradise.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RebuildQuizSchema : DbMigration
    {
        public override void Up()
        {
            // ======= 1️⃣ DROP ALL FK LIÊN QUAN TỚI 4 BẢNG quiz_* =======
            Sql(@"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql = @sql + 'ALTER TABLE [' + OBJECT_SCHEMA_NAME(parent_object_id) 
                        + '].[' + OBJECT_NAME(parent_object_id) + '] DROP CONSTRAINT [' + name + '];' + CHAR(13)
                FROM sys.foreign_keys
                WHERE referenced_object_id IN (
                    OBJECT_ID('dbo.quiz_attempts'),
                    OBJECT_ID('dbo.quiz_questions'),
                    OBJECT_ID('dbo.quiz_attempt_answers'),
                    OBJECT_ID('dbo.quiz_options')
                )
                OR parent_object_id IN (
                    OBJECT_ID('dbo.quiz_attempts'),
                    OBJECT_ID('dbo.quiz_questions'),
                    OBJECT_ID('dbo.quiz_attempt_answers'),
                    OBJECT_ID('dbo.quiz_options')
                );
                EXEC sp_executesql @sql;
            ");

            // ======= 2️⃣ DROP CHECK CONSTRAINT (CK_attempts_time) =======
            Sql(@"
                IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_attempts_time')
                    ALTER TABLE [dbo].[quiz_attempts] DROP CONSTRAINT [CK_attempts_time];
            ");

            // ======= 3️⃣ DROP ALL PRIMARY KEYS TRONG 4 BẢNG =======
            Sql(@"
                DECLARE @sql2 NVARCHAR(MAX) = N'';
                SELECT @sql2 = @sql2 + 'ALTER TABLE [' + OBJECT_SCHEMA_NAME(parent_object_id) 
                        + '].[' + OBJECT_NAME(parent_object_id) + '] DROP CONSTRAINT [' + name + '];' + CHAR(13)
                FROM sys.key_constraints
                WHERE type = 'PK'
                AND OBJECT_NAME(parent_object_id) IN ('quiz_attempts', 'quiz_questions', 'quiz_attempt_answers', 'quiz_options');
                EXEC sp_executesql @sql2;
            ");

            // ======= 4️⃣ ALTER CÁC CỘT =======
            AlterColumn("dbo.quiz_attempts", "id", c => c.Long(nullable: false, identity: true));
            AlterColumn("dbo.quiz_attempts", "started_at", c => c.DateTime());
            AlterColumn("dbo.quiz_attempts", "finished_at", c => c.DateTime());
            AlterColumn("dbo.quiz_attempts", "score", c => c.Int());

            AlterColumn("dbo.quiz_questions", "id", c => c.Long(nullable: false, identity: true));
            AlterColumn("dbo.quiz_questions", "question_text", c => c.String(nullable: false, maxLength: 1000));
            AlterColumn("dbo.quiz_questions", "created_at", c => c.DateTime());

            AlterColumn("dbo.quiz_attempt_answers", "id", c => c.Long(nullable: false, identity: true));
            AlterColumn("dbo.quiz_attempt_answers", "is_correct", c => c.Boolean(nullable: false));

            AlterColumn("dbo.quiz_options", "id", c => c.Long(nullable: false, identity: true));
            AlterColumn("dbo.quiz_options", "option_text", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.quiz_options", "is_correct", c => c.Boolean(nullable: false));

            // ======= 5️⃣ ADD LẠI PRIMARY KEYS =======
            AddPrimaryKey("dbo.quiz_attempts", "id");
            AddPrimaryKey("dbo.quiz_questions", "id");
            AddPrimaryKey("dbo.quiz_attempt_answers", "id");
            AddPrimaryKey("dbo.quiz_options", "id");

            // ======= 6️⃣ ADD LẠI FOREIGN KEYS =======
            AddForeignKey("dbo.quiz_attempt_answers", "attempt_id", "dbo.quiz_attempts", "id", cascadeDelete: true);
            AddForeignKey("dbo.quiz_attempt_answers", "question_id", "dbo.quiz_questions", "id", cascadeDelete: true);
            AddForeignKey("dbo.quiz_options", "question_id", "dbo.quiz_questions", "id", cascadeDelete: true);
            AddForeignKey("dbo.quiz_attempt_answers", "selected_option_id", "dbo.quiz_options", "id", cascadeDelete: false);

            // ======= 7️⃣ ADD LẠI CHECK CONSTRAINT =======
            Sql(@"
                ALTER TABLE [dbo].[quiz_attempts]
                ADD CONSTRAINT [CK_attempts_time]
                CHECK ([finished_at] IS NULL OR [finished_at] >= [started_at]);
            ");
        }

        public override void Down()
        {
            // SAFE revert
            Sql(@"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql = @sql + 'ALTER TABLE [' + OBJECT_SCHEMA_NAME(parent_object_id) 
                        + '].[' + OBJECT_NAME(parent_object_id) + '] DROP CONSTRAINT [' + name + '];' + CHAR(13)
                FROM sys.foreign_keys
                WHERE referenced_object_id IN (
                    OBJECT_ID('dbo.quiz_attempts'),
                    OBJECT_ID('dbo.quiz_questions'),
                    OBJECT_ID('dbo.quiz_attempt_answers'),
                    OBJECT_ID('dbo.quiz_options')
                );
                EXEC sp_executesql @sql;
            ");

            Sql(@"
                IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_attempts_time')
                    ALTER TABLE [dbo].[quiz_attempts] DROP CONSTRAINT [CK_attempts_time];
            ");

            AlterColumn("dbo.quiz_options", "option_text", c => c.String(nullable: false, maxLength: 1));
            AlterColumn("dbo.quiz_options", "id", c => c.Long(nullable: false));
            AlterColumn("dbo.quiz_attempt_answers", "id", c => c.Long(nullable: false));
            AlterColumn("dbo.quiz_questions", "question_text", c => c.String(nullable: false, maxLength: 1));
            AlterColumn("dbo.quiz_questions", "id", c => c.Long(nullable: false));
            AlterColumn("dbo.quiz_attempts", "id", c => c.Long(nullable: false));

            AddPrimaryKey("dbo.quiz_options", "id");
            AddPrimaryKey("dbo.quiz_attempt_answers", "id");
            AddPrimaryKey("dbo.quiz_questions", "id");
            AddPrimaryKey("dbo.quiz_attempts", "id");
        }
    }
}
