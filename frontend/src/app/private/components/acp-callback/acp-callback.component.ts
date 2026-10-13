// acp-callback.component.ts
import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environment/environment';
import { Router } from '@angular/router';

import { NzSpinModule } from 'ng-zorro-antd/spin';

@Component({
  selector: 'app-acpcallback',
  templateUrl: './acp-callback.component.html',
  styleUrls: ['./acp-callback.component.scss'],
  imports :[ NzSpinModule],
  standalone:true
})
export class AcpCallbackComponent implements OnInit {
  constructor(private http: HttpClient, private router: Router) {}

  async ngOnInit() {
    try {
      // Call the NavigateToAcp endpoint
      const response = await firstValueFrom(
        this.http.post<{ redirectUrl: string }>(
          `${environment.apiUrl}/Auth/NavigateToAcp`, 
          null,
          { responseType: 'json' }
        )
      );

      // Redirect to the URL returned by NavigateToAcp
      if (response?.redirectUrl) {
        window.location.href = response.redirectUrl;
      } 

    else { window.location.href = 'http://localhost:90'; }

    } catch (error) {
      console.error('ACP navigation failed:', error);
      this.router.navigate(['/'], {
        queryParams: { 
          returnUrl: 'acp',
          error: 'acp_navigation_failed'
        }
      });
    }
  }
}