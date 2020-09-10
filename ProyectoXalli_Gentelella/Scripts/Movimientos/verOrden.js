$(document).ready(function () {
    var EmpleadoId = 0;
    var loginId = $("#session").val();

    if ($("#rol").attr("val") == "true") {
        //PONER POR DEFECTO EL EMPLEADO LOGEADO
        $.ajax({
            type: "GET",
            url: "/Ordenes/LoggedUser/",
            data: { LoginId: loginId },
            success: function (data) {
                cargarTabla(data.MeseroId);
            },
            error: function () {
                Alert("Error", "Datos del Colaborador sin cargar", "error");
            }
        });
    } else {
        cargarTabla(EmpleadoId);
    }
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

//CARGA LA TABLA DE LAS ORDENES SEGUN EL ROL DEL EMPLEADO
function cargarTabla(EmpleadoId) {
    //RECUPERAR TODAS LAS ORDENES DEL DIA
    $.ajax({
        type: "GET",
        url: "/Ordenes/Ordenes/",
        data: { EmpleadoId },
        success: function (data) {

            //CREA EL ENCABEZADO DE LA TABLA DE ORDENES
            var thead = '<tr> <th>No. Orden</th> <th>Hora ordenada</th> <th>Cliente</th>';
            var theadFin = "";

            //CREA EL TBODY DE LA TABLA ORDENES
            var tbodyFin = "";
            var tbody = "";

            for (var i = 0; i < Object.keys(data).length; i++) {

                tbody = '<tr><th scope="row">' + cargarCodigo(data[i].CodigoOrden) + '</th><th>' + data[i].HoraOrden + '</th><td>' + data[i].Cliente + '</td>';

                //DEPENDIENDO SI EL ROL ES DIFERENTE A MESERO
                if ($("#rol").attr("val") == "false") {
                    theadFin = '<th>Mesero</th> <th>Acciones</th> </tr>';

                    tbodyFin = '<td>' + data[i].Mesero + '</td>' +
                        '<td>' +
                        '<div class="btn-group">' +
                        '<button data-toggle="dropdown" class="btn btn-primary dropdown-toggle btn-sm" type="button" aria-expanded="true">' +
                        'Acción   <span class="caret"></span>' +
                        '</button>' +
                        '<ul role="menu" class="dropdown-menu">' +
                        '<li>' +
                        '<a id="buttonOrder" onclick="RedirectToEdit(' + data[i].OrdenId + ')">Ver orden</a>' +
                        '</li>' +
                        '<li>' +
                        '<a onclick="RedirectToComanda(' + data[i].OrdenId + ')">Mostrar comanda</a>' +
                        '</li>' +
                        '</ul>' +
                        '</div>' +
                        '</td>' +
                        '</tr>';
                }
                else {
                    theadFin = '<th>Acciones</th> </tr>';
                    tbodyFin = '<td>' +
                        '<div class="btn-group">' +
                        '<button data-toggle="dropdown" class="btn btn-primary dropdown-toggle btn-sm" type="button" aria-expanded="true">' +
                        'Acción   <span class="caret"></span>' +
                        '</button>' +
                        '<ul role="menu" class="dropdown-menu">' +
                        '<li>' +
                        '<a id="buttonOrder" onclick="RedirectToEdit(' + data[i].OrdenId + ')">Ver orden</a>' +
                        '</li>' +
                        '<li>' +
                        '<a onclick="RedirectToComanda(' + data[i].OrdenId + ')">Mostrar comanda</a>' +
                        '</li>' +
                        '</ul>' +
                        '</div>' +
                        '</td>' +
                        '</tr>';
                }

                //AGREGA EL ENCABEZADO A LA TABLA
                $("thead").html(thead + theadFin);
                //AGREGA EL CUERPO DE LA TABLA
                $("tbody").append(tbody + tbodyFin);
            }
        }
    });//FIN AJAX
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