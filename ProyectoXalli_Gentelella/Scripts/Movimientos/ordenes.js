//CARGA EL CODIGO DE LA ORDEN AUTOMATICAMENTE
function cargarCodigo() {
    $.ajax({
        type: "GET",
        url: "/Ordenes/OrdenesCode",
        success: function (data) {
            if (data < 10) {
                $("#codigoOrden").val("000" + data);
            } else if (data >= 10 || data < 100) {
                $("#codigoOrden").val("00" + data);
            } else if (data >= 100 || data < 1000) {
                $("#codigoOrden").val("0" + data);
            } else {
                $("#codigoOrden").val(data);
            }

        }
    });
}

//CARGA DE ORDEN EL CODIGO A LA VISTA
$(document).ready(function () {
    cargarCodigo();

    //LIMPIAR
    limpiarInicio();
    limpiarInputs();
    tablaVacia();

    var loginId = $("#session").val();

    //PONER POR DEFECTO EL EMPLEADO LOGEADO
    $.ajax({
        type: "GET",
        url: "/Ordenes/LoggedUser/",
        data: { LoginId: loginId },
        success: function (data) {
            $("#mesero").val(data.Nombre);
            $("#mesero").attr("value", data.MeseroId);
        },
        error: function () {
            Alert("Error", "Datos del Colaborador sin cargar", "error");
            $("#btnGuardarOrden").attr("disabled", true);
        }
    });

    var elem = document.querySelector('.js-switch');
    init = new Switchery(elem, { secondaryColor: '#7c8bc7' });
    $(".switchery").attr("onclick", "Check()");
});

//LIMPIAR TODO
function limpiarInicio() {
    //INICIALIZAR EL SELECT2 CATEGORIA
    $("select").val("");
    $('select').trigger('change'); // Notify any JS components that the value changed

    var elem = document.querySelector('.js-switch');

    if (elem.checked == false) {
        elem.click();
    }

    $("#titleCliente").html("Visitante");//CAMBIAR EL TITULO DE TIPO CLIENTE
    $("#identificacion").attr("readonly", "true");//DESACTIVAR CAMPO IDENTIFICACION
    $("#buscarCliente").attr("disabled", "true");//DESACTIVAR BOTON BUSCAR CLIENTE
    $("#agregarCliente").attr("disabled", "true");//DESACTIVAR BOTON BUSCAR CLIENTE

    //LIMPIAR TABLA
    $("#table_body").empty();

    //var agregarNada = '<tr id="nada">' +
    //    ' <td colspan="5" style="text-align:center!important;">- Sin pedidos aun -</td>' +
    //    '</tr>';

    //$("#table_body").append(agregarNada);

    $("#total").html("$ 0");
}

//CONFIRMACION DE ACTIVAR/DESACTIVAR
function Check() {

    var elem = document.querySelector('.js-switch');
    $("#btnGuardarOrden").removeAttr("disabled");//ACTIVANDO EL BOTON

    //DEPENDE DE SU ESTADO MANDAMOS UN ALERT
    if (elem.checked == true) {
        //VISITANTE
        $("#titleCliente").html("Visitante");//CAMBIAR EL TITULO DE TIPO CLIENTE
        $("#identificacion").attr("readonly", "true");//DESACTIVAR CAMPO IDENTIFICACION
        $("#identificacion").val("readonly", "true");//DESACTIVAR CAMPO IDENTIFICACION
        $("#buscarCliente").attr("disabled", "true");//DESACTIVAR BOTON BUSCAR CLIENTE
        $("#agregarCliente").attr("disabled", "true");//DESACTIVAR BOTON BUSCAR CLIENTE

        limpiarInputs();

    } else {
        //HUESPED
        $("#titleCliente").html("Huésped");//CAMBIAR EL TITULO DE TIPO CLIENTE
        $("#identificacion").removeAttr("readonly");//ACTIVAR CAMPO IDENTIFICACION
        $("#buscarCliente").removeAttr("disabled");//ACTIVAR BOTON BUSCAR CLIENTE
        $("#agregarCliente").removeAttr("disabled");//ACTIVAR BOTON BUSCAR CLIENTE
    }
}//FIN FUNCION

//LIMPIAR INPUTS DE CLIENTE
function limpiarInputs() {
    //LIMPIAR INPUTS CLIENTE
    $("#identificacion").val("");
    $("#nombreCliente").val("");
    $("#nombreCliente").attr("val", "");
    $("#rucOrden").val("");
}

//INICIALIZADOR DE DATEPICKER
$('#fechaOrden').datetimepicker({
    format: 'DD/MM/YYYY',
    defaultDate: new Date(),
    locale: 'es'
});

