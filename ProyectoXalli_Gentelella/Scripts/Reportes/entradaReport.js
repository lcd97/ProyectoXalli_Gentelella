$(".js-example-basic-single").select2({
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

$(document).ready(function () {

    $("#busqueda").val("1");
    $('#busqueda').trigger('change'); // Notify any JS components that the value changed

    $("#bod").val("");
    $('#bod').trigger('change'); // Notify any JS components that the value changed


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


$("#busqueda").on("change", function () {
    var option = $(this).val();
    var agregar = "";

    $("#tipoBusq").empty();

    if (option == 2) {
        //PROVEEDOR
        agregar = '<br /><br /><br />' +
            '<div class="col-md-6 col-sm-4 col-xs-12">' +
            '<label>Proveedor</label>' +
            '<select class="js-example-basic-single select2_group form-control" id="proveedor">' +
            '<option value="">Seleccione una opción</option>' +
            '</select>' +
            '</div>' +

            '<div class="col-md-2 col-sm-4 col-xs-4">' +
            '<button type="button" id="btnBuscarOrd" onclick="buscarEnt()" style="margin-top: 25px !important;" class="btn btn-primary">Cargar entradas</button>' +
            '</div>';

        $("#tipoBusq").append(agregar);

        $("#proveedor").select2({
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

        cargarProveedor();
    } else if (option == 3) {
        //FECHAS

        agregar = '<br /><br /><br />' +
            '<div class="col-md-6 col-sm-4 col-xs-12">' +
            '<label>Elegir fecha</label>' +
            '<div class="input-prepend input-group">' +
            '<span class="add-on input-group-addon"><i class="glyphicon glyphicon-calendar fa fa-calendar"></i></span>' +
            '<input type="text" id="daterange" name="datefilter" class="form-control" />' +
            '</div>' +
            '</div>' +

            '<div class="col-md-2 col-sm-4 col-xs-4">' +
            '<button type="button" id="btnBuscarOrd" onclick="buscarEnt()" style="margin-top: 25px !important;" class="btn btn-primary">Cargar entradas</button>' +
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

function buscarEnt() {
    var filtro = $("#busqueda").val();
    var bodega = $("#bod").val();

    if (bodega == "") {
        Alert("Error", "Seleccione una bodega", "error");
    } else {

        $("#Table").dataTable().fnDestroy();
        //OBTENER FILTRO    
        var proveedor = "", fechaInit = "", fechaFin = "";

        if (filtro == "2") {
            //BUSQUEDA POR CLIENTE
            proveedor = $("#proveedor").val();
        } else if (filtro == "3") {
            //BUSQUEDA POR FECHAS
            var split = $("#daterange").val().split(' ');
            fechaInit = split[0];
            fechaFin = split[2];
        }


        $("#Table").DataTable({
            ajax: {
                url: "/Ingresos/CargarEntradas/", //URL DE LA UBICACION DEL METODO
                type: "GET", //TIPO DE ACCION
                data: { proveedorId: proveedor, fechaInic: fechaInit, fechaFin: fechaFin, bodega: bodega },
                dataType: "JSON" //TIPO DE DATO A RECIBIR O ENVIAR
            },
            //DEFINIENDO LAS COLUMNAS A LLENAR
            columns: [
                { data: "Codigo" },
                { data: "Fecha" },
                { data: "Proveedor" },
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
}

function cargarProveedor() {
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
}

function cargarPDF(id) {
    window.open("/Ingresos/GenerarEntrada/" + id, '_blank');
}