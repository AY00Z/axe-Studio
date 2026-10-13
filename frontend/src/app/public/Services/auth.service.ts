import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environment/environment';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpHeaders } from '@angular/common/http';
import { throwError } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import { __await } from 'tslib';
import { Credentials } from '../../private/Models/RequestModels/Credentials.interface';



@Injectable({ 
  providedIn: 'root' 
})


export class AuthService {
  private jwtHelper = new JwtHelperService(); 

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}





  isLoggedIn(): boolean {
    const token = this.getToken();
    return token ? !this.jwtHelper.isTokenExpired(token) : false;
  }


login(username: string, password: string): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(
      `${environment.apiUrl}/auth/login`,
      { username, password }
    ).pipe(
      catchError(error => {
        if (error.status === 401) {
          throw new Error('Invalid username or password');
        } else if (error.status === 0) {
          throw new Error('Unable to connect to server');
        } else {
          throw new Error('An unexpected error occurred');
        }
      })
    );
  }


async getProjectUrl(projectPath: string): Promise<string> {
  try {
    // Fetch all project URLs from backend
    const urls = await firstValueFrom(
      this.http.get<Record<string, string>>(`${environment.apiUrl}/urls`)        
    );

    const projectKey = projectPath.toLowerCase();
    const projectUrl = urls[projectKey];
    
    if (!projectUrl) {
      throw new Error(`No URL configured for project: ${projectPath}`);
    }
    return projectUrl; 

  } catch (error) {
    console.error('Failed to fetch project URL:', error);
    throw error; 
  }
}





async redirectToApp2(projectPath: string): Promise<void> {
  const token = this.getToken();
  
  if (!token) {
    this.router.navigate(['/'], {
      queryParams: { returnUrl: projectPath }
    });
    return;
  }

  try {
    const projectUrl = await this.getProjectUrl(projectPath);
    
    const existingUrls = await firstValueFrom(
      this.http.get<string[]>(`${environment.apiUrl}/urls/GetProjectUrls`)
    );

    if (!existingUrls.some(url => url.toLowerCase() === projectUrl.toLowerCase())) {
      await firstValueFrom(
        this.http.post(`${environment.apiUrl}/urls/AddProjectUrl`, { url: projectUrl })
      );
    }
    
    let callbackUrl: string;

    if (projectPath.toLowerCase() === 'acp') {
         window.location.href = projectUrl;
        return;
    } else if (projectPath.toLowerCase() === 'brm' ) {

         if (!existingUrls.some(url => url.toLowerCase() === projectUrl.toLowerCase())){
          callbackUrl = `${projectUrl}/auth-callback`;
         } 
         else{
          callbackUrl = projectUrl;
         }
      

    } else {
      callbackUrl = existingUrls.some(url => url.toLowerCase() === projectUrl.toLowerCase()) 
        ? projectUrl 
        : `${projectUrl}/auth-callback`;
    }
    
    if (!callbackUrl) {
      throw new Error('Callback URL is undefined');
    }
    window.location.href = callbackUrl;
    
  } catch (error) {
    console.error('Redirect failed:', error);
    this.router.navigate(['/'], {
      queryParams: { 
        returnUrl: projectPath,
        error: 'redirect_failed'
      }
    });
  }
}



async logout(): Promise<void> {


  const authToken = localStorage.getItem('auth_token');
  if (authToken) {
    try {
      await firstValueFrom(
        this.http.post(`${environment.apiUrl}/auth/logout`, { token: authToken })
      );
    } catch (error) {
      console.error('Logout API error:', error);

    }
  }

  localStorage.removeItem('auth_token');

  const projectUrls = await firstValueFrom(
    this.http.get<string[]>(`${environment.apiUrl}/urls/GetProjectUrls`)
  );

  const baseUrl = `${window.location.protocol}//${window.location.host}`;
  const returnUrl = encodeURIComponent(baseUrl);

  // Log out from all apps in PojectsUrls.json
  projectUrls.forEach((url: string) => { 
    const iframe = document.createElement('iframe');
    iframe.src = `${url}/logout-handler?returnUrl=${returnUrl}`;
    iframe.style.display = 'none';
    document.body.appendChild(iframe);
    setTimeout(() => iframe.remove(), 5000);
  });

  setTimeout(() => {
    localStorage.clear();
    window.location.href = '/';
  }, 1000);
}



getToken(): string | null {
    return localStorage.getItem('auth_token');
 }

 storeToken(token: string): void {
    localStorage.setItem('auth_token', token);
}


  isTokenExpired(token: string): boolean {
    return this.jwtHelper.isTokenExpired(token);
  }
}