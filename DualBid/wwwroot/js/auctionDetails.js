"use strict";

(async () => {
    const config = window.auctionDetailsConfig;

    if (!config || !config.auctionId) {
        console.warn('AuctionDetails: No hay configuración disponible');
        return;
    }

    console.log('AuctionDetails: Inicializando para subasta', config.auctionId);

    try {
        await SignalRAuction.initialize(config.auctionId, config.userId);

        // ✅ DEL ORIGINAL - Nuevas pujas
        SignalRAuction.onNewBid((bidData) => {
            if (bidData.auctionId != config.auctionId) return;

            console.log('AuctionDetails: Nueva puja recibida', bidData);

            const currentBidLabel = document.getElementById('currentBidLabel');
            if (currentBidLabel) {
                currentBidLabel.innerText = `$${bidData.nuevoMonto.toFixed(2)}`;
                currentBidLabel.classList.add('highlight-update');
                setTimeout(() => currentBidLabel.classList.remove('highlight-update'), 500);
            }

            const totalBidsLabel = document.getElementById('totalBidsLabel');
            if (totalBidsLabel) {
                const currentTotal = parseInt(totalBidsLabel.innerText) || 0;
                totalBidsLabel.innerText = currentTotal + 1;
            }

            showNotificationToast(`💸 New bid: $${bidData.nuevoMonto.toFixed(2)} by ${bidData.userName}`);
        });

        // ✅ DEL ORIGINAL - Usuario superado
        SignalRAuction.onUserOutbid((data) => {
            console.log('AuctionDetails: Usuario superado', data);

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'warning',
                    title: '⚠️ You have been outbid!',
                    text: data.mensaje || `Someone just placed a higher bid of $${data.nuevoMonto?.toFixed(2)}`,
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 6000,
                    timerProgressBar: true,
                    background: '#fff3cd',
                    iconColor: '#ffc107'
                });
            } else {
                showNotificationToast(`⚠️ ${data.mensaje || `You've been outbid! New bid: $${data.nuevoMonto?.toFixed(2)}`}`);
            }
        });

        // ✅ NUEVO - Cierre de subasta
        SignalRAuction.onAuctionClosed((data) => {
            console.log('🔒 Subasta cerrada:', data);

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: data.hasBids ? 'success' : 'info',
                    title: data.hasBids ? '🏆 Auction Ended!' : '📋 Auction Closed',
                    text: data.message,
                    confirmButtonText: 'OK',
                    allowOutsideClick: false
                }).then(() => {
                    setTimeout(() => location.reload(), 2000);
                });
            }

            const bidButton = document.querySelector('a[href*="/Bid/Create"]');
            if (bidButton) {
                bidButton.outerHTML = `<button class="btn btn-secondary btn-lg px-5" disabled>
                    <i class="bi bi-lock-fill me-2"></i>Auction Ended
                </button>`;
            }

            updateAuctionStatusBadge(data);


            //@* Editado por ALE * @
            //Esto es para determinar el ganador y mostrarle un mensaje personalizado
            const timeRemaining = document.getElementById('timeRemainingLabel');
            if (timeRemaining) {
                timeRemaining.innerHTML = '<span class="text-danger">Ended</span>';
            }

            const resultContainer = document.getElementById('auctionResultContainer');
            if (resultContainer) {
                if (data.winnerUserId) {
                    let html = `
            <div class="alert alert-success mt-4 shadow-sm">
                <h5 class="fw-bold mb-2">
                    <i class="bi bi-trophy-fill me-2"></i>
                    Auction Result
                </h5>
                <p class="mb-1"><strong>Winner:</strong> ${data.winnerName}</p>
                <p class="mb-1"><strong>Final Price:</strong> $${Number(data.finalAmount).toLocaleString()}</p>
        `;

                    if (parseInt(config.userId || "0") === data.winnerUserId) {
                        html += `
                <div class="alert alert-primary mt-2 mb-0">
                    🎉 You have won this auction!
                </div>
            `;
                    }

                    html += `</div>`;
                    resultContainer.innerHTML = html;
                } else {
                    resultContainer.innerHTML = `
            <div class="alert alert-success mt-4 shadow-sm">
                <h5 class="fw-bold mb-2">
                    <i class="bi bi-trophy-fill me-2"></i>
                    Auction Result
                </h5>
                <p class="mb-0 text-muted">No bids were placed in this auction.</p>
            </div>
        `;
                }
            }
        });

        // ✅ NUEVO - Activación de subasta
        SignalRAuction.onAuctionActivated((data) => {
            console.log('🎯 Subasta activada:', data);

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'success',
                    title: '🚀 Auction Started!',
                    text: data.message,
                    timer: 3000,
                    showConfirmButton: true
                });
            }

            showNotificationToast(`🚀 ${data.message}`, 'success');
        });

        // ✅ NUEVO - Usuario ganó
        SignalRAuction.onYouWon((data) => {
            console.log('🏆 ¡Ganaste!:', data);

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'success',
                    title: '🎉 Congratulations!',
                    text: data.message,
                    confirmButtonText: 'Great!',
                    background: '#d4edda'
                });
            }

            showNotificationToast(`🏆 ${data.message}`, 'success');
        });

        // ✅ NUEVO - Subasta del creador terminó
        SignalRAuction.onYourAuctionEnded((data) => {
            console.log('📊 Tu subasta terminó:', data);

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: data.hasBids ? 'success' : 'info',
                    title: '📋 Your Auction Has Ended',
                    text: data.message,
                    confirmButtonText: 'OK'
                });
            }
        });

        // ✅ DEL ORIGINAL - Reconexión
        SignalRAuction.onReconnected(() => {
            console.log('AuctionDetails: Reconectado a SignalR');
            showNotificationToast('🔄 Conexión restablecida', 'info');
        });

    } catch (err) {
        console.error('AuctionDetails: Error al inicializar SignalR:', err);
    }
})();

