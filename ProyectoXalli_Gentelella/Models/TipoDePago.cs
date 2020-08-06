using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("TiposDePago", Schema = "Fact")]
    public partial class TipoDePago {
        public TipoDePago() {
            this.DetallesDePago = new HashSet<DetalleDePago>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "La longitud debe ser de 3 dígitos")]
        [Display(Name = "Código")]
        public string CodigoTipoPago { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(50, ErrorMessage = "La longitud excede los 50 dígitos")]
        [Display(Name = "Tipo de pago")]
        public string DescripcionTipoPago { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoTipoPago { get; set; }

        //RELACION HIJA
        public virtual ICollection<DetalleDePago> DetallesDePago { get; set; }
    }
}