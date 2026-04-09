"use strict";

// Configuración global de SignalR para subastas
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
        onDisconnected: []
    };

    // Función para inicializar la conexión
    async function initialize(auctionId, userId) {
        currentAuctionId = auctionId;
        currentUserId = userId;

        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            console.log("SignalR: Conexión ya existente y conectada");
            return connection;
        }

        if (isConnecting) {
            console.log("SignalR: Ya hay una conexión en proceso, esperando...");
            // Esperar a que termine la conexión actual
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
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    // Reintentar después de 2, 5, 10 segundos
                    if (retryContext.previousRetryCount === 0) return 2000;
                    if (retryContext.previousRetryCount === 1) return 5000;
                    if (retryContext.previousRetryCount === 2) return 10000;
                    return null; // Dejar de reintentar después de 3 intentos
                }
            })
            .build();

        // Registrar eventos del hub
        connection.on("NuevaPujaSimulada", function (data) {
            console.log("SignalR: NuevaPujaSimulada recibida:", data);
            callbacks.onNewBid.forEach(cb => {
                try {
                    cb(data);
                } catch (err) {
                    console.error("Error en callback onNewBid:", err);
                }
            });
        });

        connection.on("UsuarioSuperado", function (data) {
            console.log("SignalR: UsuarioSuperado recibido:", data);
            callbacks.onUserOutbid.forEach(cb => {
                try {
                    cb(data);
                } catch (err) {
                    console.error("Error en callback onUserOutbid:", err);
                }
            });
        });

        // Manejar conexión exitosa
        connection.onreconnected((connectionId) => {
            console.log("SignalR: Reconectado, ID:", connectionId);
            callbacks.onReconnected.forEach(cb => cb());
        });

        connection.onreconnecting((error) => {
            console.log("SignalR: Reconectando...", error);
        });

        connection.onclose((error) => {
            console.log("SignalR: Conexión cerrada", error);
            callbacks.onDisconnected.forEach(cb => cb(error));
        });

        try {
            await connection.start();
            console.log("SignalR: Conectado exitosamente");

            // Unirse al grupo de la subasta
            if (currentAuctionId) {
                await connection.invoke("JoinAuctionGroup", currentAuctionId.toString());
                console.log(`SignalR: Unido al grupo auction-${currentAuctionId}`);
            }

            // Registrar usuario
            if (currentUserId && currentUserId !== "0") {
                await connection.invoke("RegisterUser", currentUserId.toString());
                console.log(`SignalR: Usuario ${currentUserId} registrado`);
            }

            callbacks.onConnected.forEach(cb => cb());

        } catch (err) {
            console.error("SignalR: Error de conexión:", err.toString());
            connection = null;
        } finally {
            isConnecting = false;
        }

        return connection;
    }

    // Función para suscribirse a nuevas pujas
    function onNewBid(callback) {
        if (typeof callback === 'function') {
            callbacks.onNewBid.push(callback);
        }
    }

    // Función para suscribirse a cuando superan al usuario
    function onUserOutbid(callback) {
        if (typeof callback === 'function') {
            callbacks.onUserOutbid.push(callback);
        }
    }

    // Función para suscribirse a conexión
    function onConnected(callback) {
        if (typeof callback === 'function') {
            callbacks.onConnected.push(callback);
        }
    }

    // Función para suscribirse a reconexión
    function onReconnected(callback) {
        if (typeof callback === 'function') {
            callbacks.onReconnected.push(callback);
        }
    }

    // Función para obtener el auction ID actual
    function getAuctionId() {
        return currentAuctionId;
    }

    // Función para verificar si está conectado
    function isConnected() {
        return connection && connection.state === signalR.HubConnectionState.Connected;
    }

    // Función para desconectar
    async function disconnect() {
        if (connection) {
            try {
                await connection.stop();
                connection = null;
                console.log("SignalR: Desconectado exitosamente");
            } catch (err) {
                console.error("SignalR: Error al desconectar:", err);
            }
        }
    }

    // API pública
    return {
        initialize,
        onNewBid,
        onUserOutbid,
        onConnected,
        onReconnected,
        getAuctionId,
        isConnected,
        disconnect
    };
})();