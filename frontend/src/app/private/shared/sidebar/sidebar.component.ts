import { Component } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzModalModule, NzModalService } from 'ng-zorro-antd/modal';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../public/Services/auth.service';
import { HttpClientModule } from '@angular/common/http';

type ActiveIcon = 'home' | 'modification' | 'notifications' | 'profile' | 'settings' | 'logout';

interface RouteMap {
  home: string;
  modification: {
    base: string;
    subroutes: string[];
  };
  notifications: string;
  profile: string;
  settings: string;
  logout: string;
}

interface MenuItem {
  id: ActiveIcon;
  icon: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, NzMenuModule, NzIconModule, HttpClientModule, NzModalModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  activeIcon: ActiveIcon | null = null;
  currentCadreText: string = 'A';
  currentCadreColor: string = '#60B5FF';

  private readonly colorMap: Record<string, string> = {
    'BRM': '#5CB338',
    'ACP': '#60B5FF',
    'MLP': '#3674B5',
    'IAM': '#254D70',
    'GUI': '#B2A5FF',
    'DMS': '#E6521F'
  };

  private readonly routeMap: RouteMap = {
    home: '/dashboard',
    modification: {
      base: '/dashboard/acp',
      subroutes: ['/dashboard/acp', '/dashboard/gui', '/dashboard/iam', '/dashboard/bpm', '/dashboard/mlp']
    },
    notifications: '/notifications',
    profile: '/profile',
    settings: '/settings',
    logout: '/'
  };

  menuItems: MenuItem[] = [
    { id: 'profile', icon: 'user' },
    { id: 'settings', icon: 'setting' },
    { id: 'logout', icon: 'logout' }
  ];

  constructor(
    private router: Router,
    private authService: AuthService,
    private modal: NzModalService
  ) {
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => {
        this.updateActiveIconBasedOnRoute();
        this.updateCadreContentBasedOnRoute();
      });
  }

  private isModificationRoute(route: string): boolean {
    return this.routeMap.modification.subroutes.some(subroute => 
      route.startsWith(subroute)
    );
  }

  private updateActiveIconBasedOnRoute(): void {
    const currentRoute = this.router.url;
    
    if (currentRoute.startsWith(this.routeMap.home)) {
      this.activeIcon = 'home';
    } else if (this.isModificationRoute(currentRoute)) {
      this.activeIcon = 'modification';
    } else if (currentRoute.startsWith(this.routeMap.notifications)) {
      this.activeIcon = 'notifications';
    } else if (currentRoute.startsWith(this.routeMap.profile)) {
      this.activeIcon = 'profile';
    } else if (currentRoute.startsWith(this.routeMap.settings)) {
      this.activeIcon = 'settings';
    } else {
      this.activeIcon = null;
    }
  }

  private updateCadreContentBasedOnRoute(): void {
    const currentRoute = this.router.url;
    
    if (currentRoute.startsWith('/dashboard/BRM')) {
      this.currentCadreText = 'BRM';
      this.currentCadreColor = '#5CB338';
    } else if (currentRoute.startsWith('/dashboard/ACP')) {
      this.currentCadreText = 'ACP';
      this.currentCadreColor = this.colorMap['ACP'];
    } else if (currentRoute.startsWith('/dashboard/MLP')) {
      this.currentCadreText = 'MLP';
      this.currentCadreColor = this.colorMap['MLP'];
    } else if (currentRoute.startsWith('/dashboard/IAM')) {
      this.currentCadreText = 'IAM';
      this.currentCadreColor = this.colorMap['IAM'];
    } else if (currentRoute.startsWith('/dashboard/GUI')) {
      this.currentCadreText = 'GUI';
      this.currentCadreColor = this.colorMap['GUI'];
    } else if (currentRoute.startsWith('/dashboard/DMS')) {
      this.currentCadreText = 'DMS';
      this.currentCadreColor = this.colorMap['DMS'];
    } else {
      this.currentCadreText = 'A';
      this.currentCadreColor = '#1F509A';
    }
  }

  setActive(icon: ActiveIcon): void {
    this.activeIcon = icon;
    
    switch(icon) {
      case 'home':
        this.router.navigate([this.routeMap.home]);
        break;
      case 'modification':
        this.router.navigate([this.routeMap.modification.base]);
        break;
      case 'notifications':
        this.router.navigate([this.routeMap.notifications]);
        break;
      case 'profile':
        this.router.navigate([this.routeMap.profile]);
        break;
      case 'settings':
        this.router.navigate([this.routeMap.settings]);
        break;
      case 'logout':
        this.showLogoutConfirmation();
        break;
    }
  }

  showLogoutConfirmation(): void {
    this.modal.confirm({
      nzTitle: 'Confirm Logout',
      nzContent: 'Are you sure you want to logout?',
      nzOkText: 'Logout',
      nzOkType: 'primary',
      nzOkDanger: true,
      nzOnOk: () => this.authService.logout(),
      nzCancelText: 'Cancel',
      nzOnCancel: () => console.log('Logout cancelled')
    });
  }



}