//INICIALIZAR AUTOMATICAMENTE EL DATATABLE
$(document).ready(function () {
    crearTabla();

    limpiarDT();
});

//CREAR E INICIALIZAR TABLA CON DATABALE
function crearTabla() {
    var table = '<table id="Table" class="table table-striped table-bordered">' +
        '<thead>' +
        '<tr>' +
        '<th>Código</th>' +
        '<th>Descripción</th>' +
        '<th>Acciones</th>' +
        '</tr>' +
        '</thead>' +
        '</table>';

    $("#cargar").html(table);

    var Table = $("#Table").DataTable({
        "language": {

            "sProcessing": "Procesando...",
            "sLengthMenu": "Mostrar _MENU_ registros",
            "sZeroRecords": "No se encontraron resultados",
            "sEmptyTable": "Ningún elemento cargado en esta tabla",
            "sInfo": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
            "sInfoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
            "sInfoFiltered": "(filtrado de un total de _MAX_ registros)",
            "sInfoPostFix": "",
            "sSearch": "Buscar:",
            "sUrl": "",
            "sInfoThousands": ",",
            "sLoadingRecords": "Cargando...",

            "oPaginate": {
                "sFirst": "Primero",
                "sLast": "Último",
                "sNext": "Siguiente",
                "sPrevious": "Anterior"
            }
        }//FIN IDIOMA DATATABLE
    });

    Table
        .clear()
        .draw();
}

//CARGAR EL URL CON RESPECTO A LA SELECCION DEL COMBOBOX
function obtenerURL() {
    //OBTENENGO EL VALOR DEL CBB
    var cbb = $("#catalogo").find("option:selected").val();

    $("#Table").dataTable().fnDestroy();

    switch (cbb) {
        //BODEGAS
        case "0":
            DataTable("/Activaciones/getBodegas");
            break;
        //CATEGORIAS
        case "1":
            DataTable("/Activaciones/getCategoriasProducto");
            break;
        //TIPO ENTRADA
        case "2":
            DataTable("/Activaciones/getTiposDeEntrada");
            break;
        //UM
        case "3":
            DataTable("/Activaciones/getUnidadesDeMedida");
            break;
        //PRODUCTOS
        case "4":
            DataTable("/Activaciones/getProductos");
            break;
        //PROVEEDORES
        case "5":
            DataTable("/Activaciones/getProveedores");
            break;
        //PLATILLOS
        case "6":
            DataTable("/Activaciones/getCategoriasMenu");
            break;
        //CATEGORIASMENU
        case "7":
            //DataTable("/Activaciones/getMenus");
            cargarMenu("/Activaciones/getMenus");
            break;
        //MESEROS
        case "8":
            DataTable("/Activaciones/getMeseros");
            break;
        //CLIENTE
        case "9":
            DataTable("/Activaciones/getClientes");
            break;
        //TIPOS DE PAGO
        case "10":
            DataTable("/Activaciones/getTiposDeOrden");
            break;
        //MONEDAS
        case "11":
            DataTable("/Activaciones/getMesas");
            break;
        //TIPOS DE PAGO
        case "12":
            DataTable("/Activaciones/getTiposDePago");
            break;
        //MONEDAS
        case "13":
            DataTable("/Activaciones/getMonedas");
            break;
        default: console.log("Fin");
    }//FIN SWITCH
}//FIN FUNCTION 

//CARGAR DATATABLE
function DataTable(uri) {

    //CREAR URL DE BOTONES
    var uriEdit = "/" + uri.substr(17) + "/Edit/";

    dt = $("#Table").DataTable({
        ajax: {
            url: uri, //URL DE LA UBICACION DEL METODO
            type: "GET", //TIPO DE ACCION
            dataType: "JSON" //TIPO DE DATO A RECIBIR O ENVIAR
        },
        //DEFINIENDO LAS COLUMNAS A LLENAR
        columns: [
            { data: "Codigo" },
            { data: "Descripcion" },
            {   //DEFINIENDO LOS BOTONES PARA EDITAR Y ELIMINAR POR MEDIO DE JS
                data: "Id", "render": function (data) {
                    return "<a class='btn btn-success' onclick=CargarParcialCatalog('" + uriEdit + data + "')><i class='fa fa-pencil'></i></a>";
                }
            }
        ],//FIN DE COLUMNAS
        //IDIOMA DE DATATABLE
        "language": {

            "sProcessing": "Procesando...",
            "sLengthMenu": "Mostrar _MENU_ registros",
            "sZeroRecords": "No se encontraron resultados",
            "sEmptyTable": "Ningún dato se encuentra desactivado en este catálogo",
            "sInfo": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
            "sInfoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
            "sInfoFiltered": "(filtrado de un total de _MAX_ registros)",
            "sInfoPostFix": "",
            "sSearch": "Buscar:",
            "sUrl": "",
            "sInfoThousands": ",",
            "sLoadingRecords": "Cargando...",

            "oPaginate": {
                "sFirst": "Primero",
                "sLast": "Último",
                "sNext": "Siguiente",
                "sPrevious": "Anterior"
            }
        }//FIN IDIOMA DATATABLE
    });//FIN DECLARACION DEL DATATABLE
}//FIN FUNCTION

