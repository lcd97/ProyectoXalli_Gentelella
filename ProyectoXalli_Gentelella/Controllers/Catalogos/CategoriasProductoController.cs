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
    public class CategoriasProductoController : Controller
    {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        // GET: CategoriasProducto
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// RECUPERA DATOS PARA LLENAR LA TABLA CATEGORIAS A TRAVES DE JSON
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetData() {
            db.Configuration.ProxyCreationEnabled = false;
            var categorias = await db.CategoriasProducto.Where(c => c.EstadoCategoria == true).ToListAsync();

            return Json(new { data = categorias }, JsonRequestBehavior.AllowGet);
        }

        // GET: CategoriasProducto/Details/5
        public async Task<ActionResult> Details(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CategoriaProducto categorias = await db.CategoriasProducto.FindAsync(id);
            if (categorias == null) {
                return HttpNotFound();
            }
            return View(categorias);
        }

        // GET: CategoriasProducto/Create
        public ActionResult Create() {
            return View();
        }

        // POST: CategoriasProducto/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,CodigoCategoria,DescripcionCategoria,EstadoCategoria")] CategoriaProducto CategoriaProducto) {
            //BUSCAR QUE EXISTA UNA CATEGORIA CON ESA DESCRIPCION
            CategoriaProducto bod = db.CategoriasProducto.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionCategoria.ToUpper().Trim() == CategoriaProducto.DescripcionCategoria.ToUpper().Trim());

            //SI EXISTE INGRESADO UNA CATEGORIA CON LA DESCRIPCION
            if (bod != null) {
                ModelState.AddModelError("DescripcionCategoria", "Utilice otro nombre");
                mensaje = "La descripción ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //ESTADO DE LA CATEGORIA CUANDO SE CREA SIEMPRE ES TRUE
                    CategoriaProducto.EstadoCategoria = true;
                    if (ModelState.IsValid) {
                        db.CategoriasProducto.Add(CategoriaProducto);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                    }
                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al almacenar";
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        // GET: CategoriasProducto/Edit/5
        public async Task<ActionResult> Edit(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CategoriaProducto CategoriaProducto = await db.CategoriasProducto.FindAsync(id);
            if (CategoriaProducto == null) {
                return HttpNotFound();
            }
            return View(CategoriaProducto);
        }

        // POST: CategoriasProducto/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CodigoCategoria,DescripcionCategoria,EstadoCategoria")] CategoriaProducto CategoriaProducto) {
            //BUSCAR QUE EXISTA UNA CATEGORIA CON ESA DESCRIPCION
            CategoriaProducto bod = db.CategoriasProducto.DefaultIfEmpty(null).FirstOrDefault(b => b.DescripcionCategoria.ToUpper().Trim() == CategoriaProducto.DescripcionCategoria.ToUpper().Trim() && b.Id != CategoriaProducto.Id);

            //SI EXISTE INGRESADO UNA CATEGORIA CON LA DESCRIPCION
            if (bod != null) {
                ModelState.AddModelError("DescripcionCategoria", "Utilice otro nombre");
                mensaje = "La descripción ya se encuentra registrada";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {

                try {
                    if (ModelState.IsValid) {
                        db.Entry(CategoriaProducto).State = EntityState.Modified;
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Modificado correctamente" : "Error al modificar";
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al modificar";
                    transact.Rollback();
                }
            }//FIN USING

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// RETORNA EL CODIGO AUTOMATICAMENTE A LA VISTA CREATE
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchCode() {
            //BUSCAR EL VALOR MAXIMO DE LAS BODEGAS REGISTRADAS
            var code = db.CategoriasProducto.Max(x => x.CodigoCategoria.Trim());
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

        // POST: CategoriasProducto/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            var CategoriaProducto = db.CategoriasProducto.Find(id);
            //BUSCANDO QUE CATEGORIA PRODUCTOS REGISTRADAS CON SU ID
            Producto oProd = db.Productos.DefaultIfEmpty(null).FirstOrDefault(p => p.CategoriaId == CategoriaProducto.Id);

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //SI NO SE ENCOTRARON PRODUCTOS EN LA CATEGORIA
                    if (oProd == null) {
                        db.CategoriasProducto.Remove(CategoriaProducto);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Eliminado correctamente" : "Error al eliminar";
                    } else {
                        mensaje = "Se encontraron productos registrados a esta categoría";
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