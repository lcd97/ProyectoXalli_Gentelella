//FUNCION PARA ALMACENAR AL PROVEEDOR
function saveSeller() {
    //VARIABLES DE LA TABLA PROVEEDOR
    var NombreComercial = "", Telefono = "", RUC, Local, RetenedorIR = "", NombreProveedor = "", ApellidoProveedor = "", CedulaProveedor = "";
    //ASIGNANDO VALORES GENERALES
    Telefono = $("#telefono").val();
    RUC = $("#ruc").val().toUpperCase();
    //EstadoProveedor = true;
    RetenedorIR = $(".ir").is(":checked");

    Local = $('.btn-group > .btn.active').attr("value");
    //ASIGNANDO VALORES SEGUN EL TIPO DE PROVEEDOR
    if (Local == "true") {
        NombreProveedor = $("#nombre").val();
        ApellidoProveedor = $("#apellido").val();
        CedulaProveedor = $("#cedula").val();
    } else {
        NombreComercial = $("#nombre").val();
    }//FIN IF-ELSE    

    if (validado(Local) === true) {
        $.ajax({
            type: "POST",
            url: "/Proveedores/Create",
            data: {
                //VALORES A ALMACENAR
                NombreComercial, Telefono, RUC, Local, RetenedorIR, NombreProveedor, ApellidoProveedor, CedulaProveedor
            },
            dataType: "JSON",
            success: function (data) {
                if (data.success) {
                    $("#Table").DataTable().ajax.reload(); //RECARGAR DATATABLE PARA VER LOS CAMBIOS
                    $("#small-modal").modal("hide"); //CERRAR MODAL
                    AlertTimer("Completado", data.message, "success");
                } else
                    Alert("Error", data.message, "error");//MENSAJE DE ERROR
            },
            error: function () {
                Alert("Error", "Intentelo de nuevo", "error");
            }
        });//FIN AJAX
    } else {
        Alert("Error", "Campos vacios", "error");
    }
}//FIN FUNCTION

//FUNCION PARA ACTUALIZAR CAMPOS DEL PROVEEDOR
function UpdateProvider(Id) {
    //VARIABLES DE LA TABLA PROVEEDOR
    var NombreComercial = "", Telefono = "", RUC = "", EstadoProveedor = "", Local = "", RetenedorIR = "", NombreProveedor = "", ApellidoProveedor = "", CedulaProveedor = "";
    //ASIGNANDO VALORES GENERALES
    Telefono = $("#telefono").val();
    RUC = $("#ruc").val().toUpperCase();
    EstadoProveedor = $(".activo").is(":checked");
    RetenedorIR = $(".ir").is(":checked");
    Local = $('.btn-group > .btn.active').attr("value");
    //ASIGNANDO VALORES SEGUN EL TIPO DE PROVEEDOR
    if (Local == "true") {
        NombreProveedor = $("#nombre").val();
        ApellidoProveedor = $("#apellido").val();
        CedulaProveedor = $("#cedula").val();
    } else {
        NombreComercial = $("#nombre").val();
    }//FIN IF-ELSE

    //VALIDACION SHAMPOO
    if (validado(Local) === true) {
        //FUNCION AJAX
        $.ajax({
            type: "POST",
            url: "/Proveedores/UpdateProveedor/" + Id,
            data: {
                //VALORES A ALMACENAR
                Id, NombreComercial, Telefono, RUC, EstadoProveedor, Local, RetenedorIR, NombreProveedor, ApellidoProveedor, CedulaProveedor
            },
            dataType: "JSON",
            success: function (data) {
                if (data.success) {
                    $("#Table").DataTable().ajax.reload(); //RECARGAR DATATABLE PARA VER LOS CAMBIOS
                    $("#small-modal").modal("hide"); //CERRAR MODAL
                    AlertTimer("Completado", data.message, "success");
                } else
                    Alert("Error", data.message, "error");//MENSAJE DE ERROR
            },
            error: function () {
                Alert("Error", "Intentelo de nuevo", "error");
            }
        });//FIN AJAX   
    } else {
        Alert("Eror", "Campos vacios", "error");
    }
}//FIN FUNCTION

//VALIDACIONES
function validado(Local) {
    //SI EL PROVEEDOR ES LOCAL
    if (Local == "true") {
        if ($("#nombre").val() != "" && $("#apellido").val() != "" && $("#cedula").val() != "" && $("#telefono").val() != "") {
            return true;
        } else {
            return false;
        }
    } else {//SI EL PROVEEDOR ES UN COMERCIO
        if ($("#nombre").val() != "" && $("#ruc").val() != "" && $("#telefono").val() != "") {
            return true;
        } else {
            return false;
        }
    }
}