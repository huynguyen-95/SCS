import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { UserInfo } from '../../models/user-info.model';
import { firstValueFrom } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private readonly BASE_URL = 'api/users';

    constructor(private apiService: ApiService) { }

    /**
     * Get list of users
     * @returns Promise<UserInfo[]>
     */
    getUsersListAsync(): Promise<UserInfo[]> {
        return firstValueFrom(this.apiService.get<UserInfo[]>(this.BASE_URL));
    }

    /**
     * Create a new user
     * @param user UserInfo object to create
     * @returns Promise<UserInfo>
     */
    createUserAsync(user: UserInfo): Promise<UserInfo> {
        return firstValueFrom(this.apiService.post<UserInfo>(`${this.BASE_URL}`, user));
    }
}
