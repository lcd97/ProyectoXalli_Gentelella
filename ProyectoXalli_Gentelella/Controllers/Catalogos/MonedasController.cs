using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Catalogos {
    public class MonedasController : Controller {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        // GET: Monedas
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// RECUPERA DATOS PARA LLENAR LA TABLA MONEDAS A TRAVES DE JSON
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetData() {
            var moneda = await db.Monedas.Where(c => c.EstadoMoneda == true).ToListAsync();

            return Json(new { data = moneda }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        // GET: TiposDeEntrada/Details/5
        public async Task<ActionResult> Details(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Moneda moneda = await db.Monedas.FindAsync(id);
            if (moneda == null) {
                return HttpNotFound();
            }
            return View(moneda);
        }

        [Authorize(Roles = "Admin")]
        // GET: TiposDeEntrada/Create
        public ActionResult Create() {
            return View();
        }

        // POST: TiposDeEntrada/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,CodigoMoneda,DescripcionMoneda,EstadoMoneda")] Moneda Moneda) {
            //BUSCAR QUE LA DESCRIPCION DE TIPO DE BODEGA NO EXISTA
            Moneda bod = db.Monedas.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionMoneda.ToUpper().Trim() == Moneda.DescripcionMoneda.ToUpper().Trim());

            //SI LA BODEGA EXISTE CON ESA DESCRIPCION
            if (bod != null) {
                ModelState.AddModelError("DescripcionMoneda", "Utilice otro nombre");
                mensaje = "La moneda ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //ESTADO DE TIPO DE ENTRADA CUANDO SE CREA SIEMPRE ES TRUE
                    Moneda.EstadoMoneda = true;
                    if (ModelState.IsValid) {
                        db.Monedas.Add(Moneda);
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

        [Authorize(Roles = "Admin")]
        // GET: TiposDeEntrada/Edit/5
        public async Task<ActionResult> Edit(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Moneda moneda = await db.Monedas.FindAsync(id);
            if (moneda == null) {
                return HttpNotFound();
            }
            return View(moneda);
        }

        // POST: TiposDeEntrada/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CodigoMoneda,DescripcionMoneda,EstadoMoneda")] Moneda moneda) {
            //BUSCAR QUE LA DESCRIPCION DE TIPO DE BODEGA NO EXISTA
            Moneda bod = db.Monedas.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionMoneda.ToUpper().Trim() == moneda.DescripcionMoneda.ToUpper().Trim() && b.Id != moneda.Id);

            //SI LA BODEGA EXISTE CON ESA DESCRIPCION
            if (bod != null) {
                ModelState.AddModelError("DescripcionMoneda", "Utilice otro nombre");
                mensaje = "La moneda ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    if (ModelState.IsValid) {
                        db.Entry(moneda).State = EntityState.Modified;
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
            var code = db.Monedas.Max(x => x.CodigoMoneda.Trim());
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
            var Moneda = db.Monedas.Find(id);
            //BUSCANDO QUE TIPO DE ENTRADA NO TENGA SALIDAS NI ENTRADAS REGISTRADAS CON SU ID
            Pago oPago = db.Pagos.DefaultIfEmpty(null).FirstOrDefault(p => p.MonedaId == Moneda.Id);
            DetalleDePago dPago = db.DetallesDePago.DefaultIfEmpty(null).FirstOrDefault(p => p.MonedaId == Moneda.Id);

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //SI EL TIPO DE ENTRADA NO TIENE ENTRADAS ASOCIADAS AL ID
                    if (oPago == null || dPago == null) {
                        db.Monedas.Remove(Moneda);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Eliminado correctamente" : "Error al eliminar";
                    } else {
                        mensaje = "Se encontraron movimientos registrados asociados a esta moneda";
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