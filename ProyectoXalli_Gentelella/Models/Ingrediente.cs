using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("Ingredientes", Schema = "Menu")]
    public partial class Ingrediente {
        [Key]
        public int Id { get; set; }

        //DEFINCION DE FK
        public int MenuId { get; set; }
        public int ProductoId { get; set; }

        //DEFINICION DE RELACIONES PADRES
        public virtual Menu Menu { get; set; }
        public virtual Producto Producto { get; set; }
    }
}