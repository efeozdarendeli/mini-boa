using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MiniBoa.Communication.Security;

/// <summary>
/// Başarılı girişten sonra JWT üretir. Token'ın içinde sadece müşteri numarası vardır;
/// imzalı olduğu için istemci içeriğini değiştiremez (değiştirirse imza tutmaz ve reddedilir).
/// </summary>
public sealed class TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
{
    public const string CustomerNoClaim = JwtRegisteredClaimNames.Sub;

    public string CreateToken(long customerNo)
    {
        var settings = options.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Subject = new ClaimsIdentity([new Claim(CustomerNoClaim, customerNo.ToString())]),
            NotBefore = now,
            Expires = now.AddMinutes(settings.ExpiresMinutes),
            SigningCredentials = new SigningCredentials(CreateSigningKey(settings), SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public static SymmetricSecurityKey CreateSigningKey(JwtOptions settings) =>
        new(Encoding.UTF8.GetBytes(settings.SigningKey));
}
