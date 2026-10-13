import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzCollapseModule } from 'ng-zorro-antd/collapse';
import { NzEmptyModule } from 'ng-zorro-antd/empty';
import { ProjectConfigService } from '../../services/project-config.service';
import { ProjectRequest } from '../../Models/RequestModels/project-request.interface';
import { catchError, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { ConfigCenterComponent } from '../config-center/config-center.component';
import { ProjectDetailsComponent } from '../project-details/project-details.component';

@Component({
  selector: 'app-project',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    NzSpinModule,
    NzAlertModule,
    NzCardModule,
    NzCollapseModule,
    NzEmptyModule,
    ConfigCenterComponent,
    ProjectDetailsComponent
  ],
  templateUrl: './project.component.html',
  styleUrls: ['./project.component.scss']
})
export class ProjectComponent implements OnInit {
  configFiles: string[] = [];
  currentProject: any;
  isLoading = true;
  errorMessage: string | null = null;
  expandedConfigs: { [key: string]: boolean } = {};

  constructor(
    private route: ActivatedRoute,
    private projectConfig: ProjectConfigService
  ) {}

  ngOnInit(): void {
    this.loadProjectData();
  }

  // Helper method to extract just the filename for display
  getFileName(fullPath: string): string {
    // Handle both Windows and Unix paths
    return fullPath.split(/[\\/]/).pop() || fullPath;
  }

  private loadProjectData(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const projectName = params.get('projectName');
        if (!projectName) {
          throw new Error('Project name not found in URL');
        }

        return this.projectConfig.getAllProjectsConfig().pipe(
          switchMap(allProjects => {
            // Find project by name or acronym (case insensitive)
            this.currentProject = allProjects.projects.find(p => 
              p.acronym?.toLowerCase() === projectName.toLowerCase() || 
              p.acronym?.toLowerCase() === projectName.toLowerCase()
            );

            if (!this.currentProject) {
              throw new Error(`Project '${projectName}' not found`);
            }

            const request: ProjectRequest = {
              projectName: this.currentProject.acronym || projectName
            };

            return this.projectConfig.getProjectConfigsList(request);
          })
        );
      }),
      catchError(error => {
        this.errorMessage = error.message;
        return of([]);
      })
    ).subscribe({
      next: (configFiles) => {
        this.configFiles = configFiles;
        configFiles.forEach(file => this.expandedConfigs[file] = false);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load project:', error);
        this.isLoading = false;
      }
    });
  }

  toggleConfig(configFile: string): void {
    this.expandedConfigs[configFile] = !this.expandedConfigs[configFile];
  }

  trackByConfigName(index: number, configName: string): string {
    return configName;
  }
}