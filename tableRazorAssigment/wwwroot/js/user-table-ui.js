import { UserDto } from './user-dto.js';
import { ApiService } from './api-service.js';

export class UserTableUI {
    static PAGE_SIZE = 20;

    /**
     * @param {Object} elements
     * @param {HTMLInputElement} elements.filterInput
     * @param {HTMLTableSectionElement} elements.tbody
     * @param {HTMLInputElement} elements.selectAllCheckbox
     * @param {HTMLButtonElement} elements.blockBtn
     * @param {HTMLButtonElement} elements.unblockBtn
     * @param {HTMLButtonElement} elements.deleteSelectedBtn
     * @param {HTMLButtonElement} elements.deleteNonVerifiedBtn
     * @param {HTMLTableHeaderCellElement} elements.thead
     * @param {Object} elements.sortIcons
     * @param {HTMLButtonElement} elements.prevPageBtn
     * @param {HTMLButtonElement} elements.nextPageBtn
     * @param {HTMLSpanElement} elements.pageInfo
     */
    constructor(elements) {
        this.elements = elements;
        this.apiService = new ApiService();

        /** @type {UserDto[]} */
        this.users = [];
        this.totalCount = 0;

        this.currentPage = 1;
        this.searchTerm = '';
        this.sortColumn = '';
        this.sortDescending = false;
        this.searchDebounceTimer = null;

        this.initEventListeners();
        this.loadUsers();
    }

    /**
     * Загружает данные с сервера согласно текущим параметрам.
     */
    async loadUsers() {
        try {
            const params = {
                pageNumber: this.currentPage,
                pageSize: UserTableUI.PAGE_SIZE,
                searchTerm: this.searchTerm || undefined,
                sortBy: this.sortColumn || undefined,
                sortDescending: this.sortDescending
            };
            const response = await this.apiService.fetchUsers(params);
            this.users = response.items;
            this.totalCount = response.totalCount;
        } catch (error) {
            console.error('Failed to load users:', error);
            alert('Failed to load users. Please try again later.');
        } finally {
            this.render();
        }
    }

    // ------------------- Форматирование -------------------
    formatRelativeTime(date) {
        const now = new Date();
        const diffMs = now - date;
        const diffSec = Math.floor(diffMs / 1000);
        const diffMin = Math.floor(diffSec / 60);
        const diffHrs = Math.floor(diffMin / 60);
        const diffDays = Math.floor(diffHrs / 24);

        if (diffSec < 60) return 'just now';
        if (diffMin < 60) return `${diffMin} min ago`;
        if (diffHrs < 24) return `${diffHrs} hour${diffHrs > 1 ? 's' : ''} ago`;
        if (diffDays < 30) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
        return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    }

    formatExactTime(date) {
        return date.toLocaleString('en-US', {
            year: 'numeric', month: 'short', day: 'numeric',
            hour: '2-digit', minute: '2-digit', second: '2-digit'
        });
    }

