"use strict";

/**
 * auctionDetails.js — Lógica de la página de detalles de subasta.
 *
 * CAMBIOS vs versión anterior:
 * - Pasa config.endDate a SignalRAuction.initialize() para arrancar el countdown
 * - onCountdownTick actualiza el label sin recargar la página
 * - onAuctionClosed actualiza el DOM sin location.reload()
 */
(async () => {
    const config = window.auctionDetailsConfig;

    if (!config || !config.auctionId) {
        console.warn("AuctionDetails: No hay configuración disponible");
        return;
    }

    console.log("AuctionDetails: Inicializando para subasta", config.auctionId);

    try {
        // ✅ Pasar endDate para arrancar el countdown local
        await SignalRAuction.initialize(
            config.auctionId,
            config.userId,
            config.endDate   // ISO string, ej: "2026-04-12T03:00:00Z"
        );

        // ── Countdown local (sin llamadas al servidor) ────────────────────
        SignalRAuction.onCountdownTick(({ display, isEnded, isEndingSoon }) => {
            const label = document.getElementById("timeRemainingLabel");
            if (!label) return;

            if (isEnded) {
                label.innerHTML = '<span class="text-danger fw-bold">Ended</span>';
                return;
            }

            label.textContent = display;

            // Poner rojo en los últimos 5 minutos
            label.classList.toggle("text-danger", isEndingSoon);
            label.classList.toggle("text-warning", !isEndingSoon);
        });

        // ── Nueva puja ────────────────────────────────────────────────────
        SignalRAuction.onNewBid(bidData => {
            if (bidData.auctionId != config.auctionId) return;

            const currentBidLabel = document.getElementById("currentBidLabel");
            if (currentBidLabel) {
                currentBidLabel.innerText = `$${bidData.nuevoMonto.toFixed(2)}`;
                currentBidLabel.classList.add("highlight-update");
                setTimeout(() => currentBidLabel.classList.remove("highlight-update"), 500);
            }

            const totalBidsLabel = document.getElementById("totalBidsLabel");
            if (totalBidsLabel) {
                totalBidsLabel.innerText = (parseInt(totalBidsLabel.innerText) || 0) + 1;
            }

            showNotificationToast(`💸 New bid: $${bidData.nuevoMonto.toFixed(2)} by ${bidData.userName}`);
        });

        // ── Usuario superado ──────────────────────────────────────────────
        SignalRAuction.onUserOutbid(data => {
            if (typeof Swal !== "undefined") {
                Swal.fire({
                    icon: "warning",
                    title: "⚠️ You have been outbid!",
                    text: data.mensaje || `Someone placed a higher bid of $${data.nuevoMonto?.toFixed(2)}`,
                    toast: true,
                    position: "top-end",
                    showConfirmButton: false,
                    timer: 6000,
                    timerProgressBar: true,
                    background: "#fff3cd",
                    iconColor: "#ffc107"
                });
            } else {
                showNotificationToast(`⚠️ You've been outbid! New bid: $${data.nuevoMonto?.toFixed(2)}`);
            }
        });

        // ── Subasta cerrada (SIN location.reload()) ───────────────────────
        SignalRAuction.onAuctionClosed(data => {
            console.log("🔒 Subasta cerrada:", data);

            // 1. Deshabilitar botón de puja
            const bidButton = document.querySelector('a[href*="/Bid/Create"]');
            if (bidButton) {
                bidButton.outerHTML = `
                    <button class="btn btn-secondary btn-lg px-5" disabled>
                        <i class="bi bi-lock-fill me-2"></i>Auction Ended
                    </button>`;
            }

            // 2. Actualizar badge de estado
            updateAuctionStatusBadge(data);

            // 3. Mostrar resultado en el contenedor dedicado
            renderAuctionResult(data);

            // 4. Alerta (no bloquea, no recarga)
            if (typeof Swal !== "undefined") {
                Swal.fire({
                    icon: data.hasBids ? "success" : "info",
                    title: data.hasBids ? "🏆 Auction Ended!" : "📋 Auction Closed",
                    text: data.message,
                    confirmButtonText: "OK",
                    allowOutsideClick: true   // No bloquear al usuario
                });
            }
        });

        // ── Subasta activada ──────────────────────────────────────────────
        SignalRAuction.onAuctionActivated(data => {
            if (typeof Swal !== "undefined") {
                Swal.fire({
                    icon: "success",
                    title: "🚀 Auction Started!",
                    text: data.message,
                    timer: 3000,
                    showConfirmButton: true
                });
            }
            showNotificationToast(`🚀 ${data.message}`, "success");
        });

        // ── Ganaste ───────────────────────────────────────────────────────
        SignalRAuction.onYouWon(data => {
            data.comicTitle = data.comicTitle
                || document.querySelector(".h3.fw-bold.text-primary")?.textContent?.trim()
                || "Comic";
            data.finalAmount = data.finalAmount || data.amount || data.nuevoMonto;

            Swal.fire({
                icon: "success",
                title: "Congratulations!",
                html: `<p>${data.message || "You won this auction!"}</p>
                       <p><strong>Amount: $${Number(data.finalAmount)
                        .toLocaleString("en-US", { minimumFractionDigits: 2 })}</strong></p>`,
                confirmButtonText: "Pay now",
                confirmButtonColor: "#28a745"
            }).then(() => {
                PaymentFlow.openForWinner(data);
            });
        });

        // Cuando el ganador paga → muestra recibo a todos
        SignalRAuction.onPaymentCompleted(data => {
            const container = document.getElementById("auctionResultContainer");
            if (!container) return;
            container.innerHTML = `
                <div class="card border-0 shadow-sm mt-4" style="border-radius:16px;overflow:hidden;background:#fff;">
                    <div style="height:4px;background:linear-gradient(90deg,#1a7a4a,#2ecc7a);"></div>
                    <div class="p-4">
                        <div class="d-flex justify-content-between align-items-start mb-3">
                            <div>
                                <h5 class="fw-bold mb-1" style="font-size:18px;">Payment receipt</h5>
                                <p class="mb-0" style="font-size:13px;color:#999;">Payment registered successfully</p>
                            </div>
                            <span class="px-3 py-1 rounded-pill fw-semibold"
                                  style="font-size:12px;background:#e8f5ee;color:#1a7a4a;">Confirmed</span>
                        </div>
                        <hr style="border-color:#f0f0f0;">
                        <div class="d-flex justify-content-between align-items-center py-3" style="border-bottom:0.5px solid #f2f2f2;">
                            <span style="font-size:13px;color:#888;">Auction</span>
                            <span class="fw-bold" style="font-size:14px;">${escapeHtml(data.comicTitle)}</span>
                        </div>
                        <div class="d-flex justify-content-between align-items-center py-3" style="border-bottom:0.5px solid #f2f2f2;">
                            <span style="font-size:13px;color:#888;">Buyer</span>
                            <span class="fw-bold" style="font-size:14px;">${escapeHtml(data.winnerName)}</span>
                        </div>
                        <div class="d-flex justify-content-between align-items-center py-3" style="border-bottom:0.5px solid #f2f2f2;">
                            <span style="font-size:13px;color:#888;">Amount</span>
                            <span class="fw-bold" style="font-size:22px;color:#1a7a4a;">
                                $${Number(data.finalAmount).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                            </span>
                        </div>
                        <div class="d-flex justify-content-between align-items-center py-3">
                            <span style="font-size:13px;color:#888;">Date</span>
                            <span class="fw-bold" style="font-size:14px;">${escapeHtml(data.date)}</span>
                        </div>
                    </div>
                </div>`;
        });

        // Cuando el ganador libera el cómic → recarga para todos
        SignalRAuction.onComicReleased(data => {

            // Limpiar el resultado inmediatamente
            const container = document.getElementById("auctionResultContainer");
            if (container) container.innerHTML = "";

            Swal.fire({
                icon: "info",
                title: "Comic returned to auction",
                text: data.message,
                confirmButtonText: "OK"
            }).then(() => location.reload());
        });

        

        // ── Tu subasta terminó ────────────────────────────────────────────
        SignalRAuction.onYourAuctionEnded(data => {
            if (typeof Swal !== "undefined") {
                Swal.fire({
                    icon: data.hasBids ? "success" : "info",
                    title: "📋 Your Auction Has Ended",
                    text: data.message,
                    confirmButtonText: "OK"
                });
            }
        });

        // ── Reconexión ────────────────────────────────────────────────────
        SignalRAuction.onReconnected(() => {
            showNotificationToast("🔄 Connection restored", "info");
        });
        
        PaymentFlow.init(config.auctionId);

    } catch (err) {
        console.error("AuctionDetails: Error al inicializar SignalR:", err);
    }
})();

