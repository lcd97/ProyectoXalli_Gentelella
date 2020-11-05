$(document).ready(function () {
    $('#menu_toggle').click();

    $('#wizard').smartWizard();
    $(".buttonNext").addClass("buttonDisabled");//DESACTIVAR EL BOTON SIGUIENTE

    $(".buttonNext").attr("onclick", "nextStepVal()");//DESACTIVAR EL BOTON SIGUIENTE
    obtenerCambio();

    $("#descuentoPago").mask('###00%', { reverse: true });

    $("#propinaPago").mask("0,000.00", { reverse: true });
    $("#pagar").mask("0,000.00", { reverse: true });
    $("#rec").mask("0,000.00", { reverse: true });
});

//SI SOLO SE ENCUENTRA UN CARACTER % O 0ELIMINARLO 
$("#descuentoPago").keyup(function () {
    var desc = $(this).val();

    if (desc === "%" || desc === "0%" || desc === "00%") {
        $(this).val("");
    }
});

//OBTENGO EL TIPO DE CAMBIO DEL DIA SEGUN EL BCN
function obtenerCambio() {
    $.ajax({
        type: "GET",
        url: "/Facturaciones/CalcularCambioHoy/",
        success: function (data) {
            $("#cambio").val(formatoPrecio(data.toString()));
        }
    });
}

//INICIALIZAR EL SELECT2
$('.js-example-basic-single').select2({
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

//AGREGA LA MASCARA A IDENTIFICACION
$("#identificacion").keyup(function () {
    var iden = $(this).val();

    if (iden.length == 1) {
        if ($.isNumeric(iden[0])) {

            $(this).attr("maxlength", 16);
            $(this).mask("A00-000000-0000B", {
                translation: {
                    'A': { pattern: /[0-6]/ },//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
                    'B': { pattern: /[A-Za-z]/ }//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
                }
            });
        } else {
            $(this).attr("maxlength", 10);
            $(this).mask("AAA0000000", {
                translation: {
                    'A': { pattern: /[a-zA-Z]/ }//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
                }
            });
        }
    } else if (iden.length == 0) {
        $(this).unmask();
    }
});

//BUSCA EN LA BD EL REGISTRO DE UN CLIENTE
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
                }

                if (data.mensaje != "-1") {
                    Alert("Alerta", data.mensaje, "warning");
                }
            }
        });
    } else {//SI ESTA VACIO ELIMINAR ID CLIENTE EN CASO QUE EXISTA
        $("#nombreCliente").attr("val", "");
    }
});

//CARGA TODAS LAS ORDENES DE DET CLIENTE
function buscarOrden() {
    var cliente = $("#nombreCliente").attr("val") == "" ? 0 : $("#nombreCliente").attr("val");
    $(".buttonNext").removeClass("buttonDisabled");

    //SI SE ENCUENTRA VACIO QUE CARGUE TODAS LAS ORDENES DE VISITANTE
    $.ajax({
        type: "GET",
        url: "/Facturaciones/CargarOrdenes/",
        data: { ClienteId: cliente },
        success: function (data) {
            var agregar = "";

            if (data.orden.length <= 0) {
                agregar += '<tr class="even pointer">' +
                    '<td class="a-right a-right" style="text-align:center" colspan="5"> SIN ORDENES QUE MOSTRAR</td>' +
                    '</tr>';
                $("#celdaChk").css("display", "none");
                $(".buttonNext").addClass("buttonDisabled");

            } else {
                $("#celdaChk").removeAttr("Style");
                $(".buttonNext").removeClass("buttonDisabled");

                var clase = "checkbox";
                if (data.ClienteId == 0) {
                    clase = "radio";
                    var c = document.getElementById("check-all");
                    c.style.display = "none";
                } else {
                    var d = document.getElementById("check-all");
                    d.style.display = "none";
                }


                for (var i = 0; i < data.orden.length; i++) {
                    agregar += '<tr class="even pointer">' +
                        '<td class="a-center">' +
                        '<input val="' + data.orden[i].OrdenId + '" id="seleccion" type="' + clase + '" class="flat" name="table_records">' +
                        '</td>' +
                        '<td class=" ">' + cargarCodigo(data.orden[i].CodigoOrden) + '</td>' +
                        '<td class=" ">' + data.orden[i].FechaOrden + '</td>' +
                        '<td class=" ">' + data.orden[i].Cliente + '</td>' +
                        '<td class=" ">' + data.orden[i].Mesero + '</td>' +
                        '<td class="a-right a-right ">$ ' + formatoPrecio((data.orden[i].SubTotal).toString()) + '</td>' +
                        '</tr>';
                }
            }

            $("#bodyOrden").html(agregar);
            //INICIALIZA EL CHECKBOX DE ESTADO
            $('input.flat').iCheck({
                checkboxClass: 'icheckbox_flat-green',
                radioClass: 'iradio_flat-green'
            });
        }//FIN SUCCESS
    });
}

