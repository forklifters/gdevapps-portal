using System.Collections.Generic;

internal sealed class TokenResponse
{
    public TokenResponse(bool isUpdated, IEnumerable<GdevApps.BLL.Models.AspNetUsers.AspNetUserToken> tokens)
    {
        this.IsUpdated = isUpdated;
        this.Tokens = tokens;
    }
    public bool IsUpdated { get; set; }

    public IEnumerable<GdevApps.BLL.Models.AspNetUsers.AspNetUserToken> Tokens { get; set; }
}