// ─────────────────────────────────────────────────────────────────────────────
// Helpers de UI
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Renderiza el resultado de la subasta en #auctionResultContainer.
 * No recarga la página — actualiza el DOM directamente.
 */
function renderAuctionResult(data) {
    const timeLabel = document.getElementById("timeRemainingLabel");
    if (timeLabel) {
        timeLabel.innerHTML = '<span class="text-danger fw-bold">Ended</span>';
    }

    const container = document.getElementById("auctionResultContainer");
    if (!container) return;

    const config = window.auctionDetailsConfig || {};

    if (data.winnerUserId) {
        const isWinner = parseInt(config.userId || "0") === data.winnerUserId;
        container.innerHTML = `
    <div class="card border-0 shadow-sm mt-4" style="border-radius:14px;overflow:hidden;background:#fff;border:0.5px solid #e2e4e8;">
        <div style="height:3px;background:#1a1a2e;"></div>
        <div class="p-4">
            <div class="d-flex justify-content-between align-items-center mb-4">
                <span style="font-size:11px;font-weight:600;letter-spacing:.1em;text-transform:uppercase;color:#9a9aaa;">
                    Auction result
                </span>
                <span style="font-size:11px;font-weight:600;padding:4px 12px;border-radius:20px;background:#f3f3f6;color:#5a5a7a;">
                    Pending payment
                </span>
            </div>
            <div class="d-flex justify-content-between align-items-center py-3" style="border-bottom:0.5px solid #f0f0f4;">
                <span style="font-size:13px;color:#9a9aaa;">Winner</span>
                <span style="font-size:15px;font-weight:600;color:#1a1a2e;">${escapeHtml(data.winnerName)}</span>
            </div>
            <div class="d-flex justify-content-between align-items-center py-3">
                <span style="font-size:13px;color:#9a9aaa;">Final price</span>
                <span style="font-size:24px;font-weight:600;color:#1a1a2e;">
                    $${Number(data.finalAmount).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                </span>
            </div>
            ${isWinner ? `
            <div style="display:flex;align-items:center;gap:12px;padding:14px 16px;background:#f8f8fb;border-left:3px solid #7f77dd;margin-top:16px;">
                <div style="width:8px;height:8px;border-radius:50%;background:#7f77dd;flex-shrink:0;"></div>
                <div>
                    <p style="font-size:13px;font-weight:600;color:#534ab7;margin:0;">Awaiting payment</p>
                    <span style="font-size:12px;color:#9a9aaa;">Complete your payment to claim the comic</span>
                </div>
            </div>` : ""}
        </div>
    </div>`;
    } else {
        container.innerHTML = `
            <div class="alert alert-info mt-4 shadow-sm">
                <h5 class="fw-bold mb-2">
                    <i class="bi bi-info-circle-fill me-2"></i>Auction Result
                </h5>
                <p class="mb-0 text-muted">No bids were placed in this auction.</p>
            </div>`;
    }
}

