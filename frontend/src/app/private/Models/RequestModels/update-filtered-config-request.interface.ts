import { ConfigRequest } from './config-request.interface';

export interface UpdateFilteredConfigRequest extends ConfigRequest {
  updatedConfig: any; // Use a more specific type if possible
}