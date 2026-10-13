import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AcpCallbackComponent } from './acp-callback.component';

describe('AcpCallbackComponent', () => {
  let component: AcpCallbackComponent;
  let fixture: ComponentFixture<AcpCallbackComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AcpCallbackComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AcpCallbackComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
