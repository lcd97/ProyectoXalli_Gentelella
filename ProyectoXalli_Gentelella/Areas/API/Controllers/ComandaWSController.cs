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
    [BasicAuthentication]
    public class ComandaWSController : Controller
    {
        //conexion con la db
        private DBControl db = new DBControl();

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
                        Imagen img = new Imagen();
                        string imgexte = ("/images/Comanda" + @"\" + photo.FileName);

                        //buscar la orden para modificarla
                        Orden orden = await db.Ordenes.DefaultIfEmpty(null).FirstOrDefaultAsync(o => o.Id == idorden);

                        //buscar en la db de imagenes para ver si existe
                        if (orden != null)
                        {
                           img = await db.Imagenes.DefaultIfEmpty(null).FirstOrDefaultAsync(i => i.Id == orden.ImagenId && i.Ruta.Trim() == imgexte.Trim());
                        }

                        if (img == null && orden != null)
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
                                        resultadoWS.Mensaje = "Imagen de la comanda almecenada con exito";
                                        resultadoWS.Resultado = true;
                                        transact.Commit();
                                    }
                                    else
                                    {
                                        resultadoWS.Mensaje = "Error al guardar la imagen de la orden";
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
                                resultadoWS.Mensaje = "Error al guardar la ruta de la imagen de la comanda";
                                resultadoWS.Resultado = false;
                                throw new Exception();
                            }
                        }
                        else if (img != null && orden != null)
                        {
                            //modificar
                            path = Path.Combine(Server.MapPath("~/images/Comanda"), photo.FileName);
                            photo.SaveAs(path);

                            resultadoWS.Mensaje = "Imagen de la comanda modificada con exito";
                            resultadoWS.Resultado = true;
                        }
                        else {

                            resultadoWS.Mensaje = "No existe la orden para agregar la imagen";
                            resultadoWS.Resultado = false;
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

            //string root = "http://192.168.0.52/ProyectoXalli_Gentelella";
            //string root = "http://proyectoxally.somee.com";
            //string root = "http://192.168.137.213/ProyectoXalli_Gentelella";
            Orden orden = db.Ordenes.DefaultIfEmpty(null).FirstOrDefault(o => o.Id == id);

            if (orden != null)
            {
                Imagen imagen = db.Imagenes.DefaultIfEmpty(null).FirstOrDefault(i => i.Id == orden.ImagenId);

                if (imagen.Ruta != "N/A")
                {
                    resultadoWS.Mensaje = imagen.Ruta;
                    resultadoWS.Resultado = true;
                }
            }

            return Json(resultadoWS,JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void rotarimg(HttpPostedFileBase photo)
        {
            byte[] imageData = new byte[photo.ContentLength];
            photo.InputStream.Read(imageData, 0, photo.ContentLength);

            MemoryStream ms = new MemoryStream(imageData);
            Image originalImage = Image.FromStream(ms);

            if (originalImage.PropertyIdList.Contains(0x0112))
            {
                int rotationValue = originalImage.GetPropertyItem(0x0112).Value[0];
                switch (rotationValue)
                {
                    case 1: // landscape, do nothing
                        break;

                    case 8: // rotated 90 right
                            // de-rotate:
                        originalImage.RotateFlip(rotateFlipType: RotateFlipType.Rotate270FlipNone);
                        break;

                    case 3: // bottoms up
                        originalImage.RotateFlip(rotateFlipType: RotateFlipType.Rotate180FlipNone);
                        break;

                    case 6: // rotated 90 left
                        originalImage.RotateFlip(rotateFlipType: RotateFlipType.Rotate90FlipNone);
                        break;
                }   
            }
        }

     
    }
}