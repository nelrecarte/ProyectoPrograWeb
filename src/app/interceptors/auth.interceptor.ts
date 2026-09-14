import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

// Le pega el token a cada petición y saca al usuario si el token ya venció.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const token = localStorage.getItem('token');

  const request = token
    ? req.clone({ setHeaders: { Authorization: 'Bearer ' + token } })
    : req;

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      // El token de Firebase dura una hora y no hay refresh token,
      // así que cuando vence toca volver a iniciar sesión.
      if (error.status === 401 && !req.url.includes('/auth/')) {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        router.navigate(['/login']);
      }
      return throwError(() => error);
    }),
  );
};
