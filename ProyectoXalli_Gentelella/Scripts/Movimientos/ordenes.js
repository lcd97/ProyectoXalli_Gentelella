//CARGA EL CODIGO DE LA ORDEN AUTOMATICAMENTE
function cargarCodigo() {
    $.ajax({
        type: "GET",
        url: "/Ordenes/OrdenesCode",
        success: function (data) {
            $("#codigoOrden").val("000" + data);
        }
    });
}

//CARGA DE ORDEN EL CODIGO A LA VISTA
$(document).ready(function () {
    cargarCodigo();

    //SWITCHERY LOCO
    $(".switchery").attr("onclick", "Check()");

    //LIMPIAR
    limpiarInicio();
    limpiarInputs();
});

//LIMPIAR TODO
function limpiarInicio() {
    //INICIALIZAR EL SELECT2 CATEGORIA
    $("#categoria").val("-1");
    $('#categoria').trigger('change'); // Notify any JS components that the value changed

    //AGREGAR EL MENSAJE PRINCIOAL DE SECCION MENU
    var agregar = '<h2 id="nada" style="text-align:center;" col-md-12 col-sm-12 col-xs-12>Seleccione una categoría</h2>';
    $("#menuCategory").append(agregar);

    var elem = document.querySelector('.js-switch');

    if (elem.checked == false) {
        elem.click();
    }

    $("#titleCliente").html("Visitante");//CAMBIAR EL TITULO DE TIPO CLIENTE
    $("#identificacion").attr("readonly", "true");//DESACTIVAR CAMPO IDENTIFICACION
    $("#buscarCliente").attr("disabled", "true");//DESACTIVAR BOTON BUSCAR CLIENTE

    //LIMPIAR TABLA
    $("#table_body").empty();

    var agregarNada = '<tr id="nada">' +
        ' <td colspan="5" style="text-align:center!important;">- Sin pedidos aun -</td>' +
        '</tr>';

    $("#table_body").append(agregarNada);

    $("#total").html("$ 0");
}

//CONFIRMACION DE ACTIVAR/DESACTIVAR
function Check() {
    var elem = document.querySelector('.js-switch');

    //DEPENDE DE SU ESTADO MANDAMOS UN ALERT
    if (elem.checked == true) {
        //VISITANTE
        $("#titleCliente").html("Visitante");//CAMBIAR EL TITULO DE TIPO CLIENTE
        $("#identificacion").attr("readonly", "true");//DESACTIVAR CAMPO IDENTIFICACION
        $("#identificacion").val("readonly", "true");//DESACTIVAR CAMPO IDENTIFICACION
        $("#buscarCliente").attr("disabled", "true");//DESACTIVAR BOTON BUSCAR CLIENTE

        limpiarInputs();

    } else {
        //HUESPED
        $("#titleCliente").html("Huesped");//CAMBIAR EL TITULO DE TIPO CLIENTE
        $("#identificacion").removeAttr("readonly");//ACTIVAR CAMPO IDENTIFICACION
        $("#buscarCliente").removeAttr("disabled");//ACTIVAR BOTON BUSCAR CLIENTE
    }
}//FIN FUNCION

//LIMPIAR INPUTS DE CLIENTE
function limpiarInputs() {
    //LIMPIAR INPUTS CLIENTE
    $("#identificacion").val("");
    $("#nombreCliente").val("");
    $("#nombreCliente").attr("val", "");
    $("#ruc").val("");
}

//INICIALIZADOR DE DATEPICKER
$('#fechaOrden').datetimepicker({
    format: 'MM/DD/YYYY',
    defaultDate: new Date(),
    locale: 'es'
});

//AGREGAR LA OPCION DONDE IRA EL PLACEHOLDER DEL SELECT 2
$(".js-example-basic-single").prepend("<option value='-1' readonly></option>");

//INICIALIZADOR DE LENGUAJE SELECT 2
$('.js-example-basic-single').select2({
    placeholder: { id: "-1", text: "Seleccione" },//CARGAR PRIMERO EL PLACEHOLDER
    //MODIFICAR LAS FRASES DEFAULT DE SELECT2
    language: {

        noResults: function () {

            return "No hay resultado";
        },
        searching: function () {

            return "Buscando...";
        }
    }
});

//CAMBIAR EL MENU POR CATEGORIAS
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