// ✅ DEL ORIGINAL - Toast con Bootstrap
function showNotificationToast(message, type = 'success') {
    let toastContainer = document.querySelector('.toast-notification-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-notification-container position-fixed bottom-0 end-0 p-3';
        toastContainer.style.zIndex = '1100';
        document.body.appendChild(toastContainer);
    }

    const toastId = 'toast-' + Date.now();
    const bgClass = type === 'success' ? 'bg-success' : (type === 'warning' ? 'bg-warning' : 'bg-primary');

    const toastHtml = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-bs-autohide="true" data-bs-delay="4000">
            <div class="toast-header ${bgClass} text-white">
                <i class="bi bi-megaphone-fill me-2"></i>
                <strong class="me-auto">Auction Update</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">
                ${escapeHtml(message)}
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement);
    toast.show();

    toastElement.addEventListener('hidden.bs.toast', () => toastElement.remove());
}

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>]/g, function (m) {
        if (m === '&') return '&amp;';
        if (m === '<') return '&lt;';
        if (m === '>') return '&gt;';
        return m;
    });
}

// ✅ NUEVO - Actualizar badge de estado
function updateAuctionStatusBadge(data) {
    const statusBadge = document.querySelector('.bg-gradient-primary .badge');
    if (!statusBadge) return;

    statusBadge.classList.remove('bg-warning', 'bg-success', 'bg-primary', 'bg-danger', 'bg-secondary');

    if (data.finalState === 3) {
        statusBadge.classList.add('bg-primary');
        statusBadge.innerHTML = '<i class="bi bi-stop-circle-fill me-2"></i>Finished';
    } else if (data.finalState === 4) {
        statusBadge.classList.add('bg-danger');
        statusBadge.innerHTML = '<i class="bi bi-stop-circle-fill me-2"></i>Cancelled';
    }
}

// ✅ DEL ORIGINAL - Animación highlight
const style = document.createElement('style');
style.textContent = `
    .highlight-update {
        animation: highlightPulse 0.5s ease-in-out;
    }
    
    @keyframes highlightPulse {
        0% { transform: scale(1); color: white; }
        50% { transform: scale(1.05); color: #ffd700; text-shadow: 0 0 10px rgba(255,215,0,0.5); }
        100% { transform: scale(1); color: white; }
    }
`;
document.head.appendChild(style);