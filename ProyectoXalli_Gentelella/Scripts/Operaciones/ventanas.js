//FUNCION PARA CARGAR LA MODAL
function CargarParcial(url) { //RECIBE LA URL DE LA UBICACION DEL METODO

    $("#small-modal").modal("show"); //MUESTRA LA MODAL
    $("#VistaParcial").html("");//LIMPIA LA MODAL POR DATOS PRECARGADOS

    //AGREGAR EL TITULO A LA MODAL
    if (url.includes("Edit")) {
        $("#modal-title").html("Editar");
    } else
        if (url.includes("Create")) {
            $("#modal-title").html("Ingresar nuevo");
        } else
            if (url.includes("Details")) {
                $("#modal-title").html("Detalle");
            }

    $('body').addClass("modal-open");

    $.ajax({
        "type": "GET", //TIPO DE ACCION
        "url": url, //URL DEL METODO A USAR
        success: function (parcial) {
            $("#VistaParcial").html(parcial);//CARGA LA PARCIAL CON ELEMENTOS QUE CONTEGA


            if (url.includes("/Menus/Edit")) {
                //QUITAR LA CLASE QUE DESHABILITA EL BOTON GUARDAR
                $("#finalizar").removeClass("buttonDisabled");
            }
        }//FIN SUCCESS
    });//FIN AJAX
}//FIN FUNCTION

//FUNCION PARA CARGAR LA MODAL
function CargarSmallParcial(url) { //RECIBE LA URL DE LA UBICACION DEL METODO
    $("#smallModal").modal("show"); //MUESTRA LA MODAL
    $("#vParcial").html("");//LIMPIA LA MODAL POR DATOS PRECARGADOS

    //AGREGAR EL TITULO A LA MODAL
    if (url.includes("Edit")) {
        $("#small-modaltitle").html("Editar");
    } else
        if (url.includes("Create")) {
            $("#small-modaltitle").html("Ingresar nuevo");
        } else
            if (url.includes("Details")) {
                $("#small-modaltitle").html("Detalle");
            }

    $('body').addClass("modal-open");

    $.ajax({
        "type": "GET", //TIPO DE ACCION
        "url": url, //URL DEL METODO A USAR
        success: function (parcial) {
            $("#vParcial").html(parcial);//CARGA LA PARCIAL CON ELEMENTOS QUE CONTEGA
        }//FIN SUCCESS
    });//FIN AJAX
}//FIN FUNCTION

//FUNCION PARA CERRAR LA MODAL
function CerrarModal() {
    $("#small-modal").modal("hide"); //CERRAR MODAL                
    $("#smallModal").modal("hide"); //CERRAR MODAL    

    $("body").removeClass("modal-open");
}//FIN FUNCION

//MANDAR EL SWEET ALERT DE ERROR
function Alert(title, message, status) {
    swal({
        title: title,
        text: message,
        icon: status
    });//FIN DEL SWEET ALERT
}//FIN FUNCION

//MANDA EL SWEET ALERT CON TIEMPO
function AlertTimer(title, mensaje, status) {
    swal({
        title: title,
        text: mensaje,
        icon: status,
        buttons: false,
        timer: 1500
    });

    $("body").removeClass("modal-open");
}

//MANDAR EL SWEET ALERT PARA ELIMINAR
function deleteAlert(uri, id) {
    swal({
        title: "¿Desea eliminar el registro?",
        text: "Una vez eliminado no se podrá volver a recuperar",
        icon: "warning",
        buttons: {
            cancel: "Cancelar",
            catch: {
                text: "Eliminar",
                value: "catch"
            }
        }//FIN DE BUTTONS
    })//FIN DEL SWAL

        .then((value) => {
            switch (value) {

                case "catch": Delete(uri, id);
                    break;

                default:
                    swal.close();
            }//FIN SWITCH
        });//FIN THEN
}//FIN FUCTION DELETE

function enviar() {
    var correo = $("#Correo").attr("val");

    if (correo == "N/A") {
        enviarCorreo();
    } else {
        swal({
            title: "¿Desea enviar la comanda a otro correo?",
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
                        //MANDAR CORREO DE UN SOLO
                        ajaxCorreo(correo);
                        break;

                    case "YES":
                        enviarCorreo();
                        break;

                    default:
                        {
                            swal.close();
                        }
                }//FIN SWITCH
            });//FIN THEN
    }
}

function enviarCorreo() {
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
    var orden = $("#info").attr("val");

    $.ajax({
        type: "POST",
        url: "/Ordenes/SendEmail/",
        data: { Email: value, orderId: orden },
        success: function (data) {
            if (data.success) {
                AlertTimer("Completado", data.message, "success");
            } else {
                Alert("Error", data.message, "error");
            }
        }
    });
}

//CAPTURAR LA TECLA SCAPE
jQuery(document).on('keyup', function (evt) {
    if (evt.keyCode == 27) {
        $("body").removeClass("modal-open");
    }
});

//CREA FORMATO PARA LOS PRECIOS CUYO DECIMALES ES 1
function formatoPrecio(precio) {
    var precioFinal = precio;

    //SI EL PRECIO TIENE DECIMALES
    if (precio.includes(".")) {
        var decimales = precio.split(".");

        if (decimales[1].length == 1) {//SI TIENE SOLAMENTE UN DECIMAL
            //AGREGAR UN CERO
            precioFinal = parseInt(decimales[0]) + "." + decimales[1] + "0";
        } else {//SI TIENE MAS DECIMALES
            //CORTAR A DOS DECIMALES
            precioFinal = parseInt(decimales[0]) + "." + decimales[1].substring(0, 2);
        }
    } else {//SI NO TIENE DECIMALES AGREGARLE CEROS
        precioFinal = precio + ".00";
    }

    return precioFinal;
}