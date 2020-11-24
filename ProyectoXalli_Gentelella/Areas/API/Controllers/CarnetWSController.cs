using MenuAPI.Areas.API.Models;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Areas.API.Controllers
{
    public class CarnetWSController : Controller
    {
        //conexion con la db
        private DBControl db = new DBControl();

        [HttpPost]
        public async Task<JsonResult> addPhotoCarnet(HttpPostedFileBase photo)
        {
            ResultadoWS resultadoWS = new ResultadoWS();

            string path = Server.MapPath("~/images/Carnet");

            //CREA EL DIRECTORIO DONDE SE ALMACENARN LAS FOTOS, EN CASO NO EXISTA
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //validanado photo no sea null
            if (photo != null || Request.Files[0].ContentLength > 0)
            {
                using (var transact = db.Database.BeginTransaction())
                {
                    try
                    {
                        string imgexte = ("/images/Carnet" + @"\" + photo.FileName);

                        var img = await db.Imagenes.DefaultIfEmpty(null).FirstOrDefaultAsync(i => i.Ruta == imgexte.Trim());

                        if (img == null)
                        {
                            //Crear la ruta y guardar la imagen
                            path = Path.Combine(Server.MapPath("~/images/Carnet"), photo.FileName);
                            photo.SaveAs(path);

                            Imagen imagen = new Imagen();

                            //obtener la url para la ruta concatenado con la carpeta
                            string url = Path.Combine("/images/Carnet", photo.FileName);

                            //asignar la ruta a la nueva imagen
                            imagen.Ruta = url;

                            //agregamos la nueva imagen a la db
                            db.Imagenes.Add(imagen);

                            if (await db.SaveChangesAsync() > 0)
                            {
                                resultadoWS.Mensaje = "Almecenado con exito";
                                resultadoWS.Resultado = true;
                                transact.Commit();
                            }
                            else
                            {
                                resultadoWS.Mensaje = "Error al guardar la imagen";
                                resultadoWS.Resultado = false;
                                throw new Exception();
                            }
                        }
                        else
                        {
                            //modificar
                            path = Path.Combine(Server.MapPath("~/images/Carnet"), photo.FileName);
                            photo.SaveAs(path);

                            resultadoWS.Mensaje = "Almecenado con exito";
                            resultadoWS.Resultado = true;
                        }
                    }
                    catch (Exception)
                    {
                        resultadoWS.Mensaje = "Error al guardar la imagen";
                        resultadoWS.Resultado = false;
                        transact.Rollback();
                    }
                }
            }
            else
            {
                resultadoWS.Mensaje = "No se ha enviado ninguna imagen";
                resultadoWS.Resultado = false;
            }

            return Json(resultadoWS, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult consultarFoto(int id)
        {
            ResultadoWS resultadoWS = new ResultadoWS();

            string root = "http://192.168.0.52/ProyectoXalli_Gentelella";
            //string root = "http://proyectoxally.somee.com";
            string ruta = "/images/Carnet" + @"\";

             var imagen = db.Imagenes.DefaultIfEmpty(null).FirstOrDefault(o => o.Ruta == ruta+id+".png");

            if (imagen != null)
            { 
                    resultadoWS.Mensaje = root + imagen.Ruta;
                    resultadoWS.Resultado = true;
            }

            return Json(resultadoWS, JsonRequestBehavior.AllowGet);
        }

    }
}