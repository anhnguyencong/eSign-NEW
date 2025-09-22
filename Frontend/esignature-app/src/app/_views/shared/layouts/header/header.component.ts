import {
    Component,
    OnInit,
    AfterViewInit,
    AfterContentInit,
} from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api'
import { MenuItem } from 'primeng/api';
import { ServiceService } from 'src/app/_services/service.service';
import { AuthenticationService } from 'src/app/_services/authentication.service';


export enum AppStatus {
    Start,
    Stop
}

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.css'],
    providers: [ConfirmationService, MessageService]
})
export class HeaderComponent implements OnInit {

    items: MenuItem[] = [];
    activeItem!: MenuItem;
    statusService: boolean = true;
    appStatus = AppStatus;

    constructor(private router: Router,
        private confirmationService: ConfirmationService,
        private messageService: MessageService,
        private authService: AuthenticationService,
        private serviceService: ServiceService) { }
    ngOnInit(): void {
        this.items = [
            { label: 'Dashboard', icon: 'pi pi-fw pi-home' }
        ];
        this.activeItem = this.items[0];
        this.serviceService.status().subscribe(data => {
            if (data.success) {
                this.statusService = !data.result;
            }
        });
    }

    logout() {
        this.authService.logout();
        this.router.navigate(['']);
    }

    start() {
        this.serviceService.start().subscribe(data => {
            if (data.success) {
                this.statusService = true;
                this.messageService.add({ severity: 'info', summary: 'Confirmed', detail: 'You have accepted' });
            }else{
                var strErr = '';
                for (var i = 0; i < data.errors.length; i++) {
                    strErr += data.errors[i];
                }
                this.messageService.add({ severity: 'error', summary: 'Error', detail: strErr });
            }
        });
    }

    stop() {
        this.serviceService.stop().subscribe(data => {
            if (data.success) {
                this.statusService = false;
                this.messageService.add({ severity: 'info', summary: 'Confirmed', detail: 'You have accepted' });
            } else {
                var strErr = '';
                for (var i = 0; i < data.errors.length; i++) {
                    strErr += data.errors[i];
                }
                this.messageService.add({ severity: 'error', summary: 'Error', detail: strErr });
            }
        });
    }

    confirmApp(status: AppStatus) {
        console.log(status);
        var strStatus = '';
        if (status == AppStatus.Start) {
            strStatus = 'start';
        }
        else if (status == AppStatus.Stop) {
            strStatus = 'stop';
        }
        this.confirmationService.confirm({
            message: 'Are you sure that you want to ' + strStatus + '?',
            header: 'Confirmation',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                //var str
                if (status == AppStatus.Start) {
                    this.start();
                }
                else if (status == AppStatus.Stop) {
                    this.stop();
                }

            },
        });
    }
}
