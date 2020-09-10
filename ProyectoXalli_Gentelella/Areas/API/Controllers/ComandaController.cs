using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MenuAPI.Areas.API.Models;
using System.Data.Entity;

namespace ProyectoXalli_Gentelella.Areas.API.Controllers
{
    public class ComandaController : Controller
    {
        //conexion con la db
        private DBControl db = new DBControl();

        [HttpPost]
        public JsonResult addPhoto()
        {
            ResultadoWS resultadoWS = new ResultadoWS();

            try
            {
                var photo = Request.Files["photo"];

                if (photo.ContentLength > 0 || photo != null)
                {
                    string path = Path.Combine(Server.MapPath("~/images/Comanda"), photo.FileName);
                    photo.SaveAs(path);

                    resultadoWS.Mensaje = "Almecenado con exito";
                    resultadoWS.Resultado = true;
                }

            }
            catch(Exception ex)
            {
                resultadoWS.Mensaje =ex.Message;
                resultadoWS.Resultado = false;
            }

            return Json(resultadoWS, JsonRequestBehavior.AllowGet);
        }




        [HttpPost]
        public async Task<JsonResult> addPhotoComanda(HttpPostedFileBase photo, int idorden)
        {
            ResultadoWS resultadoWS = new ResultadoWS();

            string path = Server.MapPath("~/images/Comanda");

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
                        string imgexte = ("/images/Comanda" + @"\" + photo.FileName);

                        //buscar en la db de imagenes para ver si existe
                        var img = await db.Imagenes.DefaultIfEmpty(null).FirstOrDefaultAsync(i => i.Ruta.Trim() == imgexte.Trim());
                        //buscar la orden para modificarla
                        Orden orden = await db.Ordenes.DefaultIfEmpty(null).FirstOrDefaultAsync(o => o.Id == idorden);

                        if (img == null)
                        {
                            //Crear la ruta y guardar la imagen
                            path = Path.Combine(Server.MapPath("~/images/Comanda"), photo.FileName);
                            photo.SaveAs(path);

                            Imagen imagen = new Imagen();

                            //obtener la url para la ruta concatenado con la carpeta
                            string url = Path.Combine("/images/Comanda", photo.FileName);

                            //asignar la ruta a la nueva imagen
                            imagen.Ruta = url;

                            //agregamos la nueva imagen a la db
                            db.Imagenes.Add(imagen);

                            if (await db.SaveChangesAsync() > 0)
                            {
                                if (orden != null)
                                {
                                    //modificamos el id y guardamos los nuevos datos
                                    orden.ImagenId = imagen.Id;
                                    db.Entry(orden).State = EntityState.Modified;

                                    if (await db.SaveChangesAsync() > 0)
                                    {
                                        resultadoWS.Mensaje = "Almecenado con exito";
                                        resultadoWS.Resultado = true;
                                        transact.Commit();
                                    }
                                    else
                                    {
                                        resultadoWS.Mensaje = "Error al modificar la orden";
                                        resultadoWS.Resultado = false;
                                        throw new Exception();
                                    }
                                }
                                else
                                {
                                    resultadoWS.Mensaje = "No existe la orden";
                                    resultadoWS.Resultado = false;
                                    throw new Exception();
                                }
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
                            path = Path.Combine(Server.MapPath("~/images/Comanda"), photo.FileName);
                            photo.SaveAs(path);

                            resultadoWS.Mensaje = "Almecenado con exito";
                            resultadoWS.Resultado = true;
                        }
                    }
                    catch (Exception ex)
                    {
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

            string root = "http://192.168.1.52/ProyectoXalli_Gentelella";
            //string root = "http://proyectoxally.somee.com";
            Orden orden = db.Ordenes.DefaultIfEmpty(null).FirstOrDefault(o => o.Id == id);

            if (orden != null)
            {
                Imagen imagen = db.Imagenes.DefaultIfEmpty(null).FirstOrDefault(i => i.Id == orden.ImagenId);

                if (imagen.Ruta != "N/A")
                {
                    resultadoWS.Mensaje = root + imagen.Ruta;
                    resultadoWS.Resultado = true;
                }
            }

            return Json(resultadoWS,JsonRequestBehavior.AllowGet);
        }

     
    }
}