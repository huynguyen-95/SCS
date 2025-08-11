import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import environment from '../../env';

interface LoginResponse {
  token: string;
  message?: string;
}

interface ErrorResponse {
  message: string;
  statusCode: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  public readonly TOKEN_KEY = 'access_token';

  constructor(
    private router: Router,
    private http: HttpClient
  ) { }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      // Add your token validation logic here
      // For now, just checking if token exists
      return true;
    } catch {
      this.removeToken();
      return false;
    }
  }

  login(empNo: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/api/authentication/login`, { empNo })
      .pipe(
        tap(response => {
          if (response.token) {
            this.setToken(response.token);
          }
        }),
        catchError((error: HttpErrorResponse) => {
          let errorMessage: string;

          if (error.error instanceof ErrorEvent) {
            // Client-side error
            errorMessage = 'Network error occurred. Please try again.';
          } else {
            // Server-side error
            const serverError = error.error as ErrorResponse;
            errorMessage = serverError.message || 'Login failed. Please try again.';
          }

          return throwError(() => ({ message: errorMessage, statusCode: error.status }));
        })
      );
  }

  logout(): void {
    this.removeToken();
    this.router.navigate(['/login']);
  }
}
