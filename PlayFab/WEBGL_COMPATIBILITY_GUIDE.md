# PlayFab WebGL互換性ガイド

## 概要
現在のPlayFab実装は、WebGL環境で直接実行できない問題があります。  
このガイドでは、**関数名・ファイル名・クラス名は変更せず**、WebGL対応に必要な修正をまとめます。

---

## 🔴 主な問題点と対策

### 1. **SystemInfo.deviceUniqueIdentifier の非互換性**

#### ❌ 現在のコード（Line 14）
```csharp
CustomId = UnityEngine.SystemInfo.deviceUniqueIdentifier,
```

#### 問題
- WebGL環境では `SystemInfo.deviceUniqueIdentifier` が機能しません
- 毎回異なる値が返されるか、空文字列が返されます

#### ✅ WebGL対応の解決策
```csharp
// 方法1: PlayerPrefs + GUID（推奨）
string customId;
const string CUSTOM_ID_KEY = "PlayFab_CustomID";

if (!PlayerPrefs.HasKey(CUSTOM_ID_KEY))
{
    customId = System.Guid.NewGuid().ToString();
    PlayerPrefs.SetString(CUSTOM_ID_KEY, customId);
    PlayerPrefs.Save();
}
else
{
    customId = PlayerPrefs.GetString(CUSTOM_ID_KEY);
}

// 方法2: 条件付き使い分け
#if UNITY_WEBGL
    CustomId = GetWebGLDeviceId(),
#else
    CustomId = UnityEngine.SystemInfo.deviceUniqueIdentifier,
#endif

// 方法3: ハイブリッド対応
private string GetDeviceId()
{
    #if UNITY_WEBGL
        return GetOrCreateWebGLDeviceId();
    #else
        return UnityEngine.SystemInfo.deviceUniqueIdentifier;
    #endif
}

private string GetOrCreateWebGLDeviceId()
{
    const string KEY = "PlayFab_WebGL_DeviceID";
    if (!PlayerPrefs.HasKey(KEY))
    {
        string deviceId = System.Guid.NewGuid().ToString();
        PlayerPrefs.SetString(KEY, deviceId);
        PlayerPrefs.Save();
    }
    return PlayerPrefs.GetString(KEY);
}
```

---

### 2. **コルーチンとタイミングの問題**

#### ❌ 現在の問題
- PlayFab API は非同期ですが、WebGL では `SendMessage` や特殊なスレッド処理が制限されます
- タイムアウト処理が必要な場合、通常のスレッド待機は機能しません

#### ✅ WebGL対応の解決策

**PlayFabService.cs に追加：**
```csharp
// タイムアウト処理の追加（WebGL対応）
private Coroutine currentRequest;
private const float REQUEST_TIMEOUT = 30f;

public void Login(Action onSuccess, Action<string> onError)
{
    // 前回のリクエストがある場合はキャンセル
    if (currentRequest != null)
    {
        StopCoroutine(currentRequest);
    }

    var request = new LoginWithCustomIDRequest
    {
        CustomId = GetDeviceId(),
        CreateAccount = true
    };

    var timeoutAction = new Action<string>(error =>
    {
        currentRequest = null;
        onError?.Invoke(error);
    });

    var successAction = new Action(async () =>
    {
        currentRequest = null;
        isLoggedIn = true;
        onSuccess?.Invoke();
    });

    PlayFabClientAPI.LoginWithCustomID(request,
        result => { successAction?.Invoke(); },
        error => { timeoutAction?.Invoke(error.GenerateErrorReport()); });
}

// Coroutine ベースのタイムアウト処理
private IEnumerator WithTimeout(System.Action action, float timeout)
{
    float elapsed = 0f;
    while (elapsed < timeout)
    {
        elapsed += Time.deltaTime;
        yield return null;
    }
    // タイムアウト時の処理
}
```

---

### 3. **HTTP 通信の HTTPS 要件**

#### ❌ 現在の問題
- WebGL は **HTTPS環境でのみ動作**します
- HTTP API 呼び出しはブロックされます

#### ✅ WebGL対応の解決策

**PlayFabBootstrap.cs に追加：**
```csharp
using UnityEngine;

public class PlayFabBootstrap : MonoBehaviour
{
    public static IPlayFabService Service { get; private set; }

    void Awake()
    {
        if(Service != null) return;

        #if UNITY_WEBGL
            // WebGL環境での初期化確認
            Debug.Log("PlayFab: WebGL環境で実行中");
            ValidateWebGLEnvironment();
        #endif

        Service = new PlayFabService();
    }

    #if UNITY_WEBGL
    private void ValidateWebGLEnvironment()
    {
        // HTTPS確認（開発環境ではlocalhostも許可）
        bool isHttps = System.Uri.UriSchemeHttps == new System.Uri(Application.absoluteURL).Scheme
                    || Application.absoluteURL.Contains("localhost");
        
        if (!isHttps)
        {
            Debug.LogError("PlayFab: WebGL環境ではHTTPSが必須です。HTTP接続は使用できません。");
        }
    }
    #endif
}
```

---

