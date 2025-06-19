import { createReducer, on } from "@ngrx/store";
import { UserModel } from "../../models/user.model";
import { addUser } from "./users.actions";

export interface UserState {
    users: UserModel[],
    isLoading: boolean,
    error: string | null;
}

export const initialState : UserState = {
    users: [
        {id: 1, username: 'Lea', email: 'Lea@gmail.com', role: 'Admin'},
        {id: 2, username: 'Shaun', email: 'shaun@gmail.com', role: 'User'},
        {id: 3, username: 'Glassman', email: 'aran@gmail.com', role: 'Guest'},
    ],
    isLoading: false,
    error: null
}

export const userReducer = createReducer(
  initialState,
  on(addUser, (state, { user }) => {
    const maxId = state.users.reduce((acc, curr) => Math.max(acc, curr.id), 0);
    const newUser: UserModel = {
      ...user,
      id: maxId+1,
    };
    return {
      ...state,
      users: [...state.users, newUser],
    };
  })
);
