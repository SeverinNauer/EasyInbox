using FluentMigrator;

namespace EasyInbox.Migrations.Migrations
{
    [Migration(20201024084300)]
    public class Initialize : Migration
    {
        public override void Up()
        {
            Create.Table("User")
                .WithColumn("UserId").AsGuid().PrimaryKey()
                .WithColumn("Email").AsString(100).Unique()
                .WithColumn("Password").AsString(255);
        }
        public override void Down()
        {
            Delete.Table("User");
        }
    }
}
