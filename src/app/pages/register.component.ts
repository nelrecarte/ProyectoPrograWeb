import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';
import { dateToIso } from '../shared/date-utils';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="grid min-h-screen place-items-center px-4 py-10">
      <div class="w-full max-w-xl">
        <div class="mb-8 text-center">
          <h1 class="text-2xl font-bold text-slate-900">Crear cuenta</h1>
          <p class="mt-1 text-sm text-slate-500">
            Te registrás como ciudadano. La zona se elige después, desde tu perfil.
          </p>
        </div>

        <form
          [formGroup]="form"
          (ngSubmit)="submit()"
          class="space-y-4 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
        >
          <div class="grid gap-4 sm:grid-cols-2">
            <div>
              <label for="displayName" class="mb-1 block text-sm font-medium text-slate-700">Nombre completo</label>
              <input id="displayName" formControlName="displayName" class="input" />
            </div>
            <div>
              <label for="username" class="mb-1 block text-sm font-medium text-slate-700">Usuario</label>
              <input id="username" formControlName="username" class="input" />
            </div>
            <div>
              <label for="email" class="mb-1 block text-sm font-medium text-slate-700">Correo</label>
              <input id="email" type="email" formControlName="email" class="input" />
              @if (form.controls.email.touched && form.controls.email.invalid) {
                <p class="mt-1 text-xs text-red-600">Escribí un correo válido.</p>
              }
            </div>
            <div>
              <label for="password" class="mb-1 block text-sm font-medium text-slate-700">Contraseña</label>
              <input id="password" type="password" formControlName="password" class="input" />
              @if (form.controls.password.touched && form.controls.password.invalid) {
                <p class="mt-1 text-xs text-red-600">Mínimo 6 caracteres.</p>
              }
            </div>
            <div>
              <label for="phoneNumber" class="mb-1 block text-sm font-medium text-slate-700">Teléfono</label>
              <input id="phoneNumber" formControlName="phoneNumber" class="input" />
            </div>
            <div>
              <label for="birthDate" class="mb-1 block text-sm font-medium text-slate-700">Fecha de nacimiento</label>
              <input id="birthDate" type="date" formControlName="birthDate" class="input" />
            </div>
            <div class="sm:col-span-2">
              <label for="country" class="mb-1 block text-sm font-medium text-slate-700">País</label>
              <input id="country" formControlName="country" class="input" />
            </div>
            <div class="sm:col-span-2">
              <label for="bio" class="mb-1 block text-sm font-medium text-slate-700">
                Sobre vos <span class="font-normal text-slate-400">(opcional)</span>
              </label>
              <textarea id="bio" rows="2" formControlName="bio" class="input"></textarea>
            </div>
          </div>

          <button
            type="submit"
            [disabled]="loading()"
            class="w-full rounded-lg bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-60"
          >
            {{ loading() ? 'Creando cuenta…' : 'Crear cuenta' }}
          </button>
        </form>

        <p class="mt-6 text-center text-sm text-slate-600">
          ¿Ya tenés cuenta?
          <a routerLink="/login" class="font-semibold text-brand-700 hover:underline">Entrá</a>
        </p>
      </div>
    </div>
  `,
  styles: `
    .input {
      width: 100%;
      border-radius: 0.5rem;
      border: 1px solid #cbd5e1;
      padding: 0.5rem 0.75rem;
      font-size: 0.875rem;
      outline: none;
    }
  `,
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);

  loading = signal(false);

  form = this.fb.nonNullable.group({
    displayName: ['', Validators.required],
    username: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    phoneNumber: ['', Validators.required],
    birthDate: ['', Validators.required],
    country: ['Honduras', Validators.required],
    bio: [''],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const values = this.form.getRawValue();
    this.loading.set(true);

    this.auth
      .register({ ...values, birthDate: dateToIso(values.birthDate) })
      .subscribe({
        next: (profile) => {
          this.loading.set(false);
          this.toast.success('¡Listo! Tu cuenta ya está creada.');
          this.router.navigate([this.auth.homeFor(profile.role)]);
        },
        error: (err) => {
          this.loading.set(false);
          this.toast.error(err.error?.error ?? 'No se pudo crear la cuenta.');
        },
      });
  }
}