function updateAuctionStatusBadge(data) {
    const badge = document.querySelector(".badge[data-auction-status]");
    if (!badge) return;

    badge.classList.remove("bg-warning", "bg-success", "bg-primary", "bg-danger", "bg-secondary");

    if (data.finalState === 3) {
        badge.classList.add("bg-primary");
        badge.innerHTML = '<i class="bi bi-stop-circle-fill me-2"></i>Finished';
    } else if (data.finalState === 4) {
        badge.classList.add("bg-danger");
        badge.innerHTML = '<i class="bi bi-x-circle-fill me-2"></i>Cancelled';
    }
}

function showNotificationToast(message, type = "success") {
    let container = document.querySelector(".toast-notification-container");
    if (!container) {
        container = document.createElement("div");
        container.className = "toast-notification-container position-fixed bottom-0 end-0 p-3";
        container.style.zIndex = "1100";
        document.body.appendChild(container);
    }

    const id = "toast-" + Date.now();
    const bgClass = type === "success" ? "bg-success"
        : type === "warning" ? "bg-warning"
            : "bg-primary";

    container.insertAdjacentHTML("beforeend", `
        <div id="${id}" class="toast" role="alert" aria-live="assertive" aria-atomic="true"
             data-bs-autohide="true" data-bs-delay="4000">
            <div class="toast-header ${bgClass} text-white">
                <i class="bi bi-megaphone-fill me-2"></i>
                <strong class="me-auto">Auction Update</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">${escapeHtml(message)}</div>
        </div>`);

    const el = document.getElementById(id);
    new bootstrap.Toast(el).show();
    el.addEventListener("hidden.bs.toast", () => el.remove());
}

function escapeHtml(str) {
    if (!str) return "";
    return str.replace(/[&<>"']/g, m => ({
        "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
    }[m]));
}

// Animación highlight para pujas nuevas
const style = document.createElement("style");
style.textContent = `
    .highlight-update { animation: highlightPulse 0.5s ease-in-out; }
    @keyframes highlightPulse {
        0%   { transform: scale(1);    color: white; }
        50%  { transform: scale(1.05); color: #ffd700; text-shadow: 0 0 10px rgba(255,215,0,.5); }
        100% { transform: scale(1);    color: white; }
    }`;
document.head.appendChild(style);