$(document).ready(function () {
    limpiarPantalla();
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

//LIMPIA LA PANTALLA
function limpiarPantalla() {
    $("#cedula").val("");
    $("#inss").val("");
    $("#ruc").val("");
    $("#nombre").val("");
    $("#apellido").val("");
    $("#entrada").val("");
    $("#salida").val("");
    $("#username").val("");
    $("#password").val("");

    //LIMPIAR SELECT 2
    $('.js-example-basic-single').val('');
    $('.js-example-basic-single').trigger('change'); // Notify any JS components that the value changed
}

$('#hentrada').datetimepicker({
    format: 'hh:mm A'
});

$('#hsalida').datetimepicker({
    format: 'hh:mm A'
});

//MASCARA TIPO HORA
$('#entrada').mask("01:21 AM", {
    translation: {
        '0': { pattern: /[0-1]/ },
        '1': { pattern: /[0-2]/ },
        '2': { pattern: /[0-5]/ },
        'A': { pattern: /[AaPp]/ },
        'M': { pattern: /[Mm]/ }
    }
});

//MASCARA TIPO HORA
$('#salida').mask("01:21 AM", {
    translation: {
        '0': { pattern: /[0-1]/ },
        '1': { pattern: /[0-2]/ },
        '2': { pattern: /[0-5]/ },
        'A': { pattern: /[AaPp]/ },
        'M': { pattern: /[Mm]/ }
    }
});

//MASCARA DE NUMERO TELEFONICO
$("#inss").mask("0000000-0");

//MASCARA PARA NUMERO TELEFONICO
$("#telefono").mask("0000-0000");

//MASCARA PARA EL NUMERO RUC
$("#ruc").mask("A000000000000B", {
    translation: {
        'A': { pattern: /[0-6-EeJjNnRr]/ },//MODIFICAR PARA QUE ACEPTE NUMEROS Y LETRAS
        'B': { pattern: /[0-9-A-Za-z]/ }//MODIFICAR PARA QUE ACEPTE NUMEROS Y LETRAS
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

//MASCARA PARA NUMERO DE CEDULA
$("#cedula").mask("A00-000000-0000B", {
    translation: {
        'A': { pattern: /[0-6]/ },//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
        'B': { pattern: /[A-Za-z]/ }//MODIFICAR EL ULTIMO DIGITO A SOLO LETRA
    }
});

//BUSQUEDA DE EMPLEADO QUE NO ESTE REGISTRADO
$("#cedula").blur(function () {
    var dni = $(this).val();

    if (dni != "") {
        $.ajax({
            type: "GET",
            url: "/Meseros/BuscarColaborador/",
            data: {
                Identificacion: dni
            },
            success: function (data) {
                if (data.length != 0)
                    Alert("Error", "Ya existe el personal ingresado", "error");
            }
        });
    } else {
        limpiarPantalla();
    }
});

//GENERAR EL NOMBRE DE USUARIO
$("#nombre").blur(function () {
    var valueName = ($(this).val()).substring(0, 5);//AGARRA EL NOMBRE
    var valueId = ($("#cedula").val()).substring(4, 10);

    var userName = (valueName.split(" ")[0]).toLowerCase() + valueId;

    $("#username").val(userName);

    if (userName != "") {
        var pass = generatePasswordRand();
        $("#password").val(pass);
    }
});

//GENERA LA CONTRASEÑA ALEATORIAMENTE
function generatePasswordRand() {

    characters = "0123456789";

    var pass = "";
    for (i = 0; i < 6; i++) {
        pass += characters.charAt(Math.floor(Math.random() * characters.length));
    }
    return pass;
}

function crearAcceso() {
    var nombres = $("#nombre").val(), apellidos = $("#apellido").val(), cedula = $("#cedula").val().toUpperCase(), inss = $("#inss").val(),
        ruc = $("#ruc").val().toUpperCase(), hentrada = $("#entrada").val(), hsalida = $("#salida").val();

    if (validado() == true) {
        $.ajax({
            type: "POST",
            url: "/Meseros/Create",
            data:
                "Nombres=" + nombres + "&Apellido=" + apellidos + "&Cedula=" + cedula + "&INSS=" + inss + "&RUC=" + ruc +
                "&HoraEntrada=" + hentrada + "&HoraSalida=" + hsalida
            ,
            dataType: "JSON",
            success: function (data) {
                //alert(data.meseroId);

                Acceso(data.meseroId);
            },
            error: function () {
                Alert("Error", "Revisar", "error");
            }
        });
    } else
        Alert("Error", "Campos vacios", "error");
}

function Acceso(meseroId) {

    //var account = new Object();

    //account.Username = $("#username").val();
    //account.RoleName = $("#role").val();
    //account.Password = $("#password").val();
    //account.ConfirmPassword = $("#password").val();
    //account.PeopleId = meseroId;

    var Username = $("#username").val();
    var RoleName = $("#role").val();
    var Password = $("#password").val();
    var ConfirmPassword = $("#password").val();
    var PeopleId = meseroId;


    //alert(Username + " " + RoleName + " " + Password + " " +ConfirmPassword + " " + PeopleId);

    if (meseroId != -1) {
        $.ajax({
            type: "POST",
            url: "/Account/Register/",
            data: {
                Username: Username, Password: Password, PasswordConfirmed: ConfirmPassword, RoleName: RoleName, PeopleId: PeopleId
            },
            dataType: "JSON",
            success: function (data) {
                if (data) {
                    AlertTimer("Completado", "Almacenado correctamente", "success");
                    limpiarPantalla();
                } else {
                    Alert("Error", "Es posible que el no se haya creado las credenciales del colaborador", "error");
                }
            },
            error: function (error) {
                console.log("AJAX error in request: " + JSON.stringify(error, null, 2));
            }
        });
    } else {
        Alert("Error", "No se crearon las credenciales. Intentelo de nuevo", "error");
    }
}