using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MenuAPI.Areas.API.Models;
using ProyectoXalli_Gentelella.Models;

namespace MenuAPI.Areas.API.Controllers
{
    public class MenusWSController : Controller
    {
        //conexion con la db
        private DBControl db = new DBControl();

        //obtener todos los menus
        [HttpGet]
        public async Task<JsonResult> Menus()
        {
            var menus = await (from m in db.Menus
                         select new MenuWS
                         {
                             id = m.Id,
                             codigo = m.CodigoMenu,
                             descripcion = m.DescripcionMenu,
                             tiempoestimado= m.TiempoEstimado,
                             precio = m.PrecioMenu,
                             estado = m.EstadoMenu,
                             idcategoria = m.CategoriaMenuId
                            
                              }).ToListAsync();

            return Json(menus, JsonRequestBehavior.AllowGet);
        }

        //obtener un menu
        [HttpGet]
        public JsonResult Menu(int id)
        {
            var menu = (from m in db.Menus
                        where m.Id == id
                              select new MenuWS
                              {
                                  id = m.Id,
                                  codigo = m.CodigoMenu,
                                  descripcion = m.DescripcionMenu,
                                  precio = m.PrecioMenu,
                                  estado = m.EstadoMenu

                              }).ToList();

            return Json(menu , JsonRequestBehavior.AllowGet);
        }

        //obtener un menu por la categoria
        [HttpGet]
        public async Task<JsonResult> MenusCategoria(int id)
        {
            //ruta de la imagen desde la db para la local
            string root = "http://192.168.1.52/ProyectoXalli_Gentelella";
          
            var menu = await (from m in db.Menus
                              join i in db.Imagenes on m.ImagenId equals i.Id
                              where m.CategoriaMenuId == id
                        select new MenuWS
                        {
                            id = m.Id,
                            codigo = m.CodigoMenu,
                            descripcion = m.DescripcionMenu,
                            tiempoestimado = m.TiempoEstimado,
                            precio = m.PrecioMenu,
                            estado = m.EstadoMenu,
                            ruta = root + m.Imagen.Ruta,
                            idcategoria = m.CategoriaMenuId,

                        }).ToListAsync();

            return Json(menu, JsonRequestBehavior.AllowGet);
        }

        //cerrando la db
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