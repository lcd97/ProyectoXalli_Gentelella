using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using MenuAPI.Areas.API.Models;
using ProyectoXalli_Gentelella.Areas.API;
using ProyectoXalli_Gentelella.Models;

namespace MenuAPI.Areas.API.Controllers
{
    [BasicAuthentication]
    public class CategoriasWSController : Controller
    {
        //conexion con la DB
        private DBControl db = new DBControl();

        //obteniendo las categorias del menu
        [HttpGet]
        public async Task<JsonResult> Categorias()
        {
            var categorias = await (from c in db.CategoriasMenu.Where(c =>c.EstadoCategoriaMenu == true)
                                    join b in db.Bodegas on c.BodegaId equals b.Id
                              select new CategoriaWS
                              {
                                  id = c.Id,
                                  codigo = c.CodigoCategoriaMenu,
                                  descripcion = c.DescripcionCategoriaMenu,
                                  estado = c.EstadoCategoriaMenu,
                                  bar = b.CodigoBodega == "B01" ? true : false
                              }).ToListAsync();

            return Json(categorias, JsonRequestBehavior.AllowGet);
        }

        //Obtener una categoria
        [HttpGet]
        public async Task<JsonResult> Categoria(int id)
        {
            var categoria = await (from c in db.CategoriasMenu
                                   where c.Id == id
                                   select new CategoriaWS
                                   {
                                       id = c.Id,
                                       codigo = c.CodigoCategoriaMenu,
                                       descripcion = c.DescripcionCategoriaMenu,
                                       estado = c.EstadoCategoriaMenu

                                   }).DefaultIfEmpty().FirstOrDefaultAsync();

       
            return Json(categoria, JsonRequestBehavior.AllowGet);

        }

        //Cerrar la db
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
