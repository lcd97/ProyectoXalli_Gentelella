using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("Menus", Schema = "Menu")]
    public partial class Menu {
        //CONSTRUCTOR DE RELACION HIJA
        public Menu() {
            this.Ingredientes = new HashSet<Ingrediente>();
            this.DetallesDeOrden = new HashSet<DetalleDeOrden>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "La longitud debe ser de 3 dígitos")]
        [Display(Name = "Código")]
        public string CodigoMenu { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(100, ErrorMessage = "La longitud no debe exceder de 100 dígitos")]
        [Display(Name = "Platillo")]
        public string DescripcionMenu { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Range(1, (double)decimal.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "(0:c2)")]
        [Display(Name = "Precio")]
        public double PrecioMenu { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(10, ErrorMessage = "La longitud debe ser de 10 dígitos")]
        [Display(Name = "Código")]
        public string TiempoEstimado { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoMenu { get; set; }

        //DEFINCION DE FK
        public int CategoriaMenuId { get; set; }

        public int ImagenId { get; set; }

        //DEFINCION DE RELACIONES HIJAS
        public virtual ICollection<Ingrediente> Ingredientes { get; set; }
        public virtual ICollection<DetalleDeOrden> DetallesDeOrden { get; set; }

        //DEFINICION DE RELACIONES PADRES
        public virtual CategoriaMenu CategoriaMenu { get; set; }
        public virtual Imagen Imagen { get; set; }
    }
}