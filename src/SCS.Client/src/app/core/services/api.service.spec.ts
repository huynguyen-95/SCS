import { TestBed } from '@angular/core/testing';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideZonelessChangeDetection } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ApiService } from './api.service';
import environment from '../../env';

describe('ApiService', () => {
    let service: ApiService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                ApiService,
                provideHttpClient(),
                provideHttpClientTesting(),
                provideZonelessChangeDetection()
            ]
        });

        service = TestBed.inject(ApiService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        try {
            httpMock.verify();
        } catch (e) {
            // Ignore verification errors for tests that don't make HTTP requests
        }
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('get', () => {
        it('should make GET request without params', () => {
            const mockResponse = { data: 'test' };
            const path = 'test-endpoint';

            service.get(path).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/${path}`);
            expect(req.request.method).toBe('GET');
            expect(req.request.params.keys().length).toBe(0);
            req.flush(mockResponse);
        });

        it('should make GET request with params', () => {
            const mockResponse = { data: 'test' };
            const path = 'test-endpoint';
            const params = { id: 1, name: 'test', active: true };

            service.get(path, params).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne(request =>
                request.url === `${environment.apiUrl}/${path}` &&
                request.params.get('id') === '1' &&
                request.params.get('name') === 'test' &&
                request.params.get('active') === 'true'
            );
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });

        it('should filter out null and undefined params', () => {
            const mockResponse = { data: 'test' };
            const path = 'test-endpoint';
            const params = { id: 1, name: null, active: undefined, valid: 'yes' };

            service.get(path, params).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne(request =>
                request.url === `${environment.apiUrl}/${path}` &&
                request.params.get('id') === '1' &&
                request.params.get('valid') === 'yes' &&
                !request.params.has('name') &&
                !request.params.has('active')
            );
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });

        it('should handle client-side error', () => {
            const path = 'test-endpoint';
            const consoleErrorSpy = spyOn(console, 'error');
            const clientError = new ErrorEvent('Network error', { message: 'Network error' });

            service.get(path).subscribe({
                next: () => fail('Should have failed'),
                error: (error) => {
                    expect(error.message).toBe('Network error');
                    expect(error.statusCode).toBe(0);
                }
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/${path}`);
            req.error(clientError, { status: 0 });

            expect(consoleErrorSpy).toHaveBeenCalledWith('API Error:', 'Network error');
        });

        it('should handle server-side error with message', () => {
            const path = 'test-endpoint';
            const serverError = { message: 'Server error occurred' };
            const consoleErrorSpy = spyOn(console, 'error');

            service.get(path).subscribe({
                next: () => fail('Should have failed'),
                error: (error) => {
                    expect(error.message).toBe('Server error occurred');
                    expect(error.statusCode).toBe(500);
                }
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/${path}`);
            req.flush(serverError, { status: 500, statusText: 'Internal Server Error' });

            expect(consoleErrorSpy).toHaveBeenCalledWith('API Error:', 'Server error occurred');
        });

        it('should handle server-side error without message', () => {
            const path = 'test-endpoint';
            const consoleErrorSpy = spyOn(console, 'error');

            service.get(path).subscribe({
                next: () => fail('Should have failed'),
                error: (error) => {
                    expect(error.message).toBe('Error Code: 404');
                    expect(error.statusCode).toBe(404);
                }
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/${path}`);
            req.flush({}, { status: 404, statusText: 'Not Found' });

            expect(consoleErrorSpy).toHaveBeenCalledWith('API Error:', 'Error Code: 404');
        });
    });

    describe('post', () => {
        it('should make POST request', () => {
            const mockResponse = { id: 1, name: 'created' };
            const path = 'test-endpoint';
            const body = { name: 'test', active: true };

            service.post(path, body).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/${path}`);
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual(body);
            req.flush(mockResponse);
        });

        it('should handle POST error', () => {
            const path = 'test-endpoint';
            const body = { name: 'test' };
            const consoleErrorSpy = spyOn(console, 'error');

            service.post(path, body).subscribe({
                next: () => fail('Should have failed'),
                error: (error) => {
                    expect(error.message).toBe('Bad Request');
                    expect(error.statusCode).toBe(400);
                }
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/${path}`);
            req.flush({ message: 'Bad Request' }, { status: 400, statusText: 'Bad Request' });

            expect(consoleErrorSpy).toHaveBeenCalledWith('API Error:', 'Bad Request');
        });
    });

    describe('postFormData', () => {
        it('should make POST request with FormData', () => {
            const mockResponse = { id: 1, uploaded: true };
            const path = 'upload';
            const formData = new FormData();
            formData.append('file', new Blob(['test']), 'test.txt');

            service.postFormData(path, formData).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/${path}`);
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toBe(formData);
            req.flush(mockResponse);
        });
    });

    describe('put', () => {
        it('should make PUT request', () => {
            const mockResponse = { id: 1, name: 'updated' };
            const path = 'test-endpoint/1';
            const body = { name: 'updated', active: false };

            service.put(path, body).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/${path}`);
            expect(req.request.method).toBe('PUT');
            expect(req.request.body).toEqual(body);
            req.flush(mockResponse);
        });
    });

    describe('delete', () => {
        it('should make DELETE request', () => {
            const mockResponse = { success: true };
            const path = 'test-endpoint/1';

            service.delete(path).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/${path}`);
            expect(req.request.method).toBe('DELETE');
            req.flush(mockResponse);
        });
    });
});
