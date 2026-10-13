import { Component } from '@angular/core';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzModalService } from 'ng-zorro-antd/modal';
import { AddProjectFormComponent } from '../add-project-form/add-project-form.component';

@Component({
  selector: 'app-add',
  standalone: true,
  imports: [NzCardModule, NzIconModule], 
  templateUrl: './add.component.html',
  styleUrls: ['./add.component.scss']
})
export class AddComponent {
  constructor(private modalService: NzModalService) {}

  showAddProjectModal(): void {
    this.modalService.create({
      nzTitle: 'Add New Project',
      nzContent: AddProjectFormComponent,
      nzFooter: null,
      nzWidth: '600px'
    });
  }
}