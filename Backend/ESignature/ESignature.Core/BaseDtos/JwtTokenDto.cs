namespace ESignature.Core.BaseDtos
{
    public class JwtTokenDto
    {
        public string Token { get; set; }
        public long Expiration { get; set; }
    }
}