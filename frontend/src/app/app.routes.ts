import { Routes } from '@angular/router';
import { LoginComponent } from './public/login/login.component';
import { PrivateComponent } from './private/private/private.component';
import { DashboardComponent } from './private/components/dashboard/dashboard.component';
import { NotFoundComponent } from './private/components/not-found/not-found.component';
import { AuthGuard } from './public/Services/AuthGuard.service';
import { LoggedInGuard } from './public/Services/LoggedInGuard.service';
import { ProjectComponent } from './private/components/project/project.component';
import { AcpCallbackComponent } from './private/components/acp-callback/acp-callback.component';
import { AuthCallbackComponent } from './private/components/auth-callback/auth-callback.component';


export const routes: Routes = [  
  { 
    path: '', 
    component: LoginComponent,
    canActivate: [LoggedInGuard] 
  },
  { 
    path: 'dashboard', 
    component: PrivateComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', component: DashboardComponent },
      { 
        path: ':projectName/config', 
        component: ProjectComponent
      }
    ]
  },
  { 
    path: 'acp-callback', 
    component: AcpCallbackComponent
  },
  { path: '404', component: NotFoundComponent },
  { path: '**', redirectTo: '/404' },
  { path: 'auth/callback', component: AuthCallbackComponent }
];