$(document).ready(function () {
    $('#wizard').smartWizard();
    tablaVacio();
    tablaPago();
    cargarNo();
    obtenerCambio();
    metodoPagoCB();
    $('#menu_toggle').click();

    $("#finalizar").attr("onclick", "guardarPago()");//ALMACENAR EL PAGO

    cargarOrden();
});

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

//FUNCION PARA AGREGAR LA FECHA Y NUMERO DE FACTURA
function cargarNo() {
    $.ajax({
        type: "GET",
        url: "/Facturaciones/NumeroFactura/",
        success: function (data) {
            var num = data;//OBTENGO EL NUMERO DE LA FACTURA

            //FORMATEANDO EL CODIGO
            if (data < 10) {
                num = data;
            } else if (data >= 10 || data < 100) {
                num = data;
            } else if (data >= 100 || data < 1000) {
                num = data;
            } else {
                num = data;
            }

            var today = moment().locale('es').format('L');//FECHA FACTURA

            $("#factNo").html("Factura No. " + cargarCodigo(num));

            var agregar = '<small class="pull-right" style="margin-top: 5px;">Fecha: ' + today + '</small>';
            $("#fechaFact").append(agregar);
        }
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

function tablaVacio() {
    var tbody = $("#tableInicio #bodyOrden");//OBTENEMOS EL CONTENIDO DE LA TABLA
    //SI NO HAY DATA
    if (tbody.children().length === 0) {
        var agregar = '<tr class="even pointer" id="noProd"><td colspan="6" style="text-align: center;">SIN REGISTROS DE PAGOS</td></tr>';
        $("#bodyOrden").append(agregar);
    }
}

function tablaPago() {
    var tbody = $("#tablePagos #bodyPagar");//OBTENEMOS EL CONTENIDO DE LA TABLA
    //SI NO HAY DATA
    if (tbody.children().length === 0) {
        var agregar = '<tr class="even pointer" id="noPago"><td colspan="6" style="text-align: center;">SIN REGISTROS DE PAGOS</td></tr>';
        $("#bodyPagar").append(agregar);
    }
}

//VALIDA PASOS PARA MOSTRAR INFORMACION DE DETALLE DE ORDENES
function cargarOrden() {

    var ordenId = $("#ordenId").attr("value");

    $.ajax({
        cache: false,
        type: "GET",
        traditional: true,
        url: "/Facturaciones/CargarOrden/",
        data: { ordenId: ordenId },
        success: function (data) {
            var clienteId = data.cliente.clienteId;//OBTENGO EL ID DEL CLIENTE
            var nombre = data.cliente.nombre != "" ? data.cliente.nombre : "Visitante";
            var ruc = data.cliente.ruc != "" ? data.cliente.ruc : "N/A";

            var diplomatico = data.diplomatico;
            var tipoPersona = diplomatico ? "Persona Diplomático" : "Persona Natural";
            var agregarDetail = "";
            var carnet = diplomatico ? data.img.Ruta : "";

            //CREO EL ENCABEZADO DEL PAGO
            var encabezado = '<div class="col-sm-4 invoice-col">' +
                'Cliente' +
                '<address>' +
                '<strong id="nombCliente" val="' + clienteId + '">' + nombre + '</strong>' +
                '<br> <a id="tipoPersona" val="' + diplomatico + '">' + tipoPersona + '</a>' +
                '<br><p id="rucCliente"> RUC: ' + ruc + '</p>' +
                '</address>' +
                '</div>' +
                '<div id="carnetSection" class="col-sm-4 invoice-col pull-right" hidden>' +
                '<div class="col-md-8 col-sm-2 col-xs-12">' +
                '<div class="image-upload">' +
                '<label for="file-input">' +
                '<img id="carnet" val="' + data.img.Id + '" src="' + carnet + '" style="width:250px;height:90px;" />' +
                '</label>' +
                '</div>' +
                '<p style="text-align:center!important; width:250px;">Carnet Diplomático</p>' +
                '</div>' +
                '</div>';

            $("#headMaster").html(encabezado);

            var subtotalOrd = 0;

            //CARGAR EL DETALLE DE LAS ORDENES
            for (var i = 0; i < data.ordCliente.length; i++) {
                var cantidad = data.ordCliente[i].Cantidad;//CANTIDAD PRODUCTO
                var precioUnitario = parseFloat(data.ordCliente[i].Precio).toFixed(2);//P/U PRODUCTO
                var subtotalProd = parseFloat(cantidad * parseFloat(precioUnitario).toFixed(2)).toFixed(2);//SUBTOTAL PRODUCTO

                subtotalOrd += parseFloat(subtotalProd).toFixed(2);//SUBTOTAL ORDENADO

                //AGREGAR EL DETALLE DE LA ORDEN
                agregarDetail += '<tr>' +
                    '<td>' + cantidad + '</td>' +
                    '<td>$ ' + precioUnitario + '</td>' +
                    '<td>' + data.ordCliente[i].Platillo + '</td>' +
                    '<td>$ ' + subtotalProd + '</td>' +
                    '</tr>';
            }

            $("#bodyDetalle").html(agregarDetail);//AGREGO EL DETALLE DE LAS ORDENES                                       

            totalesOrdenes(diplomatico);

            if (diplomatico) {
                //VERIFICO QUE EL CLIENTE ES DIPLOMATICO PARA MOSTRAR LA SECCION DEL CARNET
                $("#carnetSection").removeAttr("hidden");
                //$("#tipoPersona").attr("href", "#");
                //$("#tipoPersona").attr("onclick", "cambiarTipo()");

                //MOSTRAR PANTALLA PARA AGREGAR CAMPOS DE DIPLOMATICO
                cambiarTipo(clienteId);
            }//FIN IF DIPLOMATICO
        }
    });
}//FIN FUNCTION

//CAMBIAR TIPO DE PERSONA A PAGAR
function cambiarTipo(clienteId) {
    //PONER NOMBRE A LA MODAL
    $("#modal-title").html("Cliente Diplomático");

    $("#small-modal").modal("show"); //MUESTRA LA MODAL
    $("#VistaParcial").html("");//LIMPIA LA MODAL POR DATOS PRECARGADOS

    $('body').addClass("modal-open");

    $.ajax({
        type: "GET", //TIPO DE ACCION
        url: "/Facturaciones/ClienteDiplomatico/", //URL DEL METODO A USAR
        data: { clienteId: clienteId },
        success: function (parcial) {
            $("#VistaParcial").html(parcial);//CARGA LA PARCIAL CON ELEMENTOS QUE CONTEGA

        }//FIN SUCCESS
    });//FIN AJAX
}//FIN FUNCTION

//OBTENGO EL TIPO DE CAMBIO DEL DIA SEGUN EL BCN
function obtenerCambio() {
    $.ajax({
        type: "GET",
        url: "/Facturaciones/CalcularCambioHoy/",
        success: function (data) {
            $("#cambio").val(data.toFixed(2));
        }
    });
}

//CALCULO LOS TOTALES DE LA TABLA DE ORDENES
function totalesOrdenes(diplomatico) {
    var totalOrd = 0;

    //RECORRER LA TABLA PARA SUMAR TODOS LOS TOTALES DE PRODUCTOS
    $("#bodyDetalle tr").each(function () {
        var str = $(this).find("td").eq(3).html();//PRECIO TOTAL PRODUCTO
        var res = str.split("$ ")[1];
        totalOrd += parseFloat(res);
    });

    //SI EL CLIENTE ES DIPLOMATICO NO COBRAR IVA
    var IVA = diplomatico ? 0 : (totalOrd * 0.15).toFixed(2);//CALCULO DEL IVA A COBRAR
    var Total = parseFloat(totalOrd + parseFloat(IVA)).toFixed(2);//TOTAL A PAGAR

    //AGREGO LOS TOTALES A LA TABLA
    $("#subTotalOrden").html("$ " + parseFloat(totalOrd).toFixed(2));
    $("#ivaOrden").html(IVA == 0 ? "N/A" : "$ " + parseFloat(IVA).toFixed(2));
    $("#totalOrden").html("$ " + parseFloat(Total).toFixed(2));
    $("#restoDol").html("Faltan: $ " + parseFloat(Total).toFixed(2));

    CalcularCambios(totalOrd, IVA, Total);
}

//CALCULO Y AGREGO LOS TOTALES EN CORDOBAS Y DOLARES
function CalcularCambios(subtotalOrd, IVA, Total) {
    var dolares = $("#cambio").val();

    //var subCord = subtotalOrd * dolares;
    //var ivaCord = IVA * dolares;
    var totalCord = Total * dolares;

    //PONER TOTALES AL OTRO LADO DEL PAGO
    $("#subDol").html("$ " + parseFloat(subtotalOrd).toFixed(2));
    $("#ivaDol").html("$ " + parseFloat(IVA).toFixed(2));
    $("#descDol").html("$ 0");
    $("#propDol").html("$ 0");
    $("#totalDol").html("$ " + parseFloat(Total).toFixed(2));

    //$("#subCord").html("C$ " + subCord.toFixed(2));
    //$("#ivaCord").html("C$ " + ivaCord.toFixed(2));
    //$("#descCord").html("C$ 0");
    //$("#propCord").html("C$ 0");
    $("#totalCord").html("C$ " + totalCord.toFixed(2));
    $("#restoCord").html("Faltan: C$ " + totalCord.toFixed(2));
}

//REALIZA LOS CALCULOS DE DESCUENTO PROPINA Y TOTAL
function agregarVal() {
    var dolares = $("#cambio").val();//TIPO DE CAMBIO DOLAR

    var percentageDescount = $("#descuentoPago").val();//OBTENGO EL % DE DESCUENTO
    var descuento = percentageDescount != "" ? parseFloat(percentageDescount.split("%")[0] / 100).toFixed(2) : 0;//DESCUENTO EN DECIMALES

    var subtotal = parseFloat($("#subDol").html().split("$ ")[1]).toFixed(2);//OBTENGO EL SUBTOTAL DEL PAGO
    var iva = parseFloat($("#ivaDol").html().split("$ ")[1]).toFixed(2);//OBTENGO EL PORCENTAJE DE IVA

    var descDol = ((parseFloat(subtotal) + parseFloat(iva)) * parseFloat(descuento)).toFixed(2);//CALCULO EL DESCUENTO EN DOLARES
    var propina = $("#propinaPago").val();//OBTENGO LA PROPINA

    //SI HAY DESCUENTO
    if (descuento != "") {
        $("#descDol").html("$ " + descDol);//PONGO EL DESCUENTO
        $("#txtDesc").html("Descuento (" + percentageDescount + "%):");//PONGO EL PORCENTAJE A DESCONTAR
        $("#txtDesc").attr("val", percentageDescount.split("%")[0]);//EL PORCENTAJE A DESCONTAR
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
            $("#propDol").html("$ " + parseFloat(propina).toFixed(2));
            $("#propina").attr("name", "propDol");
            //CONVERTIR PROPINA EN CORDOBAS
            propinaDol = parseFloat(propina).toFixed(2);
            var convCord = (propina * dolares).toFixed(2);
            $("#propCord").html("C$ " + parseFloat(convCord).toFixed(2));
        } else {//CORDOBAS
            $("#propCord").html("C$ " + parseFloat(propina).toFixed(2));
            $("#propCord").attr("name", "propCord");
            //CONVERTIR PROPINA EN DOLARES
            propinaDol = dolares * propina;
            var convDol = (propina / dolares).toFixed(2);
            $("#propDol").html("$ " + parseFloat(convDol).toFixed(2));
        }
    } else {
        $("#propDol").html("$ 0");
        $("#propCord").html("C$ 0");

        propina = 0;
    }

    var desc = parseFloat(descDol);
    //var prop = parseFloat(propinaDol);

    //var total = ((subtotal + iva) - desc) + prop;

    convertirDesc(desc);
    calcularPagos();

    //LIMPIAR VALORES
    $("#descuentoPago").val("");
    $("#propinaPago").val("");
}

//FUNCION PARA CONVERTIR DE DESCUENTO
function convertirDesc(descuento) {
    var dolares = $("#cambio").val();
    var descuentoDol = 0;

    if (descuento != 0) {
        descuentoDol = descuento * dolares;
    }

    $("#descCord").html("C$ " + formatoPrecio(descuentoDol.toString()));
}

//FUNCION PARA CALCULAR EL TOTAL DE PAGOS (FOOTER)
function calcularPagos() {
    var dolares = $("#cambio").val();//TIPO DE CAMBIO DOLAR

    //var stCord = parseFloat($("#subCord").html().split("C$ ")[1]);
    var stDol = parseFloat($("#subDol").html().split("$ ")[1]);
    var ivaDol = parseFloat($("#ivaDol").html().split("$ ")[1]);
    //var ivaCord = parseFloat($("#ivaCord").html().split("C$ ")[1]);
    //var propCord = parseFloat($("#propCord").html().split("$ ")[1]);
    var propDol = parseFloat($("#propDol").html().split("$ ")[1]);
    //var descCord = parseFloat($("#descCord").html().split("$ ")[1]);
    var descDol = parseFloat($("#descDol").html().split("$ ")[1]);

    var totalDol = ((stDol + ivaDol) - descDol) + propDol;
    var totalCord = totalDol.toFixed(2) * parseFloat(dolares);

    $("#totalCord").html("C$ " + totalCord.toFixed(2));
    $("#totalDol").html("$ " + totalDol.toFixed(2));

    //$("#restoCord").html("Faltan: C$ " + totalCord.toFixed(2));
    //$("#restoCord").attr("name", totalCord.toFixed(2));
    //$("#restoDol").html("Faltan: $ " + totalDol.toFixed(2));
    //$("#restoDol").attr("name", totalDol.toFixed(2));

    calcularResto();
}

function calcularResto() {
    var totalDol = $("#totalDol").html().split("$ ")[1];
    var totalCord = $("#totalCord").html().split("C$ ")[1];

    var totalPagDol = $("#footDol").html().split("$ ")[1];
    var totalPagCord = $("#footCord").html().split("C$ ")[1];

    var restoCord = parseFloat(totalCord) - parseFloat(totalPagCord);
    var restoDol = parseFloat(totalDol) - parseFloat(totalPagDol);

    $("#restoCord").html("Faltan: C$ " + restoCord.toFixed(2));
    $("#restoCord").attr("name", restoCord.toFixed(2));
    $("#restoDol").html("Faltan: $ " + restoDol.toFixed(2));
    $("#restoDol").attr("name", restoDol.toFixed(2));
}

//AGREGA LA FORMA DE PAGO
function agregarPago() {
    //OBTENGO LA MONEDA DEL PAGO
    var moneda = $("#monedaPago").find("option:selected").text();
    var pagar = $("#pagar").val().replace(/,/g, "");//OBTENGO EL MONTO A PAGAR

    var totalPagDol = parseFloat($("#footDol").html().split("$ ")[1]);
    var totalPagCord = parseFloat($("#footCord").html().split("C$ ")[1]);

    var metodoPago = $("#metPago").find("option:selected").text().toUpperCase();

    if (metodoPago == "TARJETA") {
        filaPago();
    } else if (moneda.toUpperCase() == "CÓRDOBAS") {
        var totalCord = parseFloat($("#totalCord").html().split("C$ ")[1]);
        var totalC = $("#totalCord").html();

        var excedeC = parseFloat(pagar) + parseFloat(totalPagCord);

        if (excedeC > totalC) {
            Alert("Error", "El total a pagar excede el total", "error");
        } else if (totalCord == totalPagCord) {
            Alert("Error", "El pago total esta completo", "error");
        } else if (pagar > totalCord) {
            Alert("Error", "El monto a pagar no debe ser mayor que el total", "error");
        } else {
            filaPago();
        }
    } else {
        var totalDol = parseFloat($("#totalDol").html().split("$ ")[1]);
        var totalD = $("#totalCord").html();

        var excedeD = parseFloat(pagar) + parseFloat(totalPagCord);

        if (excedeD > totalD) {
            Alert("Error", "El total a pagar excede el total", "error");
        }
        if (totalDol == totalPagDol) {
            Alert("Error", "El pago total esta completo", "error");
        } else if (pagar > totalDol) {
            Alert("Error", "El monto a pagar no debe ser mayor que el total", "error");
        } else {
            filaPago();
        }
    }
}

//FUNCION PARA AGREGAR LA FORMA DE PAGO
function filaPago() {
    //OBTENGO EL METODO DE PAGO(EFECTIVO-TARJETA)
    var optionSelected = $("#metPago").find("option:selected").val();
    var agregar = "";
    var recibido = $("#rec").val();//OBTENGO EL MONTO RECIBIDO
    var pagar = $("#pagar").val();//OBTENGO EL MONTO A PAGAR

    //QUITARLE LA COMA A LA VARIABLE PARA CALCULAR BIEN NUMERO CON , EJ 1,200
    var replacePagar = parseFloat(pagar.replace(/,/g, ""));
    var replaceRecibido = parseFloat(recibido.replace(/,/g, ""));

    var entregar = replacePagar;

    //OBTENGO EL METODO DE PAGO SELECCIONADO
    var metPago = $("#metPago").find("option:selected").text();
    //OBTENGO LA MONEDA DEL PAGO
    var moneda = $("#monedaPago").find("option:selected").text();
    var monedaOption = $("#monedaPago").find("option:selected").val();
    var digitoMoneda = moneda == "Córdobas" ? "C$ " : "$ ";//PARA AGREGAR EL DIGITO DE PAGO   

    //SI EL MONTO A PAGAR ES MAYOR AL RECIBIDO MANDAR ERROR
    if (parseFloat(replacePagar) > parseFloat(replaceRecibido)) {
        Alert("Error", "El monto a pagar no puede ser mayor que el monto recibido", "error");
    } else {
        if (metPago.toUpperCase() == "EFECTIVO") {
            if (pagar == "" || recibido == "") {
                Alert("Error", "Campos vacíos. Intentelo de nuevo", "error");
            } else {
                entregar = replaceRecibido.toFixed(2) - replacePagar.toFixed(2);

                agregar = '<tr class="even pointer">' +
                    '<td class="" val="' + optionSelected + '">' + metPago + '</td>' +
                    '<td class="" val="' + monedaOption + '">' + moneda + '</td>' +
                    '<td class="" >' + digitoMoneda + parseFloat(replacePagar).toFixed(2) + '</td>' +
                    '<td class="" >' + digitoMoneda + parseFloat(replaceRecibido).toFixed(2) + '</td>' +
                    '<td class="" >' + digitoMoneda + parseFloat(entregar).toFixed(2) + '</td>' +
                    '<td class=" last"><a class="btn btn-primary" id="boton" onclick="editPago(this);"><i class="fa fa-edit"></i></a>' +
                    '<a class="btn btn-danger" onclick = "deletePago(this);" id="boton"> <i class="fa fa-trash"></i></a></td>' +
                    '</tr>';
            }
        } else if (metPago.toUpperCase() == "TARJETA") {
            if (pagar == "") {
                Alert("Error", "Campos vacíos. Intentelo de nuevo", "error");
            } else {
                recibido = replacePagar;
                entregar = 0;

                agregar = '<tr class="even pointer">' +
                    '<td class="" val="' + optionSelected + '">' + metPago + '</td>' +
                    '<td class="" val="' + monedaOption + '">' + moneda + '</td>' +
                    '<td class="" >' + digitoMoneda + parseFloat(replacePagar).toFixed(2) + '</td>' +
                    '<td class="" >' + digitoMoneda + parseFloat(recibido).toFixed(2) + '</td>' +
                    '<td class="" >' + digitoMoneda + parseFloat(entregar).toFixed(2) + '</td>' +
                    '<td class=" last"><a class="btn btn-primary" id="boton" onclick="editPago(this);"><i class="fa fa-edit"></i></a>' +
                    '<a class="btn btn-danger" onclick = "deletePago(this);" id="boton"> <i class="fa fa-trash"></i></a></td>' +
                    '</tr>';
            }
        }
        $("#bodyPagar").append(agregar);
        $("#noPago").remove();

        //LIMPIAR MONTOS Y SELECT
        $("#rec").val("");
        $("#pagar").val("");

        calcularPagosFact();
        tablaPago();
        calcularResto();
    }
}

//CALCULO LOS TOTALES DE PAGO
function calcularPagosFact() {
    var totalDol = 0.0, totalCord = 0.0, res = 0.0;
    var dolares = $("#cambio").val();//OBTENGO TIPO DE CAMBIO

    //RECORRO CADA UNA DE LAS FILAS
    $("#bodyPagar tr").each(function () {
        var row = $(this).find("td");//AGARRO LA FILA ACTUAL

        //SI EL PAGO FUE EN CORDOBAS
        if (row.eq(1).html().toUpperCase() == "CÓRDOBAS") {
            //OBTENER EL VALOR EN CORDOBAS
            res = parseFloat(row.eq(2).html().split("C$ ")[1].replace(/,/g, ""));//
            totalCord += res;//SUMAR LOS CORDOBAS   

            //alert(typeof (res));

            //CONVERSION DE CORDOBAS A DOLARES
            var convDol = parseFloat(parseFloat(res) / dolares);
            //SUMARLE EL TOTAL CONVERTIDO AL OTRO LADO
            totalDol += parseFloat(convDol);

        } else {//SI EL PAGO ES EN DOLARES
            //OBTENER EL VALOR EN DOLARES
            res = parseFloat(row.eq(2).html().split("$ ")[1].replace(/,/g, ""));

            totalDol += res;//SUMAR LOS DOLARES

            //alert(typeof (res));

            //CONVERSION DE DOLARES A CORDOBAS
            var convCord = parseFloat((res) * dolares);
            //SUMARLE EL TOTAL CONVERTIDO AL OTRO LADO
            totalCord += parseFloat(convCord);
        }
    });

    //AGREGAR EL TOTAL EN EL FOOTER
    $("#footDol").html("$ " + parseFloat(totalDol).toFixed(2));
    $("#footCord").html("C$ " + parseFloat(totalCord).toFixed(2));
}


//FUNCION ON CHANGE DEL METODO DE PAGO
$("#metPago").on("change", function () {
    metodoPagoCB();
});

function metodoPagoCB() {
    $("#pagar").val("");
    $("#rec").val("");

    var metodoPago = $("#metPago").find("option:selected").text();

    if (metodoPago.toUpperCase() == "EFECTIVO") {
        $("#rec").removeAttr("disabled");
    } else {
        $("#rec").attr("disabled", true);
    }
}

//FUNCION PARA ALMACENAR EL PAGO DE LA/LAS ORDEN(ES)
function guardarPago() {
    /*List<int> OrdenesIds, int ClienteId, int NoFactura, double FechaPago, bool Diplomatico, int DescuentoPago,
    double Propina, double Cambio, int MonedaPropina, int EvidenciaId, string DetallePago*/

    //var pagoDol = parseFloat($("#footDol").html().split("$ ")[1]);
    //var pagoCord = parseFloat($("#footCord").html().split("C$ ")[1]);

    //var calcDol = parseFloat($("#totalDol").html().split("$ ")[1]);
    //var calcCord = parseFloat($("#totalCord").html().split("C$ ")[1]);

    var restoCord = parseFloat($("#restoCord").attr("name"));
    var restoDol = parseFloat($("#restoDol").attr("name"));

    //if ((pagoDol == 0 || pagoDol != calcDol) && (pagoCord == 0 || pagoCord != calcCord)) {
    if (restoCord != 0 && restoDol != 0) {//SI FALTA PAGAR O ESTA EN NEGATIVO
        Alert("Error", "Complete correctamente la forma de pago de la factura", "error");
    } else {
        //OBTENER LOS PARAMETROS
        var ordenesIds = $("#ordenId").attr("value")//LISTA DE ORDENES IDS
        var clienteId = $("#nombCliente").attr("val");//CLIENTE ID
        var noFactura = ($("#factNo").html()).substr(12);//NUMERO DE FACTURA
        var fechaPago = moment().locale('es').format('L');
        var diplomatico = $("#tipoPersona").attr("val") == "true" ? true : false;//ES DIPLOMATICO
        var descuento = ($("#txtDesc").attr("val")) != undefined ? ($("#txtDesc").attr("val")) : 0;//PORCENTAJE DE DESCUENTO
        var propina = $("#propDol").html() == "" ? 0 : $("#propDol").html().split("$ ")[1];

        //var seleccionar = $("#propina").attr("name");//SI LA PROPINA FUE EN DOL O CORD

        ////SI LA PROPINA LA INGRESARON EN DOLARES
        //if (seleccionar == "propDol") {
        //    propina = $("#propDol").html().split("$ ")[1];//AGARRAR LA PROPINA EN DOLARES
        //} else {//SI LA PROPINA LA INGRESARON EN DOLARES
        //    propina = $("#propCord").html().split("C$ ")[1];//AGARRAR LA PROPINA EN CORDOBAS
        //}

        var tipoCambio = $("#cambio").val();//TIPO DE CAMBIO
        var monedaPropina = $("#monedaPropina").val();//TIPO DE MONEDA DE LA PROPINA
        var imagen = $("#carnet").attr("val");//AGARRA EL ID DE LA EVIDENCIA

        //RECORRER LA TABLA DE PAGO
        var detallePago = new Array();

        $("#bodyPagar tr").each(function () {

            var row = $(this);
            var item = {};

            item["Id"] = 0;

            var moneda = row.find("td").eq(1).html();
            var pagar = row.find("td").eq(2).html();
            var recibido = row.find("td").eq(3).html();

            if (moneda == "CÓRDOBAS") {
                item["CantidadPagar"] = pagar.split("C$ ")[1];
                item["MontoRecibido"] = recibido.split("C$ ")[1];
            } else {
                item["CantidadPagar"] = pagar.split("$ ")[1];
                item["MontoRecibido"] = recibido.split("$ ")[1];
            }

            item["TipoPagoId"] = row.find("td").eq(0).attr("val");
            item["PagoId"] = 0;
            item["MonedaId"] = row.find("td").eq(1).attr("val");

            detallePago.push(item);
        });

        //alert("ordenes id: " + ordenesIds + " clienteId " + clienteId + " NoFact " + noFactura + " fechaPago " + fechaPago + " diplomatico " + diplomatico + " descuento " + descuento + " propina " + propina);
        //alert("tipo cambio " + tipoCambio + " monedapropina " + monedaPropina + " imagen " + imagen);
        //alert(JSON.stringify(detallePago));

        $.ajax({
            type: "POST",
            url: "/Facturaciones/Create/",
            data: {
                OrdenesIds: ordenesIds, ClienteId: clienteId, NoFactura: noFactura, FechaPago: fechaPago, Diplomatico: diplomatico, DescuentoPago: descuento,
                Propina: propina, Cambio: tipoCambio, MonedaPropina: monedaPropina, EvidenciaId: imagen, DetallePago: JSON.stringify(detallePago)
            },
            success: function (data) {
                if (data.success) {
                    var url = "/Home/Index/?mensaje=" + data.message + "&ordenId=" + data.ordenId;
                    window.location.href = url;
                } else {
                    Alert("Error", data.message, "error");
                }
            }
        });
    }
}