import { Component } from '@angular/core';

@Component({
  selector: 'app-hoja-de-vida',
  standalone: true,
  imports: [],
  templateUrl: './hoja-de-vida.html',
  styles: ``
})
export class HojaDeVida {
  readonly name = 'Sergio Leonardo Martinez Castañeda';
  readonly title = 'Desarrollador Full-Stack';
  readonly summary = 'Presentación del estudiante tipo hoja de vida: apasionado por el desarrollo web y las apps de cine. Trabajo destacado: API Dragon Ball Z con catálogos y consumo de datos.';
  readonly contacts = [
    { label: 'Email', value: 'selemaca11@gmail.com', href: 'mailto:selemaca11@gmail.com' }, 
  ];
  readonly skills = ['Angular', 'TypeScript', '.Net',  'Bootstrap', 'SCSS', 'REST APIs'];
  readonly projects = [
    {
      name: 'Dragon Ball Z API',
      description: 'Trabajo realizado consumiendo datos públicos y presentándolos en una API desplegada.',
      url: 'https://dragonballzapi.onrender.com/'
    }
  ];
  readonly profileImg = '/assets/hoja-de-vida/foto.jpg';
}