### 4. **データの永続化の問題**

#### ❌ 現在の問題
- WebGL は PlayerPrefs を使用していますが、ブラウザのキャッシュ削除で失われます
- ローカルストレージは制限があります

#### ✅ WebGL対応の解決策

```csharp
// PlayFabService.cs に追加

private const string LOGIN_CACHE_KEY = "PlayFab_LastLogin";
private const string USER_ID_CACHE_KEY = "PlayFab_UserID";

public void Login(Action onSuccess, Action<string> onError)
{
    var request = new LoginWithCustomIDRequest
    {
        CustomId = GetDeviceId(),
        CreateAccount = true
    };

    PlayFabClientAPI.LoginWithCustomID(request,
        result =>
        {
            isLoggedIn = true;
            
            // ログイン情報をキャッシュ（WebGL対応）
            #if UNITY_WEBGL
                PlayerPrefs.SetString(LOGIN_CACHE_KEY, System.DateTime.Now.ToString());
                PlayerPrefs.SetString(USER_ID_CACHE_KEY, result.PlayFabId);
                PlayerPrefs.Save();
            #endif
            
            onSuccess?.Invoke();
        },
        error =>
        {
            onError?.Invoke(error.GenerateErrorReport());
        });
}

// キャッシュから前回のセッションを復元
public bool TryRestoreSession()
{
    #if UNITY_WEBGL
        if (PlayerPrefs.HasKey(USER_ID_CACHE_KEY))
        {
            isLoggedIn = true;
            return true;
        }
    #endif
    return false;
}
```

---

### 5. **コンソール出力と デバッグ の制限**

#### ❌ 現在の問題
- WebGL では `System.Diagnostics` が制限されています
- 一部のログ出力が機能しません

#### ✅ WebGL対応の解決策

```csharp
// ユーティリティクラス追加
public static class PlayFabDebug
{
    public static void Log(string message)
    {
        #if UNITY_WEBGL
            Debug.Log($"[PlayFab WebGL] {message}");
        #else
            Debug.Log($"[PlayFab] {message}");
        #endif
    }

    public static void LogError(string message)
    {
        #if UNITY_WEBGL
            Debug.LogError($"[PlayFab WebGL ERROR] {message}");
        #else
            Debug.LogError($"[PlayFab ERROR] {message}");
        #endif
    }
}

// 使用例
PlayFabDebug.Log("Login successful");
```

---

## 📋 修正チェックリスト

### PlayFabService.cs
- [ ] `SystemInfo.deviceUniqueIdentifier` を PlayerPrefs + GUID に置き換え
- [ ] `GetDeviceId()` ヘルパー関数を追加
- [ ] HTTPS環境チェック機能を追加（オプション）
- [ ] ログイン情報のキャッシュ機能を追加
- [ ] WebGL 専用デバッグログ関数を使用

### PlayFabBootstrap.cs
- [ ] `#if UNITY_WEBGL` による環境判定コードを追加
- [ ] HTTPS環境の検証処理を追加

### IPlayFabService.cs
- [ ] 新規メソッド `bool TryRestoreSession()` を追加（オプション）

---

## 🧪 テスト方法

### 1. エディタでのテスト
```csharp
// Test用コード
public void TestWebGLMode()
{
    #if UNITY_WEBGL
        Debug.Log("WebGL mode: Enabled");
    #else
        Debug.Log("WebGL mode: Disabled");
    #endif
}
```

### 2. ビルド設定確認
- Platform: **WebGL** に変更
- Compression Format: **Gzip** (推奨)
- Build and Run で テスト

### 3. ブラウザコンソール確認
- F12 で Developer Tools を開く
- Console タブでエラーを確認
- Network タブで API 通信をチェック

---

## 🚀 実装の優先順位

1. **高優先度（必須）**
   - [ ] `SystemInfo.deviceUniqueIdentifier` の置き換え
   - [ ] HTTPS環境チェック

2. **中優先度（推奨）**
   - [ ] セッション復元機能
   - [ ] WebGL デバッグログ

3. **低優先度（オプション）**
   - [ ] タイムアウト処理
   - [ ] 詳細なエラーハンドリング

---

## 📚 参考資料

- [PlayFab Official Docs](https://docs.microsoft.com/en-us/gaming/playfab/)
- [Unity WebGL Documentation](https://docs.unity3d.com/Manual/webgl.html)
- [Browser Storage Limits](https://developer.mozilla.org/en-US/docs/Web/API/Storage)

---

## ⚠️ 注意点

1. **ローカルストレージ容量**
   - WebGL の PlayerPrefs は通常 5-10MB に制限されています

2. **クロスオリジン（CORS）**
   - PlayFab サーバーと異なるドメインからアクセスする場合は CORS 設定が必要です

3. **キャッシュクリア**
   - ユーザーがブラウザキャッシュをクリアするとデバイスIDが失われます
   - アカウント紛失対策としてクラウド同期を検討してください

4. **パフォーマンス**
   - WebGL の非同期処理は デスクトップ版より遅い可能性があります
   - 適切なローディング画面を実装してください
