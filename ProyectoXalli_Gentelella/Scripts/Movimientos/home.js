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
            var ColabId = data.ColaboradorId;
            var role = data.Role;

            CrearTabla(ColabId, role);
            doughnutChart(role);
        }
    });

    lineChart();

});

//MUESTRA EL GRAFICO DE PASTEL EN EL INDEX HOME
function doughnutChart(role) {
    //AGREGA EL TITULO DEL GRAFICO DE PASTEL
    if (role == "Cocinero") {
        $("#titleProducts").html("Platillos más solicitados");
    } else if (role == "Bartender") {
        $("#titleProducts").html("Bebidas más solicitados");
    } else {
        $("#titleProducts").html("Productos más solicitados");
    }

    //CARGA LOS PRODUCTOS MAS VENDIDOS DEL RESTAURANTE EN GENERAL
    $.ajax({
        type: "GET",
        url: "/Busquedas/ProductosFavoritos/",
        data: { Role: role },
        dataType: "JSON",
        success: function (data) {

            var doughnutData, ctx4;

            var doughnutOptions = {
                responsive: true,
                legend: {
                    display: true
                }
            };

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
            } else {
                var menu = new Array();
                var cantidad = new Array();
                var clase = ["green", "blue", "aero"];

                for (var i = 0; i < data.length; i++) {
                    var itemP = data[i].DescripcionMenu;
                    var itemC = data[i].Cantidad;

                    menu.push(itemP);
                    cantidad.push(itemC);
                }

                if (data.length == 1) {
                    $("#producto1").append('<p id="platilloDesc"><i id="p1" class="fa fa-square green"></i> ' + data[0].DescripcionMenu + '</p>');
                } else if (data.length == 2) {
                    $("#producto1").append('<p id="platilloDesc"><i id="p1" class="fa fa-square green"></i> ' + data[0].DescripcionMenu + '</p>');
                    $("#producto2").append('<p id="platilloDesc"><i id="p1" class="fa fa-square blue"></i> ' + data[1].DescripcionMenu + '</p>');
                } else {
                    $("#producto1").append('<p id="platilloDesc"><i id="p1" class="fa fa-square green"></i> ' + data[0].DescripcionMenu + '</p>');
                    $("#producto2").append('<p id="platilloDesc"><i id="p1" class="fa fa-square blue"></i> ' + data[1].DescripcionMenu + '</p>');
                    $("#producto3").append('<p id="platilloDesc"><i id="p1" class="fa fa-square aero"></i> ' + data[2].DescripcionMenu + '</p>');
                }

                doughnutData = {
                    labels: menu,
                    datasets: [{
                        data: cantidad,
                        backgroundColor: ["#1ABB9C", "#3498DB", "#9CC2CB"]//["#a3e1d4", "#dedede", "#9CC3DA"]
                    }]
                };

            }

            ctx4 = document.getElementById("productosSolicitados").getContext("2d");
            new Chart(ctx4, { type: 'doughnut', data: doughnutData, options: doughnutOptions });

        }//FIN SUCCESS
    });
}//FIN FUNCTION

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
            $("#ventasTotales").html("$ " + formatoPrecio(ventTotales.toString()));
        }
    });
}//FIN FUNCTION

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

//CARGA LOS LINKS DE LOS BOTONES DE LOS ROLES DIFERENTE A MESEROS
function cargarLinks(EmpleadoRole, Id) {
    var links = "";

    if (EmpleadoRole == "Bartender" || EmpleadoRole == "Cocinero") {
        var empleado = EmpleadoRole == "Bartender" ? 1 : 2;
        links = '<button type="button" onclick="RedirectToPrep(' + Id + ',' + empleado + ')" class="btn btn-primary btn-sm">Ver Orden</button>';
    } else {
        //SI ES ADMIN
        links = '<div class="btn-group">' +
            '<button data-toggle="dropdown" class="btn btn-primary dropdown-toggle btn-sm" type="button" aria-expanded="true">' +
            'Acción   <span class="caret"></span>' +
            '</button>' +
            '<ul role="menu" class="dropdown-menu">' +
            '<li>' +
            '<a id="buttonOrder" onclick="RedirectToEdit(' + Id + ')">Ver orden</a>' +
            '</li>' +
            '<li>' +
            '<a onclick="RedirectToComanda(' + Id + ')">Mostrar comanda</a>' +
            '</li>' +
            '</ul>' +
            '</div>';
    }

    return links;
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

//FUNCION QUE DIRIGE AL FORMULARIO DE ORDEN PARA AGREGAR NUEVOS ITEMS
function RedirectToPrep(OrderId, Role) {
    var url = "/Ordenes/Preparacion?ordenId=" + OrderId + "&rol=" + Role;
    window.location.href = url;
}

//FUNCION PARA CREAR EL GRAFICO DE LINEA
function lineChart() {

    //CARGA LOS PRODUCTOS MAS VENDIDOS DEL RESTAURANTE EN GENERAL
    $.ajax({
        type: "GET",
        url: "/Busquedas/ventasDiaMes/",
        dataType: "JSON",
        success: function (data) {

            var fecha = new Array();
            var ventas = new Array();

            for (var i = 0; i < data.length; i++) {
                var itemF = data[i].Fecha;
                var itemV = data[i].TotalVentas;

                fecha.push(itemF);
                ventas.push(formatoPrecio(itemV.toString()));
            }

            var lineData = {
                labels: fecha,
                datasets: [
                    {
                        label: "Ventas $ ",
                        backgroundColor: 'rgba(26,179,148,0.5)',
                        borderColor: "rgba(26,179,148,0.7)",
                        pointBackgroundColor: "rgba(26,179,148,1)",
                        pointBorderColor: "#fff",
                        data: ventas
                    }
                ]
            };

            var lineOptions = {
                responsive: true,
                //LAS ESCALAS INICIARAN EN CERO, EN CASO QUE NO HAYA VALORES
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true
                        }
                    }]
                }
            };

            var ctx = document.getElementById("VentasChart").getContext("2d");
            new Chart(ctx, { type: 'line', data: lineData, options: lineOptions });
        }//FIN SUCCESS
    });
}