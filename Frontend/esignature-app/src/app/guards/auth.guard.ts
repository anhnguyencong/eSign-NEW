import { Injectable } from "@angular/core";
import { Router, CanActivate, ActivatedRouteSnapshot,RouterStateSnapshot } from "@angular/router";
import {WorkContext} from '../_context/workcontext'

@Injectable()
export class AuthGuard implements CanActivate{
    constructor(private workContext: WorkContext, private router:Router){

    }

    canActivate(activatedRoute: ActivatedRouteSnapshot, routerState: RouterStateSnapshot){
        if(this.workContext.isAuthenticated()){
            return true;
        }
        else{
            this.router.navigate([''])
            return false;
        }
    }
}