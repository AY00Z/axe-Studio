import { ProjectRequest } from './project-request.interface';
import { ConfigKeyMapping } from '../congif-key-mapping.interface';

export interface UpdateKeyMappingsRequest extends ProjectRequest {
  keyMappings: ConfigKeyMapping[];
}