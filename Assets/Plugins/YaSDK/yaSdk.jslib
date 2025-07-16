mergeInto(LibraryManager.library, {
  Yandex_IsSdkReady: function () {
    if (
      typeof ysdk !== "undefined" &&
      typeof player !== "undefined" &&
      typeof payments !== "undefined" &&
      typeof flags !== "undefined"
    )
      return true;
    return false;
  },

  Yandex_GameReady: function () {
    if (typeof ysdk === "undefined") {
      console.log("[YaSDK] ysdk not initialized");
      return;
    }
    ysdk.features.LoadingAPI.ready();
  },

  Yandex_GameplayStart: function () {
    if (typeof ysdk === "undefined") {
      console.log("[YaSDK] ysdk not initialized");
      return;
    }
    ysdk.features.GameplayAPI.start();
  },

  Yandex_GameplayStop: function () {
    if (typeof ysdk === "undefined") {
      console.log("[YaSDK] ysdk not initialized");
      return;
    }
    ysdk.features.GameplayAPI.stop();
  },

  Yandex_IsPlayerAuthorized: function () {
    if (typeof player === "undefined") {
      console.log("[YaSDK] player not initialized");
      return;
    }

    return player.isAuthorized();
  },

  //authWindow

  //no callbacks
  Yandex_SetPlayerData: function (jsonPtr) {
    if (typeof player === "undefined") {
      console.log("[YaSDK] player not initialized");
      return;
    }

    let json = UTF8ToString(jsonPtr);
    player.setData(JSON.parse(json));
  },

  Yandex_GetPlayerData: function () {
    if (typeof player === "undefined") {
      console.log("[YaSDK] player not initialized");
      return;
    }

    player
      .getData()
      .then((data) => {
        SendMessage("YaSDK", "OnPlayerGetData", JSON.stringify(data));
      })
      .catch((err) => console.error(err));
  },

  Yandex_GetPlayerName: function () {
    if (typeof player === "undefined") {
      console.log("[YaSDK] player not initialized");
      return;
    }
    const str = player.getName();
    const length = lengthBytesUTF8(str) + 1;
    const buffer = _malloc(length);
    stringToUTF8(str, buffer, length);

    return buffer;
  },

  Yandex_GetPlayerPhoto: function (sizePtr) {
    if (typeof player === "undefined") {
      console.log("[YaSDK] player not initialized");
      return;
    }

    let size = UTF8ToString(sizePtr);
    const str = player.getPhoto(size);
    const length = lengthBytesUTF8(str) + 1;
    const buffer = _malloc(length);
    stringToUTF8(str, buffer, length);

    return buffer;
  },

  Yandex_GetFlag: function (flagPtr) {
    if (typeof flags === "undefined") {
      console.log("[YaSDK] flags not initialized");
      return;
    }

    let flag = UTF8ToString(flagPtr);
    var str = String(flags[flag]);
    var bufferSize = lengthBytesUTF8(str) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(str, buffer, bufferSize);
    return buffer;
  },

  Yandex_ShowInterstitial: function () {
    if (typeof ysdk === "undefined") {
      console.warn("[YaSDK] ysdk not initialized");
      return;
    }

    ysdk.adv.showFullscreenAdv({
      callbacks: {
        onOpen: function () {
          SendMessage("YaSDK", "FullscreenOpen");
        },
        onClose: function (wasShown) {
          SendMessage("YaSDK", "FullscreenClose", String(wasShown));
        },
        onError: function (error) {
          SendMessage("YaSDK", "FullscreenError", JSON.stringify(error));
        },
      },
    });
  },

  Yandex_ShowRewardAd: function (rewarded) {
    if (typeof ysdk === "undefined") {
      console.warn("[YaSDK] ysdk not initialized");
      return;
    }

    rewarded = UTF8ToString(rewarded);
    ysdk.adv.showRewardedVideo({
      callbacks: {
        onOpen: () => {
          SendMessage("YaSDK", "RewardOpen");
        },
        onRewarded: () => {
          SendMessage("YaSDK", "Rewarded", JSON.stringify(rewarded));
        },
        onClose: () => {
          SendMessage("YaSDK", "OnRewardClose");
        },
        onError: (error) => {
          SendMessage("YaSDK", "RewardError", JSON.stringify(error));
        },
      },
    });
  },

  Yandex_Purchase: function (purchaseIdPtr) {
    if (typeof payments === "undefined") {
      console.log("[YaSDK] payments not initialized");
      return;
    }

    let purchaseId = UTF8ToString(purchaseIdPtr);

    payments
      .purchase({ id: purchaseId })
      .then((purchase) => {
        SendMessage("YaSDK", "OnPurchaseSuccess", JSON.stringify(purchase));
      })
      .catch((err) => {
        SendMessage("YaSDK", "OnPurchaseReject", JSON.stringify(err));
      });
  },

  Yandex_GetPurchased: function () {
    if (typeof payments === "undefined") {
      console.log("[YaSDK] payments not initialized");
      return;
    }

    payments.getPurchases().then((purchases) => {
      SendMessage(
        "YaSDK",
        "OnPurchases",
        JSON.stringify({ purchases: purchases })
      );
    });
  },

  Yandex_GetCatalog: function (iconSizePtr) {
    if (typeof payments === "undefined") {
      console.log("[YaSDK] payments not initialized");
      return;
    }

    let iconSize = UTF8ToString(iconSizePtr);

    payments.getCatalog().then((products) => {
      const result = products.map((p) => ({
        id: p.id,
        title: p.title,
        description: p.description,
        imageURI: p.imageURI,
        price: p.price,
        priceValue: p.priceValue,
        priceCurrencyCode: p.priceCurrencyCode,
        currencyIcon: p.getPriceCurrencyImage(iconSize),
      }));

      SendMessage(
        "YaSDK",
        "OnCatalogReceived",
        JSON.stringify({ products: result })
      );
    });
  },

  Yandex_ConsumePurchase: function (purchaseTokenPtr) {
    if (typeof payments === "undefined") {
      console.log("[YaSDK] payments not initialized");
      return;
    }

    let purchaseToken = UTF8ToString(purchaseTokenPtr);
    payments.consumePurchase(purchaseToken);
  },

  Yandex_SetScore: function (leaderboardNamePtr, score) {
    if (typeof ysdk === "undefined") {
      console.log("[YaSDK] ysdk not initialized");
      return;
    }

    let leaderboardName = UTF8ToString(leaderboardNamePtr);
    ysdk
      .getLeaderboards()
      .then((lb) => lb.setLeaderboardScore(leaderboardName, score))
      .then(() => console.log("[YaSDK] Score set"))
      .catch((e) => console.error(e));
  },

  Yandex_GetPlayerEntry: function (leaderboardNamePtr, avatarSizePtr) {
    if (typeof ysdk === "undefined") {
      console.log("[YaSDK] ysdk not initialized");
      return;
    }

    let avatarSize = UTF8ToString(avatarSizePtr);
    let leaderboardName = UTF8ToString(leaderboardNamePtr);
    ysdk.leaderboards
      .getPlayerEntry(leaderboardName)
      .then((entry) => {
        entry.player.avatarSrc = entry.player.getAvatarSrc(avatarSize);
        delete entry.player.getAvatarSrc;
        delete entry.player.getAvatarSrcSet;

        SendMessage("YaSDK", "OnPlayerEntryRecive", JSON.stringify(entry));
      })
      .catch((err) => {
        if (err.code === "LEADERBOARD_PLAYER_NOT_PRESENT") {
          SendMessage("YaSDK", "OnPlayerNotPresent");
        }
      });
  },

  Yandex_GetEntries: function (
    leaderboardNamePtr,
    include,
    around,
    top,
    avatarSizePtr
  ) {
    if (typeof ysdk === "undefined") {
      console.log("[YaSDK] ysdk not initialized");
      return;
    }

    let avatarSize = UTF8ToString(avatarSizePtr);
    let leaderboardName = UTF8ToString(leaderboardNamePtr);

    ysdk.leaderboards
      .getEntries(leaderboardName, {
        quantityTop: top,
        includeUser: include,
        quantityAround: around,
      })
      .then((result) => {
        const entries = result.entries.map((entry) => {
          return {
            score: entry.score,
            extraData: entry.extraData,
            rank: entry.rank,
            formattedScore: entry.formattedScore,
            player: {
              avatarSrc: entry.player.getAvatarSrc(avatarSize),
              lang: entry.player.lang,
              publicName: entry.player.publicName,
              scopePermissions: entry.player.scopePermissions,
              uniqueID: entry.player.uniqueID,
            },
          };
        });

        const response = {
          leaderboard: result.leaderboard,
          ranges: result.ranges,
          userRank: result.userRank,
          entries: entries,
        };

        SendMessage("YaSDK", "OnEntriesRecive", JSON.stringify(response));
      })
      .catch((err) => {
        console.error("[YaSDK] Yandex_GetEntries error:", err);
      });
  },

  Yandex_GetEnvironment: function () {
    if (typeof ysdk === "undefined") {
      console.log("[YaSDK] player not initialized");
      return;
    }

    const str = JSON.stringify(ysdk.environment);
    const bufferSize = lengthBytesUTF8(str) + 1;
    const buffer = _malloc(bufferSize);
    stringToUTF8(str, buffer, bufferSize);
    return buffer;
  },

  Yandex_GetServerTime: function () {
    if (typeof ysdk === "undefined") {
      console.log("[YaSDK] player not initialized");
      return;
    }

    return ysdk.serverTime();
  },

  Yandex_GetDeviceInfo: function () {
    if (typeof ysdk === "undefined") {
      console.log("[YaSDK] ysdk not initialized");
      return;
    }

    var str = ysdk.deviceInfo;
    var bufferSize = lengthBytesUTF8(str) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(str, buffer, bufferSize);
    return buffer;
  },
});
