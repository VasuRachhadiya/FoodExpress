// FoodExpress Client-side JavaScript

document.addEventListener("DOMContentLoaded", function () {
    // 1. Fetch live cart count on load
    updateCartBadge();

    // 2. Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        const alerts = document.querySelectorAll(".alert:not(.alert-permanent)");
        alerts.forEach(function (alert) {
            try {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            } catch (e) { }
        });
    }, 5000);
});

// Update navbar cart badge counter via AJAX
function updateCartBadge() {
    const badge = document.getElementById("cartCountBadge");
    if (!badge) return;

    fetch("/Cart/GetCount")
        .then(response => {
            if (response.ok) {
                return response.json();
            }
            return { count: 0 };
        })
        .then(data => {
            if (data && typeof data.count !== "undefined") {
                badge.textContent = data.count;
                badge.classList.add("badge-bump");
                setTimeout(() => badge.classList.remove("badge-bump"), 300);

                if (data.count === 0) {
                    badge.classList.add("d-none");
                } else {
                    badge.classList.remove("d-none");
                }
            }
        })
        .catch(() => {
            // User might be unauthenticated, silence network error
        });
}

// Quantity helper
function changeQuantity(inputElementId, delta) {
    const input = document.getElementById(inputElementId);
    if (!input) return;
    let current = parseInt(input.value) || 1;
    current += delta;
    if (current < 1) current = 1;
    if (current > 50) current = 50;
    input.value = current;
}
