import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable()
export class BaseService {
    headers: HttpHeaders;
    httpClient: HttpClient;
    protected baseUrl: string;
    constructor(httpclient: HttpClient) {
        this.headers = new HttpHeaders();
        this.headers = this.headers.append('accept', 'application/json');
        this.headers = this.headers.append('content-type', 'application/json');
        this.headers = this.headers.append('ES-Token', 'AAAAAAAA-B843-CE52-E477-D29C876EA1D1');
        this.httpClient = httpclient;
        this.baseUrl = '';
    }

    doGet(apiUrl: string) {
        return this.httpClient.get<any>(apiUrl, { headers: this.headers });
    }

    doPost(apiUrl: string, body: any) {
        return this.httpClient.post<any>(apiUrl, body, {
            headers: this.headers,
        });
    }

    doPut(apiUrl: string, body: any) {
        return this.httpClient.put<any>(apiUrl, body, {
            headers: this.headers,
        });
    }

    doDelete(apiUrl: string) {
        return this.httpClient.delete<any>(apiUrl, { headers: this.headers });
    }
}
