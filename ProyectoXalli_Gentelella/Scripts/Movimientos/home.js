$(document).ready(function () {
    //CARGANDO LOS LETREROS DEL DASHBOARD
    dashboardSign();

    //BUSCAR EL ROL DEL EMPLEADO LOGGEADO
    var loginId = $("#session").val();

    //BUSCAR EL ROL DEL EMPLEADO E ID DEL TRABAJADOR LOGEADO
    $.ajax({
        type: "GET",
        url: "/Account/ColaboradorRole/",
        data: { empleado: loginId },
        success: function (data) {
            CrearTabla(data.ColaboradorId, data.Role);
        }
    });

    //CARGA LOS PRODUCTOS MAS VENDIDOS DEL RESTAURANTE EN GENERAL
    $.ajax({
        type: "GET",
        url: "/Busquedas/ProductosFavoritos/",
        dataType: "JSON",
        success: function (data) {

            var doughnutData, doughnutOptions, ctx4;

            if (data.length <= 0) {

                $("#producto1").append('<p><i class="fa fa-square colorDisabled"></i> Sin registros disponibles</p>');

                //SI NO EXISTE NINGUN DATO
                doughnutData = {
                    labels: ["Sin registro"],
                    datasets: [{
                        data: [100],
                        backgroundColor: ["#dedede"]
                    }]
                };


                doughnutOptions = {
                    responsive: false,
                    legend: {
                        display: false
                    }
                };

                ctx4 = document.getElementById("productosSolicitados").getContext("2d");
                new Chart(ctx4, { type: 'doughnut', data: doughnutData, options: doughnutOptions });
            } else {
                var menu = new Array();
                var cantidad = new Array();

                $("#producto1").append('<p><i class="fa fa-square green"></i> ' + data[0].DescripcionMenu + '</p>');
                $("#producto2").append('<p><i class="fa fa-square blue"></i> ' + data[1].DescripcionMenu + '</p>');
                $("#producto3").append('<p><i class="fa fa-square aero"></i> ' + data[2].DescripcionMenu + '</p>');

                for (var i = 0; i < data.length; i++) {
                    menu.push(data[i].DescripcionMenu);
                    cantidad.push(data[i].Cantidad);
                }

                doughnutData = {
                    labels: menu,
                    datasets: [{
                        data: cantidad,
                        backgroundColor: ["#1ABB9C", "#3498DB", "#9CC2CB"]//["#a3e1d4", "#dedede", "#9CC3DA"]
                    }]
                };


                doughnutOptions = {
                    responsive: false,
                    legend: {
                        display: false
                    }
                };

                ctx4 = document.getElementById("productosSolicitados").getContext("2d");
                new Chart(ctx4, { type: 'doughnut', data: doughnutData, options: doughnutOptions });
            }
        }//FIN SUCCESS
    });
});

//BUSCA LOS DATOS PARA CARGAR LOS LETREROS DEL DASHSBOARD
function dashboardSign() {
    $.ajax({
        type: "GET",
        url: "/Busquedas/DashboardSign/",
        success: function (data) {
            var ordActivas = data.ordenesActivas == null ? 0 : data.ordenesActivas;
            var ordTotales = data.ordenesTotales == null ? 0 : data.ordenesTotales;
            var ventTotales = data.ventasTotales == null ? "0.0" : data.ventasTotales;

            $("#ordenesActivas").html(ordActivas);
            $("#ordenesTotales").html(ordTotales);
            $("#ventasTotales").html("$ " + ventTotales);
        }
    });
}//FIN FUNCTION

//MUESTRA LA TABLA CON LOS DATOS GENERALES (TODOS LOS PEDIDOS DE TODOS LOS MESEROS)
function CrearTabla(EmpleadoId, EmpleadoRole) {
    //RECUPERAR TODAS LAS ORDENES DEL DIA
    $.ajax({
        type: "GET",
        url: "/Ordenes/Ordenes/",
        data: { EmpleadoId, EmpleadoRole },
        success: function (data) {
            if (data.length == 0) {
                var agregar = '<h2 id="txt" style="text-align:center;">No hay ordenes activas</h2>';//AGREGA LETRERO
                $("#x_content").append(agregar);
            } else {
                //CREA EL ENCABEZADO DE LA TABLA DE ORDENES
                var thead = '<tr> <th>No. Orden</th> <th>Hora ordenada</th> <th>Cliente</th>';
                var theadFin = "";

                //CREA EL TBODY DE LA TABLA ORDENES
                var tbodyFin = "";
                var tbody = "";

                for (var i = 0; i < Object.keys(data).length; i++) {

                    tbody = '<tr><th scope="row">' + cargarCodigo(data[i].CodigoOrden) + '</th><th>' + data[i].HoraOrden + '</th><td>' + data[i].Cliente + '</td>';

                    //DEPENDIENDO SI EL ROL ES DIFERENTE A MESERO
                    if (EmpleadoRole == "Bartender") {
                        //nada
                    } else if (EmpleadoRole == "Cocina") {
                        //nada
                    } else if (EmpleadoRole != "Mesero") {
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
                    $("#hOrdenes").html(thead + theadFin);
                    //AGREGA EL CUERPO DE LA TABLA
                    $("#bOrdenes").append(tbody + tbodyFin);
                }
            }
        }
    });//FIN AJAX
}//FIN FUNCTION

function cargarLinks(EmpledoRole) {

}

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

