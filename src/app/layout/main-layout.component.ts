import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-main-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="min-h-screen">
      <header class="sticky top-0 z-40 border-b border-slate-200 bg-white/90 backdrop-blur">
        <nav class="mx-auto flex max-w-6xl items-center gap-4 px-4 py-3">
          <a routerLink="/inicio" class="flex items-center gap-2 text-lg font-bold text-slate-900">
            <span class="grid h-8 w-8 place-items-center rounded-lg bg-brand-500 text-white">⚡</span>
            Apagón<span class="-ml-1.5 text-brand-600">Ya</span>
          </a>

          <div class="hidden flex-1 items-center gap-1 md:flex">
            @for (link of links; track link.path) {
              <a
                [routerLink]="link.path"
                routerLinkActive="bg-slate-100 text-slate-900"
                class="rounded-lg px-3 py-2 text-sm font-medium text-slate-600 hover:bg-slate-100"
              >
                {{ link.label }}
              </a>
            }
          </div>

          <div class="ml-auto flex items-center gap-3">
            <div class="hidden text-right sm:block">
              <p class="text-sm font-medium text-slate-900">{{ auth.displayName() }}</p>
              <p class="text-xs text-slate-500">{{ auth.role() }}</p>
            </div>
            <button
              type="button"
              class="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-100"
              (click)="logout()"
            >
              Salir
            </button>
          </div>
        </nav>

        <div class="flex gap-1 overflow-x-auto border-t border-slate-100 px-4 py-2 md:hidden">
          @for (link of links; track link.path) {
            <a
              [routerLink]="link.path"
              routerLinkActive="bg-slate-100 text-slate-900"
              class="whitespace-nowrap rounded-lg px-3 py-1.5 text-sm text-slate-600"
            >
              {{ link.label }}
            </a>
          }
        </div>
      </header>

      <main class="mx-auto max-w-6xl px-4 py-8">
        <router-outlet />
      </main>
    </div>
  `,
})
export class MainLayoutComponent {
  auth = inject(AuthService);
  private router = inject(Router);

  // Cuando se agreguen las pantallas de admin y técnico, se agregan acá
  links = [
    { path: '/inicio', label: 'Inicio' },
    { path: '/reportes', label: 'Reportes' },
    { path: '/reportes/mios', label: 'Mis reportes' },
  ];

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
