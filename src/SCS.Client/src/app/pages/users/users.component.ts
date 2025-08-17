import { Component, OnInit, signal, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { UserInfo } from '../../models/user-info.model';
import { ChipModule } from 'primeng/chip';
import { UserService } from '../../core/services/users.service';
import { ButtonModule } from "primeng/button";
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-users',
    templateUrl: './users.component.html',
    styleUrls: ['./users.component.scss'],
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TableModule,
        ChipModule,
        ButtonModule,
        DialogModule,
        InputTextModule,
        CheckboxModule
    ],
})
export class UsersComponent implements OnInit {
    users: WritableSignal<UserInfo[]> = signal([]);
    checked: boolean = true;
    showCreateModal: boolean = false;
    newUser: UserInfo = {
        empNo: '',
        name: '',
        isAdmin: false
    };

    constructor(private userService: UserService, private messageService: MessageService) { }

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

    onCreateUser(): void {
        this.showCreateModal = true;
    }

    async onSaveUser(): Promise<void> {
        if (!this.newUser.empNo || !this.newUser.name) {
            return;
        }

        try {
            await this.userService.createUserAsync(this.newUser);
            this.messageService.add({ severity: 'success', summary: 'Success', detail: 'User created successfully!' });
            await this.loadUsers(); // Refresh user list after creation
            this.closeModal();
        } catch (error) {
            console.error('Failed to create user:', error);
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to create user.' });
        }
    }

    closeModal(): void {
        this.showCreateModal = false;
        this.resetForm();
    }

    resetForm(): void {
        this.newUser = {
            empNo: '',
            name: '',
            isAdmin: false
        };
    }
}