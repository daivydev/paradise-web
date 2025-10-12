namespace paradise.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class NvarcharForTitleChapters : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.course_chapters", "chapter_title", c => c.String());
            AlterColumn("dbo.courses", "course_title", c => c.String());


        }

        public override void Down()
        {
            AlterColumn("dbo.course_chapters", "chapter_title", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.courses", "course_title", c => c.String(nullable: false, maxLength: 255));

        }
    }
}
