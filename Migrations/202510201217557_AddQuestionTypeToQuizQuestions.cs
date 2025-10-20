namespace paradise.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddQuestionTypeToQuizQuestions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.quiz_questions", "question_type", c => c.String());
            DropColumn("dbo.quiz_options", "question_type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.quiz_options", "question_type", c => c.String());
            DropColumn("dbo.quiz_questions", "question_type");
        }
    }
}
