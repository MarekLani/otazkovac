using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Security;
using WebMatrix.WebData;

namespace CQA.Models
{
    public class CQADBContext : DbContext
    {

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UploadedImage> UploadedImages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Evaluation> Ratings { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Subject> Subjects{ get; set; }
        public DbSet<Setup> Setups { get; set; }
        public DbSet<UsersSetup> UsersSetups { get; set; }
        public DbSet<UsersAction> UsersActions { get; set; }

        // <summary>
        /// The below Method is used to define the Maping
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {


            modelBuilder.Entity<UploadedImage>()
                .HasRequired(i => i.Owner)
                .WithMany(u => u.Images)
                .HasForeignKey(u => u.UserId);

            modelBuilder.Entity<Comment>()
                .HasRequired(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(u => u.UserId)
                 .WillCascadeOnDelete(false);

            modelBuilder.Entity<Evaluation>()
              .HasKey(e => new { e.UserId, e.AnswerId});

            modelBuilder.Entity<UserProfile>()
                        .HasMany(u => u.Evaluations)
                        .WithRequired(e => e.Author)
                        .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<Answer>()
                        .HasMany(a => a.Evaluations)
                        .WithRequired(e => e.Answer)
                        .HasForeignKey(e => e.AnswerId);

            modelBuilder.Entity<Answer>()
                .HasRequired(c => c.Author)
                .WithMany(u => u.Answers)
                .HasForeignKey(u => u.UserId)
                 .WillCascadeOnDelete(false);

            modelBuilder.Entity<UsersSetup>()
              .HasKey(us => new { us.UserId, us.SetupId });

            modelBuilder.Entity<UserProfile>()
                        .HasMany(u => u.UsersSetups)
                        .WithRequired(us => us.User)
                        .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<Setup>()
                        .HasMany(s => s.UsersSetups)
                        .WithRequired(us => us.Setup)
                        .HasForeignKey(us => us.SetupId);

            //Creating many to many relationship where duplicity in foreign keys is allowed
            modelBuilder.Entity<UserProfile>()
                        .HasMany(u => u.UsersActions)
                        .WithRequired(usa => usa.User)
                        .HasForeignKey(usa => usa.UserId);

            modelBuilder.Entity<Question>()
                        .HasMany(s => s.UsersActions)
                        .WithOptional(ua => ua.Question)
                        .HasForeignKey(usa => usa.QuestionId);

            modelBuilder.Entity<Answer>()
                       .HasMany(a => a.UsersActions)
                       .WithOptional(ua => ua.Answer)
                       .HasForeignKey(ua => ua.AnswerId); 


            base.OnModelCreating(modelBuilder);
        }

        //public override int SaveChanges()
        //{
        //    foreach (var item in ChangeTracker.Entries<IModel>())
        //        item.Entity.Modified = DateTime.Now;

        //    return base.SaveChanges();
        //}

        //:IDatabaseInitializer
        public class Initializer : IDatabaseInitializer<CQADBContext>
        {
            public void InitializeDatabase(CQADBContext context)
            {
                try
                {
                    if (!context.Database.Exists())
                    {
                        // Create the SimpleMembership database without Entity Framework migration schema
                        ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                    }
                   
                    //Initialize in AuthConfig
                    WebSecurity.InitializeDatabaseConnection("CQADBContext", "UserProfile", "UserId", "UserName", autoCreateTables: true);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
                }

                #region Roles and user init
                //Roles Initialization
                var roles = (SimpleRoleProvider)Roles.Provider;

                if (!roles.RoleExists("Admin"))
                {
                    roles.CreateRole("Admin");
                }
                if (!roles.RoleExists("Editor"))
                {
                    roles.CreateRole("Editor");
                }
                if (!roles.RoleExists("User"))
                {
                    roles.CreateRole("User");
                }
                if (!roles.RoleExists("Trainer"))
                {
                    roles.CreateRole("Trainer");
                }

                //if (!WebSecurity.UserExists("mareklani@gmail.com"))
                //    WebSecurity.CreateUserAndAccount("mareklani@gmail.com", "aaaaaa", new { RealName = "Marek", UrlName = "Marek_1254" });

                //if (!roles.GetRolesForUser("Marek").Contains("Admin"))
                //{
                //    roles.AddUsersToRoles(new[] { "Marek" }, new[] { "Admin" });
                //}
                //if (!roles.GetRolesForUser("Trainer").Contains("Trainer"))
                //{
                //    roles.AddUsersToRoles(new[] { "Trainer" }, new[] { "Trainer" });
                //}
                #endregion

                if (context.Database.Exists() && !context.Database.CompatibleWithModel(false))
                    context.Database.Delete();

                if (!context.Database.Exists())
                {
                    context.Database.Create();
                    context.Database.ExecuteSqlCommand("ALTER TABLE UserProfile ADD CONSTRAINT U_email UNIQUE(Email)");
                    context.Database.ExecuteSqlCommand("ALTER TABLE Articles ADD CONSTRAINT U_title UNIQUE(CleanTitle)");
                    context.Database.ExecuteSqlCommand("CREATE INDEX IX_ArticleDescription ON Articles (Description)");
                }


            }
        }

    }



   
}
