$(document).ready(function () {
    $('#wizard').smartWizard();
    $(".buttonNext").addClass("buttonDisabled");//DESACTIVAR EL BOTON SIGUIENTE

    $(".buttonNext").attr("onclick", "cargarDetOrden()");//DESACTIVAR EL BOTON SIGUIENTE
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
                //var chk = document.getElementById("celdaChk");
                //chk.style.removeAttribute = "display";
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
                        '<input id="seleccion" type="' + clase + '" class="flat" name="table_records">' +
                        '</td>' +
                        '<td class=" " val="' + data.orden[i].OrdenId + '">' + cargarCodigo(data.orden[i].CodigoOrden) + '</td>' +
                        '<td class=" ">' + data.orden[i].FechaOrden + '</td>' +
                        '<td class=" ">' + data.orden[i].Cliente + '</td>' +
                        '<td class=" ">' + data.orden[i].Mesero + '</td>' +
                        '<td class="a-right a-right ">$ ' + formatoPrecio(data.orden[i].SubTotal) + '</td>' +
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

function cargarDetOrden() {
    var ordenes = new Array();
    var clase = $("#seleccion").attr("type");

    $("input[type=" + clase + "]:checked").each(function () {
        var data = $(this).parent().parent().parent().find('td').eq(1).attr("val");
        ordenes.push(data);
    });

    $.ajax({
        type: "GET",
        url: "/Facturaciones/CargarSeleccionadas",
        data: { OrdenesId: ordenes },
        success: function (data) {

        }
    });
}

function cambiarTipo() {
    var tipoCliente = $("#clienteId").attr("val");

    //CREAR AL CLIENTECargarParcial("/Clientes/Cre");
    //CAMBIAR DATOS DONDE VA CLIENTE
    //BUSCAR SI EXISTE CARNET DIPLOMATICO PARA LA FACTURA
    //MOSTRAR LOS CAMPOS OCULTOS
}