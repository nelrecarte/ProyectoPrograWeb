import { Role } from './auth.model';

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  username: string;
  phoneNumber: string;
  birthDate: string;
  country: string;
  bio: string;
  role: Role;
  zoneId: string;
  createdAt: string;
}

export interface UpdateProfileRequest {
  displayName: string;
  phoneNumber: string;
  country: string;
  bio: string;
  zoneId: string;
}
