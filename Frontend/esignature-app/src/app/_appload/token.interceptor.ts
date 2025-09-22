import { Injectable } from '@angular/core';
import {
    HttpInterceptor,
    HttpRequest,
    HttpHandler,
    HttpEvent,
    HttpHeaders,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { WorkContext } from '../_context/workcontext';
import { AuthenticationService } from '../_services/authentication.service';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
    constructor(
        private authenticationService: AuthenticationService,
        private workContext: WorkContext
    ) { }

    setHeaders(request: HttpRequest<any>, token: string | string[]) {
        let headers = new HttpHeaders();
        headers = headers.append('Authorization', token);
        if (
            request.url.includes('/medias') ||
            request.url.includes('/check-post-files')
        ) {
        } else {
            headers = headers.append('Content-Type', 'application/json');
            headers = headers.append('Cache-Control', 'no-cache');
            headers = headers.append('Pragma', 'no-cache');
        }
        return request.clone({ headers });
    }

    intercept(
        request: HttpRequest<any>,
        next: HttpHandler
    ): Observable<HttpEvent<any>> {
        if (
            request.url.includes('/login') ||
            request.url.includes('/appconfig.json')
        ) {
            return next.handle(request);
        }

        const token = this.workContext.getToken();

        if (token) {
            const authReq = this.setHeaders(request, this.jwtToken(token));
            return next.handle(authReq).pipe(catchError((err) => {
                if ([401, 403].indexOf(err.status) !== -1) {
                    this.authenticationService.logout();
                }
                const error = err.error.message || err.statusText;
                return throwError(error);
            }));
        } else {
            return next.handle(request);
        }
    }

    jwtToken(token: string): string {
        return 'bearer ' + token;
    }
}
