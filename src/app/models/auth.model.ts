export type Role = 'Administrador' | 'Tecnico' | 'Ciudadano';

// Lo que devuelven /api/auth/login y /api/auth/register
export interface AuthResponse {
  idToken: string;
  localId: string;
  email: string;
  role: Role;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
  username: string;
  phoneNumber: string;
  birthDate: string;
  country: string;
  bio?: string;
  zoneId?: string;
}
