using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace study4_be.Models
{
    public partial class STUDY4Context : DbContext
    {
        public STUDY4Context()
        {
        }

        public STUDY4Context(DbContextOptions<STUDY4Context> options)
            : base(options)
        {
        }

        public virtual DbSet<Audio> Audios { get; set; } = null!;
        public virtual DbSet<Container> Containers { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Lesson> Lessons { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<Question> Questions { get; set; } = null!;
        public virtual DbSet<Rating> Ratings { get; set; } = null!;
        public virtual DbSet<Tag> Tags { get; set; } = null!;
        public virtual DbSet<Translate> Translates { get; set; } = null!;
        public virtual DbSet<Unit> Units { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserCourse> UserCourses { get; set; } = null!;
        public virtual DbSet<Video> Videos { get; set; } = null!;
        public virtual DbSet<Vocabulary> Vocabularies { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=tcp:eu-az-sql-serv1.database.windows.net,1433;Initial Catalog=dhg7j87tg1pusg4;Persist Security Info=False;User ID=uwgdxq29hpow6g5;Password=IbDMPbfFDSqhd4kWlRk0W3cz*;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Audio>(entity =>
            {
                entity.ToTable("AUDIO");

                entity.Property(e => e.AudioId).HasColumnName("AUDIO_ID");

                entity.Property(e => e.AudioDescription)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("AUDIO_DESCRIPTION");

                entity.Property(e => e.AudioUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("AUDIO_URL");

                entity.Property(e => e.LessonId).HasColumnName("LESSON_ID");

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.Audios)
                    .HasForeignKey(d => d.LessonId)
                    .HasConstraintName("FK_AUDIO_LESSON");
            });

            modelBuilder.Entity<Container>(entity =>
            {
                entity.ToTable("CONTAINER");

                entity.Property(e => e.ContainerId).HasColumnName("CONTAINER_ID");

                entity.Property(e => e.ContainerTitle)
                    .HasMaxLength(100)
                    .HasColumnName("CONTAINER_TITLE");

                entity.Property(e => e.UnitId).HasColumnName("UNIT_ID");

                entity.HasOne(d => d.Unit)
                    .WithMany(p => p.Containers)
                    .HasForeignKey(d => d.UnitId)
                    .HasConstraintName("FK_CONTAINER_UNIT");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("COURSES");

                entity.Property(e => e.CourseId).HasColumnName("COURSE_ID");

                entity.Property(e => e.CourseDescription).HasColumnName("COURSE_DESCRIPTION");

                entity.Property(e => e.CourseImage)
                    .IsUnicode(false)
                    .HasColumnName("COURSE_IMAGE");

                entity.Property(e => e.CourseName)
                    .HasMaxLength(100)
                    .HasColumnName("COURSE_NAME");

                entity.Property(e => e.CoursePrice).HasColumnName("COURSE_PRICE");

                entity.Property(e => e.CourseSale).HasColumnName("COURSE_SALE");

                entity.Property(e => e.CourseTag)
                    .HasMaxLength(100)
                    .HasColumnName("COURSE_TAG");
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.ToTable("LESSON");

                entity.Property(e => e.LessonId).HasColumnName("LESSON_ID");

                entity.Property(e => e.ContainerId).HasColumnName("CONTAINER_ID");

                entity.Property(e => e.LessonTitle)
                    .HasMaxLength(200)
                    .HasColumnName("LESSON_TITLE");

                entity.Property(e => e.LessonType)
                    .HasMaxLength(200)
                    .HasColumnName("LESSON_TYPE");

                entity.Property(e => e.TagId)
                    .HasMaxLength(100)
                    .HasColumnName("TAG_ID");

                entity.HasOne(d => d.Container)
                    .WithMany(p => p.Lessons)
                    .HasForeignKey(d => d.ContainerId)
                    .HasConstraintName("FK_LESSON_CONTAINER");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.Lessons)
                    .HasForeignKey(d => d.TagId)
                    .HasConstraintName("FK_LESSON_TAG");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.OrderId).HasColumnName("Order_id");

                entity.Property(e => e.Address).HasMaxLength(250);

                entity.Property(e => e.CourseId).HasColumnName("Course_id");

                entity.Property(e => e.OrderDate)
                    .HasColumnType("datetime")
                    .HasColumnName("Order_date");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("Phone_Number");

                entity.Property(e => e.State).HasColumnName("STATE");

                entity.Property(e => e.TotalAmount).HasColumnName("Total_amount");

                entity.Property(e => e.UserId)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("User_id");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK_Orders_Courses");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("QUESTION");

                entity.Property(e => e.QuestionId).HasColumnName("QUESTION_ID");

                entity.Property(e => e.CorrectAnswer)
                    .HasMaxLength(100)
                    .HasColumnName("CORRECT_ANSWER");

                entity.Property(e => e.LessonId).HasColumnName("LESSON_ID");

                entity.Property(e => e.OptionA)
                    .HasMaxLength(200)
                    .HasColumnName("OPTION_A");

                entity.Property(e => e.OptionB)
                    .HasMaxLength(200)
                    .HasColumnName("OPTION_B");

                entity.Property(e => e.OptionC)
                    .HasMaxLength(200)
                    .HasColumnName("OPTION_C");

                entity.Property(e => e.OptionD)
                    .HasMaxLength(200)
                    .HasColumnName("OPTION_D");

                entity.Property(e => e.QuestionAudio).HasColumnName("QUESTION_AUDIO");

                entity.Property(e => e.QuestionImage).HasColumnName("QUESTION_IMAGE");

                entity.Property(e => e.QuestionParagraph).HasColumnName("QUESTION_PARAGRAPH");

                entity.Property(e => e.QuestionText).HasColumnName("QUESTION_TEXT");

                entity.Property(e => e.QuestionTranslate).HasColumnName("QUESTION_TRANSLATE");

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.LessonId)
                    .HasConstraintName("FK_QUESTION_LESSON");
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.ToTable("RATING");

                entity.Property(e => e.RatingId).HasColumnName("RATING_ID");

                entity.Property(e => e.CourseId).HasColumnName("COURSE_ID");

                entity.Property(e => e.RatingDate)
                    .HasColumnType("datetime")
                    .HasColumnName("RATING_DATE");

                entity.Property(e => e.RatingValue).HasColumnName("RATING_VALUE");

                entity.Property(e => e.Review).HasColumnName("REVIEW");

                entity.Property(e => e.UserId)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("USER_ID");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK_RATING_COURSES");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_RATING_USERS");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("TAG");

                entity.Property(e => e.TagId)
                    .HasMaxLength(100)
                    .HasColumnName("TAG_ID");
            });

            modelBuilder.Entity<Translate>(entity =>
            {
                entity.ToTable("Translate");

                entity.Property(e => e.TranslateId).HasColumnName("Translate_id");

                entity.Property(e => e.Answer).HasMaxLength(255);

                entity.Property(e => e.Hint).IsUnicode(false);

                entity.Property(e => e.LessonId).HasColumnName("LESSON_ID");

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.Translates)
                    .HasForeignKey(d => d.LessonId)
                    .HasConstraintName("FK_Translate_LESSON");
            });

            modelBuilder.Entity<Unit>(entity =>
            {
                entity.ToTable("UNIT");

                entity.Property(e => e.UnitId).HasColumnName("UNIT_ID");

                entity.Property(e => e.CourseId).HasColumnName("COURSE_ID");

                entity.Property(e => e.UnitTittle)
                    .HasMaxLength(255)
                    .HasColumnName("UNIT_TITTLE");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Units)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK_UNIT_COURSE");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");

                entity.Property(e => e.UserId)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("USER_ID");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("PHONE_NUMBER");

                entity.Property(e => e.UserBanner)
                    .IsUnicode(false)
                    .HasColumnName("USER_BANNER");

                entity.Property(e => e.UserDescription)
                    .HasMaxLength(250)
                    .HasColumnName("USER_DESCRIPTION");

                entity.Property(e => e.UserEmail)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("USER_EMAIL");

                entity.Property(e => e.UserImage)
                    .IsUnicode(false)
                    .HasColumnName("USER_IMAGE");

                entity.Property(e => e.UserName)
                    .HasMaxLength(100)
                    .HasColumnName("USER_NAME");

                entity.Property(e => e.UserPassword)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("USER_PASSWORD");
            });

            modelBuilder.Entity<UserCourse>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.CourseId });

                entity.ToTable("USER_COURSE");

                entity.Property(e => e.UserId)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("USER_ID");

                entity.Property(e => e.CourseId).HasColumnName("COURSE_ID");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("DATE");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.UserCourses)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_USER_COURSE_COURSE");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserCourses)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_USER_COURSE_USER");
            });

            modelBuilder.Entity<Video>(entity =>
            {
                entity.ToTable("VIDEO");

                entity.Property(e => e.VideoId).HasColumnName("VIDEO_ID");

                entity.Property(e => e.LessonId).HasColumnName("LESSON_ID");

                entity.Property(e => e.VideoUrl).HasColumnName("VIDEO_URL");

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.Videos)
                    .HasForeignKey(d => d.LessonId)
                    .HasConstraintName("FK_VIDEO_LESSON");
            });

            modelBuilder.Entity<Vocabulary>(entity =>
            {
                entity.HasKey(e => e.VocabId);

                entity.ToTable("VOCABULARY");

                entity.Property(e => e.VocabId).HasColumnName("VOCAB_ID");

                entity.Property(e => e.AudioUrlUk).HasColumnName("AUDIO_URL_UK");

                entity.Property(e => e.AudioUrlUs).HasColumnName("AUDIO_URL_US");

                entity.Property(e => e.Example)
                    .IsUnicode(false)
                    .HasColumnName("EXAMPLE");

                entity.Property(e => e.Explanation)
                    .IsUnicode(false)
                    .HasColumnName("EXPLANATION");

                entity.Property(e => e.LessonId).HasColumnName("LESSON_ID");

                entity.Property(e => e.Mean).HasColumnName("MEAN");

                entity.Property(e => e.VocabTitle)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("VOCAB_TITLE");

                entity.Property(e => e.VocabType)
                    .HasMaxLength(70)
                    .IsUnicode(false)
                    .HasColumnName("VOCAB_TYPE");

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.Vocabularies)
                    .HasForeignKey(d => d.LessonId)
                    .HasConstraintName("FK_VOCABULARY_LESSON");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
