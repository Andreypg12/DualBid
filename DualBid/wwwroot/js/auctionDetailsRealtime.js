"use strict"; // Modo estricto de JavaScript (evita errores comunes)

(() => {

    // Si no existe la configuración enviada desde Razor, no hace nada
    if (!window.auctionDetailsRealtimeConfig) return;

    // Obtener datos enviados desde la vista Details.cshtml
    const auctionId = window.auctionDetailsRealtimeConfig.auctionId;
    const userId = window.auctionDetailsRealtimeConfig.userId;

    console.log("Details realtime config:", { auctionId, userId });

    // Crear conexión con SignalR hacia el Hub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/auctionHub") // URL del hub configurado en Program.cs
        .withAutomaticReconnect() // reconectar automáticamente si se pierde conexión
        .build();


    // =============================
    // Evento cuando llega nueva puja
    // =============================
    connection.on("NuevaPujaSimulada", function (data) {

        console.log("NuevaPujaSimulada recibida en Details:", data);

        // Actualizar el monto actual en pantalla
        const currentBidLabel = document.getElementById("currentBidLabel");

        if (currentBidLabel) {

            // Convertimos el número a formato bonito con comas
            currentBidLabel.textContent =
                "$" + Number(data.nuevoMonto).toLocaleString();
        }

        // Actualizar contador de pujas
        const totalBidsLabel =
            document.getElementById("totalBidsLabel");

        if (totalBidsLabel) {

            const current =
                parseInt(totalBidsLabel.textContent) || 0;

            totalBidsLabel.textContent = current + 1;
        }

    });


    // =====================================
    // Evento cuando el usuario fue superado
    // =====================================
    connection.on("UsuarioSuperado", function (data) {

        console.log("UsuarioSuperado recibido en Details:", data);

        // Aquí se muestra la notificación
        Swal.fire({
            title: "Outbid!",
            html: `<b>${data.mensaje}</b>`,
            icon: "info",
            toast: false,
            position: "center",
            showConfirmButton: true,
            confirmButtonText: "Continue bidding",
            confirmButtonColor: "#198754"
        });

    });


    // =============================
    // Iniciar conexión SignalR
    // =============================
    connection.start()

        .then(async function () {

            console.log("Conectado a SignalR desde Details");


            // Unirse al grupo de la subasta
            await connection.invoke(
                "JoinAuctionGroup",
                auctionId
            );

            console.log(
                "Unido al grupo auction-" + auctionId
            );


            // Registrarse en el grupo del usuario
            await connection.invoke(
                "RegisterUser",
                userId.toString()
            );

            console.log(
                "Usuario registrado user-" + userId
            );

        })

        .catch(function (err) {

            console.error(
                "Error SignalR Details:",
                err.toString()
            );

        });

})();