import { Injectable } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { Observable } from 'rxjs';
import { map, distinctUntilChanged } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class ProjectUrlService {
  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {}

  getCurrentProject(): Observable<string | null> {
    return this.activatedRoute.firstChild?.params.pipe(
      map((params: Params) => {
        // For route like /dashboard/acp/config
        if (params['project']) {
          return params['project'].toLowerCase();
        }
        // For other cases
        const urlParts = this.router.url.split('/');
        const dashboardIndex = urlParts.indexOf('dashboard');
        return dashboardIndex > -1 && dashboardIndex + 1 < urlParts.length 
          ? urlParts[dashboardIndex + 1].toLowerCase()
          : null;
      }),
      distinctUntilChanged()
    ) ?? new Observable<string | null>(subscriber => subscriber.next(null));        
  }

  getFullUrl(): string {
    return this.router.url;
  }
}