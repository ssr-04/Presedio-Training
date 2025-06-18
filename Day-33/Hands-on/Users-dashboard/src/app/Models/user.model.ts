export interface UserModel {
  id: number;
  firstName: string;
  lastName: string;
  age: number;
  gender: string;
  username: string;
  image: string;
  email: string;
  phone: string;
  birthDate: string;
  university: string;
  address: {
    address: string;
    city: string;
    state: string;
    country: string;
    postalCode: string;
  };
  company: {
    name: string;
    department: string;
    title: string;
  };
}

export interface ApiUsersResponseModel {
  users: UserModel[];
  total: number;
  skip: number;
  limit: number;
}

// Types for the filter
export interface UserFiltersModel {
  [key: string]: string; 
}