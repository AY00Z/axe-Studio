import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfigCenterComponent } from './config-center.component';

describe('ConfigCenterComponent', () => {
  let component: ConfigCenterComponent;
  let fixture: ComponentFixture<ConfigCenterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfigCenterComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfigCenterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
