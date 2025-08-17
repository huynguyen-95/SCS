import { Injectable } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';
import { ApiService } from './api.service';
import { Premise } from '../../models/premise.model';
import { IncidentInfo } from '../../models/incident-info.model';

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

    /**
     * Get incidents for a specific premise
     * @param premiseId The ID of the premise to get incidents for
     * @returns Promise<IncidentInfo[]>
     */
    getPremiseIncidentsListAsync(premiseId: number): Promise<IncidentInfo[]> {
        return firstValueFrom(this.apiService.get<IncidentInfo[]>(`${this.BASE_URL}/incidents/${premiseId}`));
    }
}
