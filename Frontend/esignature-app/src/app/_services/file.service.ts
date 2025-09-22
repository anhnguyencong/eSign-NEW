import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient, HttpEvent, HttpHeaders, HttpRequest } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { BaseService } from './base.service';
import { ResponseModel } from '../models/response.model';
import { WorkContext } from '../_context/workcontext';


@Injectable()
export class FileService extends BaseService {
    constructor(http: HttpClient, private workContext: WorkContext) {
        super(http);
        this.baseUrl = environment.apiRoot + '/File';
    }
    
    download(url:string, token:string){
        var headers = new HttpHeaders();
        headers = headers.append('ES-Token', token);
        return this.httpClient.get(url, {responseType: 'arraybuffer', headers : headers});
    }
}