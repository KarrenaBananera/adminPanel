export class UserDto {
    constructor(id, name, userEmail, isUserBlocked, isUserEmailConfirmed, lastSeen, title = null) {
        this.id = id;
        this.name = name;
        this.userEmail = userEmail;
        this.isUserBlocked = isUserBlocked;
        this.isUserEmailConfirmed = isUserEmailConfirmed;
        this.lastSeen = lastSeen instanceof Date ? lastSeen : new Date(lastSeen);
        this.title = title;
    }

    get status() {
        if (this.isUserBlocked) return 'Blocked';
        if (!this.isUserEmailConfirmed) return 'Not verified';
        return 'Active';
    }

}
