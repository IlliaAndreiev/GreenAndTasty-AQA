import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OwnerGroupOrderPageComponent } from './owner-group-order-page.component';

describe('OwnerGroupOrderPageComponent', () => {
  let component: OwnerGroupOrderPageComponent;
  let fixture: ComponentFixture<OwnerGroupOrderPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OwnerGroupOrderPageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(OwnerGroupOrderPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
