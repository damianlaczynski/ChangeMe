export interface AuthResponse {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  token: string;
  expiresAtUtc: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}
