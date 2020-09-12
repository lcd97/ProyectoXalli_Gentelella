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
    public class TiposDeEntradaController : Controller
    {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        // GET: TiposDeEntrada
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// RECUPERA DATOS PARA LLENAR LA TABLA CATEGORIAS A TRAVES DE JSON
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetData() {
            var tiposDeEntrada = await db.TiposDeEntrada.Where(c => c.EstadoTipoEntrada == true).ToListAsync();

            return Json(new { data = tiposDeEntrada }, JsonRequestBehavior.AllowGet);
        }

        // GET: TiposDeEntrada/Details/5
        public async Task<ActionResult> Details(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoDeEntrada tiposDeEntrada = await db.TiposDeEntrada.FindAsync(id);
            if (tiposDeEntrada == null) {
                return HttpNotFound();
            }
            return View(tiposDeEntrada);
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
        public async Task<ActionResult> Create([Bind(Include = "Id,CodigoTipoEntrada,DescripcionTipoEntrada,EstadoTipoEntrada")] TipoDeEntrada TipoDeEntrada) {
            //BUSCAR QUE LA DESCRIPCION DE TIPO DE BODEGA NO EXISTA
            TipoDeEntrada bod = db.TiposDeEntrada.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionTipoEntrada.ToUpper().Trim() == TipoDeEntrada.DescripcionTipoEntrada.ToUpper().Trim());

            //SI LA BODEGA EXISTE CON ESA DESCRIPCION
            if (bod != null) {
                ModelState.AddModelError("DescripcionTipoEntrada", "Utilice otro nombre");
                mensaje = "La descripción ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //ESTADO DE TIPO DE ENTRADA CUANDO SE CREA SIEMPRE ES TRUE
                    TipoDeEntrada.EstadoTipoEntrada = true;
                    if (ModelState.IsValid) {
                        db.TiposDeEntrada.Add(TipoDeEntrada);
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
            TipoDeEntrada TipoDeEntrada = await db.TiposDeEntrada.FindAsync(id);
            if (TipoDeEntrada == null) {
                return HttpNotFound();
            }
            return View(TipoDeEntrada);
        }

        // POST: TiposDeEntrada/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CodigoTipoEntrada,DescripcionTipoEntrada,EstadoTipoEntrada")] TipoDeEntrada TipoDeEntrada) {
            //BUSCAR QUE LA DESCRIPCION DE TIPO DE BODEGA NO EXISTA
            TipoDeEntrada bod = db.TiposDeEntrada.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionTipoEntrada.ToUpper().Trim() == TipoDeEntrada.DescripcionTipoEntrada.ToUpper().Trim() && b.Id != TipoDeEntrada.Id);

            //SI LA BODEGA EXISTE CON ESA DESCRIPCION
            if (bod != null) {
                ModelState.AddModelError("DescripcionTipoEntrada", "Utilice otro nombre");
                mensaje = "La descripción ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    if (ModelState.IsValid) {
                        db.Entry(TipoDeEntrada).State = EntityState.Modified;
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
            var code = db.TiposDeEntrada.Max(x => x.CodigoTipoEntrada.Trim());
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
            var TipoDeEntrada = db.TiposDeEntrada.Find(id);
            //BUSCANDO QUE TIPO DE ENTRADA NO TENGA SALIDAS NI ENTRADAS REGISTRADAS CON SU ID
            Entrada oEntrada = db.Entradas.DefaultIfEmpty(null).FirstOrDefault(p => p.TipoEntradaId == TipoDeEntrada.Id);

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //SI EL TIPO DE ENTRADA NO TIENE ENTRADAS ASOCIADAS AL ID
                    if (oEntrada == null) {
                        db.TiposDeEntrada.Remove(TipoDeEntrada);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Eliminado correctamente" : "Error al eliminar";
                    } else {
                        mensaje = "Se encontraron entradas registrados a esta tipo de entradas";
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