import { ProjectRequest } from './project-request.interface';

export interface ConfigRequest extends ProjectRequest {
  configName: string;
}