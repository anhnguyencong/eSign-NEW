import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { environment } from "src/environments/environment";
import { ResponseModel } from "../models/response.model";
import { WorkContext } from "../_context/workcontext";
import { BaseService } from "./base.service";


@Injectable()
export class ServiceService extends BaseService {
    constructor(http: HttpClient, private workContext: WorkContext) {
        super(http);
        this.baseUrl = environment.apiRoot + '/Service';
    }

    status(): Observable<ResponseModel> {
        return this.doGet(this.baseUrl + '/status').pipe(
            map((data) => {
                return data;
            })
        );
    }

    start(): Observable<ResponseModel> {
        return this.doPost(this.baseUrl + '/Start', null).pipe(
            map((data) => {
                return data;
            })
        );
    }

    stop(): Observable<ResponseModel> {
        return this.doPost(this.baseUrl + '/Stop', null).pipe(
            map((data) => {
                return data;
            })
        );
    }


}
