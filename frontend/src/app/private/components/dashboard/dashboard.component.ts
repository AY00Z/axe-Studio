import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { ProjectCardComponent } from '../project-card/project-card.component';
import { CommonModule } from '@angular/common';
import { ProjectConfigService } from '../../services/project-config.service';
import { AuthService } from '../../../public/Services/auth.service';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AddComponent } from '../add/add.component';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzModalService } from 'ng-zorro-antd/modal';
import { AddProjectFormComponent } from '../add-project-form/add-project-form.component';
import { ProjectConfig } from '../../Models/project-config.interface';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ProjectCardComponent,
    NzSpinModule,
    AddComponent,
    NzCardModule, 
    NzIconModule
  ],
  providers: [NzModalService],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  title = 'axeStudio';
  subtitle = 'Lorem ipsum dolor sit amet , consectetur adipiscing elit.sodales metus';
  currentRoute: string = '';
  isLoading = true;
  projects: any[] = [];

  constructor(
    private projectConfigService: ProjectConfigService,
    private authService: AuthService,
    private message: NzMessageService,
    private router: Router,
    private modal: NzModalService
  ) {}

  ngOnInit(): void {
    this.updateCurrentRoute();
    this.loadProjects();
    
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.updateCurrentRoute();
      });
  }

  private updateCurrentRoute(): void {
    const segments = this.router.url.split('/');
    this.currentRoute = segments.length > 2 ? segments[2] : '';
  }

  loadProjects(): void {
    this.projectConfigService.getAllProjectsConfig().subscribe({
      next: (config) => {
        this.projects = config.projects;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading projects:', error);
        this.message.error('Failed to load projects');
        this.isLoading = false;
      }
    });
  }

  navigateToProject(projectPath: string): void {
    this.authService.redirectToApp2(projectPath);
  }

  isProjectActive(projectPath: string): boolean {  
    return this.currentRoute === projectPath;
  }

handleUpdateProject(project: ProjectConfig): void {
  const modalRef = this.modal.create({
    nzTitle: 'Update Project',
    nzContent: AddProjectFormComponent,
    nzData: { 
      projectToUpdate: project
    },
    nzFooter: null,
    nzWidth: '700px',
    nzOnOk: () => {
      this.loadProjects(); 
    }
  });

  modalRef.afterClose.subscribe(() => {
    this.loadProjects(); 
  });
  
}
}