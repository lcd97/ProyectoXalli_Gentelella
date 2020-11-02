using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("Pagos", Schema = "Fact")]
    public partial class Pago {
        //CONSTRUCTOR CLASE
        public Pago() {
            this.OrdenesPago = new HashSet<OrdenPago>();
            this.DetallesPago = new HashSet<DetalleDePago>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Número de factura")]
        public int NumeroPago { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de entrada")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaPago { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Descuento")]
        public int Descuento { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Impuestos")]
        public int IVA { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "(0:c2)")]
        [Display(Name = "Propina voluntaria")]
        public double Propina { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "(0:c2)")]
        [Display(Name = "Tipo cambio")]
        public double TipoCambio { get; set; }

        //FOREING KEY
        public int MonedaId { get; set; }

        //DECLARACION DE RELACIONES PADRES
        public virtual Moneda Moneda { get; set; }

        //DECLARACION DE RELACIONES HIJAS
        public virtual ICollection<OrdenPago> OrdenesPago { get; set; }
        public virtual ICollection<DetalleDePago> DetallesPago { get; set; }
    }
}