import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import environment from '../../env';
import { UserInfo } from '../../models/user-info.model';
import { ErrorResponse, LoginResponse } from '../../models/auth-response.model';

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

  getUserInfo(): UserInfo | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      return jwtDecode<UserInfo>(token);
    } catch {
      this.removeToken();
      return null;
    }
  }

  isAuthenticated(): boolean {
    const userInfo = this.getUserInfo();
    if (!userInfo) return false;

    // Check if token has expired
    if (userInfo.exp) {
      const currentTime = Math.floor(Date.now() / 1000); // Current time in Unix timestamp
      if (userInfo.exp < currentTime) {
        this.removeToken(); // Remove expired token
        return false;
      }
    }

    return true;
  }

  hasRole(role: string): boolean {
    const userInfo = this.getUserInfo();
    return userInfo?.roles?.includes(role) || false;
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
