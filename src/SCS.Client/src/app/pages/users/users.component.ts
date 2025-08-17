import { Component, OnInit, signal, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { UserInfo } from '../../models/user-info.model';
import { ChipModule } from 'primeng/chip';
import { UserService } from '../../core/services/users.service';

@Component({
    selector: 'app-users',
    templateUrl: './users.component.html',
    styleUrls: ['./users.component.scss'],
    standalone: true,
    imports: [CommonModule, TableModule, ChipModule],
})
export class UsersComponent implements OnInit {
    users: WritableSignal<UserInfo[]> = signal([]);
    checked: boolean = true;

    constructor(private userService: UserService) { }

    async ngOnInit() {
        await this.loadUsers();
    }

    private async loadUsers(): Promise<void> {
        try {
            const users = await this.userService.getUsersListAsync();
            this.users.set(users);
        } catch (error) {
            console.error('Failed to load users:', error);
            // Handle error appropriately, e.g., show a message to the user
        }
    }

    getExpiryDate(exp: number): Date {
        return new Date(exp * 1000);
    }

    getExpiryClass(exp: number): string {
        const now = Math.floor(Date.now() / 1000);
        const hoursUntilExpiry = (exp - now) / 3600;

        if (hoursUntilExpiry < 24) {
            return 'expiry-soon';
        } else if (hoursUntilExpiry < 48) {
            return 'expiry-warning';
        }
        return 'expiry-normal';
    }
}
