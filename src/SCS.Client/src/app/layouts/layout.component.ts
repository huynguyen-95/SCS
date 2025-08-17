import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { PremiseService } from '../core/services/premise.service';
import { Premise } from '../models/premise.model';
import { AuthService } from '../core/services/auth.service';
import { ToastModule } from 'primeng/toast';
import { MenuItem, MessageService } from 'primeng/api';
import { UserInfo } from '../models/user-info.model';
import { MenuModule } from 'primeng/menu';

@Component({
    selector: 'app-layout',
    templateUrl: './layout.component.html',
    styleUrls: ['./layout.component.scss'],
    standalone: true,
    imports: [
        ToastModule,
        CommonModule,
        RouterOutlet,
        RouterLink,
        RouterLinkActive,
        MenuModule
    ],
    providers: [MessageService]
})
export class LayoutComponent implements OnInit {
    sidebarVisible: boolean = true;
    expandedMenus: { [key: string]: boolean } = {};
    user: UserInfo | null = null;
    premises: Premise[] = [];
    items: MenuItem[] = [
        {
            label: "Logout",
            icon: "pi pi-sign-out",
            command: () => {
                this.authService.logout();
            }
        }
    ]

    constructor(
        private authService: AuthService,
        private premiseService: PremiseService
    ) {
        this.user = this.authService.getUserInfo();
    }

    ngOnInit(): void {
        this.premiseService.getPremiseList().subscribe(
            (premises: Premise[]) => {
                this.premises = premises;
            },
            error => {
                console.error('Error fetching premises:', error);
            }
        );
    }

    toggleSidebar(): void {
        this.sidebarVisible = !this.sidebarVisible;
    }

    toggleSubmenu(menuKey: string): void {
        this.expandedMenus[menuKey] = !this.expandedMenus[menuKey];
    }

    isSubmenuExpanded(menuKey: string): boolean {
        return this.expandedMenus[menuKey] || false;
    }
}
