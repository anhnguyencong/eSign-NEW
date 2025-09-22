export class UserInfoModel {
    id!: string;
    firstName!: string;
    lastName!: string;
    fullName!: string;
    isSuperAdmin: boolean = false;
    isAdmin: boolean = false;
    isEditor: boolean = false;
    isUser: boolean = false;
    isSocialUser: boolean = false;
}
