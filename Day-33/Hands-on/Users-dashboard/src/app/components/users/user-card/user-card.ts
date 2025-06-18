import { Component, Input, input } from '@angular/core';
import { UserModel } from '../../../Models/user.model';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-user-card',
  imports: [CommonModule, RouterLink],
  templateUrl: './user-card.html',
  styleUrl: './user-card.css'
})

export class UserCard {
 @Input() user!:UserModel;
}
