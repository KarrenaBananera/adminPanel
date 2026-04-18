import { UserTableModel } from './user-table-model.js';

export class UserTableView {
    constructor(elements, model = new UserTableModel()) {
        this.elements = elements;
        this.model = model;
        this.model.subscribe(state => this.render(state));
        this.initEventListeners();
        this.model.loadUsers();
    }

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

    render(state) {
        const { users, totalCount, currentPage, sortColumn, sortDescending } = state;

        let html = '';
        users.forEach(user => {
            const exactTime = this.formatExactTime(user.lastSeen);
            const relativeTime = this.formatRelativeTime(user.lastSeen);
            const rowClass = user.isUserBlocked ? 'blocked-user-row' : '';
            const userTitle = user.title ? user.title : 'N/A';

            html += `<tr class="${rowClass}">`;
            html += `<td class="checkbox-col"><input type="checkbox" class="row-checkbox" data-user-id="${user.id}"></td>`;
            html += `<td>
                        <span class="user-name">${user.name}</span>
                        <div class="user-title" style="font-size: 0.75rem; color: #6c757d;">${userTitle}</div>
                      </td>`;
            html += `<td>${user.userEmail}</td>`;
            html += `<td>${user.status}</td>`;
            html += `<td><span class="lastseen-cell" title="${exactTime}">${relativeTime}</span></td>`;
            html += `</tr>`;
        });

        this.elements.tbody.innerHTML = html;

        this.updateSortIcons(sortColumn, sortDescending);
        this.updateSelectAllState();
        this.updateSelectedCount();

        const totalPages = Math.ceil(totalCount / UserTableModel.PAGE_SIZE) || 1;
        this.elements.pageInfo.textContent = `Page ${currentPage} of ${totalPages}`;
        this.elements.prevPageBtn.disabled = currentPage <= 1;
        this.elements.nextPageBtn.disabled = currentPage >= totalPages;
    }

    updateSortIcons(sortColumn, sortDescending) {
        const icons = this.elements.sortIcons;
        Object.values(icons).forEach(icon => {
            if (icon) icon.className = 'fas fa-sort';
        });
        if (sortColumn) {
            const activeIcon = icons[sortColumn.toLowerCase()];
            if (activeIcon) {
                activeIcon.className = sortDescending ? 'fas fa-sort-down' : 'fas fa-sort-up';
            }
        }
    }

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

    initEventListeners() {
        this.initFilterListener();
        this.initSortListener();
        this.initPaginationListeners();
        this.initSelectAllListener();
        this.initRowCheckboxListener();
        this.initBlockListener();
        this.initUnblockListener();
        this.initDeleteSelectedListener();
        this.initDeleteNonVerifiedListener();
    }

    initFilterListener() {
        this.elements.filterInput.addEventListener('input', (e) => {
            this.model.setSearchTerm(e.target.value);
        });
    }

    initSortListener() {
        this.elements.thead.addEventListener('click', (e) => {
            const th = e.target.closest('th.sortable');
            if (!th) return;
            const column = th.dataset.column;
            if (!column) return;
            this.model.setSort(column);
        });
    }

    initPaginationListeners() {
        this.elements.prevPageBtn.addEventListener('click', () => this.model.prevPage());
        this.elements.nextPageBtn.addEventListener('click', () => this.model.nextPage());
    }

    initSelectAllListener() {
        this.elements.selectAllCheckbox.addEventListener('change', () => {
            const rowCbs = this.elements.tbody.querySelectorAll('.row-checkbox');
            const anyChecked = Array.from(rowCbs).some(cb => cb.checked);
            const newState = !anyChecked;
            rowCbs.forEach(cb => cb.checked = newState);
            this.elements.selectAllCheckbox.checked = newState;
            this.elements.selectAllCheckbox.indeterminate = false;
            this.updateSelectedCount();
        });
    }

    initRowCheckboxListener() {
        this.elements.tbody.addEventListener('change', (e) => {
            if (e.target.classList.contains('row-checkbox')) {
                this.updateSelectAllState();
                this.updateSelectedCount();
            }
        });
    }

    initBlockListener() {
        this.elements.blockBtn.addEventListener('click', async () => {
            const ids = this.getSelectedUserIds();
            if (ids.length === 0) return;
            if (!this.isContainsCurrentUser(ids))
                await this.model.blockUsers(ids);
            else {
                var confirmed = await this.showConfirmDialog(ids, 'Are you sure you want block');
                if (confirmed)
                {
                    await this.model.blockUsers(ids);
                    window.location.href = '/Login';
                }
            }
        });
    }

    initUnblockListener() {
        this.elements.unblockBtn.addEventListener('click', async () => {
            const ids = this.getSelectedUserIds();
            if (ids.length === 0) return;
            await this.model.unblockUsers(ids);
        });
    }

    initDeleteSelectedListener() {
        this.elements.deleteSelectedBtn.addEventListener('click', async () => {
            const ids = this.getSelectedUserIds();
            if (ids.length === 0) return;
            var confirmed = await this.showConfirmDialog(ids, 'Are you sure you want delete');
            if (confirmed) {
                await this.model.deleteUsers(ids);
                if (this.isContainsCurrentUser(ids))
                    window.location.href = '/Login';
            }
        });
    }

    initDeleteNonVerifiedListener() {
        this.elements.deleteNonVerifiedBtn.addEventListener('click', async () => {
            const selectedIds = this.getSelectedUserIds();
            if (selectedIds.length === 0) return
            var confirmed = await this.showConfirmDialog(selectedIds, 'Are you sure you want delete');
            if (confirmed) {
                await this.model.clearNonVerifiedUsers(selectedIds);
                if (this.isContainsCurrentUser(selectedIds))
                    window.location.href = '/Login';
            }
        });
    }

    updateSelectedCount() {
        const selectedCount = this.getSelectedUserIds().length;
        if (this.elements.selectedCountDisplay) {
            this.elements.selectedCountDisplay.textContent = `Selected: ${selectedCount}`;
        }
    }

    createConfirmDialog(message) {
        return new Promise((resolve) => {
            const dialog = document.getElementById('confirmDialog');
            const messageSpan = document.getElementById('confirmDialogMessage');
            const cancelBtn = document.getElementById('confirmDialogCancelBtn');
            const confirmBtn = document.getElementById('confirmDialogOkBtn');

            messageSpan.textContent = message;

            const cleanup = (result) => {
                dialog.close();
                cancelBtn.removeEventListener('click', onCancel);
                confirmBtn.removeEventListener('click', onConfirm);
                dialog.removeEventListener('cancel', onCancel);
                resolve(result);
            };

            const onCancel = () => cleanup(false);
            const onConfirm = () => cleanup(true);

            cancelBtn.addEventListener('click', onCancel);
            confirmBtn.addEventListener('click', onConfirm);
            dialog.addEventListener('cancel', onCancel);

            dialog.showModal();
        });
    }

    async showConfirmDialog(ids, message) {
        let deleteMessage = message + ' this users';
        if (this.isContainsCurrentUser(ids))
            deleteMessage = message + ' yourself';
        return await this.createConfirmDialog(deleteMessage);
    }

    isContainsCurrentUser(users) {
        return users.indexOf(this.model.currentUser) !== -1;
    }

}