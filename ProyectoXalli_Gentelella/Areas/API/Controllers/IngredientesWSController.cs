using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MenuAPI.Areas.API.Models;
using ProyectoXalli_Gentelella.Areas.API;
using ProyectoXalli_Gentelella.Models;

namespace MenuAPI.Areas.API.Controllers
{
    [BasicAuthentication]
    public class IngredientesWSController : Controller
    {
        //conexion con la DB
        private DBControl db = new DBControl();

        //obtener los ingredientes de un menu
        [HttpGet]
        public async Task<JsonResult> IngredientesMenu(int id)
        {
            var ingredientes = await (from p in db.Productos
                                join u in db.UnidadesDeMedida on p.UnidadMedidaId equals u.Id 
                                join i in db.Ingredientes on p.Id equals i.ProductoId
                                join m in db.Menus on i.MenuId equals m.Id
                                where m.Id == id
                                select new IngredienteWS
                                {
                                    descripcion = p.NombreProducto +" "+ p.MarcaProducto
                                }).ToListAsync();

            return Json(ingredientes,JsonRequestBehavior.AllowGet);
        }


        //cerar la conexion con la db
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