using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using System.Net;

namespace ProyectoXalli_Gentelella.Controllers.Catalogos {
    public class MesasController : Controller {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";


        // GET: Mesas
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// RECUPERA DATOS PARA LLENAR LA TABLA MESA A TRAVES DE JSON
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetData() {
            db.Configuration.ProxyCreationEnabled = false;
            var mesas = await db.Mesas.Where(c => c.EstadoMesa == true).ToListAsync();

            return Json(new { data = mesas }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        // GET: Categorias/Create
        public ActionResult Create() {
            return View();
        }

        // POST: Bodegas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,CodigoMesa,DescripcionMesa,EstadoMesa")] Mesa mesa) {
            //BUSCAR SI YA SE ENCUENTRA REGISTRADO UNA BODEGA CON LA DESCRIPCION INGRESADA
            Mesa bod = db.Mesas.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionMesa.ToUpper().Trim() == mesa.DescripcionMesa.ToUpper());

            //SI ENCUENTRA UNA BODEGA CON ESA DESCRIPCION
            if (bod != null) {
                ModelState.AddModelError("DescripcionMesa", "Utilice otro nombre");
                mensaje = "La descripción ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {

                try {
                    mesa.EstadoMesa = true;
                    if (ModelState.IsValid) {
                        db.Mesas.Add(mesa);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Almacenado correctamente" : "Error al guardar";
                    } else {
                        //ESTO ES PARA VER EL ERROR QUE DEVUELVE EL MODELO
                        string cad = "";
                        foreach (ModelState modelState in ViewData.ModelState.Values) {
                            foreach (ModelError error in modelState.Errors) {
                                cad += (error);
                            }
                        }
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al almacenar";
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        // GET: Bodegas/Edit/5
        public async Task<ActionResult> Edit(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mesa mesa = await db.Mesas.FindAsync(id);
            if (mesa == null) {
                return HttpNotFound();
            }
            return View(mesa);
        }

        // POST: Bodegas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CodigoMesa,DescripcionMesa,EstadoMesa")] Mesa mesa) {
            //COMPROBAR QUE NO EXISTA LA MISMA DESCRIPCION DE BODEGAS
            var comprobar = db.Mesas.DefaultIfEmpty(null).FirstOrDefault(c => c.DescripcionMesa.ToUpper().Trim() == mesa.DescripcionMesa.ToUpper().Trim() && c.Id != mesa.Id);

            //SI YA EXISTE LA BODEGA MANDAR ERROR
            if (comprobar != null) {
                ModelState.AddModelError("DescripcionMesa", "Utilice otro nombre");
                mensaje = "La descripción ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    if (ModelState.IsValid) {
                        db.Entry(mesa).State = EntityState.Modified;
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

        // GET: Bodegas/Delete/5
        public async Task<ActionResult> Delete(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mesa mesa = await db.Mesas.FindAsync(id);
            if (mesa == null) {
                return HttpNotFound();
            }
            return View(mesa);
        }

        /// <summary>
        /// RETORNA EL CODIGO AUTOMATICAMENTE A LA VISTA CREATE
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchCode() {
            string num;
            //BUSCAR EL VALOR MAXIMO DE LAS BODEGAS REGISTRADAS
            var code = db.Mesas.Max(x => x.CodigoMesa.Trim());
            int valor;

            //SI EXISTE ALGUN REGISTRO
            if (code != null) {
                //CONVERTIR EL CODIGO A ENTERO
                valor = int.Parse(code);

                //SE COMIENZA A AGREGAR UN VALOR SECUENCIAL AL CODIGO ENCONTRADO
                if (valor <= 8)
                    num = "00" + (valor + 1);
                else
                if (valor >= 9 && valor < 100)
                    num = "0" + (valor + 1);
                else
                    num = (valor + 1).ToString();
            } else
                num = "001";//SE COMIENZA CON EL PRIMER CODIGO DEL REGISTRO

            return Json(num, JsonRequestBehavior.AllowGet);
        }

        // POST: Bodegas/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            var mesa = db.Mesas.Find(id);
            //BUSCANDO QUE BODEGA NO TENGAREGISTROS ASIGNADOS CON SU ID
            var orden = db.Ordenes.DefaultIfEmpty(null).FirstOrDefault(e => e.MesaId == mesa.Id);

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //SI NO SE ENCONTRARON SALIDAS O ENTRADAS AL ALMACEN
                    if (orden == null) {
                        db.Mesas.Remove(mesa);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Eliminado correctamente" : "Error al eliminar";
                    } else {
                        mensaje = "Se encontraron productos registrados a este almacen";
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