![deltaDNA logo](https://deltadna.com/wp-content/uploads/2015/06/deltadna_www@1x.png)

## deltaDNA分析和智能广告Unity SDK

资源库包含了分析和智能广告SDK两部分源码。为了便于安装，它们被打包成相互分离的Unity包。分析SDK可以被独立安装，但智能广告SDK需要依赖于分析SDK。Unity包可以从GitHub直接下载，通过输入文件名查找到最新版本。通过“资源（Assets）->导入包（Import Package）->定制包（Custom Package）”导入到Unity。

分析SDK可以支持Unity 4和Unity 5，但是智能广告SDK只能支持Unity 5。

## 分析

我们的分析SDK已经完成了在Unity中的全部代码，不需要任何本地代码支持。在Unity支持的任何平台中都可以直接运行。入门的最简单方式是从资源库中下载`deltadna-sdk-*.unitypackage`，并导入到你的Unity项目中。

## 快速开始

有关于如何使用分析SDK的全部信息，请参阅我们的文档[网站](http://docs.deltadna.com/advanced-integration/unity-sdk/)。

查看在`Assets\DeltaDNA\Example`的这个例子以了解如何使用这个SDK。你只需要设置客户端版本和通过一个定制方法`MonoBehaviour`启用这个SDK。

```csharp
DDNA.Instance.ClientVersion = "1.0.0";
DDNA.Instance.StartSDK("YOUR_ENVIRONMENT_KEY",
                       "YOUR_COLLECT_URL",
                       "YOUR_ENGAGE_URL");
```

第一次运行时，这将创建新用户的id并发送一个`newPlayer`事件。每次调用时，它将发送一个`gameStarted`和`clientDevice`事件。

#### 自定义事件

你可以轻松的通过使用`GameEvent`类标记自定义事件。使用你的事件项目名称创建一个`GameEvent`方法。调用`AddParam`函数来给这个事件添加自定义事件属性。例如：

```csharp
var gameEvent = new GameEvent("myEvent")
    .AddParam("option", "sword")
    .AddParam("action", "sell");

DDNA.Instance.RecordEvent(gameEvent);
```

#### 吸引（Engage）

通过一个`Engagement`方法改变游戏的行为。例如：

```csharp
var engagement = new Engagement("gameLoaded")
    .AddParam("userLevel", 4)
    .AddParam("experience", 1000)
    .AddParam("missionName", "Disco Volante");

DDNA.Instance.RequestEngagement(engagement, (response) =>
{
    // 响应（Response）是一个从吸引（Engage）返回的键值字典
});
```

## 智能广告

将智能广告集成到你的Unity项目中需要与我们提供的源码相分离的本地代码扩展。[这里](http://docs.deltadna.com/advanced-integration/smart-ads/)提供了关于如何接入我们智能广告平台的更多信息。添加Unity扩展需要下载和导入`deltadna-smartads-*.unitypackage`。我们支持iOS和Andriod平台。

### 用法

学习如何使用智能广告的最快方法是查阅`Assets\DeltaDNAAds\Example`中的例子。`AdsDemo`类展示了如何使用空闲广告和奖励广告。通过调用`RegisterForAds`函数可以激活对智能广告的支持。这必须在开启了分析SDK以后才能够被调用。`DDNASmartAds`类定义了一系列的事件，你可以通过标记回调来获知广告何时开启或关闭。

开启分析SDK。

```csharp
DDNA.Instance.ClientVersion = "1.0.0";
DDNA.Instance.StartSDK("YOUR_ENVIRONMENT_KEY",
                       "YOUR_COLLECT_URL",
                       "YOUR_ENGAGE_URL");
```

标记广告。

```csharp
DDNASmartAds.Instance.RegisterForAds();
```

你可以使用`DDNASmartAds.Instance.IsInterstitialAdAvailable()`函数测试一个空闲广告是否可以显示。

通过调用`DDNASmartAds.Instance.ShowInterstitialAd()`函数展示一个空闲广告。

你可以使用`DDNASmartAds.Instance.IsRewardedAdAvailable()`函数测试一个奖励广告是否可以显示。

通过调用`DDNASmartAds.Instance.ShowRewardedAd()`函数展示一个奖励广告。

#### 事件

回调可以被添加到下述事件，从而获知广告何时开启或关闭。

* `OnDidRegisterForInterstitialAds` - 当你成功将空闲广告嵌入到你的游戏时调用。
* `OnDidFailToRegisterForInterstitialAds` - 当如果因某些原因一个空闲广告不能使用时调用。通过一个字符串参数报告可能的错误。
* `OnInterstitialAdOpened` - 当一个空闲广告显示在屏幕时调用。
* `OnInterstitialAdFailedToOpen` - 如果一个空闲广告显示失败时调用。
* `OnInterstitialAdClosed` - 当用户关闭一个空闲广告时调用。
* `OnDidRegisterForRewardedAds` - 当你成功将奖励广告嵌入到你的游戏时调用。
* `OnDidFailToRegisterForRewardedAds` - 当如果因某些原因一个奖励广告不能使用时调用。通过一个字符串参数报告可能的错误。
* `OnRewardedAdOpened` - 当一个奖励广告显示在屏幕时调用。
* `OnRewardedAdFailedToOpen` - 如果一个奖励广告显示失败时调用。
* `OnRewardedAdClosed` - 当用户关闭一个奖励广告时调用。一个布尔参数被标识用户是否完整的看完这个奖励广告。

#### 决策点

你可以通过使用*决策点*来添加对哪一类玩家可以看到广告的控制。通过`ShowInterstitialAd("pointInGameToShowAnAd")`或者`ShowRewardedAd("anotherPointInGameToShowAnAd")`展示一个广告，在deltaDNA的后台中标记这个决策点。SDK将询问是否在玩家现在的场景中显示广告。当你第一次集成时必须使用决策点，如果决策点没有在deltaDNA的后台被标记，它将被忽略而且广告将一直显示。

### iOS集成

我们使用[CocoaPods](https://cocoapods.org/)来安装我们的智能广告库外加第三方广告网络库。一个最小化的Podfile被包含在DeltaDNAAds/Editor/iOS。它将添加我们的iOS智能广告Pod连同我们支持的所有广告网络到你的Xcode项目。一个后期处理构建Hook配备Unity生成的XCode项目以支持CocoaPods，并添加Podfile到iOS构建路径。你必须从命令行运行`pod install`以安装这些Pod。最终打开由pod install创建的*Unity-iPhone.xcworkspace*。一个`pods.command`文件也被包含，从而为你运行pod install和打开XCode工作空间。

所包含的Podfile将为所有deltaDNA支持的广告网络提供支持。你可以定制Podfile以通过使用[Subspecs](https://guides.cocoapods.org/syntax/podfile.html#pod)下载仅仅是你要求的广告网络。这个过程和本地的[iOS智能广告SDK](https://github.com/deltaDNA/ios-smartads-sdk)相同。更多的关于定制Podfile的细节可以从那里找到。

### Android集成

我们提供一个Python脚本来帮助管理第三方广告网络Dependencies。在`Assets\DeltaDNAAds\Editor\Android`编辑`config.json`以添加你想要集成的广告网络。然后从命令行运行`download.py`。这将下载和复制这些依赖的AARs及Jar文件到`Assets\DeltaDNAAds\Plugins\Android`文件夹。Unity将在你编译APK时加载他们。

这个SDK已经在`Assets\DeltaDNA\Plugins\Android`为Google Play Services预封装一些Dependencies来推送通知（也就是智能广告）。如果你想使用你自己版本的Google Play Services，你需要删除这些Dependencies（即play-services-base-7.8.0.aar，play-services-gcm-7.8.0.aar等）以避免在编译阶段的重复类定义错误。请注意我们无法保证我们的SDK可以在除了7.8.0版本以外的Google Play Services正常使用。

## 授权

该资源适用于Apache 2.0授权。
