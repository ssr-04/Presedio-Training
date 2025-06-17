import { BehaviorSubject, Observable } from "rxjs";
import { UserLoginModel } from "../models/UserLoginModel";
import { inject } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";

export class UserService
{
    private http = inject(HttpClient);
    private usernameSubject = new BehaviorSubject<string|null>(null);
    username$:Observable<string|null> = this.usernameSubject.asObservable();

    validateUserLogin(user:UserLoginModel)
    {
        if(user.username.length<3)
        {
            this.usernameSubject.next(null);
            this.usernameSubject.error("Too short for username");
        }
            
        else
            {
            this.callLoginAPI(user).subscribe(
                {
                    next:(data:any)=>{
                        this.usernameSubject.next(user.username);
                        localStorage.setItem("token",data.accessToken);
                    },
                    error:(err:any)=>{
                        console.log("Something went wrong", err.error.message);
                        this.usernameSubject.next(null);
                    }
                }
            )
            
        }
    }

    callGetProfile()
    {
        var token = localStorage.getItem("token")
        const httpHeader = new HttpHeaders({
            'Authorization':`Bearer ${token}`
        })
        return this.http.get('https://dummyjson.com/auth/me',{headers:httpHeader});
        
    }

    logout(){
        this.usernameSubject.next(null);
    }

    callLoginAPI(user:UserLoginModel)
    {
        return this.http.post("https://dummyjson.com/auth/login",user);
    }
}