import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

@Component({
    selector: 'app-premise',
    templateUrl: './premise.component.html',
    styleUrls: ['./premise.component.scss'],
    standalone: true,
    imports: [CommonModule]
})
export class PremiseComponent implements OnInit {
    premiseId: number = 0;

    constructor(private route: ActivatedRoute) { }

    ngOnInit() {
        this.route.params.subscribe(params => {
            this.premiseId = +params['id']; // Convert string to number using '+'
        });
    }
}
