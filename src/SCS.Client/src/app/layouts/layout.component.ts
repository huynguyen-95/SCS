import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
    selector: 'app-layout',
    templateUrl: './layout.component.html',
    styleUrls: ['./layout.component.scss'],
    standalone: true,
    imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive]
})
export class LayoutComponent {
    sidebarVisible: boolean = true;

    toggleSidebar(): void {
        this.sidebarVisible = !this.sidebarVisible;
    }
}
