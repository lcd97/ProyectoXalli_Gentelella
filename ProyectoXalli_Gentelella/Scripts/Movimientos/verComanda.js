$(document).ready(function () {
    var orderId = $("#comandaId").attr("name");

    $.ajax({
        type: "GET",
        url: "/Ordenes/getOrderAndDetails/",
        data: { orderId },
        success: function (data) {
            $("#fechaComanda").html("Fecha: " + data.Principal.Fecha);
            agregarInfo = '<b>Información</b><br><br><b>Código de orden:</b> ' + cargarCodigo(data.Principal.Codigo) + '<br><b>Xalli Ometepe Beach Hotel</b><br><b>Restaurant & Lounge Bar</b>';
            $("#info").append(agregarInfo);
            $("#info").attr("val", data.Principal.Id);
            $("#Mesero").html(data.Principal.Mesero);

            var cliente = "Visitante", ruc = "N/A", correo = "N/A", telefono = "N/A";

            if (data.Principal.TipoCliente == 1) {
                cliente = data.Principal.Nombres;
                ruc = data.Principal.RUC != null ? data.Principal.RUC : "N/A";
                correo = data.Principal.Correo != null ? data.Principal.Correo : "N/A";
                telefono = data.Principal.Telefono != null ? data.Principal.Telefono : "N/A";
            }

            agregarCustomer = 'Cliente <address>' +
                '<strong id="Cliente" val="' + data.Principal.ClienteId + '">' + cliente + '</strong>' +
                '<br id="RUC">RUC:  ' + ruc +
                '<br id="Telefono">Teléfono: ' + telefono +
                '<br id="Correo" val="' + correo + '">Correo: ' + correo +
                '</address>';

            $("#Customer").append(agregarCustomer);

            var agregarDetail = {};

            for (var i = 0; i < Object.keys(data.Details).length; i++) {
                var subtotal = data.Details[i].Cantidad * data.Details[i].PrecioUnitario;

                //AGREGAR EL DETALLE DE LA ORDEN
                agregarDetail += '<tr>' +
                    '<td>' + data.Details[i].Cantidad + '</td>' +
                    '<td> $ ' + data.Details[i].PrecioUnitario.toFixed(2) + '</td>' +
                    '<td>' + data.Details[i].Platillo + '</td>' +
                    '<td> $ ' + subtotal.toFixed(2) + '</td>' +
                    '</tr>';
            }

            $("#tbComanda").append(agregarDetail);
            CalcularTotales();
        }
    });
});

//CALCULA EL TOTAL DE LA TABLA
function CalcularTotales() {
    var subTotal = 0;

    //RECORRER LA TABLA PARA SUMAR TODOS LOS TOTALES DE PRODUCTOS
    $("#tbComanda tr").each(function () {
        var str = $(this).find("td").eq(3).html();
        var res = str.split("$");
        subTotal += parseFloat(res[1]);
    });

    //CALCULAR EL IVA
    var iva = subTotal * 0.15;
    var totalFinal = subTotal + iva;

    $("#subTotal").html("$ " + subTotal.toFixed(2));
    $("#IVA").html("$ " + iva.toFixed(2));
    $("#Total").html("$ " + totalFinal.toFixed(2));
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

function facturar() {
    swal({
        title: "¿Desea cerrar la orden para facturar?",
        icon: "warning",
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
                    cerrarOrden();
                    break;

                default:
                    {
                        swal.close();
                    }
            }//FIN SWITCH
        });//FIN THEN
}

//FUNCION PARA CERRAR LA ORDEN DE LA COMANDA
function cerrarOrden() {
    var orden = $("#info").attr("val");

    $.ajax({
        type: "POST",
        url: "/Ordenes/CerrarComanda/",
        data: { ordenId: orden },
        success: function (data) {
            if (data.success) {
                //REDIRECCIONA A LA VISTA FACTURAR
                var url = "/Facturaciones/Index/";
                window.location.href = url;
            } else {
                Alert("Error", data.message, "error");
            }
        }
    });
}