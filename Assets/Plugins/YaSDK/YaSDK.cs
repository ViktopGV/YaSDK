using System;
using UnityEngine;

public static class YaSDK
{
    public static event Action SDKInitialized;
    public static Saves Saves => _saves;
    public static Environment Env => _env;

    private static YaSDKEvents _sdk;
    private static Saves _saves = new();
    private static Environment _env = new();
    private static bool _isSDKReady = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        GameObject sdkEvents = new GameObject("YaSDK");
        _sdk = sdkEvents.AddComponent<YaSDKEvents>();
        UnityEngine.Object.DontDestroyOnLoad(sdkEvents);
        _sdk.YaSDKReady += _sdk_YaSDKReady;

        _sdk.StartCoroutine(_sdk.WaitForSDKReady());
    }

    private static void _sdk_YaSDKReady()
    {
        SDKInitialized?.Invoke();
        _isSDKReady = true;
    }

    public static void SetSaves(Saves newSaves) => _saves = newSaves;

    public static void SendMetrica(string goal) => _sdk.SendMetrica(goal);
    public static void ShowInterstitial(Action onOpen = null, Action<bool> onClose = null, Action<string> onError = null) => _sdk.ShowInterstitial(onOpen, onClose, onError);
    public static void ShowRewarded(Action<string> onRewarded, Action onOpen = null, Action onClose = null, Action<string> onError = null, string rewarded = "") => _sdk.ShowRewarded(rewarded, onRewarded, onOpen, onClose, onError);

    public static void GameReady() => _sdk.GameReady();
    public static void GameplayStart() => _sdk.GameplayStart();
    public static void GameplayStop() => _sdk.GameplayStop();

    public static bool IsPlayerAuthorized() => _sdk.IsPlayerAuthorized();
    public static void SavePlayerData() => _sdk.SavePlayerData();
    public static void GetPlayerData(Action<Saves> onRecive = null) => _sdk.GetPlayerData(onRecive);
    public static string GetPlayerName() => _sdk.GetPlayerName();
    public static string GetPlayerPhoto() => _sdk.GetPlayerPhoto();
    public static void OpenAuthDialog(Action onAuth = null, Action onReject = null) => _sdk.OpenAuthDialog(onAuth, onReject);

    public static void SetScore(string leaderboard, long score) => _sdk.SetScore(leaderboard, score);
    public static void GetPlayerEntry(string leaderboard, Action<LeaderboardEntry> onEntry, Action onNotPresent = null, string avatarSize = "small") => _sdk.GetPlayerEntry(leaderboard, onEntry, onNotPresent, avatarSize);
    public static void GetLeaderboardEntries(string leaderboard, Action<LeaderboardEntries> onEntries, bool include = false, int around = 6, int top = 3, string avatarSize = "small")
        => _sdk.GetLeaderboardEntries(leaderboard, onEntries, include, around, top, avatarSize);

    public static string GetFlag(string flag) => _sdk.GetFlag(flag);

    public static void Buy(string purchaseID, Action<PurchaseData> onSuccses, Action<string> onError = null) => _sdk.Buy(purchaseID, onSuccses, onError);
    public static void ConsumePurchase(string token) => _sdk.ConsumePurchase(token);
    public static void GetPurchased(Action<PurchaseList> onPurchased) => _sdk.GetPurchased(onPurchased);
    public static void GetCatalog(Action<ProductList> onRecive, string iconSize = "small") => _sdk.GetCatalog(onRecive, iconSize);

    public static long ServerTime() => _sdk.ServerTime();

    public static DeviceInfo GetDevice() => _sdk.GetDevice();

    public static bool IsSDKReady() => _isSDKReady;

}




#region PurchaseStructure
[Serializable]
public class PurchaseData
{
    public string productID;
    public string purchaseToken;
    public string developerPayload;
}

[Serializable]
public class Product
{
    public string id;
    public string title;
    public string description;
    public string imageURI;
    public string price;
    public string priceValue;
    public string priceCurrencyCode;
    public string currencyIcon;
}

[Serializable]
public class ProductList
{
    public Product[] products;
}

[Serializable]
public class PurchaseList
{
    public PurchaseData[] purchases;
}
#endregion

#region LeaderboardStructure
[Serializable]
public class LeaderboardInfo
{
    public string appID;
    public bool @default;
    public Description description;
    public string name;
    public Title title;

    [Serializable]
    public class Description
    {
        public bool invert_sort_order;
        public ScoreFormat score_format;

        [Serializable]
        public class ScoreFormat
        {
            public string type;
            public ScoreFormatOptions options;

            [Serializable]
            public class ScoreFormatOptions
            {
                public int decimal_offset;
            }
        }
    }

    [Serializable]
    public class Title
    {
        public string en;
        public string ru;
        public string be;
        public string uk;
        public string kk;
        public string uz;
        public string tr;
    }
}

[Serializable]
public class LeaderboardEntry
{
    public int score;
    public string extraData;
    public int rank;
    public string formattedScore;
    public PlayerInfo player;

    [Serializable]
    public class PlayerInfo
    {
        public string avatarSrc;
        public string lang;
        public string publicName;
        public ScopePermissions scopePermissions;
        public string uniqueID;
    }

    [Serializable]
    public class ScopePermissions
    {
        public string avatar;
        public string public_name;
    }
}

[Serializable]
public class LeaderboardEntries
{
    public LeaderboardInfo leaderboard;
    public LeaderboardRange[] ranges;
    public int userRank;
    public LeaderboardEntry[] entries;
}

[Serializable]
public class LeaderboardRange
{
    public int start;
    public int size;
}
#endregion

#region EnvironmentStructure
[Serializable]
public class App
{
    public string id;
}

[Serializable]
public class I18n
{
    public string lang = "ru";
    public string tld;
}

[Serializable]
public class Environment
{
    public App app;
    public I18n i18n = new();
    public string payload;
}
#endregion

#region DeviceInfo
public enum DeviceInfo
{
    Desktop,
    Mobile,
    Tablet, 
    TV
}
#endregion