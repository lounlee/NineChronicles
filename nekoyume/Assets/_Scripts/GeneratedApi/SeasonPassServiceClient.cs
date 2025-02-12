//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Do not modify the contents of this file directly.
//     Changes might be overwritten the next time the code is generated.
//     Source URL: https://9s6qapxbef.execute-api.us-east-2.amazonaws.com/internal/openapi.json
// </auto-generated>
//------------------------------------------------------------------------------
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;

public class SeasonPassServiceClient
{
    private string Url;
    private readonly HttpClient _client;

    public SeasonPassServiceClient(string url)
    {
        Url = url;
        _client = new System.Net.Http.HttpClient();
        _client.Timeout = TimeSpan.FromSeconds(10);
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [JsonConverter(typeof(ActionTypeTypeConverter))]
    public enum ActionType
    {
        hack_and_slash,
        hack_and_slash_sweep,
        battle_arena,
        raid,
    }

    public class ActionTypeTypeConverter : JsonConverter<ActionType>
    {
        public override ActionType Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Number => (ActionType)reader.GetInt32(),
                JsonTokenType.String => Enum.Parse<ActionType>(reader.GetString()),
                _ => throw new JsonException(
                    $"Expected token type to be {string.Join(" or ", new[] { JsonTokenType.Number, JsonTokenType.String })} but got {reader.TokenType}")
            };
        }
        public override void Write(
            Utf8JsonWriter writer,
            ActionType value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class ClaimRequestSchema
    {
        [JsonPropertyName("planet_id")]
        public PlanetID? PlanetId { get; set; }
        [JsonPropertyName("agent_addr")]
        public string AgentAddr { get; set; }
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
        [JsonPropertyName("season_id")]
        public int SeasonId { get; set; }
    }

    public class ClaimResultSchema
    {
        [JsonPropertyName("reward_list")]
        public List<ClaimSchema> RewardList { get; set; }
        [JsonPropertyName("user")]
        public UserSeasonPassSchema User { get; set; }
        [JsonPropertyName("items")]
        public List<ItemInfoSchema> Items { get; set; }
        [JsonPropertyName("currencies")]
        public List<CurrencyInfoSchema> Currencies { get; set; }
    }

    public class ClaimSchema
    {
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; }
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
        [JsonPropertyName("decimal_places")]
        public int DecimalPlaces { get; set; }
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class CurrencyInfoSchema
    {
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }

    public class ExpInfoSchema
    {
        [JsonPropertyName("action_type")]
        public ActionType ActionType { get; set; }
        [JsonPropertyName("exp")]
        public int Exp { get; set; }
    }

    public class HTTPValidationError
    {
        [JsonPropertyName("detail")]
        public List<ValidationError> Detail { get; set; }
    }

    public class ItemInfoSchema
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }

