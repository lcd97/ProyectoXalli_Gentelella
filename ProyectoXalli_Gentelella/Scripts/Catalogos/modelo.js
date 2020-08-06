//FUNCION PARA HACER EL CRUD A BODEGA POR MEDIO DEL MODAL (RECIBE UN FORM = FORMULARIO)
function SubmitForm(form) {
    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        $.ajax({
            type: "POST", //TIPO DE ACCION
            url: form.action, //ACCION O METODO A REALIZAR
            data: $(form).serialize(), //SERIALIZACION DE LOS DATOS A ENVIAR
            success: function (data) {
                if (data.success) {//SI SE REALIZO CORRECTAMENTE
                    $("#Table").DataTable().ajax.reload(); //RECARGAR DATATABLE PARA VER LOS CAMBIOS
                    $("#small-modal").modal("hide"); //CERRAR MODAL
                    AlertTimer("Completado", data.message, "success");
                }//FIN IF
                else {
                    //MANDAR EL ERROR DEL CONTROLADOR
                    Alert("Error", data.message, "error");
                }
            },//FIN SUCCESS
            error: function () {
                //AQUI MANDAR EL MENSAJE DE ERROR
                Alert("Error al almacenarlo", "Intentelo de nuevo", "error");
            }//FIN ERROR
        });//FIN AJAX
    }//FIN DEL IF FORM VALID
    return false; //EVITA SALIRSE DEL METODO ACTUAL
}//FIN FUNCTION

//FUNCION POST PARA ELIMINAR UN REGISTRO (CRUD) -- SOLO PARA ELIMINAR (RECARGA LA PAGINA CON AJAX -OJO-)
function Delete(uri, id) {

    //COMIENZO DE LA PETICION AJAX PARA ELIMINAR REGISTRO
    $.ajax({
        type: "POST", //TIPO DE ACCION
        url: uri,//ACCION O METODO A REALIZAR
        data: { "id": id }, //SERIALIZACION DE LOS DATOS A ENVIAR
        success: function (data) {
            if (data.success) {//SI SE ELIMINO CORRECTAMENTE
                $("#Table").DataTable().ajax.reload(); //RECARGAR DATATABLE PARA VER LOS CAMBIOS
                AlertTimer(data.message, "El registro ha sido eliminado", "success");
            }//FIN IF
            else
                Alert("Error", data.message, "error");
        },//FIN SUCCESS
        error: function () {
            Alert("Error", "Vuelvalo a intentar", "error");
        }//FIN ERROR
    });//FIN AJAX

    return false;

}//FIN FUNCTION