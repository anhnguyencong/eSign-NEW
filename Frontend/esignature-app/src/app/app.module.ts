import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ClipboardModule } from 'ngx-clipboard';

import { AppComponent } from './app.component';
import { WorkContext } from './_context/workcontext';
import { AuthenticationService } from './_services/authentication.service';
import { JobService } from './_services/job.service';
import { FileService } from './_services/file.service';
import { TokenInterceptor } from './_appload/token.interceptor';
import { ServiceService } from './_services/service.service';

import { AuthGuard } from './guards/auth.guard'

import { FooterComponent } from './_views/shared/layouts/footer/footer.component';
import { HeaderComponent } from './_views/shared/layouts/header/header.component';
import { DashboardComponent } from './_views/jobs/dashboard/dashboard.component';
import { LoginComponent } from './_views/users/login/login.component';
import { UserlayoutComponent } from './_views/shared/layouts/userlayout/userlayout.component';

// primeng
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { MessagesModule } from 'primeng/messages';
import { MessageModule } from 'primeng/message';
import { TabMenuModule } from 'primeng/tabmenu';
import { DropdownModule } from 'primeng/dropdown';
import { ContextMenuModule } from 'primeng/contextmenu';
import { PaginatorModule } from 'primeng/paginator';
import { PasswordModule } from 'primeng/password';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { ConfirmationService, MessageService } from 'primeng/api';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { InputNumberModule } from 'primeng/inputnumber';
import {ConfirmDialogModule} from 'primeng/confirmdialog';
import {ToastModule} from 'primeng/toast';

@NgModule({
    declarations: [
        AppComponent,
        LoginComponent,
        DashboardComponent,
        UserlayoutComponent,
        FooterComponent,
        HeaderComponent,
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        AppRoutingModule,
        FormsModule,
        ReactiveFormsModule,
        HttpClientModule,
        TableModule,
        InputTextModule,
        InputNumberModule,
        DialogModule,
        ButtonModule,
        MessageModule,
        MessagesModule,
        TabMenuModule,
        DropdownModule,
        ContextMenuModule,
        PaginatorModule,
        PasswordModule,
        TooltipModule, ConfirmPopupModule,
        ClipboardModule, OverlayPanelModule,
        ConfirmDialogModule, ToastModule
    ],
    providers: [
        AuthenticationService,
        JobService,
        FileService,
        WorkContext,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: TokenInterceptor,
            multi: true,
        },
        ConfirmationService,
        MessageService,
        AuthGuard,
        ServiceService
    ],
    bootstrap: [AppComponent],
})
export class AppModule { }
