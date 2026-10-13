import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProjectConfigService } from '../../services/project-config.service';
import { ProjectUrlService } from '../../services/ProjectUrlService';
import { Observable, of } from 'rxjs';
import { switchMap, catchError, map } from 'rxjs/operators';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzTypographyModule } from 'ng-zorro-antd/typography';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { ProjectConfig } from '../../Models/project-config.interface';
import { ProjectsConfig } from '../../Models/projects-config.interface';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { Router } from '@angular/router';

@Component({
  selector: 'app-project-details',
  standalone: true,
  imports: [
    CommonModule,
    NzCardModule,
    NzAlertModule,
    NzTypographyModule,
    NzSpinModule,
    NzDropDownModule,
    NzIconModule
  ],
  templateUrl: './project-details.component.html',
  styleUrls: ['./project-details.component.scss']
})
export class ProjectDetailsComponent implements OnInit {
  project$: Observable<ProjectConfig | null> = of(null);
  isLoading = true;
  error: string | null = null;

  constructor(
    private projectConfigService: ProjectConfigService,
    private projectUrlService: ProjectUrlService,
    private modal: NzModalService,
    private message: NzMessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProjectData();
  }

  private loadProjectData(): void {
    this.project$ = this.projectUrlService.getCurrentProject().pipe(
      switchMap(projectPath => {
        this.isLoading = true;
        this.error = null;
        
        if (!projectPath) {
          this.error = 'No project specified in URL';
          this.isLoading = false;
          return of(null);
        }

        return this.projectConfigService.getAllProjectsConfig().pipe(
          map((config: ProjectsConfig) => {
            const project = config.projects.find(p => p.path.toLowerCase() === projectPath.toLowerCase());
            if (!project) {
              this.error = `Project '${projectPath}' not found`;
              return null;
            }
            return project;
          }),
          catchError(err => {
            this.error = 'Failed to load project data';
            console.error('Error loading project:', err);
            return of(null);
          })
        );
      }),
      map(project => {
        this.isLoading = false;
        return project;
      })
    );
  }

  showDeleteConfirm(project: ProjectConfig): void {
    this.modal.confirm({
      nzTitle: 'Are you sure you want to delete this project?',
      nzContent: `<b>${project.title} - ${project.acronym}</b> will be permanently deleted.`,
      nzOkText: 'Delete',
      nzOkType: 'primary',
      nzOkDanger: true,
      nzOnOk: () => this.deleteProject(project.path),
      nzCancelText: 'Cancel',
    });
  }

  deleteProject(projectPath: string): void {
    this.projectConfigService.deleteProject(projectPath).subscribe({
      next: () => {
        this.message.success('Project deleted successfully');
        this.router.navigate(['/dashboard']); 
      },
      error: (err) => {
        this.message.error('Failed to delete project');
        console.error('Error deleting project:', err);
      }
    });
  }
}