import { UserInfoModel } from './users/userinfo.model';

export class TokenModel {
    token!: string;
    expiration!: string;
    userInfo!: UserInfoModel;
}
