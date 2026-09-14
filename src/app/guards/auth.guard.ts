import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// Rutas que necesitan sesión
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn()) {
    return true;
  }
  router.navigate(['/login']);
  return false;
};

// Login y registro: si ya hay sesión, no tiene sentido mostrarlos
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    return true;
  }
  router.navigate(['/inicio']);
  return false;
};

// Para las pantallas del administrador
export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAdmin()) {
    return true;
  }
  router.navigate(['/sin-permiso']);
  return false;
};

// Para las pantallas del técnico
export const tecnicoGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isTecnico()) {
    return true;
  }
  router.navigate(['/sin-permiso']);
  return false;
};
