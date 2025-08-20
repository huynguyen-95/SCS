import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { PremiseService } from './premise.service';
import { ApiService } from './api.service';
import { Premise } from '../../models/premise.model';
import { IncidentInfo } from '../../models/incident-info.model';
import { of, throwError } from 'rxjs';

describe('PremiseService', () => {
    let service: PremiseService;
    let apiServiceSpy: jasmine.SpyObj<ApiService>;

    const mockPremises: Premise[] = [
        {
            id: 1,
            name: 'Building A'
        },
        {
            id: 2,
            name: 'Building B'
        }
    ];

    const mockIncidents: IncidentInfo[] = [
        {
            date: '2024-01-01T10:00:00Z',
            description: 'Unauthorized access detected',
            filePath: '/uploads/incident1.jpg'
        },
        {
            date: '2024-01-02T14:30:00Z',
            description: 'Camera 3 offline',
            filePath: '/uploads/incident2.jpg'
        }
    ];

    beforeEach(() => {
        const spy = jasmine.createSpyObj('ApiService', ['get']);

        TestBed.configureTestingModule({
            providers: [
                PremiseService,
                { provide: ApiService, useValue: spy },
                provideZonelessChangeDetection()
            ]
        });

        service = TestBed.inject(PremiseService);
        apiServiceSpy = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('getPremiseListAsync', () => {
        it('should get premise list as Promise', async () => {
            apiServiceSpy.get.and.returnValue(of(mockPremises));

            const result = await service.getPremiseListAsync();

            expect(apiServiceSpy.get).toHaveBeenCalledWith('api/premise');
            expect(result).toEqual(mockPremises);
        });

        it('should handle error when getting premise list', async () => {
            const errorResponse = new Error('API Error');
            apiServiceSpy.get.and.returnValue(throwError(() => errorResponse));

            try {
                await service.getPremiseListAsync();
                fail('Should have thrown an error');
            } catch (error) {
                expect(error).toBe(errorResponse);
                expect(apiServiceSpy.get).toHaveBeenCalledWith('api/premise');
            }
        });
    });

    describe('getPremiseList', () => {
        it('should get premise list as Observable', (done) => {
            apiServiceSpy.get.and.returnValue(of(mockPremises));

            service.getPremiseList().subscribe({
                next: (result) => {
                    expect(apiServiceSpy.get).toHaveBeenCalledWith('api/premise');
                    expect(result).toEqual(mockPremises);
                    done();
                },
                error: done.fail
            });
        });

        it('should handle error when getting premise list as Observable', (done) => {
            const errorResponse = new Error('API Error');
            apiServiceSpy.get.and.returnValue(throwError(() => errorResponse));

            service.getPremiseList().subscribe({
                next: () => done.fail('Should have thrown an error'),
                error: (error) => {
                    expect(error).toBe(errorResponse);
                    expect(apiServiceSpy.get).toHaveBeenCalledWith('api/premise');
                    done();
                }
            });
        });
    });

    describe('getPremiseIncidentsListAsync', () => {
        it('should get incidents for a specific premise', async () => {
            const premiseId = 1;
            apiServiceSpy.get.and.returnValue(of(mockIncidents));

            const result = await service.getPremiseIncidentsListAsync(premiseId);

            expect(apiServiceSpy.get).toHaveBeenCalledWith(`api/premise/incidents/${premiseId}`);
            expect(result).toEqual(mockIncidents);
        });

        it('should handle error when getting premise incidents', async () => {
            const premiseId = 1;
            const errorResponse = new Error('Incidents not found');
            apiServiceSpy.get.and.returnValue(throwError(() => errorResponse));

            try {
                await service.getPremiseIncidentsListAsync(premiseId);
                fail('Should have thrown an error');
            } catch (error) {
                expect(error).toBe(errorResponse);
                expect(apiServiceSpy.get).toHaveBeenCalledWith(`api/premise/incidents/${premiseId}`);
            }
        });

        it('should handle different premise IDs', async () => {
            const premiseId = 999;
            const emptyIncidents: IncidentInfo[] = [];
            apiServiceSpy.get.and.returnValue(of(emptyIncidents));

            const result = await service.getPremiseIncidentsListAsync(premiseId);

            expect(apiServiceSpy.get).toHaveBeenCalledWith(`api/premise/incidents/${premiseId}`);
            expect(result).toEqual(emptyIncidents);
        });
    });
});
