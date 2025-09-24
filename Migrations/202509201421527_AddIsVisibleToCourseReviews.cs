namespace paradise.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddIsVisibleToCourseReviews : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.course_reviews", "is_visible", c => c.Boolean(nullable: false, defaultValue: true));
        }

        public override void Down()
        {
            DropColumn("dbo.course_reviews", "is_visible");
        }
    }
}
