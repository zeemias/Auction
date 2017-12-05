namespace Auction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Items",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        DefaultBet = c.Int(nullable: false),
                        Step = c.Int(nullable: false),
                        LastBet = c.Int(nullable: false),
                        LastUserId = c.String(),
                        LastBetTime = c.DateTime(nullable: false),
                        TimeOut = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Stories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItemId = c.Int(nullable: false),
                        User = c.String(),
                        UserId = c.String(),
                        LastBet = c.Int(nullable: false),
                        NewBet = c.Int(nullable: false),
                        Time = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AspNetUsers", "Coints", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Coints");
            DropTable("dbo.Stories");
            DropTable("dbo.Items");
        }
    }
}
