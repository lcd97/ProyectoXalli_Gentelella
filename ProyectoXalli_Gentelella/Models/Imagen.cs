using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("Imagenes", Schema = "Menu")]
    public partial class Imagen {
        //CONSTRUCTOR DE CLASE HIJA
        public Imagen() {
            this.Menus = new HashSet<Menu>();
            this.Ordenes = new HashSet<Orden>();
            this.Pagos = new HashSet<Pago>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(100, ErrorMessage = "La longitud no debe exceder de 100 dígitos")]
        [Display(Name = "Ruta")]
        public string Ruta { get; set; }

        //DEFINCION DE RELACIONES HIIJAS
        public virtual ICollection<Menu> Menus { get; set; }
        public virtual ICollection<Orden> Ordenes { get; set; }
        public virtual ICollection<Pago> Pagos { get; set; }
    }
}