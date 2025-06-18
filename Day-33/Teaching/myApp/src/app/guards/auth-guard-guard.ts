import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, CanActivateFn, GuardResult, MaybeAsync, Router, RouterStateSnapshot } from '@angular/router';

@Injectable()
export class AuthGuard implements CanActivate{

  constructor(private router:Router){}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): MaybeAsync<GuardResult> {
    const isAuthenticated = localStorage.getItem("token")?true:false;
    if(!isAuthenticated)
    {
      this.router.navigate(["login"]);
      return false
    }

    return true;
  }
  
}
