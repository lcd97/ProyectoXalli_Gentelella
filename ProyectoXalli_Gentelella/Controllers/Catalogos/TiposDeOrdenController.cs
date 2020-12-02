using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProyectoXalli_Gentelella.Models;

namespace ProyectoXalli_Gentelella.Controllers.Catalogos {
    public class TiposDeOrdenController : Controller {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        [Authorize(Roles = "Admin")]
        // GET: TiposDeOrden
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// RECUPERA DATOS PARA LLENAR LA TABLA CATEGORIAS A TRAVES DE JSON
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetData() {
            var tiposDeOrden = await db.TiposDeOrden.Where(c => c.EstadoTipoOrden == true).ToListAsync();

            return Json(new { data = tiposDeOrden }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        // GET: TiposDeOrden/Details/5
        public async Task<ActionResult> Details(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoDeOrden tiposDeOrden = await db.TiposDeOrden.FindAsync(id);
            if (tiposDeOrden == null) {
                return HttpNotFound();
            }
            return View(tiposDeOrden);
        }

        [Authorize(Roles = "Admin")]
        // GET: TiposDeOrden/Create
        public ActionResult Create() {
            return View();
        }

        // POST: TiposDeOrden/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,CodigoTipoOrden,DescripcionTipoOrden,EstadoTipoOrden")] TipoDeOrden TipoDeOrden) {
            //BUSCAR QUE LA DESCRIPCION DE TIPO DE BODEGA NO EXISTA
            TipoDeOrden bod = db.TiposDeOrden.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionTipoOrden.ToUpper().Trim() == TipoDeOrden.DescripcionTipoOrden.ToUpper().Trim());

            //SI LA BODEGA EXISTE CON ESA DESCRIPCION
            if (bod != null) {
                ModelState.AddModelError("DescripcionTipoOrden", "Utilice otro nombre");
                mensaje = "La descripción ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //ESTADO DE TIPO DE ENTRADA CUANDO SE CREA SIEMPRE ES TRUE
                    TipoDeOrden.EstadoTipoOrden = true;
                    if (ModelState.IsValid) {
                        db.TiposDeOrden.Add(TipoDeOrden);
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
        // GET: TiposDeOrden/Edit/5
        public async Task<ActionResult> Edit(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoDeOrden TipoDeOrden = await db.TiposDeOrden.FindAsync(id);
            if (TipoDeOrden == null) {
                return HttpNotFound();
            }
            return View(TipoDeOrden);
        }

        // POST: TiposDeOrden/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CodigoTipoOrden,DescripcionTipoOrden,EstadoTipoOrden")] TipoDeOrden TipoDeOrden) {
            //BUSCAR QUE LA DESCRIPCION DE TIPO DE BODEGA NO EXISTA
            TipoDeOrden bod = db.TiposDeOrden.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionTipoOrden.ToUpper().Trim() == TipoDeOrden.DescripcionTipoOrden.ToUpper().Trim() && b.Id != TipoDeOrden.Id);

            //SI LA BODEGA EXISTE CON ESA DESCRIPCION
            if (bod != null) {
                ModelState.AddModelError("DescripcionTipoOrden", "Utilice otro nombre");
                mensaje = "La descripción ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    if (ModelState.IsValid) {
                        db.Entry(TipoDeOrden).State = EntityState.Modified;
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
            var code = db.TiposDeOrden.Max(x => x.CodigoTipoOrden.Trim());
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

        // POST: TiposDeOrden/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            var TipoDeOrden = db.TiposDeOrden.Find(id);
            //BUSCANDO QUE TIPO DE ENTRADA NO TENGA SALIDAS NI ENTRADAS REGISTRADAS CON SU ID
            Orden oEntrada = db.Ordenes.DefaultIfEmpty(null).FirstOrDefault(p => p.TipoOrdenId == TipoDeOrden.Id);

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //SI EL TIPO DE ENTRADA NO TIENE ENTRADAS ASOCIADAS AL ID
                    if (oEntrada == null) {
                        db.TiposDeOrden.Remove(TipoDeOrden);
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
    }
}