export class User {
    constructor(public username:string, public email:string, public role: 'admin' | 'user'){}
}