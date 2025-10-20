using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace paradise.Models
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
            : base("name=AppDbContext")
        {
        }

        public virtual DbSet<course_chapters> course_chapters { get; set; }
        public virtual DbSet<course_enrollments> course_enrollments { get; set; }
        public virtual DbSet<course_lessons> course_lessons { get; set; }
        public virtual DbSet<course_reviews> course_reviews { get; set; }
        public virtual DbSet<cours> courses { get; set; }
        public virtual DbSet<lesson_contents> lesson_contents { get; set; }
        public virtual DbSet<lesson_progress> lesson_progress { get; set; }
        public virtual DbSet<quiz> quizs { get; set; }
        public virtual DbSet<quiz_attempt_answers> quiz_attempt_answers { get; set; }
        public virtual DbSet<quiz_attempts> quiz_attempts { get; set; }
        public virtual DbSet<quiz_options> quiz_options { get; set; }
        public virtual DbSet<quiz_questions> quiz_questions { get; set; }
        public virtual DbSet<role> roles { get; set; }
        public virtual DbSet<topic_quiz> topic_quiz { get; set; }
        public virtual DbSet<topic> topics { get; set; }
        public virtual DbSet<user_profiles> user_profiles { get; set; }
        public virtual DbSet<user> users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<course_chapters>()
                .HasMany(e => e.course_lessons)
                .WithRequired(e => e.course_chapters)
                .HasForeignKey(e => e.chapter_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<course_lessons>()
                .HasMany(e => e.lesson_contents)
                .WithOptional(e => e.course_lessons)
                .HasForeignKey(e => e.lesson_id);

            modelBuilder.Entity<course_lessons>()
                .HasMany(e => e.lesson_progress)
                .WithRequired(e => e.course_lessons)
                .HasForeignKey(e => e.lesson_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<cours>()
                .Property(e => e.price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<cours>()
                .HasMany(e => e.course_chapters)
                .WithRequired(e => e.cours)
                .HasForeignKey(e => e.course_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<cours>()
                .HasMany(e => e.course_enrollments)
                .WithOptional(e => e.cours)
                .HasForeignKey(e => e.course_id);

            modelBuilder.Entity<cours>()
                .HasMany(e => e.course_reviews)
                .WithOptional(e => e.cours)
                .HasForeignKey(e => e.course_id);

            modelBuilder.Entity<quiz>()
                .Property(e => e.time)
                .HasPrecision(18, 0);

            modelBuilder.Entity<quiz>()
                .HasMany(e => e.quiz_attempts)
                .WithRequired(e => e.quiz)
                .HasForeignKey(e => e.quiz_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<quiz>()
                .HasMany(e => e.quiz_questions)
                .WithRequired(e => e.quiz)
                .HasForeignKey(e => e.quiz_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<quiz_attempts>()
                .HasMany(e => e.quiz_attempt_answers)
                .WithRequired(e => e.quiz_attempts)
                .HasForeignKey(e => e.attempt_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<quiz_options>()
                .HasMany(e => e.quiz_attempt_answers)
                .WithRequired(e => e.quiz_options)
                .HasForeignKey(e => e.selected_option_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<quiz_questions>()
                .HasMany(e => e.quiz_attempt_answers)
                .WithRequired(e => e.quiz_questions)
                .HasForeignKey(e => e.question_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<quiz_questions>()
                .HasMany(e => e.quiz_options)
                .WithRequired(e => e.quiz_questions)
                .HasForeignKey(e => e.question_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<role>()
                .HasMany(e => e.user_profiles)
                .WithRequired(e => e.role)
                .HasForeignKey(e => e.role_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<topic_quiz>()
                .HasMany(e => e.quizs)
                .WithRequired(e => e.topic_quiz)
                .HasForeignKey(e => e.topic)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<topic>()
                .HasMany(e => e.courses)
                .WithRequired(e => e.topic)
                .HasForeignKey(e => e.topics_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<user>()
                .HasMany(e => e.course_enrollments)
                .WithOptional(e => e.user)
                .HasForeignKey(e => e.user_id);

            modelBuilder.Entity<user>()
                .HasMany(e => e.course_reviews)
                .WithOptional(e => e.user)
                .HasForeignKey(e => e.user_id);

            modelBuilder.Entity<user>()
                .HasMany(e => e.courses)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.author_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<user>()
                .HasMany(e => e.lesson_progress)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.user_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<user>()
                .HasMany(e => e.quiz_attempts)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.user_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<user>()
                .HasOptional(e => e.user_profiles)
                .WithRequired(e => e.user);
        }
    }
}