//CARGA EL CODIGO DE LA ORDEN AUTOMATICAMENTE
function cargarCodigo(data) {
    var num = data;

    if (data < 10) {
        num = "000" + data;
    } else if (data >= 10 || data < 100) {
        num = "00" + data;
    } else if (data >= 100 || data < 1000) {
        num = "0" + data;
    }

    return num;
}

//VALIDA PASOS PARA MOSTRAR INFORMACION DE DETALLE DE ORDENES
function nextStepVal() {
    //OBTENGO EN QUE PASO ESTA ACTUALMENTE
    var step = $(".selected").attr("rel");
    var clienteId = $("#nombreCliente").attr("val");//OBTENGO EL ID DEL CLIENTE
    var nombre = $("#nombreCliente").val() != "" ? $("#nombreCliente").val() : "Visitante";
    var ruc = $("#rucOrden").val() != "" ? $("#rucOrden").val() : "N/A";

    //SI ESTA EN EL PASO 1 - CARGAR DATOS DE PASO 2
    if (step == 1) {

        //LIMPIAMOS DETALLE PAGO ORDEN EN CASO QUE HAYA ALGO
        $("#carnetSection").attr("hidden");
        $("#bodyDetalle").empty();
        $("#subTotalOrden").html("$ 0");
        $("#ivaOrden").html("$ 0");
        $("#totalOrden").html("$ 0");
        $("#tipoPersona").html("Persona Natural");
        $("#rucCliente").html("N/A");

        var ordenes = new Array();//ARREGLO DONDE ALMACENO EL ID DE ORDENES
        var clase = $("#seleccion").attr("type");//OBTENGO SI ES CHECKBOX O RADIOBUTTOM

        //RECORRO LA TABLA DONDE EL RADIOBUTTON O CHECKBOX ESTE SELECCIONADO
        $("input[type=" + clase + "]:checked").each(function () {

            //OBTENGO LOS IDS DE LOS CHECKBOX O RADIOBUTTOM SELECCIONADO
            if ($(this).attr("val") != undefined) {
                var data = $(this).attr("val");
                ordenes.push(data);
            }
        });

        //SI NO SE HA SELECCIONADO AL MENOS UNA ORDEN MOSTRAR ERROR
        if (ordenes.length == 0) {
            Alert("Error", "Seleccione al menos una orden", "error");
        } else {
            $.ajax({
                cache: false,
                type: "GET",
                traditional: true,
                url: "/Facturaciones/CargarSeleccionadas/",
                data: { ordenIds: ordenes, clienteId: clienteId },
                success: function (data) {
                    var diplomatico = data.diplomatico;

                    ////LLENAR ENCABEZADO DE CLIENTE Y CARNET DIPLOMATICO
                    //if (data.img !== "N/A" || data.img !== null) {
                    //    diplomatico = true;
                    //}

                    var tipoPersona = diplomatico ? "Persona Diplomático" : "Persona Natural";
                    var agregarDetail = "";
                    var carnet = diplomatico ? data.img : "";

                    ////SI LA PERSONA ES DIPLOMATICA CARGA DATOS
                    //if (diplomatico) {
                    //    tipoPersona = "Persona Diplomático";
                    //    carnet = data.img;
                    //}

                    //CREO EL ENCABEZADO DEL PAGO
                    var encabezado = '<div class="col-sm-4 invoice-col">' +
                        'Cliente' +
                        '<address>' +
                        '<strong id="nombCliente">' + nombre + '</strong>' +
                        '<br> <a id="tipoPersona">' + tipoPersona + '</a>' +
                        '<br id="rucCliente"> RUC: ' + ruc +
                        '</address>' +
                        '</div>' +
                        '<div id="carnetSection" class="col-sm-4 invoice-col pull-right" hidden>' +
                        '<div class="col-md-8 col-sm-2 col-xs-12">' +
                        '<div class="image-upload">' +
                        '<label for="file-input">' +
                        '<img src="' + carnet + '" style="width:250px;height:90px;" />' +
                        '</label>' +
                        '</div>' +
                        '<p style="text-align:center!important; width:250px;">Carnet Diplomático</p>' +
                        '</div>' +
                        '</div>';

                    $("#headMaster").html(encabezado);

                    var subtotalOrd = 0;

                    //CARGAR EL DETALLE DE LAS ORDENES
                    for (var i = 0; i < data.detalleFinal.length; i++) {
                        var cantidad = data.detalleFinal[i].Cantidad;//CANTIDAD PRODUCTO
                        var precioUnitario = formatoPrecio((data.detalleFinal[i].Precio).toString());//P/U PRODUCTO
                        var subtotalProd = cantidad * precioUnitario;//SUBTOTAL PRODUCTO

                        subtotalOrd += subtotalProd;//SUBTOTAL ORDENADO

                        //AGREGAR EL DETALLE DE LA ORDEN
                        agregarDetail += '<tr>' +
                            '<td>' + cantidad + '</td>' +
                            '<td> $ ' + precioUnitario + '</td>' +
                            '<td>' + data.detalleFinal[i].Platillo + '</td>' +
                            '<td> $ ' + formatoPrecio(subtotalProd.toString()) + '</td>' +
                            '</tr>';
                    }

                    $("#bodyDetalle").html(agregarDetail);//AGREGO EL DETALLE DE LAS ORDENES


                    if (diplomatico) {
                        //VERIFICO QUE EL CLIENTE ES DIPLOMATICO PARA MOSTRAR LA SECCION DEL CARNET
                        $("#carnetSection").removeAttr("hidden");
                    } else {
                        $("#tipoPersona").attr("href", "#");
                        $("#tipoPersona").attr("onclick", "cambiarTipo()");
                    }

                    //CALCULO LOS TOTALES
                    //SI EL CLIENTE ES DIPLOMATICO NO COBRAR IVA
                    var IVA = diplomatico ? 0 : subtotalOrd * 0.15;//CALCULO DEL IVA A COBRAR
                    var Total = subtotalOrd + IVA;//TOTAL A PAGAR

                    //AGREGO LOS TOTALES A LA TABLA
                    $("#subTotalOrden").html("$ " + formatoPrecio(subtotalOrd.toString()));
                    $("#ivaOrden").html(IVA == 0 ? "N/A" : "$ " + formatoPrecio(IVA.toString()));
                    $("#totalOrden").html("$ " + formatoPrecio(Total.toString()));

                    CalcularCambios(formatoPrecio(subtotalOrd.toString()), formatoPrecio(IVA.toString()), formatoPrecio(Total.toString()));
                }
            });
        }
    }
}

