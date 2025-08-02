import { Component } from '@angular/core';
import { LayoutComponent } from './shared/components/layout/layout.component';

@Component({
  selector: 'app-root',
  imports: [LayoutComponent],
  template: `<app-layout></app-layout>`,
})
export class App {
  protected title = 'meetme-angular';
}
