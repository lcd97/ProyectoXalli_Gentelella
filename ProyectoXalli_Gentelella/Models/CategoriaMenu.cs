using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("CategoriasMenu", Schema = "Menu")]
    public partial class CategoriaMenu {
        //CONSTRUCTOR DE LA CLASE HIJA
        public CategoriaMenu() {
            this.Menus = new HashSet<Menu>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "La longitud debe ser de 3 dígitos")]
        [Display(Name = "Código")]
        public string CodigoCategoriaMenu { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(50, ErrorMessage = "La longitud no debe exceder de 50 dígitos")]
        [Display(Name = "Categoría")]
        public string DescripcionCategoriaMenu { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoCategoriaMenu { get; set; }

        //FOREIGN KEY
        public int BodegaId { get; set; }

        //DEFINCION DE RELACIONES HIJAS
        public virtual ICollection<Menu> Menus { get; set; }

        //DEFINICION DE RELACIONES PADRES
        public virtual Bodega Bodega { get; set; }
    }
}