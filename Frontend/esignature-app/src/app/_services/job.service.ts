import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { BaseService } from './base.service';
import { ResponseModel } from '../models/response.model';
import { WorkContext } from '../_context/workcontext';

@Injectable()
export class JobService extends BaseService{
    constructor(http: HttpClient, private workContext: WorkContext) {
        super(http);
        this.baseUrl = environment.apiRoot + '/Job';
    }

    getList(model: any): Observable<ResponseModel> {
        return this.doGet(this.baseUrl + model).pipe(
            map((data) => {
                return data;
            })
        );
    }

    setPriority(model:any): Observable<ResponseModel>{
        return this.doPost(this.baseUrl+'/Priority', model).pipe(
            map((data) => {
                return data;
            })
        );
    }

    retryJob(id:string): Observable<ResponseModel>{
        return this.doPost(this.baseUrl+'/Retry/' + id, '').pipe(
            map((data) => {
                return data;
            })
        );
    }

    retryByBatchId(model: any): Observable<ResponseModel>{
        var data = {'BatchId': model}
        return this.doPost(this.baseUrl+'/RetryByBatchId', data).pipe(
            map((data) => {
                return data;
            })
        );
    }

    retryCallback(id:string): Observable<ResponseModel>{
        return this.doPost(this.baseUrl+'/RetryCallback/' + id, '').pipe(
            map((data) => {
                return data;
            })
        );
    }

    retryCallbackByBatchId(model: any): Observable<ResponseModel>{
        var data = {'BatchId': model}
        return this.doPost(this.baseUrl+'/RetryCallbackByBatchId', data).pipe(
            map((data) => {
                return data;
            })
        );
    }
}
