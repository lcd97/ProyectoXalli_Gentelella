using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("Datos", Schema = "Inv")]
    public partial class Dato {
        //CONSTRUCTOR
        public Dato() {
            this.Proveedores = new HashSet<Proveedor>();
            this.Clientes = new HashSet<Cliente>();
            this.Meseros = new HashSet<Mesero>();
        }

        [Key]
        public int Id { get; set; }

        [StringLength(16, MinimumLength = 16, ErrorMessage = "La longitud debe ser de 16 dígitos")]
        [Display(Name = "Documento de Identidad")]
        public string Cedula { get; set; }

        [StringLength(50, ErrorMessage = "La longitud debe ser de 16 dígitos")]
        [Display(Name = "Nombres")]
        public string PNombre { get; set; }

        [StringLength(50, ErrorMessage = "La longitud debe ser de 16 dígitos")]
        [Display(Name = "Apellidos")]
        public string PApellido { get; set; }

        [StringLength(14, MinimumLength = 14, ErrorMessage = "La longitud excede los 14 dígitos")]
        [Display(Name = "RUC")]
        public string RUC { get; set; }

        //[StringLength(50, ErrorMessage = "La longitud debe ser de 16 dígitos")]
        //[Display(Name = "Apellido 2")]
        //public string SApellido { get; set; }

        //DEFINCION DE RELACIONES HIIJAS
        public virtual ICollection<Proveedor> Proveedores { get; set; }
        public virtual ICollection<Cliente> Clientes { get; set; }
        public virtual ICollection<Mesero> Meseros { get; set; }
    }
}