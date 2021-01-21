$(document).ready(function () {
    limpiarComandas();
    $("codeOrden").val("");
});

function enviarEmail() {
    swal("Ingrese el correo electrónico del cliente", {
        content: "input"
    })
        .then((value) => {
            if (value == "") {
                Alert("Error", "No ingresó ningún correo a enviar", "error");
            } else {
                ajaxCorreo(value);
            }
        });
}

function ajaxCorreo(value) {
    var orden = parseInt($("#codeOrden").val());

    $.ajax({
        type: "POST",
        url: "/Comandas/SendEmail/",
        data: { Email: value, codeOrder: orden },
        success: function (data) {
            if (data.success) {
                AlertTimer("Completado", data.message, "success");
            } else {
                Alert("Error", data.message, "error");
            }
        }
    });
}

//ACTIVAMOS/DESACTIVAMOS ACCIONES
$("#codeOrden").blur(function () {
    var codigo = $(this).val().trim();

    if (codigo == "") {
        //LIMPIAMOS PANTALLA Y DESACTIVAMOS BOTONES
        limpiarComandas();
    } else {
        $("#buscarImg").removeAttr("disabled");
        $("#impComanda").removeAttr("disabled");
        $("#emailComanda").removeAttr("disabled");
    }
});

//LIMPIAMOS PANTALLA
function limpiarComandas() {
    $("#imgComanda").attr("src", "");
    $("#impComanda").attr("disabled", true);
    $("#emailComanda").attr("disabled", true);
}

//BUSCAR IMAGEN COMANDA
function BuscarImg() {
    var code = $("#codeOrden").val().trim();

    if (code == "" || code <= 0 || !$.isNumeric(code)) {
        Alert("Error", "Ingrese un código válido de orden", "error");
    } else {
        $.ajax({
            type: "GET",
            url: "/Comandas/getComanda",
            data: { codeOrder: code },
            success: function (data) {
                if (data.existe) {
                    $("#imgComanda").attr("src", data.ruta);
                } else {
                    Alert("Atención", "No existe comanda para esta orden", "info");
                    $("#impComanda").attr("disabled", true);
                    $("#emailComanda").attr("disabled", true);
                }


            }
        });
    }
}//FIN FUNCTION