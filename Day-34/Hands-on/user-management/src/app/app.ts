import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UserManagement } from "./user-management/user-management";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, UserManagement],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected title = 'user-management';
}
