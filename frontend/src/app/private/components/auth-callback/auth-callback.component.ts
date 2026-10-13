import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../public/Services/auth.service';
import { environment } from '../../../../environment/environment';


@Component({
  selector: 'app-auth-callback',
  template: `
    <div class="loading-container">
     
      <p>Completing Microsoft login...</p>
    </div>
  `,
  styles: [`
    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100vh;
    }
  `]
})
export class AuthCallbackComponent implements OnInit {
    private apiUrl = environment.apiUrl;
  
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const code = params['code'];
      const state = params['state'];

      // 1. Verify state (CSRF protection)
      const storedState = sessionStorage.getItem('microsoft_oauth_state');
      sessionStorage.removeItem('microsoft_oauth_state');

      if (!state || state !== storedState) {
        this.router.navigate(['/login'], { 
          queryParams: { error: 'Invalid authentication state' }
        });
        return;
      }

      // 2. Exchange code for token
      this.http.post<any>(`${environment.apiUrl}/auth/microsoft-token`, { code })
        .subscribe({
          next: (response) => {
            // 3. Store token and redirect
            this.authService.storeToken(response.token);
            this.router.navigate(['/dashboard']);
          },
          error: (err) => {
            console.error('Token exchange failed:', err);
            this.router.navigate(['/login'], {
              queryParams: { error: 'microsoft_login_failed' }
            });
          }
        });
    });
  }
}