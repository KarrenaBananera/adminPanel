import { UserDto } from './user-dto.js';

export class ApiService {
    static API_BASE = '/api/users';

    async fetchUsers(params) {
        const { pageNumber, pageSize, searchTerm, sortBy, sortDescending } = params;
        const url = this._buildFetchUrl({ pageNumber, pageSize, searchTerm, sortBy, sortDescending });
        const data = await this._get(url);
        return this._mapUserListResponse(data);
    }

    async deleteUsers(ids) {
        await this._postWithHandler('Delete', ids);
    }

    async blockUsers(ids) {
        await this._postWithHandler('Block', ids);
    }

    async unblockUsers(ids) {
        await this._postWithHandler('Unblock', ids);
    }

    async clearNonVerifiedUsers(ids) {
        await this._postWithHandler('DeleteUnverified', ids);
    }

    async getCurrentUser() {
        const url = `${ApiService.API_BASE}?handler=CurrentUser`;
        const data = await this._get(url);
        var user = this._mapUser(data.item);
        return user;
    }

    static _getAntiForgeryToken() {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement?.value ?? '';
    }

    _buildFetchUrl({ pageNumber, pageSize, searchTerm, sortBy, sortDescending }) {
        const url = new URL(ApiService.API_BASE, window.location.origin);
        url.searchParams.set('PageNumber', pageNumber);
        url.searchParams.set('PageSize', pageSize);
        if (searchTerm) url.searchParams.set('SearchTerm', searchTerm);
        if (sortBy) {
            url.searchParams.set('SortItemsBy', sortBy);
            url.searchParams.set('SortDescending', String(sortDescending));
        }
        return url.toString();
    }

    async _get(url) {
        const response = await fetch(url, {
            method: 'GET',
            headers: this._jsonHeaders()
        });
        await this._throwIfUnauthorized(response);
        if (!response.ok) {
            throw new Error(`GET request failed: ${response.status} ${response.statusText}`);
        }
        return response.json();
    }

    async _post(url, body) {
        const response = await fetch(url, {
            method: 'POST',
            headers: this._postHeaders(),
            body: JSON.stringify(body)
        });
        await this._throwIfUnauthorized(response);
        if (!response.ok) {
            throw new Error(`POST request failed: ${response.status} ${response.statusText}`);
        }
    }

    async _postWithHandler(handler, ids) {
        const url = `${ApiService.API_BASE}?handler=${handler}`;
        await this._post(url, ids);
    }

    _jsonHeaders() {
        return {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        };
    }

    _postHeaders() {
        return {
            ...this._jsonHeaders(),
            'RequestVerificationToken': ApiService._getAntiForgeryToken()
        };
    }

    async _throwIfUnauthorized(response) {
        if (response.status === 401) {
            const error = new Error('Unauthorized');
            error.status = 401;
            throw error;
        }
    }

    _mapUserListResponse(data) {
        const items = data.items.map(item => this._mapUser(item));
        return { ...data, items };
    }

    _mapUser(item) {
        return new UserDto(
            item.id,
            item.name,
            item.userEmail,
            item.isUserBlocked,
            item.isUserEmailConfirmed,
            item.lastSeen,
            item.title
        );
    }
}