import { ConfigKeyMapping } from "./congif-key-mapping.interface";

export interface ProjectConfig {
   acronym: string;
  path: string;
  title: string;
  description: string;
  keyMappings: ConfigKeyMapping[];
}