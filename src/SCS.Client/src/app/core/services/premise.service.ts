import { Injectable } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';
import { ApiService } from './api.service';
import { Premise } from '../../models/premise.model';

@Injectable({
    providedIn: 'root'
})
export class PremiseService {
    private readonly BASE_URL = 'api/premise';

    constructor(private apiService: ApiService) { }

    /**
     * Get list of premises
     * @returns Promise<Premise[]>
     */
    getPremiseListAsync(): Promise<Premise[]> {
        return firstValueFrom(this.apiService.get<Premise[]>(this.BASE_URL));
    }

    /**
     * Get list of premises as Observable
     * @returns Observable<Premise[]>
     */
    getPremiseList(): Observable<Premise[]> {
        return this.apiService.get<Premise[]>(this.BASE_URL);
    }
}
