import { Injectable } from "@angular/core";
import { User } from "../Models/user";

@Injectable()
export class AuthService {

    private readonly DUMMY_USERS = [
        {username: 'user1', password: 'pass123', email: 'user@test.com', role: 'user' as const}, // defining as const so to avoid casting issue in login
        {username: 'admin1', password: 'pass123', email: 'admin@test.com', role: 'admin' as const}
    ]

    private readonly LOCAL_STORAGE_USER_KEY = 'currentUser';
    private readonly SESSION_STORAGE_LAST_ACTIVITY_KEY = 'lastActivity';
    private readonly SESSION_STORAGE_CART_KEY = 'currentCart';

    constructor() {}

    login(username: string, password: string) : User | null {
        const foundUser = this.DUMMY_USERS.find(
            u => u.username === username && u.password === password
        );

        if(foundUser){
            const loggedInUser: User = {
                username: foundUser.username,
                email: foundUser.email,
                role: foundUser.role
            };
            this.storeUserInLocalStorage(loggedInUser);
            this.updateSessionActivity();
            return loggedInUser;
        }
        return null;
    }

    private storeUserInLocalStorage(user: User): void {
        try {
            localStorage.setItem(this.LOCAL_STORAGE_USER_KEY, JSON.stringify(user));
            console.log('User stored in local storage:', user);
        } catch (e) {
            console.error("Error storing user in local storage:", e);
        }
    }

    getUserFromLocalStorage(): User | null {
        try {
        const userString = localStorage.getItem(this.LOCAL_STORAGE_USER_KEY);
        if (userString) {
            return JSON.parse(userString) as User;
        }
        } catch (e) {
        console.error("Error parsing user from local storage:", e);
        }
        return null;
    }

    isLoggedIn(): boolean {
        return this.getUserFromLocalStorage() !== null;
    }

    logout(): void {
        localStorage.removeItem(this.LOCAL_STORAGE_USER_KEY);
        sessionStorage.removeItem(this.SESSION_STORAGE_LAST_ACTIVITY_KEY);
        sessionStorage.removeItem(this.SESSION_STORAGE_CART_KEY); 
        console.log("User logged out and data cleared from storage.");
    }

    updateSessionActivity(): void {
        try {
        sessionStorage.setItem(this.SESSION_STORAGE_LAST_ACTIVITY_KEY, new Date().toISOString());
        console.log('Session activity updated:', new Date().toISOString());
        } catch (e) {
        console.error("Error updating session activity:", e);
        }
    }

    getLastSessionActivity(): string | null {
        try {
        return sessionStorage.getItem(this.SESSION_STORAGE_LAST_ACTIVITY_KEY);
        } catch (e) {
        console.error("Error retrieving session activity:", e);
        return null;
        }
    }

}