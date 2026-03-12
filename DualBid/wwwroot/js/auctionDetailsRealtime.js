"use strict";

(() => {
    console.log("[DetailsRealtime] script cargado");

    if (!window.auctionDetailsRealtimeConfig) {
        console.warn("[DetailsRealtime] falta window.auctionDetailsRealtimeConfig");
        return;
    }

    console.log("[DetailsRealtime] config:", window.auctionDetailsRealtimeConfig);

    if (typeof signalR === "undefined") {
        console.error("[DetailsRealtime] signalR es undefined. Falta incluir signalr.min.js antes de este script.");
        return;
    }

    const auctionId = Number(window.auctionDetailsRealtimeConfig.auctionId || 0);
    const userId = Number(window.auctionDetailsRealtimeConfig.userId || 0);

    console.log("[DetailsRealtime] parsed:", { auctionId, userId });

    if (!auctionId || auctionId <= 0) {
        console.warn("[DetailsRealtime] auctionId inválido. Abortando.");
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/auctionHub")
        .withAutomaticReconnect()
        .build();

    // ✅ ACTUALIZAR UI
    connection.on("NuevaPujaSimulada", (data) => {
        console.log("[DetailsRealtime] NuevaPujaSimulada:", data);

        const currentBidLabel = document.getElementById("currentBidLabel");
        if (currentBidLabel && data?.nuevoMonto != null) {
            const formatted = new Intl.NumberFormat("en-US").format(data.nuevoMonto);
            currentBidLabel.textContent = `$${formatted}`;
        }

        const totalBidsLabel = document.getElementById("totalBidsLabel");
        if (totalBidsLabel && data?.totalBids != null) {
            totalBidsLabel.textContent = data.totalBids;
        }
    });

    connection.on("UsuarioSuperado", (data) => {
        console.log("[DetailsRealtime] UsuarioSuperado:", data);
    });

    connection
        .start()
        .then(async () => {
            console.log("[DetailsRealtime] connection started");

            await connection.invoke("JoinAuctionGroup", auctionId.toString());
            console.log("[DetailsRealtime] joined auction group", auctionId);

            if (userId > 0) {
                await connection.invoke("RegisterUser", userId.toString());
                console.log("[DetailsRealtime] registered user", userId);
            }
        })
        .catch((err) => {
            console.error("[DetailsRealtime] start error:", err);
        });
})();