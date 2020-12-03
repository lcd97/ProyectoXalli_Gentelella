$(document).ready(function () {
    //INICIALIZAR EL SELECT2 CATEGORIA
    $("#categoria").val("-1");
    $('#categoria').trigger('change'); // Notify any JS components that the value changed

    var orderId = $("#codigoOrden").attr("name");

    var elem = document.querySelector('.js-switch');
    var init;

    $.ajax({
        type: "GET",
        url: "/Ordenes/getOrderAndDetails",
        data: { orderId },
        success: function (data) {

            $("#codigoOrden").val(cargarCodigo(data.Principal.Codigo));
            $("#fechaOrden").val(data.Principal.Fecha);
            $("#mesero").val(data.Principal.Mesero);
            $("#mesa").val(data.Principal.Mesa);

            //HUESPED
            if (data.Principal.TipoCliente == 1) {
                $("#titleCliente").html("Huesped");//CAMBIAR EL TITULO DE TIPO CLIENTE
                init = new Switchery(elem, { secondaryColor: '#7c8bc7' });
                init.disable();

                $("#identificacion").val(data.Principal.ClienteIdent);
                $("#nombreCliente").val(data.Principal.Nombres);
                $("#rucOrden").val(data.Principal.RUC);
            } else {
                init = new Switchery(elem, { secondaryColor: '#64BD63' });
                init.disable();
                $("#identificacion").attr("name", data.Principal.ClienteIdent);
            }

            //LLENAR EL DETALLE
            var agregar = "";
            //var precioTotal = 0;

            for (var i = 0; i < Object.keys(data.Details).length; i++) {

                var calculo = data.Details[i].PrecioUnitario * data.Details[i].Cantidad;
                //precioTotal += calculo;

                var estado = data.Details[i].Estado == true ? "Finalizado" : "Pendiente";

                //GENERAR FILA DEL PRODUCTO A LA TABLA
                agregar += '<tr class="even pointer">';
                agregar += '<td class="" value ="' + data.Details[i].PlatilloId + '">' + data.Details[i].Platillo + '</td>';
                agregar += '<td class="" value = "' + data.Details[i].Nota + '">' + "$ " + (data.Details[i].PrecioUnitario) + '</td>';
                agregar += '<td class="" Style ="text-align: center;">' + data.Details[i].Cantidad + '</td>';
                agregar += '<td class="" >' + "$ " + (calculo) + '</td>';
                agregar += '<td class="" value ="true"><span class="label label-success pull-right">' + estado + '</span></td>';
                agregar += '<td class=" last"><a disabled class="btn btn-primary btn-xs"><i class="fa fa-edit"></i></a>';
                agregar += '<a disabled class="btn btn-danger btn-xs"> <i class="fa fa-trash"></i></a></td>';
                agregar += '</tr>';
            }

            ////CALCULAR EL TOTAL
            //var total = parseFloat(CalcularTotal());

            //AGREGAR PRODUCTO A LA TABLA
            $("#table_body").append(agregar);
            ////AGREGAR EL TOTAL TFOOT
            //$("#total").html("$ " + (total + precioTotal));

            CalcularTotal();
        }
    });
});

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

//INICIALIZADOR DE DATEPICKER
$('#fechaOrden').datetimepicker({
    format: 'DD/MM/YYYY',
    defaultDate: new Date(),
    locale: 'es'
});

//INICIALIZADOR DE LENGUAJE SELECT 2
$('.js-example-basic-single').select2({
    placeholder: { id: "-1", text: "Seleccione" },//CARGAR PRIMERO EL PLACEHOLDER
    //MODIFICAR LAS FRASES DEFAULT DE SELECT2
    language: {

        noResults: function () {

            return "No hay resultado";
        },
        searching: function () {

            return "Buscando...";
        }
    }
});

//AGREGA EL PLATILLO SELECCIONADO A LA TABLA
function addDetails() {
    var cantidad = $("#cantidadOrden").val(), existencia = $("#existencia small").attr("val");

    if (cantidad <= 0) {
        Alert("Error", "Ingrese una cantidad a ordenar", "error");
    } else if (existencia > 0) {
        if (parseInt(cantidad) <= parseInt(existencia)) {
            agregarDetalle();
        } else {
            Alert("Error", "La cantidad no puede ser mayor a la existencia", "error");
        }
    } else {
        agregarDetalle();
    }
}//FIN FUNCTION

