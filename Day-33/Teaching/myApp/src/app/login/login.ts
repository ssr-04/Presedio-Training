import { Component } from '@angular/core';
import { UserLoginModel } from '../models/UserLoginModel';
import { UserService } from '../services/UserService';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { PasswordValidator } from '../validators/passwordValidator';

@Component({
  selector: 'app-login',
  imports: [FormsModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  
  loginForm: FormGroup;

  constructor(private userService: UserService, private route: Router) {
    this.loginForm = new FormGroup({
      un:new FormControl(null, Validators.required),
      pass:new FormControl(null, [Validators.required, PasswordValidator()])
    })
  }

  public get un() : any {
    return this.loginForm.get("un")
  }
  public get pass() : any {
    return this.loginForm.get("pass")
  }

  handleLogin() {
    
    if (this.loginForm.invalid)
    {
      alert("Invalid details");
      return
    }
    var user = new UserLoginModel(this.un.value, this.pass.value);
    this.userService.validateUserLogin(user);
    this.route.navigateByUrl("/home/" + user.username);
  }
}
