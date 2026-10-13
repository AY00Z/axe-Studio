import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { ProjectConfigService } from '../../services/project-config.service';
import { ProjectConfig } from '../../Models/project-config.interface';

@Component({
  selector: 'app-popup-menu',
  standalone: true,
  imports: [CommonModule, NzButtonModule, NzGridModule],
  templateUrl: './popup-menu.component.html',
  styleUrls: ['./popup-menu.component.scss']
})
export class PopupMenuComponent implements OnInit {
  @Input() currentRoute: string = '';
  @Output() optionSelected = new EventEmitter<string>();

  menuOptions: { label: string; value: string }[] = [];
  isLoading = true;

  constructor(private projectConfigService: ProjectConfigService) {}

  ngOnInit(): void {
    this.loadProjectAcronyms();
  }

  loadProjectAcronyms(): void {
    this.projectConfigService.getAllProjectsConfig().subscribe({
      next: (config: { projects: ProjectConfig[] }) => {
        this.menuOptions = config.projects.map(project => ({
          label: project.acronym,
          value: project.path
        }));
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load project acronyms:', error);
        this.isLoading = false;
      
      }
    });
  }

  selectOption(option: string) {
    this.optionSelected.emit(option);
  }

  isActive(option: string): boolean {
    return this.currentRoute === option;
  }
}