// Lógica específica para la página de historial de pujas
(async () => {
    const config = window.bidHistoryConfig;

    if (!config || !config.auctionId) {
        console.warn('BidHistory: No hay configuración disponible');
        return;
    }

    console.log('BidHistory: Inicializando para subasta', config.auctionId);

    try {
        await SignalRAuction.initialize(config.auctionId, config.userId);

        SignalRAuction.onNewBid((bidData) => {
            if (bidData.auctionId != config.auctionId) return;
            addNewBidToPage(bidData);
        });

        SignalRAuction.onUserOutbid((data) => {
            showOutbidNotification(data);
        });

    } catch (err) {
        console.error('BidHistory: Error al inicializar SignalR:', err);
    }
})();

function addNewBidToPage(bidData) {
    const container = document.getElementById('bids-container');
    if (!container) return;

    // Verificar si la bid ya existe
    const existingBids = document.querySelectorAll('.bid-card');
    let exists = false;
    existingBids.forEach(card => {
        const userEmail = card.querySelector('small')?.innerText?.replace(/[✉️]/g, '').trim();
        const amount = card.querySelector('.badge.rounded-pill')?.innerText?.replace('$', '').trim();
        if (userEmail === bidData.userEmail && parseFloat(amount) === bidData.nuevoMonto) {
            exists = true;
        }
    });

    if (exists) return;

    const isHighest = checkIfHighest(bidData.nuevoMonto);
    const bidHtml = `
        <div class="col-12 col-md-6 col-lg-4 bid-card" 
             data-date="${bidData.date}"
             data-amount="${bidData.nuevoMonto}">
            <div class="card border-0 h-100 hover-shadow rounded-4 overflow-hidden">
                <div class="position-relative">
                    <div class="${isHighest ? 'bg-success' : 'bg-primary'}" style="height: 4px;"></div>
                </div>
                <div class="card-body p-4">
                    <div class="d-flex justify-content-between align-items-start mb-3">
                        <div class="d-flex align-items-center gap-2">
                            <div class="rounded-circle bg-light p-2">
                                <i class="bi bi-person-fill text-primary"></i>
                            </div>
                            <div>
                                <h6 class="fw-bold mb-0">${escapeHtml(bidData.userName)}</h6>
                            </div>
                        </div>
                        <span class="badge ${isHighest ? 'bg-success' : 'bg-primary'} rounded-pill px-3 py-2 fs-6">
                            $${bidData.nuevoMonto.toFixed(2)}
                        </span>
                    </div>
                    <div class="d-flex justify-content-between align-items-center mt-3 pt-3 border-top border-light">
                        <span class="badge bg-light text-dark">
                            <i class="bi bi-stopwatch me-1"></i>${new Date(bidData.date).toLocaleString()}
                        </span>
                    </div>
                    ${isHighest ? `
                    <div class="mt-3">
                        <span class="badge bg-warning text-dark">
                            <i class="bi bi-trophy-fill me-1"></i>Highest bid
                        </span>
                    </div>
                    ` : ''}
                </div>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('afterbegin', bidHtml);
    updateStatistics();
    showNotificationToast(`💰 New bid: $${bidData.nuevoMonto.toFixed(2)} by ${bidData.userName}`);

    // Reordenar si es necesario
    const isDescending = document.getElementById('sortDesc')?.checked ?? true;
    if (!isDescending && typeof sortBids === 'function') {
        sortBids(false);
    }
}

function checkIfHighest(amount) {
    const allAmounts = Array.from(document.querySelectorAll('.bid-card .badge.rounded-pill'))
        .map(badge => parseFloat(badge.innerText.replace('$', '').trim()));
    const maxAmount = Math.max(...allAmounts, amount);
    return amount === maxAmount;
}

function updateStatistics() {
    const allAmounts = Array.from(document.querySelectorAll('.bid-card .badge.rounded-pill'))
        .map(badge => parseFloat(badge.innerText.replace('$', '').trim()));

    const allEmails = Array.from(document.querySelectorAll('.bid-card small'))
        .map(small => small.innerText.replace(/[✉️]/g, '').trim());

    if (allAmounts.length === 0) return;

    const max = Math.max(...allAmounts);
    const min = Math.min(...allAmounts);
    const total = allAmounts.length;
    const uniqueBidders = new Set(allEmails).size;

    const statsContainer = document.querySelector('.bg-gradient');
    if (statsContainer) {
        const statValues = statsContainer.querySelectorAll('.h4.fw-bold');
        if (statValues[0]) statValues[0].innerText = `$${max.toFixed(2)}`;
        if (statValues[1]) statValues[1].innerText = `$${min.toFixed(2)}`;
        if (statValues[2]) statValues[2].innerText = total;
        if (statValues[3]) statValues[3].innerText = uniqueBidders;
    }
}

function showNotificationToast(message) {
    let toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        toastContainer.style.zIndex = '1100';
        document.body.appendChild(toastContainer);
    }

    const toastId = 'toast-' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-bs-autohide="true" data-bs-delay="3000">
            <div class="toast-header bg-primary text-white">
                <i class="bi bi-megaphone-fill me-2"></i>
                <strong class="me-auto">New Bid!</strong>
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

function showOutbidNotification(data) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'warning',
            title: 'You have been outbid!',
            text: data.mensaje || `Someone just placed a higher bid of $${data.nuevoMonto?.toFixed(2)}`,
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 5000,
            timerProgressBar: true
        });
    } else {
        showNotificationToast(`⚠️ ${data.mensaje || `You've been outbid! New bid: $${data.nuevoMonto?.toFixed(2)}`}`);
    }
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