function CargarCliente() {
    $("#clientBody").empty();

    $.ajax({
        type: "GET",
        url: "/Busquedas/busquedaClientes/",
        data: {
            Nombre: $("#nombre").val().trim(), Apellido: $("#apellido").val().trim()
        },
        contentType: "application/json",//SE ESPECIFICA EL TIPO DE ENCABEZADO DESEADO PARA EVITAR OBJETOS UNDEFINED
        success: function (data) {

            if (data != null) {
                //ELIMINAR LA FILA POR DEFECTO
                $("#data").remove();
                var agregar = "";

                //LLENAR LA TABLA DE CLIENTES
                for (var i = 0; i < Object.keys(data).length; i++) {
                    agregar += '<tr class="headings" id = "data">' +
                        '<td>' + data[i].Documento + '</td>' +
                        '<td>' + data[i].RUC + '</td>' +
                        '<td value=' + data[i].DatoId + '>' + data[i].Nombres + '</td>' +
                        '<td Style ="text-align: center;">' + '<a type="button" onclick = "seleccionarCliente(this)" class="btn btn-primary btn-xs" id="btnSelect"><i class="fa fa-check"></i></a>' + '</td>' +
                        '</tr>';
                }

                $("#clientBody").append(agregar);
            }
        }
    });
}//FIN FUNCTION

//FUNCION PARA SELECCIONAR UN CLIENTE
function seleccionarCliente(row) {
    //OBTENER LOS VALORES A UTILIZAR
    var documento = "", ruc = "", cliente = "";

    //OBTENER LOS VALORES A UTILIZAR
    documento = $(row).parents("tr").find("td").eq(0).html();
    ruc = $(row).parents("tr").find("td").eq(1).html();
    cliente = $(row).parents("tr").find("td").eq(2);

    var numRuc = "";

    //SI EL RUC NO ESTA VACIO
    if (!ruc.includes("-")) {
        numRuc == ruc;
    }

    var clienteId = cliente.attr("value");//ID CLIENTE
    var nameClient = cliente.text();//NOMBRE CLIENTE

    $("#identificacion").val(documento);
    $("#nombreCliente").val(nameClient);
    $("#nombreCliente").attr("val", clienteId);
    $("#ruc").val(numRuc);

    $("#small-modal").modal("hide");
}//FIN FUNCION
