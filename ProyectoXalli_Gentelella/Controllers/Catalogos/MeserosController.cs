using Microsoft.AspNet.Identity;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Catalogos {
    [Authorize]
    public class MeserosController : Controller {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        //CONEXION A LA BASE DE DATOS SEGURIDAD
        private ApplicationDbContext context = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        [Authorize(Roles = "Admin")]
        // GET: Meseros
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// METODO PARA OBTENER LOS DATOS DE LA VISTA INDEX
        /// </summary>
        /// <returns></returns>
        public JsonResult GetData() {
            var meseros = (from obj in db.Meseros
                           join d in db.Datos on obj.DatoId equals d.Id
                           where obj.EstadoMesero == true
                           select new {
                               Id = obj.Id,
                               Documento = d.Cedula,
                               NombreMesero = d.PNombre + " " + d.PApellido,
                               Horario = obj.HoraEntrada + " - " + obj.HoraSalida
                           }).ToList();

            return Json(new { data = meseros }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        /// <summary>
        /// RETORNA LA VISTA CREATE
        /// </summary>
        /// <returns></returns>
        public ActionResult Create() {
            return View();
        }

        /// <summary>
        /// METODO POST DE CREATE
        /// </summary>
        /// <param name="Nombres"></param>
        /// <param name="Apellido"></param>
        /// <param name="Cedula"></param>
        /// <param name="RUC"></param>
        /// <param name="estado"></param>
        /// <param name="HoraEntrada"></param>
        /// <param name="HoraSalida"></param>
        /// <param name="InicioTurno"></param>
        /// <param name="FinTurno"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(string Nombres, string Apellido, string Cedula, string INSS, string RUC, string HoraEntrada, string HoraSalida) {

            var meseroId = -1;

            if (INSS.Length != 9) {
                mensaje = "El número INSS debe ser de 8 dígitos";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            if (RUC != "") {
                if (RUC.Length != 14) {
                    mensaje = "El número RUC debe ser de 14 dígitos";
                    return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                }
            }

            if (Cedula.Length != 16) {
                mensaje = "El número de cédula debe ser de 14 dígitos";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);

            }

            //BUSCAR QUE EL NUMERO RUC NO SE REPITA
            var buscarRUC = RUC.Trim() != "" ? db.Datos.DefaultIfEmpty(null).FirstOrDefault(r => r.RUC == RUC && r.RUC != null) : null;

            //SI EXISTE UN REGISTRO CON EL NUMERO RUC
            if (buscarRUC != null) {
                mensaje = "El número RUC ya se encuentra registrado";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            //BUSCAR SI LA PERSONA EXISTE
            var persona = db.Datos.DefaultIfEmpty(null).FirstOrDefault(p => p.Cedula.Trim() == Cedula.Trim());

            //SI SE ENCUENTRA REGISTRADO
            if (persona != null) {
                //SE BUSCA QUE EL COLABORADOR CON EL DATO ID
                var colaborador = db.Meseros.DefaultIfEmpty(null).FirstOrDefault(c => c.DatoId == persona.Id);

                //SI YA EXISTE EL COLABORADOR MANDAR ERROR
                if (colaborador != null) {
                    mensaje = "El colaborador ya se encuentra registrado";
                    return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                }

                //TRANSACT SQL
                using (var transact = db.Database.BeginTransaction()) {
                    try {
                        //SI LLEGA UN REGISTRO DE RUC VALIDO
                        if (RUC.Trim() != "") {
                            persona.RUC = RUC.Trim();
                        }

                        //SI NO EXISTE REGISTRADO, AGREGARLO
                        Mesero mesero = new Mesero();

                        mesero.INSS = INSS;
                        mesero.HoraEntrada = HoraEntrada;
                        mesero.HoraSalida = HoraSalida;
                        mesero.DatoId = persona.Id;
                        mesero.EstadoMesero = true;

                        db.Meseros.Add(mesero);

                        completado = db.SaveChanges() > 0 ? true : false;
                        mensaje = completado ? "Almacenado Correctamente" : "Error al almacenar";

                        //Si todo se hizo correctamente se guardan los datos definitivamente
                        transact.Commit();


                        meseroId = mesero.Id;
                    } catch (Exception) {
                        //Si hubo algun error en el almacenamiento de los datos
                        //deshacemos todos lo que habiamos guardado
                        transact.Rollback();
                        mensaje = "Error al almacenar";
                    }//FIN TRY-CATCH
                }//FIN USING
            } else {

                //TRANSACT SQL
                using (var transact = db.Database.BeginTransaction()) {
                    try {
                        //SI NO SE ENCUENTRA REGISTRADO
                        Dato dato = new Dato();

                        //LLENAMOS LA TABLA DE DATOS
                        dato.Cedula = Cedula.Trim();
                        dato.PNombre = Nombres.Trim();
                        dato.PApellido = Apellido.Trim();
                        dato.RUC = RUC.Trim() != "" ? RUC.Trim() : null;

                        db.Datos.Add(dato);

                        //SI SE ALMACENO CORRECTAMENTE
                        if (db.SaveChanges() > 0) {
                            Mesero waiter = new Mesero();

                            waiter.INSS = INSS;
                            waiter.HoraEntrada = HoraEntrada;
                            waiter.HoraSalida = HoraSalida;
                            waiter.DatoId = dato.Id;
                            waiter.EstadoMesero = true;

                            db.Meseros.Add(waiter);

                            completado = db.SaveChanges() > 0 ? true : false;
                            mensaje = completado ? "Almacenado Correctamente" : "Error al almacenar";

                            //Si todo se hizo correctamente se guardan los datos definitivamente
                            transact.Commit();

                            meseroId = waiter.Id;
                        }//FIN SAVECHANGES DATO               
                    } catch (Exception) {

                        mensaje = "Error al almacenar";

                        //Si hubo algun error en el almacenamiento de los datos
                        //deshacemos todos lo que habiamos guardado
                        transact.Rollback();
                    }//FIN TRY-CATCH
                }
            }//FIN ELSE

            return Json(new { success = completado, message = mensaje, meseroId }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        /// <summary>
        /// OBTIENE LA VISTA EDIT
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mesero Mesero = db.Meseros.Find(id);
            if (Mesero == null) {
                return HttpNotFound();
            }

            return View(Mesero);
        }

        /// <summary>
        /// OBTIENE LOS DATOS DEL MESERO
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult getMeseros(int id) {
            var mesero = (from obj in db.Meseros
                          join a in db.Datos on obj.DatoId equals a.Id
                          where obj.Id == id
                          select new {
                              Nombre = a.PNombre,
                              Apellido = a.PApellido,
                              Cedula = a.Cedula,
                              Inss = obj.INSS,
                              RUC = a.RUC,
                              EntradaH = obj.HoraEntrada,
                              SalidaH = obj.HoraSalida,
                              Estado = obj.EstadoMesero
                          }).FirstOrDefault();

            var rol = (from tb1 in context.Users
                       from tb2 in tb1.Roles
                       join tb3 in context.Roles on tb2.RoleId equals tb3.Id
                       where tb1.PeopleId == id
                       orderby tb1.UserName, tb3.Name
                       select tb3.Name).FirstOrDefault();

            return Json(new { mesero, rol }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// ACTUALIZA EL OBJETO MESERO
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="lastName"></param>
        /// <param name="Cedula"></param>
        /// <param name="RUC"></param>
        /// <param name="HoraEntrada"></param>
        /// <param name="HoraSalida"></param>
        /// <param name="InicioTurno"></param>
        /// <param name="FinTurno"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(string Nombres, string Apellido, string Cedula, string RUC, string HoraEntrada, string HoraSalida, bool Estado) {

            if (RUC != "") {
                if (RUC.Length != 14) {
                    mensaje = "El número RUC debe ser de 14 dígitos";
                    return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                }
            }

            if (Cedula.Length != 16) {
                mensaje = "El número de cédula debe ser de 14 dígitos";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);

            }

            //BUSCAR QUE EL NUMERO RUC NO SE REPITA
            var buscarRUC = RUC.Trim() != "" ? db.Datos.DefaultIfEmpty(null).FirstOrDefault(r => r.RUC == RUC && r.RUC != null && r.Cedula.Trim() != Cedula.Trim()) : null;

            //SI EXISTE UN REGISTRO CON EL NUMERO RUC
            if (buscarRUC != null) {
                mensaje = "El número RUC ya se encuentra registrado";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //BUSCAR AL OBJETO A EDITAR
                    var data = db.Datos.DefaultIfEmpty(null).FirstOrDefault(d => d.Cedula.Trim() == Cedula.Trim());
                    var waiter = db.Meseros.DefaultIfEmpty(null).FirstOrDefault(w => w.DatoId == data.Id);

                    //ACTUALIZAR EL REGISTRO DEL MESERO
                    if (waiter != null) {
                        //EDITAR DATOS PRINCIPALES
                        data.PNombre = Nombres.Trim();
                        data.PApellido = Apellido.Trim();
                        data.RUC = RUC != "" ? RUC : null;

                        db.Entry(data).State = EntityState.Modified;

                        //GUARDAMOS LOS CAMBIOS
                        if (db.SaveChanges() > 0) {
                            //ACTUALIZAR DATOS DE MESERO
                            waiter.HoraEntrada = HoraEntrada;
                            waiter.HoraSalida = HoraSalida;
                            waiter.EstadoMesero = Estado;

                            db.Entry(waiter).State = EntityState.Modified;

                            if (db.SaveChanges() > 0) {
                                //BUSCAR EN LA BD DE SEGURIDAD
                                var credenciales = context.Users.Where(w => w.PeopleId == waiter.Id).SingleOrDefault();

                                //SI EL COLABORADOR ESTA INACTIVO. BLOQUEAR CREDENCIALES DE ACCESO
                                if (Estado) {
                                    //HABILITAR
                                    credenciales.LockoutEnabled = true;
                                } else {
                                    //DESHABILITAR
                                    credenciales.LockoutEnabled = false;
                                }

                                completado = context.SaveChanges() > 0;
                                mensaje = completado ? "Modificado correctamente" : "Error al modificar";
                            }
                        }
                    } else {//NO SE ENCONTRO EL REGISTRO COMO MESERO
                        mensaje = "La persona a modificar no se encuentra registrado";
                    }//Si todo se hizo correctamente se guardan los datos definitivamente
                    transact.Commit();

                } catch (Exception) {
                    //Si hubo algun error en el almacenamiento de los datos
                    //deshacemos todos lo que habiamos guardado
                    transact.Rollback();
                    mensaje = "Error al modificar";
                }//FIN TRY-CATCH
            }//FIN USING

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        // GET: Meseros/Details/5
        public async Task<ActionResult> Details(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mesero mesero = await db.Meseros.FindAsync(id);
            if (mesero == null) {
                return HttpNotFound();
            }
            return View(mesero);
        }

        // POST: Meseros/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id) {

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    var mesero = db.Meseros.Find(id);
                    var userRole = context.Users.FirstOrDefault(u => u.PeopleId == mesero.Id);

                    bool existe = false;

                    if (userRole != null) {
                        existe = true;
                    }


                    //BUSCANDO QUE Categoria NO TENGA SALIDAS NI ENTRADAS REGISTRADAS CON SU ID
                    Orden orden = db.Ordenes.DefaultIfEmpty(null).FirstOrDefault(p => p.MeseroId == mesero.Id);

                    if (orden == null && existe == false) {
                        db.Meseros.Remove(mesero);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Eliminado correctamente" : "Error al eliminar";
                    } else {
                        mensaje = "Se encontraron ordenes realizadas con este mesero";
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al eliminar";

                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING
            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> BuscarColaborador(string Identificacion) {

            var colaborador = await (from obj in db.Datos
                                     join d in db.Meseros on obj.Id equals d.DatoId
                                     where obj.Cedula.Trim().ToUpper() == Identificacion.Trim().ToUpper()
                                     select new {
                                         DatoId = obj.Id,
                                         MeseroId = d.Id,
                                         Nombres = obj.PNombre,
                                         Apellidos = obj.PApellido,
                                         Cedula = obj.Cedula,
                                         INSS = d.INSS,
                                         RUC = obj.RUC,
                                         EntradaH = d.HoraEntrada,
                                         SalidaH = d.HoraSalida,
                                     }).FirstOrDefaultAsync();

            return Json(colaborador, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}