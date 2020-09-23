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
    public class InventarioWSController : Controller
    {
        //conexion con la db
        private DBControl db = new DBControl();

        //consultar la existencia de un producto 
        [HttpGet]
        public async Task<JsonResult> Existencia(int id)
        {
            int existencia;

            if (await esDeBar(id))
            {
                int salidas;
                double entradas;

                int idProd = await (from m in db.Menus
                                    join i in db.Ingredientes on m.Id equals i.MenuId
                                    where i.MenuId == id
                                    select i.ProductoId).DefaultIfEmpty(-1).FirstOrDefaultAsync();

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
                //no se debe validar en el menu
                existencia = -2;
            }

            return Json(existencia, JsonRequestBehavior.AllowGet);
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
    }
}