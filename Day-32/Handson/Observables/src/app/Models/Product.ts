
export interface Product {
    id: number;
    title: string,
    description: string,
    price: number,
    thumbnail: string
}

export interface ApiResponse {
    products: Product[],
    total: number,
    skip: number,
    limit: number
}

export interface ProductDetailModel extends Product {
    category: string,
    rating: number,
    stock: number,
    brand: string,
    images: string[],
    reviews: Review[]
}

export interface Review {
    rating: number,
    comment: string,
    date: string,
    reviewerName: string,
    reviewerEmail: string
}

export interface AuthResponse {
    id: number,
    username: string,
    email: string,
    firstName: string,
    lastName: string,
    gender: string,
    image: string,
    accessToken: string
}

export type User = Omit<AuthResponse, 'accessToken'>; // '/auth/me' returns same except the toekn