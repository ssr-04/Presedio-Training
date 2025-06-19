import { createFeatureSelector, createSelector } from "@ngrx/store";
import { UserState } from "./users.reducers";


export const selectUsersFeature = createFeatureSelector<UserState>('users');

export const selectAllUsers = createSelector(
    selectUsersFeature,
    (state: UserState) => state.users
);