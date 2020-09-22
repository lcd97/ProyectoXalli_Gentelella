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

//function myfunction() {
//    swal({
//        title: "",
//        text: "Ingrese el correo del cliente:",
//        type: "input",
//        showCancelButton: true,
//        closeOnConfirm: false,
//        inputPlaceholder: "example@hotmail.com"
//    }, function (inputValue) {
//        if (inputValue === false) return false;
//        if (inputValue === "") {
//            swal.showInputError("You need to write something!");
//            return false;
//        }
//        swal("Nice!", "You wrote: " + inputValue, "success");
//    });
////}

//CAPTURAR LA TECLA SCAPE
jQuery(document).on('keyup', function (evt) {
    if (evt.keyCode == 27) {
        $("body").removeClass("modal-open");
    }
});
