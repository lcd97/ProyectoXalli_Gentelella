namespace ProyectoXalli_Gentelella.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ComponiendoCampos : DbMigration
    {
        public override void Up()
        {
            DropIndex("Inv.Entradas", new[] { "TipoDeEntrada_Id" });
            DropIndex("Inv.Productos", new[] { "UnidadDeMedida_Id" });
            DropColumn("Inv.Entradas", "TipoEntradaId");
            DropColumn("Inv.Productos", "UnidadMedidaId");
            RenameColumn(table: "Inv.Productos", name: "UnidadDeMedida_Id", newName: "UnidadMedidaId");
            RenameColumn(table: "Inv.Entradas", name: "TipoDeEntrada_Id", newName: "TipoEntradaId");
            AlterColumn("Inv.Entradas", "TipoEntradaId", c => c.Int(nullable: false));
            AlterColumn("Inv.Productos", "UnidadMedidaId", c => c.Int(nullable: false));
            CreateIndex("Inv.Entradas", "TipoEntradaId");
            CreateIndex("Inv.Productos", "UnidadMedidaId");
        }
        
        public override void Down()
        {
            DropIndex("Inv.Productos", new[] { "UnidadMedidaId" });
            DropIndex("Inv.Entradas", new[] { "TipoEntradaId" });
            AlterColumn("Inv.Productos", "UnidadMedidaId", c => c.Int());
            AlterColumn("Inv.Entradas", "TipoEntradaId", c => c.Int());
            RenameColumn(table: "Inv.Entradas", name: "TipoEntradaId", newName: "TipoDeEntrada_Id");
            RenameColumn(table: "Inv.Productos", name: "UnidadMedidaId", newName: "UnidadDeMedida_Id");
            AddColumn("Inv.Productos", "UnidadMedidaId", c => c.Int(nullable: false));
            AddColumn("Inv.Entradas", "TipoEntradaId", c => c.Int(nullable: false));
            CreateIndex("Inv.Productos", "UnidadDeMedida_Id");
            CreateIndex("Inv.Entradas", "TipoDeEntrada_Id");
        }
    }
}
