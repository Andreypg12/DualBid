"use strict";

// ALEJANDRO — Flujo de pago para el ganador.
// Pay now: muestra recibo. Release comic: cancela la subasta.
const PaymentFlow = (() => {
    let _auctionId = null, _amount = 0, _comicTitle = "";
    let _modal = null;
    let _closing = false;

    function init(auctionId) {
        _auctionId = auctionId;
        _setupInputMask();
        _injectStyles();

        document.getElementById("confirmPaymentBtn")
            ?.addEventListener("click", _handlePay);

        document.getElementById("releaseComicBtn")
            ?.addEventListener("click", _handleRelease);

        // Intercepta ESC y clic fuera del modal — misma lógica que Release
        const modalEl = document.getElementById("paymentModal");
        if (modalEl) {
            modalEl.addEventListener("hide.bs.modal", async (e) => {
                if (_closing) return;
                e.preventDefault();
                await _handleRelease();
            });
        }


    }

    function openForWinner(data) {
        _closing    = false;
        _amount     = parseFloat(data.finalAmount ?? data.amount ?? data.nuevoMonto ?? 0);
        _comicTitle = data.comicTitle
            || document.querySelector(".h3.fw-bold.text-primary")?.textContent?.trim()
            || "Comic";

        _setEl("paymentComicTitle",    _comicTitle);
        _setEl("paymentAmountDisplay", "$" + _fmt(_amount));
        _setEl("payBtnAmount",         "$" + _fmt(_amount));

        _modal = bootstrap.Modal.getOrCreateInstance(
            document.getElementById("paymentModal")
        );
        _modal.show();
    }

    // Pay now: notifica al servidor → SignalR muestra recibo a todos
    async function _handlePay() {
        if (!_validateForm()) return;

        const btn = document.getElementById("confirmPaymentBtn");
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processing...';

        await new Promise(r => setTimeout(r, 1000));

        try {
            await fetch("/Auction/NotifyPaymentComplete", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": _csrf()
                },
                body: JSON.stringify({ auctionId: _auctionId })
            });
        } catch { }

        const container = document.getElementById("auctionResultContainer");
        if (container) {
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
                    <span class="fw-bold" style="font-size:14px;">${_comicTitle}</span>
                </div>
                <div class="d-flex justify-content-between align-items-center py-3">
                    <span style="font-size:13px;color:#888;">Amount</span>
                    <span class="fw-bold" style="font-size:22px;color:#1a7a4a;">$${_fmt(_amount)}</span>
                </div>
            </div>
        </div>`;
        }

        _closing = true;
        _modal?.hide();

        await Swal.fire({
            icon: "success",
            title: "Payment successful!",
            text: "Your payment has been registered.",
            confirmButtonText: "Great!",
            confirmButtonColor: "#28a745"
        });
    }

    // Release: cancela la subasta (estado 4) y notifica a todos
    async function _handleRelease() {
        try {
            await fetch("/Auction/CancelAfterWin", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": _csrf()
                },
                body: JSON.stringify({ auctionId: _auctionId })
            });
        } catch { }

        _closing = true;
        _modal?.hide();

        await Swal.fire({
            icon: "info",
            title: "Comic released",
            text: "The comic has been returned to auction.",
            confirmButtonText: "OK"
        });

        location.reload();
    }

    function _validateForm() {
        if (!_val("cardName",   v => v.length >= 3))                    return false;
        if (!_val("cardNumber", v => v.replace(/\s/g,"").length >= 13)) return false;
        if (!_val("cardExpiry", v => /^\d{2}\/\d{2}$/.test(v)))         return false;
        if (!_val("cardCvv",    v => v.length >= 3))                    return false;
        return true;
    }

    function _val(id, test) {
        const el = document.getElementById(id);
        if (!el) return true;
        const ok = test(el.value.trim());
        el.classList.toggle("is-invalid", !ok);
        if (!ok) {
            el.focus();
            el.style.animation = "none";
            el.offsetHeight;
            el.style.animation = "payShake .4s ease";
        } else {
            el.classList.remove("is-invalid");
        }
        return ok;
    }

    function _setupInputMask() {
        document.getElementById("cardNumber")?.addEventListener("input", e => {
            e.target.value = e.target.value.replace(/\D/g,"").slice(0,16).replace(/(.{4})/g,"$1 ").trim();
        });
        document.getElementById("cardExpiry")?.addEventListener("input", e => {
            let v = e.target.value.replace(/\D/g,"").slice(0,4);
            if (v.length >= 3) v = v.slice(0,2) + "/" + v.slice(2);
            e.target.value = v;
        });
        document.getElementById("cardCvv")?.addEventListener("input", e => {
            e.target.value = e.target.value.replace(/\D/g,"").slice(0,4);
        });
    }

    function _setEl(id, txt) { const e = document.getElementById(id); if (e) e.textContent = txt; }
    function _fmt(n) { return Number(n).toLocaleString("en-US",{minimumFractionDigits:2,maximumFractionDigits:2}); }
    function _csrf() { return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? ""; }

    function _injectStyles() {
        if (document.getElementById("payStyles")) return;
        const s = document.createElement("style");
        s.id = "payStyles";
        s.textContent = `@keyframes payShake{0%,100%{transform:translateX(0)}20%{transform:translateX(-6px)}40%{transform:translateX(6px)}60%{transform:translateX(-4px)}80%{transform:translateX(4px)}}`;
        document.head.appendChild(s);
    }

    return { init, openForWinner };
})();
