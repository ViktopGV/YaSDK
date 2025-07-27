using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class YaSDKEvents : MonoBehaviour
{
    public event Action YaSDKReady;

    public IEnumerator WaitForSDKReady()
    {
        yield return new WaitUntil(() => Yandex_IsSdkReady() == true);
        InitializeEnvironment();
#if UNITY_WEBGL && !UNITY_EDITOR
        JsonUtility.FromJsonOverwrite(Yandex_GetPlayerDataSync(), YaSDK.Saves);
#else
        GetPlayerData();
#endif
        YaSDKReady?.Invoke();
        SavePlayerData();
    }


    #region GameplayAPI
    public void GameReady()
    {

#if UNITY_WEBGL && !UNITY_EDITOR
        Yandex_GameReady();
#else
        Debug.Log("GameReady");
#endif
    }
    public void GameplayStart()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Yandex_GameplayStart();
#else
        Debug.Log("GamePlay Start");
#endif
    }
    public void GameplayStop()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Yandex_GameplayStop();
#else
        Debug.Log("GamePlay stop");
#endif
    }

    [DllImport("__Internal")]
    private static extern void Yandex_GameReady();
    [DllImport("__Internal")]
    private static extern void Yandex_GameplayStart();
    [DllImport("__Internal")]
    private static extern void Yandex_GameplayStop();
#endregion

    #region Player
    private Action<Saves> onGetPlayerData;
    private Action onPlayerAuth;
    private Action onPlayerAuthReject;

    public bool IsPlayerAuthorized() => Yandex_IsPlayerAuthorized();
    public void SavePlayerData()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Yandex_SetPlayerData(JsonUtility.ToJson(YaSDK.Saves));
#else
        PlayerPrefs.SetString("data", JsonUtility.ToJson(YaSDK.Saves));
        PlayerPrefs.Save();
#endif

    }
    public void GetPlayerData(Action<Saves> onRecive = null) 
    { 
        onGetPlayerData = onRecive;
#if UNITY_WEBGL && !UNITY_EDITOR
        Yandex_GetPlayerData(); 
#else
        string data = PlayerPrefs.GetString("data");
        OnPlayerGetData(data);
#endif
    }
    public string GetPlayerName() => Yandex_GetPlayerName();
    public string GetPlayerPhoto(string size = "small") => Yandex_GetPlayerPhoto(size);
    public void OpenAuthDialog(Action onAuth=null, Action onReject = null)
    {
        onPlayerAuth = onAuth;
        onPlayerAuthReject = onReject;
        Yandex_OpenAuthDialog();
    }

    private void OnPlayerGetData(string json)
    {
        //JsonUtility.FromJsonOverwrite(json, YaSDK.Saves);
        Saves data = JsonUtility.FromJson<Saves>(json);
        onGetPlayerData?.Invoke(data);
        onGetPlayerData = null;
    }

    private void OnPlayerAuthorized()
    {
        onPlayerAuth?.Invoke();

        onPlayerAuth = null;
        onPlayerAuthReject = null;
    }

    private void OnPlayerAuthorizedReject()
    {
        onPlayerAuthReject?.Invoke();

        onPlayerAuth = null;
        onPlayerAuthReject = null;
    }


    [DllImport("__Internal")]
    public static extern bool Yandex_IsPlayerAuthorized();
    [DllImport("__Internal")]
    private static extern void Yandex_SetPlayerData(string jsonObj);
    [DllImport("__Internal")]
    private static extern void Yandex_GetPlayerData();
    [DllImport("__Internal")]
    private static extern string Yandex_GetPlayerDataSync();
    [DllImport("__Internal")]
    private static extern string Yandex_GetPlayerName();
    [DllImport("__Internal")]
    private static extern string Yandex_GetPlayerPhoto(string size);
    [DllImport("__Internal")]
    private static extern void Yandex_OpenAuthDialog();
    #endregion

    #region Flags
    public string GetFlag(string flag)
    {

#if UNITY_WEBGL && !UNITY_EDITOR
        return Yandex_GetFlag(flag);
#else
        return "90";
#endif
    }
    [DllImport("__Internal")]
    private static extern string Yandex_GetFlag(string flag);
