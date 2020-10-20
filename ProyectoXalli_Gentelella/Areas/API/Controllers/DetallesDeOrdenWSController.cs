using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MenuAPI.Areas.API.Models;
using Newtonsoft.Json;
using MenuAPI;
using System.Threading.Tasks;
using System.Data.Entity;
using ProyectoXalli_Gentelella.Models;
using ProyectoXalli_Gentelella.Areas.API;

namespace MenuAPI.Areas.API.Controllers
{
    [BasicAuthentication]
    public class DetallesDeOrdenWSController : Controller
    {

        private DBControl db = new DBControl();

        
        //Obtener los detalles de orden de una orden dada
        [HttpGet]
        public async Task<JsonResult> DetalleDeOrden(int id)
        {
            var DetalleDeOrdenes = await (from dor in db.DetallesDeOrden
                                          join m in db.Menus on dor.MenuId equals m.Id                                       
                                          where dor.OrdenId == id
                                          select new DetallesDeOrdenWS
                                          {
                                              id = dor.Id,
                                              cantidadorden =  dor.CantidadOrden,
                                              notaorden = dor.NotaDetalleOrden,
                                              nombreplatillo = m.DescripcionMenu,
                                              preciounitario = dor.PrecioOrden,
                                              estado = dor.EstadoDetalleOrden,
                                              menuid = dor.MenuId,
                                              ordenid = dor.OrdenId,
                                              fromservice = true

                                          }).ToListAsync();

            return Json(DetalleDeOrdenes, JsonRequestBehavior.AllowGet);

        }

        //Obtener los detalles de orden de una orden dada
        [HttpGet]
        public async Task<JsonResult> DetalleDeOrdenCuenta(int id)
        {
            var DetalleDeOrdenes = await (from dor in db.DetallesDeOrden
                                          join m in db.Menus on dor.MenuId equals m.Id
                                          where dor.OrdenId == id
                                          group dor by new { m.DescripcionMenu, dor.PrecioOrden, dor.EstadoDetalleOrden, dor.MenuId, dor.OrdenId } into g
                                          select new DetallesDeOrdenWS
                                          {
                                              cantidadorden = g.Sum(x=>x.CantidadOrden),
                                              nombreplatillo = g.Key.DescripcionMenu,
                                              preciounitario = g.Key.PrecioOrden,
                                              estado = g.Key.EstadoDetalleOrden,
                                              menuid = g.Key.MenuId,
                                              ordenid = g.Key.OrdenId,
                                              fromservice = true

                                          }).ToListAsync();

            return Json(DetalleDeOrdenes, JsonRequestBehavior.AllowGet);

        }

