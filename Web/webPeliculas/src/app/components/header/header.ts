import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  styles: ``
})
export class Header {
  open = signal(false);
  toggle() { this.open.set(!this.open()); }
}
