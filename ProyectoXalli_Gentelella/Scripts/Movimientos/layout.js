//ESTABLECE LA HORA EN LA VISTA INDEX ORDENES
function startTime() {

    clock = new Date();
    hours = ((clock.getHours() + 11) % 12 + 1);

    hour = fixTime(hours);
    minutes = fixTime(clock.getMinutes());
    seconds = fixTime(clock.getSeconds());
    suffix = clock.getHours() >= 12 ? 'PM' : 'AM';

    $("#Hora").val(hour + ":" + minutes + ":" + seconds + " " + suffix);

    var time = setTimeout(function () { startTime(); }, 500);
}

//AGREGA EL 0 DE LA HORA, MINUTO O SEGUNDO SI ES MENOR A 10
function fixTime(time) {
    return time < 10 ? "0" + time : time;
}

//PARA CARGAR EL LOADER EN CADA PETICION AJAX
$(document).ready(function () {
    var screen = $('#Cargando'); //obtiene modal Cargando
    configureLoadingScreen(screen); //llamada a metodo usando AJAX
});

//PARA CARGAR EL LOADER EN CADA PETICION AJAX
function configureLoadingScreen(screen) {  // metodo para mostrar Loader
    $(document)
        .ajaxStart(function () { //muestra imagen
            screen.modal("show");
            $('#Cargando').modal({ backdrop: 'static', keyboard: true });//EVITAR SALIRSE DEL CARGANDO PRESIONANDO FUERA DE EL
        })
        .ajaxStop(function () { //oculta imagen
            screen.modal("hide");
        });
}

$(function () {
    //BUSCAR EL ROL DEL EMPLEADO LOGGEADO
    var loginId = $("#session").val();
    var EmpleadoRole = "", colabId = 0;

    //BUSCAR EL ROL DEL EMPLEADO E ID DEL TRABAJADOR LOGEADO
    $.ajax({
        type: "GET",
        url: "/Account/ColaboradorRole/",
        data: { empleado: loginId },
        success: function (data) {
            colabId = data.ColaboradorId;
            EmpleadoRole = data.Role;
        }
    });

    //INSTANCIA DEL HUB DE SIGNAL (WEB-SOCKET)
    var hub = $.connection.rowAdd;

    hub.client.nuevaFila = function () {
        //LIMPIANDO LA TABLA
        $("#bOrdenes").empty();
        $("#txt").remove();
        $("#hOrdenes").empty();

        CrearTabla(colabId, EmpleadoRole);
        dashboardSign();

        //    hub.server.getData(colabId, EmpleadoRole).done(function (datos) {
        //        ////CREA EL ENCABEZADO DE LA TABLA DE ORDENES
        //        //var thead = '<tr> <th>No. Orden</th> <th>Hora ordenada</th> <th>Cliente</th>';
        //        //var theadFin = "";

        //        ////CREA EL TBODY DE LA TABLA ORDENES
        //        //var tbodyFin = "";
        //        //var tbody = "";

        //        //tbody = '<tr><th scope="row">' + cargarCodigo(data.CodigoOrden) + '</th><th>' + data.HoraOrden + '</th><td>' + data.Cliente + '</td>';

        //        //$.each(datos, function (index, data) {

        //        //    //DEPENDIENDO SI EL ROL ES DIFERENTE A MESERO
        //        //    if (EmpleadoRole == "Bartender") {
        //        //        //nada
        //        //    } else if (EmpleadoRole == "Cocina") {
        //        //        //nada
        //        //    } else if (EmpleadoRole != "Mesero") {
        //        //        theadFin = '<th>Mesero</th> <th>Acciones</th> </tr>';

        //        //        tbodyFin = '<td>' + data.Mesero + '</td>' +
        //        //            '<td>' +
        //        //            '<div class="btn-group">' +
        //        //            '<button data-toggle="dropdown" class="btn btn-primary dropdown-toggle btn-sm" type="button" aria-expanded="true">' +
        //        //            'Acción   <span class="caret"></span>' +
        //        //            '</button>' +
        //        //            '<ul role="menu" class="dropdown-menu">' +
        //        //            '<li>' +
        //        //            '<a id="buttonOrder" onclick="RedirectToEdit(' + data.OrdenId + ')">Ver orden</a>' +
        //        //            '</li>' +
        //        //            '<li>' +
        //        //            '<a onclick="RedirectToComanda(' + data.OrdenId + ')">Mostrar comanda</a>' +
        //        //            '</li>' +
        //        //            '</ul>' +
        //        //            '</div>' +
        //        //            '</td>' +
        //        //            '</tr>';
        //        //    }
        //        //    else {
        //        //        theadFin = '<th>Acciones</th> </tr>';
        //        //        tbodyFin = '<td>' +
        //        //            '<div class="btn-group">' +
        //        //            '<button data-toggle="dropdown" class="btn btn-primary dropdown-toggle btn-sm" type="button" aria-expanded="true">' +
        //        //            'Acción   <span class="caret"></span>' +
        //        //            '</button>' +
        //        //            '<ul role="menu" class="dropdown-menu">' +
        //        //            '<li>' +
        //        //            '<a id="buttonOrder" onclick="RedirectToEdit(' + data.OrdenId + ')">Ver orden</a>' +
        //        //            '</li>' +
        //        //            '<li>' +
        //        //            '<a onclick="RedirectToComanda(' + data.OrdenId + ')">Mostrar comanda</a>' +
        //        //            '</li>' +
        //        //            '</ul>' +
        //        //            '</div>' +
        //        //            '</td>' +
        //        //            '</tr>';
        //        //    }
        //        //})

        //        ////AGREGA EL ENCABEZADO A LA TABLA
        //        //$("#hOrdenes").html(thead + theadFin);
        //        ////AGREGA EL CUERPO DE LA TABLA
        //        //$("#bOrdenes").html(tbody + tbodyFin);


        //        //alert(datos);


        //    })


    }

    $.connection.hub.start().done();
});