    public class LevelInfoSchema
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("exp")]
        public int Exp { get; set; }
    }

    public class LevelRequestSchema
    {
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
        [JsonPropertyName("level")]
        public int? Level { get; set; }
        [JsonPropertyName("exp")]
        public int? Exp { get; set; }
    }

    public class NewRewardSchema
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("normal")]
        public List<ClaimSchema> Normal { get; set; }
        [JsonPropertyName("premium")]
        public List<ClaimSchema> Premium { get; set; }
    }

    public class NewSeasonPassSchema
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }
        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }
        [JsonPropertyName("start_timestamp")]
        public string StartTimestamp { get; set; }
        [JsonPropertyName("end_timestamp")]
        public string EndTimestamp { get; set; }
        [JsonPropertyName("reward_list")]
        public List<NewRewardSchema> RewardList { get; set; }
    }

    [JsonConverter(typeof(PlanetIDTypeConverter))]
    public enum PlanetID
    {
        _0x000000000000,
        _0x000000000001,
        _0x000000000002,
        _0x100000000000,
        _0x100000000001,
        _0x100000000002,
    }

    public class PlanetIDTypeConverter : JsonConverter<PlanetID>
    {
        public override PlanetID Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Number => (PlanetID)reader.GetInt32(),
                JsonTokenType.String => Enum.Parse<PlanetID>("_"+reader.GetString()),
                _ => throw new JsonException(
                    $"Expected token type to be {string.Join(" or ", new[] { JsonTokenType.Number, JsonTokenType.String })} but got {reader.TokenType}")
            };
        }
        public override void Write(
            Utf8JsonWriter writer,
            PlanetID value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().Substring(1));
        }
    }

    public class PremiumRequestSchema
    {
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
        [JsonPropertyName("is_premium")]
        public bool IsPremium { get; set; }
        [JsonPropertyName("is_premium_plus")]
        public bool IsPremiumPlus { get; set; }
    }

    public class RegisterRequestSchema
    {
        [JsonPropertyName("planet_id")]
        public PlanetID? PlanetId { get; set; }
        [JsonPropertyName("agent_addr")]
        public string AgentAddr { get; set; }
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
    }

    public class RewardDetailSchema
    {
        [JsonPropertyName("item")]
        public List<ItemInfoSchema> Item { get; set; }
        [JsonPropertyName("currency")]
        public List<CurrencyInfoSchema> Currency { get; set; }
    }

    public class RewardSchema
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("normal")]
        public RewardDetailSchema Normal { get; set; }
        [JsonPropertyName("premium")]
        public RewardDetailSchema Premium { get; set; }
    }

    public class SeasonChangeRequestSchema
    {
        [JsonPropertyName("season_id")]
        public int SeasonId { get; set; }
        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }
    }

    public class SeasonPassSchema
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }
        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }
        [JsonPropertyName("start_timestamp")]
        public string StartTimestamp { get; set; }
        [JsonPropertyName("end_timestamp")]
        public string EndTimestamp { get; set; }
        [JsonPropertyName("reward_list")]
        public List<RewardSchema> RewardList { get; set; }
    }

    public class UpgradeRequestSchema
    {
        [JsonPropertyName("planet_id")]
        public PlanetID? PlanetId { get; set; }
        [JsonPropertyName("agent_addr")]
        public string AgentAddr { get; set; }
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
        [JsonPropertyName("season_id")]
        public int SeasonId { get; set; }
        [JsonPropertyName("is_premium")]
        public bool IsPremium { get; set; }
        [JsonPropertyName("is_premium_plus")]
        public bool IsPremiumPlus { get; set; }
        [JsonPropertyName("g_sku")]
        public string GSku { get; set; }
        [JsonPropertyName("a_sku")]
        public string ASku { get; set; }
        [JsonPropertyName("reward_list")]
        public List<ClaimSchema> RewardList { get; set; }
    }

    public class UserSeasonPassSchema
    {
        [JsonPropertyName("planet_id")]
        public PlanetID PlanetId { get; set; }
        [JsonPropertyName("agent_addr")]
        public string AgentAddr { get; set; }
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
        [JsonPropertyName("season_pass_id")]
        public int SeasonPassId { get; set; }
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("exp")]
        public int Exp { get; set; }
        [JsonPropertyName("is_premium")]
        public bool IsPremium { get; set; }
        [JsonPropertyName("is_premium_plus")]
        public bool IsPremiumPlus { get; set; }
        [JsonPropertyName("last_normal_claim")]
        public int LastNormalClaim { get; set; }
        [JsonPropertyName("last_premium_claim")]
        public int LastPremiumClaim { get; set; }
    }

    public class ValidationError
    {
        [JsonPropertyName("loc")]
        public List<string?> Loc { get; set; }
        [JsonPropertyName("msg")]
        public string Msg { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public async Task GetSeasonpassCurrentAsync(Action<SeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/season-pass/current";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), url))
        {
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                SeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<SeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetSeasonpassCurrentNewAsync(Action<NewSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/season-pass/current/new";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), url))
        {
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                NewSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<NewSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetSeasonpassLevelAsync(Action<LevelInfoSchema[]> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/season-pass/level";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), url))
        {
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                LevelInfoSchema[] result = System.Text.Json.JsonSerializer.Deserialize<LevelInfoSchema[]>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetSeasonpassExpAsync(Action<ExpInfoSchema[]> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/season-pass/exp";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), url))
        {
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                ExpInfoSchema[] result = System.Text.Json.JsonSerializer.Deserialize<ExpInfoSchema[]>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetUserStatusAsync(int season_id, string avatar_addr, string planet_id, Action<UserSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/user/status";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), url))
        {
            url += $"?season_id={season_id}&avatar_addr={avatar_addr}&planet_id={planet_id}";
            request.RequestUri = new Uri(url);
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                UserSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostUserUpgradeAsync(string authorization, UpgradeRequestSchema requestBody, Action<UserSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/user/upgrade";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), url))
        {
            request.RequestUri = new Uri(url);
            request.Headers.Add("authorization", authorization.ToString());
            request.Content = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                UserSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostUserClaimAsync(ClaimRequestSchema requestBody, Action<ClaimResultSchema> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/user/claim";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), url))
        {
            request.Content = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                ClaimResultSchema result = System.Text.Json.JsonSerializer.Deserialize<ClaimResultSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostTmpRegisterAsync(RegisterRequestSchema requestBody, Action<UserSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/tmp/register";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), url))
        {
            request.Content = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                UserSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostTmpPremiumAsync(PremiumRequestSchema requestBody, Action<UserSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/tmp/premium";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), url))
        {
            request.Content = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                UserSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostTmpLevelAsync(LevelRequestSchema requestBody, Action<UserSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/tmp/level";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), url))
        {
            request.Content = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                UserSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostTmpChangeseasonAsync(SeasonChangeRequestSchema requestBody, Action<SeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/tmp/change-season";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), url))
        {
            request.Content = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                SeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<SeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetBlockstatusAsync(Action<string> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/block-status";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), url))
        {
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                onSuccess?.Invoke(responseBody);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetInvalidclaimAsync(Action<string> onSuccess, Action<string> onError)
    {
        string url = Url + "/api/invalid-claim";
        using (var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), url))
        {
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                onSuccess?.Invoke(responseBody);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

}
