import { UserTableUI } from './user-table-ui.js';

document.addEventListener('DOMContentLoaded', () => {
    const elements = {
        filterInput: document.getElementById('filterInput'),
        thead: document.querySelector('thead'),
        tbody: document.getElementById('tableBody'),
        selectAllCheckbox: document.getElementById('selectAllCheckbox'),
        blockBtn: document.getElementById('blockSelectedBtn'),
        unblockBtn: document.getElementById('unblockSelectedBtn'),
        deleteSelectedBtn: document.getElementById('deleteSelectedBtn'),
        deleteNonVerifiedBtn: document.getElementById('deleteNonVerifiedBtn'),
        sortIcons: {
            name: document.getElementById('sortIcon-name'),
            email: document.getElementById('sortIcon-email'),
            status: document.getElementById('sortIcon-status'),
            lastseen: document.getElementById('sortIcon-lastSeen')
        },
        prevPageBtn: document.getElementById('prevPageBtn'),
        nextPageBtn: document.getElementById('nextPageBtn'),
        pageInfo: document.getElementById('pageInfo')
    };

    new UserTableUI(elements);
});