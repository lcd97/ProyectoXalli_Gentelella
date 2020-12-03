using Newtonsoft.Json;
using ProyectoXalli_Gentelella.Models;
using ProyectoXalli_Gentelella.Web_Sockets;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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

        public ActionResult Salidas() {
            return View();
        }

        public ActionResult categoryList() {
            var data = (from obj in db.CategoriasMenu.ToList()
                        join c in db.Bodegas.ToList() on obj.BodegaId equals c.Id
                        where c.DescripcionBodega.ToUpper() == "BAR"
                        select new {
                            Id = obj.Id,
                            Descripcion = obj.DescripcionCategoriaMenu
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


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
        /// RECUPERA LOS DATOS DEL CLIENTE POR MEDIO DE LA IDENTIFICACION EN ORDENES
        /// </summary>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        public ActionResult DataClient(string identificacion) {
            string mensaje = "";
            //SE CREA EL OBJETO
            var validacionOrden = (dynamic)null;

            //REALIZO LA BUSQUEDA DEL CLIENTE QUE PERTENCE A LA ID
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

            //SI EXISTE UN REGISTRO DE CLIENTE CON ESE ID
            if (cliente != null) {
                //CONSULTO SI EL CLIENTE TIENE ORDENES ACTIVAS (1)
                validacionOrden = (from obj in db.Clientes
                                   join c in db.Ordenes on obj.Id equals c.ClienteId
                                   where c.EstadoOrden == 1 && c.ClienteId == cliente.ClienteId
                                   select obj).FirstOrDefault();
            }

            //SI EL CLIENTE TIENEN ORDENES ACTIVAS MOSTRAR LETRERO DE ERROR
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
        /// MUESTRA LA VISTA DEL DETALLE DE ORDEN
        /// </summary>
        /// <returns></returns>
        public ActionResult DetalleSalida() {
            return View();
        }

        public ActionResult guardarSalida(int Codigo, int MeseroId, DateTime FechaOrden, string detalleOrden) {
            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //DESERIALIZACION DE OBJETO DONDE ESTA EL DETALLE DE LA ORDEN
                    var DetalleOrden = JsonConvert.DeserializeObject<List<DetalleDeOrden>>(detalleOrden);
                    var tipoOrden = db.TiposDeOrden.DefaultIfEmpty(null).FirstOrDefault(t => t.DescripcionTipoOrden.ToUpper() == "SALIDAS");

                    //SI NO EXISTE EL TIPO DE ORDEN CREARLO
                    if (tipoOrden == null) {
                        tipoOrden = new TipoDeOrden();

                        tipoOrden.CodigoTipoOrden = codigoOrden();
                        tipoOrden.DescripcionTipoOrden = "Salidas";
                        tipoOrden.EstadoTipoOrden = true;

                        db.TiposDeOrden.Add(tipoOrden);
                        db.SaveChanges();
                    }

                    //SE CREA UN NUEVO OBJETO PARA ALMACENAR LA ORDEN(TABLA PADRE)
                    Orden orden = new Orden();

                    //SE CREAN OBJETOS PARA ALMACENAR A CLIENTE POR DEFAULT EN CASO DE QUE NO EXISTA EN LA BD
                    Dato datoDefault = new Dato();
                    Cliente clienteDefault = new Cliente();

                    //SE CREA OBJETO PARA ALMACENAR EL CLIENTE
                    Cliente clientePlantilla = new Cliente();

                    //SE GUARDDA LOS DATOS DE LA ORDEN
                    orden.CodigoOrden = Codigo;
                    orden.FechaOrden = Convert.ToDateTime(FechaOrden);
                    orden.EstadoOrden = 1;//1 ORDENADA 2 SIN FACTURAR 3 FACTURADA
                    orden.MeseroId = MeseroId;
                    orden.TipoOrdenId = tipoOrden.Id;

                    //SE BUSCA EL CLIENTE DEFAULT
                    var buscarP = db.Datos.DefaultIfEmpty(null).FirstOrDefault(b => b.Cedula == "000-000000-0000X");

                    //SI NO EXISTE LA PLANTILLA, SE MANDA A CREAR
                    if (buscarP == null) {
                        datoDefault.Cedula = "000-000000-0000X";
                        datoDefault.PNombre = "Default";
                        datoDefault.PApellido = "User";

                        db.Datos.Add(datoDefault);
                        db.SaveChanges();

                        clienteDefault.DatoId = datoDefault.Id;
                        clienteDefault.EmailCliente = "defaultuser@xalli.com";
                        clienteDefault.EstadoCliente = false;

                        db.Clientes.Add(clienteDefault);
                        db.SaveChanges();

                        //ASIGNAR EL ID DE LA PLANTILLA RECIEN CREADA
                        clientePlantilla = clienteDefault;
                    } else {//YA EXISTE EL REGISTRO DE CLIENTE PLANTILLA
                            //SE MANDA A BUSCAR PARA OBTENER EL ID DE LA PLANTILLA
                        clientePlantilla = db.Clientes.FirstOrDefault(c => c.DatoId == buscarP.Id);
                    }

                    //ALMACENO EL REGISTRO CON EL CLIENTE DE VISITANTE (PLANTILLA DEFAULT)
                    orden.ClienteId = clientePlantilla.Id;

                    //BUSCO SI EXISTE EL REGISTRO DE LA COMANDA POR DEFAULT
                    var comandaCero = db.Imagenes.DefaultIfEmpty(null).FirstOrDefault(i => i.Ruta == "N/A");
                    Imagen img = new Imagen();//CREO UNA INSTANCIA DE IMAGEN PARA ALMACENAR EL DEFAULT

                    //EN CASO QUE NO EXISTA EL REGISTRO DEFAULT DE LA COMANDA
                    if (comandaCero == null) {
                        //SE CREA EL DEFAUTL
                        img.Ruta = "N/A";
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
                            DetalleDeOrden detalleItem = new DetalleDeOrden();//SE CREA LA INSTANCIA DE DETALLE DE ORDEN

                            detalleItem.CantidadOrden = item.CantidadOrden;
                            //EN CASO DE QUE NO EXISTA NOTA DE PLATILLO SE MANDA NULL
                            detalleItem.NotaDetalleOrden = null;
                            detalleItem.PrecioOrden = 0;
                            detalleItem.MenuId = item.MenuId;
                            detalleItem.OrdenId = orden.Id;
                            //SE GUARDA FALSE PARA DECIR QUE NO ESTA ATENDIDO EL DETALLE (FALSE-NO ATENDIDO; TRUE-ATENDIDO)
                            detalleItem.EstadoDetalleOrden = false;

                            //SE ALMACENA CADA ITEM
                            db.DetallesDeOrden.Add(detalleItem);
                            completado = db.SaveChanges() > 0 ? true : false;
                        }
                        mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                    }

                    transact.Commit();
                } catch (DbEntityValidationException dbEx) {

                    //CAPTURAR ERRORES DE VALIDACION
                    foreach (var validationErrors in dbEx.EntityValidationErrors) {
                        foreach (var validationError in validationErrors.ValidationErrors) {
                            System.Console.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }

                    //mensaje = "Error al almacenar orden";
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING
            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
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
                    //DESERIALIZACION DE OBJETO DONDE ESTA EL DETALLE DE LA ORDEN
                    var DetalleOrden = JsonConvert.DeserializeObject<List<DetalleDeOrden>>(detalleOrden);
                    var tipoOrden = db.TiposDeOrden.DefaultIfEmpty(null).FirstOrDefault(t => t.DescripcionTipoOrden.ToUpper() == "VENTAS");

                    //SI NO EXISTE EL TIPO DE ORDEN CREARLO
                    if (tipoOrden == null) {
                        tipoOrden = new TipoDeOrden();

                        tipoOrden.CodigoTipoOrden = codigoOrden();
                        tipoOrden.DescripcionTipoOrden = "Ventas";
                        tipoOrden.EstadoTipoOrden = true;

                        db.TiposDeOrden.Add(tipoOrden);
                        db.SaveChanges();
                    }

                    //SE CREA UN NUEVO OBJETO PARA ALMACENAR LA ORDEN(TABLA PADRE)
                    Orden orden = new Orden();

                    //SE CREAN OBJETOS PARA ALMACENAR A CLIENTE POR DEFAULT EN CASO DE QUE NO EXISTA EN LA BD
                    Dato datoDefault = new Dato();
                    Cliente clienteDefault = new Cliente();

                    //SE CREA OBJETO PARA ALMACENAR EL CLIENTE
                    Cliente clientePlantilla = new Cliente();

                    //SE GUARDDA LOS DATOS DE LA ORDEN
                    orden.CodigoOrden = Codigo;
                    orden.FechaOrden = Convert.ToDateTime(FechaOrden);
                    orden.EstadoOrden = 1;//1 ORDENADA 2 SIN FACTURAR 3 FACTURADA
                    orden.MeseroId = MeseroId;
                    orden.TipoOrdenId = tipoOrden.Id;

                    //SI EL CLIENTE ES VISITANTE SE MANDA EL CLIENTE POR DEFAULT
                    if (ClienteId == 0) {
                        var buscarP = db.Datos.DefaultIfEmpty(null).FirstOrDefault(b => b.Cedula == "000-000000-0000X");

                        //SI NO EXISTE LA PLANTILLA, SE MANDA A CREAR
                        if (buscarP == null) {
                            datoDefault.Cedula = "000-000000-0000X";
                            datoDefault.PNombre = "Default";
                            datoDefault.PApellido = "User";

                            db.Datos.Add(datoDefault);
                            db.SaveChanges();

                            clienteDefault.DatoId = datoDefault.Id;
                            clienteDefault.EmailCliente = "defaultuser@xalli.com";
                            clienteDefault.EstadoCliente = false;

                            db.Clientes.Add(clienteDefault);
                            db.SaveChanges();

                            //ASIGNAR EL ID DE LA PLANTILLA RECIEN CREADA
                            clientePlantilla = clienteDefault;
                        } else {//YA EXISTE EL REGISTRO DE CLIENTE PLANTILLA
                            //SE MANDA A BUSCAR PARA OBTENER EL ID DE LA PLANTILLA
                            clientePlantilla = db.Clientes.FirstOrDefault(c => c.DatoId == buscarP.Id);
                        }

                        //ALMACENO EL REGISTRO CON EL CLIENTE DE VISITANTE (PLANTILLA DEFAULT)
                        orden.ClienteId = clientePlantilla.Id;

                    } else {
                        //SI EL CLIENTE ES HUESPED
                        orden.ClienteId = ClienteId;
                    }

                    //BUSCO SI EXISTE EL REGISTRO DE LA COMANDA POR DEFAULT
                    var comandaCero = db.Imagenes.DefaultIfEmpty(null).FirstOrDefault(i => i.Ruta == "N/A");
                    Imagen img = new Imagen();//CREO UNA INSTANCIA DE IMAGEN PARA ALMACENAR EL DEFAULT

                    //EN CASO QUE NO EXISTA EL REGISTRO DEFAULT DE LA COMANDA
                    if (comandaCero == null) {
                        //SE CREA EL DEFAUTL
                        img.Ruta = "N/A";
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
                            DetalleDeOrden detalleItem = new DetalleDeOrden();//SE CREA LA INSTANCIA DE DETALLE DE ORDEN

                            detalleItem.CantidadOrden = item.CantidadOrden;
                            //EN CASO DE QUE NO EXISTA NOTA DE PLATILLO SE MANDA NULL
                            detalleItem.NotaDetalleOrden = item.NotaDetalleOrden != "" ? item.NotaDetalleOrden.Trim() : null;
                            detalleItem.PrecioOrden = item.PrecioOrden;
                            detalleItem.MenuId = item.MenuId;
                            detalleItem.OrdenId = orden.Id;
                            //SE GUARDA FALSE PARA DECIR QUE NO ESTA ATENDIDO EL DETALLE (FALSE-NO ATENDIDO; TRUE-ATENDIDO)
                            detalleItem.EstadoDetalleOrden = false;

                            //SE ALMACENA CADA ITEM
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
        public ActionResult VerOrdenes(string mensaje = "") {
            ViewBag.Message = mensaje;

            return View();
        }

        public string codigoOrden() {
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

            return num;
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
            //OBTENGO TODAS LAS ORDENES REALIZADAS POR EL MESERO LOGGEADO
            if (EmpleadoRol == "Mesero") {
                orden = (from obj in db.Ordenes.ToList()
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                         where obj.EstadoOrden == 1 && obj.MeseroId == empleadoId && t.CodigoTipoOrden == "V01"
                         select new {
                             OrdenId = obj.Id,
                             CodigoOrden = obj.CodigoOrden,
                             HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
                             Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A"
                         }).ToList();
            } else if (EmpleadoRol == "Cocinero") {
                //OBTENGO TODAS LAS ORDENES CON PLATILLO PENDIENTES
                orden = (from obj in db.Ordenes.ToList()
                         join det in db.DetallesDeOrden.ToList() on obj.Id equals det.OrdenId
                         join men in db.Menus.ToList() on det.MenuId equals men.Id
                         join cat in db.CategoriasMenu.ToList() on men.CategoriaMenuId equals cat.Id
                         join bod in db.Bodegas.ToList() on cat.BodegaId equals bod.Id
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                         where det.EstadoDetalleOrden == false && bod.DescripcionBodega.ToUpper() == "COCINA" && t.CodigoTipoOrden == "V01"
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
                         }).Distinct().ToList();
            } else if (EmpleadoRol == "Bartender") {
                //OBTENGO TODAS LAS ORDENES CON BEBIDAS PENDIENTES
                orden = (from obj in db.Ordenes.ToList()
                         join det in db.DetallesDeOrden.ToList() on obj.Id equals det.OrdenId
                         join men in db.Menus.ToList() on det.MenuId equals men.Id
                         join cat in db.CategoriasMenu.ToList() on men.CategoriaMenuId equals cat.Id
                         join bod in db.Bodegas.ToList() on cat.BodegaId equals bod.Id
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                         where det.EstadoDetalleOrden == false && bod.DescripcionBodega.ToUpper() == "BAR" && t.CodigoTipoOrden == "V01"
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
                         }).Distinct().ToList();
            } else {
                orden = (from obj in db.Ordenes.ToList()
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                         where obj.EstadoOrden == 1 && t.CodigoTipoOrden == "V01"
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
        public ActionResult SendEmail(string Email, int orderId) {
            if (Email == "") {
                mensaje = "Ingrese el correo del cliente";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            string emailHotel = "proyectoshotel2020@gmail.com";
            string passwordHotel = "bxqalmibjgxzjqux";
            //string passwordHotel = "Calabazas#sin.Nombre2020";

            var orden = db.Ordenes.FirstOrDefault(w => w.Id == orderId);
            var cliente = db.Clientes.FirstOrDefault(w => w.Id == orden.ClienteId && w.EmailCliente != "defaultuser@xalli.com");
            var dato = cliente != null ? db.Datos.DefaultIfEmpty(null).FirstOrDefault(w => w.Id == cliente.DatoId) : null;
            var nombreCliente = dato != null ? dato.PNombre + " " + dato.PApellido : "";

            var encabezado = "Estimado cliente: " + nombreCliente + "<br/> " +
                "Le enviamos el registro de los productos consumidos en el bar y restaurante del hotel Xalli:<br/><br/>";

            var footer = "<table style='font-family:Roboto-Regular,Helvetica,Arial,sans-serif;font-size:10px;color:#666666;line-height:18px;padding-bottom:10px'>" +
                         "<tbody>" +
                         "<tr>" +
                         "<td>Este correo electrónico no puede recibir respuestas. Para obtener más información, accede sitio " +
                         "<a href='https://www.xallihotel.com/' style='text-decoration:none;color:#4d90fe' target='_blank'data-saferedirectreason='2' data-saferedirecturl='https://www.xallihotel.com/'>" +
                         "XALLI, OMETEPE BEACH HOTEL" +
                         "</a>." +
                         "<br> Copyright - 2020.All Rights Reserved." +
                         "</td>" +
                         "</tr>" +
                         "</tbody>" +
                         "</table>";

            string bodyMessage = "", code = "";

            encabezadoEmail(ref code, ref bodyMessage, orderId);

            try {
                MailMessage correo = new MailMessage();
                correo.From = new MailAddress(emailHotel);
                correo.To.Add(Email);
                correo.Subject = "Comanda de la orden: " + code;
                correo.Body = encabezado + bodyMessage + footer;
                correo.IsBodyHtml = true;
                correo.Priority = MailPriority.Normal;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(emailHotel, passwordHotel);

                smtp.Send(correo);

                completado = true;
                mensaje = "Correo enviado correctamente";

            } catch (Exception ex) {
                mensaje = ex.Message;
            }

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="orderId"></param>
        /// <param name="meseroName"></param>
        /// <param name="fechaOrden"></param>
        /// <param name="bodyMessage"></param>
        public void encabezadoEmail(ref string code, ref string bodyMessage, int orderId) {
            var orden = db.Ordenes.Join(db.Meseros, o => o.MeseroId, m => m.Id, (o, m) => new { o, m })
                                .Join(db.Datos, mes => mes.m.DatoId, d => d.Id, (mes, d) => new { mes, d })
                                .Where(w => w.mes.o.Id == orderId).FirstOrDefault();

            if (orden.mes.o.CodigoOrden < 10) {
                code = "000" + orden.mes.o.CodigoOrden;
            } else if (orden.mes.o.CodigoOrden >= 10 || orden.mes.o.CodigoOrden < 100) {
                code = "00" + orden.mes.o.CodigoOrden;
            } else if (orden.mes.o.CodigoOrden >= 100 || orden.mes.o.CodigoOrden < 1000) {
                code = "0" + orden.mes.o.CodigoOrden;
            } else {
                code = (orden.mes.o.CodigoOrden).ToString();
            }

            var fechaOrden = orden.mes.o.FechaOrden.ToShortDateString();
            var meseroName = orden.d.PNombre + " " + orden.d.PApellido;

            var detalleOrden = db.DetallesDeOrden.Join(db.Menus, d => d.MenuId, m => m.Id, (d, m) => new { d, m })
                .Where(w => w.d.OrdenId == orderId).ToList();

            bodyMessage += "<table style='color: #73879C;border: 1px solid #ddd; padding: 8px 8px 8px 8px;'>" +
                                "<thead>" +
                                "<th style='color:#73879C; padding: 8px 8px 8px 8px;'>No. orden " + code + " </th>" +
                                "<th style='color:#73879C padding: 8px 8px 8px 8px;'>Fecha: " + fechaOrden + "</th>" +
                                "<th style='color:#73879C padding: 8px 8px 8px 8px;'>Mesero: " + meseroName + "</th>" +
                                "</tr>" +
                                "<tr>" +
                                "<th style='color: #73879C;border: 1px solid #ddd; padding: 8px 8px 8px 8px;'>Cantidad</th>" +
                                "<th style='color: #73879C;border: 1px solid #ddd; padding: 8px 8px 8px 8px;'>Precio Unitario</th>" +
                                "<th style='width: 65%; text-align: center;color: #73879C;border: 1px solid #ddd; padding: 8px 8px 8px 8px;'>Platillo/Bebida</th>" +
                                "<th style='color: #73879C;border: 1px solid #ddd; padding: 8px 8px 8px 8px;'>Subtotal</th>" +
                                "</tr>" +
                                "</thead>" +
                                "<tbody>";

            foreach (var item in detalleOrden) {
                var subtotal = item.d.CantidadOrden * item.d.PrecioOrden;

                //AGREGAR EL DETALLE DE LA ORDEN
                bodyMessage += "<tr>" +
                    "<td style='color: #73879C; border: 1px solid #ddd; padding: 8px 8px 8px 8px;'>" + item.d.CantidadOrden + "</td>" +
                    "<td style='color: #73879C; border: 1px solid #ddd; padding: 8px 8px 8px 8px;'> $" + item.d.PrecioOrden + "</td>" +
                    "<td style='width: 150px; text-align: center; color: #73879C;border: 1px solid #ddd;padding: 8px 8px 8px 8px;'>" + item.m.DescripcionMenu + "</td>" +
                    "<td style='color: #73879C; border: 1px solid #ddd; padding: 8px 8px 8px 8px;'> $" + subtotal + "</td>" +
                    "</tr>";
            }

            bodyMessage += "</tbody></table>";
        }

        /// <summary>
        /// ACTUALIZA LOS ELEMENTOS DE UNA ORDEN EN ESPECIFICO
        /// </summary>
        /// <param name="Codigo"></param>
        /// <param name="FechaOrden"></param>
        /// <param name="EstadoOrden">SE REFIERE A SI LA ORDEN SE PUEDE CERRAR Y PASAR DE 1 - ORDENADA A 2 - SIN FACTURAR</param>
        /// <param name="detalleOrden"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditOrder(int Codigo, DateTime FechaOrden, int EstadoOrden, string detalleOrden) {
            //BUSCAR LA ORDEN
            var Orden = db.Ordenes.DefaultIfEmpty(null).FirstOrDefault(o => o.CodigoOrden == Codigo);
            //DESERIALIZACION DE OBJETO DEL DETALLE (EN CASO DE QUE HAYA, EN CASO CONTRARIO SALE VACIO)
            var DetalleOrden = JsonConvert.DeserializeObject<List<DetalleDeOrden>>(detalleOrden);

            if (Orden != null) {
                using (var transact = db.Database.BeginTransaction()) {
                    try {
                        //SI NO LLEGA NINGUN OBJETO PARA EL DETALLE DE ORDEN
                        if (DetalleOrden == null) {
                            //MANDO EL ESTADO DE LA ORDEN
                            Orden.EstadoOrden = EstadoOrden;
                            db.Entry(Orden).State = EntityState.Modified;
                            completado = db.SaveChanges() > 0 ? true : false;
                            mensaje = completado ? "Orden finalizada" : "Error al almacenar";
                        } else {
                            Orden.FechaOrden = Convert.ToDateTime(FechaOrden);//CAMBIO LA HORA DEL PEDIDO
                            Orden.EstadoOrden = EstadoOrden;//MANDO EL ESTADO DE LA ORDEN
                            //1 ABIERTO
                            //2 CERRADO

                            db.Entry(Orden).State = EntityState.Modified;
                            completado = db.SaveChanges() > 0 ? true : false;

                            if (completado) {
                                //SE PROCEDE A ALMACENAR LOS NUEVOS PLATILLOS/BEBIDAS
                                //var details = db.DetallesDeOrden.FirstOrDefault(d => d.OrdenId == Orden.Id);
                                //RECORRO LA LISTA DE LOS NUEVOS PEDIDOS
                                foreach (var item in DetalleOrden) {
                                    //ABRO UNA NUEVA INSTANCIA DE DETALLE
                                    DetalleDeOrden detalleItem = new DetalleDeOrden();

                                    //SI EL ESTADO DE LA LISTA DE CADA ITEM ES ACTIVO
                                    if (item.EstadoDetalleOrden == false) {
                                        detalleItem.CantidadOrden = item.CantidadOrden;
                                        detalleItem.NotaDetalleOrden = item.NotaDetalleOrden != "" ? item.NotaDetalleOrden.Trim() : null;
                                        detalleItem.PrecioOrden = item.PrecioOrden;
                                        detalleItem.MenuId = item.MenuId;
                                        detalleItem.OrdenId = Orden.Id;
                                        detalleItem.EstadoDetalleOrden = false;

                                        db.DetallesDeOrden.Add(detalleItem);
                                        completado = db.SaveChanges() > 0 ? true : false;
                                    }
                                }

                                mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                            }
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

        public ActionResult Preparacion(int ordenId, int rol) {
            ViewBag.OrdenId = ordenId;//ENVIAR EL ID DE LA ORDEN A MOSTRAR
            ViewBag.role = rol == 1 ? "Bar" : "Cocina";

            return View();
        }

        /// <summary>
        /// OBTENGO LOS DATOS DE LA ORDEN A VER (PARA EL AREA DE COCINA O BAR)
        /// </summary>
        /// <param name="area">RECIBE SI ES COCINA O BAR</param>
        /// <param name="ordenId">ID DE LA ORDEN</param>
        /// <returns></returns>
        public ActionResult getItemsPrep(string area, int ordenId) {
            /*
              select o.CodigoOrden,
						CONVERT(varchar, o.FechaOrden,3) as fecha, 
						do.CantidadOrden,
						m.DescripcionMenu,
						do.NotaDetalleOrden, 
						d.PNombre + ' ' + d.PApellido	 as mesero
				 from ord.Ordenes o
                 inner join ord.DetallesDeOrden do    
                 on o.Id = do.OrdenId
				 inner join Menu.Menus m
				 on m.Id = do.MenuId
				 inner join menu.CategoriasMenu cm 
				 on m.CategoriaMenuId = cm.Id
				 inner join inv.Bodegas b
				 on b.Id = cm.BodegaId
				 inner join ord.Meseros me
				 on me.id = o.MeseroId
				 INNER join inv.Datos d
				 on d.Id = me.DatoId
				 where do.EstadoDetalleOrden = 1 and o.CodigoOrden = 2 and cm.BodegaId = 2
             */

            //RECUPERO EL ENCABEZADO DE LA ORDEN
            var encabezado = (from ord in db.Ordenes.ToList()
                              join det in db.DetallesDeOrden.ToList() on ord.Id equals det.OrdenId
                              join menu in db.Menus.ToList() on det.MenuId equals menu.Id
                              join cat in db.CategoriasMenu.ToList() on menu.CategoriaMenuId equals cat.Id
                              join bod in db.Bodegas.ToList() on cat.BodegaId equals bod.Id
                              join cli in db.Clientes.ToList() on ord.ClienteId equals cli.Id
                              join dat in db.Datos.ToList() on cli.DatoId equals dat.Id
                              where det.OrdenId == ordenId && det.EstadoDetalleOrden == false && bod.DescripcionBodega.ToUpper() == area.ToUpper()
                              select new {
                                  Id = ord.Id,
                                  Codigo = ord.CodigoOrden,
                                  Fecha = ord.FechaOrden.ToShortDateString(),
                                  HoraOrden = ConvertHour(ord.FechaOrden.Hour, ord.FechaOrden.Minute),
                                  Cliente = dat.PNombre != null ? dat.PNombre.Trim() + " " + dat.PApellido.Trim() : null,
                                  Mesero = (from o in db.Ordenes
                                            join m in db.Meseros on o.MeseroId equals m.Id
                                            join a in db.Datos on m.DatoId equals a.Id
                                            where ord.Id == o.Id
                                            select a.PNombre + " " + a.PApellido).FirstOrDefault()
                              }).FirstOrDefault();

            //OBTENGO EL DETALLE DE LA ORDEN (PLATILLOS O BEBIDAS)
            var values = (from ord in db.Ordenes.ToList()
                          join det in db.DetallesDeOrden.ToList() on ord.Id equals det.OrdenId
                          join menu in db.Menus.ToList() on det.MenuId equals menu.Id
                          join cat in db.CategoriasMenu.ToList() on menu.CategoriaMenuId equals cat.Id
                          join bod in db.Bodegas.ToList() on cat.BodegaId equals bod.Id
                          where det.OrdenId == ordenId && det.EstadoDetalleOrden == false && bod.DescripcionBodega.ToUpper() == area.ToUpper()
                          group new { det, menu } by new { menu.DescripcionMenu, det.NotaDetalleOrden } into grouped
                          select new {
                              Cantidad = grouped.Sum(s => s.det.CantidadOrden),//SUMO LA CANTIDAD SOLICITADA EN CASO DE QUE SEA EL MISMO PRODUCTO
                              Platillo = grouped.Key.DescripcionMenu,
                              Nota = grouped.Key.NotaDetalleOrden,
                          }).ToList();

            //RECUPERO LOS IDS DE CADA UNO DE LOS ITEMS SOLICITADOS
            var ids = (from ord in db.Ordenes.ToList()
                       join det in db.DetallesDeOrden.ToList() on ord.Id equals det.OrdenId
                       join menu in db.Menus.ToList() on det.MenuId equals menu.Id
                       join cat in db.CategoriasMenu.ToList() on menu.CategoriaMenuId equals cat.Id
                       join bod in db.Bodegas.ToList() on cat.BodegaId equals bod.Id
                       where det.OrdenId == ordenId && det.EstadoDetalleOrden == false && bod.DescripcionBodega.ToUpper() == area.ToUpper()
                       select det.Id).ToList();

            return Json(new { encabezado, values, ids }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        /// <summary>
        /// ATIENDE LOS PEDIDOS SOLICITADOS POR EL CLIENTE
        /// </summary>
        /// <param name="detailsId">CADENA DE IDS DE PRODUCTOS A ATENDER</param>
        /// <returns></returns>
        public ActionResult EditOrderDetails(string detailsId) {
            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //CONVIERTO LA CADENA A UN ARRAY DE STRING
                    var detalle = detailsId.Split(',');

                    //RECORRO EL ARRAY
                    foreach (var item in detalle) {
                        var deta = int.Parse(item);//PARSEO DE STRING A ENTERO
                        //BUSCO EL ID DEL DETALLE DE PRODUCTO SOLICITADO
                        DetalleDeOrden detalleDeOrden = db.DetallesDeOrden.FirstOrDefault(c => c.Id == deta);

                        //CAMBIO EL ESTADO DEL PLATILLO
                        detalleDeOrden.EstadoDetalleOrden = true;
                        completado = db.SaveChanges() > 0 ? true : false;//GUARDO CAMBIOS
                    }

                    mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al almacenar";
                    transact.Rollback();
                }
            }

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

        /// <summary>
        /// METODO PARA CERRA LA ORDEN DESDE LA COMANDA
        /// </summary>
        /// <param name="ordenId"></param>
        /// <returns></returns>
        public ActionResult CerrarComanda(int ordenId) {
            //BSUCA LA ORDEN
            var cerrar = db.Ordenes.FirstOrDefault(w => w.Id == ordenId);

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    cerrar.EstadoOrden = 2;//TERMINAR LA ORDEN
                    db.Entry(cerrar).State = EntityState.Modified;

                    completado = db.SaveChanges() > 0 ? true : false;
                    mensaje = completado ? "Cerrado correctamente" : "Error al cerrar";

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al cerrar";
                    transact.Rollback();
                }
            }
            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
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

        //si el menuid es de bar o no 
        public async Task<bool> esDeBar(int id) {
            string Bodega = await (from c in db.CategoriasMenu
                                   join b in db.Bodegas on c.BodegaId equals b.Id
                                   join m in db.Menus on c.Id equals m.CategoriaMenuId
                                   where m.Id == id
                                   select b.CodigoBodega).DefaultIfEmpty().FirstOrDefaultAsync();

            if (Bodega == "B01") {
                return true;
            }

            return false;
        }

        /// <summary>
        /// CONSULTA LA EXISTENCIAS 
        /// </summary>
        /// <param name="id">ID DE MENU</param>
        /// <returns></returns>
        public async Task<ActionResult> existencia(int id) {
            int existencia;
            int salidas;
            double entradas;

            if (await esDeBar(id)) {

                int idProd = (from m in db.Menus
                              join i in db.Ingredientes on m.Id equals i.MenuId
                              where i.MenuId == id
                              select i.ProductoId).DefaultIfEmpty(-1).FirstOrDefault();

                if (idProd != -1) {
                    salidas = (from m in db.Menus
                               join dor in db.DetallesDeOrden on m.Id equals dor.MenuId
                               join o in db.Ordenes on dor.OrdenId equals o.Id
                               where m.Id == id
                               select dor.CantidadOrden).DefaultIfEmpty(0).Sum();

                    entradas = (from p in db.Productos
                                join de in db.DetallesDeEntrada on p.Id equals de.ProductoId
                                join e in db.Entradas on de.EntradaId equals e.Id
                                where p.Id == idProd
                                select de.CantidadEntrada).DefaultIfEmpty(0).Sum();

                    //calculando las existencias
                    existencia = (int)entradas - salidas;
                } else {
                    //Este menu no tiene un producto relacionado
                    existencia = -1;
                }
            } else {
                existencia = -1;
            }

            return Json(existencia, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}