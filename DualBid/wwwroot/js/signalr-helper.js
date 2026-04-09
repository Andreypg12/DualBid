"use strict";

const SignalRAuction = (() => {
    let connection = null;
    let currentAuctionId = null;
    let currentUserId = null;
    let isConnecting = false;
    let callbacks = {
        onNewBid: [],
        onUserOutbid: [],
        onConnected: [],
        onReconnected: [],
        onDisconnected: [],
        onAuctionClosed: [],      // NUEVO
        onAuctionActivated: [],   // NUEVO
        onYouWon: [],             // NUEVO
        onYourAuctionEnded: []    // NUEVO
    };

    async function initialize(auctionId, userId) {
        currentAuctionId = auctionId;
        currentUserId = userId;

        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            console.log("SignalR: Conexión ya existente");
            return connection;
        }

        if (isConnecting) {
            console.log("SignalR: Esperando conexión en proceso...");
            await new Promise(resolve => {
                const checkInterval = setInterval(() => {
                    if (!isConnecting) {
                        clearInterval(checkInterval);
                        resolve();
                    }
                }, 100);
            });
            return connection;
        }

        isConnecting = true;

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/auctionHub")
            .withAutomaticReconnect([2000, 5000, 10000])
            .build();

        // Eventos existentes
        connection.on("NuevaPujaSimulada", (data) => {
            console.log("📨 Nueva puja:", data);
            callbacks.onNewBid.forEach(cb => cb(data));
        });

        connection.on("UsuarioSuperado", (data) => {
            console.log("📨 Usuario superado:", data);
            callbacks.onUserOutbid.forEach(cb => cb(data));
        });

        // NUEVOS EVENTOS
        connection.on("AuctionClosed", (data) => {
            console.log("🔒 Subasta cerrada:", data);
            callbacks.onAuctionClosed.forEach(cb => cb(data));
        });

        connection.on("AuctionActivated", (data) => {
            console.log("🎯 Subasta activada:", data);
            callbacks.onAuctionActivated.forEach(cb => cb(data));
        });

        connection.on("YouWonAuction", (data) => {
            console.log("🏆 ¡Ganaste la subasta!:", data);
            callbacks.onYouWon.forEach(cb => cb(data));
        });

        connection.on("YourAuctionEnded", (data) => {
            console.log("📊 Tu subasta terminó:", data);
            callbacks.onYourAuctionEnded.forEach(cb => cb(data));
        });

        connection.on("YourAuctionActivated", (data) => {
            console.log("🎯 Tu subasta fue activada:", data);
            // También usar el callback de activación
            callbacks.onAuctionActivated.forEach(cb => cb(data));
        });

        connection.on("TimeUpdate", (data) => {
            console.log("⏰ Actualización de tiempo:", data);
            // Se manejará en el frontend
        });

        connection.onreconnected((connectionId) => {
            console.log("SignalR: Reconectado");
            callbacks.onReconnected.forEach(cb => cb());
        });

        connection.onclose((error) => {
            console.log("SignalR: Desconectado", error);
            callbacks.onDisconnected.forEach(cb => cb(error));
        });

        try {
            await connection.start();
            console.log("✅ SignalR conectado");

            if (currentAuctionId) {
                await connection.invoke("JoinAuctionGroup", currentAuctionId.toString());
                console.log(`📌 Unido al grupo auction-${currentAuctionId}`);
            }

            if (currentUserId && currentUserId !== "0") {
                await connection.invoke("RegisterUser", currentUserId.toString());
                console.log(`👤 Usuario ${currentUserId} registrado`);
            }

            callbacks.onConnected.forEach(cb => cb());

        } catch (err) {
            console.error("❌ Error de conexión SignalR:", err);
            connection = null;
        } finally {
            isConnecting = false;
        }

        return connection;
    }

    // NUEVOS MÉTODOS DE CALLBACK
    function onAuctionClosed(callback) {
        if (typeof callback === 'function') {
            callbacks.onAuctionClosed.push(callback);
        }
    }

    function onAuctionActivated(callback) {
        if (typeof callback === 'function') {
            callbacks.onAuctionActivated.push(callback);
        }
    }

    function onYouWon(callback) {
        if (typeof callback === 'function') {
            callbacks.onYouWon.push(callback);
        }
    }

    function onYourAuctionEnded(callback) {
        if (typeof callback === 'function') {
            callbacks.onYourAuctionEnded.push(callback);
        }
    }

    // Métodos existentes...
    function onNewBid(callback) {
        if (typeof callback === 'function') {
            callbacks.onNewBid.push(callback);
        }
    }

    function onUserOutbid(callback) {
        if (typeof callback === 'function') {
            callbacks.onUserOutbid.push(callback);
        }
    }

    function onConnected(callback) {
        if (typeof callback === 'function') {
            callbacks.onConnected.push(callback);
        }
    }

    function onReconnected(callback) {
        if (typeof callback === 'function') {
            callbacks.onReconnected.push(callback);
        }
    }

    function getAuctionId() {
        return currentAuctionId;
    }

    function isConnected() {
        return connection && connection.state === signalR.HubConnectionState.Connected;
    }

    async function disconnect() {
        if (connection) {
            try {
                await connection.stop();
                connection = null;
                console.log("SignalR: Desconectado");
            } catch (err) {
                console.error("SignalR: Error al desconectar:", err);
            }
        }
    }

    return {
        initialize,
        onNewBid,
        onUserOutbid,
        onConnected,
        onReconnected,
        onAuctionClosed,      // NUEVO
        onAuctionActivated,   // NUEVO
        onYouWon,             // NUEVO
        onYourAuctionEnded,   // NUEVO
        getAuctionId,
        isConnected,
        disconnect
    };
})();