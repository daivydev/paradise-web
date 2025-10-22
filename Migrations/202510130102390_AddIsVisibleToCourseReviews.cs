namespace paradise.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsVisibleToCourseReviews : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.course_chapters", "chapter_title", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.courses", "course_title", c => c.String(nullable: false, maxLength: 255));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.courses", "course_title", c => c.String(nullable: false, maxLength: 255, unicode: false));
            AlterColumn("dbo.course_chapters", "chapter_title", c => c.String(nullable: false, maxLength: 255, unicode: false));
        }
    }
}