//CAMBIAR TIPO DE PERSONA A PAGAR
function cambiarTipo() {
    var tipoCliente = $("#clienteId").attr("val");

    //CREAR AL CLIENTE
    CargarParcial("/Facturaciones/ClienteDiplomatico/");
    //CAMBIAR DATOS DONDE VA CLIENTE
    //BUSCAR SI EXISTE CARNET DIPLOMATICO PARA LA FACTURA
    //MOSTRAR LOS CAMPOS OCULTOS
}

//CALCULO Y AGREGO LOS TOTALES EN CORDOBAS Y DOLARES
function CalcularCambios(subtotalOrd, IVA, Total) {
    var dolares = $("#cambio").val();

    //PONER TOTALES AL OTRO LADO DEL PAGO
    $("#subDol").html("$ " + subtotalOrd);
    $("#ivaDol").html("$ " + IVA);
    $("#descDol").html("$ 0");
    $("#propDol").html("$ 0");
    $("#totalDol").html("$ " + Total);

    var subCord = subtotalOrd * dolares;
    var ivaCord = IVA * dolares;
    var totalCord = Total * dolares;

    $("#subCord").html("C$ " + formatoPrecio(subCord.toString()));
    $("#ivaCord").html("C$ " + formatoPrecio(ivaCord.toString()));
    $("#descCord").html("C$ 0");
    $("#propCord").html("C$ 0");
    $("#totalCord").html("C$ " + formatoPrecio(totalCord.toString()));
}

