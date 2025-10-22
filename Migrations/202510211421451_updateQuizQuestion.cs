namespace paradise.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateQuizQuestion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.quiz_questions", "question_type", c => c.String());
        }

        public override void Down()
        {
            AddColumn("dbo.quiz_options", "question_type", c => c.String());
        }
    }
}
