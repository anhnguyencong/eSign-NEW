import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { BaseService } from './base.service';
import { ResponseModel } from '../models/response.model';
import { WorkContext } from '../_context/workcontext';

@Injectable()
export class AuthenticationService extends BaseService {
    constructor(http: HttpClient, private workContext: WorkContext) {
        super(http);
        this.baseUrl = environment.apiRoot + '/User';
    }

    login(model: any): Observable<ResponseModel> {
        return this.doPost(this.baseUrl + '/login', model).pipe(
            map((data) => {
                return data;
            })
        );
    }

    logout() {
        this.workContext.removeToken();
    }
}
