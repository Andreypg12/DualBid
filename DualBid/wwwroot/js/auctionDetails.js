// Lógica específica para la página de detalles de subasta
(async () => {
    // Obtener configuración de la vista
    const config = window.auctionDetailsConfig;

    if (!config || !config.auctionId) {
        console.warn('AuctionDetails: No hay configuración disponible');
        return;
    }

    console.log('AuctionDetails: Inicializando para subasta', config.auctionId);

    try {
        // Inicializar conexión SignalR
        await SignalRAuction.initialize(config.auctionId, config.userId);

        // Escuchar nuevas pujas
        SignalRAuction.onNewBid((bidData) => {
            if (bidData.auctionId != config.auctionId) return;

            console.log('AuctionDetails: Nueva puja recibida', bidData);

            // Actualizar el monto actual
            const currentBidLabel = document.getElementById('currentBidLabel');
            if (currentBidLabel) {
                currentBidLabel.innerText = `$${bidData.nuevoMonto.toFixed(2)}`;
                // Agregar animación de actualización
                currentBidLabel.classList.add('highlight-update');
                setTimeout(() => currentBidLabel.classList.remove('highlight-update'), 500);
            }

            // Actualizar total de bids
            const totalBidsLabel = document.getElementById('totalBidsLabel');
            if (totalBidsLabel) {
                const currentTotal = parseInt(totalBidsLabel.innerText) || 0;
                totalBidsLabel.innerText = currentTotal + 1;
            }

            // Mostrar notificación tipo toast
            showNotificationToast(`💸 New bid: $${bidData.nuevoMonto.toFixed(2)} by ${bidData.userName}`);
        });

        // Escuchar cuando superan al usuario
        SignalRAuction.onUserOutbid((data) => {
            console.log('AuctionDetails: Usuario superado', data);

            // Mostrar SweetAlert si está disponible
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

        // Escuchar reconexión
        SignalRAuction.onReconnected(() => {
            console.log('AuctionDetails: Reconectado a SignalR');
            showNotificationToast('🔄 Conexión restablecida', 'info');
        });

    } catch (err) {
        console.error('AuctionDetails: Error al inicializar SignalR:', err);
    }
})();

// Función para mostrar notificaciones toast
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

// Agregar estilos para animación
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