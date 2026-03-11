"use strict";

(() => {
    if (!window.auctionDetailsRealtimeConfig) return;

    const auctionId = window.auctionDetailsRealtimeConfig.auctionId;
    const userId = window.auctionDetailsRealtimeConfig.userId;

    console.log("Details realtime config:", { auctionId, userId });

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/auctionHub")
        .withAutomaticReconnect()
        .build();

    connection.on("NuevaPujaSimulada", function (data) {
        console.log("NuevaPujaSimulada recibida en Details:", data);

        const currentBidLabel = document.getElementById("currentBidLabel");
        if (currentBidLabel) {
            currentBidLabel.textContent = Number(data.nuevoMonto).toLocaleString();
        }

        const totalBidsLabel = document.getElementById("totalBidsLabel");
        if (totalBidsLabel) {
            const current = parseInt(totalBidsLabel.textContent) || 0;
            totalBidsLabel.textContent = current + 1;
        }
    });

    connection.on("UsuarioSuperado", function (data) {
        console.log("UsuarioSuperado recibido en Details:", data);
        /*alert(data.mensaje);*/
    });

    connection.start()
        .then(async function () {
            console.log("Conectado a SignalR desde Details");

            await connection.invoke("JoinAuctionGroup", auctionId);
            console.log("Unido al grupo auction-" + auctionId);

            await connection.invoke("RegisterUser", userId.toString());
            console.log("Usuario registrado user-" + userId);
        })
        .catch(function (err) {
            console.error("Error SignalR Details:", err.toString());
        });
})();