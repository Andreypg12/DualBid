"use strict";

/**
 * SignalRAuction — Helper para páginas de subasta.
 *
 * Si ya existe una conexión global (window.__globalSignalRConnection),
 * la reutiliza en lugar de crear una nueva — así no hay dos WebSockets abiertos.
 */
const SignalRAuction = (() => {
    let connection = null;
    let currentAuctionId = null;
    let currentUserId = null;
    let isConnecting = false;

    let _countdownInterval = null;
    let _endDate = null;

    const callbacks = {
        onNewBid: [],
        onUserOutbid: [],
        onConnected: [],
        onReconnected: [],
        onDisconnected: [],
        onAuctionClosed: [],
        onAuctionActivated: [],
        onYouWon: [],
        onYourAuctionEnded: [],
        onCountdownTick: []
    };

    // ─────────────────────────────────────────
    // Inicialización
    // ─────────────────────────────────────────

    async function initialize(auctionId, userId, endDateIso = null) {
        currentAuctionId = auctionId;
        currentUserId = userId;

        if (endDateIso) startCountdown(new Date(endDateIso));

        // Reutilizar la conexión global del layout si existe y está conectada
        if (window.__globalSignalRConnection &&
            window.__globalSignalRConnection.state === signalR.HubConnectionState.Connected) {
            connection = window.__globalSignalRConnection;
            console.log("SignalR: Reutilizando conexión global");
            _registerHandlers();
            await _joinGroups();
            callbacks.onConnected.forEach(cb => cb());
            return connection;
        }

        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            return connection;
        }

        if (isConnecting) {
            await new Promise(resolve => {
                const t = setInterval(() => { if (!isConnecting) { clearInterval(t); resolve(); } }, 100);
            });
            return connection;
        }

        isConnecting = true;

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/auctionHub")
            .withAutomaticReconnect([2000, 5000, 10000, 30000])
            .build();

        _registerHandlers();

        connection.onreconnected(async () => {
            console.log("SignalR: Reconectado");
            await _joinGroups();
            callbacks.onReconnected.forEach(cb => cb());
        });

        connection.onclose(error => {
            callbacks.onDisconnected.forEach(cb => cb(error));
        });

        try {
            await connection.start();
            console.log("✅ SignalR conectado (auctionDetails)");
            await _joinGroups();
            callbacks.onConnected.forEach(cb => cb());
        } catch (err) {
            console.error("❌ Error de conexión SignalR:", err);
            connection = null;
        } finally {
            isConnecting = false;
        }

        return connection;
    }

    function _registerHandlers() {
        // Evitar registrar handlers duplicados si se reutiliza la conexión global
        if (connection._auctionHandlersRegistered) return;
        connection._auctionHandlersRegistered = true;

        connection.on("NuevaPujaSimulada", data => {
            callbacks.onNewBid.forEach(cb => cb(data));
        });

        connection.on("UsuarioSuperado", data => {
            callbacks.onUserOutbid.forEach(cb => cb(data));
        });

        connection.on("AuctionClosed", data => {
            stopCountdown();
            callbacks.onAuctionClosed.forEach(cb => cb(data));
        });

        connection.on("AuctionActivated", data => {
            if (data.endDate) startCountdown(new Date(data.endDate));
            callbacks.onAuctionActivated.forEach(cb => cb(data));
        });

        connection.on("YouWonAuction", data => {
            callbacks.onYouWon.forEach(cb => cb(data));
        });

        connection.on("YourAuctionEnded", data => {
            callbacks.onYourAuctionEnded.forEach(cb => cb(data));
        });

        connection.on("YourAuctionActivated", data => {
            callbacks.onAuctionActivated.forEach(cb => cb(data));
        });
    }

    async function _joinGroups() {
        try {
            if (currentAuctionId) {
                await connection.invoke("JoinAuctionGroup", currentAuctionId.toString());
                console.log(`📌 Unido a auction-${currentAuctionId}`);
            }
            if (currentUserId && currentUserId !== "0") {
                await connection.invoke("RegisterUser", currentUserId.toString());
                console.log(`👤 Usuario ${currentUserId} registrado`);
            }
        } catch (err) {
            console.error("SignalR: Error al unirse a grupos:", err);
        }
    }

    // ─────────────────────────────────────────
    // Countdown local
    // ─────────────────────────────────────────

    function startCountdown(endDate) {
        stopCountdown();
        _endDate = endDate;

        _countdownInterval = setInterval(() => {
            const diffSeconds = Math.floor((_endDate - new Date()) / 1000);

            if (diffSeconds <= 0) {
                stopCountdown();
                callbacks.onCountdownTick.forEach(cb => cb({ seconds: 0, display: "00:00:00", isEnded: true }));
                return;
            }

            const h = Math.floor(diffSeconds / 3600);
            const m = Math.floor((diffSeconds % 3600) / 60);
            const s = diffSeconds % 60;

            callbacks.onCountdownTick.forEach(cb => cb({
                seconds: diffSeconds,
                display: `${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`,
                isEnded: false,
                isEndingSoon: diffSeconds <= 300
            }));
        }, 1000);
    }

    function stopCountdown() {
        if (_countdownInterval) { clearInterval(_countdownInterval); _countdownInterval = null; }
    }

    // ─────────────────────────────────────────
    // Callbacks
    // ─────────────────────────────────────────

    const _on = (key, cb) => { if (typeof cb === "function") callbacks[key].push(cb); };

    return {
        initialize,
        startCountdown,
        stopCountdown,
        onNewBid: cb => _on("onNewBid", cb),
        onUserOutbid: cb => _on("onUserOutbid", cb),
        onConnected: cb => _on("onConnected", cb),
        onReconnected: cb => _on("onReconnected", cb),
        onDisconnected: cb => _on("onDisconnected", cb),
        onAuctionClosed: cb => _on("onAuctionClosed", cb),
        onAuctionActivated: cb => _on("onAuctionActivated", cb),
        onYouWon: cb => _on("onYouWon", cb),
        onYourAuctionEnded: cb => _on("onYourAuctionEnded", cb),
        onCountdownTick: cb => _on("onCountdownTick", cb),
        getAuctionId: () => currentAuctionId,
        isConnected: () => connection && connection.state === signalR.HubConnectionState.Connected,
        async disconnect() {
            stopCountdown();
            // No cerrar si es la conexión global compartida
            if (connection && connection !== window.__globalSignalRConnection) {
                try { await connection.stop(); connection = null; } catch { }
            }
        }
    };
})();