import { Component, Input, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { PopupMenuComponent } from '../popup-menu/popup-menu.component';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    NzLayoutModule,    
    NzIconModule,
    NzDropDownModule,
    PopupMenuComponent
  ],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {
  @Input() showDefaultIcon: boolean = true;
  @Input() title: string = 'Dashboard';
  currentRoute: string = '';

  constructor(private router: Router) {}

  ngOnInit() {
    this.currentRoute = this.router.url.split('/')[2] || '';
    
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.currentRoute = event.url.split('/')[2] || '';
      });
  }

  navigateToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  selectOption(option: string) {  
    this.router.navigate([`/dashboard/${option}/config`]);
   
  }
}