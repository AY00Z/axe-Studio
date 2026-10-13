import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NgStyle } from '@angular/common';
import { CommonModule } from '@angular/common';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { ProjectConfig } from '../../Models/project-config.interface';

@Component({
  selector: 'app-project-card',
  standalone: true,
  imports: [
    NzCardModule,
    NgStyle,
    CommonModule,
    NzIconModule,
    NzDropDownModule,
    NzMenuModule
  ],
  templateUrl: './project-card.component.html',
  styleUrls: ['./project-card.component.scss']
})
export class ProjectCardComponent {
  @Input() acronym: string = '';
  @Input() title: string = '';
  @Input() description: string = '';
  @Input() isActive: boolean = false;
  @Input() path: string = '';
  @Output() updateProject = new EventEmitter<ProjectConfig>();

  showDropdown = false;

  constructor(private router: Router) {}

  getAcronymColor(): string {
    const colorMap: {[key: string]: string} = {
      'BRM': '#5CB338',
      'ACP': '#60B5FF',
      'MLP': '#3674B5',
      'IAM': '#254D70',
      'GUI': '#B2A5FF',
      'DMS': '#E6521F'
    };
    return colorMap[this.acronym] || '#1a73e8';
  }

  onVisibleChange(visible: boolean): void {
    this.showDropdown = visible;
  }

  handleConfigurationClick(): void {
    this.showDropdown = false;
    this.router.navigate(['/dashboard', this.acronym, 'config']);
  }

  handleUpdateClick(): void {
    this.showDropdown = false;

    this.updateProject.emit({
      acronym: this.acronym,
      title: this.title,
      description: this.description,
      path: this.path,
     
      keyMappings: [] // You might need to fetch the actual config files
    });
  }
}