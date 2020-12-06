namespace ProyectoXalli_Gentelella.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CampoInventariado : DbMigration
    {
        public override void Up()
        {
            AddColumn("Menu.Menus", "Inventariado", c => c.Boolean(nullable: false));
            DropColumn("Menu.Menus", "shot");
        }
        
        public override void Down()
        {
            AddColumn("Menu.Menus", "shot", c => c.Boolean(nullable: false));
            DropColumn("Menu.Menus", "Inventariado");
        }
    }
}
