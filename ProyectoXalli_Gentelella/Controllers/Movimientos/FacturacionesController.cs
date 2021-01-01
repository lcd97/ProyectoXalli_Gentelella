using Newtonsoft.Json;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProyectoXalli_Gentelella.tipoCambioBCN;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace ProyectoXalli_Gentelella.Controllers.Movimientos {
    public class FacturacionesController : Controller {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        //INSTANCIA DE SERVICIO WEB
        Tipo_Cambio_BCNSoapClient tipoCambio = new Tipo_Cambio_BCNSoapClient();

        public ActionResult CalcularCambioHoy() {
            int dia = DateTime.Now.Day;
            int mes = DateTime.Now.Month;
            int anio = DateTime.Now.Year;
            //var cambio = 34.94;

            var cambio = tipoCambio.RecuperaTC_Dia(anio, mes, dia);

            return Json(cambio, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult NumeroFactura() {
            //BUSCAR EL VALLOR MAXIMO ALMACENADO DE CODIGO
            var num = (from obj in db.Pagos
                       select obj.NumeroPago).DefaultIfEmpty().Max();

            int codigo = 1;

            if (num != 0) {
                codigo = num + 1;
            }

            return Json(codigo, JsonRequestBehavior.AllowGet);
        }

        // GET: Facturaciones
        public ActionResult Index(string mensaje = "") {
            ViewBag.Message = mensaje;//MENSAJE DE RECARGO

            ViewBag.FormaPagoId = new SelectList(db.TiposDePago, "Id", "DescripcionTipoPago");
            ViewBag.MonedaId = new SelectList(db.Monedas, "Id", "DescripcionMoneda");

            return View();
        }

        /// <summary>
        /// CARGA LAS ORDENDES DEL CLIENTE
        /// </summary>
        /// <param name="ClienteId">CLIENTE ID</param>
        /// <returns></returns>
        public ActionResult CargarOrdenes(int ClienteId) {
            int cliente = 0;

            //SI EL ID DEL CLIENTE ES 0 - CARGAR LAS ORDENES DE LOS VISITANTES
            if (ClienteId == 0) {
                cliente = db.Clientes.Where(c => c.EmailCliente.Trim() == "defaultuser@xalli.com").Select(c => c.Id).FirstOrDefault();
            } else
                //SI ES DIF A 0 CARGAR LOS HUESPEDES
                cliente = ClienteId;

            //RECUPERO TODAS LAS ORDENES DE DET ID
            var orden = (from obj in db.Ordenes.ToList()
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         join det in db.DetallesDeOrden.ToList() on obj.Id equals det.OrdenId
                         join mes in db.Mesas.ToList() on obj.MesaId equals mes.Id
                         where obj.EstadoOrden == 2 && c.Id == cliente//TODAS LAS ORDENES SIN FACTURAR E INACTIVAS
                         group new { obj, d, det, mes } by new { obj.Id, obj.CodigoOrden, obj.FechaOrden, d.PNombre, d.PApellido, mes.DescripcionMesa } into grouped
                         select new {
                             OrdenId = grouped.Key.Id,
                             CodigoOrden = grouped.Key.CodigoOrden,
                             FechaOrden = grouped.Key.FechaOrden.ToShortDateString(),
                             Cliente = grouped.Key.PNombre.ToUpper() != "DEFAULT" ? grouped.Key.PNombre + " " + grouped.Key.PApellido : "N/A",
                             Mesero = (from o in db.Ordenes
                                       join m in db.Meseros on o.MeseroId equals m.Id
                                       join a in db.Datos on m.DatoId equals a.Id
                                       where grouped.Key.Id == o.Id
                                       select a.PNombre + " " + a.PApellido).FirstOrDefault(),
                             SubTotal = grouped.Sum(s => s.det.CantidadOrden * s.det.PrecioOrden),
                             Mesa = grouped.Key.DescripcionMesa
                         }).ToList();

            return Json(new { orden, ClienteId }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// OBTIENE EL DETALLE DE LAS ORDENES SELECCIONADAS DEL HUESPED
        /// </summary>
        /// <param name="ordenIds">RECUPERA TODAS LAS ORDENES SELECCIONADAS DEL CLIENTE</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CargarSeleccionadas(List<int> ordenIds, int clienteId = 0) {
            //CREO LAS INSTANCIA DONDE SE ALMACENARA LAS/LA ORDEN(ES)
            List<detalleCliente> ordenesCliente = new List<detalleCliente>();
            Imagen img = new Imagen();
            bool diplomatico = false;

            ordenIds.Sort();//ORDENO LA LISTA OBTENIDA

            //RECORRO LA LISTA DE IDS PARA OBTENER SU DETALLE
            foreach (var item in ordenIds) {
                //ALMACENO EL DETALLE EN EL OBJETO
                ordenesCliente.AddRange(IteracionObjeto(item));
                //BUSCO LA EVIDENCIA EN LA TABLA IMAGEN CON DIRECCION (/IMAGES/PAGO/)
                Imagen agregar = buscarEvidencia(item);//GUARDAMOS EL RESULTADO DE LA BUSQUEDA

                //SI LA BUSQUEDA RECUPERA ALGO Y DIPLOMATICO SIGUE EN FALSO
                if (agregar != null && diplomatico == false) {
                    img = agregar;//GUARDAMOS EL OBJETO DE IMAGEN
                    diplomatico = true;//ES DIPLOMATICO
                }
            }//FIN FOREACH

            //RECORRO LA LISTA DEL CLIENTE Y AGRUPO ELEMENTOS DE DIFERENTES ORDENES
            var detalleFinal = (from obj in ordenesCliente
                                group new { obj } by new { obj.Id, obj.Cantidad, obj.Precio, obj.Platillo } into grouped
                                select new detalleCliente {
                                    Id = grouped.Key.Id,
                                    Cantidad = grouped.Sum(s => s.obj.Cantidad),
                                    Platillo = grouped.Key.Platillo,
                                    Precio = grouped.Key.Precio
                                }).ToList();

            return Json(new { detalleFinal, img, diplomatico }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// BUSCA EL OBJETO CARNET EN CASO QUE EL PAGO TENGA EVIDENCIAS
        /// </summary>
        /// <param name="evidenciaCode">ID DEL PAGO</param>
        /// <returns></returns>
        public Imagen buscarEvidencia(int evidenciaCode) {
            string ruta = "/images/Carnet\\" + evidenciaCode + ".png";

            //SE REALIZA LA BUSQUEDA EN LA TABLA IMAGENES
            Imagen img = db.Imagenes.DefaultIfEmpty(null).FirstOrDefault(r => r.Ruta == ruta);

            return img;
        }

        /// <summary>
        /// OBTENGO LA CONSULTA DEL DETALLE DE ORDEN DEL CLIENTE (1X1)
        /// </summary>
        /// <param name="OrdenId">ID ACTUAL</param>
        /// <returns></returns>
        public List<detalleCliente> IteracionObjeto(int OrdenId) {
            //OBTENGO EL DETALLE DE DETERMINADA ORDEN
            List<detalleCliente> ordCliente = (from obj in db.Ordenes.ToList()
                                               join det in db.DetallesDeOrden.ToList() on obj.Id equals det.OrdenId
                                               join menu in db.Menus.ToList() on det.MenuId equals menu.Id
                                               where det.OrdenId == OrdenId
                                               group new { det, menu } by new { menu.DescripcionMenu, det.NotaDetalleOrden, det.PrecioOrden, det.MenuId } into grouped
                                               select new detalleCliente {
                                                   Id = grouped.Key.MenuId,
                                                   Cantidad = grouped.Sum(s => s.det.CantidadOrden),//SUMO LA CANTIDAD SOLICITADA EN CASO DE QUE SEA EL MISMO PRODUCTO
                                                   Platillo = grouped.Key.DescripcionMenu,
                                                   Precio = grouped.Key.PrecioOrden
                                               }).ToList();

            return ordCliente;
        }

        /// <summary>
        /// CARGA LA VISTA PARA AGREGAR UN CLIENTE DIPLOMATICO
        /// </summary>
        /// <returns></returns>
        public ActionResult ClienteDiplomatico(int clienteId) {
            ViewBag.Cliente = clienteId;

            return View();
        }

        [HttpPost]
        public ActionResult saveDiplomatico(string Nombre, string Apellido, string Documento, string RUC, string Email, string Telefono, uint? Tipo) {
            //var clienteId = 0;//PARA ENVIAR EN OTRAS VISTAS

            //BUSCAR QUE EL RUC INGRESADO NO EXISTA
            //var bruc = db.Datos.DefaultIfEmpty(null).FirstOrDefault(r => r.RUC == RUC.Trim());

            ////SI EL NUMERO RUC YA SE ENCUENTRA REGISTRADO
            //if (bruc != null) {
            //    mensaje = "El número RUC ya se encuentra registrado";
            //    return Json(new { mensaje });
            //}

            //SE BUSCA DESDE LAS DOS TABLAS
            Cliente cliente = new Cliente();
            Dato dato = new Dato();

            var persona = (dynamic)null;

            //SI ES UN CLIENTE NACIONAL
            if (Tipo == 1) {
                dato = db.Datos.DefaultIfEmpty(null).FirstOrDefault(t => t.Cedula.Trim() == Documento.Trim());
                cliente = dato != null ? db.Clientes.DefaultIfEmpty(null).FirstOrDefault(c => c.DatoId == dato.Id) : null;
            } else {
                //CLIENTE EXTRANJERO
                cliente = db.Clientes.DefaultIfEmpty(null).FirstOrDefault(c => c.PasaporteCliente.Trim() == Documento.Trim());
                dato = cliente != null ? db.Datos.DefaultIfEmpty(null).FirstOrDefault(d => d.Id == cliente.DatoId) : null;
            }//FIN BUSCAR

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //SI EXISTE ALGUN REGISTRO DE CLIENTE
                    if (dato != null && cliente != null) {
                        //ACTUALIZAMOS EL OBJETO
                        dato.PNombre = Nombre;
                        dato.PApellido = Apellido;
                        dato.RUC = RUC != "" ? RUC : null;

                        db.Entry(dato).State = EntityState.Modified;

                        //SI EXISTE UN REGISTRO CLIENTE
                        if (db.SaveChanges() > 0) {
                            cliente.TelefonoCliente = Telefono != "" ? Telefono : null;
                            cliente.EmailCliente = Email != "" ? Email : null;

                            db.Entry(cliente).State = EntityState.Modified;
                            completado = db.SaveChanges() > 0 ? true : false;
                            mensaje = "Actualizado correctamente";

                        }//SI SOLO HAY REGISTRO EN DATO PERO NO EN CLIENTE
                    } else if (dato != null && cliente == null) {
                        //SI SOLO EXISTE LOS DATOS DE LA PERSONA   

                        //MODIFICO
                        dato.RUC = RUC != "" ? RUC : null;
                        dato.PNombre = Nombre;
                        dato.PApellido = Apellido;
                        //SE ACTUALIZA EL CAMPO
                        db.Entry(dato).State = EntityState.Modified;

                        //Cliente client = new Cliente();
                        cliente = new Cliente();
                        //SE COMPRUEBA EL TIPO DE DOCUMENTO PARA ACTUALIZAR
                        if (Tipo == 2) {
                            cliente.PasaporteCliente = Documento;
                        }
                        //ALMACENAR LOS CAMPOS DE CLIENTE
                        cliente.EmailCliente = Email != "" ? Email : null;
                        cliente.TelefonoCliente = Telefono != "" ? Telefono : null;
                        cliente.EstadoCliente = true;
                        cliente.DatoId = dato.Id;

                        db.Clientes.Add(cliente);
                        completado = db.SaveChanges() > 0 ? true : false;
                        mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                        //clienteId = client.Id;
                    } else {
                        //SI NO EXISTE REGISTRO EN NINGUNO DE LAS DOS TABLAS
                        //Dato data = new Dato();

                        dato = new Dato();
                        cliente = new Cliente();

                        //SI EL DOCUMENTO ES CEDULA
                        if (Tipo == 1) {
                            dato.Cedula = Documento.Trim();
                        }
                        dato.PNombre = Nombre;
                        dato.PApellido = Apellido;
                        dato.RUC = RUC != "" ? RUC : null;

                        db.Datos.Add(dato);

                        if (db.SaveChanges() > 0) {
                            //ALMACENAR DATOS DE CLIENTE
                            //Cliente customer = new Cliente();

                            if (Tipo == 2) {
                                cliente.PasaporteCliente = Documento;
                            }
                            cliente.EmailCliente = Email != "" ? Email : null;
                            cliente.TelefonoCliente = Telefono != "" ? Telefono : null;
                            cliente.EstadoCliente = true;
                            cliente.DatoId = dato.Id;

                            db.Clientes.Add(cliente);

                            completado = db.SaveChanges() > 0 ? true : false;
                            mensaje = completado ? "Almacenado correctamente" : "Error al almacenar";
                            //clienteId = customer.Id;
                        }
                    }

                    transact.Commit();

                    persona = (from obj in db.Datos.ToList()
                               join client in db.Clientes.ToList() on obj.Id equals client.DatoId
                               where obj.Id == dato.Id && client.Id == cliente.Id
                               select new {
                                   Id = client.Id,
                                   Nombre = dato.PNombre + " " + dato.PApellido,
                                   RUC = dato.RUC,
                                   DatoId = dato.Id
                               }).FirstOrDefault();

                } catch (DbEntityValidationException dbEx) {
                    mensaje = "Error al modificar";
                    transact.Rollback();

                    //CAPTURAR ERRORES DE VALIDACION
                    foreach (var validationErrors in dbEx.EntityValidationErrors) {
                        foreach (var validationError in validationErrors.ValidationErrors) {
                            System.Console.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }//FIN TRY-CATCH
            }//FIN USING

            return Json(new { success = completado, message = mensaje, cliente = persona }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(string OrdenesIds, int ClienteId, int NoFactura, DateTime FechaPago, bool Diplomatico, int DescuentoPago, double Propina, double Cambio, int MonedaPropina, int EvidenciaId, string DetallePago) {

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //DESERIALIZACION DE OBJETO JSON     
                    var detallePagar = JsonConvert.DeserializeObject<List<DetalleDePago>>(DetallePago);
                    var listaOrdenes = OrdenesIds.Split(',');

                    //INSTANCIAS DE OBJETOS A ALMACENAR
                    Pago pago = new Pago();

                    //ALMACENAR PAGO
                    pago.FechaPago = FechaPago;
                    pago.NumeroPago = NoFactura;
                    pago.Descuento = DescuentoPago;
                    pago.IVA = 15;
                    pago.Propina = Propina;
                    pago.TipoCambio = Cambio;
                    pago.MonedaId = MonedaPropina;

                    //SI EL CLIENTE ES DIPLOMATICO
                    if (Diplomatico) {
                        pago.ImagenId = EvidenciaId;

                        //MODIFICAMOS LA/LAS ORDEN/ES DEL PAGO CAMBIANDO CLIENTE ID
                        foreach (var item in listaOrdenes) {
                            var data = int.Parse(item);
                            ModificarOrdenes(ClienteId, data, false);
                        }
                    } else {
                        //SI EL CLIENTE ES DIPLOMATICO
                        Imagen evidenciaCarnet = db.Imagenes.DefaultIfEmpty(null).FirstOrDefault(i => i.Ruta == "N/A");

                        //SI NO EXISTE DEFAULT DE CARNET CREARLO
                        if (evidenciaCarnet == null) {
                            evidenciaCarnet = new Imagen();//"RESETEAMOS LA INSTANCIA"

                            evidenciaCarnet.Ruta = "N/A";
                            db.SaveChanges();
                        }
                        //ASIGNAMOS EL CARNET POR DEFAULT
                        pago.ImagenId = evidenciaCarnet.Id;
                    }

                    db.Pagos.Add(pago);
                    //SI SE ALMACENO CORRECTAMENTE
                    if (db.SaveChanges() > 0) {
                        bool detalleAlmacenado = false;

                        foreach (var item in detallePagar) {
                            detalleAlmacenado = false;

                            DetalleDePago detalleDePago = new DetalleDePago();

                            //AGREGAR EL DETALLE
                            detalleDePago.CantidadPagar = item.CantidadPagar;
                            detalleDePago.MontoRecibido = item.MontoRecibido;
                            detalleDePago.TipoPagoId = item.TipoPagoId;
                            detalleDePago.MonedaId = item.MonedaId;
                            detalleDePago.PagoId = pago.Id;

                            db.DetallesDePago.Add(detalleDePago);
                            detalleAlmacenado = db.SaveChanges() > 0 ? true : false;
                        }

                        //SI TODOS LOS OBJETOS DEL DETALLE SE ALMACENO CORRECTAMENTE
                        if (detalleAlmacenado) {
                            //SE ALMACENA LAS ORDENES Y PAGOS IDS INVOLUCRADOS
                            foreach (var item in listaOrdenes) {
                                var data = int.Parse(item);

                                //SE CREA LA INSTANCIA A GUARDAR
                                OrdenPago ordenPago = new OrdenPago();

                                ordenPago.OrdenId = data;
                                ordenPago.PagoId = pago.Id;

                                db.OrdenesPago.Add(ordenPago);
                                completado = db.SaveChanges() > 0 ? true : false;
                            }

                            if (completado) {

                                //MODIFICAMOS EL ESTADO DE LA/LAS ORDEN/ES DEL PAGO
                                foreach (var item in listaOrdenes) {
                                    var data = int.Parse(item);
                                    ModificarOrdenes(0, data, true);
                                }

                                mensaje = completado ? "Pago almacenado correctamente" : "Error al almacenar el pago";
                            }
                        }
                    }

                    transact.Commit();

                } catch (Exception) {
                    mensaje = "Error al almacenar el pago";
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        public void ModificarOrdenes(int ClienteId, int OrdenId, bool Finalizado) {
            //BUSCAMOS LA ORDEN A MODIFICAR
            var modificarOrden = db.Ordenes.DefaultIfEmpty(null).FirstOrDefault(o => o.Id == OrdenId);

            //SI SE ENCUENTRA LA ORDEN
            if (modificarOrden != null) {
                //SI HAY QUE ACTUALIZAR EL CLIENTE ID
                if (ClienteId != 0) {
                    modificarOrden.ClienteId = ClienteId;//CAMBIAR EL ID CLIENTE
                } else if (Finalizado) {
                    //ACTUALIZAR EL ESTADO DE LA ORDEN
                    modificarOrden.EstadoOrden = 3;//FACTURADO
                }

                db.Entry(modificarOrden).State = EntityState.Modified;
                completado = db.SaveChanges() > 0 ? true : false;
            }
        }

        /// <summary>
        /// CLASE INTERNA DONDE SE ALMACENA EL DETALLE DE LA ORDEN DE CLIENTES
        /// </summary>
        public class detalleCliente {
            public int Id { get; set; }
            public int Cantidad { get; set; }
            public string Platillo { get; set; }
            public double Precio { get; set; }
        }
    }
}