//REINICIALIZAR Y LIMPIAR DE DATOS TABLA DATATABLE
$("#catalogo").change(function () {
    crearTabla();
});

function limpiarDT() {
    //LIMPIAR DATOS DEL DATATABLE
    var table = $('#Table').DataTable();

    table
        .clear()
        .draw();

    //INICIALIZAR EL SELECT2 CATEGORIA
    $("#catalogo").val("0");
    $('#catalogo').trigger('change'); // Notify any JS components that the value changed
}

//CARGA TODOS LOS DATOS A LA VISTA
function CargarParcialCatalog(url) {
    $('#catalogo').prop('disabled', false);

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

    $('body').addClass("modal-open");

    $.ajax({
        "type": "GET", //TIPO DE ACCION
        "url": url, //URL DEL METODO A USAR
        success: function (parcial) {
            $("#VistaParcial").html(parcial);//CARGA LA PARCIAL CON ELEMENTOS QUE CONTEGA

            //DESACTIVAR TODOS LOS CAMPOS A MOSTRAR
            $('select').prop('disabled', true);

            //$('#catalogo').prop('readonly', false);
            $('#catalogo').prop('disabled', false);
            $("input").attr("readonly", true);
            $(".js-switch").attr("readonly", false);
            $(".input-sm").attr("readonly", false);

        }//FIN SUCCESS
    });//FIN AJAX    
}

//CARGA TODOS LOS DATOS CON VISTA DIFERENTE A DT DE MENUS
function cargarMenu(uri) {
    $("#Table_wrapper").remove();
    $("#Table").remove();

    var agregar = '<div class="row" id="menuAdd"></div>';

    $("#cargar").html(agregar);

    //CARGA TODOS LO PLATILLOS DEL MENU
    $.ajax({
        type: "GET",
        url: uri,
        dataType: "JSON",
        success: function (data) {
            var agregarPlatillo = "";
            if (data.length <= 0) {//SI NO HAY NINGUN ELEMENTO
                $("#filtro").attr("disabled", true);//DESHABILITAR EL INPUT FILTRAR

                agregarPlatillo = '<h2 id="txt" style="text-align:center;">Sin elementos disponibles</h2>';//AGREGA LETRERO
            } else {
                $("#filtro").removeAttr("disabled");//HABILITAR EL INPUT FILTRAR

                for (var i = 0; i < data.length; i++) {
                    agregarPlatillo += '<div class="col-md-55 items" id="' + data[i].Id + '">' +
                        '<div class="thumbnail">' +
                        '<div class="image view view-first">' +
                        '<img style="width: 100%; height:100%; display: block;" src="' + data[i].Imagen + '"alt="' + data[i].DescripcionPlatillo + '" />' +
                        '<div class="mask no-caption">' +
                        '<div class="tools tools-bottom">' +
                        '<a onclick=CargarParcial("/Menus/Edit/' + data[i].Id + '")><i class="fa fa-pencil"></i></a>' +
                        //'<a onclick=CargarParcial("/Menus/Details/' + data[i].Id + '")><i class="fa fa-eye"></i></a>' +
                        //'<a onclick=deleteAlert("/Menus/Delete/",' + data[i].Id + ')><i class="fa fa-trash"></i></a>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +
                        '<div class="caption" id="data">' +
                        '<p>' +
                        '<strong>' + data[i].DescripcionPlatillo + '</strong>' +
                        '</p>' +
                        '<p> $ ' + data[i].Precio + '</p>' +
                        '</div>' +
                        '</div >' +
                        '</div >';
                }//FIN FOR
            }//IF IF-ELSE

            //AGREGA LOS ELEMENTOS
            $("#menuAdd").html(agregarPlatillo);
        }//FIN SUCCESS
    });//FIN AJAX
}//FIN FUNCTION

