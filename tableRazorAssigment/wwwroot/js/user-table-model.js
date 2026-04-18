import { ApiService } from './api-service.js';

export class UserTableModel {
    static PAGE_SIZE = 20;

    constructor(apiService = new ApiService()) {
        this.apiService = apiService;
        this._state = {
            users: [],
            totalCount: 0,
            currentPage: 1,
            searchTerm: '',
            sortColumn: '',
            sortDescending: false,
        };
        this._listeners = [];
        this._searchDebounceTimer = null;
        this.currentUser = null;
    }
    get users() { return this._state.users; }
    get totalCount() { return this._state.totalCount; }
    get currentPage() { return this._state.currentPage; }
    get searchTerm() { return this._state.searchTerm; }
    get sortColumn() { return this._state.sortColumn; }
    get sortDescending() { return this._state.sortDescending; }
    get totalPages() { return Math.ceil(this._state.totalCount / UserTableModel.PAGE_SIZE) || 1; }

    subscribe(listener) {
        this._listeners.push(listener);
        listener(this._state);
    }

    _notify() {
        const stateCopy = { ...this._state };
        this._listeners.forEach(fn => fn(stateCopy));
    }

    async loadUsers() {
        try {
            const params = {
                pageNumber: this._state.currentPage,
                pageSize: UserTableModel.PAGE_SIZE,
                searchTerm: this._state.searchTerm || undefined,
                sortBy: this._state.sortColumn || undefined,
                sortDescending: this._state.sortDescending,
            };
            const response = await this.apiService.fetchUsers(params);
            this._state.users = response.items;
            this._state.totalCount = response.totalCount;
            if (this.currentUser == undefined) {
                let tempUser = await this.apiService.getCurrentUser();
                this.currentUser = tempUser.id;
            }

        } catch (error) {
            if (error && error.status === 401) {
                window.location.href = '/Login';
            } else {
                console.error('Failed to load users:', error);
            }
        } finally {
            this._notify();
        }
    }

    setSearchTerm(term) {
        clearTimeout(this._searchDebounceTimer);
        this._searchDebounceTimer = setTimeout(() => {
            this._state.searchTerm = term;
            this._state.currentPage = 1;
            this.loadUsers();
        }, 300);
    }

    setSort(column) {
        if (this._state.sortColumn === column) {
            this._state.sortDescending = !this._state.sortDescending;
        } else {
            this._state.sortColumn = column;
            this._state.sortDescending = false;
        }
        this._state.currentPage = 1;
        this.loadUsers();
    }

    prevPage() {
        if (this._state.currentPage > 1) {
            this._state.currentPage--;
            this.loadUsers();
        }
    }

    nextPage() {
        if (this._state.currentPage < this.totalPages) {
            this._state.currentPage++;
            this.loadUsers();
        }
    }

    async blockUsers(ids) {
        if (ids.length === 0) return;
        try {
            await this.apiService.blockUsers(ids);
            await this.loadUsers();
        } catch (e) {
            console.error('Block users failed:', e);
        }
    }

    async unblockUsers(ids) {
        if (ids.length === 0) return;
        try {
            await this.apiService.unblockUsers(ids);
            await this.loadUsers();
        } catch (e) {
            console.error('Unblock users failed:', e);
        }
    }

    async deleteUsers(ids) {
        if (ids.length === 0) return;
        try {
            await this.apiService.deleteUsers(ids);
            if (this._state.users.length === ids.length && this._state.currentPage > 1) {
                this._state.currentPage--;
            }
            await this.loadUsers();
        } catch (e) {
            console.error('Delete users failed:', e);
        }
    }

    async clearNonVerifiedUsers(selectedIds) {
        if (selectedIds.length === 0) return;
        const nonVerifiedIds = this._state.users
            .filter(u => selectedIds.includes(u.id) && u.status === 'Not verified')
            .map(u => u.id);
        if (nonVerifiedIds.length === 0) return;
        try {
            await this.apiService.clearNonVerifiedUsers(nonVerifiedIds);
            if (this._state.users.length === nonVerifiedIds.length && this._state.currentPage > 1) {
                this._state.currentPage--;
            }
            await this.loadUsers();
        } catch (e) {
            console.error('Clear non-verified users failed:', e);
        }
    }
}