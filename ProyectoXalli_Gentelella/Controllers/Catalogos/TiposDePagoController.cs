using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Catalogos
{
    public class TiposDePagoController : Controller
    {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        // GET: TiposDePago
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// RECUPERA DATOS PARA LLENAR LA TABLA CATEGORIAS A TRAVES DE JSON
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetData() {
            var tiposPago = await db.TiposDePago.Where(c => c.EstadoTipoPago == true).ToListAsync();

            return Json(new { data = tiposPago }, JsonRequestBehavior.AllowGet);
        }

        // GET: TiposDeEntrada/Details/5
        public async Task<ActionResult> Details(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoDePago tiposPago = await db.TiposDePago.FindAsync(id);
            if (tiposPago == null) {
                return HttpNotFound();
            }
            return View(tiposPago);
        }

        // GET: TiposDeEntrada/Create
        public ActionResult Create() {
            return View();
        }

        // POST: TiposDeEntrada/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,CodigoTipoPago,DescripcionTipoPago,EstadoTipoPago")] TipoDePago TipoDePago) {
            //BUSCAR QUE LA DESCRIPCION DE TIPO DE BODEGA NO EXISTA
            TipoDePago bod = db.TiposDePago.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionTipoPago.ToUpper().Trim() == TipoDePago.DescripcionTipoPago.ToUpper().Trim());

            //SI LA BODEGA EXISTE CON ESA DESCRIPCION
            if (bod != null) {
                ModelState.AddModelError("DescripcionTipoPago", "Utilice otro nombre");
                mensaje = "La descripción ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //ESTADO DE TIPO DE ENTRADA CUANDO SE CREA SIEMPRE ES TRUE
                    TipoDePago.EstadoTipoPago = true;
                    if (ModelState.IsValid) {
                        db.TiposDePago.Add(TipoDePago);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al almacenar";
                    transact.Rollback();
                }
            }//FIN USING

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        // GET: TiposDeEntrada/Edit/5
        public async Task<ActionResult> Edit(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoDePago TipoDePago = await db.TiposDePago.FindAsync(id);
            if (TipoDePago == null) {
                return HttpNotFound();
            }
            return View(TipoDePago);
        }

        // POST: TiposDeEntrada/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CodigoTipoPago,DescripcionTipoPago,EstadoTipoPago")] TipoDePago TipoDePago) {
            //BUSCAR QUE LA DESCRIPCION DE TIPO DE BODEGA NO EXISTA
            TipoDePago bod = db.TiposDePago.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionTipoPago.ToUpper().Trim() == TipoDePago.DescripcionTipoPago.ToUpper().Trim() && b.Id != TipoDePago.Id);

            //SI LA BODEGA EXISTE CON ESA DESCRIPCION
            if (bod != null) {
                ModelState.AddModelError("DescripcionTipoPago", "Utilice otro nombre");
                mensaje = "La descripción ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    if (ModelState.IsValid) {
                        db.Entry(TipoDePago).State = EntityState.Modified;
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Modificado correctamente" : "Error al modificar";
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al modificar";
                    transact.Rollback();
                }
            }

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// RETORNA EL CODIGO AUTOMATICAMENTE A LA VISTA CREATE
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchCode() {
            //BUSCAR EL VALOR MAXIMO DE LAS BODEGAS REGISTRADAS
            var code = db.TiposDePago.Max(x => x.CodigoTipoPago.Trim());
            int valor;
            string num;

            //SI EXISTE ALGUN REGISTRO
            if (code != null) {
                //CONVERTIR EL CODIGO A ENTERO
                valor = int.Parse(code);

                //SE COMIENZA A AGREGAR UN VALOR SECUENCIAL AL CODIGO ENCONTRADO
                if (valor <= 8)
                    num = "00" + (valor + 1);
                else
                if (valor >= 9 && valor < 99)
                    num = "0" + (valor + 1);
                else
                    num = (valor + 1).ToString();
            } else
                num = "001";//SE COMIENZA CON EL PRIMER CODIGO DEL REGISTRO

            return Json(num, JsonRequestBehavior.AllowGet);
        }

        // POST: TiposDeEntrada/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            var TipoDePago = db.TiposDePago.Find(id);
            //BUSCANDO QUE TIPO DE ENTRADA NO TENGA SALIDAS NI ENTRADAS REGISTRADAS CON SU ID
            DetalleDePago oPago = db.DetallesDePago.DefaultIfEmpty(null).FirstOrDefault(p => p.TipoPagoId == TipoDePago.Id);

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //SI EL TIPO DE ENTRADA NO TIENE ENTRADAS ASOCIADAS AL ID
                    if (oPago == null) {
                        db.TiposDePago.Remove(TipoDePago);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Eliminado correctamente" : "Error al eliminar";
                    } else {
                        mensaje = "Se encontraron pagos registrados a esta tipo de pago";
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al eliminar";
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}