//REALIZA LOS CALCULOS DE DESCUENTO PROPINA Y TOTAL
function agregarVal() {
    var dolares = $("#cambio").val();//TIPO DE CAMBIO DOLAR

    var percentageDescount = $("#descuentoPago").val();//OBTENGO EL % DE DESCUENTO
    var descuento = percentageDescount != "" ? (percentageDescount.split("%")[0] / 100) : 0;//DESCUENTO EN DECIMALES

    var subtotal = parseFloat($("#subDol").html().split("$ ")[1]);//OBTENGO EL SUBTOTAL DEL PAGO
    var iva = parseFloat($("#ivaDol").html().split("$ ")[1]);//OBTENGO EL PORCENTAJE DE IVA

    var descDol = (subtotal + iva) * descuento;//CALCULO EL DESCUENTO EN DOLARES
    var propina = $("#propinaPago").val();//OBTENGO LA PROPINA

    //SI HAY DESCUENTO
    if (descuento != "") {
        $("#descDol").html("$ " + formatoPrecio(descDol.toString()));//PONGO EL DESCUENTO
        $("#txtDesc").html("Descuento (" + percentageDescount + "):");//PONGO EL PORCENTAJE A DESCONTAR
    } else {//SI NO HAY DESCUENTO PONER 0 (VACIO)
        $("#descDol").html("$ 0");
        $("#txtDesc").html("Descuento:");
        descuento = 0;
    }

    var propinaDol = 0;
    var propinaSelected = $("#monedaPropina").find("option:selected").text();

    if (propina != "") {
        //DOLARES
        if (propinaSelected.toUpperCase() === "DÓLARES") {
            $("#propDol").html("$ " + formatoPrecio(propina.toString()));
            //CONVERTIR PROPINA EN CORDOBAS
            propinaDol = propina;
            var convCord = propina * dolares;
            $("#propCord").html("C$ " + formatoPrecio(convCord.toString()));
        } else {//CORDOBAS
            $("#propCord").html("C$ " + formatoPrecio(propina.toString()));
            //CONVERTIR PROPINA EN DOLARES
            propinaDol = dolares * propina;
            var convDol = propina / dolares;
            $("#propDol").html("$ " + formatoPrecio(convDol.toString()));
        }
    } else {
        $("#propDol").html("$ 0");
        $("#propCord").html("C$ 0");

        propina = 0;
    }

    var desc = parseFloat(descDol);
    var prop = parseFloat(propinaDol);

    //var total = ((subtotal + iva) - desc) + prop;

    convertirDesc(desc);
    calcularPagos();

    //LIMPIAR VALORES
    $("#descuentoPago").val("");
    $("#propinaPago").val("");
}

function calcularPagos() {
    var stCord = parseFloat($("#subCord").html().split("C$ ")[1]);
    var stDol = parseFloat($("#subDol").html().split("$ ")[1]);
    var ivaDol = parseFloat($("#ivaDol").html().split("$ ")[1]);
    var ivaCord = parseFloat($("#ivaCord").html().split("C$ ")[1]);
    var propCord = parseFloat($("#propCord").html().split("$ ")[1]);
    var propDol = parseFloat($("#propDol").html().split("$ ")[1]);
    var descCord = parseFloat($("#descCord").html().split("$ ")[1]);
    var descDol = parseFloat($("#descDol").html().split("$ ")[1]);

    var totalCord = ((stCord + ivaCord) - descCord) + propCord;
    var totalDol = ((stDol + ivaDol) - descDol) + propDol;

    $("#totalCord").html("C$ " + formatoPrecio(totalCord.toString()));
    $("#totalDol").html("$ " + formatoPrecio(totalDol.toString()));
}

