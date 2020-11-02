using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("DetallesDePago", Schema = "Fact")]
    public partial class DetalleDePago {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "(0:c2)")]
        [Display(Name = "Propina voluntaria")]
        public double CantidadPagar { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "(0:c2)")]
        [Display(Name = "Propina voluntaria")]
        public double MontoRecibido { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "(0:c2)")]
        [Display(Name = "Propina voluntaria")]
        public double MontoEntregado { get; set; }

        //FOREING KEY
        public int TipoPagoId { get; set; }
        public int PagoId { get; set; }
        public int MonedaId { get; set; }

        //DECLARACION DE RELACIONES PADRES
        public virtual TipoDePago TipoDePago { get; set; }
        public virtual Pago Pago { get; set; }
        public virtual Moneda Moneda { get; set; }

    }
}