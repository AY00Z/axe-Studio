import { Component, OnInit, Input } from '@angular/core';
import { FormBuilder, FormGroup, FormControl, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ProjectConfigService } from '../../services/project-config.service';
import { ProjectUrlService } from '../../services/ProjectUrlService';
import { finalize, switchMap, take } from 'rxjs/operators';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzInputNumberModule } from 'ng-zorro-antd/input-number';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { ConfigRequest } from '../../Models/RequestModels/config-request.interface';
import { UpdateFilteredConfigRequest } from '../../Models/RequestModels/update-filtered-config-request.interface';

interface ConfigField {
  key: string;
  displayKey: string;
  value: any;
  type: 'string' | 'number' | 'boolean' | 'object' | 'array';
  children?: ConfigField[];
  isParent?: boolean;
}

@Component({
  selector: 'app-config-center',
  templateUrl: './config-center.component.html',
  styleUrls: ['./config-center.component.scss'],
  standalone: true,
  imports: [
    ReactiveFormsModule,
    CommonModule,
    NzInputModule,
    NzButtonModule,
    NzAlertModule,
    NzInputNumberModule,
    NzIconModule,
    NzModalModule
  ]
})
export class ConfigCenterComponent implements OnInit {
  @Input() configName!: string;
  @Input() isExpanded: boolean | null = false;
  
  configForm: FormGroup;
  isLoading = false;
  saveSuccess = false;
  errorMessage = '';
  configFields: ConfigField[] = [];
  currentProjectPath: string | null = null;
  isSaving = false;
  isConfirmModalVisible = false;

  constructor(
    private fb: FormBuilder,
    private projectConfigService: ProjectConfigService,
    private projectUrlService: ProjectUrlService
  ) {
    this.configForm = this.fb.group({});
  }

  ngOnInit(): void {
    if (!this.configName) {
      this.errorMessage = 'No config file specified';
      return;
    }
    this.loadConfig();
  }

  private loadConfig(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.projectUrlService.getCurrentProject().pipe(
      switchMap((projectPath: string | null) => {
        if (!projectPath) {
          throw new Error('No project specified in URL');
        }
        this.currentProjectPath = projectPath;
        
        const request: ConfigRequest = {
          projectName: projectPath,
          configName: this.configName
        };
        
        return this.projectConfigService.getFilteredProjectConfig(request);
      }),
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: (config: any) => {
        this.configFields = this.parseConfigToFields(config);
        this.buildForm(this.configFields);
      },
      error: (error: any) => {
        console.error('Failed to load config:', error);
        this.errorMessage = error.message || 'Failed to load configuration. Please try again.';
      }
    });
  }

  showConfirmModal(): void {
    this.isConfirmModalVisible = true;
  }

  handleConfirmModalOk(): void {
    this.isConfirmModalVisible = false;
    this.saveConfiguration();
  }

  handleConfirmModalCancel(): void {
    this.isConfirmModalVisible = false;
  }

  saveConfiguration(): void {
    if (this.isSaving || !this.currentProjectPath) {
      return;
    }

    this.isSaving = true;
    this.saveSuccess = false;
    this.errorMessage = '';
    
    const formValue = this.configForm.value;
    const configToSave = this.reconstructConfig(formValue);
    
    const request: UpdateFilteredConfigRequest = {
      projectName: this.currentProjectPath,
      configName: this.configName,
      updatedConfig: configToSave
    };

    this.projectConfigService.updateSingleProjectConfig(request)
      .pipe(
        take(1),
        finalize(() => {
          this.isSaving = false;
        })
      )
      .subscribe({
        next: () => {
          this.saveSuccess = true;
          this.configForm.markAsPristine();
          setTimeout(() => {
            this.saveSuccess = false;
          }, 3000);
        },
        error: (error: any) => {
          console.error('Failed to save config:', error);
          this.errorMessage = error.message || 'Failed to save configuration. Please try again.';
        }
      });
  }

  private parseConfigToFields(config: any, parentKey = ''): ConfigField[] {
    if (!config || typeof config !== 'object') {
      return [];
    }

    return Object.entries(config).flatMap(([key, value]: [string, any]) => {
      const fullKey = parentKey ? `${parentKey}.${key}` : key;
      const displayKey = key;
      
      if (this.isNestedObject(value)) {
        const children = this.parseConfigToFields(value, fullKey);
        return {
          key: fullKey,
          displayKey: displayKey,
          value: null,
          type: Array.isArray(value) ? 'array' : 'object',
          children: children,
          isParent: true
        };
      }
      
      return {
        key: fullKey,
        displayKey: displayKey,
        value: value,
        type: this.determineType(value)
      };
    });
  }

  private isNestedObject(value: any): boolean {
    return typeof value === 'object' && 
           value !== null && 
           !(value instanceof Date) && 
           !(value instanceof RegExp);
  }

  private determineType(value: any): 'string' | 'number' | 'boolean' {
    if (typeof value === 'number') return 'number';
    if (typeof value === 'boolean') return 'boolean';
    return 'string';
  }

  private buildForm(fields: ConfigField[]): void {
    fields.forEach((field: ConfigField) => {
      if (field.children) {
        this.buildForm(field.children);
      } else {
        this.configForm.addControl(field.key, new FormControl(field.value));
      }
    });
  }

  private reconstructConfig(flatConfig: any): any {
    const result: Record<string, any> = {};
    
    Object.entries(flatConfig).forEach(([key, value]: [string, any]) => {
      this.setNestedValue(result, key.split('.'), value);
    });
    
    return result;
  }

  private setNestedValue(obj: Record<string, any>, keys: string[], value: any): void {
    const currentKey = keys[0];
    
    if (keys.length === 1) {
      obj[currentKey] = value;
      return;
    }
    
    if (!obj[currentKey]) {
      obj[currentKey] = {};
    }
    
    this.setNestedValue(obj[currentKey], keys.slice(1), value);
  }
}