function agregarDetalle() {
    var platillo = $("#platillo").val(), platilloId = $("#platillo").attr("name"), cantidad = $("#cantidadOrden").val(),
        precio = $("#precioOrden").val(), nota = $("#notaOrden").val();

    //SE QUITA EL SIGNO DE DOLAR
    precio = (precio.split("$ "))[1];

    var filas = $("#table_body").find("tr");
    var registrado = false, i = 0;
    var precioTotal = (precio * cantidad);

    var agregar = "";

    //RECORRER LOS VALORES DE LA TABLA
    while (i < filas.length && registrado === false) {
        var celdas = $(filas[i]).find("td"); //devolverá las celdas de una fila

        //AGARRAR EL VALUE ALMACENADO EN LA FILA - PRODUCTO
        var comp = $(celdas[0]).attr("value");

        //COMPARAMOS QUE EL PRODUCTO A INGRESAR NO SEA EL MISMO AL QUE YA ESTA AGREGADO
        if (comp === platilloId && celdas.eq(4).attr("value") == "false") {
            registrado = true;
        } else {
            registrado = false;
        }

        i++;
    }//FIN WHILE

    //SI NO SE EN
    if (!registrado) {
        //GENERAR FILA DEL PRODUCTO A LA TABLA
        agregar += '<tr class="even pointer">';
        agregar += '<td class="" value ="' + platilloId + '">' + platillo + '</td>';
        agregar += '<td class="" value = "' + nota + '">' + "$ " + precio + '</td>';
        agregar += '<td class="" Style ="text-align: center;">' + cantidad + '</td>';
        agregar += '<td class="" >' + "$ " + precioTotal + '</td>';
        agregar += '<td class="" value = "false"><span class="label label-warning pull-right">Nuevo</span></td>';
        agregar += '<td class=" last"><a class="btn btn-primary btn-xs" id="boton" onclick="editPlatillo(this);"><i class="fa fa-edit"></i></a>';
        agregar += '<a class="btn btn-danger btn-xs" onclick = "deletePlatillo(this);"> <i class="fa fa-trash"></i></a></td>';
        agregar += '</tr>';

        ////CALCULAR EL TOTAL
        //var total = parseFloat(CalcularTotal());

        //AGREGAR PRODUCTO A LA TABLA
        $("#table_body").prepend(agregar);
        ////AGREGAR EL TOTAL TFOOT
        //$("#total").html("$ " + (total + parseFloat(precioTotal)));

        $("#smallModal").modal("hide");
        CalcularTotal();
    } else {
        Alert("Error", "El platillo seleccionado ya se encuentra en la tabla", "error");
    }

    $("body").removeClass("modal-open");
}

//FUNCION PARA EDITAR LA ORDEN -INICIO
function editOrden(terminar) {
    //SI SE PRESIONO EL BOTON PARA CERRAR LA ORDEN
    if (terminar == 2) {
        swal({
            title: "¿Esta seguro que quiere cerrar la orden?",
            icon: "warning",
            closeOnClickOutside: false,
            closeOnEsc: false,
            buttons: {
                activar: {
                    text: "No",
                    value: "NO" //VALOR PARA UTILIZARLO EN EL SWITCH
                },
                desactivar: {
                    text: "Sí",
                    value: "YES" //VALOR PARA UTILIZARLO EN EL SWITCH
                }
            }//FIN DE BUTTONS
        })//FIN DEL SWAL

            .then((value) => {
                switch (value) {

                    case "NO":
                        swal.close();
                        break;

                    case "YES":
                        editarOrden(2);
                        break;
                    default: swal.close();
                }//FIN SWITCH
            });//FIN THEN
    } else {
        editarOrden(1);
    }
}

//ALMACENA TODOS LOS NUEVOS ITEMS DE LA ORDEN
function editarOrden(terminar) {
    var codigo = $("#codigoOrden").val(), date = $("#fechaOrden").val() + " " + moment().format('h:mm:ss a');
    var OrdenDetails = new Array();

    var itemsNuevos = false;

    //ALMACEMAR ELEMENTOS DE LA TABLA
    $("#table_body tr").each(function () {

        var row = $(this);
        var item = {};

        var precio = row.find("td").eq(1).html();
        var getPrice = precio.split("$ ");

        item["Id"] = 0;
        item["CantidadOrden"] = row.find("td").eq(2).html();
        item["PrecioOrden"] = getPrice[1];
        item["NotaDetalleOrden"] = row.find("td").eq(1).attr("value");
        item["MenuId"] = row.find("td").eq(0).attr("value");
        item["OrdenId"] = 0;
        item["EstadoDetalleOrden"] = row.find("td").eq(4).attr("value");

        OrdenDetails.push(item);

        //SI EXISTEN ELEMENTOS NUEVOS A LA ORDEN
        if (row.find("td").eq(4).attr("value") == "false") {
            itemsNuevos = true;
        }
    });

    if (!itemsNuevos) {
        OrdenDetails = "";
    }

    var data = "Codigo=" + codigo + "&FechaOrden=" + date + "&EstadoOrden=" + terminar + "&detalleOrden=" + JSON.stringify(OrdenDetails);

    $.ajax({
        type: "POST",
        url: "/Ordenes/EditOrder/",
        data: data,
        success: function (data) {
            if (data.success) {
                //REDIRECCIONAR A TODAS LAS ORDENES
                //Alert("Completado", data.message, "success");
                var url = "/Ordenes/VerOrdenes/?mensaje=" + data.message;
                window.location.href = url;
            } else {
                Alert("Error", data.mesage, "error");
            }
        }
    });//FIN AJAX
}//FIN FUNCION