//FUNCION PARA ELIMINAR EL DIV DEL MENU ENCASO DE QUE ESTE ACTIVO
function estadoMenu(estado, id) {
    //ELIMINAR SI EL REGISTRO ESTA DESACTIVADO
    if (estado == true) {
        $("#" + id).remove(); //ELIMINA EL DIV DEL PLATILLO
    }

    if ($("#menuAdd > div").length == 0) {
        $("#filtro").attr("disabled", true);//INHABILITAR EL INPUT FILTRAR
        //MOSTRAR QUE NO HAY ELEMENTOS DISPONIBLES
        agregarPlatillo = '<h2 id="txt" style="text-align:center;">Sin elementos disponibles</h2>';//AGREGA LETRERO
        $("#menuAdd").html(agregarPlatillo);
    }
}//FIN FUNCTION

//TIPO DE METODO A UTILIZAR
function comprobar() {
    //AGARRAR EL CODIGO DE LA VISTA
    var codigoMenu = $("#codigoMenu").val();

    $.ajax({
        type: "GET",
        url: "/Menus/Comprobar/",
        data: { Codigo: codigoMenu },
        success: function (data) {
            validar(data);
        }
    });
}//FIN FUNCTION

//FUNCION VALIDA QUE NO EXISTAN CAMPOS VACIOS
function validar(nuevo) {
    var a = $("#platillo").val();
    var img = $("#file")[0].files[0];
    var code = $("#codigoMenu").val();
    var precio = $("#precio").val();
    var ingredients = $("#ingredientes").val();

    //VALIDACIONES SHAMPOO
    if (code !== "" && a !== "" && precio !== "" && ingredients !== "") {
        editMenuItem();//METODO PARA EDITAR UN ELEMENTO
    } else {
        Alert("Error", "Campos vacios", "error");
    }//FIN VALIDACIONES IF-ELSE
    ///FIN IF !NUEVO
}

//FUNCION PARA ALMACENAR PLATILLO DEL MENU
function editMenuItem() {

    //SE CREA UN OBJETO DE LA CLASE FORMDATA
    var formData = new FormData();
    var a = $("#platillo").val();

    //USANDO EL METODO APPEND(CLAVE, VALOR) SE AGREGAN LOS PARAMETROS A ENVIAR
    formData.append("urlImage", $("#file")[0].files[0]);
    formData.append("codigoMenu", $("#codigoMenu").val());
    //ASIGNAR DESCRIPCION DE MENU CON LA PRIMERA LETRA EN MAYUSCULA
    formData.append("descripcionMenu", a.charAt(0).toUpperCase() + a.slice(1).toLowerCase());
    formData.append("precio", $("#precio").val());
    formData.append("categoriaId", $("#categoria").val());
    formData.append("tiempo", $("#tiempo").val());
    formData.append("ingredientes", $("#ingredientes").val());
    //TOMAR EL VALOR DEL SWITCHERY
    formData.append("estado", $("#activo").is(":checked"));
    formData.append("shot", $("#shot").is(":checked"));

    //FUNCION AJAX
    $.ajax({
        type: "POST",
        url: "/Menus/Edit",
        data: formData,//SE ENVIA TODO EL OBJETO FORMDATA CON LOS PARAMETROS EN EL APPEND 
        processData: false,
        contentType: false,
        success: function (data) {
            if (data.success) {
                $("#small-modal").modal("hide"); //CERRAR MODAL                

                modificarItem(data.Id);//MODIFICAR EL REGISTRO EN LA VISTA INDEX

                estadoMenu(data.estado, data.Id);

                AlertTimer("Completado", data.message, "success");
            } else
                Alert("Error", data.message, "error");//MENSAJE DE ERROR
        },
        error: function () {
            Alert("Error al modificar", "Intentelo de nuevo", "error");
        }
    });//FIN AJAX
}//FIN FUNCTION

//MODIFICAR EL DIV DEL ELEMENTO DEL MENU
function modificarItem(id) {
    $.ajax({
        type: "GET",
        url: "/Menus/getMenuItem/" + id,
        success: function (data) {
            //ASIGNAR NUEVOS VALORES EN LA VISTA
            $("#" + id).find("strong").text(data.menu.Platillo);
            $("#" + id).find("p:last-child").text("$ " + formatoPrecio(data.menu.Precio));
            $("#" + id).find("img").attr("src", "");
            $("#" + id).find("img").attr("src", data.menu.Ruta);
        },
        error: function () {
            Alert("Error al modificar", "Recargue la página", "error");
        }
    });//FIN AJAX      
}//FIN FUNCTION