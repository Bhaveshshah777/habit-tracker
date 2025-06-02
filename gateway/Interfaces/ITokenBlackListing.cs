namespace Gateway.Interfaces;

public interface ITokenBlackListing
{
    public Task BlacklistTokenAsync(string token);
    public Task<bool> IsTokenBlacklistedAsync(string token);
}
