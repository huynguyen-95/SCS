import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { Ripple, RippleModule } from 'primeng/ripple';
import { AvatarModule } from 'primeng/avatar';
import { StyleClass } from 'primeng/styleclass';
import { DrawerModule } from 'primeng/drawer';
import { AuthService } from '../core/services/auth.service';

@Component({
    selector: 'app-layout',
    templateUrl: './layout.component.html',
    styleUrls: ['./layout.component.scss'],
    standalone: true,
    imports: [
        CommonModule,
        RouterOutlet,
        RouterLink,
        RouterLinkActive
    ]
})
export class LayoutComponent {
    sidebarVisible: boolean = false;
    expandedMenus: { [key: string]: boolean } = {};
    user: UserInfo | null = null;

    constructor(private authService: AuthService) {
        this.user = this.authService.getUserInfo();
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
