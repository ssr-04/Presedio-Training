import { createAction, props } from "@ngrx/store";
import { UserModel } from "../../models/user.model";


export const addUser = createAction(
    '[User Management] Add User',
    props<{user: Omit<UserModel, 'id'>}>()
)