using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Busquedas {
    public class ComandasController : Controller {
        private string mensaje = "";
        private bool completado = false;
        private DBControl db = new DBControl();

        // GE T: Comandas
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// ENVIA UN MENSJAE CON LA COMANDA AL CORREO ELECTRONICO
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Cliente"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendEmail(string Email, int codeOrder) {
            if (Email == "") {
                mensaje = "Ingrese el correo del cliente";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            string emailHotel = "proyectoshotel2020@gmail.com";
            string passwordHotel = "bxqalmibjgxzjqux";
            //string passwordHotel = "Calabazas#sin.Nombre2020";

            Orden orden = db.Ordenes.Where(w => w.CodigoOrden == codeOrder).FirstOrDefault();
            var cliente = db.Clientes.FirstOrDefault(w => w.Id == orden.ClienteId && w.EmailCliente != "defaultuser@xalli.com");
            var dato = cliente != null ? db.Datos.DefaultIfEmpty(null).FirstOrDefault(w => w.Id == cliente.DatoId) : null;
            var nombreCliente = dato != null ? dato.PNombre + " " + dato.PApellido : "";

            var filename = Path.Combine(Server.MapPath("~/images/Comanda"), nombreCliente.Replace(" ", "") + orden.Id + ".png");

            var code = (dynamic)null;

            if (codeOrder < 10) {
                code = "000" + codeOrder;
            } else if (codeOrder >= 10 || codeOrder < 100) {
                code = "00" + orden.CodigoOrden;
            } else if (codeOrder >= 100 || codeOrder < 1000) {
                code = "0" + codeOrder;
            } else {
                code = (codeOrder).ToString();
            }

            var encabezado = "Estimado cliente: " + nombreCliente + "<br/> " +
               "Le enviamos evidencias de la comanda de su orden " + code + ":<br/><br/>";

            var footer = "<table style='font-family:Roboto-Regular,Helvetica,Arial,sans-serif;font-size:10px;color:#666666;line-height:18px;padding-bottom:10px'>" +
                         "<tbody>" +
                         "<tr>" +
                         "<td>Este correo electrónico no puede recibir respuestas. Para obtener más información, accede sitio " +
                         "<a href='https://www.xallihotel.com/' style='text-decoration:none;color:#4d90fe' target='_blank'data-saferedirectreason='2' data-saferedirecturl='https://www.xallihotel.com/'>" +
                         "XALLI, OMETEPE BEACH HOTEL" +
                         "</a>." +
                         "<br> Copyright - " + DateTime.Now.Year + ". All Rights Reserved." +
                         "</td>" +
                         "</tr>" +
                         "</tbody>" +
                         "</table>";

            try {
                MailMessage correo = new MailMessage();
                correo.From = new MailAddress(emailHotel);
                correo.To.Add(Email);
                correo.Subject = "Comanda de la orden: " + code;
                correo.Body = encabezado + footer;
                correo.IsBodyHtml = true;
                correo.Priority = MailPriority.Normal;
                correo.Attachments.Add(new Attachment(filename));

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(emailHotel, passwordHotel);

                smtp.Send(correo);

                completado = true;
                mensaje = "Correo enviado correctamente";

            } catch (Exception ex) {
                mensaje = ex.Message;
            }

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        public void encabezadoEmail(ref string code, ref string bodyMessage, int orderId) {
            var orden = db.Ordenes.Join(db.Meseros, o => o.MeseroId, m => m.Id, (o, m) => new { o, m })
                                .Join(db.Datos, mes => mes.m.DatoId, d => d.Id, (mes, d) => new { mes, d })
                                .Where(w => w.mes.o.Id == orderId).FirstOrDefault();

            if (orden.mes.o.CodigoOrden < 10) {
                code = "000" + orden.mes.o.CodigoOrden;
            } else if (orden.mes.o.CodigoOrden >= 10 || orden.mes.o.CodigoOrden < 100) {
                code = "00" + orden.mes.o.CodigoOrden;
            } else if (orden.mes.o.CodigoOrden >= 100 || orden.mes.o.CodigoOrden < 1000) {
                code = "0" + orden.mes.o.CodigoOrden;
            } else {
                code = (orden.mes.o.CodigoOrden).ToString();
            }

            var fechaOrden = orden.mes.o.FechaOrden.ToShortDateString();
            var meseroName = orden.d.PNombre + " " + orden.d.PApellido;

            var detalleOrden = db.DetallesDeOrden.Join(db.Menus, d => d.MenuId, m => m.Id, (d, m) => new { d, m })
                .Where(w => w.d.OrdenId == orderId).ToList();

            bodyMessage += "<table style='color: #73879C;border: 1px solid #ddd; padding: 8px 8px 8px 8px;'>" +
                                "<thead>" +
                                "<th style='color:#73879C; padding: 8px 8px 8px 8px;'>No. orden " + code + " </th>" +
                                "<th style='color:#73879C padding: 8px 8px 8px 8px;'>Fecha: " + fechaOrden + "</th>" +
                                "<th style='color:#73879C padding: 8px 8px 8px 8px;'>Mesero: " + meseroName + "</th>" +
                                "</tr>" +
                                "<tr>" +
                                "<th style='color: #73879C;border: 1px solid #ddd; padding: 8px 8px 8px 8px;'>Cantidad</th>" +
                                "<th style='color: #73879C;border: 1px solid #ddd; padding: 8px 8px 8px 8px;'>Precio Unitario</th>" +
                                "<th style='width: 65%; text-align: center;color: #73879C;border: 1px solid #ddd; padding: 8px 8px 8px 8px;'>Platillo/Bebida</th>" +
                                "<th style='color: #73879C;border: 1px solid #ddd; padding: 8px 8px 8px 8px;'>Subtotal</th>" +
                                "</tr>" +
                                "</thead>" +
                                "<tbody>";

            bodyMessage += "</tbody></table>";
        }

        public ActionResult getComanda(int codeOrder) {
            Orden orden = db.Ordenes.Where(w => w.CodigoOrden == codeOrder).FirstOrDefault();
            string ruta = "";

            if (orden != null) {
                var cliente = db.Clientes.FirstOrDefault(w => w.Id == orden.ClienteId && w.EmailCliente != "defaultuser@xalli.com");
                var dato = cliente != null ? db.Datos.DefaultIfEmpty(null).FirstOrDefault(w => w.Id == cliente.DatoId) : null;
                var nombreCliente = dato != null ? dato.PNombre + " " + dato.PApellido : "";

                ruta = "";

                if (System.IO.File.Exists(Server.MapPath("~/images/Comanda/") + nombreCliente.Replace(" ", "") + orden.Id + ".png")) {
                    completado = true;
                    ruta = @"\images\Comanda\" + nombreCliente.Replace(" ", "") + orden.Id + ".png";
                } else {
                    ruta = "";
                }
            }

            return Json(new { existe = completado, ruta }, JsonRequestBehavior.AllowGet);
        }
    }
}