function addDetails() {
    //SE ELIMINA LA FILA DE INICIO
    $("#nada").remove();

    var platillo = $("#platillo").val(), platilloId = $("#platillo").attr("name"), cantidad = $("#cantidadOrden").val(),
        precio = $("#precioOrden").val(), nota = $("#notaOrden").val();

    //SE QUITA EL SIGNO DE DOLAR
    precio = (precio.split("$ "))[1];

    var filas = $("#table_body").find("tr");
    var registrado = false, i = 0;
    var precioTotal = precio * cantidad;

    var agregar = "";

    //RECORRER LOS VALORES DE LA TABLA
    while (i < filas.length && registrado === false) {
        var celdas = $(filas[i]).find("td"); //devolverá las celdas de una fila

        //AGARRAR EL VALUE ALMACENADO EN LA FILA - PRODUCTO
        var comp = $(celdas[0]).attr("value");

        //COMPARAMOS QUE EL PRODUCTO A INGRESAR NO SEA EL MISMO AL QUE YA ESTA AGREGADO
        if (comp === platilloId) {
            registrado = true;
        } else {
            registrado = false;
        }

        i++;
    }//FIN WHILE

    //SI NO SE EN
    if (!registrado) {
        //GENERAR FILA DEL PRODUCTO A LA TABLA
        agregar += '<tr class="even pointer">';
        agregar += '<td class="" value ="' + platilloId + '">' + platillo + '</td>';
        agregar += '<td class="" value = "' + nota + '">' + "$ " + precio + '</td>';
        agregar += '<td class="" Style ="text-align: center;">' + cantidad + '</td>';
        agregar += '<td class="" >' + "$ " + precioTotal + '</td>';
        agregar += '<td class=" last"><a class="btn btn-primary btn-xs" id="boton" onclick="editPlatillo(this);"><i class="fa fa-edit"></i></a>';
        agregar += '<a class="btn btn-danger btn-xs" onclick = "deletePlatillo(this);"> <i class="fa fa-trash"></i></a></td>';
        agregar += '</tr>';

        //CALCULAR EL TOTAL
        var total = parseFloat(CalcularTotal());

        //AGREGAR PRODUCTO A LA TABLA
        $("#table_body").append(agregar);
        //AGREGAR EL TOTAL TFOOT
        $("#total").html("$ " + (total + precioTotal));

        $("#smallModal").modal("hide");
    } else {
        Alert("Error", "El platillo seleccionado ya se encuentra en la tabla", "error");
    }
}//FIN FUNCTION

//CARGAR LOS DATOS DEL PLATILLO A LA MODAL
function cargarDetalle(id) {
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

$("#table_body").on("change", function () {
    alert("change");
});

$("#filtro").on("keyup", function () {
    var value = $(this).val().toLowerCase().trim();

    $(".items").filter(function () {
        $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);//SI ES DIFERENTE A -1 ES QUE ENCONTRO
    });
});

$("#identificacion").blur(function () {
    $("#nombreCliente").val("");
    $("#ruc").val("");

    if ($(this).val() != "") {
        $.ajax({
            type: "GET",
            url: "/Ordenes/DataClient/",
            data: {
                identificacion: $(this).val().trim()
            },
            dataType: "JSON",
            success: function (data) {
                if (data != null) {
                    $("#nombreCliente").val(data.Nombres);
                    $("#nombreCliente").attr("val", data.ClienteId);
                    $("#ruc").val(data.RUC);
                }
            }
        });
    }
});

//FUNCION PARA ALMACENAR LA ORDEN
function guardarOrden() {
    var codigo = $("#codigoOrden").val(), meseroId = 2, clienteId = $("#nombreCliente").attr("val");

    var date = $("#fechaOrden").val() + " " + moment().format('h:mm:ss');


    var OrdenDetails = new Array();

    $("#table_body tr").each(function () {

        var row = $(this);
        var item = {};

        var precio = row.find("td").eq(1).html();
        var getPrice = precio.split("$ ");

        item["Id"] = 0;
        item["CantidadOrden"] = row.find("td").eq(2).html();
        item["PrecioOrden"] = getPrice[1];
        item["NotaDetalleOrden"] = row.find("td").eq(1).attr("value");
        item["MenuId"] = row.find("td").eq(0).attr("value");
        item["OrdenId"] = 0;
        item["EstadoDetalleOrden"] = true;

        OrdenDetails.push(item);
    });

    //alert(JSON.stringify(OrdenDetails));

    var data = "Codigo=" + parseInt(codigo) + "&MeseroId=" + meseroId + "&ClienteId=" + clienteId + "&FechaOrden=" + date + "&detalleOrden=" + JSON.stringify(OrdenDetails);

    //alert(data);

    $.ajax({
        type: "POST",
        url: "/Ordenes/Create/",
        data: data,
        success: function (data) {
            if (data.success) {
                //LIMPIAR PANTALLA
                limpiarInputs();
                limpiarInicio();

                //VOLVER A CARGAR EL CODIGO
                cargarCodigo();

                AlertTimer("Completado", data.message, "success");
            } else {
                Alert("Error", data.mesage, "error");
            }
        }
    });
}//FIN FUNCTION