    escapeHtml(text) {
        if (!text) return text;
        return String(text)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    // ------------------- Рендеринг -------------------
    updateSortIcons() {
        const icons = this.elements.sortIcons;
        Object.values(icons).forEach(icon => {
            if (icon) icon.className = 'fas fa-sort';
        });
        if (this.sortColumn) {
            const activeIcon = icons[this.sortColumn.toLowerCase()];
            if (activeIcon) {
                activeIcon.className = this.sortDescending ? 'fas fa-sort-down' : 'fas fa-sort-up';
            }
        }
    }

    render() {
        let html = '';
        this.users.forEach(user => {
            const exactTime = this.formatExactTime(user.lastSeen);
            const relativeTime = this.formatRelativeTime(user.lastSeen);
            const rowClass = user.isUserBlocked ? 'blocked-user-row' : '';

            html += `<tr class="${rowClass}">`;
            html += `<td class="checkbox-col"><input type="checkbox" class="row-checkbox" data-user-id="${this.escapeHtml(user.id)}"></td>`;
            html += `<td><span class="user-name">${this.escapeHtml(user.name)}</span></td>`;
            html += `<td>${this.escapeHtml(user.userEmail)}</td>`;
            html += `<td>${this.escapeHtml(user.status)}</td>`;
            html += `<td><span class="lastseen-cell" title="${this.escapeHtml(exactTime)}">${this.escapeHtml(relativeTime)}</span></td>`;
            html += `</tr>`;
        });

        this.elements.tbody.innerHTML = html;
        this.updateSelectAllState();
        this.updateSortIcons();
        this.updatePaginationInfo();
    }

    updatePaginationInfo() {
        const totalPages = Math.ceil(this.totalCount / UserTableUI.PAGE_SIZE);
        this.elements.pageInfo.textContent = `Page ${this.currentPage} of ${totalPages || 1}`;
        this.elements.prevPageBtn.disabled = this.currentPage <= 1;
        this.elements.nextPageBtn.disabled = this.currentPage >= totalPages;
    }

    // ------------------- Управление чекбоксами -------------------
    getSelectedUserIds() {
        const checkboxes = this.elements.tbody.querySelectorAll('.row-checkbox:checked');
        return Array.from(checkboxes).map(cb => cb.dataset.userId);
    }

    updateSelectAllState() {
        const rowCbs = this.elements.tbody.querySelectorAll('.row-checkbox');
        const checkedCount = this.elements.tbody.querySelectorAll('.row-checkbox:checked').length;
        const selectAll = this.elements.selectAllCheckbox;

        if (rowCbs.length === 0) {
            selectAll.checked = false;
            selectAll.indeterminate = false;
        } else if (checkedCount === 0) {
            selectAll.checked = false;
            selectAll.indeterminate = false;
        } else if (checkedCount === rowCbs.length) {
            selectAll.checked = true;
            selectAll.indeterminate = false;
        } else {
            selectAll.indeterminate = true;
        }
    }

    // ------------------- Обработчики действий -------------------
    async handleBlockSelected() {
        const ids = this.getSelectedUserIds();
        if (ids.length === 0) {
            alert('No users selected.');
            return;
        }
        try {
            await this.apiService.blockUsers(ids);
            await this.loadUsers();
        } catch (e) {
            alert('Failed to block users.');
        }
    }

    async handleUnblockSelected() {
        const ids = this.getSelectedUserIds();
        if (ids.length === 0) {
            alert('No users selected.');
            return;
        }
        try {
            await this.apiService.unblockUsers(ids);
            await this.loadUsers();
        } catch (e) {
            alert('Failed to unblock users.');
        }
    }

    async handleDeleteSelected() {
        const ids = this.getSelectedUserIds();
        if (ids.length === 0) {
            alert('No users selected.');
            return;
        }
        if (!confirm(`Delete ${ids.length} user(s)?`)) return;
        try {
            await this.apiService.deleteUsers(ids);
            // Если удалили все строки на текущей странице и это не первая страница — переходим назад
            if (this.users.length === ids.length && this.currentPage > 1) {
                this.currentPage--;
            }
            await this.loadUsers();
        } catch (e) {
            alert('Failed to delete users.');
        }
    }

    /**
     * Удаляет только выбранных неподтверждённых пользователей.
     * Аналогично другим кнопкам: работает с выделенными чекбоксами.
     */
    async handleDeleteNonVerified() {
        const selectedIds = this.getSelectedUserIds();
        if (selectedIds.length === 0) {
            alert('No users selected.');
            return;
        }

        // Находим среди выбранных тех, у кого статус "Not verified"
        const selectedUsers = this.users.filter(u => selectedIds.includes(u.id));
        const nonVerifiedIds = selectedUsers
            .filter(u => u.status === 'Not verified')
            .map(u => u.id);

        if (nonVerifiedIds.length === 0) {
            alert('No non‑verified users among the selected ones.');
            return;
        }

        if (!confirm(`Delete ${nonVerifiedIds.length} non‑verified user(s)?`)) return;

        try {
            await this.apiService.clearNonVerifiedUsers(nonVerifiedIds);
            // Если после удаления страница опустела, переходим на предыдущую (если не первая)
            if (this.users.length === nonVerifiedIds.length && this.currentPage > 1) {
                this.currentPage--;
            }
            await this.loadUsers();
        } catch (e) {
            alert('Failed to clear non‑verified users.');
        }
    }

    // ------------------- Инициализация событий -------------------
    initEventListeners() {
        // Фильтр с debounce
        this.elements.filterInput.addEventListener('input', () => {
            clearTimeout(this.searchDebounceTimer);
            this.searchDebounceTimer = setTimeout(() => {
                this.searchTerm = this.elements.filterInput.value;
                this.currentPage = 1;
                this.loadUsers();
            }, 300);
        });

        // Сортировка
        this.elements.thead.addEventListener('click', (e) => {
            const th = e.target.closest('th.sortable');
            if (!th) return;
            const column = th.dataset.column;
            if (!column) return;

            if (this.sortColumn === column) {
                this.sortDescending = !this.sortDescending;
            } else {
                this.sortColumn = column;
                this.sortDescending = false;
            }
            this.currentPage = 1;
            this.loadUsers();
        });

        // Пагинация
        this.elements.prevPageBtn.addEventListener('click', () => {
            if (this.currentPage > 1) {
                this.currentPage--;
                this.loadUsers();
            }
        });

        this.elements.nextPageBtn.addEventListener('click', () => {
            const totalPages = Math.ceil(this.totalCount / UserTableUI.PAGE_SIZE);
            if (this.currentPage < totalPages) {
                this.currentPage++;
                this.loadUsers();
            }
        });

        // Мастер-чекбокс
        this.elements.selectAllCheckbox.addEventListener('change', () => {
            const rowCbs = this.elements.tbody.querySelectorAll('.row-checkbox');
            const anyChecked = Array.from(rowCbs).some(cb => cb.checked);
            const newState = !anyChecked;
            rowCbs.forEach(cb => cb.checked = newState);
            this.elements.selectAllCheckbox.checked = newState;
            this.elements.selectAllCheckbox.indeterminate = false;
        });

        // Чекбоксы строк
        this.elements.tbody.addEventListener('change', (e) => {
            if (e.target.classList.contains('row-checkbox')) {
                this.updateSelectAllState();
            }
        });

        // Кнопки действий
        this.elements.blockBtn.addEventListener('click', () => this.handleBlockSelected());
        this.elements.unblockBtn.addEventListener('click', () => this.handleUnblockSelected());
        this.elements.deleteSelectedBtn.addEventListener('click', () => this.handleDeleteSelected());
        this.elements.deleteNonVerifiedBtn.addEventListener('click', () => this.handleDeleteNonVerified());
    }
}