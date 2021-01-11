//LLENANDO EL DATATABLE CRUD
$(document).ready(function () {

    $("#busqueda").val("1");
    $('#busqueda').trigger('change'); // Notify any JS components that the value changed


    //DEFINIENDO EL DATATABLE
    $("#Table").DataTable({
        //IDIOMA DE DATATABLE
        "language": {

            "sProcessing": "Procesando...",
            "sLengthMenu": "Mostrar _MENU_ registros",
            "sZeroRecords": "No se encontraron resultados",
            "sEmptyTable": "Ningún dato disponible en esta tabla",
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
});//FIN FUNCTION

$("#busqueda").select2({
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

$("#busqueda").on("change", function () {
    var option = $(this).val();
    var agregar = "";

    $("#tipoBusq").empty();

    if (option == 2) {
        //CLIENTE
        agregar = '<br /><br /><br />' +
            '<div class="col-md-4 col-sm-4 col-xs-12">' +
            '<label>Identificación</label>' +
            '<input id="identificacion" class="form-control" type="text" maxlength="16" placeholder="Incluya guiones si es necesario" autocomplete="off" style="text-transform:uppercase!important;"></div>' +
            '<div class="col-md-2 col-sm-4 col-xs-12" style="margin-top: 25px !important;">' +
            '<button type="button" id="btnCliente" class="btn btn-primary"><i class="fa fa-search"></i> Buscar cliente</button>' +
            '</div>' +
            '<div class="col-md-4 col-sm-4 col-xs-12">' +
            '<label>Cliente</label>' +
            '<input id="nombreCliente" class="form-control col-md-2 col-sm-2 col-xs-2" type="text" val="0" autocomplete="off" readonly>' +
            '</div>' +
            '<div class="col-md-2 col-sm-4 col-xs-4" style="margin-top: 25px !important;">' +
            '<button type="button" id="btnBuscarOrd" onclick="buscarFact()" class="btn btn-primary">Cargar facturas</button>' +
            '</div>';
        $("#tipoBusq").append(agregar);

        $("#btnCliente").attr("onclick", "CargarParcial('/Busquedas/BuscarCliente/')");

        //BUSCA EN LA BD EL REGISTRO DE UN CLIENTE
        $("#identificacion").blur(function () {
            $("#nombreCliente").val("");
            $("#nombreCliente").attr("val", "");

            if ($(this).val() != "") {
                $.ajax({
                    type: "GET",
                    url: "/Ordenes/DataClient/",
                    data: {
                        identificacion: $(this).val().trim()
                    },
                    dataType: "JSON",
                    success: function (data) {
                        $("#btnGuardarOrden").attr("disabled", false);

                        if (data.cliente != null) {
                            $("#nombreCliente").val(data.cliente.Nombres);
                            $("#nombreCliente").attr("val", data.cliente.ClienteId);
                        } else {
                            Alert("Error", "No existe registros con esa identificación", "error");
                        }
                    }
                });
            } else {//SI ESTA VACIO ELIMINAR ID CLIENTE EN CASO QUE EXISTA
                $("#nombreCliente").attr("val", "0");
                $("#nombreCliente").val("");
            }
        });
    } else if (option == 3) {
        //FECHAS

        agregar = '<br /><br /><br />' +
            '<div class="col-md-5 col-sm-4 col-xs-12">' +
            '<label>Elegir fecha</label>' +
            '<div class="input-prepend input-group">' +
            '<span class="add-on input-group-addon"><i class="glyphicon glyphicon-calendar fa fa-calendar"></i></span>' +
            '<input type="text" id="daterange" name="datefilter" class="form-control" />' +
            '</div>' +
            '</div>' +

            '<div class="col-md-2 col-sm-4 col-xs-4">' +
            '<button type="button" id="btnBuscarOrd" onclick="buscarFact()" style="margin-top: 25px !important;" class="btn btn-primary">Cargar facturas</button>' +
            '</div>';

        $("#tipoBusq").append(agregar);

        $(function () {

            var start = moment().subtract(10, 'days');
            var end = moment();

            function cb(start, end) {
                $('#reportrange span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
            }

            $('#daterange').daterangepicker({
                minDate: '01/01/2012',
                maxDate: '12/31/2030',
                showDropdowns: true,
                showWeekNumbers: true,
                timePicker: false,
                timePickerIncrement: 1,
                timePicker12Hour: true,
                startDate: start,
                endDate: end,
                opens: 'left',
                buttonClasses: ['btn btn-default'],
                applyClass: 'btn-small btn-primary',
                cancelClass: 'btn-small',
                separator: ' to ',
                locale: {
                    applyLabel: 'Elegir',
                    cancelLabel: 'Limpiar',
                    fromLabel: 'Desde',
                    toLabel: 'A',
                    weekLabel: 'S',
                    format: 'DD/MM/YYYY',
                    customRangeLabel: 'Personalizar',
                    daysOfWeek: ['Dom', 'Lun', 'Mar', 'Mie', 'Jue', 'Vie', 'Sab'],
                    monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
                    firstDay: 1
                }
            }, cb);

            cb(start, end);
        });

    }
});

function buscarFact() {
    $("#Table").dataTable().fnDestroy();
    //OBTENER FILTRO
    var filtro = $("#busqueda").val();
    var clienteId = "", fechaInit = "", fechaFin = "";

    if (filtro == "2") {
        //BUSQUEDA POR CLIENTE
        clienteId = $("#nombreCliente").attr("val");
    } else if (filtro == "3") {
        //BUSQUEDA POR FECHAS
        var split = $("#daterange").val().split(' ');
        fechaInit = split[0];
        fechaFin = split[2];
    }

    $("#Table").DataTable({
        ajax: {
            url: "/Facturas/cargarFacturas/", //URL DE LA UBICACION DEL METODO
            type: "GET", //TIPO DE ACCION
            data: { ClienteId: clienteId, fechaInic: fechaInit, fechaFin: fechaFin },
            dataType: "JSON" //TIPO DE DATO A RECIBIR O ENVIAR
        },
        //DEFINIENDO LAS COLUMNAS A LLENAR
        columns: [
            { data: "NumFact" },
            { data: "FechaFact" },
            { data: "Cliente" },
            {   //DEFINIENDO LOS BOTONES PARA EDITAR Y ELIMINAR POR MEDIO DE JS
                data: "Id", "render": function (data) {
                    return "<a class='btn btn-success' onclick='cargarPDF(" + data + ")'><i class='fa fa-file-pdf-o'></i> Ver</a>";
                }
            }
        ],//FIN DE COLUMNAS
        //IDIOMA DE DATATABLE
        "language": {

            "sProcessing": "Procesando...",
            "sLengthMenu": "Mostrar _MENU_ registros",
            "sZeroRecords": "No se encontraron resultados",
            "sEmptyTable": "No se encontraron registros",
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
}

function cargarPDF(id) {
    window.open("/Facturas/GenerarFactura/" + id, '_blank');
}