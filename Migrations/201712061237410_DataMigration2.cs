namespace Auction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "Group", c => c.String());
            AddColumn("dbo.AspNetUsers", "Group", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Group");
            DropColumn("dbo.Items", "Group");
        }
    }
}
