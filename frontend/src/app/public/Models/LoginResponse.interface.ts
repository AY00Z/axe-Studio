export interface LoginResponse {
  success: boolean;
  jwtToken?: string;
  errorType?: string;
  responseStatus: number;
}