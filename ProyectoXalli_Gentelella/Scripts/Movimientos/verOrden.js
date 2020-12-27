$(document).ready(function () {
    var EmpleadoId = 0;
    var loginId = $("#session").val();

    //if ($("#rol").attr("val") == "true") {
    //    //PONER POR DEFECTO EL EMPLEADO LOGEADO
    //    $.ajax({
    //        type: "GET",
    //        url: "/Ordenes/LoggedUser/",
    //        data: { LoginId: loginId },
    //        success: function (data) {
    //            cargarTabla(data.MeseroId);
    //        },
    //        error: function () {
    //            Alert("Error", "Datos del Colaborador sin cargar", "error");
    //        }
    //    });
    //} else {
    //    cargarTabla(EmpleadoId);
    //}

    //BUSCAR EL ROL DEL EMPLEADO E ID DEL TRABAJADOR LOGEADO
    $.ajax({
        type: "GET",
        url: "/Account/ColaboradorRole/",
        data: { empleado: loginId },
        success: function (data) {
            var ColabId = data.ColaboradorId;
            var role = data.Role;

            CrearTabla(ColabId, role);
        }
    });

    var mensaje = $("#mensaje").attr("value");

    if (mensaje != "") {
        AlertTimer("Completado", mensaje, "success");
    }

    window.history.pushState('page2', 'Title', '/Ordenes/VerOrdenes');//QUITAMOS CUALQUIER MENSAJE DE LA URL

});//FIN DOCUMENT READY

//CREA EL FORMATO DEL CODIGO
function cargarCodigo(data) {
    var code = "";

    if (data < 10) {
        code = "000" + data;
    } else if (data >= 10 || data < 100) {
        code = "00" + data;
    } else if (data >= 100 || data < 1000) {
        code = "0" + data;
    } else {
        code = data;
    }

    return code;
}

//MUESTRA LA TABLA CON LOS DATOS GENERALES (TODOS LOS PEDIDOS DE TODOS LOS MESEROS)
function CrearTabla(EmpleadoId, EmpleadoRole) {
    //RECUPERAR TODAS LAS ORDENES DEL DIA
    $.ajax({
        type: "GET",
        url: "/Ordenes/Ordenes/",
        data: { empleadoId: EmpleadoId, EmpleadoRol: EmpleadoRole },
        success: function (data) {
            if (data.length == 0) {
                var agregar = '<h2 id="txt" style="text-align:center;">Ordenes vacías</h2>';//AGREGA LETRERO
                $("#x_content").append(agregar);
            } else {
                //CREA EL ENCABEZADO DE LA TABLA DE ORDENES
                var thead = '<tr> <th>No. Orden</th> <th>No. Mesa</th> <th>Hora ordenada</th> <th>Cliente</th>';
                var theadFin = "";
                var huesped = "";

                //CREA EL TBODY DE LA TABLA ORDENES
                var tbodyFin = "";
                var tbody = "";

                for (var i = 0; i < Object.keys(data).length; i++) {
                    huesped = data[i].Cliente != "N/A" ? true : false;

                    tbody = '<tr><td scope="row">' + cargarCodigo(data[i].CodigoOrden) + '</td><td>' + data[i].Mesa + '</td><td>' + data[i].HoraOrden + '</td><td>' + data[i].Cliente + '</td>';

                    if (EmpleadoRole != "Mesero") {
                        theadFin = '<th>Mesero</th> <th>Acciones</th> </tr>';

                        tbodyFin = '<td>' + data[i].Mesero + '</td>' +
                            '<td>' + cargarLinks(EmpleadoRole, data[i].OrdenId) + '</td>' +
                            '</tr>';
                    }
                    else {
                        theadFin = '<th>Acciones</th> </tr>';
                        tbodyFin = '<td>' +
                            '<div class="btn-group">' +
                            '<button data-toggle="dropdown" class="btn btn-primary dropdown-toggle btn-sm" type="button" aria-expanded="true">' +
                            'Acción   <span class="caret"></span>' +
                            '</button>' + buttonComanda(huesped, data[i].OrdenId) +
                            '</div>' +
                            '</td>' +
                            '</tr>';
                    }

                    //AGREGA EL ENCABEZADO A LA TABLA
                    $("#hOrdenes").html(thead + theadFin);
                    //AGREGA EL CUERPO DE LA TABLA
                    $("#bOrdenes").append(tbody + tbodyFin);
                }
            }
        }
    });//FIN AJAX
}//FIN FUNCTION


function buttonComanda(huesped, ordenId) {
    var link = "";

    if (huesped) {
        link = '<ul role="menu" class="dropdown-menu">' +
            '<li>' +
            '<a id="buttonOrder" onclick="RedirectToEdit(' + ordenId + ')">Ver orden</a>' +
            '</li>' +
            '<li>' +
            '<a onclick="RedirectToComanda(' + ordenId + ')">Mostrar comanda</a>' +
            '</li>' +
            '</ul>';
    } else {
        link = '<ul role="menu" class="dropdown-menu">' +
            '<li>' +
            '<a id="buttonOrder" onclick="RedirectToEdit(' + ordenId + ')">Ver orden</a>' +
            '</li>' +
            '</ul>';
    }

    return link;
}

//FUNCION QUE DIRIGE AL FORMULARIO DE ORDEN PARA AGREGAR NUEVOS ITEMS
function RedirectToEdit(OrderId) {

    var url = "/Ordenes/Edit?OrderId=" + OrderId;
    window.location.href = url;
}

//FUNCION QUE DIRIGE AL FORMULARIO DE ORDEN PARA AGREGAR NUEVOS ITEMS
function RedirectToComanda(OrderId) {

    var url = "/Ordenes/Comanda?OrderId=" + OrderId;
    window.location.href = url;
}