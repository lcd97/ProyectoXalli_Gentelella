$(document).ready(function () {
    //CARGA DE ENTRADA EL CODIGO A LA VISTA
    cargarCodigo();

    //CARGAR DATOS DE PROVEEDOR PARA SELECT2
    $.ajax({
        type: "GET",
        url: '/Entradas/getProveedor',
        dataType: 'JSON',
        success: function (data) {

            var agregar = "";

            for (var i = 0; i < Object.keys(data.data).length; i++) {
                agregar += '<option value="' + data.data[i].Id + '">' + data.data[i].Proveedor + '</option>';
            }

            $("#proveedor").append(agregar);
        }
    });

    //CARGAR DATOS DE TIPO DE ENTRADA PARA SELECT2
    $.ajax({
        type: "GET",
        url: '/Entradas/getTipoEntrada',
        dataType: 'JSON',
        success: function (data) {

            var agregar = "";

            for (var i = 0; i < Object.keys(data.data).length; i++) {
                agregar += '<option value="' + data.data[i].Id + '">' + data.data[i].Entrada + '</option>';
            }

            $("#entrada").append(agregar);
        }
    });

    //CARGAR DATOS DE PRODUCTOS PARA SELECT2
    $.ajax({
        type: "GET",
        url: '/Entradas/getProductos',
        dataType: 'JSON',
        success: function (data) {

            var agregar = "";

            for (var i = 0; i < Object.keys(data.data).length; i++) {
                agregar += '<option value="' + data.data[i].Id + '">' + data.data[i].Presentacion + '</option>';
            }

            $("#producto").append(agregar);
        }
    });

    //CARGAR DATOS DE ENTRADAS PARA SELECT2
    $.ajax({
        type: "GET",
        url: '/Entradas/getArea',
        dataType: 'JSON',
        success: function (data) {

            var agregar = "";

            for (var i = 0; i < Object.keys(data.data).length; i++) {
                agregar += '<option value="' + data.data[i].Id + '">' + data.data[i].Bodega + '</option>';
            }

            $("#area").append(agregar);
        }
    });

    limpiarPantalla();
    tablaVacio();
});

function tablaVacio() {
    var tbody = $("#productTable #table_body");//OBTENEMOS EL CONTENIDO DE LA TABLA
    //SI NO HAY DATA
    if (tbody.children().length === 0) {
        var agregar = '<tr class="even pointer" id="noProd"><td colspan="5" style="text-align: center;">SIN REGISTROS DE PRODUCTOS</td></tr>';
        $("#table_body").append(agregar);
    }
}

//CARGA EL CODIGO DE LA ENTRADA AUTOMATICAMENTE
function cargarCodigo() {
    $.ajax({
        type: "GET",
        url: "/Entradas/EntradaCode",
        success: function (data) {
            $("#codigoEntrada").val(data);
        }
    });
}

//INICIALIZADOR DE DATEPICKER
$('#fechaEntrada').datetimepicker({
    format: 'DD/MM/YYYY',
    defaultDate: new Date(),
    locale: 'es'
});

