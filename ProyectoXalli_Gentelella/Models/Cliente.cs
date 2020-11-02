using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("Clientes", Schema = "Ord")]
    public partial class Cliente {
        //CONTRUCTOR DE CLASE HIJA
        public Cliente() {
            this.Ordenes = new HashSet<Orden>();
        }

        [Key]
        public int Id { get; set; }

        [StringLength(150, ErrorMessage = "El correo no debe exceder los 50 dígitos")]
        [Display(Name = "Correo Electrónico")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        [Required(ErrorMessage = "El campo es obligatorio")]
        public string EmailCliente { get; set; }

        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número telefónico no debe exceder los 8 dígitos")]
        [Display(Name = "Teléfono")]
        public string TelefonoCliente { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoCliente { get; set; }

        [StringLength(10, ErrorMessage = "El pasaporte no debe exceder los 10 dígitos")]
        [Display(Name = "Pasaporte")]
        public string PasaporteCliente { get; set; }

        //FOREIGN KEY
        public int DatoId { get; set; }
        public int ImagenId { get; set; }

        //DECLARACION DE RELACION PADRE
        public virtual Dato Dato { get; set; }
        public virtual Imagen Imagen { get; set; }

        //DEFINICION DE RELACION HIJA
        public virtual ICollection<Orden> Ordenes { get; set; }
    }
}