function convertirDesc(descuento) {
    var dolares = $("#cambio").val();
    var descuentoDol = 0;

    if (descuento != 0) {
        descuentoDol = descuento * dolares;
    }

    $("#descCord").html("C$ " + formatoPrecio(descuentoDol.toString()));
}

//AGREGA LA FORMA DE PAGO
function agregarPago() {
    //OBTENGO LA MONEDA DEL PAGO
    var moneda = $("#monedaPago").find("option:selected").text();
    var pagar = $("#pagar").val().replace(/,/g, "");//OBTENGO EL MONTO A PAGAR

    var totalPagDol = parseFloat($("#footDol").html().split("$ ")[1]);
    var totalPagCord = parseFloat($("#footCord").html().split("C$ ")[1]);

    if (moneda.toUpperCase() == "CÓRDOBAS") {
        var totalCord = parseFloat($("#totalCord").html().split("C$ ")[1]);

        if (totalCord == totalPagCord) {
            Alert("Error", "El pago total esta completo", "error");
        } else if (pagar > totalCord) {
            Alert("Error", "El monto a pagar no debe ser mayor que el total", "error");
        } else {
            validado();
        }
    } else {
        var totalDol = parseFloat($("#totalDol").html().split("$ ")[1]);

        if (totalDol == totalPagCord) {
            Alert("Error", "El pago total esta completo", "error");
        } else if (pagar > totalDol) {
            Alert("Error", "El monto a pagar no debe ser mayor que el total", "error");
        } else {
            validado();
        }
    }
}

function validado() {
    //OBTENGO EL METODO DE PAGO(EFECTIVO-TARJETA)
    var optionSelected = $("#metPago").find("option:selected").text().toUpperCase();
    var agregar = "";
    var recibido = $("#rec").val();//OBTENGO EL MONTO RECIBIDO
    var pagar = $("#pagar").val();//OBTENGO EL MONTO A PAGAR

    //QUITARLE LA COMA A LA VARIABLE PARA CALCULAR BIEN NUMERO CON , EJ 1,200
    var replacePagar = parseFloat(pagar.replace(/,/g, ""));
    var replaceRecibido = parseFloat(recibido.replace(/,/g, ""));

    var entregar = replaceRecibido - replacePagar;

    //OBTENGO EL METODO DE PAGO SELECCIONADO
    var metPago = $("#metPago").find("option:selected").text();
    //OBTENGO LA MONEDA DEL PAGO
    var moneda = $("#monedaPago").find("option:selected").text();
    var digitoMoneda = moneda == "Córdobas" ? "C$ " : "$ ";//PARA AGREGAR EL DIGITO DE PAGO   

    //SI EL MONTO A PAGAR ES MAYOR AL RECIBIDO MANDAR ERROR
    if (parseFloat(pagar) > parseFloat(recibido)) {
        Alert("Error", "El monto a pagar no puede ser mayor que el monto recibido", "error");
    } else {
        if (optionSelected == "EFECTIVO") {
            if (pagar == "" || recibido == "") {
                Alert("Error", "Campos vacíos. Intentelo de nuevo", "error");
            } else {
                agregar = '<tr class="even pointer">' +
                    '<td class="">' + metPago + '</td>' +
                    '<td class="" >' + moneda + '</td>' +
                    '<td class="" >' + digitoMoneda + pagar + '</td>' +
                    '<td class="" >' + digitoMoneda + recibido + '</td>' +
                    '<td class="" >' + digitoMoneda + formatoPrecio(entregar.toString()) + '</td>' +
                    '<td class=" last"><a class="btn btn-primary" id="boton" onclick="editPago(this);"><i class="fa fa-edit"></i></a>' +
                    '<a class="btn btn-danger" onclick = "deletePago(this);" id="boton"> <i class="fa fa-trash"></i></a></td>' +
                    '</tr>';
            }
        } else if (optionSelected == "TARJETA") {
            if (pagar == "") {
                Alert("Error", "Campos vacíos. Intentelo de nuevo", "error");
            } else {
                recibido = pagar;

                agregar = '<tr class="even pointer">' +
                    '<td class="">' + metPago + '</td>' +
                    '<td class="" >' + moneda + '</td>' +
                    '<td class="" >' + digitoMoneda + pagar.toString() + '</td>' +
                    '<td class="" >' + digitoMoneda + recibido.toString() + '</td>' +
                    '<td class="" >' + digitoMoneda + formatoPrecio(entregar.toString()) + '</td>' +
                    '<td class=" last"><a class="btn btn-primary" id="boton" onclick="editPago(this);"><i class="fa fa-edit"></i></a>' +
                    '<a class="btn btn-danger" onclick = "deletePago(this);" id="boton"> <i class="fa fa-trash"></i></a></td>' +
                    '</tr>';
            }
        }
        $("#bodyPagar").append(agregar);
        //LIMPIAR MONTOS Y SELECT
        $("#rec").val("");
        $("#pagar").val("");

        calcularPagosFact();
    }
}