//INICIALIZADOR DE LENGUAJE SELECT 2
$('.js-example-basic-single').select2({
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

//FUNCION PARA ALMACENAR AL PROVEEDOR
function saveSeller() {
    //VARIABLES DE LA TABLA PROVEEDOR
    var NombreComercial = "", Telefono = "", RUC, Local, RetenedorIR = "", NombreProveedor = "", ApellidoProveedor = "", CedulaProveedor = "";
    //ASIGNANDO VALORES GENERALES
    Telefono = $("#telefono").val().trim();
    RUC = $("#ruc").val().toUpperCase().trim();
    RetenedorIR = $(".ir").is(":checked");

    Local = $('.btn-group > .btn.active').attr("value");
    //ASIGNANDO VALORES SEGUN EL TIPO DE PROVEEDOR
    if (Local == "true") {
        NombreProveedor = $("#nombre").val().trim();
        ApellidoProveedor = $("#apellido").val().trim();
        CedulaProveedor = $("#cedula").val().trim();
    } else {
        NombreComercial = $("#nombre").val().trim();
    }//FIN IF-ELSE    

    if (validado(Local) === true) {
        $.ajax({
            type: "POST",
            url: "/Proveedores/Create",
            data: {
                //VALORES A ALMACENAR
                NombreComercial, Telefono, RUC, Local, RetenedorIR, NombreProveedor, ApellidoProveedor, CedulaProveedor
            },
            success: function (data) {
                if (data.success) {
                    //AGREGAR EL REGISTRO AL SELECT 2
                    var agregar = '<option value="' + data.Id + '">' + data.Proveedor + '</option>';

                    $("#proveedor").append(agregar);

                    $("#small-modal").modal("hide"); //CERRAR MODAL
                    //MOSTRANDO EL SWEET ALERT
                    AlertTimer("Completado", data.message, "success");
                } else
                    Alert("Error al almacenar", data.message, "error");//MENSAJE DE ERROR
            },
            error: function () {
                Alert("Error al almacenar", "Intentelo de nuevo", "error");
            }
        });//FIN AJAX
    } else {
        Alert("Error", "Campos vacios", "error");
    }
}//FIN FUNCTION

//VALIDACIONES
function validado(Local) {
    //SI EL PROVEEDOR ES LOCAL
    if (Local == "true") {
        if ($("#nombre").val().trim() != "" && $("#apellido").val().trim() != "" && $("#cedula").val().trim() != "" && $("#telefono").val().trim() != "") {
            return true;
        } else {
            return false;
        }
    } else {//SI EL PROVEEDOR ES UN COMERCIO
        if ($("#nombre").val().trim() != "" && $("#ruc").val().trim() != "" && $("#telefono").val().trim() != "") {
            return true;
        } else {
            return false;
        }
    }
}

//FUNCION PARA HACER EL CRUD A PRODUCTO POR MEDIO DEL MODAL (RECIBE UN FORM = FORMULARIO)
function SubmitForm(form) {
    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        $.ajax({
            type: "POST", //TIPO DE ACCION
            url: form.action, //ACCION O METODO A REALIZAR
            data: $(form).serialize(), //SERIALIZACION DE LOS DATOS A ENVIAR
            success: function (data) {
                if (data.success) {//SI SE REALIZO CORRECTAMENTE
                    //AGREGAR EL REGISTRO AL SELECT 2
                    var agregar = '<option value="' + data.Id + '">' + data.Producto + '</option>';
                    $("#producto").append(agregar);

                    //CERRAR MODAL
                    $("#small-modal").modal("hide"); //CERRAR MODAL
                    AlertTimer("Completado", data.message, "success");
                }//FIN IF
                else
                    //MANDAR EL ERROR DEL CONTROLADOR
                    Alert("Error", data.message, "error");
            },//FIN SUCCESS
            error: function () {
                //AQUI MANDAR EL MENSAJE DE ERROR
                Alert("Error al almacenarlo", "Intentelo de nuevo", "error");
            }//FIN ERROR
        });//FIN AJAX
    }//FIN DEL IF FORM VALID
    return false; //EVITA SALIRSE DEL METODO ACTUAL
}//FIN FUNCTION

function TableAdd() {
    $("#noProd").remove();//ELIMINA LA FILA DE MENSAJE

    //ALMACENAR LAS VARIABLES
    var producto = $("#producto").find("option:selected").val(), precio = $("#precio").val(), cantidad = $("#cantidad").val();

    //QUITARLE LA COMA A LA VARIABLE PARA CALCULAR BIEN NUMERO CON , EJ 1,200
    var precSb = parseFloat(precio.replace(/,/g, "")).toFixed(2);
    var cantSb = parseFloat(cantidad.replace(/,/g, "")).toFixed(2);
    //CALCULO EL PRECIO TOTAL DEL PRODUCTO
    var precioTotal = parseFloat(precSb * cantSb).toFixed(2);

    var agregar = "";

    //VALIDAR QUE LOS CAMPOS NO ESTEN VACIOS
    if (producto == "-1" || precio === "" || cantidad === "") {
        Alert("Error", "Llene todos los campos para agregar el producto", "error");
    } else {
        //GUARDAR LAS FILAS EXISTENTES DE LA TABLA
        var filas = $("#table_body").find("tr");
        var registrado = false, i = 0;

        //RECORRER LOS VALORES DE LA TABLA
        while (i < filas.length && registrado === false) {
            var celdas = $(filas[i]).find("td"); //devolverá las celdas de una fila

            //AGARRAR EL VALUE ALMACENADO EN LA FILA - PRODUCTO
            var comp = $(celdas[0]).attr("value");

            //COMPARAMOS QUE EL PRODUCTO A INGRESAR NO SEA EL MISMO AL QUE YA ESTA AGREGADO
            if (comp === producto) {
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
            agregar += '<td class="" value ="' + producto + '">' + $("#producto").find("option:selected").text() + '</td>';
            agregar += '<td class="" >' + "C$ " + precSb + '</td>';
            agregar += '<td class="" >' + cantidad + '</td>';
            agregar += '<td class="" >' + "C$ " + precioTotal + '</td>';
            agregar += '<td class=" last"><a class="btn btn-primary" id="boton" onclick="editProduct(this);"><i class="fa fa-edit"></i></a>';
            agregar += '<a class="btn btn-danger" onclick = "deleteProduct(this);"> <i class="fa fa-trash"></i></a></td>';
            agregar += '</tr>';

            //AGREGAR PRODUCTO A LA TABLA
            $("#table_body").append(agregar);

            //LIMPIAR LOS INPUTS Y SELECT
            $("#producto").val("-1");
            $('#producto').trigger('change'); // Notify any JS components that the value changed
            $("#precio").val("");
            $("#cantidad").val("");

            CalcularTotal();
        } else {
            Alert("Error", "El producto seleccionado ya se encuentra en la tabla", "error");
        }
    }//FIN ELSE DE VALIDACION CAMPOS VACIOS

    tablaVacio();

}//FIN FUNCTION

//FUNCION PARA EDITAR UN PRODUCTO DE LA TABLA
function editProduct(indice) {
    //EVENTO ONCLICK DEL BOTON EDITAR
    $("#productTable").on("click", "#boton", function () {
        //OBTENER LOS VALORES A UTILIZAR
        var prod = $(this).parents("tr").find("td").eq(0).attr("value");
        var precio = $(this).parents("tr").find("td").eq(1).html();
        var cantidad = $(this).parents("tr").find("td").eq(2).html();

        $("#producto").val(prod);
        $('#producto').trigger('change'); // Notify any JS components that the value changed

        //RECORTAR VALOR DE LA FILA PARA AGREGARLO AL INPUT
        var str = precio;
        var res = str.split("C$ ");
        $("#precio").val(res[1]);

        $("#cantidad").val(cantidad);

        ////RECALCULAR TOTAL TFOOT
        //var totalF = $("#total").html();
        //var prodPrecio = parseFloat(totalF.split("C$ ")[1]).toFixed(2);
        //var resta = parseFloat(parseFloat(prodPrecio).toFixed(2) - (parseFloat(res[1]).toFixed(2) * parseFloat(cantidad)).toFixed(2));

        indice.closest("tr").remove();//ELIMINAMOS LA FILA A EDITAR

        //var total = parseFloat(CalcularTotal());

        CalcularTotal();
        //$("#total").html("C$ " + total);
        tablaVacio();
    });
}//FIN FUNCTION

//FUNCION PARA ELIMINAR UNA FILA SELECCIONADA DE LA TABLA
function deleteProduct(row) {
    //SE BUSCA LA POSICION DE LA FILA SELECCIONADA PARA ELIMINARLA
    var indice = row.parentNode.parentNode.rowIndex;
    document.getElementById('productTable').deleteRow(indice);

    ////RECALCULAMOS EL TOTAL
    //var resta = parseFloat(CalcularTotal());
    //$("#total").html("C$ " + resta);
    CalcularTotal();
    tablaVacio();
}//FIN FUNCTION

//FUNCION PARA CALCULAR EL TOTAL GENERAL DE LA TABLA
function CalcularTotal() {
    //AGREGAR EL TOTAL TFOOT
    var total = 0;

    //RECORRER LA TABLA PARA SUMAR TODOS LOS TOTALES DE PRODUCTOS
    $("#table_body tr").each(function () {
        var str = $(this).find("td").eq(3).html();//PRECIO TOTAL PRODUCTO
        var res = str.split("C$ ")[1];
        total += parseFloat(res);
    });

    $("#total").html("C$ " + total.toFixed(2));
    //return parseFloat(total).toFixed(2);
}//FIN FUNCTION

//FUNCION PARA ALMACENAR EL INVENTARIO
function saveInventario() {
    var data = false, codigoEntrada = $("#codigoEntrada").val(), fecha = $("#fechaEntrada").val(), entrada = $("#entrada").find("option:selected").val(),
        proveedor = $("#proveedor").find("option:selected").val(), area = $("#area").find("option:selected").val();

    if (codigoEntrada !== "" && fecha !== "" && entrada !== "-1" && proveedor !== "-1" && area !== "-1") {

        var detalleEntrada = new Array();

        $("#table_body tr").each(function () {

            var row = $(this);

            //SI LA LINEA QUE ESTA ES DIFERENTE A LA DE LETRERO
            if (row.attr("id") !== "noProd") {
                var item = {};

                if (row.length > 0) {
                    data = true;
                } else {
                    data = false;
                }

                item["Id"] = 0;
                item["ProductoId"] = row.find("td").eq(0).attr("value");
                var precio = row.find("td").eq(1).html();
                var getPrice = precio.split("C$ ");
                item["PrecioEntrada"] = getPrice[1];
                item["CantidadEntrada"] = row.find("td").eq(2).html();
                item["Entrada"] = null;
                item["Producto"] = null;

                detalleEntrada.push(item);
            }
        });

        //alert(JSON.stringify(detalleEntrada));

        if (data) {

            //CAMBIAR FORMATO DE FECHA

            var datos = "Codigo=" + codigoEntrada + "&FechaEntrada=" + fecha + "&TipoEntradaId=" + entrada + "&BodegaId=" + area + "&ProveedorId=" + proveedor + "&detalleEntrada=" + JSON.stringify(detalleEntrada);

            $.ajax({
                type: "POST",
                url: "/Entradas/AddEntrada",
                data: datos,
                dataType: "JSON",
                success: function (data) {
                    if (data.success) {
                        //MOSTRANDO EL SWEET ALERT
                        AlertTimer("Completado", data.message, "success");
                        cargarCodigo();
                        limpiarPantalla();
                        tablaVacio();
                    } else {
                        Alert("Error", "Hubieron errores al guardar", "error");
                    }
                },
                error: function () {
                    Alert("Error", "Revise", "error");
                }
            });
        } else {
            Alert("Error", "No se encontraron registros de productos", "error");
        }
    } else {
        Alert("Error", "Campos vacios. Revise e intente de nuevo", "error");
    }//FIN VALIDACION
}//FIN FUNCTION

//FUNCION PARA LIMPIAR LA PANTALLA
function limpiarPantalla() {
    //LIMPIAR INPUTS
    //$("#codigoEntrada").val(cargarCodigo());
    $("#total").html("C$ 0.00");
    $("#precio").html("");
    $("#cantidad").html("");
    //LIMPIAR TABLA
    $("#table_body tr").remove();
    //LIMPIAR LOS SELECT2
    $('.js-example-basic-single').val('-1');
    $('.js-example-basic-single').trigger('change'); // Notify any JS components that the value changed
}