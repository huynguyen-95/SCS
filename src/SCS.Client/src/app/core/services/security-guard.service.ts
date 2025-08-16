import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class SecurityGuardService {
    private readonly BASE_URL = 'api/security-guard';

    constructor(private apiService: ApiService) { }

    public dispatchSecurityGuard(premiseId: number, guardEmail: string): Observable<void> {
        return this.apiService.post<void>(`${this.BASE_URL}/dispatch-to-premise`, { premiseId, guardEmail });
    }
}
