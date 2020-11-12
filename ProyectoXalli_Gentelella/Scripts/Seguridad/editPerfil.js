$(document).ready(function () {

    //BUSCAR EL ROL DEL EMPLEADO LOGGEADO
    var loginId = $("#session").val();

    $.ajax({
        type: "GET",
        url: "/Account/userProfileData/",
        data: { empleado: loginId },
        success: function (data) {
            if (data.dataProfile == null) {
                Alert("Error", "Inicie sesión con credenciales válidas", "error");
            } else {
                var nombre = data.dataProfile.Nombre;
                var apellido = data.dataProfile.Apellido;

                $("#cedula").val(data.dataProfile.Cedula);
                $("#cedula").attr("val", data.dataProfile.ColaboradorId);
                $("#inss").val(data.dataProfile.INSS);
                $("#ruc").val(data.dataProfile.RUC);
                $("#nombre").val(nombre);
                $("#apellido").val(apellido);
                $("#correo").val(data.colaborador.Correo);
                $("#entrada").val(data.dataProfile.Entrada);
                $("#salida").val(data.dataProfile.Salida);
                $("#userName").val(data.colaborador.UserName);

                var general = '<h2 class="title" id="nombreCol" style="text-align: center;">' + nombre + ' ' + apellido + '</h2>' +
                    '<p style="margin-top:-5px !important;text-align: center;" id="roleId" val="' + data.colaborador.UserId + '"><i class="fa fa-circle green"></i>&nbsp; ' + data.colaborador.Role + '</p>';

                $("#generalData").append(general);
            }
        }
    });
});

function editProfile() {
    var colaboradorId = $("#cedula").attr("val");
    var roleId = $("#roleId").attr("val");
    var nombre = $("#nombre").val();
    var apellido = $("#apellido").val();
    var correo = $("#correo").val();
    var ruc = $("#ruc").val();

    var datos = "colaboradorId=" + colaboradorId + "&userId=" + roleId + "&nombreCol=" + nombre + "&apellidoCol=" + apellido + "&correo=" + correo + "&ruc=" + ruc;
    //alert(datos);

    $.ajax({
        type: "POST",
        url: "/Account/EditProfile/",
        dataType: "JSON",
        data: datos,
        success: function (data) {
            if (data.success) {
                AlertTimer("Completado", data.message, "success");
                $("#nombreCol").html(data.Nombre);
            } else {
                Alert("Error", data.message, "error");
            }
        }
    });
}

//FUNCION PARA HACER EL CRUD A BODEGA POR MEDIO DEL MODAL (RECIBE UN FORM = FORMULARIO)
function SubmitFormPassword(form) {

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