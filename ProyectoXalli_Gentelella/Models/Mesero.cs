using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("Meseros", Schema = "Ord")]
    public partial class Mesero {
        public Mesero() {
            this.Ordenes = new HashSet<Orden>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(9, ErrorMessage = "El código INSS no debe exceder los 8 caracteres")]
        [Display(Name = "INSS")]
        public string INSS { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(10, ErrorMessage = "La Hora de entrada no debe exceder los 10 caracteres")]
        [Display(Name = "Hora Entrada")]
        public string HoraEntrada { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(10, ErrorMessage = "La Hora de entrada no debe exceder los 10 caracteres")]
        [Display(Name = "Hora Salida")]
        public string HoraSalida { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoMesero { get; set; }

        //FOREIGN KEY
        public int DatoId { get; set; }

        //DECLARACION DE RELACION PADRE
        public virtual Dato Dato { get; set; }

        //DECLARACION DE RELACION HIJA
        public ICollection<Orden> Ordenes { get; set; }
    }
}