#endregion

    #region Ads
    private Action onFSOpen = null;
    private Action<bool> onFSClose = null;
    private Action<string> onFSError = null;

    private Action onRWOpen = null;
    private Action onRWClose = null;
    private Action<string> onRWError = null;
    private Action<string> onRWReward = null;

    public void ShowInterstitial(Action onOpen = null, Action<bool> onClose = null, Action<string> onError = null)
    {
        onFSOpen = onOpen;
        onFSClose = onClose;
        onFSError = onError;
        Debug.Log("Показываем рекламу");

#if !UNITY_EDITOR && UNITY_WEBGL
            Yandex_ShowInterstitial();
#else
        Debug.Log("Показали рекламу");
#endif
    }
    public void ShowRewarded(string rewarded, Action<string> onRewarded, Action onOpen = null, Action onClose = null, Action<string> onError = null)
    {
        onRWOpen = onOpen;
        onRWClose = onClose;
        onRWError = onError;
        onRWReward = onRewarded;

#if !UNITY_EDITOR && UNITY_WEBGL
            Yandex_ShowRewardAd(rewarded);
#else
        Debug.Log("Реклама за вознаграждение!");
        Rewarded(rewarded);
#endif
    }

    private void FullscreenOpen() { onFSOpen?.Invoke(); onFSOpen = null; }
    private void FullscreenClose(string wasShown) { onFSClose?.Invoke(bool.Parse(wasShown)); onFSClose = null; }
    private void FullscreenError(string error) { onFSError?.Invoke(error); onFSError = null; }
    private void RewardError(string error) { onRWError?.Invoke(error); onRWError = null; }
    private void RewardOpen() { onRWOpen?.Invoke(); onRWOpen = null; }
    private void Rewarded(string reward) { onRWReward?.Invoke(reward); onRWReward = null; }
    private void OnRewardClose() { onRWClose?.Invoke(); onRWClose = null; }

    [DllImport("__Internal")]
    private static extern void Yandex_ShowInterstitial();
    [DllImport("__Internal")]
    private static extern void Yandex_ShowRewardAd(string reward);
    #endregion

    #region InAppPurchase
    private Action<PurchaseData> onPurchaseSuccsess;
    private Action<string> onPurchaseReject;
    private Action<PurchaseList> onPurchasesRecive;
    private Action<ProductList> onCatalogRecive;

    public void Buy(string purchaseID, Action<PurchaseData> onSuccses, Action<string> onError) { onPurchaseSuccsess = onSuccses; onPurchaseReject = onError; Yandex_Purchase(purchaseID); }
    public void ConsumePurchase(string token) => Yandex_ConsumePurchase(token);
    public void GetPurchased(Action<PurchaseList> onPurchased) { onPurchasesRecive = onPurchased; Yandex_GetPurchased(); }
    public void GetCatalog(Action<ProductList> onRecive, string iconSize = "small") { onCatalogRecive = onRecive; Yandex_GetCatalog(iconSize); }

    private void OnPurchaseSuccess(string json) { onPurchaseSuccsess?.Invoke(JsonUtility.FromJson<PurchaseData>(json)); onPurchaseSuccsess = null; }
    private void OnPurchaseReject(string error) { onPurchaseReject?.Invoke(error); onPurchaseReject = null; }
    private void OnPurchases(string json)
    {
        onPurchasesRecive?.Invoke(JsonUtility.FromJson<PurchaseList>(json));
        onPurchasesRecive = null;
    }
    public void OnCatalogReceived(string json) { onCatalogRecive?.Invoke(JsonUtility.FromJson<ProductList>(json)); onCatalogRecive = null; }



    [DllImport("__Internal")]
    private static extern void Yandex_Purchase(string id);
    [DllImport("__Internal")]
    private static extern void Yandex_GetPurchased();
    [DllImport("__Internal")]
    private static extern void Yandex_GetCatalog(string iconSize);
    [DllImport("__Internal")]
    private static extern void Yandex_ConsumePurchase(string purchaseToken);
    #endregion

    #region Leaderboard
    private Action<LeaderboardEntries> onEntries;
    private Action<LeaderboardEntry> onPlayerEntry;
    private Action onPlayerNotPresent;

    public void SetScore(string leaderboard, long score)
    {
        if (IsPlayerAuthorized())
            Yandex_SetScore(leaderboard, score);
    }

    public void GetPlayerEntry(string leaderboard, Action<LeaderboardEntry> onEntry, Action onNotPresent = null, string avatarSize = "small")
    {
        onPlayerEntry = onEntry;
        onPlayerNotPresent = onNotPresent;
#if UNITY_WEBGL && !UNITY_EDITOR
            Yandex_GetPlayerEntry(leaderboard, avatarSize);
#else
        Debug.Log($"[YaSDK] GetScore({leaderboard}) [Editor stub]");
        onPlayerEntry?.Invoke(new LeaderboardEntry()
        {
            extraData = "",
            formattedScore = "42",
            player = new LeaderboardEntry.PlayerInfo()
            {
                avatarSrc = "",
                lang = "ru",
                publicName = "You!",
                scopePermissions = new LeaderboardEntry.ScopePermissions()
                {
                    avatar = "",
                    public_name = ""
                },
                uniqueID = "SvinkaPeppa"
            },
        });
#endif
    }
    public void GetLeaderboardEntries(string leaderboard, Action<LeaderboardEntries> onEntries, bool include = false, int around = 6, int top = 3, string avatarSize = "small")
    {
        this.onEntries = onEntries;

#if UNITY_WEBGL && !UNITY_EDITOR
            Yandex_GetEntries(leaderboard, include, around, top, avatarSize);
#else
        Debug.Log($"[YaSDK] GetEntries({leaderboard}, {include}, {around}, {top}) [Editor stub]");
        onEntries?.Invoke(new LeaderboardEntries()
        {
            entries = new LeaderboardEntry[]
            {
                    new LeaderboardEntry()
                    {
                        extraData = "",
                        formattedScore = "15",
                        player = new LeaderboardEntry.PlayerInfo()
                        {
                            avatarSrc = "",
                            lang = "ru",
                            publicName = "Tima",
                            scopePermissions = new LeaderboardEntry.ScopePermissions()
                            {
                                avatar = "",
                                public_name = ""
                            },
                            uniqueID = "SvinkaPeppa"
                        },
                    },
                    new LeaderboardEntry()
                    {
                        extraData = "",
                        formattedScore = "45",
                        player = new LeaderboardEntry.PlayerInfo()
                        {
                            avatarSrc = "",
                            lang = "ru",
                            publicName = "Oleg",
                            scopePermissions = new LeaderboardEntry.ScopePermissions()
                            {
                                avatar = "",
                                public_name = ""
                            },
                            uniqueID = "SvinkaPeppa"
                        },
                    },
                    new LeaderboardEntry()
                    {
                        extraData = "",
                        formattedScore = "156",
                        player = new LeaderboardEntry.PlayerInfo()
                        {
                            avatarSrc = "",
                            lang = "ru",
                            publicName = "Vitek",
                            scopePermissions = new LeaderboardEntry.ScopePermissions()
                            {
                                avatar = "",
                                public_name = ""
                            },
                            uniqueID = "SvinkaPeppa"
                        },
                    }
            }
        });
#endif
    }
    
    
    public void OnEntriesRecive(string json) { onEntries?.Invoke(JsonUtility.FromJson<LeaderboardEntries>(json)); onEntries = null; }
    public void OnPlayerEntryRecive(string json) { onPlayerEntry?.Invoke(JsonUtility.FromJson<LeaderboardEntry>(json)); onPlayerEntry = null; }
    public void OnPlayerNotPresent() { onPlayerNotPresent?.Invoke(); onPlayerNotPresent = null; }



    [DllImport("__Internal")]
    private static extern void Yandex_SetScore(string leaderboardName, long score);
    [DllImport("__Internal")]
    private static extern void Yandex_GetPlayerEntry(string leaderboardName, string avatarSize);
    [DllImport("__Internal")]
    private static extern void Yandex_GetEntries(string leaderboardName, bool include, int around, int top, string avatarSize);
    #endregion

    #region Environment
    public void InitializeEnvironment()
    {

#if UNITY_WEBGL && !UNITY_EDITOR
        JsonUtility.FromJsonOverwrite(Yandex_GetEnvironment(), YaSDK.Env);
#else

#endif

    }

    [DllImport("__Internal")]
    private static extern string Yandex_GetEnvironment();
#endregion

    #region ServerTime
    public long ServerTime()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
            return (long)Yandex_GetServerTime();
#else
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
#endif
    }

    [DllImport("__Internal")]
    private static extern double Yandex_GetServerTime();
    #endregion

    #region DeviceInfo
    public DeviceInfo GetDevice()
    {
        return Yandex_GetDeviceInfo() switch
        {
            "desktop" => DeviceInfo.Desktop,
            "mobile" => DeviceInfo.Mobile,
            "tablet" => DeviceInfo.Tablet,
            "tv" => DeviceInfo.TV,
            _ => DeviceInfo.Desktop
        };
    }

    [DllImport("__Internal")]
    private static extern string Yandex_GetDeviceInfo();
    #endregion

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool Yandex_IsSdkReady();
#else
    private static bool Yandex_IsSdkReady() => true;
#endif

    public void SendMetrica(string goal) => Yanedx_SendMetrica(goal);

    [DllImport("__Internal")]
    private static extern void Yanedx_SendMetrica(string goal);
}
