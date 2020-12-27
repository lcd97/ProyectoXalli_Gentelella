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
            ViewBag.MesasId = new SelectList(ListaMesas(), "Id", "Descripcion");
            ViewBag.CategoriaId = new SelectList(db.CategoriasMenu, "Id", "DescripcionCategoriaMenu");

            return View();
        }

        /// <summary>
        /// RETORNA EL CODIGO DE ENTRADA AUTOMATICAMENTE
        /// </summary>
        /// <returns></returns>
        public ActionResult OrdenesCode() {
            int codigo = calcularMax();

            return Json(codigo, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// CALCULA EL NUMERO DE ORDEN MAXIMO DE LA ORDEN
        /// </summary>
        /// <returns></returns>
        public int calcularMax() {
            //BUSCAR EL VALLOR MAXIMO ALMACENADO DE CODIGO
            var num = (from obj in db.Ordenes
                       select obj.CodigoOrden).DefaultIfEmpty().Max();

            int codigo = 1;

            if (num != 0) {
                codigo = num + 1;
            }

            return codigo;
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
        /// RECUPERA TODOS LOS PLATILLOS POR CATEGORIA DE BEBIDAS INVENTARIADS
        /// </summary>
        /// <param name="id">CATEGORIA ID</param>
        /// <returns></returns>
        public ActionResult BebidasInventariado(int id) {
            var menu = from obj in db.Menus.ToList()
                       join i in db.Imagenes.ToList() on obj.ImagenId equals i.Id
                       where obj.CategoriaMenuId == id && obj.Inventariado
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

        public async Task<ActionResult> guardarSalida(int Codigo, int MeseroId, DateTime FechaOrden, string detalleOrden) {
            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //DESERIALIZACION DE OBJETO DONDE ESTA EL DETALLE DE LA ORDEN
                    var DetalleOrden = JsonConvert.DeserializeObject<List<DetalleDeOrden>>(detalleOrden);
                    var tipoOrden = db.TiposDeOrden.DefaultIfEmpty(null).FirstOrDefault(t => t.CodigoTipoOrden.ToUpper() == "S01");

                    //SI NO EXISTE EL TIPO DE ORDEN CREARLO
                    if (tipoOrden == null) {
                        tipoOrden = new TipoDeOrden();

                        tipoOrden.CodigoTipoOrden = "S01";
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
                    orden.CodigoOrden = calcularMax();
                    orden.FechaOrden = Convert.ToDateTime(FechaOrden);
                    orden.EstadoOrden = 4;//1 ORDENADA 2 SIN FACTURAR 3 FACTURADA 4 SALIDAS
                    orden.MeseroId = MeseroId;
                    orden.TipoOrdenId = tipoOrden.Id;
                    orden.MesaId = db.Mesas.FirstOrDefault().Id; //SE PASA LA PRIMERA MESA QUE ENCUENTRE

                    //SE BUSCA EL CLIENTE DEFAULT
                    var buscarP = db.Datos.DefaultIfEmpty(null).FirstOrDefault(b => b.Cedula == "000-000000-0000X");

                    //SI NO EXISTE LA PLANTILLA, SE MANDA A CREAR
                    if (buscarP == null) {
                        datoDefault.Cedula = "000-000000-0000X";
                        datoDefault.PNombre = "DEFAULT";
                        datoDefault.PApellido = "USER";

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
                            //RECALCULAR LA EXISTENCIA DEL PRODUCTO DE MENU
                            int existAntigua = await recalcularExist(item.MenuId);

                            if (existAntigua == -1) {//NO HAY EXISTENCIAS
                                completado = false;
                                var platillo = db.Menus.Find(item.MenuId);
                                mensaje = "La existencia es menor que la cantidad específicada del producto: " + platillo.DescripcionMenu;
                                transact.Rollback();

                                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                            } else if (existAntigua == -2) {//ES PRODUCTO NO CONTROLADO (EJEMPLO AREA COCINA)
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
                            } else {
                                //PRODUCTO CONTROLADO
                                int existencia = existAntigua - item.CantidadOrden;

                                if (existencia < 0) {
                                    //NO ES POSIBLE ALMACENAR EL DETALLE
                                    completado = false;
                                    var platillo = db.Menus.Find(item.MenuId);
                                    mensaje = "La existencia es menor que la cantidad específicada del producto: " + platillo.DescripcionMenu;
                                    transact.Rollback();

                                    return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                                } else {
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
                            }
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
        public async Task<ActionResult> Create(int Codigo, int MeseroId, int ClienteId, DateTime FechaOrden, string detalleOrden, int mesaId) {

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //DESERIALIZACION DE OBJETO DONDE ESTA EL DETALLE DE LA ORDEN
                    var DetalleOrden = JsonConvert.DeserializeObject<List<DetalleDeOrden>>(detalleOrden);
                    var tipoOrden = db.TiposDeOrden.DefaultIfEmpty(null).FirstOrDefault(t => t.CodigoTipoOrden.ToUpper() == "V01");

                    //SI NO EXISTE EL TIPO DE ORDEN CREARLO
                    if (tipoOrden == null) {
                        tipoOrden = new TipoDeOrden();

                        tipoOrden.CodigoTipoOrden = "V01";
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
                    orden.CodigoOrden = calcularMax();//EN CASO DE QUE HAYA UNA ORDEN ATRAS DE ESTA CON EL MISMO NUMERO DE ORDEN
                    orden.FechaOrden = Convert.ToDateTime(FechaOrden);
                    orden.EstadoOrden = 1;//1 ORDENADA 2 SIN FACTURAR 3 FACTURADA
                    orden.MeseroId = MeseroId;
                    orden.TipoOrdenId = tipoOrden.Id;
                    orden.MesaId = mesaId;

                    //SI EL CLIENTE ES VISITANTE SE MANDA EL CLIENTE POR DEFAULT
                    if (ClienteId == 0) {
                        var buscarP = db.Datos.DefaultIfEmpty(null).FirstOrDefault(b => b.Cedula == "000-000000-0000X");

                        //SI NO EXISTE LA PLANTILLA, SE MANDA A CREAR
                        if (buscarP == null) {
                            datoDefault.Cedula = "000-000000-0000X";
                            datoDefault.PNombre = "DEFAULT";
                            datoDefault.PApellido = "USER";

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
                            //RECALCULAR LA EXISTENCIA DEL PRODUCTO DE MENU
                            int existAntigua = await recalcularExist(item.MenuId);

                            if (existAntigua == -1) {//NO HAY EXISTENCIAS
                                completado = false;
                                var platillo = db.Menus.Find(item.MenuId);
                                mensaje = "Uno o varios productos que son ingredientes de " + platillo.DescripcionMenu + " no poseen existencias";
                                transact.Rollback();

                                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                            } else if (existAntigua == -2) {//ES PRODUCTO NO CONTROLADO (EJEMPLO AREA COCINA)
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
                            } else {
                                //PRODUCTO CONTROLADO
                                int existencia = existAntigua - item.CantidadOrden;

                                if (existencia < 0) {
                                    //NO ES POSIBLE ALMACENAR EL DETALLE
                                    completado = false;
                                    var platillo = db.Menus.Find(item.MenuId);
                                    mensaje = "La existencia es menor que la cantidad específicada del producto: " + platillo.DescripcionMenu;
                                    transact.Rollback();

                                    return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                                } else {
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
                            }
                        }
                        mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                    }

                    transact.Commit();
                    AddNewOrder.Preppend(/*obj*/);//CONEXION DE WEBSOCKETS
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
                         join m in db.Mesas.ToList() on obj.MesaId equals m.Id
                         where obj.EstadoOrden == 1 && obj.MeseroId == empleadoId && t.CodigoTipoOrden == "V01"
                         orderby obj.FechaOrden.ToLongTimeString() ascending
                         select new {
                             OrdenId = obj.Id,
                             CodigoOrden = obj.CodigoOrden,
                             HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
                             Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
                             Mesa = m.DescripcionMesa
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
                         join m in db.Mesas.ToList() on obj.MesaId equals m.Id
                         where det.EstadoDetalleOrden == false && bod.DescripcionBodega.ToUpper() == "COCINA" && t.CodigoTipoOrden == "V01"
                         orderby obj.FechaOrden.ToLongTimeString() ascending
                         select new {
                             OrdenId = obj.Id,
                             CodigoOrden = obj.CodigoOrden,
                             HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
                             Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
                             Mesa = m.DescripcionMesa,
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
                         join m in db.Mesas.ToList() on obj.MesaId equals m.Id
                         where det.EstadoDetalleOrden == false && bod.DescripcionBodega.ToUpper() == "BAR" && t.CodigoTipoOrden == "V01"
                         orderby obj.FechaOrden.ToLongTimeString() ascending
                         select new {
                             OrdenId = obj.Id,
                             CodigoOrden = obj.CodigoOrden,
                             HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
                             Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
                             Mesa = m.DescripcionMesa,
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
                         join m in db.Mesas.ToList() on obj.MesaId equals m.Id
                         where obj.EstadoOrden == 1 && t.CodigoTipoOrden == "V01"
                         orderby obj.FechaOrden.ToLongTimeString() ascending
                         select new {
                             OrdenId = obj.Id,
                             CodigoOrden = obj.CodigoOrden,
                             HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
                             Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
                             Mesa = m.DescripcionMesa,
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
                             join m in db.Mesas.ToList() on obj.MesaId equals m.Id
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
                                           select a.PNombre + " " + a.PApellido).FirstOrDefault(),
                                 Mesa = m.DescripcionMesa
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
        public async Task<ActionResult> EditOrder(int Codigo, DateTime FechaOrden, int EstadoOrden, string detalleOrden) {
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
                                    //RECALCULAR LA EXISTENCIA DEL PRODUCTO DE MENU
                                    int existAntigua = await recalcularExist(item.MenuId);

                                    if (existAntigua == -1) {//NO HAY EXISTENCIAS
                                        completado = false;
                                        var platillo = db.Menus.Find(item.MenuId);
                                        mensaje = "Uno o varios productos que son ingredientes de " + platillo.DescripcionMenu + " no poseen existencias";
                                        transact.Rollback();

                                        return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                                    } else if (existAntigua == -2) {//ES PRODUCTO NO CONTROLADO (EJEMPLO AREA COCINA)
                                        DetalleDeOrden detalleItem = new DetalleDeOrden();//SE CREA LA INSTANCIA DE DETALLE DE ORDEN

                                        detalleItem.CantidadOrden = item.CantidadOrden;
                                        //EN CASO DE QUE NO EXISTA NOTA DE PLATILLO SE MANDA NULL
                                        detalleItem.NotaDetalleOrden = item.NotaDetalleOrden != "" ? item.NotaDetalleOrden.Trim() : null;
                                        detalleItem.PrecioOrden = item.PrecioOrden;
                                        detalleItem.MenuId = item.MenuId;
                                        detalleItem.OrdenId = Orden.Id;
                                        //SE GUARDA FALSE PARA DECIR QUE NO ESTA ATENDIDO EL DETALLE (FALSE-NO ATENDIDO; TRUE-ATENDIDO)
                                        detalleItem.EstadoDetalleOrden = false;

                                        //SE ALMACENA CADA ITEM
                                        db.DetallesDeOrden.Add(detalleItem);
                                        completado = db.SaveChanges() > 0 ? true : false;
                                    } else {
                                        //PRODUCTO CONTROLADO
                                        int existencia = existAntigua - item.CantidadOrden;

                                        if (existencia < 0) {
                                            //NO ES POSIBLE ALMACENAR EL DETALLE
                                            completado = false;
                                            var platillo = db.Menus.Find(item.MenuId);
                                            mensaje = "La existencia es menor que la cantidad específicada del producto: " + platillo.DescripcionMenu;
                                            transact.Rollback();

                                            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                                        } else {
                                            DetalleDeOrden detalleItem = new DetalleDeOrden();//SE CREA LA INSTANCIA DE DETALLE DE ORDEN

                                            detalleItem.CantidadOrden = item.CantidadOrden;
                                            //EN CASO DE QUE NO EXISTA NOTA DE PLATILLO SE MANDA NULL
                                            detalleItem.NotaDetalleOrden = item.NotaDetalleOrden != "" ? item.NotaDetalleOrden.Trim() : null;
                                            detalleItem.PrecioOrden = item.PrecioOrden;
                                            detalleItem.MenuId = item.MenuId;
                                            detalleItem.OrdenId = Orden.Id;
                                            //SE GUARDA FALSE PARA DECIR QUE NO ESTA ATENDIDO EL DETALLE (FALSE-NO ATENDIDO; TRUE-ATENDIDO)
                                            detalleItem.EstadoDetalleOrden = false;

                                            //SE ALMACENA CADA ITEM
                                            db.DetallesDeOrden.Add(detalleItem);
                                            completado = db.SaveChanges() > 0 ? true : false;
                                        }
                                    }
                                }

                                mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                            }
                        }
                        transact.Commit();
                        AddNewOrder.Preppend(/*obj*/);//CONEXION DE WEBSOCKETS
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

        /// <summary>
        /// GENERA UNA LISTA DE LAS MESAS DISPONIBLES
        /// </summary>
        /// <returns></returns>
        public List<MesasClass> ListaMesas() {
            /*
            SELECT M.DescripcionMesa		
            FROM ORD.Mesas M
            WHERE M.EstadoMesa=1 AND M.Id not in (SELECT o.MesaId FROM ORD.Ordenes O WHERE O.EstadoOrden=1) 
             */
            var mesas = (from obj in db.Mesas.ToList()
                         where obj.EstadoMesa == true && !(from ord in db.Ordenes.ToList() where ord.EstadoOrden == 1
                                                           select ord.MesaId).Contains(obj.Id)
                         select new MesasClass {
                             Id = obj.Id,
                             Descripcion = obj.DescripcionMesa
                         }).ToList();

            return mesas;
        }

        public class MesasClass {
            public int Id { get; set; }
            public string Descripcion { get; set; }
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

        [HttpGet]
        /// <summary>
        /// CONSULTA LA EXISTENCIAS 
        /// </summary>
        /// <param name="id">ID DE MENU</param>
        /// <returns></returns>
        public async Task<ActionResult> existencia(int id) {
            bool completo = true;
            double entradas = 0;
            int salidas = 0, existencia = 0;

            //SI LA CATEGORIA DEL PLATILLO PERTENECE A BAR
            if (await esDeBar(id)) {

                //LISTAR TODOS LOS PRODUCTOS QUE CONFORMAN EL MENU
                var idProd = (from m in db.Menus
                              join i in db.Ingredientes on m.Id equals i.MenuId
                              where i.MenuId == id
                              select i.ProductoId).ToList();

                //EN ESTA CONDICION SE SACARA LA EXISTENCIA EN NUMEROS-EN CASO QUE SEA
                if (idProd.Count == 1) {
                    ExistEntrada(idProd[0], ref entradas);//OBTENEMOS LA ENTRADA DEL PRODUCTO

                    if (entradas == 0) {
                        mensaje = "No disponible";//NO TIENE ENTRADAS, NO HAY EXISTENCIA
                        existencia = -1;
                    } else {
                        //BUSCAR LA BEBIDA PARA SABER SI ES TRAGO O BOTELLA
                        var menu = db.Menus.Find(id);

                        //SI HAY ENTRADAS BUSCAR LAS SALIDAS
                        ExistSalidas(idProd[0], ref salidas);//OBTENEMOS LAS SALIDAS DEL PRODUCTO                    

                        if (menu.Inventariado) {
                            existencia = (int)entradas - salidas;//CALCULO DE LA EXISTENCIA
                            mensaje = existencia.ToString();//MANDO LA CANTIDAD DE EXISTENCIA DEL PRODUCTO
                        } else {
                            //SI ES UN TRAGO
                            existencia = (int)entradas - salidas;//CALCULO DE LA EXISTENCIA
                            mensaje = existencia.ToString();//MANDO LA CANTIDAD DE EXISTENCIA DEL PRODUCTO

                            //MANDO -2 PARA QUE PUEDA AGREGAR CUANTOS TRAGOS SE NECESITEN
                            existencia = -2;
                        }
                    }
                } else {
                    int w = 0;//CONTADOR DE WHILE

                    //RECORRER LA LISTA DE LOS INGREDIENTES Y COMRPOBAR QUE TENGA ENTRADAS DEL AREA DE BODEGA
                    while (w < idProd.Count && completo) {
                        salidas = 0;
                        entradas = 0;

                        if (ExistEntrada(idProd[w], ref entradas)) {//SI LAS ENTRADAS PERTENECEN AL AREA DE BAR
                                                                    //SI EL PRODUCTO ES DE BAR
                            if (entradas > 0) {//SI LAS ENTRADAS FUERON MAYOR A 0
                                ExistSalidas(idProd[w], ref salidas);//CALCULAR LAS SALIDAS

                                existencia = (int)entradas - salidas;//CALCULO LA EXISTENCIA

                                //NO HAY UN PRODUCTO EN EXISTENCIA
                                if (existencia == 0) {
                                    completo = false;
                                    mensaje = "Productos faltantes";
                                    existencia = -1;//NO PUEDE SELECCIONAR PARA ORDENAR
                                } else {
                                    mensaje = "Disponible";
                                    existencia = -2;//PUEDE SELECCIONAR PARA ORDENAR
                                }
                            }
                        }

                        w++;
                    }
                }//FIN IF-ELSE
            } else {
                //PERTENECE A COCINA
                mensaje = "No inventariado";
                existencia = -2;
            }

            /*
             CUANDO DEVUELVO EN EXISTENCIA 
             -1 ES PARA QUE NO AGREGUE A LA ORDEN, 
             -2 PERMITE AGREGAR A LA ORDEN (EN CASO QUE SEA DE LA COCINA O BIEN VARIOS PRODUCTOS Y MUESTRE DISPONIBLE)
             CUANDO ES MAYOR A 0 ES LA EXISTENCIA DE BOTELLAS Y OTROS
             */

            return Json(new { mensaje, existencia }, JsonRequestBehavior.AllowGet);
        }

        public async Task<int> recalcularExist(int id) {
            bool completo = true;
            double entradas = 0;
            int salidas = 0, existencia = 0;

            //SI LA CATEGORIA DEL PLATILLO PERTENECE A BAR
            if (await esDeBar(id)) {

                //LISTAR TODOS LOS PRODUCTOS QUE CONFORMAN EL MENU
                var idProd = (from m in db.Menus
                              join i in db.Ingredientes on m.Id equals i.MenuId
                              where i.MenuId == id
                              select i.ProductoId).ToList();

                //EN ESTA CONDICION SE SACARA LA EXISTENCIA EN NUMEROS-EN CASO QUE SEA
                if (idProd.Count == 1) {
                    ExistEntrada(idProd[0], ref entradas);//OBTENEMOS LA ENTRADA DEL PRODUCTO

                    if (entradas == 0) {
                        existencia = -1;
                    } else {
                        //BUSCAR LA BEBIDA PARA SABER SI ES TRAGO O BOTELLA
                        var menu = db.Menus.Find(id);

                        //SI HAY ENTRADAS BUSCAR LAS SALIDAS
                        ExistSalidas(idProd[0], ref salidas);//OBTENEMOS LAS SALIDAS DEL PRODUCTO                    

                        if (menu.Inventariado) {
                            existencia = (int)entradas - salidas;//CALCULO DE LA EXISTENCIA
                        } else {
                            //SI ES UN TRAGO
                            existencia = (int)entradas - salidas;//CALCULO DE LA EXISTENCIA

                            //MANDO -2 PARA QUE PUEDA AGREGAR CUANTOS TRAGOS SE NECESITEN
                            existencia = -2;
                        }
                    }
                } else {
                    int w = 0;//CONTADOR DE WHILE

                    //RECORRER LA LISTA DE LOS INGREDIENTES Y COMRPOBAR QUE TENGA ENTRADAS DEL AREA DE BODEGA
                    while (w < idProd.Count && completo) {
                        salidas = 0;
                        entradas = 0;

                        if (ExistEntrada(idProd[w], ref entradas)) {//SI LAS ENTRADAS PERTENECEN AL AREA DE BAR
                                                                    //SI EL PRODUCTO ES DE BAR
                            if (entradas > 0) {//SI LAS ENTRADAS FUERON MAYOR A 0
                                ExistSalidas(idProd[w], ref salidas);//CALCULAR LAS SALIDAS

                                existencia = (int)entradas - salidas;//CALCULO LA EXISTENCIA

                                //NO HAY UN PRODUCTO EN EXISTENCIA
                                if (existencia == 0) {
                                    completo = false;
                                    existencia = -1;//NO PUEDE SELECCIONAR PARA ORDENAR
                                } else {
                                    existencia = -2;//PUEDE SELECCIONAR PARA ORDENAR
                                }
                            }
                        }

                        w++;
                    }
                }//FIN IF-ELSE
            } else {
                //PERTENECE A COCINA
                existencia = -2;
            }

            return existencia;
        }


        public bool ExistEntrada(int prodId, ref double entradas) {
            bool esBar = false;

            //SE COMPRUEBA QUE HAYAN ENTRADAS EN EL BAR            
            var entradaProd = (from obj in db.Entradas
                               join de in db.DetallesDeEntrada on obj.Id equals de.EntradaId
                               join b in db.Bodegas on obj.BodegaId equals b.Id
                               where de.ProductoId == prodId && b.CodigoBodega == "B01"
                               group new { de, b } by new { b.CodigoBodega } into grouped
                               select new {
                                   Entradas = grouped.Sum(s => s.de.CantidadEntrada),//SUMAR TODAS LAS ENTRADAS
                                   Bar = grouped.Key.CodigoBodega == "B01" ? true : false//DETERMINAR SI LAS ENTRADAS SON DE BAR O BODEGA
                               }).FirstOrDefault();

            //SI CONTIENE AL MENOS UN ELEMENTO EL PRODUCTO TIENE EXISTENCIA
            if (entradaProd != null) {
                entradas = (double)entradaProd.Entradas;
                esBar = entradaProd.Bar;
            }

            return esBar;
        }

        public void ExistSalidas(int item, ref int salidas) {
            //BUSCAR TODOS LOS PLATILLOS DEL AREA DE BAR QUE TENGAN EL INGREDIENTE            
            var salidasProd = (from obj in db.Menus
                               join i in db.Ingredientes on obj.Id equals i.MenuId
                               join c in db.CategoriasMenu on obj.CategoriaMenuId equals c.Id
                               join b in db.Bodegas on c.BodegaId equals b.Id
                               join d in db.DetallesDeOrden on obj.Id equals d.MenuId
                               where i.ProductoId == item && b.CodigoBodega == "B01" && obj.Inventariado == true
                               select (int?)d.CantidadOrden).Sum();

            //SI CONTIENE AL MENOS UN ELEMENTO EL PRODUCTO TIENE EXISTENCIA
            if (salidasProd != null) {
                salidas = (int)salidasProd;
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}