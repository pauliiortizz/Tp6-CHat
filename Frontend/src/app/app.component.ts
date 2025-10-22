import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BlockingModalComponent } from './blocking-modal/blocking-modal.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, BlockingModalComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
}
