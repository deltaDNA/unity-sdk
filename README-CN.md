![deltaDNA logo](https://deltadna.com/wp-content/uploads/2015/06/deltadna_www@1x.png)

## deltaDNA分析和智能广告Unity SDK

资源库包含了分析和智能广告SDK两部分源码。为了便于安装，它们被打包成相互分离的Unity包。分析SDK可以被独立安装，但智能广告SDK需要依赖于分析SDK。Unity包可以从GitHub直接下载，通过输入文件名查找到最新版本。通过“资源（Assets）->导入包（Import Package）->定制包（Custom Package）”导入到Unity。

分析SDK可以支持Unity 4和Unity 5，但是智能广告SDK只能支持Unity 5。

## 目录

* [分析](#分析)
* [快速入门](#快速入门)
 * [自定义事件](#自定义事件)
 * [吸引（Engage）](#吸引（Engage）)
* [智能广告](#智能广告)
 * [用法](#用法)
 * [创建空闲广告](#创建空闲广告)
 * [创建奖励广告](#创建奖励广告)
 * [使用吸引（Engage）](#使用吸引（Engage）)
 * [遗留接口](#遗留接口)
 * [事件](#事件)
* [iOS整合](#iOS整合)
 * [推送通知](#推送通知)
 * [iOS版智能广告](#iOS版智能广告)
* [Android整合](#Android整合)
 * [推送通知](#推送通知)
 * [Android版智能广告](#Android版智能广告)
 * [权限](#权限)
* [迁移](#迁移)
 * [4.2](#版本4.2)
 * [4.3](#版本4.3)
* [授权](#授权)

## 分析

我们的分析SDK已经完成了在Unity中的全部代码，不需要任何本地代码支持。在Unity支持的任何平台中都可以直接运行。入门的最简单方式是从资源库中下载`deltadna-sdk-*.unitypackage`，并导入到你的Unity项目中。

## 快速入门

有关于如何使用分析SDK的全部信息，请参阅我们的文档[网站](http://docs.deltadna.com/advanced-integration/unity-sdk/)。

查看在`Assets\DeltaDNA\Example`的这个例子以了解如何使用这个SDK。你只需要设置客户端版本和通过一个定制方法`MonoBehaviour`启用这个SDK。

```csharp
DDNA.Instance.ClientVersion = "1.0.0";
DDNA.Instance.StartSDK("YOUR_ENVIRONMENT_KEY",
                       "YOUR_COLLECT_URL",
                       "YOUR_ENGAGE_URL");
```

第一次运行时，这将创建新用户的id并发送一个`newPlayer`事件。每次调用时，它将发送一个`gameStarted`和`clientDevice`事件。

### 自定义事件

你可以轻松的通过使用`GameEvent`类标记自定义事件。使用你的事件项目名称创建一个`GameEvent`方法。调用`AddParam`函数来给这个事件添加自定义事件属性。例如：

```csharp
var gameEvent = new GameEvent("myEvent")
    .AddParam("option", "sword")
    .AddParam("action", "sell");

DDNA.Instance.RecordEvent(gameEvent);
```

### 吸引（Engage）

通过一个`Engagement`方法改变游戏的行为。例如：

```csharp
var engagement = new Engagement("gameLoaded")
    .AddParam("userLevel", 4)
    .AddParam("experience", 1000)
    .AddParam("missionName", "Disco Volante");

DDNA.Instance.RequestEngagement(engagement, (response) =>
{
    // 响应（Response）是一个从吸引（Engage）返回的键值字典。
    // 如果找不到匹配的活动或发生错误，此字段将为空。
});
```

如果你需要对这个来自吸引（Engage）使用`DDNA.Instance.RequestEngagement(Engagement engagement, Action<Engagement> onCompleted, Action<Exception> onError)`的响应更多的控制。这将使用包括来自吸引（Engage）响应的吸引（Engagement）来调用onCompleted回调。如果发生任何错误，你还可以处理。如果吸引（Engagement）支持，使用这个方法可以随意的创建一个`ImageMessage`。例如：

```csharp
var engagement = new Engagement("imageMessage")
    .AddParam("userLevel", 4)
    .AddParam("experience", 1000)
    .AddParam("missionName", "Disco Volante");

DDNA.Instance.RequestEngagement(engagement, (response) => {

    ImageMessage imageMessage = ImageMessage.Create(response);

    // 检查我们对一个有效的图片消息获取吸引（Engagement）
    if (imageMessage != null) {
        imageMessage.OnDidReceiveResources += () => {
            // 一旦我们获得这个资源就可以展示出来。
            imageMessage.Show();
        };
        // 下载图片消息资源。
        imageMessage.FetchResources();
    }
    else {
        // 吸引（Engage）没有返回一个图片消息。
    }
}, (exception) => {
    Debug.Log("Engage reported an error: "+exception.Message);
});
```

## 智能广告

将智能广告集成到你的Unity项目中需要与我们提供的源码相分离的本地代码扩展。[这里](http://docs.deltadna.com/advanced-integration/smart-ads/)提供了关于如何接入我们智能广告平台的更多信息。添加Unity扩展需要下载和导入`deltadna-smartads-*.unitypackage`。我们支持iOS和Andriod平台。

### 用法

学习如何使用智能广告的最快方法是查阅`Assets\DeltaDNAAds\Example`中的例子。`AdsDemo`类展示了如何使用空闲广告和奖励广告。通过调用`RegisterForAds`函数可以激活对智能广告的支持。这*必须*在开启了分析SDK以后才能够被调用。`DDNASmartAds`类定义了一系列的事件，你可以通过标记回调来获知广告何时开启或关闭。

启用分析SDK。

```csharp
DDNA.Instance.ClientVersion = "1.0.0";
DDNA.Instance.StartSDK("YOUR_ENVIRONMENT_KEY",
                       "YOUR_COLLECT_URL",
                       "YOUR_ENGAGE_URL");
```

注册广告。

```csharp
DDNASmartAds.Instance.RegisterForAds();
```

如果一切顺利，智能广告服务将开始从后台抓取广告。`DDNASmartAds`类提供了如下代理以报告是否服务被成功配置：

* `OnDidRegisterForInterstitialAds` - 当空闲广告被成功配置时调用。
* `OnDidFailToRegisterForInterstitialAds` - 如果空闲广告因某些原因无法被配置时调用。
* `OnDidRegisterForRewardedAds` - 当奖励广告被成功配置时调用。
* `OnDidFailToRegisterForRewardedAds` - 当奖励广告因某些原因无法被配置时调用。

### 创建空闲广告

空闲广告是玩家可以通过关闭按钮关闭的全屏弹出式广告。为了展示一个空闲广告，尝试创建一个`InterstitialAd`然后展示它。

```csharp
var interstitialAd = InterstitialAd.Create();
if (interstitialAd != null) {
    interstitialAd.Show();
}
```

`Create`检查游戏是否要求在这一点展示广告。其检查广告是否已经加载，这一会话中广告的数量是否已经超出，以及自上一广告展示后时间是否够久。如果返回一个非空对象，你将可以展示广告。这将允许你轻松控制从我们的平台给你的玩家播放的广告数量和频率。

以下事件可以被添加到一个`InterstitialAd`：

* `OnInterstitialAdOpened` - 当广告在屏幕显示时调用。
* `OnInterstitialAdFailedToOpen` - 如果广告因某些原因未能成功打开时调用。
* `OnInterstitialAdClosed` - 当广告被关闭时调用。

### 创建奖励广告

奖励广告是一个短视频，通常长度为30秒。在能够关闭前玩家必须看完。为了展示一个奖励广告，尝试创建一个`RewardedAd`然后展示它。

```csharp
var rewardedAd = RewardedAd.Create();
if (rewardedAd != null) {
    rewardedAd.Show();
}
```

与`InterstitialAd`的`Create`方法只返回一个对象相同，如果你被允许展示一个奖励广告，那么将会有一个可用的广告在这一点展示。因此，例如当你得到一个非空对象时，你可以给玩家展示一个UI显示将为他们提供一个奖励广告来观看。

以下事件可以被添加到一个`RewardedAd`：

* `OnRewardedAdOpened` - 当广告在屏幕显示时调用。
* `OnRewardedAdFailedToOpen` - 如果广告因某些原因未能成功打开时调用。
* `OnRewardedAdClosed` - 当广告结束时调用。一个布尔奖励标志位表示这个广告是否被观看了足够多的内容，从而你可以奖励玩家。

### 使用吸引

要充分利用deltaDNA的智能广告的优势，你需要使用我们的吸引（Engage）服务。如果游戏要向某个特定玩家展示一个广告，那么需要使用吸引（Engage）。吸引（Engage）将根据哪个活动在进行以及玩家在哪个分组中来定制响应。你尝试从一个`Engagement`对象创建一个广告，其将只会在吸引（Engage）响应允许且会话、时间以及加载限制都符合时能够成功。我们还可以添加游戏可以使用的额外参数到吸引（Engage）响应中，也许可以为玩家自定义奖励。

```csharp
var engagement = new Engagement("showRewarded");

DDNA.Instance.RequestEngagement(engagement, response => {

    var rewardedAd = RewardedAd.Create(response);

    if (rewardedAd != null) {

        // 查看什么奖励被提供
        if (rewardedAd.Parameters.ContainsKey("rewardAmount")) {
            int rewardAmount = System.Convert.ToInt32(rewardedAd.Parameters["rewardAmount"]);

            // 当前为玩家提供...

            // 如果他们选择查看添加
            rewardedAd.Show();
        }
    }

}, exception => {
    Debug.Log("Engage encountered an error: "+exception.Message);
});
```

查看包含的案例项目以了解更多信息。

### 遗留接口

在将`InterstitialAd`和`RewardedAd`类包含进来之前，你可以直接从`DDNASmartAds`对象中显示广告。这在广告类使用后仍然可用，但是其最好单独使用。

你可以使用`DDNASmartAds.Instance.IsInterstitialAdAvailable()`测试一个空闲广告是否已经被加载。通过调用`DDNASmartAds.Instance.ShowInterstitialAd()`展示一个空闲广告。你可以使用`DDNASmartAds.Instance.IsRewardedAdAvailable()`测试一个奖励广告是否已经被加载。通过调用`DDNASmartAds.Instance.ShowRewardedAd()`展示一个奖励广告。由于会话和时间的限制，这些调用不会事先告诉你展示广告是否会失败。这也是为什么我们推荐使用`InterstitialAd`和`RewardedAd`类的原因。

使用决策点（Decision Points）的其他展示方法现在已经被弃用，因为它们隐藏了什么吸引（Engage）被返回。这将阻止你控制是否以及何时在你的游戏中展示广告。

### 事件

回调可以被添加到下述事件，从而获知广告何时开启或关闭。

* `OnDidRegisterForInterstitialAds` - 当你成功将空闲广告嵌入到你的游戏时调用。
* `OnDidFailToRegisterForInterstitialAds` - 当如果因某些原因一个空闲广告不能使用时调用。通过一个字符串参数报告可能的错误。
* ~~`OnInterstitialAdOpened` - 当一个空闲广告显示在屏幕时调用。~~首选`InterstitialAd.OnInterstitialAdOpened`。
* ~~`OnInterstitialAdFailedToOpen` - 如果一个空闲广告显示失败时调用。~~首选`InterstitialAd.OnInterstitialAdFailedToOpen`。
* ~~`OnInterstitialAdClosed` - 当用户关闭一个空闲广告时调用。~~首选`InterstitialAd.OnInterstitialAdClosed`。
* `OnDidRegisterForRewardedAds` - 当你成功将奖励广告嵌入到你的游戏时调用。
* `OnDidFailToRegisterForRewardedAds` - 当如果因某些原因一个奖励广告不能使用时调用。通过一个字符串参数报告可能的错误。
* ~~`OnRewardedAdOpened` - 当一个奖励广告显示在屏幕时调用。~~首选`RewardedAd.OnRewardedAdOpened`。
* ~~`OnRewardedAdFailedToOpen` - 如果一个奖励广告显示失败时调用。~~首选`RewardedAd.OnRewardedAdFailedToOpen`。
* ~~`OnRewardedAdClosed` - 当用户关闭一个奖励广告时调用。一个布尔参数被标识用户是否完整的看完这个奖励广告。~~查看`RewardedAd.OnRewardedAdClosed`。

## iOS整合

### 推送通知

要支持iOS推送通知，你需要调用`IosNotifications.RegisterForPushNotifications()`。这使用Unity的`NotificationServices`来请求一个推送Token，然后在一个`notificationServices`事件中回传给我们。你还需要将游戏的相关APN证书输入我们的平台。

我们通过玩家点击推送通知记录你的游戏是否开始。然而要使其正常工作，`DDNA`游戏对象需要比游戏运行更早一点儿被加载。这可以通过在管理SDK的一个游戏对象中的`Awake`方法内添加一个代理到`OnDidLaunchWithPushNotification`来完成。

### iOS版智能广告

我们使用[CocoaPods](https://cocoapods.org/)来安装我们的智能广告库以及第三方广告网络库。除了我们支持的所有广告网络，包含的Podfile将添加我们的iOS智能广告Pod到你的XCode项目。一个后期处理构建Hook配备Unity生成的XCode项目以支持CocoaPods，并添加Podfile到iOS构建路径。这时其运行`pod install`以下载依赖（Dependencies）和创建*Unity-iPhone.xcworkspace*。由于Unity不知道工作空间文件，你将需要打开它。点击*build and run*因此将不被支持。

__广告网络要求最低对象版本是第7版，最好是第8版以获得最新的SDK。如果默认使用第6版，cocoapods将会失败，xcworkspace文件将不会生成。__

__如果从一个之前的SDK版本升级，运行`pod repo update`以更新你的本地缓存并确保你使用最新的依赖进行构建。从CocoaPods v1.0起将不再会默认执行`pod install`。__

要选择哪个广告网络应当被包含到游戏中，需从Unity菜单栏中选择*DeltaDNA*，导航到*智能广告（SmartAds）->选择网络（Select Networks）*，然后将打开一个用于设置的选项卡。这时广告网络可以被选中或取消选中，点击*应用（Apply）*将保存更改。

如果你更改已经启用的网络，Podfile的更改应提交到版本控制。

#### UnityAds

最新版的Unity会与Unity内部的UnityAds插件产生冲突。当PostBuildProcess方法运行时可能会发生一个错误。我已经通过让pod install进程最后运行来解决了这个问题。如果你的游戏包含多个库，你可能需要使用PostProcessBuild调用来改变PostBuildProcess的顺序。

## Android整合

### Android依赖Google Firebase/Play Services库

任何库依赖，例如Google的Firebase（Google Play Services）都由Google的[Unity Jar Resolver](https://github.com/googlesamples/unity-jar-resolver)插件控制。这些库将自动下载到*Assets/Plugins/Android*文件夹中。如果你在你的应用中有其他的不使用这个Resolver下载依赖的Unity插件，你可能想要考虑也使用这个Resolver来管理他们的依赖，否则你将不得不手动解决所有冲突。

### 推送通知

我们的推送通知使用Firebase进行消息传递（这在4.3版本中已更改，如果你在升级请查看下面的迁移[指导](#version-4.3)）。为了配置通知，你将需要在配置页面设置*应用（Application）*和*发送者ID（Sender IDs）*，这可以从Unity编辑（Editor）菜单的*DeltaDNA->通知（Notifications）->Android->配置（Configure）*进入。ID可以从你的应用（[1](Docs/firebase_console_1.png)，[2](Docs/firebase_console_2.png)和[3](Docs/firebase_console_3.png)）的Firebase控制面板找到。点击*应用（Apply）*将保存对你的项目中资源文件的更改，这应当被提交到源码管理。

如果你的应用使用Google Cloud Console设置，你可以从[这里](https://developers.google.com/cloud-messaging/android/android-migrate-fcm#import_your_gcm_project_as_a_firebase_project)找到关于如何迁移项目到Firebase的操作指南。Firebase项目向下兼容使用Google Could Messaging的应用。

推送通知的样式可以通过覆盖库行为改变。关于如何做到这一点的说明可以从[这里](https://github.com/deltaDNA/android-sdk/tree/master/library-notifications#unity)找到。一旦你添加了修改的库或者添加了新的类作为一个单独的库，你将需要在配置中将*Listener Service*字段更改为你的新类的完全限定名。

如果你不再想要在Android上使用推送通知，那么你可以从项目中删除*Assets/DeltaDNA/Plugins/Android*和*Assets/Plugins/Android/deltadna-sdk-unity-notifications*文件夹以减少方法的数量和你的游戏APK的大小。

### Android版智能广告

你想要构建到你的应用的广告网络可以从*DeltaDNA->智能广告（SmartAds）->选择网络（Select Networks）*中选择。在应用这个更改后，SDK将从Maven仓库中下载最新的库。如果你更改为启用网络，那么对*build.gradle*文件的更改应当提交到版本控制。

库可以随时从*DeltaDNA->智能广告（SmartAds）->Android->下载库（Download Libraries）*菜单项下载。我们建议在更新DletaDNA SDK或从版本控制中提取更改后执行此操作。SDK将尝试检测何时下载的库会过期，并在编辑器控制面板记录一个警告。

为了使菜单项工作，你需要为你的Unity项目安装和设置Android SDK。在Android SDK中，你需要安装一个*build-tools*版本和一个*SDK平台*，以及最新版本的*Android Support Repository*和*Google Repository*。

### MultiDex；解决Android的65k方法限制
1. 使用*Gradle*构建系统导出你的Unity项目。这些操作可以从*构建设置（Build Setting）*对话框中找到。
2. 在Android Studio中打开导出的项目，如果被要求，并选择使用Gradle封装。
3. 为你的项目打开顶层的*build.gradle*文件，并按照[此处](https://developer.android.com/studio/build/multidex.html#mdex-gradle)的描述应用MultiDex解决方法。

### 权限

Android库要求的权限可以通过使用[Android清单联合体](http://tools.android.com/tech-docs/new-build-system/user-guide/manifest-merger)被覆盖。例如，如果你想要从`WRITE_EXTERNAL_STORAGE`权限删除`maxSdkVersion`属性，那么你可以在你的清单文件中做如下指定：
```xml
<uses-permission
    android:name="android.permission.WRITE_EXTERNAL_STORAGE"
    tools:remove="android:maxSdkVersion"/>
```

如果上述情况仍然在清单合并过程中导致冲突，那么可以在清单（manifest）文件中使用如下内容：
```xml
<uses-permission
    android:name="android.permission.WRITE_EXTERNAL_STORAGE"
    tools:merge="override"/>
```

## 迁移

### 版本4.2
配置哪些网络应当被用于智能广告已经更改为添加菜单项到Unity编辑器的方式，这取消了一些容易出错的手动步骤。对于Android，不再需要通过安装Python或者设置Android SDK路径来下载库依赖，因为这一任务的菜单项将会处理这些步骤。如果你更改了选择的网络，你将需要把对build.gradle和/或Podfile的更改提交到你的版本控制，以便你的其他团队成员可以使用这些更改。

由于我们没有更改如何定义智能广告网络，你可能需要查看选中的网络以防止你从你的项目中事先移除了其中的一些网络。

### 版本4.3
在版本4.2和版本4.3之间，我们更新了我们的推送通知以使用Firebase（play-services-*-10.2）。这需要改变推送通知整合工作的方式。要更好的管理Android依赖，我们现在使用Google的[Unity Jar Resolver](https://github.com/googlesamples/unity-jar-resolver)。这允许其他插件也在Firebase/Paly-Services库中指定依赖，且Unity Jar Resolver将确定哪个库被使用，希望可以减少在构建时的重复库错误。

#### SDK正常检查
在你升级SDK后，你可以执行正常检查以识别与前一版本相关的错误，例如冲突的配置记录和重复的库。这可以从编辑（Editor）菜单的*DeltaDNA->正常检查SDK*进入。请注意你的项目仍然可能存在这个功能无法检测的问题。请始终参阅这个文档以了解更多详细信息。

#### Android依赖
在导入新的DeltaDNA SDK包到你的项目后，请确保已经从*Assets/DeltaDNA/Plugins/Android*删除了旧的*deltadna-sdk-notifications* AAR文件。你还需要删除在那个位置的所有*play-services*和*support* AAR和JAR库，因为其将导致与使用Unity Jar Resolver下载的库之间的冲突。

与所有SDK更新一样，你应当从*DeltaDNA->智能广告（SmartAds）->Android->下载库（Download Libraries）*更新Android智能广告（SmartAds）库。

#### Android通知
我们已经添加了一个UI用于在Android配置推送通知，其可以从Unity编辑（Editor）菜单中的*DeltaDNA->通知（Notifications）->Android->配置（Configure）*进行访问。如果你想要使用通知或者已经在我们SDK之前版本中使用了它们，你将需要从Firebase控制面板为你等应用填写应用（Application）和发送者ID（Sender IDs）。

我们强烈建议删除所有之前从*Assets/Plugins/Android*的*AndroidManifest.xml*文件为DeltaDNA通知添加的记录，因为其可能与Firebase的实施相冲突。如果你从未添加任何其他东西到清单文件，那么你或许可以将其全部删除。关于哪个XML应当被删除的更多详细信息请查看[这里](https://github.com/deltaDNA/android-sdk/blob/master/docs/migrations/4.3.md#manifest)。另外你还应能够从*Assets/Plugins/Android/res/values*删除包含你的应用的发送者ID（Sender ID）的*string*资源。

如果你不再想要使用通知，那么请从你的项目删除*Assets/Plugins/Android/deltadna-sdk-unity-notifications*和*Assets/DeltaDNA/Plugins/Android*文件夹。

## 授权

该资源适用于Apache2.0授权。
