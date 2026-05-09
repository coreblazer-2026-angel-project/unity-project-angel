// 第一章：小男孩与玩具车
// 主角传送到达小镇，遇到哭泣的小男孩

VAR has_met_lila = false
VAR collected_hope_1 = false

=== start ===
#scene 小镇_白天
// 传送特效，白色闪光
#ch 主角
#expr 惊讶
#action flash
#action shake
哇啊...！

#ch 主角
#expr 难过
#action enter_left
// 从左侧缓慢走入地图
呼...

#ch 主角
#expr 难过
#action shake
怎么没人告诉我传送点在天上啊喂！

#ch 主角
#expr 难过
#action bounce
#action bounce
唉...没想到第一天收集"希望"就栽了个跟头啊

#ch 主角
#expr 惊讶
#action lean_left
话说回来这里是...？

#ch 主角
#expr 开心
#action jump
这里就是人类的小镇吗？

#ch 主角
#expr 默认
#action pulse
啊！得先把认知修正打开才行

// 主角的小人身上出现绿色粒子特效

#ch 主角
#expr 默认
#action pulse
嗯...这样应该就没问题了

#ch 主角
#expr 惊讶
#action lean_right
莉拉，莉拉能听到我说话吗？

#ch 主角
#expr 惊讶
#action shake
什么情况，信号不好吗？

#ch 主角
#expr 难过
好吧，总之先在这个小镇转转吧

// 玩家操控角色向右走了一小段路后

#ch 主角
#expr 惊讶
话说回来，现在莉拉不在...怎么才能知道谁是需要"修复"的人呢?

#ch 主角
#expr 惊讶
难道要当面问吗...？

#ch 主角
#expr 难过
#action shake
不行不行，这也太尴尬了

#ch 主角
#expr 惊讶
#action lean_left
嗯嗯......（思考）

// 镜头左右晃动
小男孩：哇啊！！

// 镜头向右移动，一个小男孩出现在了地图中

#ch 主角
#expr 惊讶
#action flash
诶？！发生什么了

// 主角走向前去

#ch 主角
#expr 难过
那个...小朋友？你怎么了吗？

#ch 小男孩
#expr 小男孩哭泣
#action shake
妈妈刚给我买的玩具车...呜呜

#ch 小男孩
#expr 小男孩哭泣
#action shake
#action shake
被我不小心摔坏了...呜啊！！

#ch 主角
#expr 惊慌
#action bounce
诶诶...那个...你先不要哭了好不好？

（怎么办怎么办我最不擅长对付这种小孩子了啊...）

#ch 主角
#expr 惊讶
要，要不然趁现在试一下吧

#ch 主角
#expr 惊讶
#action pulse
心理维修...

// 玩家可移动，与小男孩交互后进入关卡
// 第一关开始

=== level_1_complete ===
// 第一关全部解谜面通关，解谜区退出屏幕

#ch 主角
#expr 难过
感觉好些了吗

#ch 小男孩
#expr 小男孩难过
嗯...但是小车还是...

#ch 主角
#expr 开心
这个小车对你一定很重要吧

#ch 主角
#expr 开心
嗯...可以把它给我一下吗？

#ch 小男孩
#expr 小男孩疑惑
好...姐姐你会修这个吗？

// 玩家画图修复玩具车

#ch 主角
#expr 惊讶
#action lean_left
嗯……

#ch 主角
#expr 开心
#action bounce
嗯，这样差不多了

#ch 小男孩
#expr 小男孩惊喜
#action jump
哇！真的又能跑了！

#ch 小男孩
#expr 小男孩开心
#action bounce
#action bounce
姐姐你好厉害！

#ch 主角
#expr 开心
#action jump
嘿嘿...没什么啦

#ch 主角
#expr 开心
以后要小心一点呀，这可是妈妈送你的礼物。

#ch 小男孩
#expr 小男孩开心
#action jump
嗯！我会好好爱护它的！

// 小小男孩的小人身上蹦出了一些闪闪发光的"希望"，然后吸附到了主角身上后消失了

#ch 主角
#expr 开心
#action pulse
这应该就是希望了吧？

#ch 主角
#expr 开心
#action bounce
#action bounce
这么看这份工作好像也没有那么难呢

=== lila_appears ===
莉拉：诺艾尔，诺艾尔？

#ch 主角
#expr 开心
#action bounce
莉拉！刚才怎么回事？

#ch 莉拉
#expr 默认
刚才不知道你被传送到哪里了，这不才找到你的信号嘛

#ch 主角
#expr 开心
#action jump
你看，这是刚收集到的希望，厉害吧

#ch 莉拉
#expr 惊讶
#action flash
这么快？

#ch 莉拉
#expr 默认
刚下到人间就收集到希望了啊

#ch 主角
#expr 开心
#action bounce
#action bounce
哼哼~

#ch 莉拉
#expr 调侃
真是容易得意忘形呢

#ch 莉拉
#expr 严肃
别大意哦，人间的负面情绪可比学校里教的复杂多啦。

#ch 主角
#expr 难过
#action lean_left
知道啦知道啦

#ch 主角
#expr 难过
刚才其实是运气好，接下来还得靠你啊

#ch 莉拉
#expr 默认
明白，我帮你搜索一下

#ch 莉拉
#expr 发现
#action pulse
啊，你附近好像就有一位需要帮助的人哦

#ch 主角
#expr 惊讶
#action flash
诶？在哪里

#ch 莉拉
#expr 指引
似乎在你的前面，往那边走走吧

// 玩家操控角色继续向右行走，背景音变得安静
// 长椅上坐着一女初中生
// 交互后触发对话

-> END
