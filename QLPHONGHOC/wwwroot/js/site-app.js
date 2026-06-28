/* ============================================================
   QLPHONGHOC — Site App JavaScript
   Sidebar toggle, Toast notifications, Modal helpers,
   Skeleton loaders
   ============================================================ */

(function () {
    'use strict';

    // ============================================================
    // 1. SIDEBAR TOGGLE
    // ============================================================
    function initSidebar() {
        const sidebar = document.getElementById('appSidebar');
        const overlay = document.getElementById('sidebarOverlay');
        const toggleBtn = document.getElementById('sidebarToggle');
        if (!sidebar) return;

        function openSidebar() {
            sidebar.classList.add('open');
            if (overlay) overlay.classList.add('active');
            document.body.style.overflow = 'hidden';
        }
        function closeSidebar() {
            sidebar.classList.remove('open');
            if (overlay) overlay.classList.remove('active');
            document.body.style.overflow = '';
        }

        if (toggleBtn) toggleBtn.addEventListener('click', openSidebar);
        if (overlay) overlay.addEventListener('click', closeSidebar);

        // Auto-highlight active nav item
        const currentPath = window.location.pathname.toLowerCase();
        document.querySelectorAll('.nav-item[href]').forEach(link => {
            const href = link.getAttribute('href').toLowerCase();
            if (href && href !== '/' && currentPath.startsWith(href)) {
                link.classList.add('active');
            } else if (href === '/' && currentPath === '/') {
                link.classList.add('active');
            }
        });
    }

    // ============================================================
    // 2. TOAST NOTIFICATION SYSTEM
    // ============================================================
    window.Toast = {
        container: null,

        getContainer() {
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.className = 'toast-container';
                document.body.appendChild(this.container);
            }
            return this.container;
        },

        show(type, title, message, duration = 4500) {
            const icons = {
                success: '✅',
                error:   '❌',
                warning: '⚠️',
                info:    'ℹ️'
            };

            const toast = document.createElement('div');
            toast.className = `toast toast-${type}`;
            toast.innerHTML = `
                <span class="toast-icon">${icons[type] || 'ℹ️'}</span>
                <div class="toast-content">
                    <div class="toast-title">${title}</div>
                    ${message ? `<div class="toast-message">${message}</div>` : ''}
                </div>
                <button class="toast-close" onclick="Toast.remove(this.parentElement)">×</button>
            `;

            this.getContainer().appendChild(toast);

            if (duration > 0) {
                setTimeout(() => this.remove(toast), duration);
            }
            return toast;
        },

        success(title, msg, duration) { return this.show('success', title, msg, duration); },
        error(title, msg, duration)   { return this.show('error', title, msg, duration); },
        warning(title, msg, duration) { return this.show('warning', title, msg, duration); },
        info(title, msg, duration)    { return this.show('info', title, msg, duration); },

        remove(toast) {
            if (!toast || toast.classList.contains('removing')) return;
            toast.classList.add('removing');
            toast.addEventListener('animationend', () => toast.remove(), { once: true });
        }
    };

    // Show server-side TempData toasts from hidden inputs
    function showTempDataToasts() {
        const success = document.getElementById('tempDataSuccess');
        const error   = document.getElementById('tempDataError');
        if (success && success.value) Toast.success('Thành công', success.value);
        if (error   && error.value)   Toast.error('Lỗi',        error.value);
    }

    // ============================================================
    // 3. MODAL HELPERS
    // ============================================================
    window.Modal = {
        open(id) {
            const overlay = document.getElementById(id);
            if (overlay) {
                overlay.classList.add('active');
                document.body.style.overflow = 'hidden';
            }
        },
        close(id) {
            const overlay = typeof id === 'string'
                ? document.getElementById(id)
                : id.closest('.modal-overlay');
            if (overlay) {
                overlay.classList.remove('active');
                document.body.style.overflow = '';
            }
        }
    };

    // Close modal on overlay click
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('modal-overlay')) {
            Modal.close(e.target);
        }
    });

    // ============================================================
    // 4. CONFIRM DELETE / NGỪNG SỬ DỤNG
    // ============================================================
    function initConfirmForms() {
        document.querySelectorAll('[data-confirm]').forEach(el => {
            el.addEventListener('click', function (e) {
                const msg = this.dataset.confirm || 'Bạn có chắc muốn thực hiện thao tác này?';
                if (!confirm(msg)) e.preventDefault();
            });
        });
    }

    // ============================================================
    // 5. BUTTON LOADING STATE
    // ============================================================
    function initFormSubmitLoading() {
        document.querySelectorAll('form[data-loading]').forEach(form => {
            form.addEventListener('submit', function () {
                const btn = this.querySelector('button[type="submit"]');
                if (btn) btn.classList.add('loading');
            });
        });
    }

    // ============================================================
    // 6. FILTER FORM AUTO-SUBMIT (on select change)
    // ============================================================
    function initAutoSubmitFilters() {
        document.querySelectorAll('.filter-bar select').forEach(sel => {
            sel.addEventListener('change', function () {
                this.closest('form').submit();
            });
        });
    }

    // ============================================================
    // 7. TIET SELECTOR
    // ============================================================
    window.TietSelector = {
        init(startInputId, endInputId, occupiedTiets) {
            const startInput = document.getElementById(startInputId);
            const endInput   = document.getElementById(endInputId);
            const container  = document.getElementById('tietSelectorContainer');
            if (!container) return;

            let selectedStart = startInput ? parseInt(startInput.value) || 0 : 0;
            let selectedEnd   = endInput   ? parseInt(endInput.value)   || 0 : 0;

            function render() {
                container.innerHTML = '';
                for (let i = 1; i <= 12; i++) {
                    const btn = document.createElement('button');
                    btn.type = 'button';
                    btn.className = 'tiet-btn';
                    btn.textContent = i;

                    const isOccupied = occupiedTiets && occupiedTiets.includes(i);
                    if (isOccupied) {
                        btn.classList.add('occupied');
                        btn.title = 'Tiết này đã có lịch';
                    } else if (selectedStart && selectedEnd && i >= selectedStart && i <= selectedEnd) {
                        btn.classList.add('selected');
                    } else if (selectedStart && !selectedEnd && i === selectedStart) {
                        btn.classList.add('selected');
                    }

                    btn.addEventListener('click', () => {
                        if (isOccupied) return;
                        if (!selectedStart || (selectedStart && selectedEnd)) {
                            selectedStart = i; selectedEnd = 0;
                        } else if (i < selectedStart) {
                            selectedStart = i; selectedEnd = 0;
                        } else {
                            selectedEnd = i;
                        }
                        if (startInput) startInput.value = selectedStart;
                        if (endInput)   endInput.value   = selectedEnd || selectedStart;
                        render();
                    });

                    container.appendChild(btn);
                }
            }
            render();
        }
    };

    // ============================================================
    // 8. DATA TABLE SORT (client-side visual only)
    // ============================================================
    function initTableSort() {
        document.querySelectorAll('.data-table thead th:not(.no-sort)').forEach(th => {
            th.addEventListener('click', function () {
                const table = this.closest('.data-table');
                const idx = Array.from(this.parentElement.children).indexOf(this);
                const rows = Array.from(table.querySelectorAll('tbody tr'));
                const asc = this.dataset.sortAsc !== 'true';
                this.dataset.sortAsc = asc;

                rows.sort((a, b) => {
                    const av = a.cells[idx]?.textContent.trim() || '';
                    const bv = b.cells[idx]?.textContent.trim() || '';
                    return asc
                        ? av.localeCompare(bv, 'vi', { numeric: true })
                        : bv.localeCompare(av, 'vi', { numeric: true });
                });

                rows.forEach(r => table.querySelector('tbody').appendChild(r));

                // Visual indicator
                table.querySelectorAll('thead th').forEach(h => h.removeAttribute('data-active'));
                this.setAttribute('data-active', asc ? 'asc' : 'desc');
            });
        });
    }

    // ============================================================
    // INIT
    // ============================================================
    document.addEventListener('DOMContentLoaded', function () {
        initSidebar();
        showTempDataToasts();
        initConfirmForms();
        initFormSubmitLoading();
        initAutoSubmitFilters();
        initTableSort();
    });
})();
