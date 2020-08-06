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

    $.ajax({
        "type": "GET", //TIPO DE ACCION
        "url": url, //URL DEL METODO A USAR
        success: function (parcial) {
            $("#VistaParcial").html(parcial);//CARGA LA PARCIAL CON ELEMENTOS QUE CONTEGA
        }//FIN SUCCESS
    });//FIN AJAX
}//FIN FUNCTION

//FUNCION PARA CARGAR LA MODAL
function CargarSmallParcial(url) { //RECIBE LA URL DE LA UBICACION DEL METODO
    $("#smallModal").modal("show"); //MUESTRA LA MODAL
    $("#vParcial").html("");//LIMPIA LA MODAL POR DATOS PRECARGADOS

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
}//FIN FUNCION

//MANDAR EL SWEET ALERT DE ERROR
function Alert(title, message, status) {
    swal({
        title: title,
        text: message,
        icon: status
    });//FIN DEL SWEET ALERT
}//FIN FUNCION

function AlertTimer(title, mensaje, status) {
    swal({
        title: title,
        text: mensaje,
        icon: status,
        buttons: false,
        timer: 1500
    });
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