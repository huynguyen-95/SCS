export interface UserInfo {
    empNo: string;
    name: string;
    isAdmin: boolean;
    email?: string;
    role?: string;
    roles?: string[];
    exp?: number; // Expiry date-time as Unix timestamp
}