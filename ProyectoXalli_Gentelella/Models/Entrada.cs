using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("Entradas", Schema = "Inv")]
    public partial class Entrada {
        //CONSTRUCTOR
        public Entrada() {
            this.DetallesDeEntrada = new HashSet<DetalleDeEntrada>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "La longitud debe ser de 3 dígitos")]
        [Display(Name = "Código")]
        public string CodigoEntrada { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de entrada")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaEntrada { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [Display(Name = "Activo")]
        public bool EstadoEntrada { get; set; }

        //DEFINCION DE FK
        public int TipoEntradaId { get; set; }
        public int BodegaId { get; set; }
        public int ProveedorId { get; set; }

        //DEFINCION DE RELACIONES HIJAS
        public virtual ICollection<DetalleDeEntrada> DetallesDeEntrada { get; set; }

        //DEFINICION DE RELACIONES PADRES
        public virtual Bodega Bodega { get; set; }
        public virtual Proveedor Proveedor { get; set; }
    }
}