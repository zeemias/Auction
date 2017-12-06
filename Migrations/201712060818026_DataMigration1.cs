namespace Auction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "Image", c => c.String());
            AddColumn("dbo.Items", "LastUser", c => c.String());
            DropColumn("dbo.Items", "LastUserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Items", "LastUserId", c => c.String());
            DropColumn("dbo.Items", "LastUser");
            DropColumn("dbo.Items", "Image");
        }
    }
}
