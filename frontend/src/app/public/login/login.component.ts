import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AuthService } from '../Services/auth.service';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { EyeInvisibleOutline, EyeOutline } from '@ant-design/icons-angular/icons';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environment/environment';
import { firstValueFrom } from 'rxjs';


@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  imports: [ 
    CommonModule,
    ReactiveFormsModule,
    NzFormModule,
    NzAlertModule,
    NzInputModule,       
    NzButtonModule,
    NzSpinModule,
    NzIconModule,
    RouterModule  
  ]
})
export class LoginComponent implements OnInit {
   isMicrosoftLoading = false;
  loginForm = new FormGroup({
    username: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required])
  });
  
  isLoading = false;
  errorMessage: string | null = null;
  passwordVisible = false;
  icons = { EyeOutline, EyeInvisibleOutline };

  constructor(
    private authService: AuthService,
    private message: NzMessageService,
    private router: Router,
    private http: HttpClient
  ) {}

  async ngOnInit(): Promise<void> {
    try {

      // Clear all project URLs when login page loads
      await firstValueFrom(
        this.http.delete(`${environment.apiUrl}/urls/ClearProjectUrls`),

      );
    } catch (error) {
      console.error('Failed to clear Projects URLs:', error);
    }
  }

  onSubmit(): void {
    Object.values(this.loginForm.controls).forEach(control => {
      control.markAsDirty();
      control.updateValueAndValidity();
    });

    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = null;
      
      const { username, password } = this.loginForm.value;

      this.authService.login(username!, password!).subscribe({
        next: (response) => {
          this.authService.storeToken(response.token);
          this.router.navigate(['/dashboard']);
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          
          if (error.status === 401) {
            this.errorMessage = 'Invalid username or password';
          } else if (error.status === 0) {
            this.errorMessage = 'Unable to connect to server. Please check your connection.';
          } else {
            this.errorMessage = 'An unexpected error occured . Please try again later';
          }
          
          this.message.error(this.errorMessage);
        }
      });
    }
  }

  get username() {
    return this.loginForm.get('username');
  }

  get password() {
    return this.loginForm.get('password');
  }

  


 loginWithAzure(): void {
    this.isMicrosoftLoading = true;
    
    // 1. Generate and store state
    const state = Math.random().toString(36).substring(2);
    sessionStorage.setItem('microsoft_oauth_state', state);

    // 2. Build Keycloak Microsoft login URL
    const redirectUri = encodeURIComponent(
      `${window.location.origin}/auth/callback`
    );
    
    // 3. Redirect to Microsoft via Keycloak
    window.location.href = 
      `http://localhost:8080/realms/MyAppRealm/protocol/openid-connect/auth?` +
      `client_id=myapp-backend&` +
      `redirect_uri=${redirectUri}&` +
      `response_type=code&` +
      `scope=openid+email+profile&` +
      `kc_idp_hint=microsoft&` +
      `state=${state}`;
  }
}