using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Areas.API.Controllers
{
    [BasicAuthentication]
    public class InventarioWSController : Controller
    {
        //conexion con la db
        private DBControl db = new DBControl();
        private string mensaje = "";

        ////consultar la existencia de un producto 
        //[HttpGet]
        //public async Task<JsonResult> Existencia(int id)
        //{
        //    int existencia;

        //    if (await esDeBar(id))
        //    {
        //        int salidas;
        //        double entradas;

        //        int idProd = await (from m in db.Menus
        //                            join i in db.Ingredientes on m.Id equals i.MenuId
        //                            where i.MenuId == id
        //                            select i.ProductoId).DefaultIfEmpty(-1).FirstOrDefaultAsync();

        //        salidas = await (from m in db.Menus
        //                         join dor in db.DetallesDeOrden on m.Id equals dor.MenuId
        //                         join o in db.Ordenes on dor.OrdenId equals o.Id
        //                         where m.Id == id
        //                         select dor.CantidadOrden).DefaultIfEmpty(0).SumAsync();

        //        entradas = await (from p in db.Productos
        //                          join de in db.DetallesDeEntrada on p.Id equals de.ProductoId
        //                          join e in db.Entradas on de.EntradaId equals e.Id
        //                          where p.Id == idProd
        //                          select de.CantidadEntrada).DefaultIfEmpty(0).SumAsync();

        //        //calculando las existencias
        //        existencia = (int)entradas - salidas;
        //    }
        //    else
        //    {
        //        //no se debe validar en el menu
        //        existencia = -2;
        //    }

        //    return Json(existencia, JsonRequestBehavior.AllowGet);
        //}

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

        [HttpGet]
        /// <summary>
        /// CONSULTA LA EXISTENCIAS 
        /// </summary>
        /// <param name="id">ID DE MENU</param>
        /// <returns></returns>
        public async Task<ActionResult> existencia(int id)
        {
            bool completo = true;
            double entradas = 0;
            int salidas = 0, existencia = 0;

            //SI LA CATEGORIA DEL PLATILLO PERTENECE A BAR
            if (await esDeBar(id))
            {

                //LISTAR TODOS LOS PRODUCTOS QUE CONFORMAN EL MENU
                var idProd = (from m in db.Menus
                              join i in db.Ingredientes on m.Id equals i.MenuId
                              where i.MenuId == id
                              select i.ProductoId).ToList();

                //EN ESTA CONDICION SE SACARA LA EXISTENCIA EN NUMEROS-EN CASO QUE SEA
                if (idProd.Count == 1)
                {
                    ExistEntrada(idProd[0], ref entradas);//OBTENEMOS LA ENTRADA DEL PRODUCTO

                    if (entradas == 0)
                    {
                        //NO TIENE ENTRADAS, NO HAY EXISTENCIA
                        mensaje = "No disponible";
                        existencia = 0;
                    }
                    else
                    {
                        //BUSCAR LA BEBIDA PARA SABER SI ES TRAGO O BOTELLA
                        var menu = db.Menus.Find(id);

                        //SI HAY ENTRADAS BUSCAR LAS SALIDAS
                        ExistSalidas(idProd[0], ref salidas);//OBTENEMOS LAS SALIDAS DEL PRODUCTO                    

                        if (menu.Inventariado)
                        {
                            existencia = (int)entradas - salidas;//CALCULO DE LA EXISTENCIA
                        }
                        else
                        {
                            //SI ES UN TRAGO
                            existencia = (int)entradas - salidas;//CALCULO DE LA EXISTENCIA
                            mensaje = existencia.ToString();//MANDO LA CANTIDAD DE EXISTENCIA DEL PRODUCTO
                            //MANDO -2 PARA QUE PUEDA AGREGAR CUANTOS TRAGOS SE NECESITEN
                            existencia = -2;
                        }
                    }
                }
                else
                {
                    int w = 0;//CONTADOR DE WHILE

                    //RECORRER LA LISTA DE LOS INGREDIENTES Y COMRPOBAR QUE TENGA ENTRADAS DEL AREA DE BODEGA
                    while (w < idProd.Count && completo)
                    {
                        entradas = 0;
                        salidas = 0;

                            if (ExistEntrada(idProd[w], ref entradas))
                            {//SI LAS ENTRADAS PERTENECEN AL AREA DE BAR
                             //SI EL PRODUCTO ES DE BAR
                                if (entradas > 0)
                                {//SI LAS ENTRADAS FUERON MAYOR A 0
                                    ExistSalidas(idProd[w], ref salidas);//CALCULAR LAS SALIDAS

                                    existencia = (int)entradas - salidas;//CALCULO LA EXISTENCIA

                                    //NO HAY UN PRODUCTO EN EXISTENCIA
                                    if (existencia == 0)
                                    {
                                        completo = false;
                                        mensaje = "Productos faltantes";
                                        existencia = -1;//NO PUEDE SELECCIONAR PARA ORDENAR
                                    }
                                    else
                                    {
                                        mensaje = "Disponible";
                                        existencia = -2;//PUEDE SELECCIONAR PARA ORDENAR
                                    }
                                }
                            }
                        w++;
                    }
                }//FIN IF-ELSE
            }
            else
            {
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


        public bool ExistEntrada(int prodId, ref double entradas)
        {
            bool esBar = false;

            //SE COMPRUEBA QUE HAYAN ENTRADAS EN EL BAR            
            var entradaProd = (from obj in db.Entradas
                               join de in db.DetallesDeEntrada on obj.Id equals de.EntradaId
                               join b in db.Bodegas on obj.BodegaId equals b.Id
                               where de.ProductoId == prodId && b.CodigoBodega == "B01"
                               group new { de, b } by new { b.CodigoBodega } into grouped
                               select new
                               {
                                   Entradas = grouped.Sum(s => s.de.CantidadEntrada),//SUMAR TODAS LAS ENTRADAS
                                   Bar = grouped.Key.CodigoBodega == "B01" ? true : false//DETERMINAR SI LAS ENTRADAS SON DE BAR O BODEGA
                               }).FirstOrDefault();

            //SI CONTIENE AL MENOS UN ELEMENTO EL PRODUCTO TIENE EXISTENCIA
            if (entradaProd != null)
            {
                entradas = (double)entradaProd.Entradas;
                esBar = entradaProd.Bar;
            }

            return esBar;
        }

        public void ExistSalidas(int item, ref int salidas)
        {
            //BUSCAR TODOS LOS PLATILLOS DEL AREA DE BAR QUE TENGAN EL INGREDIENTE            
            var salidasProd = (from obj in db.Menus
                               join i in db.Ingredientes on obj.Id equals i.MenuId
                               join c in db.CategoriasMenu on obj.CategoriaMenuId equals c.Id
                               join b in db.Bodegas on c.BodegaId equals b.Id
                               join d in db.DetallesDeOrden on obj.Id equals d.MenuId
                               where i.ProductoId == item && b.CodigoBodega == "B01" && obj.Inventariado == true
                               select (int?)d.CantidadOrden).Sum();

            //SI CONTIENE AL MENOS UN ELEMENTO EL PRODUCTO TIENE EXISTENCIA
            if (salidasProd != null)
            {
                salidas = (int)salidasProd;
            }
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