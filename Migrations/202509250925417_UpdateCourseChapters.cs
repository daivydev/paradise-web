namespace paradise.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateCourseChapters : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.course_chapters", "chapter_description", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.course_chapters", "chapter_description", c => c.String(nullable: false));
        }
    }
}