        // Agregar nueva orden y nuevo detalle
        [HttpPost]
        public async Task<JsonResult> OrdenesDetalle(OrdenWS ordenWS, List<DetallesDeOrdenWS> detallesWS)
        {
            ResultadoWS resultadoWS = new ResultadoWS();

            using (var transact = db.Database.BeginTransaction())
            {
                try
                {
                    //orden
                    Orden orden = new Orden();

                    //por default
                    Dato datop = new Dato();
                    Cliente clientep = new Cliente();

                    //si es un cliente
                    Cliente cliente = new Cliente();

                    //Guardar la orden
                    orden.CodigoOrden = ordenWS.codigo;
                    orden.FechaOrden = ordenWS.fechaorden.Date + ordenWS.tiempoorden.TimeOfDay;
                    orden.EstadoOrden = ordenWS.estado;
                    orden.MeseroId = ordenWS.meseroid;

                    //Buscar el cliente
                    if (ordenWS.clienteid == -1)
                    {
                        var buscarP = db.Datos.DefaultIfEmpty(null).FirstOrDefault(b => b.Cedula == "000-000000-0000X");

                        if (buscarP == null)
                        {
                            datop.Cedula = "000-000000-0000X";
                            datop.PNombre = "DEFAULT";
                            datop.PApellido = "USER";

                            db.Datos.Add(datop);
                            db.SaveChanges();

                            clientep.DatoId = datop.Id;
                            clientep.EmailCliente = "defaultuser@xalli.com";
                            clientep.EstadoCliente = false;

                            db.Clientes.Add(clientep);
                            db.SaveChanges();

                            cliente = clientep;
                        }
                        else
                        {
                            clientep = db.Clientes.FirstOrDefault(c => c.DatoId == buscarP.Id);

                        }

                        orden.ClienteId = clientep.Id;

                    }
                    else
                    {
                        orden.ClienteId = ordenWS.clienteid;
                    }

                    //Buscar la imagen
                    var comandap = db.Imagenes.DefaultIfEmpty(null).FirstOrDefault(i => i.Ruta == "N/A");
                    Imagen img = new Imagen();

                    if (comandap == null)
                    {
                        //SE CREA EL DEFAUTL
                        img.Ruta = "N/A";
                        db.Imagenes.Add(img);
                        db.SaveChanges();
                    }

                    //ALMACENAMOS UNA PLANTILLA
                    orden.ImagenId = comandap != null ? comandap.Id : img.Id;

                    //guardar la orden
                    db.Ordenes.Add(orden);

                    if (db.SaveChanges() > 0)
                    {
                        foreach (var DetalleActual in detallesWS)
                        {
                            if (await esDeBar(DetalleActual.menuid))
                            {
                                int existencias = await existencia(DetalleActual.menuid);

                                if (existencias >= DetalleActual.cantidadorden)
                                {
                                    DetalleDeOrden detallesDeOrden = new DetalleDeOrden
                                    {
                                        CantidadOrden = DetalleActual.cantidadorden,
                                        NotaDetalleOrden = DetalleActual.notaorden != "" ? DetalleActual.notaorden : null,
                                        PrecioOrden = DetalleActual.preciounitario,
                                        EstadoDetalleOrden = DetalleActual.estado,
                                        MenuId = DetalleActual.menuid,
                                        OrdenId = orden.Id
                                    };

                                    db.DetallesDeOrden.Add(detallesDeOrden);

                                }
                                else
                                {
                                    resultadoWS.Mensaje = "La existencia es menor que la cantidad especificada del producto: " + DetalleActual.nombreplatillo;
                                    resultadoWS.Resultado = false;
                                    throw new Exception();
                                }
                            }
                            else
                            {
                                DetalleDeOrden detallesDeOrden = new DetalleDeOrden
                                {
                                    CantidadOrden = DetalleActual.cantidadorden,
                                    NotaDetalleOrden = DetalleActual.notaorden,
                                    PrecioOrden = DetalleActual.preciounitario,
                                    EstadoDetalleOrden = DetalleActual.estado,
                                    MenuId = DetalleActual.menuid,
                                    OrdenId = orden.Id
                                };

                                db.DetallesDeOrden.Add(detallesDeOrden);
                            }
                        }

                        if (db.SaveChanges() > 0)
                        {
                            resultadoWS.Mensaje = "Almecenado con exito";
                            resultadoWS.Resultado = true;
                            transact.Commit();
                            //falta poner lo del websocket :v
                        }
                        else
                        {
                            throw new Exception();
                        }

                    }
                }
                catch (Exception)
                {
                    if (resultadoWS.Mensaje.Length == 0)
                    {
                        resultadoWS.Mensaje = "Error al guardar la orden";
                        resultadoWS.Resultado = false;
                    }
                    transact.Rollback();
                }
            }

            return Json(resultadoWS);
        }

