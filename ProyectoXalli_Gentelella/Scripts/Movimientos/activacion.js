//INICIALIZAR AUTOMATICAMENTE EL DATATABLE
$(document).ready(function () {
    $("#Table").DataTable({
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
});

//CARGAR EL URL CON RESPECTO A LA SELECCION DEL COMBOBOX
function obtenerURL() {
    //OBTENENGO EL VALOR DEL CBB
    var cbb = $("#catalogo").find("option:selected").val();

    $("#Table").dataTable().fnDestroy();

    switch (cbb) {
        //BODEGAS
        case "0":
            DataTable("/Activar/getBodegas");
            break;
        //CATEGORIAS
        case "1":
            DataTable("/Activar/getCategoriasProducto");
            break;
        //TIPO ENTRADA
        case "2":
            DataTable("/Activar/getTiposDeEntrada");
            break;
        //TIPO SALIDA
        case "3":
            DataTable("/Activar/getTiposDeSalida");
            break;
        //UM
        case "4":
            DataTable("/Activar/getUnidadesDeMedida");
            break;
        //PRODUCTOS
        case "5":
            DataTable("/Activar/getProductos");
            break;
        //PROVEEDORES
        case "6":
            DataTable("/Activar/getProveedores");
            break;
        //PLATILLOS
        case "7":
            DataTable("/Activar/getMenus");
            break;
        //CATEGORIASMENU
        case "8":
            DataTable("/Activar/getCategoriasMenu");
            break;
        //MESEROS
        case "9":
            DataTable("/Activar/getMeseros");
            break;
        //TIPOS DE CLIENTE
        case "10":
            DataTable("/Activar/getTiposDeCliente");
            break;
        //CLIENTE
        case "11":
            DataTable("/Activar/getClientes");
            break;
        //TIPOS DE PAGO
        case "12":
            DataTable("/Activar/getTiposDePago");
            break;        
        default: console.log("Fin");
    }//FIN SWITCH
}//FIN FUNCTION 

//CARGAR DATATABLE
function DataTable(uri) {       

    //CREAR URL DE BOTONES
    var uriEdit = "/" + uri.substr(12) + "/Edit/";
    var uriDetails = "/" + uri.substr(12) + "/Details/";

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
                    return "<a class='btn btn-success' onclick=CargarParcial('" + uriEdit + data + "')><i class='fa fa-pencil'></i></a>" +
                        "<a class='btn btn-primary' Style='margin-left: 3px' onclick=CargarParcial('" + uriDetails + data + "')><i class='fa fa-eye'></i></a>";
                }
            }
        ],//FIN DE COLUMNAS
        //IDIOMA DE DATATABLE
        "language": {

            "sProcessing": "Procesando...",
            "sLengthMenu": "Mostrar _MENU_ registros",
            "sZeroRecords": "No se encontraron resultados",
            "sEmptyTable": "Ningún dato se encuentra desactivado en esta tabla",
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
    //LIMPIAR DATOS DEL DATATABLE
    var table = $('#Table').DataTable();

    table
        .clear()
        .draw();
});