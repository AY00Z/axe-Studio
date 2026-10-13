import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { ProjectConfigService } from '../../services/project-config.service';
import { Router } from '@angular/router';
import { ProjectConfig } from '../../Models/project-config.interface';
import { NzModalRef, NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { Inject } from '@angular/core';
import { ConfigKeyMapping } from '../../Models/congif-key-mapping.interface';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-add-project-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NzFormModule,
    NzInputModule,
    NzButtonModule,
    NzDividerModule
  ],
  templateUrl: './add-project-form.component.html',
  styleUrls: ['./add-project-form.component.scss']
})
export class AddProjectFormComponent implements OnInit {
  projectForm: FormGroup;
  isLoading = false;
  isUpdateMode = false;
  originalPath = '';
  originalAcronym = '';

  constructor(
    private fb: FormBuilder,
    private projectService: ProjectConfigService,
    private notification: NzNotificationService,
    private router: Router,
    private modal: NzModalRef,
    @Inject(NZ_MODAL_DATA) public data: { projectToUpdate: ProjectConfig }
  ) {
    this.projectForm = this.fb.group({
      acronym: ['', [Validators.required, Validators.pattern(/^[A-Z]{2,4}$/)]],
      path: ['', [Validators.required, Validators.pattern(/^[a-z0-9-]+$/)]],
      title: ['', Validators.required],
      description: [''],
      configurationFiles: this.fb.array([this.createConfigFile()])
    });
  }

  ngOnInit(): void {
    if (this.data?.projectToUpdate) {
      this.isUpdateMode = true;
      this.originalPath = this.data.projectToUpdate.path;
      this.originalAcronym = this.data.projectToUpdate.acronym;
      this.loadProjectData();
    }
  }

  loadProjectData(): void {
    this.isLoading = true;
    
    // First patch the basic form values
    this.projectForm.patchValue({
      acronym: this.data.projectToUpdate.acronym,
      path: this.data.projectToUpdate.path,
      title: this.data.projectToUpdate.title,
      description: this.data.projectToUpdate.description || ''
    });

    // Then fetch the key mappings using the action endpoint
    this.projectService.getProjectKeyMappings(this.originalAcronym).subscribe({
      next: (response: any) => {
        // Handle different possible response formats
        const keyMappings = response.keyMappings || response || [];
        this.processKeyMappings(keyMappings);
        this.isLoading = false;
      },
     
    });
  }

  processKeyMappings(keyMappings: ConfigKeyMapping[]): void {
    // Clear existing configuration files
    this.configurationFiles.clear();

    if (keyMappings && keyMappings.length > 0) {
      keyMappings.forEach(mapping => {
        this.configurationFiles.push(this.fb.group({
          fileName: [mapping.configFileName, Validators.required],
          visibleKeys: [this.formatVisibleKeys(mapping.visibleKeys)]
        }));
      });
    } else {
      // Add default empty config if no mappings exist
      this.configurationFiles.push(this.createConfigFile());
    }
  }

  private formatVisibleKeys(keys: string[]): string {
    if (!keys || keys.length === 0) return '*';
    if (keys.length === 1 && keys[0] === '*') return '*';
    return keys.join(', ');
  }

  get configurationFiles(): FormArray {
    return this.projectForm.get('configurationFiles') as FormArray;
  }

  createConfigFile(): FormGroup {
    return this.fb.group({
      fileName: ['', Validators.required],
      visibleKeys: ['*']
    });
  }

  addConfigFile(): void {
    this.configurationFiles.push(this.createConfigFile());
  }

  removeConfigFile(index: number): void {
    this.configurationFiles.removeAt(index);
  }

  submitForm(): void {
    if (this.projectForm.valid) {
      this.isLoading = true;
      
      const projectConfig: ProjectConfig = {
        acronym: this.projectForm.value.acronym,
        path: this.projectForm.value.path,
        title: this.projectForm.value.title,
        description: this.projectForm.value.description,
        keyMappings: this.projectForm.value.configurationFiles.map((config: any) => ({
          configFileName: config.fileName,
          visibleKeys: this.parseVisibleKeys(config.visibleKeys)
        }))
      };

      if (this.isUpdateMode) {
        this.updateProject(projectConfig);
      } else {
        this.createProject(projectConfig);
      }
    } else {
      Object.values(this.projectForm.controls).forEach(control => {
        if (control.invalid) {
          control.markAsDirty();
          control.updateValueAndValidity({ onlySelf: true });
        }
      });
    }
  }

  private parseVisibleKeys(keysString: string): string[] {
    if (!keysString || keysString.trim() === '*') return ['*'];
    return keysString.split(',')
      .map(key => key.trim())
      .filter(key => key.length > 0);
  }

  createProject(projectConfig: ProjectConfig): void {
    this.projectService.createProject(projectConfig).subscribe({
      next: () => {
        this.notification.success(
          'Success',
          'Project created successfully!'
        );
        this.modal.close(true);
      },
      error: (err) => {
        this.isLoading = false;
        this.notification.error(
          'Error',
          err.message || 'Failed to create project'
        );
      }
    });
  }

  updateProject(projectConfig: ProjectConfig): void {
    this.projectService.updateProject(this.originalPath, projectConfig).subscribe({
      next: () => {
        this.notification.success(
          'Success',
          'Project updated successfully!'
        );
        this.modal.close(true);
      },
      error: (err) => {
        this.isLoading = false;
        this.notification.error(
          'Error',
          err.message || 'Failed to update project'
        );
      }
    });
  }
}