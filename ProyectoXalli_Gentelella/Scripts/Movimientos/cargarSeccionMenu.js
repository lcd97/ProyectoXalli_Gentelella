﻿//CAMBIAR EL MENU POR CATEGORIAS
$("#categoria").change(function () {
    //ALMACENO EL ID DEL SELECT 2
    var categoriaId = $("#categoria").val();

    //SI CATEGORIA ES DIFERENTE AL PLACEHOLDER
    if (categoriaId != "") {
        $.ajax({
            type: "GET",
            url: "/Ordenes/MenuByCategoria/" + categoriaId,
            dataType: "JSON",
            success: function (data) {
                //SI NO HAY ELEMENTOS
                if (data.length <= 0) {
                    $("#filtro").attr("disabled", true);//DESACTIVO EL INPUT DE FILTRO

                    //LIMPIAR BUSQUEDA ANTERIOS
                    deleteRows();

                    var agregar = '<h2 id="nada" style="text-align:center;" col-md-12 col-sm-12 col-xs-12>Sin elementos disponibles</h2>';
                    $("#menuCategory").append(agregar);
                } else if (data.length > 0) {//SI HAY ELEMENTOS
                    var agregarMenu = "";
                    $("#filtro").removeAttr("disabled");

                    //ELIMINAR BUSQUEDA ANTERIOR
                    deleteRows();

                    //var paginarAdd = '<nav id="paginar" aria-label="Page navigation example" class="col-md-12 col-sm-12 col-xs-12">' +
                    //    '<ul class="pagination" id="myPager"></ul>' +
                    //    '</nav>';

                    //RECORRER TODOS LOS ELEMENTOS A MOSTRAR
                    for (var i = 0; i < data.length; i++) {
                        agregarMenu += '<div class="col-md-4 items" id="' + data[i].Id + '">' +//SE LE ASIGNA UN IDENTIFICADOR PARA REALIZAR EL CRUD Y ACTUALIZAR VISTA
                            '<div class="thumbnail">' +
                            '<div class="image view view-first">' +
                            '<img style="width: 100%; height:100%; display: block;" src="' + data[i].Imagen + '"alt="' + data[i].DescripcionPlatillo + '" />' +
                            '<div class="mask no-caption">' +
                            '<div class="tools tools-bottom">' +
                            '<a onclick=detallePedido("/Ordenes/DetalleOrden/",' + data[i].Id + ')><i class="fa fa-plus"></i></a>' +
                            '<a onclick=CargarParcial("/Menus/Details/' + data[i].Id + '")><i class="fa fa-eye"></i></a>' +
                            '</div>' +
                            '</div>' +
                            '</div>' +
                            '<div class="caption" id="data">' +
                            '<p>' +
                            '<strong>' + data[i].Platillo + '</strong>' +
                            '</p>' +
                            '<p> $ ' + data[i].Precio + '</p>' +
                            '</div>' +
                            '</div >' +
                            '</div >';
                    }

                    $("#menuCategory").append(agregarMenu);
                    //$("#menuCategory").append(paginarAdd);

                    //cargarPaginacion();
                }//FIN IF-ELSE ELEMENTOS
            }
        });
    } else {
        $("#filtro").attr("disabled", true);//DESACTIVO EL INPUT DE FILTRO

        //LIMPIAR BUSQUEDA ANTERIOS
        deleteRows();

        var agregar = '<h2 id="nada" style="text-align:center;" col-md-12 col-sm-12 col-xs-12>Seleccione una categoría</h2>';
        $("#menuCategory").append(agregar);
    }
});

//ELIMINA TODOS LOS ELEMENTOS DENTRO DE UN DIV
function deleteRows() {
    //TOMO EL DIV PRINCIPAL (PADRE)
    var element = document.getElementById("menuCategory");

    //RECORRO EL ELEMENTO Y BORRO TODOS LOS DIV CHILD
    while (element.firstChild) {
        element.removeChild(element.firstChild);
    }//FIN CICLO

    ////ELIMINAR LA PAGINACION
    //$('#paginar').remove();
}

//FUNCION PARA CREAR EL DETALLE DE PLATILLO A LA TABLA
function detallePedido(url, id) {
    $("#small-modaltitle").html("Detalle");

    CargarSmallParcial(url);
    cargarDetalle(id);
}

//CARGAR LOS DATOS DEL PLATILLO A LA MODAL
function cargarDetalle(id){
    $.ajax({
        type: "GET", //TIPO DE ACCION
        url: "/Menus/getMenuItem/" + id, //URL DEL METODO A USAR
        success: function (data) {
            $("#precioOrden").val("$ " + data.menu.Precio);
            $("#platillo").val(data.menu.Platillo);
            $("#platillo").attr("name", data.menu.PlatilloId);
        }//FIN SUCCESS
    });//FIN AJAX

}

//FUNCION PARA CALCULAR EL TOTAL GENERAL DE LA TABLA
function CalcularTotal() {
    var total = 0;

    //RECORRER LA TABLA PARA SUMAR TODOS LOS TOTALES DE PRODUCTOS
    $("#table_body tr").each(function () {
        var str = $(this).find("td").eq(3).html();
        var res = str.split("$ ");
        total += parseFloat(res[1]);
    });

    return total;
}//FIN FUNCTION

$("#filtro").on("keyup", function () {
    var value = $(this).val().toLowerCase().trim();

    $(".items").filter(function () {
        $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);//SI ES DIFERENTE A -1 ES QUE ENCONTRO
    });
});

//FUNCION PARA EDITAR UN PRODUCTO DE LA TABLA
function editPlatillo(indice) {
    //EVENTO ONCLICK DEL BOTON EDITAR
    $("#table_body").on("click", "#boton", function () {
        //OBTENER LOS VALORES A UTILIZAR
        var id = $(this).parents("tr").find("td").eq(0);
        var nota = $(this).parents("tr").find("td").eq(1);
        var cantidad = $(this).parents("tr").find("td").eq(2).html();

        $("#smallModal").modal("show"); //MUESTRA LA MODAL
        $("#vParcial").html("");//LIMPIA LA MODAL POR DATOS PRECARGADOS

        $.ajax({
            type: "GET", //TIPO DE ACCION
            url: "/Ordenes/DetalleOrden", //URL DEL METODO A USAR
            success: function (parcial) {
                $("#vParcial").html(parcial);//CARGA LA PARCIAL CON ELEMENTOS QUE CONTEGA
                cargarDetalle(id.attr("value"));
                $("#cantidadOrden").val(cantidad);
                $("#notaOrden").val(nota.attr("value"));
            }//FIN SUCCESS
        });//FIN AJAX        

        var prec = (nota.text()).split("$ ");

        //RECALCULAR TOTAL TFOOT
        var totalF = $("#total").html();//AGARRAR EL TOTAL DE LA TABLA
        var prodPrecio = totalF.split("$ ");//QUITARLE EL SIGNO DE DOLAR
        var resta = parseFloat(prodPrecio[1] - (prec[1] * cantidad));

        $("#total").html("$ " + resta);

        indice.closest("tr").remove();//ELIMINAR LA FILA

    });
}//FIN FUNCTION

//FUNCION PARA ELIMINAR UNA FILA SELECCIONADA DE LA TABLA
function deletePlatillo(row) {
    //SE BUSCA LA POSICION DE LA FILA SELECCIONADA PARA ELIMINARLA
    var indice = row.parentNode.parentNode.rowIndex;
    document.getElementById('productTable').deleteRow(indice);

    //RECALCULAMOS EL TOTAL
    var resta = parseFloat(CalcularTotal());
    $("#total").html("$ " + resta);

}//FIN FUNCTION