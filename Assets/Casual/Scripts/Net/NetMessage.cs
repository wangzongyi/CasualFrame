public class NetError
{
    public int code;
    public string message;
}

public class NetMessage<T>
{
    public bool succ;
    public NetError errorCode;

    public T value;
}

public enum CaptchaType
{
    Login = 0,
    ResetPassword = 1,
    Register = 2,
}

public class Net_GetCaptcha
{
    /// <summary>
    /// {"phone":string, "codeType":int}
    /// </summary>
    public class Request
    {
        public string phone;
        public CaptchaType codeType;
    }
}

public class UserInfo
{
    public bool isBase;// 是否创建了基础信息
    public int gender;// 1:boy 2:girl
    public string nick;
    public string birthday;// 格式：20190519
}

public class Net_Login
{
    public class RequestRegister
    {
        public string phone;
        public string password;
        public string captcha;
        public string channel;
    }

    /// <summary>
    /// {"phone":string, "password":string}
    /// </summary>
    public class RequestByPassword
    {
        public string phone;
        public string password;
    }

    /// <summary>
    /// {"phone":string, "captcha"}
    /// </summary>
    public class RequestByCaptcha
    {
        public string phone;
        public string captcha;
    }

    /// <summary>
    /// {"phone":string, "token":string}
    /// </summary>
    public class Response
    {
        public string phone;
        public string token;
        public UserInfo appUserInfo;
    }
}

/// <summary>
/// 心跳
/// </summary>
public class Net_HeartBeat
{
    public class Response
    {
        public long timeMillis;
    }
}

public enum Gender
{
    NONE = 0,
    Boy = 1,
    Girl = 2,
}

public class Net_CreateBase
{
    public class Request
    {
        public string birthday;
        public Gender gender;
    }

    //response = UserInfo
}


public class Net_GetLevel
{
    public string levelCode;
}