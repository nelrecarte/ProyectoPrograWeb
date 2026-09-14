import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  template: `
    <div class="grid min-h-screen place-items-center px-4">
      <div class="text-center">
        <p class="text-6xl font-black text-slate-300">404</p>
        <h1 class="mt-2 text-xl font-bold text-slate-900">Esa página no existe</h1>
        <a routerLink="/inicio" class="mt-6 inline-block rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white">
          Ir al inicio
        </a>
      </div>
    </div>
  `,
})
export class NotFoundComponent {}
