using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {
    public class DBControl : DbContext {
        public DBControl() : base("HotelDB") {
            this.Configuration.ProxyCreationEnabled = false;
        }

        //MODULO CONTROL DE INSUMOS
        public virtual DbSet<Bodega> Bodegas { get; set; }
        public virtual DbSet<CategoriaProducto> CategoriasProducto { get; set; }
        public virtual DbSet<Dato> Datos { get; set; }
        public virtual DbSet<DetalleDeEntrada> DetallesDeEntrada { get; set; }
        public virtual DbSet<Entrada> Entradas { get; set; }
        public virtual DbSet<Producto> Productos { get; set; }
        public virtual DbSet<Proveedor> Proveedores { get; set; }
        public virtual DbSet<TipoDeEntrada> TiposDeEntrada { get; set; }
        public virtual DbSet<UnidadDeMedida> UnidadesDeMedida { get; set; }

        //MODULO MENU
        public virtual DbSet<Ingrediente> Ingredientes { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<CategoriaMenu> CategoriasMenu { get; set; }
        public virtual DbSet<Imagen> Imagenes { get; set; }

        //MODULO ORDENES
        public virtual DbSet<Mesero> Meseros { get; set; }
        public virtual DbSet<Cliente> Clientes { get; set; }
        public virtual DbSet<Orden> Ordenes { get; set; }
        public virtual DbSet<DetalleDeOrden> DetallesDeOrden { get; set; }

        //MODULO FACTURACION
        public virtual DbSet<Pago> Pagos { get; set; }
        public virtual DbSet<DetalleDePago> DetallesDePago { get; set; }
        public virtual DbSet<OrdenPago> OrdenesPago { get; set; }
        public virtual DbSet<TipoDePago> TiposDePago { get; set; }
        public virtual DbSet<Moneda> Monedas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingEntitySetNameConvention>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }
    }
}