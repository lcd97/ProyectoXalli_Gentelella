$(document).ready(function () {

    //BUSCAR EL ROL DEL EMPLEADO LOGGEADO
    var loginId = $("#session").val();

    $.ajax({
        type: "GET",
        url: "/Account/userProfileData/",
        data: { empleado: loginId },
        success: function (data) {
            if (data.dataProfile == null) {
                Alert("Error", "Inicie sesión con credenciales válidas", "error");
            } else {
                var nombre = data.dataProfile.Nombre;
                var apellido = data.dataProfile.Apellido;

                $("#cedula").val(data.dataProfile.Cedula);
                $("#cedula").attr("val", data.dataProfile.ColaboradorId);
                $("#inss").val(data.dataProfile.INSS);
                $("#ruc").val(data.dataProfile.RUC);
                $("#nombre").val(nombre);
                $("#apellido").val(apellido);
                $("#correo").val(data.colaborador.Correo);
                $("#entrada").val(data.dataProfile.Entrada);
                $("#salida").val(data.dataProfile.Salida);
                $("#userName").val(data.colaborador.UserName);

                var general = '<h2 class="title" id="nombreCol" style="text-align: center;">' + nombre + ' ' + apellido + '</h2>' +
                    '<p style="margin-top:-5px !important;text-align: center;" id="roleId" val="' + data.colaborador.UserId + '"><i class="fa fa-circle green"></i>&nbsp; ' + data.colaborador.Role + '</p>';

                $("#generalData").append(general);


                barChart(data.colaborador.Role, data.dataProfile.ColaboradorId);
            }
        }
    });//FIN AJAX
});//FIN DOCUMENT READY

function barChart(role, mesero) {
    //OBTENER LAS ESTADISTICAS DEL COLABORADOR
    $.ajax({
        type: "GET",
        url: "/Busquedas/StatticsRole/",
        data: { Role: role, MeseroId: mesero },
        success: function (data) {
            var lineData = "";
            var agPorcOrd = "", agPorcVent = "";

            //SI EL PROCENTAJE ES DE CRECIMIENTO
            if (data.porcOrdenes > 0) {
                agPorcOrd = '<i class="green"><i class="fa fa-sort-asc"></i>' + (data.porcOrdenes * -1).toFixed() + '%</i> más que el mes pasado';
                $("#cbOrd").html(agPorcOrd);
            } else {
                agPorcOrd = '<i class="red"><i class="fa fa-sort-desc"></i>' + (data.porcOrdenes * -1).toFixed() + '%</i> menos que el mes pasado';
                $("#cbOrd").html(agPorcOrd);
            }

            //SI EL PROCENTAJE ES DE CRECIMIENTO
            if (data.porcVentas > 0) {
                agPorcVent = '<i class="green"><i class="fa fa-sort-asc"></i>' + (data.porcVentas * -1).toFixed() + '%</i> más que el mes pasado';
                $("#cbVenta").html(agPorcVent);
            } else {
                agPorcVent = '<i class="red"><i class="fa fa-sort-desc"></i>' + (data.porcVentas * -1).toFixed() + '%</i> menos que el mes pasado';
                $("#cbVenta").html(agPorcVent);
            }

            $("#ordMes").html(data.recOrd);
            $("#ventaMes").html(formatoPrecio(data.recVenta.toString()));

            if (data.bodegas) {//SI LA CONSULTA ES POR BODEGAS

                //var dashBar = '<i class="fa fa-usd"></i> Ventas totales del mes en bar';
                //var dashCocina = '<i class="fa fa-usd"></i> Ventas totales del mes en cocina';

                //$("#txtMesBar").html(dashBar);
                //$("#txtMesCocina").html(dashCocina);

                $("#dashboard").remove();
                document.getElementById("barStattics").className = "col-md-12 col-sm-12 col-xs-12";

                //CREO LOS ARREGLOS PARA ALMACENAR LOS REGISTROS
                var fecha = new Array();
                var cocina = new Array();
                var bar = new Array();

                //RECORRO
                for (var i = 0; i < data.ordenes.length; i++) {
                    var itemFecha = data.ordenes[i].Fecha;
                    var itemVentaCocina = formatoPrecio(data.ordenes[i].TVCocina.toString());
                    var itemVentaBar = formatoPrecio(data.ordenes[i].TVBar.toString());

                    fecha.push(itemFecha);
                    cocina.push(itemVentaCocina);
                    bar.push(itemVentaBar);
                }

                lineData = {
                    labels: fecha,
                    datasets: [
                        {
                            label: "Cocina ",
                            backgroundColor: 'rgba(26,179,148,0.5)',
                            borderColor: "rgba(26,179,148,0.7)",
                            pointBackgroundColor: "rgba(26,179,148,1)",
                            pointBorderColor: "#fff",
                            data: cocina
                        },
                        {
                            label: "Bar ",
                            backgroundColor: 'rgba(26,179,148,0.5)',
                            borderColor: "rgba(26,179,148,0.7)",
                            pointBackgroundColor: "rgba(26,179,148,1)",
                            pointBorderColor: "#fff",
                            data: bar
                        }
                    ]
                };
            } else {
                var fechaVenta = new Array();
                var ventas = new Array();

                for (var j = 0; j < data.ordenes.length; j++) {
                    var itemF = data.ordenes[j].Fecha;
                    var itemV = formatoPrecio(data.ordenes[j].TotalVentas.toString());

                    fechaVenta.push(itemF);
                    ventas.push(itemV);
                }

                lineData = {
                    labels: fechaVenta,
                    datasets: [
                        {
                            label: "Ventas $ ",
                            backgroundColor: 'rgba(26,179,148,0.5)',
                            borderColor: "rgba(26,179,148,0.7)",
                            pointBackgroundColor: "rgba(26,179,148,1)",
                            pointBorderColor: "#fff",
                            data: ventas
                        }
                    ]
                };
            }

            var lineOptions = {
                responsive: true,
                //LAS ESCALAS INICIARAN EN CERO, EN CASO QUE NO HAYA VALORES
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true
                        }
                    }]
                }
            };

            var ctx = document.getElementById("barChart").getContext("2d");
            new Chart(ctx, { type: 'bar', data: lineData, options: lineOptions });
        }//FIN SUCCESS
    });
}

