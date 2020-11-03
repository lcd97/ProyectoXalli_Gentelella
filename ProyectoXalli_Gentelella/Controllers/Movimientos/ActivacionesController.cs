using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Movimientos {

    [Authorize]
    public class ActivacionesController : Controller {
        private DBControl db = new DBControl();

        [Authorize(Roles = "Admin")]
        // GET: Activaciones
        public ActionResult Index() {
            return View();
        }

        /**************************************************
         *          MODULO CONTROL DE INSUMOS           */

        /// <summary>
        /// LISTA TODAS LAS BODEGAS DESACTIVADAS
        /// </summary>
        /// <returns></returns>
        public JsonResult getBodegas() {
            var bodegas = (from obj in db.Bodegas.ToList()
                           where obj.EstadoBodega == false
                           select new {
                               Id = obj.Id,
                               Descripcion = obj.DescripcionBodega,
                               Codigo = obj.CodigoBodega
                           });

            return Json(new { data = bodegas }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// LISTA TODAS LAS CATEGORIAS DESACTIVADAS
        /// </summary>
        /// <returns></returns>
        public JsonResult getCategoriasProducto() {
            var categorias = (from obj in db.CategoriasProducto.ToList()
                              where obj.EstadoCategoria == false
                              select new {
                                  Id = obj.Id,
                                  Descripcion = obj.DescripcionCategoria,
                                  Codigo = obj.CodigoCategoria
                              });

            return Json(new { data = categorias }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// LISTA TODAS LAS TIPOS DE ENTRADAS DESACTIVADAS
        /// </summary>
        /// <returns></returns>
        public JsonResult getTiposDeEntrada() {
            var tiposEntrada = (from obj in db.TiposDeEntrada.ToList()
                                where obj.EstadoTipoEntrada == false
                                select new {
                                    Id = obj.Id,
                                    Descripcion = obj.DescripcionTipoEntrada,
                                    Codigo = obj.CodigoTipoEntrada
                                });

            return Json(new { data = tiposEntrada }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// LISTA TODAS LAS TIPOS DE ENTRADAS DESACTIVADAS
        /// </summary>
        /// <returns></returns>
        public JsonResult getUnidadesDeMedida() {
            var um = (from obj in db.UnidadesDeMedida.ToList()
                      where obj.EstadoUnidadMedida == false
                      select new {
                          Id = obj.Id,
                          Descripcion = obj.DescripcionUnidadMedida,
                          Codigo = obj.CodigoUnidadMedida
                      });

            return Json(new { data = um }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// LISTA TODAS LOS PRODUCTOS DESACTIVADAS
        /// </summary>
        /// <returns></returns>
        public JsonResult getProductos() {
            var um = (from obj in db.Productos.ToList()
                      where obj.EstadoProducto == false
                      select new {
                          Id = obj.Id,
                          Descripcion = obj.NombreProducto,
                          Codigo = obj.CodigoProducto
                      });

            return Json(new { data = um }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// LISTA TODAS LOS PROVEEDORES DESACTIVADAS
        /// </summary>
        /// <returns></returns>
        public JsonResult getProveedores() {
            var um = (from obj in db.Proveedores.ToList()
                      join u in db.Datos.ToList() on obj.DatoId equals u.Id
                      where obj.EstadoProveedor == false
                      select new {
                          Id = obj.Id,
                          //CONSULTA PARA ASIGNARLE A LA VARIABLE DESCRIPCION EL NOMBRE COMERCIAL O NOMBRE DE LA PERSONA NATURAL
                          Descripcion = obj.NombreComercial != null ? obj.NombreComercial : u.PNombre + " " + u.PApellido,
                          Codigo = u.RUC != null ? u.RUC : u.Cedula
                      });

            return Json(new { data = um }, JsonRequestBehavior.AllowGet);
        }

        /**************************************************
         *                  MODULO MENU                 */
        /// <summary>
        /// LISTA TODAS LAS CATEGORIAS DESACTIVADAS
        /// </summary>
        /// <returns></returns>
        public JsonResult getCategoriasMenu() {
            var categorias = (from obj in db.CategoriasMenu.ToList()
                              where obj.EstadoCategoriaMenu == false
                              select new {
                                  Id = obj.Id,
                                  Descripcion = obj.DescripcionCategoriaMenu,
                                  Codigo = obj.CodigoCategoriaMenu
                              });

            return Json(new { data = categorias }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// LISTA TODAS LOS ITEMS DEL MENU (PLATILLOS)
        /// </summary>
        /// <returns></returns>
        public JsonResult getMenus() {
            var platillos = (from obj in db.Menus.ToList()
                             join i in db.Imagenes.ToList() on obj.ImagenId equals i.Id
                             where obj.EstadoMenu == false
                             select new {
                                 Id = obj.Id,
                                 DescripcionPlatillo = obj.DescripcionMenu,
                                 Precio = obj.PrecioMenu,
                                 Imagen = i.Ruta
                             });

            return Json(platillos, JsonRequestBehavior.AllowGet);
        }

        /**************************************************
         *                MODULO ORDENES                 */
        public JsonResult getMeseros() {
            var meseros = (from obj in db.Meseros.ToList()
                           join d in db.Datos.ToList() on obj.DatoId equals d.Id
                           where obj.EstadoMesero == false
                           select new {
                               Id = obj.Id,
                               Descripcion = d.PNombre + " " + d.PApellido,
                               Codigo = d.Cedula.Trim()
                           });

            return Json(new { data = meseros }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getClientes() {
            var meseros = (from obj in db.Clientes.ToList()
                           join d in db.Datos.ToList() on obj.DatoId equals d.Id
                           where obj.EstadoCliente == false
                           select new {
                               Id = obj.Id,
                               Descripcion = d.PNombre + " " + d.PApellido,
                               Codigo = d.Cedula.Trim()
                           });

            return Json(new { data = meseros }, JsonRequestBehavior.AllowGet);
        }

        /**************************************************
         *            MODULO FACTURACION                 */
        public JsonResult getTiposDePago() {
            var pagos = (from obj in db.TiposDePago.ToList()
                         where obj.EstadoTipoPago == false
                         select new {
                             Id = obj.Id,
                             Descripcion = obj.DescripcionTipoPago,
                             Codigo = obj.CodigoTipoPago.Trim()
                         });

            return Json(new { data = pagos }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getMonedas() {
            var moneda = (from obj in db.Monedas.ToList()
                         where obj.EstadoMoneda == false
                         select new {
                             Id = obj.Id,
                             Descripcion = obj.DescripcionMoneda,
                             Codigo = obj.CodigoMoneda.Trim()
                         });

            return Json(new { data = moneda }, JsonRequestBehavior.AllowGet);
        }
    }
}