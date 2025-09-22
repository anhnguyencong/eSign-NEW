import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { TokenModel } from '../models/token.model';
import { UserInfoModel } from '../models/users/userinfo.model';

@Injectable()
export class WorkContext {
    private readonly sessionUserInfoName = 'esuser';
    private readonly sessionWorkContextName = 'workcontext';

    constructor(private router: Router) { }

    saveToken(data: TokenModel) {
        localStorage.removeItem(this.sessionWorkContextName);
        localStorage.setItem(
            this.sessionWorkContextName,
            JSON.stringify({
                accessToken: data.token,
                expiration: data.expiration,
            })
        );
        this.saveUserInfo(data.userInfo);
    }

    saveUserInfo(userInfo: UserInfoModel) {
        localStorage.removeItem(this.sessionUserInfoName);
        localStorage.setItem(
            this.sessionUserInfoName,
            JSON.stringify({ userInfo })
        );
    }

    isAuthenticated(): boolean {
        if (localStorage.getItem(this.sessionWorkContextName)) {
            return true;
        }
        return false;
    }

    getWorkcontext(): any {
        const workcontext = JSON.parse(
            localStorage.getItem(this.sessionWorkContextName)!
        );
        if (workcontext) {
            return workcontext;
        }
        return '';
    }

    getToken(): string {
        const workcontext = JSON.parse(
            localStorage.getItem(this.sessionWorkContextName)!
        );
        if (workcontext) {
            return workcontext.accessToken;
        }
        return '';
    }

    getCurrentUser(): UserInfoModel {
        const user = JSON.parse(
            localStorage.getItem(this.sessionUserInfoName)!
        );
        if (user) {
            return user.userInfo;
        }
        return new UserInfoModel();
    }

    isTokenExpired(needToLogout: boolean): boolean {
        const workcontext = JSON.parse(localStorage.getItem('workcontext')!);
        if (workcontext) {
            const t = Math.round((new Date()).getTime() / 1000);
            const expiredDate = parseInt(workcontext.expiration);
            if (needToLogout) {
                if (t >= expiredDate) {
                    return true;
                }
            } else {
                if (t + 300 >= expiredDate) {
                    return true;
                }
            }
            return false;
        }
        return false;
    }

    removeToken() {
        localStorage.removeItem(this.sessionUserInfoName);
        localStorage.removeItem(this.sessionWorkContextName);
    }
}
