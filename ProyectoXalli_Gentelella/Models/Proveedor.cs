using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("Proveedores", Schema = "Inv")]
    public partial class Proveedor {
        
        //CONSTRUCTOR
        public Proveedor() {
            this.Entradas = new HashSet<Entrada>();
        }

        [Key]
        public int Id { get; set; }

        [StringLength(50, ErrorMessage = "La longitud excede los 50 dígitos")]
        [Display(Name = "Nombre comercial")]
        public string NombreComercial { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(9, ErrorMessage = "La longitud excede los 9 dígitos")]
        [Display(Name = "Número telefónico")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoProveedor { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Proveedor local")]
        public bool Local { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Retiene IR")]
        public bool RetenedorIR { get; set; }

        //FOREIGN KEY
        public int DatoId { get; set; }

        //DECLARACION DE RELACIONES HIJOS
        public virtual ICollection<Entrada> Entradas { get; set; }

        //DECLARACION DE RELACION PADRE
        public virtual Dato Dato { get; set; }
    }
}