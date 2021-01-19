$(document).ready(function () {

    var role = $("#fecha").attr("name");//OBTENGO EL ROL DEL LOGGEADO
    var ordenId = $("#codigoOrden").attr("name");//EL ID DE LA ORDEN
    var detalleOrden = new Array();

    //RECUPERO LA INFORMACION DE LA ORDEN
    $.ajax({
        type: "GET",
        url: "/Ordenes/getItemsPrep/",
        data: { area: role, ordenId: ordenId },
        success: function (data) {
            //LLENO EL ENCABEZADO
            $("#codigoOrden").html(cargarCodigo(data.encabezado.Codigo));
            $("#fecha").html(data.encabezado.Fecha);
            $("#hora").html(data.encabezado.HoraOrden);
            $("#mesero").html(data.encabezado.Mesero);
            $("#cliente").html(data.encabezado.Cliente);
            $("#mesa").html(data.encabezado.Mesa);

            var agregar = "";

            if (data.values != null) {
                //RECORRO EL DETALLE DE LOS ELEMENTOS
                for (var i = 0; i < data.values.length; i++) {
                    //SI NO EXISTE NOTA PONER QUE NO HAY
                    var nota = data.values[i].Nota == null ? "SIN NOTAS" : data.values[i].Nota;

                    agregar += '<li val="' + data.values[i].IdDetalle + '">' +
                        '<h2>' + data.values[i].Cantidad + ' x ' + data.values[i].Platillo + '</h2>' +
                        '<p>&nbsp;&nbsp;&nbsp;<strong>Notas:</strong> ' + nota + '</p>' + '</li>';
                }
                $(".to_do").append(agregar);//AGREGO LAS ORDENES

                //RECUPERO LOS IDS DE LOS PRODUCTOS
                for (var d = 0; d < data.ids.length; d++) {
                    detalleOrden.push(data.ids[d]);//LO AGREGO A UN ARRAY
                }

                //PONGO LOS IDS DE LA ORDEN PARA LUEGO RECUPERARLOS AL GUARDAR
                $(".data").attr("name", detalleOrden);
            }
        }
    });
});

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

//GUARDA EL DETALLE DE LA ORDEN
function saveEntries() {
    //RECUPERO LOS IDS DE LOS PLATILLO/BEBIDAS
    var data = $(".data").attr("name");

    //RECORRER UNA LISTA UL LI PARA OBTENER SUS VALORES
    //$(".to_do li").each(function () {
    //    alert($(this).attr("val"));
    //});

    //GUARDO CON AJAX
    $.ajax({
        type: "POST",
        url: "/Ordenes/EditOrderDetails/",
        data: { detailsId: data },
        success: function (data) {
            if (data.success) {
                var url = "/Home/Index/";
                window.location.href = url;
            } else {
                Alert("Error", data.message, "error");
            }
        }
    });
}