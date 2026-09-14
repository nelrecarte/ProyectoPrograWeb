import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-forbidden',
  imports: [RouterLink],
  template: `
    <div class="mx-auto max-w-lg rounded-xl border border-slate-200 bg-white p-8 text-center">
      <p class="text-4xl">🔒</p>
      <h1 class="mt-3 text-xl font-bold text-slate-900">Esta sección no es para tu rol</h1>
      <p class="mt-2 text-sm text-slate-600">
        Si te acaban de cambiar el rol, cerrá sesión y volvé a entrar: el rol viaja dentro del token.
      </p>
      <a routerLink="/inicio" class="mt-6 inline-block rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white">
        Volver al inicio
      </a>
    </div>
  `,
})
export class ForbiddenComponent {}
