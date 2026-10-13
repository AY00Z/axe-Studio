import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environment/environment';
import { ProjectConfig } from '../Models/project-config.interface';
import { ProjectsConfig } from '../Models/projects-config.interface';
import { ProjectRequest } from '../Models/RequestModels/project-request.interface';
import { ConfigRequest } from '../Models/RequestModels/config-request.interface';
import { UpdateFilteredConfigRequest } from '../Models/RequestModels/update-filtered-config-request.interface';
import { UpdateKeyMappingsRequest } from '../Models/RequestModels/update-key-mapping-request.interface';
import { ConfigKeyMapping } from '../Models/congif-key-mapping.interface';

@Injectable({
  providedIn: 'root'
})
export class ProjectConfigService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /* Existing Methods (unchanged) */
  getAllProjectsConfig(): Observable<ProjectsConfig> {
    return this.http.get<ProjectConfig[]>(`${this.apiUrl}/projects`).pipe(
      map(projects => ({ projects }))
    );
  }

  updateProjectsConfig(projects: ProjectConfig[]): Observable<any> {
    const config: ProjectsConfig = { projects };
    return this.http.put(`${this.apiUrl}/projects/UpdateAllProjects`, config);
  }

  getProjectConfigsList(request: ProjectRequest): Observable<string[]> {
    return this.http.post<string[]>(`${this.apiUrl}/projects/GetProjectConfigsList`, request);
  }

  getSingleProjectConfig(request: ConfigRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/projects/GetSingleProjectConfig`, request);
  }

  updateSingleProjectConfig(request: UpdateFilteredConfigRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/projects/UpdateFilteredProjectConfig`, request);
  }

  getFilteredProjectConfig(request: ConfigRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/projects/GetFilteredProjectConfig`, request);
  }



getProjectKeyMappings(acronym: string): Observable<ConfigKeyMapping[]> {
  return this.http.post<ConfigKeyMapping[]>(
    `${this.apiUrl}/GetProjectKeyMappings`,
    { projectName: acronym }  
  );
}



  updateKeyMappings(request: UpdateKeyMappingsRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/projects/UpdateKeyMappings`, request);
  }

  updateFilteredProjectConfig(request: UpdateFilteredConfigRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/projects/UpdateFilteredProjectConfig`, request);
  }

  getProjectConfig(projectPath: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/projects/${projectPath}/config`);
  }

  updateProjectConfig(projectPath: string, config: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/projects/${projectPath}/configs`, config);
  }

  getProject(path: string): Observable<ProjectConfig> {
    return this.http.get<ProjectConfig>(`${this.apiUrl}/projects/${path}`);
  }



  /* New CQRS Methods */
  createProject(project: ProjectConfig): Observable<ProjectConfig> {
    return this.http.post<ProjectConfig>(`${this.apiUrl}/projectsCrud`, project);
  }

  updateProject(path: string, project: ProjectConfig): Observable<ProjectConfig> {
    return this.http.put<ProjectConfig>(`${this.apiUrl}/projectsCrud/${path}`, project);
  }

  deleteProject(path: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/projectsCrud/${path}`);
  }



}