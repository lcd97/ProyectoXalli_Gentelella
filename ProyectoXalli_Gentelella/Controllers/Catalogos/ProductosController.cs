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
    public class ProductosController : Controller {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        // GET: Productos
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// RECUPERA DATOS PARA LLENAR LA TABLA PRODUCTOS A TRAVES DE JSON
        /// </summary>
        /// <returns></returns>
        public JsonResult GetData() {
            var productos = (from obj in db.Productos
                             join u in db.CategoriasProducto on obj.CategoriaId equals u.Id
                             join c in db.UnidadesDeMedida on obj.UnidadMedidaId equals c.Id
                             where obj.EstadoProducto == true
                             select new {
                                 Id = obj.Id,
                                 NombreProducto = obj.NombreProducto + " " + obj.MarcaProducto,
                                 UnidadMedida = obj.PresentacionProducto == 1 ? c.DescripcionUnidadMedida : obj.PresentacionProducto + " " + c.DescripcionUnidadMedida,
                                 CodigoProducto = obj.CodigoProducto,
                                 Categoria = u.DescripcionCategoria
                             }).ToList();

            //await db.Productos.Join(u => u.UnidadDeMedida).join(c => c.Categoria).Where(c => c.EstadoProducto == true).ToListAsync();

            return Json(new { data = productos }, JsonRequestBehavior.AllowGet);
        }

        // GET: Productos/Create
        public ActionResult Create() {
            ViewBag.CategoriaId = new SelectList(db.CategoriasProducto, "Id", "DescripcionCategoria");
            ViewBag.UnidadMedidaId = new SelectList(db.UnidadesDeMedida, "Id", "DescripcionUnidadMedida");

            return View();
        }

        // POST: Productos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,CodigoProducto,NombreProducto,MarcaProducto,PresentacionProducto,CantidadMaxProducto,CantidadMinProducto,EstadoProducto,UnidadMedidaId,CategoriaId")] Producto Producto) {

            if (Producto.CantidadMaxProducto < Producto.CantidadMinProducto) {
                mensaje = "La cantidad mínima de producto no debe ser mayor a la cantidad máxima";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            //BUSCAR LA PRESENTACION DEL PRODUCTO (COMBINACION DE DESCRIPCION, MARCA Y UM - ID -)
            Producto bod = db.Productos.DefaultIfEmpty(null)
                .FirstOrDefault(b => b.NombreProducto.ToUpper().Trim() == Producto.NombreProducto.ToUpper().Trim() &&
                                b.MarcaProducto.ToUpper().Trim() == Producto.MarcaProducto.ToUpper().Trim() &&
                                b.UnidadMedidaId == Producto.UnidadMedidaId && b.CategoriaId == Producto.CategoriaId);

            //SI EL PRODUCTO YA EXISTE
            if (bod != null) {
                ModelState.AddModelError("NombreProducto", "La presentación del producto ya existe");
                mensaje = "La presentación del producto ya existe";
                return Json(new { success = completado, message = mensaje, Id = Producto.Id, Producto = 0 }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //ESTADO DE LA CATEGORIA CUANDO SE CREA SIEMPRE ES TRUE
                    Producto.EstadoProducto = true;
                    if (ModelState.IsValid) {
                        db.Productos.Add(Producto);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                    } else {
                        //ESTO ES PARA VER EL ERROR QUE DEVUELVE EL MODELO
                        string cad = "";
                        foreach (ModelState modelState in ViewData.ModelState.Values) {
                            foreach (ModelError error in modelState.Errors) {
                                cad += (error);
                            }
                        }

                        var errors = ModelState.Values.SelectMany(v => v.Errors);
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al almacenar";
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING

            //ESTO ES PARA AGREGARLO EN EL FORMULARIO DE ENTRADAS
            var um = db.UnidadesDeMedida.Find(Producto.UnidadMedidaId);
            var pro = Producto.NombreProducto + " " + Producto.MarcaProducto + " " + um.DescripcionUnidadMedida;

            return Json(new { success = completado, message = mensaje, Id = Producto.Id, Producto = pro }, JsonRequestBehavior.AllowGet);
        }

        // GET: CategoriasProducto/Edit/5
        public async Task<ActionResult> Edit(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto Producto = await db.Productos.FindAsync(id);
            if (Producto == null) {
                return HttpNotFound();
            }

            ViewBag.CategoriaId = new SelectList(db.CategoriasProducto, "Id", "DescripcionCategoria", Producto.CategoriaId);
            ViewBag.UnidadMedidaId = new SelectList(db.UnidadesDeMedida, "Id", "DescripcionUnidadMedida", Producto.UnidadMedidaId);

            return View(Producto);
        }

        // POST: CategoriasProducto/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CodigoProducto,NombreProducto,MarcaProducto,PresentacionProducto,CantidadMaxProducto,CantidadMinProducto,EstadoProducto,UnidadMedidaId,CategoriaId")] Producto Producto) {
            //BUSCAR LA PRESENTACION DEL PRODUCTO (COMBINACION DE DESCRIPCION, MARCA Y UM - ID -)
            Producto bod = db.Productos.DefaultIfEmpty(null)
                .FirstOrDefault(b => b.NombreProducto.ToUpper().Trim() == Producto.NombreProducto.ToUpper().Trim() &&
                                b.MarcaProducto.ToUpper().Trim() == Producto.MarcaProducto.ToUpper().Trim() &&
                                b.UnidadMedidaId == Producto.UnidadMedidaId && b.Id != Producto.Id);

            //SI EL PRODUCTO YA EXISTE
            if (bod != null) {
                ModelState.AddModelError("NombreProducto", "La presentación del producto ya existe");
                mensaje = "El producto con esas características ya existe";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    if (ModelState.IsValid) {
                        db.Entry(Producto).State = EntityState.Modified;
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Modificado correctamente" : "Error al modificar";
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
                    mensaje = "Error al modificar";
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        // GET: CategoriasProducto/Details/5
        public async Task<ActionResult> Details(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto producto = await db.Productos.FindAsync(id);
            if (producto == null) {
                return HttpNotFound();
            }

            return View(producto);
        }

        /// <summary>
        /// OBTIENE DETALLES DE UN OBJETO PRODUCTO
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult getDetails(int id) {
            var producto = (from obj in db.Productos
                            join c in db.CategoriasProducto on obj.CategoriaId equals c.Id
                            join u in db.UnidadesDeMedida on obj.UnidadMedidaId equals u.Id
                            where obj.Id == id
                            select new {
                                CodigoProducto = obj.CodigoProducto,
                                NombreProducto = obj.NombreProducto,
                                MarcaProducto = obj.MarcaProducto,
                                CantidadMaxProducto = obj.CantidadMaxProducto,
                                CantidadMinProducto = obj.CantidadMinProducto,
                                EstadoProducto = obj.EstadoProducto,
                                UnidadMedida = u.AbreviaturaUM,
                                Categoria = c.DescripcionCategoria
                            }).FirstOrDefault();

            return Json(producto, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// DEVUELVE LA CANTIDAD ACTUAL DEL PRODUCTO Y DONDE SE ALMACENA (SOLO SE LLEVARA EL CONTROL DEL BAR)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CantidadActual(int id) {

            //DEVULEVE LA CANTIDAD DE EXISTENCIA DEL PRODUCTO
            var cantActual = (from bod in db.Bodegas
                              join ent in db.Entradas on bod.Id equals ent.BodegaId
                              join det in db.DetallesDeEntrada on ent.Id equals det.EntradaId
                              join pro in db.Productos on det.ProductoId equals pro.Id
                              where pro.Id == id && bod.DescripcionBodega.ToUpper() == "BAR"
                              group new { bod, det } by new { bod.Id } into grouped
                              select grouped.Sum(b => b.det.CantidadEntrada)).FirstOrDefault();

            return Json(cantActual, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// RETORNA EL CODIGO AUTOMATICAMENTE A LA VISTA CREATE
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchCode() {
            //BUSCAR EL VALOR MAXIMO DE LAS BODEGAS REGISTRADAS
            var code = db.Productos.Max(x => x.CodigoProducto.Trim());
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

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            var Producto = db.Productos.Find(id);
            //BUSCANDO QUE PRODUCTO NO TENGA SALIDAS NI ENTRADAS REGISTRADAS CON SU ID
            DetalleDeEntrada oEnt = db.DetallesDeEntrada.DefaultIfEmpty(null).FirstOrDefault(e => e.ProductoId == Producto.Id);
            Ingrediente oIng = db.Ingredientes.DefaultIfEmpty(null).FirstOrDefault(s => s.ProductoId == Producto.Id);

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //SI NO SE ENCUENTRAN SALIDAS O ENTRADAS
                    if (oEnt == null || oIng == null) {
                        db.Productos.Remove(Producto);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Eliminado correctamente" : "Error al eliminar";
                    } else {
                        mensaje = "Se encontraron movimientos asociados a este producto";
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