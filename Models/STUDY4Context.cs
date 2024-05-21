﻿using System;
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
        public virtual DbSet<Quiz> Quizzes { get; set; } = null!;
        public virtual DbSet<Rating> Ratings { get; set; } = null!;
        public virtual DbSet<Translate> Translates { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Vocabulary> Vocabularies { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=LAPTOP-62MKG1UJ;Initial Catalog=STUDY4;Integrated Security=True;Trust Server Certificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Audio>(entity =>
            {
                entity.ToTable("AUDIO");

                entity.Property(e => e.AudioId)
                    .ValueGeneratedNever()
                    .HasColumnName("AUDIO_ID");

                entity.Property(e => e.AudioDescription)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("AUDIO_DESCRIPTION");

                entity.Property(e => e.AudioUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("AUDIO_URL");

                entity.Property(e => e.ContainerId).HasColumnName("Container_id");

                entity.HasOne(d => d.Container)
                    .WithMany(p => p.Audios)
                    .HasForeignKey(d => d.ContainerId)
                    .HasConstraintName("FK_AUDIO_Container");
            });

            modelBuilder.Entity<Container>(entity =>
            {
                entity.ToTable("Container");

                entity.Property(e => e.ContainerId)
                    .ValueGeneratedNever()
                    .HasColumnName("Container_id");

                entity.Property(e => e.CoursesId).HasColumnName("COURSES_ID");

                entity.HasOne(d => d.Courses)
                    .WithMany(p => p.Containers)
                    .HasForeignKey(d => d.CoursesId)
                    .HasConstraintName("FK_Container_COURSE");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CoursesId)
                    .HasName("PK_COURSE");

                entity.ToTable("COURSES");

                entity.Property(e => e.CoursesId)
                    .ValueGeneratedNever()
                    .HasColumnName("COURSES_ID");

                entity.Property(e => e.CourseDescription)
                    .HasMaxLength(255)
                    .HasColumnName("COURSE_DESCRIPTION");

                entity.Property(e => e.CourseImage)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("COURSE_IMAGE");

                entity.Property(e => e.CourseName)
                    .HasMaxLength(100)
                    .HasColumnName("COURSE_NAME");

                entity.Property(e => e.CoursePrice).HasColumnName("COURSE_PRICE");

                entity.Property(e => e.CourseTag)
                    .HasMaxLength(100)
                    .HasColumnName("COURSE_TAG");
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasKey(e => e.LessonsId);

                entity.ToTable("LESSONS");

                entity.Property(e => e.LessonsId)
                    .ValueGeneratedNever()
                    .HasColumnName("LESSONS_ID");

                entity.Property(e => e.Content)
                    .HasMaxLength(100)
                    .HasColumnName("CONTENT");

                entity.Property(e => e.CoursesId).HasColumnName("COURSES_ID");

                entity.Property(e => e.LessonsTitle)
                    .HasMaxLength(200)
                    .HasColumnName("LESSONS_TITLE");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.OrderId)
                    .ValueGeneratedNever()
                    .HasColumnName("Order_id");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CourseId).HasColumnName("Course_id");

                entity.Property(e => e.OrderDate)
                    .HasColumnType("date")
                    .HasColumnName("Order_date");

                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(10, 3)")
                    .HasColumnName("Total_amount");

                entity.Property(e => e.UsersId)
                    .HasMaxLength(70)
                    .IsUnicode(false)
                    .HasColumnName("Users_id");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK_Orders_Courses");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("QUESTION");

                entity.Property(e => e.QuestionId)
                    .ValueGeneratedNever()
                    .HasColumnName("QUESTION_ID");

                entity.Property(e => e.CorrectAnswer)
                    .HasMaxLength(100)
                    .HasColumnName("CORRECT_ANSWER");

                entity.Property(e => e.IdQuizzes).HasColumnName("ID_QUIZZES");

                entity.Property(e => e.OptionA)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("OPTION_A");

                entity.Property(e => e.OptionB)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("OPTION_B");

                entity.Property(e => e.OptionC)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("OPTION_C");

                entity.Property(e => e.OptionD)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("OPTION_D");

                entity.Property(e => e.QuestionText)
                    .HasMaxLength(100)
                    .HasColumnName("QUESTION_TEXT");

                entity.HasOne(d => d.IdQuizzesNavigation)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.IdQuizzes)
                    .HasConstraintName("FK_QUESTION_QUIZZES");
            });

            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(e => e.QuizzesId);

                entity.ToTable("QUIZZES");

                entity.Property(e => e.QuizzesId)
                    .ValueGeneratedNever()
                    .HasColumnName("QUIZZES_ID");

                entity.Property(e => e.ContainerId).HasColumnName("Container_id");

                entity.Property(e => e.CreatedTime)
                    .HasColumnType("datetime")
                    .HasColumnName("CREATED_TIME");

                entity.Property(e => e.DescriptionQuizzes)
                    .HasMaxLength(100)
                    .HasColumnName("DESCRIPTION_QUIZZES");

                entity.Property(e => e.Title)
                    .HasMaxLength(70)
                    .HasColumnName("TITLE");

                entity.HasOne(d => d.Container)
                    .WithMany(p => p.Quizzes)
                    .HasForeignKey(d => d.ContainerId)
                    .HasConstraintName("FK_QUIZZES_CONTAINER");
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.ToTable("RATING");

                entity.Property(e => e.RatingId)
                    .ValueGeneratedNever()
                    .HasColumnName("RATING_ID");

                entity.Property(e => e.CoursesId).HasColumnName("COURSES_ID");

                entity.Property(e => e.RatingDate)
                    .HasColumnType("datetime")
                    .HasColumnName("RATING_DATE");

                entity.Property(e => e.RatingValue).HasColumnName("RATING_VALUE");

                entity.Property(e => e.Review)
                    .HasMaxLength(200)
                    .HasColumnName("REVIEW");

                entity.Property(e => e.UsersId)
                    .HasMaxLength(70)
                    .IsUnicode(false)
                    .HasColumnName("USERS_ID");

                entity.HasOne(d => d.Courses)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(d => d.CoursesId)
                    .HasConstraintName("FK_RATING_COURSES");

                entity.HasOne(d => d.Users)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(d => d.UsersId)
                    .HasConstraintName("FK_RATING_USERS");
            });

            modelBuilder.Entity<Translate>(entity =>
            {
                entity.ToTable("Translate");

                entity.Property(e => e.TranslateId)
                    .ValueGeneratedNever()
                    .HasColumnName("Translate_id");

                entity.Property(e => e.Answer).HasMaxLength(255);

                entity.Property(e => e.ContainerId).HasColumnName("Container_id");

                entity.Property(e => e.Hint)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Text).HasMaxLength(255);

                entity.HasOne(d => d.Container)
                    .WithMany(p => p.Translates)
                    .HasForeignKey(d => d.ContainerId)
                    .HasConstraintName("FK_Translate_Container");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UsersId);

                entity.ToTable("USERS");

                entity.Property(e => e.UsersId)
                    .HasMaxLength(70)
                    .IsUnicode(false)
                    .HasColumnName("USERS_ID");

                entity.Property(e => e.UsersBanner)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("USERS_BANNER");

                entity.Property(e => e.UsersDescription)
                    .HasMaxLength(100)
                    .HasColumnName("USERS_DESCRIPTION");

                entity.Property(e => e.UsersEmail)
                    .HasMaxLength(70)
                    .IsUnicode(false)
                    .HasColumnName("USERS_EMAIL");

                entity.Property(e => e.UsersImage)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("USERS_IMAGE");

                entity.Property(e => e.UsersName)
                    .HasMaxLength(70)
                    .HasColumnName("USERS_NAME");

                entity.Property(e => e.UsersPassword)
                    .HasMaxLength(70)
                    .IsUnicode(false)
                    .HasColumnName("USERS_PASSWORD");
            });

            modelBuilder.Entity<Vocabulary>(entity =>
            {
                entity.HasKey(e => e.VocabId);

                entity.ToTable("VOCABULARY");

                entity.Property(e => e.VocabId)
                    .ValueGeneratedNever()
                    .HasColumnName("VOCAB_ID");

                entity.Property(e => e.AudioUrlUk)
                    .HasMaxLength(100)
                    .HasColumnName("AUDIO_URL_UK");

                entity.Property(e => e.AudioUrlUs)
                    .HasMaxLength(100)
                    .HasColumnName("AUDIO_URL_US");

                entity.Property(e => e.ContainerId).HasColumnName("Container_ID");

                entity.Property(e => e.Example)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("EXAMPLE");

                entity.Property(e => e.Explanation)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("EXPLANATION");

                entity.Property(e => e.Mean)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("MEAN");

                entity.Property(e => e.VocabType)
                    .HasMaxLength(70)
                    .IsUnicode(false)
                    .HasColumnName("VOCAB_TYPE");

                entity.HasOne(d => d.Container)
                    .WithMany(p => p.Vocabularies)
                    .HasForeignKey(d => d.ContainerId)
                    .HasConstraintName("FK_VOCABULARY_Container");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
