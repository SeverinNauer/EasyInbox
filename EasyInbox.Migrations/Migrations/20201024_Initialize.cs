using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInbox.Migrations.Migrations
{
    [Migration(20201024084300)]
    public class Initialize : Migration
    {
        public override void Down()
        {
            Delete.Table("User");
        }

        public override void Up()
        {
            Create.Table("User")
                .WithColumn("UserId").AsGuid().PrimaryKey().Identity()
                .WithColumn("Email").AsString(100).Unique()
                .WithColumn("Password").AsString(255);
        }
    }
}
