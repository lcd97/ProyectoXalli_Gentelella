using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {
    [Table("TasasCambio", Schema = "Tasa")]
    public partial class TasaCambio {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        public DateTime FechaTasa { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "(0:c2)")]
        [Display(Name = "Tasa cambio")]
        public double Cambio { get; set; }
    }
}