function editProfile() {
    var colaboradorId = $("#cedula").attr("val");
    var roleId = $("#roleId").attr("val").trim();
    var nombre = $("#nombre").val().trim();
    var apellido = $("#apellido").val().trim();
    var correo = $("#correo").val().trim();
    var ruc = $("#ruc").val().trim();

    var datos = "colaboradorId=" + colaboradorId + "&userId=" + roleId + "&nombreCol=" + nombre + "&apellidoCol=" + apellido + "&correo=" + correo + "&ruc=" + ruc;
    //alert(datos);

    if (validarVacios()) {
        $.ajax({
            type: "POST",
            url: "/Account/EditProfile/",
            dataType: "JSON",
            data: datos,
            success: function (data) {
                if (data.success) {
                    AlertTimer("Completado", data.message, "success");
                    $("#nombreCol").html(data.Nombre);
                } else {
                    Alert("Error", data.message, "error");
                }
            }
        });
    } else {
        Alert("Error", "Campos nombre y apellido se encuentran vacios", "error");
    }

}

//MASCARA PARA EL NUMERO RUC
$("#ruc").mask("A00CDEF000000B", {
    translation: {
        'A': { pattern: /[0-6]/ },//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
        'B': { pattern: /[A-Za-z]/ },//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
        'C': { pattern: /[0-3]/ },//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
        'D': { pattern: /[0-9]/ },//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
        'E': { pattern: /[0-1]/ },//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
        'F': { pattern: /[0-9]/ },//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
        /*
         FORMATO DE RUC - PRIMERA LETRA
         PERSONA JURIDICA : J
         PERSONA NATURAL SIN CEDULA : N
         PERSONA NATURAL CON CEDULA : NUMERO DE CEDULA ( 0 - 6 )
         PERSONA RESIDENTE : R
         PERSONA NO RESIDENTE : E
         */
    }
});

//FUNCION PARA HACER EL CRUD A BODEGA POR MEDIO DEL MODAL (RECIBE UN FORM = FORMULARIO)
function SubmitFormPassword(form) {

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        $.ajax({
            type: "POST", //TIPO DE ACCION
            url: form.action, //ACCION O METODO A REALIZAR
            data: $(form).serialize(), //SERIALIZACION DE LOS DATOS A ENVIAR
            success: function (data) {
                if (data.success) {//SI SE REALIZO CORRECTAMENTE
                    //REDIRECCIONAR AL LOGIN
                    var url = "/Account/Login/?returnUrl=" + " " + "&mensaje=" + data.message;
                    window.location.href = url;
                }//FIN IF
                else {
                    //MANDAR EL ERROR DEL CONTROLADOR
                    Alert("Error", data.message, "error");
                }
            },//FIN SUCCESS
            error: function () {
                //AQUI MANDAR EL MENSAJE DE ERROR
                Alert("Error al almacenarlo", "Intentelo de nuevo", "error");
            }//FIN ERROR
        });//FIN AJAX
    }//FIN DEL IF FORM VALID
    return false; //EVITA SALIRSE DEL METODO ACTUAL
}//FIN FUNCTION

function validarVacios() {
    var nom = $("#nombre").val().trim();
    var ape = $("#apellido").val().trim();

    if (nom != "" && ape != "") {
        return true;
    } else
        return false;
}