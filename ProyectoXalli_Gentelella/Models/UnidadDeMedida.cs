using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("UnidadesDeMedida", Schema = "Inv")]
    public partial class UnidadDeMedida {
        //CONSTRUCTOR
        public UnidadDeMedida() {
            this.Productos = new HashSet<Producto>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "La longitud debe ser de 3 dígitos")]
        [Display(Name = "Código")]
        public string CodigoUnidadMedida { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(50, ErrorMessage = "La longitud excede los 50 dígitos")]
        [Display(Name = "U/M")]
        public string DescripcionUnidadMedida { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(10, ErrorMessage = "La longitud excede los 10 dígitos")]
        [Display(Name = "Abreviatura")]
        public string AbreviaturaUM { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoUnidadMedida { get; set; }

        //CAMPO MAPEABLE
        [NotMapped]
        public string unidadMedida { get { return DescripcionUnidadMedida.Trim() + " - " + AbreviaturaUM; } }

        //DEFINICION DE RELACIONES HIJOS
        public virtual ICollection<Producto> Productos { get; set; }
    }
}