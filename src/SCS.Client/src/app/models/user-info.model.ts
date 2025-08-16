interface UserInfo {
    empNo: string;
    name: string;
    email?: string;
    roles?: string[];
    exp?: number; // Expiry date-time as Unix timestamp
}