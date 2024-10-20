using System;
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

    public virtual DbSet<AggregatedCounter> AggregatedCounters { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Container> Containers { get; set; }

    public virtual DbSet<Counter> Counters { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<Hash> Hashes { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobParameter> JobParameters { get; set; }

    public virtual DbSet<JobQueue> JobQueues { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<List> Lists { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<PlanCourse> PlanCourses { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<RatingImage> RatingImages { get; set; }

    public virtual DbSet<RatingReply> RatingReplies { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schema> Schemas { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<Set> Sets { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<Subscriptionplan> Subscriptionplans { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    public virtual DbSet<UserCourse> UserCourses { get; set; }

    public virtual DbSet<UserDocument> UserDocuments { get; set; }

    public virtual DbSet<UserSub> UserSubs { get; set; }

    public virtual DbSet<UsersExam> UsersExams { get; set; }

    public virtual DbSet<Video> Videos { get; set; }

    public virtual DbSet<Vocabulary> Vocabularies { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AggregatedCounter>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PK_HangFire_CounterAggregated");

            entity.ToTable("AggregatedCounter", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_AggregatedCounter_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(255);
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

        modelBuilder.Entity<Counter>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Id }).HasName("PK_HangFire_Counter");

            entity.ToTable("Counter", "HangFire");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
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
            entity.HasKey(e => e.DocumentId).HasName("PK__Document__1ABEEF0F95D42938");

            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.DownloadCount).HasDefaultValue(0);
            entity.Property(e => e.FileType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FileUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.IsPublic).HasDefaultValue(true);
            entity.Property(e => e.PreviewUrl).IsUnicode(false);
            entity.Property(e => e.ThumbnailUrl).IsUnicode(false);
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
            entity.HasKey(e => e.ExamId).HasName("PK__Exam__C782CA592E035805");

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

        modelBuilder.Entity<Hash>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Field }).HasName("PK_HangFire_Hash");

            entity.ToTable("Hash", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Hash_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Field).HasMaxLength(100);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_HangFire_Job");

            entity.ToTable("Job", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Job_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.HasIndex(e => e.StateName, "IX_HangFire_Job_StateName").HasFilter("([StateName] IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            entity.Property(e => e.StateName).HasMaxLength(20);
        });

        modelBuilder.Entity<JobParameter>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.Name }).HasName("PK_HangFire_JobParameter");

            entity.ToTable("JobParameter", "HangFire");

            entity.Property(e => e.Name).HasMaxLength(40);

            entity.HasOne(d => d.Job).WithMany(p => p.JobParameters)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_HangFire_JobParameter_Job");
        });

        modelBuilder.Entity<JobQueue>(entity =>
        {
            entity.HasKey(e => new { e.Queue, e.Id }).HasName("PK_HangFire_JobQueue");

            entity.ToTable("JobQueue", "HangFire");

            entity.Property(e => e.Queue).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FetchedAt).HasColumnType("datetime");
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

        modelBuilder.Entity<List>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Id }).HasName("PK_HangFire_List");

            entity.ToTable("List", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_List_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__F1FF8453306BB4C9");

            entity.Property(e => e.OrderId)
                .HasMaxLength(255)
                .HasColumnName("Order_id");
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.Code)
                .IsUnicode(false)
                .HasColumnName("CODE");
            entity.Property(e => e.CourseId).HasColumnName("Course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DocumentId).HasColumnName("Document_Id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("EMAIL");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("Order_date");
            entity.Property(e => e.PlanId).HasColumnName("Plan_id");
            entity.Property(e => e.State).HasColumnName("STATE");
            entity.Property(e => e.TotalAmount).HasColumnName("Total_amount");
            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("User_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Orders_Courses");

            entity.HasOne(d => d.Document).WithMany(p => p.Orders)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("FK_Orders_Documents");

            entity.HasOne(d => d.Plan).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("fk_orders_subscriptionplan");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Orders_USERS");
        });

        modelBuilder.Entity<PlanCourse>(entity =>
        {
            entity.HasKey(e => new { e.PlanId, e.CourseId }).HasName("PK__tmp_ms_x__47E48A7C9A7DDED1");

            entity.ToTable("PLAN_COURSES");

            entity.Property(e => e.PlanId).HasColumnName("PLAN_ID");
            entity.Property(e => e.CourseId).HasColumnName("COURSE_ID");
            entity.Property(e => e.Isactive).HasColumnName("ISACTIVE");

            entity.HasOne(d => d.Course).WithMany(p => p.PlanCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PLAN_COURSES_COURSES");

            entity.HasOne(d => d.Plan).WithMany(p => p.PlanCourses)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PLAN_COURSES_SUBSCRIPTIONPLAN");
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
            entity.HasKey(e => e.Id).HasName("PK__RATING__3214EC27075FC169");

            entity.ToTable("RATING");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.EntityType)
                .HasMaxLength(10)
                .HasColumnName("ENTITY_TYPE");
            entity.Property(e => e.RatingDate)
                .HasColumnType("datetime")
                .HasColumnName("RATING_DATE");
            entity.Property(e => e.RatingValue).HasColumnName("RATING_VALUE");
            entity.Property(e => e.Review).HasColumnName("REVIEW");
            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("USER_ID");

            entity.HasOne(d => d.Course).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Rating_Course");

            entity.HasOne(d => d.Document).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Rating_Document");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RATING__USER_ID__15A53433");
        });

        modelBuilder.Entity<RatingImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__RATING_I__7EA98689A61FCD9C");

            entity.ToTable("RATING_IMAGES");

            entity.Property(e => e.ImageId).HasColumnName("IMAGE_ID");
            entity.Property(e => e.ImageUrl).HasColumnName("IMAGE_URL");
            entity.Property(e => e.ReferenceId).HasColumnName("REFERENCE_ID");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(10)
                .HasColumnName("REFERENCE_TYPE");
            entity.Property(e => e.ReplyId).HasColumnName("REPLY_ID");

            entity.HasOne(d => d.Reference).WithMany(p => p.RatingImages)
                .HasForeignKey(d => d.ReferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RATING_IMAGES_RATING");

            entity.HasOne(d => d.Reply).WithMany(p => p.RatingImages)
                .HasForeignKey(d => d.ReplyId)
                .HasConstraintName("FK_RATING_IMAGES_REPLY");
        });

        modelBuilder.Entity<RatingReply>(entity =>
        {
            entity.HasKey(e => e.ReplyId).HasName("PK__RATING_R__C48F2A201B1790B4");

            entity.ToTable("RATING_REPLY");

            entity.Property(e => e.ReplyId).HasColumnName("REPLY_ID");
            entity.Property(e => e.ParentReplyId).HasColumnName("PARENT_REPLY_ID");
            entity.Property(e => e.RatingId).HasColumnName("RATING_ID");
            entity.Property(e => e.ReplyContent).HasColumnName("REPLY_CONTENT");
            entity.Property(e => e.ReplyDate)
                .HasColumnType("datetime")
                .HasColumnName("REPLY_DATE");
            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("USER_ID");

            entity.HasOne(d => d.ParentReply).WithMany(p => p.InverseParentReply)
                .HasForeignKey(d => d.ParentReplyId)
                .HasConstraintName("FK_REPLY_PARENT");

            entity.HasOne(d => d.Rating).WithMany(p => p.RatingReplies)
                .HasForeignKey(d => d.RatingId)
                .HasConstraintName("FK_RATING_REPLY_RATING");

            entity.HasOne(d => d.User).WithMany(p => p.RatingReplies)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RATING_RE__USER___1A69E950");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("ROLE");

            entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
            entity.Property(e => e.RoleName)
                .HasMaxLength(60)
                .HasColumnName("ROLE_NAME");
        });

        modelBuilder.Entity<Schema>(entity =>
        {
            entity.HasKey(e => e.Version).HasName("PK_HangFire_Schema");

            entity.ToTable("Schema", "HangFire");

            entity.Property(e => e.Version).ValueGeneratedNever();
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_HangFire_Server");

            entity.ToTable("Server", "HangFire");

            entity.HasIndex(e => e.LastHeartbeat, "IX_HangFire_Server_LastHeartbeat");

            entity.Property(e => e.Id).HasMaxLength(200);
            entity.Property(e => e.LastHeartbeat).HasColumnType("datetime");
        });

        modelBuilder.Entity<Set>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Value }).HasName("PK_HangFire_Set");

            entity.ToTable("Set", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Set_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.HasIndex(e => new { e.Key, e.Score }, "IX_HangFire_Set_Score");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(256);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffCmnd);

            entity.ToTable("STAFF");

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
            entity.Property(e => e.StaffPassword)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("STAFF_PASSWORD");
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

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.Id }).HasName("PK_HangFire_State");

            entity.ToTable("State", "HangFire");

            entity.HasIndex(e => e.CreatedAt, "IX_HangFire_State_CreatedAt");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.Reason).HasMaxLength(100);

            entity.HasOne(d => d.Job).WithMany(p => p.States)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_HangFire_State_Job");
        });

        modelBuilder.Entity<Subscriptionplan>(entity =>
        {
            entity.HasKey(e => e.PlanId);

            entity.ToTable("SUBSCRIPTIONPLAN");

            entity.Property(e => e.PlanId).HasColumnName("PLAN_ID");
            entity.Property(e => e.PlanDescription).HasColumnName("PLAN_DESCRIPTION");
            entity.Property(e => e.PlanDuration).HasColumnName("PLAN_DURATION");
            entity.Property(e => e.PlanName).HasColumnName("PLAN_NAME");
            entity.Property(e => e.PlanPrice).HasColumnName("PLAN_PRICE");
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
            entity.HasKey(e => e.UserAnswerId).HasName("PK__UserAnsw__47CE237F4287ED31");

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

        modelBuilder.Entity<UserDocument>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.DocumentId });

            entity.ToTable("USER_DOCUMENTS");

            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("USER_ID");
            entity.Property(e => e.DocumentId).HasColumnName("DOCUMENT_ID");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("ORDER_DATE");
            entity.Property(e => e.State).HasColumnName("STATE");

            entity.HasOne(d => d.Document).WithMany(p => p.UserDocuments)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_DOCUMENTS_DOCUMENT");

            entity.HasOne(d => d.User).WithMany(p => p.UserDocuments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_DOCUMENTS_USER");
        });

        modelBuilder.Entity<UserSub>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PlanId });

            entity.ToTable("USER_SUBS");

            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("USER_ID");
            entity.Property(e => e.PlanId).HasColumnName("PLAN_ID");
            entity.Property(e => e.State).HasColumnName("STATE");
            entity.Property(e => e.UsersubsEnddate)
                .HasColumnType("datetime")
                .HasColumnName("USERSUBS_ENDDATE");
            entity.Property(e => e.UsersubsStartdate)
                .HasColumnType("datetime")
                .HasColumnName("USERSUBS_STARTDATE");

            entity.HasOne(d => d.Plan).WithMany(p => p.UserSubs)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USER_SUBS__PLAN___23F3538A");

            entity.HasOne(d => d.User).WithMany(p => p.UserSubs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USER_SUBS__USER___24E777C3");
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
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VOCABULARY_LESSON");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
