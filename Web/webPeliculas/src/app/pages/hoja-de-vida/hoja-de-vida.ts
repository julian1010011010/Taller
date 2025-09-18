import { Component } from '@angular/core';
import { MapComponent } from '../../components/map/map';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-hoja-de-vida',
  standalone: true,
  imports: [MapComponent],
  templateUrl: './hoja-de-vida.html',
  styles: ``
})
export class HojaDeVida {
  readonly name = 'Sergio Leonardo Martinez Castañeda';
readonly title = 'Desarrollador Fullstack';
readonly summary = 'Desarrollador de software con experiencia en el diseño e implementación de soluciones end-to-end, trabajando con Angular en el frontend y .NET en el backend. He participado en proyectos que requieren arquitectura sólida, mejores prácticas de desarrollo, pruebas automatizadas y despliegue en entornos productivos. Mi enfoque está en construir aplicaciones escalables, mantenibles y alineadas a las necesidades del cliente, integrando tanto la capa de presentación como la lógica de negocio y el acceso a datos.';

  readonly contacts = [
    { label: 'Email', value: 'mr.sergio1111@gmail.com', href: 'mailto:mr.sergio1111@gmail.com' },
    { label: 'Email alterno', value: 'selemaca11@gmail.com', href: 'mailto:selemaca11@gmail.com' },
    { label: 'Teléfono', value: '+57 314 3441078', href: 'tel:+573143441078' },
    { label: 'Ubicación', value: 'Bogotá, Colombia', href: 'https://maps.google.com/?q=Bogotá, Colombia' }
  ];

  readonly skills = [
    'Angular', 'TypeScript', '.NET', 'Bootstrap', 'SCSS', 'REST APIs',
    'Visual Studio', 'Visual Studio Code', 'GitHub', 'GitLab', 'Postman'
  ];

  readonly projects = [
    {
      name: 'Dragon Ball Z API',
      description: 'Trabajo realizado consumiendo datos públicos y presentándolos en una API desplegada.',
      url: 'https://dragonballzapi.onrender.com/'
    }
  ];

  readonly profileImg = '/assets/hoja-de-vida/foto.jpg';

  // Enlazar a un PDF público (pon el archivo en /public)
  readonly cvPdfUrl = '/Hoja%20de%20vida.pdf';
  readonly cvPdfSafeUrl: SafeResourceUrl;

  constructor(private sanitizer: DomSanitizer) {
    this.cvPdfSafeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.cvPdfUrl);
  }

  // Educación
  readonly education: Array<{
    title: string; institution: string; start?: string; end?: string; details?: string;
  }> = [
    {
      title: 'Ingeniería de Software (en proceso)',
      institution: 'Universidad Manuela Beltrán',
      start: '2021',
      end: 'Actualidad'
    }
  ];

  // Experiencia profesional
  readonly experience: Array<{
    role: string; company: string; start?: string; end?: string; description?: string;
  }> = [
    {
      role: 'Desarrollador de Software',
      company: 'Rastreo Satelital SAS',
      start: 'mayo 2023',
      end: 'junio 2024',
      description: 'Diseño y desarrollo de un sistema de formularios a la medida para controlar el tráfico de solicitudes de conductores. Implementación de lógica de negocio personalizada para la gestión eficiente de solicitudes. Tecnologías: .NET (backend) y Angular (frontend).'
    },
    {
      role: 'Desarrollador de Software',
      company: 'EscobarCSI',
      start: 'marzo 2022',
      end: 'abril 2023',
      description: 'Desarrollo de código funcional para la página web EscobarCSI utilizando Angular y .NET. Apoyo al área de soporte técnico: identificación, análisis y corrección de errores en la plataforma. Tecnologías: .NET (backend) y Angular (frontend).'
    }
  ];

  readonly certifications: Array<{ name: string; issuer?: string; year?: string; url?: string; }> = [];

  readonly languages: Array<{ name: string; level: string; }> = [
    { name: 'Español', level: 'Nativo' },
    { name: 'Inglés', level: 'En curso' }
  ];
}
