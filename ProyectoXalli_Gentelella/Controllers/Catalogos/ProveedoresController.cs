using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Catalogos
{
    [Authorize]
    public class ProveedoresController : Controller
    {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        [Authorize(Roles = "Admin")]
        // GET: Proveedor
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// RECUPERA DATOS PARA LLENAR LA TABLA PROVEEDORES A TRAVES DE JSON
        /// </summary>
        /// <returns></returns>
        public JsonResult GetData() {
            var proveedores = (from obj in db.Proveedores
                               join u in db.Datos on obj.DatoId equals u.Id
                               where obj.EstadoProveedor == true
                               select new {
                                   Id = obj.Id,
                                   //CONDICION PARA ASIGNAR A UN CAMPO UN VALOR ALTERNATIVO EN CASO DE SER NULO (CASE-WHEN)
                                   NombreComercial = obj.NombreComercial != null ? obj.NombreComercial : u.PNombre + " " + u.PApellido,
                                   Telefono = obj.Telefono,
                                   RUC = u.RUC != null ? u.RUC : "N/A",
                                   Local = obj.Local
                               }).ToList();

            return Json(new { data = proveedores }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        // GET: Productos/Create
        public ActionResult Create() {
            return View();
        }

        // POST: Productos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(string NombreComercial, string Telefono, string RUC, bool Local, bool RetenedorIR, string NombreProveedor, string ApellidoProveedor, string CedulaProveedor) {
            if (Telefono.Length != 9) {
                mensaje = "El número telefónico debe ser de 8 dígitos";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            if (RUC != "") {
                if (RUC.Length != 14) {
                    mensaje = "El número RUC debe ser de 14 dígitos";
                    return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                }
            }

            if (CedulaProveedor != "") {
                if (CedulaProveedor.Length != 16) {
                    mensaje = "El número de cédula debe ser de 14 dígitos";
                    return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                }
            }

            //SE CREAR UNA INSTANCIA PARA ALMACENAR AL PROVEEDOR
            Proveedor proveedor = new Proveedor();
            //INICIO CAMPOS A USAR EN VISTA ENTRADA
            int proveedorId = 0;
            string providerName = "";
            //FIN CAMPOS A USAR EN VISTA ENTRADA

            //BUSCAR QUE EL NUMERO RUC NO SE REPITA
            var buscarRUC = db.Datos.DefaultIfEmpty(null).FirstOrDefault(r => r.RUC == RUC && r.RUC != null);

            //SI EXISTE UN REGISTRO CON EL NUMERO RUC
            if (buscarRUC != null) {
                mensaje = "El número RUC ya se encuentra registrado";
                return Json(new { success = completado, message = mensaje, Id = proveedorId, Proveedor = providerName }, JsonRequestBehavior.AllowGet);
            }


            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //DEPENDE DEL TIPO DE PROVEEDOR SE ALMACENA LOS DATOS
                    if (Local) {
                        //INSTANCIA DE LA TABLA DATO PARA GUARDAR DATOS DEL PROVEEDOR LOCAL
                        Dato dato = new Dato();
                        Proveedor validando = new Proveedor();

                        //VALIDACION DEL CAMPO CEDULA PROVEEDOR EN LA TABLA DATOS
                        Dato Validacion = db.Datos.DefaultIfEmpty(null).FirstOrDefault(d => d.Cedula.Trim() == CedulaProveedor.Trim());
                        //SI EXISTE UN REGISTRO DE DATOS, BUSCAR PROVEEDOR
                        if (Validacion != null) {
                            //VALIDANDO QUE EL PROVEEDOR NO ESTE REGISTRADO EN LA TABLA Y QUE SEA DIFERENTE DEL ID 2 QUE ES MI PLANTILLA
                            validando = db.Proveedores.DefaultIfEmpty(null).FirstOrDefault(d => d.DatoId == Validacion.Id);
                        }

                        //SI NO EXISTE EL OBJETO DATO
                        if (Validacion == null) {
                            //SE GUARDAN DATOS DEL PROVEEDOR LOCAL
                            dato.Cedula = CedulaProveedor.ToUpper();
                            dato.PNombre = NombreProveedor;
                            dato.PApellido = ApellidoProveedor;

                            //SI SE INGRESO UN NUMERO RUC-ALMACENAR
                            dato.RUC = RUC != "" ? RUC : null;

                            db.Datos.Add(dato);

                            //SI SE GUARDO SE ALMACENAN LOS OTROS CAMPOS
                            if (db.SaveChanges() > 0) {
                                //GUARDAR A PROVEEDOR
                                proveedor.Telefono = Telefono;
                                proveedor.EstadoProveedor = true;
                                proveedor.Local = Local;
                                proveedor.RetenedorIR = RetenedorIR;
                                proveedor.DatoId = dato.Id;//GUARDAR EL ID DEL CAMPO ALMACENADO

                                //GUARDA CAMBIOS EN LA DB
                                db.Proveedores.Add(proveedor);
                                completado = db.SaveChanges() > 0 ? true : false;
                                mensaje = completado ? "Almacenado correctamente" : "Error al guardar";
                                //AGARRAR LOS DATOS A UTILIZAR PARA EL MAESTRO DETALLE DE ENTRADAS
                                proveedorId = proveedor.Id;
                                providerName = dato.PNombre + " " + dato.PApellido;
                            }//FIN SAVECHANGES
                        }//FIN VALIDACION
                        else//SI EXISTE YA UN REGISTRO CON LA CEDULA O RUC
                        {
                            //SI NO EXISTE EL PROVEEDOR
                            if (validando == null) {
                                //AGREGAR PROVEEDOR
                                proveedor.Telefono = Telefono;
                                proveedor.EstadoProveedor = true;
                                proveedor.Local = Local;
                                proveedor.RetenedorIR = RetenedorIR;
                                proveedor.DatoId = Validacion.Id;//GUARDAR EL ID DEL OBJETO ENCONTRADO

                                //GUARDA CAMBIOS EN LA DB
                                db.Proveedores.Add(proveedor);
                                completado = db.SaveChanges() > 0 ? true : false;
                                mensaje = completado ? "Almacenado correctamente" : "Error al guardar";
                            } else//EXISTE YA EL PROVEEDOR
                              {
                                completado = false;
                                mensaje = "El proveedor local ya se encuentra registrado";
                            }

                            proveedorId = proveedor.Id;

                            //dato = db.Datos.Find(datoId);
                            providerName = dato.PNombre + " " + dato.PApellido;
                        }//FIN ELSE VALIDACION      

                    }//FIN LOCAL
                    else if (!Local) {
                        //VALIDANDO QUE EL PROVEEDOR NO EXISTA
                        Proveedor proValidacion = db.Proveedores.DefaultIfEmpty(null).FirstOrDefault(d => d.NombreComercial.ToUpper().Trim() == NombreComercial.ToUpper().Trim() || d.Dato.RUC == RUC);

                        //SI NO EXISTE UN PROVEEDOR REGISTRADO DE LA BUSQUEDA
                        if (proValidacion == null) {
                            Dato data = new Dato();

                            data.RUC = RUC;//ALMACENAR UNICO CAMPO OBLIGATORIO DE UN PROVEEDOR COMERCIAL

                            db.Datos.Add(data);
                            if (db.SaveChanges() > 0) {
                                //GUARDA EL PROVEEDOR
                                proveedor.Telefono = Telefono;
                                proveedor.EstadoProveedor = true;
                                proveedor.Local = Local;
                                proveedor.RetenedorIR = RetenedorIR;
                                proveedor.NombreComercial = NombreComercial;
                                proveedor.DatoId = data.Id;

                                //GUARDA CAMBIOS EN LA DB
                                db.Proveedores.Add(proveedor);
                                completado = db.SaveChanges() > 0 ? true : false;
                                mensaje = completado ? "Almacenado correctamente" : "Error al guardar";

                                proveedorId = proveedor.Id;
                                providerName = proveedor.NombreComercial;
                            } else
                                mensaje = "Error al guardar";
                        } else {
                            completado = false;
                            mensaje = "El proveedor ingresado ya se encuentra registrado";
                        }
                    }//FIN ELSE NO LOCAL

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al almacenar";
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING
            return Json(new { success = completado, message = mensaje, Id = proveedorId, Proveedor = providerName }, JsonRequestBehavior.AllowGet);
        }//FIN POST CREATE

        [Authorize(Roles = "Admin")]
        // GET: Categorias/Edit/5
        public async Task<ActionResult> Edit(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Proveedor Proveedor = await db.Proveedores.FindAsync(id);
            if (Proveedor == null) {
                return HttpNotFound();
            }

            return View(Proveedor);
        }

        /// <summary>
        /// RECUPERA LOS DATOS DEL PROVEEDOR PARA MOSTRAR
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult getProveedor(int id) {
            var provider = (from obj in db.Proveedores
                            join u in db.Datos on obj.DatoId equals u.Id
                            where obj.Id == id
                            select new {
                                //CAMPOS DEL PROVEEDOR
                                NombreComercial = obj.NombreComercial,
                                Telefono = obj.Telefono,
                                local = obj.Local,
                                RUC = u.RUC,
                                IR = obj.RetenedorIR,
                                Estado = obj.EstadoProveedor,
                                Nombre = u.PNombre,
                                Apellido = u.PApellido,
                                Cedula = u.Cedula
                            }).FirstOrDefault();

            return Json(provider, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult UpdateProveedor(int Id, string NombreComercial, string Telefono, string RUC, bool EstadoProveedor, bool Local, bool RetenedorIR, string NombreProveedor, string ApellidoProveedor, string CedulaProveedor) {
            if (Telefono.Length != 9) {
                mensaje = "El número telefónico debe ser de 8 dígitos";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            if (RUC != "") {
                if (RUC.Length != 14) {
                    mensaje = "El número RUC debe ser de 14 dígitos";
                    return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                }
            }

            if (CedulaProveedor != "") {
                if (CedulaProveedor.Length != 16) {
                    mensaje = "El número de cédula debe ser de 14 dígitos";
                    return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
                }
            }

            //BUSCAR AL PROVEEDOR POR MEDIO DEL ID
            Proveedor proveedor = db.Proveedores.Find(Id);

            //BUSCAR QUE EL NUMERO RUC NO SE REPITA Y QUE NO SEA EL PROVEEDOR A MODIFICAR
            var buscarRUC = db.Datos.DefaultIfEmpty(null).FirstOrDefault(r => r.RUC == RUC && r.Id != proveedor.DatoId && r.RUC != "");


            //SI EXISTE UN REGISTRO CON EL NUMERO RUC
            if (buscarRUC != null) {
                mensaje = "El número RUC ya se encuentra registrado";
                return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
            }

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //DEPENDE DEL TIPO DE PROVEEDOR ALMACENAMOS LOS DATOS
                    if (Local) {
                        //BUSCAR LOS DATOS A MODIFICAR DEL PROVEEDOR LOCAL POR MEDIO DE LA CEDULA
                        Dato dato = db.Datos.FirstOrDefault(d => d.Cedula.Trim() == CedulaProveedor.Trim());

                        //ASIGNAMOS VALORES A DATOS DE PROVEEDOR LOCAL
                        dato.PNombre = NombreProveedor;
                        dato.PApellido = ApellidoProveedor;
                        dato.RUC = RUC != "" ? RUC : null;

                        //GUARDAR CAMBIOS
                        db.Entry(dato).State = EntityState.Modified;
                        //CONFIRMACION DE CAMBIOS GUARDADOS
                        if (db.SaveChanges() > 0) {
                            //ASIGNAMOS VALORES DE PROVEEDOR
                            //proveedor.NombreComercial = NombreComercial;
                            proveedor.Telefono = Telefono;
                            proveedor.RetenedorIR = RetenedorIR;
                            proveedor.EstadoProveedor = EstadoProveedor;
                            //GUARDAR CAMBIOS DEL PROVEEDOR
                            db.Entry(proveedor).State = EntityState.Modified;
                            completado = db.SaveChanges() > 0 ? true : false;
                            mensaje = completado ? "Modificado correctamente" : "Error al modificar";
                        } else {
                            //REVERTIR CAMBIOS EN DATOS
                        }
                    } else {
                        //BUSCAR EL REGISTRO DATO DEL PROVEEDOR ATRAVES DEL RUC
                        //var buscarDato = db.Datos.DefaultIfEmpty(null).FirstOrDefault(r => r.RUC.Trim() == RUC);

                        //MODIFICAR RUC
                        //buscarDato.RUC = RUC;
                        //db.Entry(buscarDato).State = EntityState.Modified;

                        //ASIGNAMOS VALORES DE PROVEEDOR
                        //proveedor.NombreComercial = NombreComercial;NO PIENSO CAMBIAR DE NOMBRE COMERCIAL
                        proveedor.Telefono = Telefono;
                        proveedor.RetenedorIR = RetenedorIR;
                        proveedor.EstadoProveedor = EstadoProveedor;
                        //GUARDAR CAMBIOS DEL PROVEEDOR
                        db.Entry(proveedor).State = EntityState.Modified;
                        completado = db.SaveChanges() > 0 ? true : false;
                        mensaje = completado ? "Modificado correctamente" : "Error al modificar";
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al modificar";
                    transact.Rollback();
                }//FIN TRY-CATCH
            }//FIN USING

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        // GET: CategoriasProducto/Details/5
        public async Task<ActionResult> Details(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Proveedor proveedor = await db.Proveedores.FindAsync(id);
            if (proveedor == null) {
                return HttpNotFound();
            }

            return View(proveedor);
        }

        public ActionResult buscarProv(string proveedor) {
            var dato = (from obj in db.Datos
                        where obj.Cedula == proveedor
                        select new {
                            Nombre = obj.PNombre,
                            Apellido = obj.PApellido,
                            RUC = obj.RUC
                        }).FirstOrDefault();

            return Json(dato, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// METODO RETORNA DETALLE DE PROVEEDOR
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult getDetails(int id) {
            var proveedor = (from obj in db.Proveedores
                             join d in db.Datos on obj.DatoId equals d.Id
                             where obj.Id == id
                             select new {
                                 //CONDICION PARA ASIGNAR A UN CAMPO UN VALOR ALTERNATIVO EN CASO DE SER NULO (CASE-WHEN)
                                 //NombreComercial = obj.NombreComercial,
                                 //NombreProveedor = d.PNombre + " " + d.PApellido,
                                 Nombre = obj.NombreComercial != null ? obj.NombreComercial : d.PNombre + " " + d.PApellido,
                                 Telefono = obj.Telefono,
                                 RUC = d.RUC,
                                 Cedula = d.Cedula,
                                 Local = obj.Local,
                                 RetieneIR = obj.RetenedorIR,
                                 Estado = obj.EstadoProveedor
                             }).FirstOrDefault();

            return Json(proveedor, JsonRequestBehavior.AllowGet);
        }

        // POST: Proveedor/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id) {
            var Proveedor = db.Proveedores.Find(id);
            //BUSCANDO QUE PRODUCTO NO TENGA SALIDAS NI ENTRADAS REGISTRADAS CON SU ID
            Entrada oEnt = db.Entradas.DefaultIfEmpty(null).FirstOrDefault(e => e.ProveedorId == Proveedor.Id);

            using (var transact = db.Database.BeginTransaction()) {
                try {
                    //SI NO EXISTEN ENTRADAS CON EL ID ASOCIADO
                    if (oEnt == null) {
                        db.Proveedores.Remove(Proveedor);
                        completado = await db.SaveChangesAsync() > 0 ? true : false;
                        mensaje = completado ? "Eliminado correctamente" : "Error al eliminar";
                    } else {
                        mensaje = "Se encontraron movimientos asociados a este proveedor";
                    }

                    transact.Commit();
                } catch (Exception) {
                    mensaje = "Error al eliminar";
                    transact.Rollback();
                }
            }

            return Json(new { success = completado, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}