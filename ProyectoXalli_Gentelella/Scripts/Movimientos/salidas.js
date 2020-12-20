//CARGA EL CODIGO DE LA ORDEN AUTOMATICAMENTE
function cargarCodigo() {
    $.ajax({
        type: "GET",
        url: "/Ordenes/OrdenesCode",
        success: function (data) {
            if (data < 10) {
                $("#codigoOrden").val("000" + data);
            } else if (data >= 10 || data < 100) {
                $("#codigoOrden").val("00" + data);
            } else if (data >= 100 || data < 1000) {
                $("#codigoOrden").val("0" + data);
            } else {
                $("#codigoOrden").val(data);
            }

        }
    });
}

//INICIALIZADOR DE DATEPICKER
$('#fechaOrden').datetimepicker({
    format: 'DD/MM/YYYY',
    defaultDate: new Date(),
    locale: 'es'
});

function categoryList() {
    $.ajax({
        type: "GET",
        url: "/Ordenes/CategoryList",
        success: function (data) {
            var agregar = "";

            for (var i = 0; i < data.length; i++) {
                agregar += '<option value="' + data[i].Id + '">' + data[i].Descripcion + '</option>';
            }

            $("#categoria").append(agregar);
        }
    });
}

$("#filtro").on("keyup", function () {
    var value = $(this).val().toLowerCase().trim();

    $(".items").filter(function () {
        $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);//SI ES DIFERENTE A -1 ES QUE ENCONTRO
    });
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

//CARGA DE ORDEN EL CODIGO A LA VISTA
$(document).ready(function () {
    cargarCodigo();

    //LIMPIAR
    limpiarInicio();
    tablaVacia();

    var loginId = $("#session").val();

    //PONER POR DEFECTO EL EMPLEADO LOGEADO
    $.ajax({
        type: "GET",
        url: "/Ordenes/LoggedUser/",
        data: { LoginId: loginId },
        success: function (data) {
            $("#mesero").val(data.Nombre);
            $("#mesero").attr("value", data.MeseroId);
        },
        error: function () {
            Alert("Error", "Datos del Colaborador sin cargar", "error");
            $("#btnGuardarOrden").attr("disabled", true);
        }
    });

    categoryList();
});

//LIMPIAR TODO
function limpiarInicio() {
    //INICIALIZAR EL SELECT2 CATEGORIA
    $("#categoria").val("-1");
    $('#categoria').trigger('change'); // Notify any JS components that the value changed

    //LIMPIAR TABLA
    $("#table_body").empty();
}

//MEUSTRA MENSAJE CUANDO LA TABLA ESTA VACIA
function tablaVacia() {
    var tbody = $("#productTable #table_body");//OBTENEMOS EL CONTENIDO DE LA TABLA
    //SI NO HAY DATA
    if (tbody.children().length === 0) {
        var agregar = '<tr class="even pointer" id="noProd"><td colspan="5" style="text-align: center;">SIN REGISTROS DE PRODUCTOS</td></tr>';
        $("#table_body").append(agregar);
    }
}

//CAMBIAR EL MENU POR CATEGORIAS
$("#categoria").change(function () {
    //ALMACENO EL ID DEL SELECT 2
    var categoriaId = $("#categoria").val();

    //SI CATEGORIA ES DIFERENTE AL PLACEHOLDER
    if (categoriaId != "") {
        $.ajax({
            type: "GET",
            url: "/Ordenes/BebidasInventariado/" + categoriaId,
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

                    //RECORRER TODOS LOS ELEMENTOS A MOSTRAR
                    for (var i = 0; i < data.length; i++) {
                        agregarMenu += '<div class="col-md-4 items" id="' + data[i].Id + '">' +//SE LE ASIGNA UN IDENTIFICADOR PARA REALIZAR EL CRUD Y ACTUALIZAR VISTA
                            '<div class="thumbnail">' +
                            '<div class="image view view-first">' +
                            '<img style="width: 100%; height:100%; display: block;" src="' + data[i].Imagen + '"alt="' + data[i].Platillo + '" />' +
                            '<div class="mask no-caption">' +
                            '<div class="tools tools-bottom">' +
                            '<a onclick=detalleSalidas("/Ordenes/DetalleSalida/",' + data[i].Id + ')><i class="fa fa-plus"></i></a>' +
                            '<a onclick=CargarParcial("/Menus/Details/' + data[i].Id + '")><i class="fa fa-eye"></i></a>' +
                            '</div>' +
                            '</div>' +
                            '</div>' +
                            '<div class="caption" id="data">' +
                            '<p>' +
                            '<strong id="platilloDesc" data-toggle="tooltip" title="' + data[i].Platillo + '">' + data[i].Platillo + '</strong>' +
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

//FUNCION PARA CREAR EL DETALLE DE PLATILLO A LA TABLA
function detalleSalidas(url, id) {
    $("#small-modaltitle").html("Detalle");

    CargarSmallParcial(url);
    cargarDet(id);
    mostrarExistencia(id);
}


//FUNCION PARA MOSTRAR LA EXISTENCIA
function mostrarExistencia(id) {
    $.ajax({
        type: "GET",
        url: "/Ordenes/existencia/",
        data: { id },
        dataType: "JSON",
        success: function (data) {

            var agregar = '<small val="' + data.existencia + '">Existencia: ' + data.mensaje + '</small>';

            $("#existencia").html(agregar);
        }
    });
}

//CARGAR LOS DATOS DEL PLATILLO A LA MODAL
function cargarDet(id) {
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

//AGREGA EL PLATILLO SELECCIONADO A LA TABLA
function addDetails() {
    var cantidad = $("#cantidadOrden").val(), existencia = $("#existencia small").attr("val");

    if (cantidad <= 0) {
        Alert("Error", "Ingrese una cantidad a ordenar", "error");
    } else if (existencia == -1) {
        Alert("Error", "La bebida no esta disponible para agregar", "error");
    } else if (existencia == -2) {
        agregarDetalle();
    } else if (existencia > 0) {
        if (parseInt(cantidad) <= parseInt(existencia)) {
            agregarDetalle();
        } else {
            Alert("Error", "La cantidad no puede ser mayor a la existencia", "error");
        }
    } if (existencia == 0) {
        Alert("Error", "Este producto no tiene existencias", "error");
    }
}//FIN FUNCTION

function agregarDetalle() {
    //SE ELIMINA LA FILA DE INICIO
    $("#noProd").remove();

    var platillo = $("#platillo").val(), platilloId = $("#platillo").attr("name"), cantidad = $("#cantidadOrden").val();

    var filas = $("#table_body").find("tr");
    var registrado = false, i = 0;

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
        agregar += '<td class="" Style ="text-align: center;">' + cantidad + '</td>';
        agregar += '<td class=" last"><a class="btn btn-primary btn-xs" id="boton" onclick="editPlatillo(this);"><i class="fa fa-edit"></i></a>';
        agregar += '<a class="btn btn-danger btn-xs" onclick = "deletePlatillo(this);"> <i class="fa fa-trash"></i></a></td>';
        agregar += '</tr>';

        //AGREGAR PRODUCTO A LA TABLA
        $("#table_body").append(agregar);

        $("#smallModal").modal("hide");
    } else {
        Alert("Error", "El platillo seleccionado ya se encuentra en la tabla", "error");
    }

    $("body").removeClass("modal-open");
}

//ELIMINA TODOS LOS ELEMENTOS DENTRO DE UN DIV
function deleteRows() {
    //TOMO EL DIV PRINCIPAL (PADRE)
    var element = document.getElementById("menuCategory");

    //RECORRO EL ELEMENTO Y BORRO TODOS LOS DIV CHILD
    while (element.firstChild) {
        element.removeChild(element.firstChild);
    }//FIN CICLO
}

//FUNCION PARA ALMACENAR LA ORDEN
function guardarSalidas() {
    var codigo = $("#codigoOrden").val(), meseroId = $("#mesero").attr("value");

    //CREAR LA FECHA DE ORDEN
    var date = $("#fechaOrden").val() + " " + moment().format('h:mm:ss a');

    var OrdenDetails = new Array();

    //ALMACEMAR ELEMENTOS DE LA TABLA
    $("#table_body tr").each(function () {

        if ($(this).attr("id") != "noProd") {
            var row = $(this);
            var item = {};

            item["Id"] = 0;
            item["CantidadOrden"] = row.find("td").eq(1).html();
            item["PrecioOrden"] = 0;
            item["NotaDetalleOrden"] = "";
            item["MenuId"] = row.find("td").eq(0).attr("value");
            item["OrdenId"] = 0;
            item["EstadoDetalleOrden"] = false;

            OrdenDetails.push(item);
        }
    });


    if (OrdenDetails.length > 0) {
        var data = "Codigo=" + parseInt(codigo) + "&MeseroId=" + meseroId + "&FechaOrden=" + date + "&detalleOrden=" + JSON.stringify(OrdenDetails);

        $.ajax({
            type: "POST",
            url: "/Ordenes/guardarSalida/",
            data: data,
            success: function (data) {
                if (data.success) {
                    //LIMPIAR PANTALLA
                    limpiarInicio();
                    tablaVacia();

                    //VOLVER A CARGAR EL CODIGO
                    cargarCodigo();

                    AlertTimer("Completado", data.message, "success");
                } else {
                    Alert("Error", data.message, "error");
                }
            }
        });
    } else {
        Alert("Error", "No se encontraron registros para realizar salidas", "error");
    }
}//FIN FUNCTION

//FUNCION PARA EDITAR UN PRODUCTO DE LA TABLA
function editPlatillo(indice) {
    //EVENTO ONCLICK DEL BOTON EDITAR
    $("#table_body").on("click", "#boton", function () {
        //OBTENER LOS VALORES A UTILIZAR
        var id = $(this).parents("tr").find("td").eq(0);
        var cantidad = $(this).parents("tr").find("td").eq(1).html();

        $("#smallModal").modal("show"); //MUESTRA LA MODAL
        $("#vParcial").html("");//LIMPIA LA MODAL POR DATOS PRECARGADOS

        $.ajax({
            type: "GET", //TIPO DE ACCION
            url: "/Ordenes/DetalleSalida", //URL DEL METODO A USAR
            success: function (parcial) {
                $("#vParcial").html(parcial);//CARGA LA PARCIAL CON ELEMENTOS QUE CONTEGA
                cargarDet(id.attr("value"));
                $("#cantidadOrden").val(cantidad);
                mostrarExistencia(id.attr("value"));
            }//FIN SUCCESS
        });//FIN AJAX        

        indice.closest("tr").remove();//ELIMINAR LA FILA
        tablaVacia();
    });
}//FIN FUNCTION

//FUNCION PARA ELIMINAR UNA FILA SELECCIONADA DE LA TABLA
function deletePlatillo(row) {
    //SE BUSCA LA POSICION DE LA FILA SELECCIONADA PARA ELIMINARLA
    var indice = row.parentNode.parentNode.rowIndex;
    document.getElementById('productTable').deleteRow(indice);


    tablaVacia();
}//FIN FUNCTION