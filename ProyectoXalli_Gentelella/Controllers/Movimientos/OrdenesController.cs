using Newtonsoft.Json;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Movimientos {
    public class OrdenesController : Controller {
        private DBControl db = new DBControl();
        private string mensaje = "";
        private bool completado = false;

        //CONEXION A LA BASE DE DATOS SEGURIDAD
        private ApplicationDbContext context = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        // GET: Ordenes
        public ActionResult Index() {
            ViewBag.CategoriaId = new SelectList(db.CategoriasMenu, "Id", "DescripcionCategoriaMenu");

            return View();
        }

        /// <summary>
        /// RETORNA EL CODIGO DE ENTRADA AUTOMATICAMENTE
        /// </summary>
        /// <returns></returns>
        public ActionResult OrdenesCode() {
            //BUSCAR EL VALLOR MAXIMO ALMACENADO DE CODIGO
            var num = (from obj in db.Ordenes
                       select obj.CodigoOrden).DefaultIfEmpty().Max();

            int codigo = 1;

            if (num != 0) {
                codigo = num + 1;
            }

            return Json(codigo, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// RECUPERA TODOS LOS PLATILLOS POR CATEGORIA
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MenuByCategoria(int id) {
            var menu = from obj in db.Menus.ToList()
                       join i in db.Imagenes.ToList() on obj.ImagenId equals i.Id
                       where obj.CategoriaMenuId == id
                       select new {
                           Id = obj.Id,
                           Platillo = obj.DescripcionMenu,
                           Precio = obj.PrecioMenu,
                           Imagen = i.Ruta
                       };

            return Json(menu, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// RECUPERA LOS DATOS DEL CLIENTE POR MEDIO DE LA IDENTIFICACION
        /// </summary>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        public ActionResult DataClient(string identificacion) {
            var cliente = (from obj in db.Datos
                           join c in db.Clientes on obj.Id equals c.DatoId
                           where obj.Cedula.Trim() == identificacion.Trim() || c.PasaporteCliente.Trim() == identificacion.Trim()
                           select new {
                               DatoId = obj.Id,
                               ClienteId = obj.Id,
                               Nombres = obj.PNombre + " " + obj.PApellido,
                               RUC = obj.RUC != null ? obj.RUC : null,
                               Documento = obj.Cedula != null ? obj.Cedula : c.PasaporteCliente
                           }).FirstOrDefault();

            return Json(cliente, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// MUESTRA LA VISTA DEL DETALLE DE ORDEN
        /// </summary>
        /// <returns></returns>
        public ActionResult DetalleOrden() {
            return View();
        }

        /// <summary>
        /// ALMACENAR UNA NUEVA ORDEN
        /// </summary>
        /// <param name="Codigo"></param>
        /// <param name="MeseroId"></param>
        /// <param name="ClienteId"></param>
        /// <param name="FechaOrden"></param>
        /// <param name="DetalleOrden"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(int Codigo, int MeseroId, int ClienteId, DateTime FechaOrden, string detalleOrden) {

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //DESERIALIZACION DE OBJETO
                    var DetalleOrden = JsonConvert.DeserializeObject<List<DetalleDeOrden>>(detalleOrden);

                    Orden orden = new Orden();

                    orden.CodigoOrden = Codigo;
                    orden.FechaOrden = Convert.ToDateTime(FechaOrden);
                    orden.EstadoOrden = 1;//1 ORDENADA 2 SIN FACTURAR 3 FACTURADA
                    orden.MeseroId = MeseroId;
                    orden.ClienteId = ClienteId;
                    orden.ImagenId = 1;

                    //GUARDAR LA ORDEN
                    db.Ordenes.Add(orden);

                    //SI CAMBIOS SE REALIZO BIEN
                    if (db.SaveChanges() > 0) {
                        //ALMACENAR EL DETALLE DE LA ORDEN
                        foreach (var item in DetalleOrden) {
                            DetalleDeOrden detalleItem = new DetalleDeOrden();

                            detalleItem.CantidadOrden = item.CantidadOrden;
                            detalleItem.NotaDetalleOrden = item.NotaDetalleOrden != "" ? item.NotaDetalleOrden.Trim() : null;
                            detalleItem.PrecioOrden = item.PrecioOrden;
                            detalleItem.MenuId = item.MenuId;
                            detalleItem.OrdenId = orden.Id;
                            detalleItem.EstadoDetalleOrden = true;

                            db.DetallesDeOrden.Add(detalleItem);

                            completado = db.SaveChanges() > 0 ? true : false;
                        }

                        mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al almacenar orden";
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING
            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// BUSCA EL EMPLEADO LOGEADO
        /// </summary>
        /// <param name="LoginId"></param>
        /// <returns></returns>
        public ActionResult LoggedUser(string LoginId) {
            var user = context.Users.FirstOrDefault(u => u.Id == LoginId);

            var data = (from obj in db.Datos
                        join m in db.Meseros on obj.Id equals m.DatoId
                        where m.Id == user.PeopleId
                        select new {
                            MeseroId = m.Id,
                            Nombre = obj.PNombre + " " + obj.PApellido
                        }).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}