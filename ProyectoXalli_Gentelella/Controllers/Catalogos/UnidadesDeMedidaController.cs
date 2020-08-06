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
    public class UnidadesDeMedidaController : Controller
    {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        // GET: UnidadesDeMedida
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// RECUPERA DATOS PARA LLENAR LA TABLA UNIDADES DE MEDIDA A TRAVES DE JSON
        /// </summary>
        /// <returns></returns>
        public JsonResult GetData() {
            //OBTIENE TODOS LOS OBJETOS UNIDADES DE MEDIDA ACTIVOS
            var unidades = (from u in db.UnidadesDeMedida.ToList()
                            where u.EstadoUnidadMedida == true
                            select new {
                                Id = u.Id,
                                CodigoUnidadMedida = u.CodigoUnidadMedida,
                                DescripcionUnidadMedida = u.DescripcionUnidadMedida + " - " + u.AbreviaturaUM,
                            });

            return Json(new { data = unidades }, JsonRequestBehavior.AllowGet);
        }

        // GET: UnidadesDeMedida/Details/5
        public async Task<ActionResult> Details(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnidadDeMedida unidad = await db.UnidadesDeMedida.FindAsync(id);
            if (unidad == null) {
                return HttpNotFound();
            }
            return View(unidad);
        }

        // GET: UnidadDeMedida/Create
        public ActionResult Create() {
            return View();
        }

        // POST: UnidadDeMedida/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,CodigoUnidadMedida,DescripcionUnidadMedida,AbreviaturaUM,EstadoUnidadMedida")] UnidadDeMedida UnidadDeMedida) {
            //SE BUSCA UNIDADES DE MEDIDA REGISTRADAS CON LA DESCRIPCION REGISTRADA
            UnidadDeMedida bod = db.UnidadesDeMedida.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionUnidadMedida.ToUpper().Trim() == UnidadDeMedida.DescripcionUnidadMedida.ToUpper().Trim());

            //SI YA EXISTE UNA DESCRIPCION DE U/M
            if (bod != null) {
                ModelState.AddModelError("DescripcionUnidadMedida", "Utilice otro nombre");
                mensaje = "La unidad de medida ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //ESTADO DE UNIDADES DE MEDIDA CUANDO SE CREA SIEMPRE ES TRUE
                    UnidadDeMedida.EstadoUnidadMedida = true;
                    if (ModelState.IsValid) {
                        db.UnidadesDeMedida.Add(UnidadDeMedida);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al almacenar";
                    transact.Rollback();
                }
            }

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        // GET: UnidadDeMedida/Edit/5
        public async Task<ActionResult> Edit(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnidadDeMedida UnidadDeMedida = await db.UnidadesDeMedida.FindAsync(id);
            if (UnidadDeMedida == null) {
                return HttpNotFound();
            }
            return View(UnidadDeMedida);
        }

        // POST: UnidadDeMedida/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CodigoUnidadMedida,DescripcionUnidadMedida,AbreviaturaUM,EstadoUnidadMedida")] UnidadDeMedida UnidadDeMedida) {
            //SE BUSCA UNIDADES DE MEDIDA REGISTRADAS CON LA DESCRIPCION REGISTRADA
            UnidadDeMedida bod = db.UnidadesDeMedida.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionUnidadMedida.Trim() == UnidadDeMedida.DescripcionUnidadMedida.Trim() && b.Id != UnidadDeMedida.Id);

            //SI YA EXISTE UNA DESCRIPCION DE U/M
            if (bod != null) {
                ModelState.AddModelError("DescripcionUnidadMedida", "Utilice otro nombre");
                mensaje = "La unidad de medida ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    if (ModelState.IsValid) {
                        db.Entry(UnidadDeMedida).State = EntityState.Modified;
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Modificado correctamente" : "Error al modificar";
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al modificar";
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// RETORNA EL CODIGO AUTOMATICAMENTE A LA VISTA CREATE
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchCode() {
            //BUSCAR EL VALOR MAXIMO DE LAS BODEGAS REGISTRADAS
            var code = db.UnidadesDeMedida.Max(x => x.CodigoUnidadMedida.Trim());
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
                if (valor >= 9 && valor < 100)
                    num = "0" + (valor + 1);
                else
                    num = (valor + 1).ToString();
            } else
                num = "001";//SE COMIENZA CON EL PRIMER CODIGO DEL REGISTRO

            return Json(num, JsonRequestBehavior.AllowGet);
        }

        // POST: UnidadDeMedida/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            var unidadDeMedida = db.UnidadesDeMedida.Find(id);
            //BUSCANDO QUE UNIDAD DE MEDIDA NO TENGA PRODUCTOS REGISTRADOS CON SU ID
            Producto oProd = db.Productos.DefaultIfEmpty(null).FirstOrDefault(p => p.UnidadMedidaId == unidadDeMedida.Id);

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //SI NO SE ENCUENTRAN PRODUCTOS ASOCIADOS AL ID
                    if (oProd == null) {
                        db.UnidadesDeMedida.Remove(unidadDeMedida);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Eliminado correctamente" : "Error al eliminar";
                    } else {
                        mensaje = "Se encontraron productos registrados a esta unidad de medida";
                    }

                    transact.Commit();
                } catch (Exception) {
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