﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace study4_be.Models;

public partial class Study4Context : DbContext
{
    public Study4Context()
    {
    }

    public Study4Context(DbContextOptions<Study4Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Container> Containers { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    public virtual DbSet<UserCourse> UserCourses { get; set; }

    public virtual DbSet<UsersExam> UsersExams { get; set; }

    public virtual DbSet<Video> Videos { get; set; }

    public virtual DbSet<Vocabulary> Vocabularies { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-62MKG1UJ;Initial Catalog=STUDY4;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Container>(entity =>
        {
            entity.ToTable("CONTAINER");

            entity.Property(e => e.ContainerId).HasColumnName("CONTAINER_ID");
            entity.Property(e => e.ContainerTitle)
                .HasMaxLength(100)
                .HasColumnName("CONTAINER_TITLE");
            entity.Property(e => e.UnitId).HasColumnName("UNIT_ID");

            entity.HasOne(d => d.Unit).WithMany(p => p.Containers)
                .HasForeignKey(d => d.UnitId)
                .HasConstraintName("FK_CONTAINER_UNIT");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK_COURSE");

            entity.ToTable("COURSES");

            entity.Property(e => e.CourseId).HasColumnName("COURSE_ID");
            entity.Property(e => e.CourseDescription).HasColumnName("COURSE_DESCRIPTION");
            entity.Property(e => e.CourseImage)
                .IsUnicode(false)
                .HasColumnName("COURSE_IMAGE");
            entity.Property(e => e.CourseName).HasColumnName("COURSE_NAME");
            entity.Property(e => e.CoursePrice).HasColumnName("COURSE_PRICE");
            entity.Property(e => e.CourseSale).HasColumnName("COURSE_SALE");
            entity.Property(e => e.CourseTag)
                .HasMaxLength(100)
                .HasColumnName("COURSE_TAG");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK_DEPARTMENT_ID");

            entity.ToTable("DEPARTMENT");

            entity.Property(e => e.DepartmentId).HasColumnName("DEPARTMENT_ID");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .HasColumnName("DEPARTMENT_NAME");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Document__1ABEEF0FAA9EFC00");

            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.DownloadCount).HasDefaultValue(0);
            entity.Property(e => e.FileType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FileUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.IsPublic).HasDefaultValue(true);
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UploadDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("USER_ID");

            entity.HasOne(d => d.Category).WithMany(p => p.Documents)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Documents_Category");

            entity.HasOne(d => d.Course).WithMany(p => p.Documents)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Documents_Courses");

            entity.HasOne(d => d.User).WithMany(p => p.Documents)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Documents_Users");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.ExamId).HasName("PK__Exam__C782CA59B52A7679");

            entity.ToTable("Exam");

            entity.Property(e => e.ExamId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Exam_Id");
            entity.Property(e => e.ExamAudio)
                .HasMaxLength(255)
                .HasColumnName("Exam_Audio");
            entity.Property(e => e.ExamImage)
                .HasMaxLength(255)
                .HasColumnName("Exam_Image");
            entity.Property(e => e.ExamName)
                .HasMaxLength(100)
                .HasColumnName("Exam_Name");
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

            entity.HasOne(d => d.Container).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.ContainerId)
                .HasConstraintName("FK_LESSON_CONTAINER");

            entity.HasOne(d => d.Tag).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.TagId)
                .HasConstraintName("FK_LESSON_TAG");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__F1FF84531EED0047");

            entity.Property(e => e.OrderId)
                .HasMaxLength(255)
                .HasColumnName("Order_id");
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.Code)
                .IsUnicode(false)
                .HasColumnName("CODE");
            entity.Property(e => e.CourseId).HasColumnName("Course_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("EMAIL");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("Order_date");
            entity.Property(e => e.State).HasColumnName("STATE");
            entity.Property(e => e.TotalAmount).HasColumnName("Total_amount");
            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("User_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Orders_Courses");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Orders_USERS");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("QUESTION");

            entity.Property(e => e.QuestionId).HasColumnName("QUESTION_ID");
            entity.Property(e => e.CorrectAnswer)
                .HasMaxLength(100)
                .HasColumnName("CORRECT_ANSWER");
            entity.Property(e => e.ExamId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Exam_Id");
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
            entity.Property(e => e.OptionMeanA)
                .HasMaxLength(200)
                .HasColumnName("OPTION_MEAN_A");
            entity.Property(e => e.OptionMeanB)
                .HasMaxLength(200)
                .HasColumnName("OPTION_MEAN_B");
            entity.Property(e => e.OptionMeanC)
                .HasMaxLength(200)
                .HasColumnName("OPTION_MEAN_C");
            entity.Property(e => e.OptionMeanD)
                .HasMaxLength(200)
                .HasColumnName("OPTION_MEAN_D");
            entity.Property(e => e.QuestionAudio).HasColumnName("QUESTION_AUDIO");
            entity.Property(e => e.QuestionImage).HasColumnName("QUESTION_IMAGE");
            entity.Property(e => e.QuestionParagraph).HasColumnName("QUESTION_PARAGRAPH");
            entity.Property(e => e.QuestionParagraphMean).HasColumnName("QUESTION_PARAGRAPH_MEAN");
            entity.Property(e => e.QuestionTag)
                .HasMaxLength(100)
                .HasColumnName("QUESTION_TAG");
            entity.Property(e => e.QuestionText).HasColumnName("QUESTION_TEXT");
            entity.Property(e => e.QuestionTextMean).HasColumnName("QUESTION_TEXT_MEAN");
            entity.Property(e => e.QuestionTranslate).HasColumnName("QUESTION_TRANSLATE");

            entity.HasOne(d => d.Exam).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK_QUESTION_EXAM");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Questions)
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

            entity.HasOne(d => d.UserCourse).WithMany(p => p.Ratings)
                .HasForeignKey(d => new { d.UserId, d.CourseId })
                .HasConstraintName("FK_RATING_RATING1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("ROLE");

            entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
            entity.Property(e => e.RoleName)
                .HasMaxLength(60)
                .HasColumnName("ROLE_NAME");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => new { e.StaffId, e.StaffCmnd });

            entity.ToTable("STAFF");

            entity.Property(e => e.StaffId)
                .ValueGeneratedOnAdd()
                .HasColumnName("STAFF_ID");
            entity.Property(e => e.StaffCmnd)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("STAFF_CMND");
            entity.Property(e => e.DepartmentId).HasColumnName("DEPARTMENT_ID");
            entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
            entity.Property(e => e.StaffEmail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("STAFF_EMAIL");
            entity.Property(e => e.StaffName)
                .HasMaxLength(100)
                .HasColumnName("STAFF_NAME");
            entity.Property(e => e.StaffType)
                .HasMaxLength(100)
                .HasColumnName("STAFF_TYPE");

            entity.HasOne(d => d.Department).WithMany(p => p.Staff)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_STAFF_DEPARTMENT");

            entity.HasOne(d => d.Role).WithMany(p => p.Staff)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_STAFF_ROLE");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("TAG");

            entity.Property(e => e.TagId)
                .HasMaxLength(100)
                .HasColumnName("TAG_ID");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.ToTable("UNIT");

            entity.Property(e => e.UnitId).HasColumnName("UNIT_ID");
            entity.Property(e => e.CourseId).HasColumnName("COURSE_ID");
            entity.Property(e => e.Process).HasColumnName("PROCESS");
            entity.Property(e => e.UnitTittle)
                .HasMaxLength(255)
                .HasColumnName("UNIT_TITTLE");

            entity.HasOne(d => d.Course).WithMany(p => p.Units)
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
            entity.Property(e => e.Isverified).HasColumnName("ISVERIFIED");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
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

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_USERS_ROLE");
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.UserAnswerId).HasName("PK__UserAnsw__47CE237F8919256B");

            entity.Property(e => e.QuestionId).HasColumnName("QUESTION_ID");
            entity.Property(e => e.UserExamId)
                .HasMaxLength(100)
                .HasColumnName("UserExam_Id");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAnswers_QUESTION");

            entity.HasOne(d => d.UserExam).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.UserExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAnswers_USERS_EXAM");
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
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Process).HasColumnName("PROCESS");

            entity.HasOne(d => d.Course).WithMany(p => p.UserCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_COURSE_COURSES");

            entity.HasOne(d => d.User).WithMany(p => p.UserCourses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_COURSE_USERS");
        });

        modelBuilder.Entity<UsersExam>(entity =>
        {
            entity.HasKey(e => e.UserExamId);

            entity.ToTable("USERS_EXAM");

            entity.Property(e => e.UserExamId)
                .HasMaxLength(100)
                .HasColumnName("UserExam_Id");
            entity.Property(e => e.DateTime)
                .HasColumnType("datetime")
                .HasColumnName("Date_Time");
            entity.Property(e => e.ExamId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Exam_Id");
            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("User_Id");
            entity.Property(e => e.UserTime).HasColumnName("userTime");

            entity.HasOne(d => d.Exam).WithMany(p => p.UsersExams)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USERS_EXAM_Exam");

            entity.HasOne(d => d.User).WithMany(p => p.UsersExams)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USERS_EXAM_USERS");
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.ToTable("VIDEO");

            entity.Property(e => e.VideoId).HasColumnName("VIDEO_ID");
            entity.Property(e => e.LessonId).HasColumnName("LESSON_ID");
            entity.Property(e => e.VideoUrl).HasColumnName("VIDEO_URL");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Videos)
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
            entity.Property(e => e.Example).HasColumnName("EXAMPLE");
            entity.Property(e => e.Explanation).HasColumnName("EXPLANATION");
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

            entity.HasOne(d => d.Lesson).WithMany(p => p.Vocabularies)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("FK_VOCABULARY_LESSON");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
