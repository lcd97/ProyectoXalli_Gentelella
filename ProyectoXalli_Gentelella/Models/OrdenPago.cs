using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Models {

    [Table("OrdenesPago", Schema = "Fact")]
    public partial class OrdenPago {
        [Key]
        public int Id { get; set; }

        //FOREING KEY
        public int OrdenId { get; set; }
        public int PagoId { get; set; }

        //RELACION PADRE
        public virtual Orden Orden { get; set; }
        public virtual Pago Pago { get; set; }
    }
}