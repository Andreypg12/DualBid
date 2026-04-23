"use strict";

// Inicializar SignalR cuando carga la página
(async () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/auctionHub")
        .withAutomaticReconnect()
        .build();

    try {
        await connection.start();
        console.log("SignalR conectado para el index");

        // Escuchar nuevas subastas
        connection.on("NewAuctionCreated", (auctionData) => {
            console.log("Nueva subasta recibida:", auctionData);
            addAuctionToPage(auctionData);
        });

    } catch (err) {
        console.error("Error conectando SignalR:", err);
    }
})();

function addAuctionToPage(auction) {
    const container = document.querySelector('.row.row-cols-1');
    if (!container) return;

    if (document.querySelector(`[data-auction-id="${auction.id}"]`)) return;

    // Crear la imagen o el placeholder
    const imageHtml = auction.imageBase64
        ? `<img class="w-100 h-100 object-fit-cover"
                src="data:image/jpeg;base64,${auction.imageBase64}"
                alt="Cover of ${escapeHtml(auction.title)}"
                loading="lazy" />`
        : `<div class="d-flex flex-column justify-content-center align-items-center text-muted h-100">
            <i class="bi bi-image fs-1"></i>
            <span class="small">No Cover</span>
           </div>`;

    const auctionHtml = `
        <div class="col" data-auction-id="${auction.id}">
            <a href="/Auction/Details/${auction.id}" class="text-decoration-none text-dark">
                <article class="card h-100 shadow-sm border-0 auction-card">
                    <div class="position-relative rounded-top overflow-hidden bg-body-tertiary" style="height: 360px;">
                        ${imageHtml}
                        <div class="auction-overlay d-flex align-items-center justify-content-center">
                            <span class="badge bg-dark fs-6">
                                <i class="bi bi-eye-fill me-1"></i> View Auction
                            </span>
                        </div>
                    </div>
                    <div class="card-body bg-light text-center">
                        <h2 class="h6 fw-semibold mb-3 text-truncate">${escapeHtml(auction.title)}</h2>
                        <div class="d-flex flex-wrap justify-content-center gap-2">
                            <span class="badge text-bg-light border">
                                <i class="bi bi-calendar-event me-1"></i>
                                End Date<br />${formatDate(new Date(auction.expectedEndDate))}
                            </span>
                            <span class="badge text-bg-light border">
                                <i class="bi bi-cash-coin me-1"></i>
                                Current Bid<br />$${auction.currentBid}
                            </span>
                            <span class="badge text-bg-light border">
                                <i class="bi bi-hammer me-1"></i>
                                Bids<br />${auction.numberOfBids}
                            </span>
                        </div>
                    </div>
                </article>
            </a>
        </div>
    `;

    container.insertAdjacentHTML('afterbegin', auctionHtml);

    const alertDiv = document.querySelector('.alert.alert-info');
    if (alertDiv) alertDiv.remove();
}

function formatDate(date) {
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
        'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const day = date.getDate().toString().padStart(2, '0');
    const month = months[date.getMonth()];
    const year = date.getFullYear();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${day} ${month} ${year} : ${hours}:${minutes}`;
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