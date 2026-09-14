import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="grid min-h-screen place-items-center px-4 py-10">
      <div class="w-full max-w-sm">
        <div class="mb-8 text-center">
          <span class="inline-grid h-12 w-12 place-items-center rounded-xl bg-brand-500 text-2xl">⚡</span>
          <h1 class="mt-4 text-2xl font-bold text-slate-900">
            Apagón<span class="text-brand-600">Ya</span>
          </h1>
          <p class="mt-1 text-sm text-slate-500">Reportá los cortes de luz de tu zona</p>
        </div>

        <form
          [formGroup]="form"
          (ngSubmit)="submit()"
          class="space-y-4 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
        >
          <div>
            <label for="email" class="mb-1 block text-sm font-medium text-slate-700">Correo</label>
            <input
              id="email"
              type="email"
              formControlName="email"
              class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm outline-none focus:border-brand-500"
            />
            @if (form.controls.email.touched && form.controls.email.invalid) {
              <p class="mt-1 text-xs text-red-600">Escribí un correo válido.</p>
            }
          </div>

          <div>
            <label for="password" class="mb-1 block text-sm font-medium text-slate-700">Contraseña</label>
            <input
              id="password"
              type="password"
              formControlName="password"
              class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm outline-none focus:border-brand-500"
            />
            @if (form.controls.password.touched && form.controls.password.invalid) {
              <p class="mt-1 text-xs text-red-600">Mínimo 6 caracteres.</p>
            }
          </div>

          <button
            type="submit"
            [disabled]="loading()"
            class="w-full rounded-lg bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-60"
          >
            {{ loading() ? 'Entrando…' : 'Entrar' }}
          </button>
        </form>

        <p class="mt-6 text-center text-sm text-slate-600">
          ¿No tenés cuenta?
          <a routerLink="/registro" class="font-semibold text-brand-700 hover:underline">Registrate</a>
        </p>
      </div>
    </div>
  `,
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);

  loading = signal(false);

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.auth.login(this.form.getRawValue()).subscribe({
      next: (profile) => {
        this.loading.set(false);
        this.router.navigate([this.auth.homeFor(profile.role)]);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(err.error?.error ?? 'No se pudo iniciar sesión.');
      },
    });
  }
}
