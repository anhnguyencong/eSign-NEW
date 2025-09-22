import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './_views/jobs/dashboard/dashboard.component';
import { LoginComponent } from './_views/users/login/login.component';
import { UserlayoutComponent } from './_views/shared/layouts/userlayout/userlayout.component';
import { AuthGuard } from './guards/auth.guard'

const routes: Routes = [
    { path: '', component: LoginComponent },
    {
        path: '',
        component: UserlayoutComponent,
        children: [
            { path: 'dashboard', component: DashboardComponent, pathMatch: 'full' },
        ], canActivate: [AuthGuard]
    },
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule],
})
export class AppRoutingModule { }
