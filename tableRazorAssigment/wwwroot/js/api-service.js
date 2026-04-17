import { UserDto } from './user-dto.js';
export class ApiService {
    static API_BASE = '/api/users';

    async fetchUsers({ pageNumber, pageSize, searchTerm, sortBy, sortDescending }) {
        const url = new URL(ApiService.API_BASE, window.location.origin);
        console.log(url.toString());
        url.searchParams.set('PageNumber', pageNumber);
        url.searchParams.set('PageSize', pageSize);
        if (searchTerm) {
            url.searchParams.set('SearchTerm', searchTerm);
        }
        if (sortBy) {
            url.searchParams.set('SortItemsBy', sortBy);
            url.searchParams.set('SortDescending', sortDescending);
        }

        const response = await fetch(url.toString(), {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`Failed to fetch users: ${response.status} ${response.statusText}`);
        }

        const data = await response.json();
        const items = data.items.map(item => new UserDto(
            item.id,
            item.name,
            item.userEmail,
            item.isUserBlocked,
            item.isUserEmailConfirmed,
            item.lastSeen,
            item.title
        ));
        const result = { ...data, items };
        return result;
    }

    async deleteUsers(ids) {
        const response = await fetch(`${ApiService.API_BASE}?handler=Delete`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': this.getAntiForgeryToken()
            },
            body: JSON.stringify(ids)
        });

        if (!response.ok) {
            throw new Error(`Failed to delete users: ${response.status} ${response.statusText}`);
        }
    }

    async blockUsers(ids) {
        const response = await fetch(`${ApiService.API_BASE}?handler=Block`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': this.getAntiForgeryToken()
            },
            body: JSON.stringify(ids)
        });

        if (!response.ok) {
            throw new Error(`Failed to block users: ${response.status} ${response.statusText}`);
        }
    }

    async unblockUsers(ids) {
        const response = await fetch(`${ApiService.API_BASE}?handler=Unblock`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': this.getAntiForgeryToken()
            },
            body: JSON.stringify(ids)
        });

        if (!response.ok) {
            throw new Error(`Failed to unblock users: ${response.status} ${response.statusText}`);
        }
    }

    async clearNonVerifiedUsers(ids) {
        const response = await fetch(`${ApiService.API_BASE}?handler=DeleteUnverified`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': this.getAntiForgeryToken()
            },
            body: JSON.stringify(ids)
        });

        if (!response.ok) {
            throw new Error(`Failed to clear non-verified users: ${response.status} ${response.statusText}`);
        }
    }

    getAntiForgeryToken() {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    }
}