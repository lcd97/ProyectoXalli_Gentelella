using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("Productos", Schema = "Inv")]
    public partial class Producto {
        //CONSTRUCTOR DE CLASES HIJAS
        public Producto() {
            this.DetallesDeEntrada = new HashSet<DetalleDeEntrada>();
            this.Ingredientes = new HashSet<Ingrediente>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "La longitud debe ser de 3 dígitos")]
        [Display(Name = "Código")]
        public string CodigoProducto { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(100, ErrorMessage = "La longitud excede los 100 dígitos")]
        [Display(Name = "Producto")]
        public string NombreProducto { get; set; }

        [StringLength(50, ErrorMessage = "La longitud excede los 50 dígitos")]
        [Display(Name = "Marca")]
        public string MarcaProducto { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DisplayFormat(DataFormatString = "(0:c2)")]//MUESTRA EL FORMATO "10.20"
        [Display(Name = "Presentación")]
        public double PresentacionProducto { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DisplayFormat(DataFormatString = "(0:c2)")]//MUESTRA EL FORMATO "10.20"
        [Display(Name = "Cantidad máxima")]
        public double CantidadMaxProducto { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DisplayFormat(DataFormatString = "(0:c2)")]//MUESTRA EL FORMATO "10.20"
        [Display(Name = "Cantidad mínima")]
        public double CantidadMinProducto { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoProducto { get; set; }

        //DEFINICION DE FK
        public int UnidadMedidaId { get; set; }
        public int CategoriaId { get; set; }

        //DEFINICION DE RELACIONES PADRES
        public virtual UnidadDeMedida UnidadDeMedida { get; set; }
        public virtual CategoriaProducto Categoria { get; set; }

        //DEFINCION DE RELACIONES HIJOS
        public virtual ICollection<DetalleDeEntrada> DetallesDeEntrada { get; set; }
        public virtual ICollection<Ingrediente> Ingredientes { get; set; }
    }
}