export interface LoginResponse {
    token: string;
}

export interface ErrorResponse {
    message: string;
    statusCode: number;
}