        [HttpPost]
        public async Task<JsonResult> NuevosDetalle(DateTime fechaOrden, List<DetallesDeOrdenWS> nuevoDetallesWS)
        {
            ResultadoWS resultadoWS = new ResultadoWS();
            resultadoWS.Mensaje = "";
            resultadoWS.Resultado = false;

            using (var transact = db.Database.BeginTransaction())
            {
                try
                {
                    var idorden = nuevoDetallesWS[0].ordenid;
                    var Orden = db.Ordenes.DefaultIfEmpty(null).FirstOrDefault(o => o.Id == idorden);

                    if (Orden != null)
                    {
                        Orden.FechaOrden = Convert.ToDateTime(fechaOrden);//CAMBIO LA HORA DEL PEDIDO
                        db.Entry(Orden).State = EntityState.Modified;

                        if (db.SaveChanges() > 0)
                        {
                            foreach (var DetalleActual in nuevoDetallesWS)
                            {
                                if (await esDeBar(DetalleActual.menuid))
                                {
                                    int existencias = await existencia(DetalleActual.menuid);

                                    if (existencias >= DetalleActual.cantidadorden)
                                    {
                                        DetalleDeOrden detallesDeOrden = new DetalleDeOrden
                                        {
                                            CantidadOrden = DetalleActual.cantidadorden,
                                            NotaDetalleOrden = DetalleActual.notaorden != "" ? DetalleActual.notaorden : null,
                                            PrecioOrden = DetalleActual.preciounitario,
                                            EstadoDetalleOrden = DetalleActual.estado,
                                            MenuId = DetalleActual.menuid,
                                            OrdenId = DetalleActual.ordenid
                                        };

                                        db.DetallesDeOrden.Add(detallesDeOrden);

                                    }
                                    else
                                    {
                                        resultadoWS.Mensaje = "La existencia es menor que la cantidad especificada del producto: " + DetalleActual.nombreplatillo;
                                        resultadoWS.Resultado = false;
                                        throw new Exception();
                                    }
                                }
                                else
                                {
                                    DetalleDeOrden detallesDeOrden = new DetalleDeOrden
                                    {
                                        CantidadOrden = DetalleActual.cantidadorden,
                                        NotaDetalleOrden = DetalleActual.notaorden,
                                        PrecioOrden = DetalleActual.preciounitario,
                                        EstadoDetalleOrden = DetalleActual.estado,
                                        MenuId = DetalleActual.menuid,
                                        OrdenId = DetalleActual.ordenid
                                    };

                                    db.DetallesDeOrden.Add(detallesDeOrden);
                                }

                            }

                            if (db.SaveChanges() > 0)
                            {
                                resultadoWS.Mensaje = "Almecenado con exito";
                                resultadoWS.Resultado = true;
                                transact.Commit();
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    if (resultadoWS.Mensaje.Length == 0)
                    {
                        resultadoWS.Mensaje = "Error al guardar el detalle de orden";
                        resultadoWS.Resultado = false;
                    }
                    transact.Rollback();
                }
            }

            return Json(resultadoWS);
        }

        public async Task<int> existencia(int id)
        {
            int existencia;
            int salidas;
            double entradas;

                int idProd = await (from m in db.Menus
                                   join i in db.Ingredientes on m.Id equals i.MenuId
                                   where i.MenuId == id
                                   select i.ProductoId).DefaultIfEmpty(-1).FirstOrDefaultAsync();

                if (idProd != -1)
                {
                    salidas = await (from m in db.Menus
                                    join dor in db.DetallesDeOrden on m.Id equals dor.MenuId
                                    join o in db.Ordenes on dor.OrdenId equals o.Id
                                    where m.Id == id
                                    select dor.CantidadOrden).DefaultIfEmpty(0).SumAsync();

                    entradas = await (from p in db.Productos
                                     join de in db.DetallesDeEntrada on p.Id equals de.ProductoId
                                     join e in db.Entradas on de.EntradaId equals e.Id
                                     where p.Id == idProd
                                     select de.CantidadEntrada).DefaultIfEmpty(0).SumAsync();

                    //calculando las existencias
                    existencia = (int)entradas - salidas;
                }
                else
                {
                    //Este menu no tiene un producto relacionado
                    existencia = -1;
                }
            
            return existencia;
        }

        //si el menuid es de bar o no 
        public async Task<bool> esDeBar(int id)
        {
            string Bodega = await (from c in db.CategoriasMenu
                                   join b in db.Bodegas on c.BodegaId equals b.Id
                                   join m in db.Menus on c.Id equals m.CategoriaMenuId
                                   where m.Id == id
                                   select b.CodigoBodega).DefaultIfEmpty().FirstOrDefaultAsync();

            if (Bodega == "B01")
            {
                return true;
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    } 
}