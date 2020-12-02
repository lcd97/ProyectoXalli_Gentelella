using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("TiposDeOrden", Schema = "Ord")]
    public partial class TipoDeOrden {
        //CONSTRUCTOR DE CLASE HIJA
        public TipoDeOrden() {
            this.Ordenes = new HashSet<Orden>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "La longitud debe ser de 3 dígitos")]
        [Display(Name = "Código")]
        public string CodigoTipoOrden { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(50, ErrorMessage = "La longitud excede los 50 dígitos")]
        [Display(Name = "Tipo de orden")]
        public string DescripcionTipoOrden { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoTipoOrden { get; set; }

        //RELACION HIJA
        public ICollection<Orden> Ordenes { get; set; }
    }
}