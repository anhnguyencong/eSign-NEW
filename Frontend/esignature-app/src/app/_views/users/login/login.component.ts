import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ResponseModel } from 'src/app/models/response.model';
import { LoginModel } from 'src/app/models/users/login.model';
import { WorkContext } from 'src/app/_context/workcontext';
import { EmailFormatValidator } from 'src/app/_helpers/utils.helper';
import { AuthenticationService } from 'src/app/_services/authentication.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
    hide: boolean = true;
    loginForm!: FormGroup;
    isSubmitted = false;
    isSubmitError = false;
    returnUrl!: string;
    errorMessage!: string;
    loginModel!: LoginModel;

    constructor(
        private router: Router,
        private activatedRoute: ActivatedRoute,
        private authService: AuthenticationService,
        private workContext: WorkContext,
        private formBuilder: FormBuilder) { }

    get f() { return this.loginForm.controls; }

    ngOnInit(): void {
        this.loginModel = new LoginModel();
        //this.loginModel.username = 'admin@gic.com.vn';
        //this.loginModel.password = 'ESignature@123';
        this.loginModel.username = '';
        this.loginModel.password = '';
        this.loginForm = this.formBuilder.group({
            email: [this.loginModel.username, [Validators.required, EmailFormatValidator()]],
            password: [this.loginModel.password, Validators.required]
        });
        // this.returnUrl = this.activatedRoute.snapshot.queryParams.returnUrl;
        // if (this.workContext.isAuthenticated() && !this.workContext.isTokenExpired(true)) {
        //     this.router.navigate(['/dashboard']);
        // }
    }

    onSubmit() {
        this.isSubmitted = true;

        if (this.loginForm.invalid) {
            this.isSubmitted = false;
            return;
        }

        const model = new LoginModel();
        model.username = this.f.email.value;
        model.password = this.f.password.value;

        this.authService.login(model).subscribe(res => {
            console.log(res);
            this.processDataLogin(res);
            this.isSubmitted = false;
        }, (e) => {
            console.log(e);
            this.isSubmitted = false;
            this.isSubmitError = true;
            this.errorMessage = 'User Name or Password is not correct. Please try again';
        });
    }

    processDataLogin(data: ResponseModel) {
        if (data.success) {
            this.workContext.saveToken(data.result);
            this.router.navigate(['/dashboard']);
        } else {
            this.isSubmitError = true;
            this.errorMessage = 'User Name or Password is not correct. Please try again';
        }
    }

    myFunction() {
        this.hide = !this.hide;
        console.log(this.hide);
    }
}