//CALCULO LOS TOTALES DE PAGO
function calcularPagosFact() {
    var totalDol = 0, totalCord = 0, res = 0;
    var dolares = $("#cambio").val();//OBTENGO TIPO DE CAMBIO

    //RECORRO CADA UNA DE LAS FILAS
    $("#bodyPagar tr").each(function () {
        var row = $(this).find("td");//AGARRO LA FILA ACTUAL

        //SI EL PAGO FUE EN CORDOBAS
        if (row.eq(1).html().toUpperCase() == "CÓRDOBAS") {
            //OBTENER EL VALOR EN CORDOBAS
            res = (row.eq(2).html().split("C$ ")[1]).replace(/,/g, "");//QUITARLE LA COMA A LA VARIABLE PARA CALCULAR BIEN NUMERO CON , EJ 1,200

            totalCord += parseFloat(res);//SUMAR LOS CORDOBAS     

            //CONVERSION DE CORDOBAS A DOLARES
            var convDol = res / dolares;
            //SUMARLE EL TOTAL CONVERTIDO AL OTRO LADO
            totalDol += convDol;

        } else {//SI EL PAGO ES EN DOLARES
            //OBTENER EL VALOR EN DOLARES
            res = row.eq(2).html().split("$ ")[1].replace(/,/g, "");
            totalDol += parseFloat(res);//SUMAR LOS DOLARES

            //CONVERSION DE DOLARES A CORDOBAS
            var convCord = res * dolares;
            //SUMARLE EL TOTAL CONVERTIDO AL OTRO LADO
            totalCord += convCord;
        }
    });

    //AGREGAR EL TOTAL EN EL FOOTER
    $("#footDol").html("$ " + formatoPrecio(totalDol.toString()));
    $("#footCord").html("C$ " + formatoPrecio(totalCord.toString()));
}

//FUNCION PARA ELIMINAR UNA FILA SELECCIONADA DE LA TABLA
function deletePago(row) {
    //SE BUSCA LA POSICION DE LA FILA SELECCIONADA PARA ELIMINARLA
    row.closest("tr").remove();

    calcularPagosFact();
}//FIN FUNCTION

function editPago(row) {
    //EVENTO ONCLICK DEL BOTON EDITAR
    $("#tablePagos").on("click", "#boton", function () {
        //OBTENER LOS VALORES A UTILIZAR
        var pago = $(this).parents("tr").find("td").eq(0).html();
        var moneda = $(this).parents("tr").find("td").eq(1).html();
        var pagar = $(this).parents("tr").find("td").eq(2).html();
        var recibido = $(this).parents("tr").find("td").eq(3).html();
        var entregado = $(this).parents("tr").find("td").eq(4).html();

        ////alert(pago + " " + moneda + " " + pagar + " " + recibido + " " + entregado);
        //$("#producto").val(prod);
        //$('#producto').trigger('change'); // Notify any JS components that the value changed

        $("#rec").val(recibido.split("$ ")[1]);
        $("#pagar").val(pagar.split("$ ")[1]);

        deletePago(row);
    });
}

$("#metPago").on("change", function () {
    var metodoPago = $(this).find("option:selected").text();

    if (metodoPago.toUpperCase() == "EFECTIVO") {
        $("#rec").removeAttr("disabled");
    } else {
        $("#rec").attr("disabled", true);
    }

});