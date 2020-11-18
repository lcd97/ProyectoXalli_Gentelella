using Newtonsoft.Json;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Movimientos {

    [Authorize]
    public class EntradasController : Controller {
        private DBControl db = new DBControl();
        bool completado = false;
        string mensaje = "";

        [Authorize(Roles = "Admin, Cocinero, Recepcionista")]
        /// <summary>
        /// MUESTRA INDEX DE ENTRADAS RESTAURANTE
        /// </summary>
        /// <returns></returns>
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// OBTIENE UNA LISTA DE TODOS LOS PROVEEDORES
        /// </summary>
        /// <returns></returns>
        public ActionResult getProveedor() {
            var provider = from obj in db.Proveedores.ToList()
                           join u in db.Datos.ToList() on obj.DatoId equals u.Id
                           where obj.EstadoProveedor == true
                           select new {
                               //CONSULTA PARA ASIGNARLE A LA VARIABLE PROVEEDOR EL NOMBRE COMERCIAL O NOMBRE DE LA PERSONA NATURAL
                               Proveedor = obj.NombreComercial != null ? obj.NombreComercial : u.PNombre + " " + u.PApellido,
                               Id = obj.Id
                           };

            return Json(new { data = provider }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// OBTIENE UNA LISTA DE TODOS LOS TIPOS DE ENTRADA
        /// </summary>
        /// <returns></returns>
        public ActionResult getTipoEntrada() {

            var entrada = from obj in db.TiposDeEntrada.ToList()
                          where obj.EstadoTipoEntrada == true
                          select new {
                              //CONSULTA PARA ASIGNARLE A LA VARIABLE PROVEEDOR EL NOMBRE COMERCIAL O NOMBRE DE LA PERSONA NATURAL
                              Entrada = obj.DescripcionTipoEntrada,
                              Id = obj.Id
                          };

            return Json(new { data = entrada }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// OBTIENE UNA LISTA DE LOS PRODUCTOS ALMACENADOS
        /// </summary>
        /// <returns></returns>
        public ActionResult getProductos() {
            var producto = from obj in db.Productos.ToList()
                           join um in db.UnidadesDeMedida.ToList() on obj.UnidadMedidaId equals um.Id
                           where obj.EstadoProducto == true
                           select new {
                               //CONSULTA PARA ASIGNARLE A LA VARIABLE PROVEEDOR EL NOMBRE COMERCIAL O NOMBRE DE LA PERSONA NATURAL
                               Presentacion = obj.MarcaProducto != null && obj.PresentacionProducto != 1 ?
                               obj.NombreProducto + " - " + obj.MarcaProducto + " - " + obj.PresentacionProducto + " " + um.AbreviaturaUM :
                               obj.NombreProducto + " - " + um.AbreviaturaUM,
                               Id = obj.Id
                           };

            return Json(new { data = producto }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// OBTIENE UNA LISTA DE LAS BODEGAS REGISTRADAS
        /// </summary>
        /// <returns></returns>
        public ActionResult getArea() {
            var bodegas = from obj in db.Bodegas.ToList()
                          where obj.EstadoBodega == true
                          select new {
                              //CONSULTA PARA ASIGNARLE A LA VARIABLE PROVEEDOR EL NOMBRE COMERCIAL O NOMBRE DE LA PERSONA NATURAL
                              Bodega = obj.DescripcionBodega,
                              Id = obj.Id
                          };

            return Json(new { data = bodegas }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// ALMACENA LA ENTRADA NUEVA
        /// </summary>
        /// <param name="Codigo"></param>
        /// <param name="FechaEntrada"></param>
        /// <param name="TipoEntradaId"></param>
        /// <param name="BodegaId"></param>
        /// <param name="ProveedorId"></param>
        /// <param name="detalleEntrada"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddEntrada(string Codigo, DateTime FechaEntrada, int TipoEntradaId, int BodegaId, int ProveedorId, string detalleEntrada) {

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    var DetalleEntrada = JsonConvert.DeserializeObject<List<DetalleDeEntrada>>(detalleEntrada);

                    //GUARDAR PRIMERO EL ENCABEZAADO
                    Entrada entrada = new Entrada();

                    entrada.CodigoEntrada = Codigo;
                    entrada.FechaEntrada = Convert.ToDateTime(FechaEntrada);
                    entrada.TipoEntradaId = TipoEntradaId;
                    entrada.BodegaId = BodegaId;
                    entrada.ProveedorId = ProveedorId;
                    entrada.EstadoEntrada = true;

                    db.Entradas.Add(entrada);

                    if (db.SaveChanges() > 0) {

                        foreach (var item in DetalleEntrada) {
                            DetalleDeEntrada detalleItem = new DetalleDeEntrada();

                            detalleItem.CantidadEntrada = item.CantidadEntrada;
                            detalleItem.PrecioEntrada = item.PrecioEntrada;
                            detalleItem.ProductoId = item.ProductoId;
                            detalleItem.EntradaId = entrada.Id;

                            db.DetallesDeEntrada.Add(detalleItem);
                            completado = db.SaveChanges() > 0 ? true : false;
                        }

                        mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";

                    }

                    transact.Commit();
                } catch (Exception) {
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// RETORNA EL CODIGO DE ENTRADA AUTOMATICAMENTE
        /// </summary>
        /// <returns></returns>
        public ActionResult EntradaCode() {
            var code = db.Entradas.Max(x => x.CodigoEntrada.Trim());
            int valor;
            string num;

            if (code != null) {

                valor = int.Parse(code);

                if (valor <= 8)
                    num = "00" + (valor + 1);
                else
                if (valor >= 9 && valor < 100)
                    num = "0" + (valor + 1);
                else
                    num = (valor + 1).ToString();
            } else
                num = "001";

            return Json(num, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}