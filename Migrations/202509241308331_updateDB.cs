namespace paradise.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateDB : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.courses", "course_thumbnail", c => c.String(maxLength: 255, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.courses", "course_thumbnail", c => c.String(nullable: false, maxLength: 255, unicode: false));
        }
    }
}
