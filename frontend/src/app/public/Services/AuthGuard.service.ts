import { Injectable } from '@angular/core';
import { 
  CanActivate, 
  Router, 
  ActivatedRouteSnapshot, 
  RouterStateSnapshot 
} from '@angular/router';
import { AuthService } from './auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environment/environment';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService, 
    private router: Router,
    private http: HttpClient
  ) {} 

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> {
    const token = this.authService.getToken();
    
    // Initial local checks
    if (!token || this.authService.isTokenExpired(token)) {
      this.handleInvalidToken();
      return of(false);
    }

    return this.validateTokenWithBackend(token).pipe(
      map(isValid => {
        if (!isValid) {
          this.handleInvalidToken();
        }
        return isValid;
      }),
      catchError(() => {
        this.handleInvalidToken();
        return of(false);
      })
    );
  }

  private validateTokenWithBackend(token: string): Observable<boolean> {                  
    return this.http.get<{ valid: boolean }>(
      `${environment.apiUrl}/auth/validate?token=${encodeURIComponent(token)}`
    ).pipe(
      map(response => response.valid)
    );
  }

  private handleInvalidToken(): void {
    this.authService.logout();
    this.router.navigate(['/'], {
      queryParams: { 
        returnUrl: this.router.url 
      }
    });
  }
}