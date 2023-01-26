# VAU320
scripts for vrc aerospace university 320
VAU320 or V320 is a plane we developed with udonsharp, it requires
- SaccFlight 1.61 (https://github.com/Sacchan-VRC/SaccFlightAndVehicles)
- YuxiFlightInstrumentsforSF (https://github.com/Heriyadi235/YuxiFlightInstrumentsforSF)
- EsnyaSFAddons (https://github.com/Esnya/EsnyaSFAddons/tree/beta)
# forehead
these are VRC Aerospace University 320's scripts, includes the accesories and avionics.
Due to the file size and some other reason, it's difficult to share the whole project folders.
but feel free to do whatever you want with these scripts.

# 写给群友
## 安装方法
> 也可以参考 [将飞机导入到项目](https://yuxiaviation.com/v320neo/developer/install-aircraft.html)
### 安装依赖
- 首先导入SF1.61 (https://github.com/Sacchan-VRC/SaccFlightAndVehicles)

- 然后安装 YuxiFlightInstruments (https://github.com/Heriyadi235/YuxiFlightInstrumentsforSF)

  建议fork或直接下载解压后，使用unity包管理器导入
- 然后安装 udonradioncommunication (https://github.com/Heriyadi235/UdonRadioCommunication/tree/beta)

  新版的udonradio移除了一些utilities,这个仓库的版本将他们恢复到了***\Packages\com.nekometer.esnya.udon-radio-communications-sf\Scripts\Utilities***

- 然后安装 EsnyaSFAddons (https://github.com/Esnya/EsnyaSFAddons/tree/beta)

    使用包管理器导入，请注意EsnyaSFAddons需要一些额外依赖，在此页面 (https://github.com/Esnya/EsnyaSFAddons/tree/beta) 可以查看。如果不希望安装udonchip,可以***只导入***com.nekometer.esnya.esnya-sf-addons
  ***而不需导入***com.nekometer.esnya.esnya-sf-addons-ucs
  

### 获取资产
仓库中并未包含飞机的网格、纹理、动画以及音效，这些可以在QQ群（526014547）文件中找到
我还没更新群文件里的unitypackage, 所以现在需要**手动选择**只导入资产包中的/YuxiPlanes/A320NEO/文件夹及其中内容

### 克隆这个仓库
现在你的资产文件夹中应该已经有了/VAU320/目录，下载解压本仓库的内容，在***文件资源管理器***中覆盖掉对应位置的文件,也可以直接把项目克隆到对应的资产文件夹中
（不要直接拖进Unity,避免GUID混乱）

### 完成以上步骤应该就行了？
还有其他啥事的话可以加群问

也可以discord找我：YUXI#3129

也可以关注我twitter: @YUXI917
