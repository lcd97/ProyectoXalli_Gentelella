using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("DetallesDeEntrada", Schema = "Inv")]
    public partial class DetalleDeEntrada {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Cantidad")]
        public double CantidadEntrada { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "(0:c2)")]
        [Display(Name = "Precio")]
        public double PrecioEntrada { get; set; }

        //DEFINCION DE FK
        public int EntradaId { get; set; }
        public int ProductoId { get; set; }

        //DECLARACION DE RELACIONES PADRES
        public virtual Entrada Entrada { get; set; }
        public virtual Producto Producto { get; set; }
    }
}