//AGREGAR LA OPCION DONDE IRA EL PLACEHOLDER DEL SELECT 2
//$("select").prepend("<option value='-1' disabled>Seleccione</option>");
//$("#mesa").prepend("");

//INICIALIZADOR DE LENGUAJE SELECT 2
$('select').select2({
    placeholder: { id: "-1", text: "Seleccione" },//CARGAR PRIMERO EL PLACEHOLDER
    allowClear: true,
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
    } else if (existencia == -1) {
        Alert("Error", "La bebida no esta disponible para agregar", "error");
    } else if (existencia == -2) {
        agregarDetalle();
    } else if (existencia > 0) {
        if (parseInt(cantidad) <= parseInt(existencia)) {
            agregarDetalle();
        } else {
            Alert("Error", "La cantidad no puede ser mayor a la existencia", "error");
        }
    } if (existencia == 0) {
        Alert("Error", "Este producto no tiene existencias", "error");
    }
}//FIN FUNCTION

function agregarDetalle() {
    //SE ELIMINA LA FILA DE INICIO
    $("#noProd").remove();

    var platillo = $("#platillo").val(), platilloId = $("#platillo").attr("name"), cantidad = $("#cantidadOrden").val(),
        precio = $("#precioOrden").val(), nota = $("#notaOrden").val();

    //SE QUITA EL SIGNO DE DOLAR
    precio = (precio.split("$ "))[1];

    var filas = $("#table_body").find("tr");
    var registrado = false, i = 0;
    var precioTotal = (parseFloat(precio).toFixed(2) * parseFloat(cantidad).toFixed(2)).toFixed(2);

    var agregar = "";

    //RECORRER LOS VALORES DE LA TABLA
    while (i < filas.length && registrado === false) {
        var celdas = $(filas[i]).find("td"); //devolverá las celdas de una fila

        //AGARRAR EL VALUE ALMACENADO EN LA FILA - PRODUCTO
        var comp = $(celdas[0]).attr("value");

        //COMPARAMOS QUE EL PRODUCTO A INGRESAR NO SEA EL MISMO AL QUE YA ESTA AGREGADO
        if (comp === platilloId) {
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
        agregar += '<td class=" last"><a class="btn btn-primary btn-xs" id="boton" onclick="editPlatillo(this);"><i class="fa fa-edit"></i></a>';
        agregar += '<a class="btn btn-danger btn-xs" onclick = "deletePlatillo(this);"> <i class="fa fa-trash"></i></a></td>';
        agregar += '</tr>';

        //AGREGAR PRODUCTO A LA TABLA
        $("#table_body").append(agregar);

        CalcularTotal();

        $("#smallModal").modal("hide");
    } else {
        Alert("Error", "El platillo seleccionado ya se encuentra en la tabla", "error");
    }

    $("body").removeClass("modal-open");
}

$("#identificacion").blur(function () {
    $("#nombreCliente").val("");
    $("#rucOrden").val("");

    if ($(this).val() != "") {
        $.ajax({
            type: "GET",
            url: "/Ordenes/DataClient/",
            data: {
                identificacion: $(this).val().trim()
            },
            dataType: "JSON",
            success: function (data) {
                $("#btnGuardarOrden").attr("disabled", false);

                if (data.cliente != null) {
                    $("#nombreCliente").val(data.cliente.Nombres);
                    $("#nombreCliente").attr("val", data.cliente.ClienteId);
                    $("#rucOrden").val(data.cliente.RUC);
                } else {
                    Alert("Error", "No existe registros con esa identificación", "error");
                    $("#btnGuardarOrden").attr("disabled", true);
                }

                if (data.mensaje != "-1") {
                    Alert("Alerta", data.mensaje, "warning");
                    $("#btnGuardarOrden").attr("disabled", true);
                }
            }
        });
    }
});

//FUNCION PARA ALMACENAR LA ORDEN
function guardarOrden() {
    var codigo = $("#codigoOrden").val(), meseroId = $("#mesero").attr("value"), clienteId = "", mesa = $("#mesa").val();

    //PASAR UN VALOR POR DEFECTO EN CASO QUE NO EXISTA REGISTRO DE UN NUEVO CLIENTE
    clienteId = $("#nombreCliente").attr("val") != "" ? $("#nombreCliente").attr("val") : 0;

    //CREAR LA FECHA DE ORDEN
    var date = $("#fechaOrden").val() + " " + moment().format('h:mm:ss a');

    var OrdenDetails = new Array();

    //ALMACEMAR ELEMENTOS DE LA TABLA
    $("#table_body tr").each(function () {

        if ($(this).attr("id") != "noProd") {
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
            item["EstadoDetalleOrden"] = false;

            OrdenDetails.push(item);
        }
    });

    //alert(JSON.stringify(OrdenDetails));

    if (OrdenDetails.length > 0) {
        var data = "Codigo=" + parseInt(codigo) + "&MeseroId=" + meseroId + "&ClienteId=" + clienteId + "&FechaOrden=" + date + "&detalleOrden=" + JSON.stringify(OrdenDetails) + "&mesaId=" + mesa;

        //alert(data);

        if (validado()) {
            $.ajax({
                type: "POST",
                url: "/Ordenes/Create/",
                data: data,
                success: function (data) {
                    if (data.success) {
                        //ELIMINAR LA MESA 
                        var mesa = document.getElementById("mesa");
                        mesa.remove(mesa.selectedIndex);

                        //LIMPIAR PANTALLA
                        limpiarInputs();
                        limpiarInicio();
                        tablaVacia();

                        //VOLVER A CARGAR EL CODIGO
                        cargarCodigo();

                        AlertTimer("Completado", data.message, "success");
                    } else {
                        Alert("Error", data.message, "error");
                    }
                }
            });
        }
        else {
            Alert("Error", "Se encontraron campos requeridos vacios", "error");
        }
    } else {
        Alert("Error", "Se encontraron campos requeridos vacios", "error");
    }
}//FIN FUNCTION

//VALIDAR CAMPOS QUE NO QUEDEN VACIOS
function validado() {
    var validado = false;

    if ($("#table_body tr").length != 0 && $("#codigoOrden").val() && $("#fechaOrden").val() != "" && $("#mesa").val() != null) {
        //SI ES HUESPED Y NO ESTA VACIO LOS DATOS PASAR TRUE
        if (!$(".js-switch").is(":checked") && $("#nombreCliente").val() != "") {
            validado = true;
        } else
            //SI NO ES HUESPED Y LOS DATOS ESTAN VACIOS NO HAY PEDO
            if ($(".js-switch").is(":checked") && $("#nombreCliente").val() == "") {
                validado = true;
            }
    }

    return validado;
}//FIN FUNCTION

//FUNCION PARA ALMACENAR UN OBJETO CLIENTE
function saveCustomer() {
    var nombre, apellido, documento, ruc, email, telefono, tipo;

    nombre = $("#nombre").val();
    apellido = $("#apellido").val();
    documento = $("#numero").val().toUpperCase();
    ruc = $("#ruc").val().toUpperCase();
    email = $("#email").val();
    telefono = $("#telefono").val();
    tipo = $("#documento").val();

    if (validandoCliente() == true) {
        //FUNCION AJAX
        $.ajax({
            type: "POST",
            url: "/Clientes/Create",
            dataType: "JSON",
            data: {
                Nombre: nombre, Apellido: apellido, Documento: documento, RUC: ruc,
                Email: email, Telefono: telefono, Tipo: tipo
            },//OTRA MANERA DE ENVIAR PARAMETROS AL CONTROLADOR
            success: function (data) {
                if (data.success) {
                    var id = data.clienteId;

                    $("#nombreCliente").val(nombre + " " + apellido);
                    $("#identificacion").val(documento);
                    $("#rucOrden").val(ruc);
                    $("#nombreCliente").attr("val", id);

                    $("#small-modal").modal("hide"); //CERRAR MODAL
                    AlertTimer("Completado", data.message, "success");
                } else
                    Alert("Error al almacenar", data.message, "error");//MENSAJE DE ERROR
            },
            error: function () {
                Alert("Error al almacenar", "Intentelo de nuevo", "error");
            }
        });//FIN AJAX
    } else {
        Alert("Error", "Campos vacios", "error");
    }
}

//FUNCION PARA VALIDAR CAMPOS VACIOS
function validandoCliente() {
    if ($("#nombre").val() != "" && $("#apellido").val() != "" && $("#numero").val() != "" && $("#email").val() != "") {
        return true;
    } else
        return false;
}//FIN FUNCTION

//$("#identificacion").keyup(function () {
//    $("#btnGuardarOrden").removeAttr("disabled");//ACTIVANDO EL BOTON
//    var iden = $(this).val();

//    if (iden.length == 1) {
//        if ($.isNumeric(iden[0])) {

//            $(this).attr("maxlength", 16);
//            $(this).mask("A00-000000-0000B", {
//                translation: {
//                    'A': { pattern: /[0-6]/ },//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
//                    'B': { pattern: /[A-Za-z]/ }//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
//                }
//            });
//        } else {
//            $(this).attr("maxlength", 10);
//            $(this).mask("AAA0000000", {
//                translation: {
//                    'A': { pattern: /[a-zA-Z]/ }//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
//                }
//            });
//        }
//    } else if (iden.length == 0) {
//        $(this).unmask();
//    }
//});