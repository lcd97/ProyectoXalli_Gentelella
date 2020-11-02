using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoXalli_Gentelella.Models {

    [Table("Monedas", Schema = "Fact")]
    public partial class Moneda {
        public Moneda() {
            this.Pagos = new HashSet<Pago>();
            this.DetallesDePago = new HashSet<DetalleDePago>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "La longitud debe ser de 3 dígitos")]
        [Display(Name = "Código")]
        public string CodigoMoneda { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(50, ErrorMessage = "La longitud excede los 50 dígitos")]
        [Display(Name = "Moneda")]
        public string DescripcionMoneda { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoMoneda { get; set; }

        //RELACION HIJA
        public virtual ICollection<Pago> Pagos { get; set; }
        public virtual ICollection<DetalleDePago> DetallesDePago { get; set; }
    }
}