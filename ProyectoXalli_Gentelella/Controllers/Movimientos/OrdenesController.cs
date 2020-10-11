using Newtonsoft.Json;
using ProyectoXalli_Gentelella.Models;
using ProyectoXalli_Gentelella.Web_Sockets;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Movimientos {

    [Authorize]
    public class OrdenesController : Controller {
        private DBControl db = new DBControl();
        private string mensaje = "";
        private bool completado = false;

        //CONEXION A LA BASE DE DATOS SEGURIDAD
        private ApplicationDbContext context = new ApplicationDbContext();

        [Authorize(Roles = "Admin, Mesero")]
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
            string mensaje = "";
            var validacionOrden = (dynamic)null;

            var cliente = (from obj in db.Datos
                           join c in db.Clientes on obj.Id equals c.DatoId
                           where obj.Cedula.Trim() == identificacion.Trim() || c.PasaporteCliente.Trim() == identificacion.Trim()
                           select new {
                               DatoId = obj.Id,
                               ClienteId = c.Id,
                               Nombres = obj.PNombre + " " + obj.PApellido,
                               RUC = obj.RUC != null ? obj.RUC : null,
                               Documento = obj.Cedula != null ? obj.Cedula : c.PasaporteCliente
                           }).FirstOrDefault();

            if (cliente != null) {
                validacionOrden = (from obj in db.Clientes
                                   join c in db.Ordenes on obj.Id equals c.ClienteId
                                   where c.EstadoOrden == 1 && c.ClienteId == cliente.ClienteId
                                   select obj).FirstOrDefault();
            }

            mensaje = validacionOrden != null ? "Ya existe una orden activa asociada al cliente" : "-1";

            return Json(new { cliente, mensaje }, JsonRequestBehavior.AllowGet);
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

                    Dato plantilla1 = new Dato();
                    Cliente plantilla2 = new Cliente();

                    Cliente clientePlantilla = new Cliente();

                    orden.CodigoOrden = Codigo;
                    orden.FechaOrden = Convert.ToDateTime(FechaOrden);
                    orden.EstadoOrden = 1;//1 ORDENADA 2 SIN FACTURAR 3 FACTURADA
                    orden.MeseroId = MeseroId;

                    //SI EL CLIENTE ES VISITANTE
                    if (ClienteId == 0) {
                        var buscarP = db.Datos.DefaultIfEmpty(null).FirstOrDefault(b => b.Cedula == "000-000000-0000X");

                        //SI NO EXISTE LA PLANTILLA
                        if (buscarP == null) {
                            plantilla1.Cedula = "000-000000-0000X";
                            plantilla1.PNombre = "Default";
                            plantilla1.PApellido = "User";

                            db.Datos.Add(plantilla1);
                            db.SaveChanges();

                            plantilla2.DatoId = plantilla1.Id;
                            plantilla2.EmailCliente = "defaultuser@xalli.com";
                            plantilla2.EstadoCliente = false;

                            db.Clientes.Add(plantilla2);
                            db.SaveChanges();

                            clientePlantilla = plantilla2;
                        } else {
                            clientePlantilla = db.Clientes.FirstOrDefault(c => c.DatoId == buscarP.Id);
                        }

                        orden.ClienteId = clientePlantilla.Id;

                    } else {
                        //SI EL CLIENTE ES HUESPED
                        orden.ClienteId = ClienteId;
                    }

                    var comandaCero = db.Imagenes.DefaultIfEmpty(null).FirstOrDefault(i => i.Ruta == "Comanda");
                    Imagen img = new Imagen();

                    if (comandaCero == null) {
                        img.Ruta = "Comanda";
                        db.Imagenes.Add(img);
                        db.SaveChanges();
                    }
                    //ALMACENAMOS UNA PLANTILLA
                    orden.ImagenId = comandaCero != null ? comandaCero.Id : img.Id;

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

        [Authorize(Roles = "Admin, Mesero")]
        /// <summary>
        /// RETORNA LA VISTA DE ORDENES
        /// </summary>
        /// <returns></returns>
        public ActionResult VerOrdenes() {
            return View();
        }

        /// <summary>
        /// MUESTRA LA VISTA DE LA COMANDA
        /// </summary>
        /// <returns></returns>
        public ActionResult Comanda(int OrderId) {
            ViewBag.OrdenId = OrderId;//ENVIAR EL ID DE LA ORDEN A MOSTRAR

            return View();
        }

        /// <summary>
        /// MUESTRA LA LISTA DE TODAS LAS ORDENES
        /// </summary>
        /// <returns></returns>
        public ActionResult Ordenes(int empleadoId, string EmpleadoRol) {
            var orden = (dynamic)null;

            if (EmpleadoRol == "Mesero") {
                orden = (from obj in db.Ordenes.ToList()
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         where obj.EstadoOrden == 1 && obj.MeseroId == empleadoId
                         select new {
                             OrdenId = obj.Id,
                             CodigoOrden = obj.CodigoOrden,
                             HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
                             Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
                             Mesero = (from o in db.Ordenes
                                       join m in db.Meseros on o.MeseroId equals m.Id
                                       join a in db.Datos on m.DatoId equals a.Id
                                       where obj.Id == o.Id
                                       select a.PNombre + " " + a.PApellido).FirstOrDefault()
                         }).ToList();
            } else {
                orden = (from obj in db.Ordenes.ToList()
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         where obj.EstadoOrden == 1
                         select new {
                             OrdenId = obj.Id,
                             CodigoOrden = obj.CodigoOrden,
                             HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
                             Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
                             Mesero = (from o in db.Ordenes
                                       join m in db.Meseros on o.MeseroId equals m.Id
                                       join a in db.Datos on m.DatoId equals a.Id
                                       where obj.Id == o.Id
                                       select a.PNombre + " " + a.PApellido).FirstOrDefault()
                         }).ToList();
            }

            return Json(orden, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// MUESTRA LA VISTA PARA AGREGAR NUEVOS ITEMS DEL MENU
        /// </summary>
        /// <param name="OrderId"></param>
        /// <returns></returns>
        public ActionResult Edit(int OrderId) {
            ViewBag.OrdenId = OrderId;//ENVIAR EL ID DE LA ORDEN A MOSTRAR
            ViewBag.CategoriaId = new SelectList(db.CategoriasMenu, "Id", "DescripcionCategoriaMenu");

            return View();
        }

        /// <summary>
        /// OBTENER EL DETALLE DE LA ORDEN
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult getOrderAndDetails(int orderId) {

            //BUSCAR EL ENCABEZADO
            var OrderHead = (from obj in db.Ordenes.ToList()
                             join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                             join d in db.Datos.ToList() on c.DatoId equals d.Id
                             where obj.Id == orderId
                             select new {
                                 Id = obj.Id,
                                 Codigo = obj.CodigoOrden,
                                 Fecha = (obj.FechaOrden.Day < 10 ? "0" + obj.FechaOrden.Day.ToString() : obj.FechaOrden.Day.ToString()) + "/" +
                                        (obj.FechaOrden.Month < 10 ? "0" + obj.FechaOrden.Month.ToString() : obj.FechaOrden.Month.ToString()) + "/" +
                                       (obj.FechaOrden.Year.ToString()),
                                 TipoCliente = d.PNombre.Trim().ToUpper() == "DEFAULT" ? 0 : 1,
                                 ClienteId = c.Id,
                                 ClienteIdent = d.Cedula != null ? d.Cedula.Trim() : c.PasaporteCliente,
                                 Nombres = d.PNombre != null ? d.PNombre.Trim() + " " + d.PApellido.Trim() : null,
                                 RUC = d.RUC,
                                 Correo = c.EmailCliente,
                                 Telefono = c.TelefonoCliente,
                                 Mesero = (from o in db.Ordenes
                                           join m in db.Meseros on o.MeseroId equals m.Id
                                           join a in db.Datos on m.DatoId equals a.Id
                                           where obj.Id == o.Id
                                           select a.PNombre + " " + a.PApellido).FirstOrDefault()
                             }).FirstOrDefault();

            //DETALLE DE LA ORDEN
            var OrderDetails = (from obj in db.Ordenes
                                join d in db.DetallesDeOrden on obj.Id equals d.OrdenId
                                join p in db.Menus on d.MenuId equals p.Id
                                where d.OrdenId == orderId
                                select new {
                                    PlatilloId = d.MenuId,
                                    Platillo = p.DescripcionMenu.Trim(),
                                    PrecioUnitario = d.PrecioOrden,
                                    Cantidad = d.CantidadOrden,
                                    Nota = d.NotaDetalleOrden,
                                    Estado = d.EstadoDetalleOrden
                                }).ToList();

            return Json(new { Principal = OrderHead, Details = OrderDetails }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// ENVIA UN MENSJAE CON LA COMANDA AL CORREO ELECTRONICO
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Cliente"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendEmail(string Email, string Cliente) {
            string emailHotel = "proyectoshotel2020@gmail.com";
            string passwordHotel = "2020canelaazul";
            string error = "ENVIADO";

            try {
                MailMessage correo = new MailMessage();
                correo.From = new MailAddress(emailHotel);
                correo.To.Add("danycordero9@gmail.com");
                correo.Subject = "Pruebas de campo";
                correo.Body = "Este mensaje es de prueba";
                correo.IsBodyHtml = true;
                correo.Priority = MailPriority.Normal;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(emailHotel, passwordHotel);

                smtp.Send(correo);

            } catch (Exception ex) {

                error = ex.Message;
            }

            return Json(error, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// ACTUALIZA LOS ELEMENTOS DE UNA ORDEN EN ESPECIFICO
        /// </summary>
        /// <param name="Codigo"></param>
        /// <param name="FechaOrden"></param>
        /// <param name="detalleOrden"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditOrder(int Codigo, DateTime FechaOrden, int EstadoOrden, string detalleOrden) {
            //BUSCAR LA ORDEN
            var Orden = db.Ordenes.DefaultIfEmpty(null).FirstOrDefault(o => o.CodigoOrden == Codigo);

            if (Orden != null) {
                using (var transact = db.Database.BeginTransaction()) {
                    try {
                        //DESERIALIZACION DE OBJETO
                        var DetalleOrden = JsonConvert.DeserializeObject<List<DetalleDeOrden>>(detalleOrden);

                        Orden.FechaOrden = Convert.ToDateTime(FechaOrden);
                        Orden.EstadoOrden = EstadoOrden;

                        db.Entry(Orden).State = EntityState.Modified;
                        completado = db.SaveChanges() > 0 ? true : false;

                        if (completado) {
                            //BUSCAR EL DETALLE DE LA ORDEN
                            var details = db.DetallesDeOrden.FirstOrDefault(d => d.OrdenId == Orden.Id);

                            foreach (var item in DetalleOrden) {
                                DetalleDeOrden detalleItem = new DetalleDeOrden();

                                //SI EL ESTADO DE LA LISTA DE CADA ITEM ES ACTIVO
                                if (item.EstadoDetalleOrden == true) {
                                    detalleItem.CantidadOrden = item.CantidadOrden;
                                    detalleItem.NotaDetalleOrden = item.NotaDetalleOrden != "" ? item.NotaDetalleOrden.Trim() : null;
                                    detalleItem.PrecioOrden = item.PrecioOrden;
                                    detalleItem.MenuId = item.MenuId;
                                    detalleItem.OrdenId = Orden.Id;
                                    detalleItem.EstadoDetalleOrden = true;

                                    db.DetallesDeOrden.Add(detalleItem);
                                    completado = db.SaveChanges() > 0 ? true : false;
                                }
                            }

                            mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                        }

                        transact.Commit();
                    } catch (Exception) {
                        transact.Rollback();
                        mensaje = "Error al almacenar";
                    }//FIN TRY-CATCH
                }//FIN USING
            }//FIN ORDEN DIF NULL

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// CONVERTIR LA HORA DE 24HRS A HORA LOCAL
        /// </summary>
        /// <returns></returns>
        public string ConvertHour(int Hora, int Minuto) {
            //CONVIERTE LA HORA DE FORMATO 24 A FORMATO 12
            int hour = (Hora + 11) % 12 + 1;
            string Meridiano = Hora > 12 ? "PM" : "AM";

            //AGREGAR UN 0 A LA HORA O MINUTO SI EL VALOR ES MENOR A 10
            string horaEnviar = (hour < 10 ? "0" + hour.ToString() : hour.ToString()) + ":" +
                                (Minuto < 10 ? "0" + Minuto.ToString() : Minuto.ToString()) + " " + Meridiano;

            return horaEnviar;
        }

        public void RetornarAlgoFelix() {

            var primero = db.Ordenes.FirstOrDefault();

            primero.CodigoOrden = 30;

            db.Entry(primero).State = EntityState.Modified;
            db.SaveChanges();

            dynamic obj = new {
                NoOrden = "1",
                Hora = "10 00 AM",
                Cliente = "Felix Hdez"
            };



            AddNewOrder.Preppend(obj);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}