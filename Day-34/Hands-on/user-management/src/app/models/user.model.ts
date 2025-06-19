export interface UserModel
{
    id: number;
    username: string;
    email: string;
    password?: string; // Optional as not to be stored or send in response
    role: 'Admin' | 'User' |'Guest';
}