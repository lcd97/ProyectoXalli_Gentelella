using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("CategoriasProducto", Schema = "Inv")]
    public partial class CategoriaProducto {
        //CONSTRUCTOR DE CLASE HIJA
        public CategoriaProducto() {
            this.Productos = new HashSet<Producto>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "La longitud debe ser de 3 dígitos")]
        [Display(Name = "Código")]
        public string CodigoCategoria { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(50, ErrorMessage = "La longitud excede los 50 dígitos")]
        [Display(Name = "Categoría")]
        public string DescripcionCategoria { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoCategoria { get; set; }

        //DEFINICION DE RELACIONES HIJOS
        public virtual ICollection<Producto> Productos { get; set; }
    }
}