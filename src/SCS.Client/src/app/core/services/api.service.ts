import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import environment from '../../env';

@Injectable({
    providedIn: 'root'
})
export class ApiService {
    private readonly apiUrl = environment.apiUrl;

    constructor(private http: HttpClient) { }

    /**
     * Generic GET request
     * @param path - API endpoint path
     * @param params - Optional query parameters
     */
    get<T>(path: string, params?: { [key: string]: any }): Observable<T> {
        const httpParams = this.buildParams(params);
        return this.http
            .get<T>(`${this.apiUrl}/${path}`, { params: httpParams })
            .pipe(catchError(this.handleError));
    }

    /**
     * Generic POST request
     * @param path - API endpoint path
     * @param body - Request body
     */
    post<T>(path: string, body: any): Observable<T> {
        return this.http
            .post<T>(`${this.apiUrl}/${path}`, body)
            .pipe(catchError(this.handleError));
    }

    /**
     * Generic PUT request
     * @param path - API endpoint path
     * @param body - Request body
     */
    put<T>(path: string, body: any): Observable<T> {
        return this.http
            .put<T>(`${this.apiUrl}/${path}`, body)
            .pipe(catchError(this.handleError));
    }

    /**
     * Generic DELETE request
     * @param path - API endpoint path
     */
    delete<T>(path: string): Observable<T> {
        return this.http
            .delete<T>(`${this.apiUrl}/${path}`)
            .pipe(catchError(this.handleError));
    }

    /**
     * Build HTTP parameters
     */
    private buildParams(params?: { [key: string]: any }): HttpParams {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                if (params[key] !== null && params[key] !== undefined) {
                    httpParams = httpParams.append(key, params[key].toString());
                }
            });
        }
        return httpParams;
    }

    /**
     * Error handler
     */
    private handleError(error: HttpErrorResponse) {
        let errorMessage = 'An error occurred';

        if (error.error instanceof ErrorEvent) {
            // Client-side error
            errorMessage = error.error.message;
        } else {
            // Server-side error
            errorMessage = error.error?.message || `Error Code: ${error.status}`;
        }

        console.error('API Error:', errorMessage);
        return throwError(() => ({ message: errorMessage, statusCode: error.status }));
    }
}
