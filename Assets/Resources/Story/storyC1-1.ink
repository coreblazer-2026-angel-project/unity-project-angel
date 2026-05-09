// 第一章：小男孩与玩具车
// 主角传送到达小镇，遇到哭泣的小男孩

VAR has_met_lila = false
VAR collected_hope_1 = false

=== start ===
#scene 小镇_白天
// 传送特效，白色闪光
#ch 主角
#expr 主角惊讶
#action flash
#action shake
哇啊...！

#ch 主角
#expr 主角难过
#action enter_left
// 从左侧缓慢走入地图
呼...

#ch 主角
#expr 主角难过
#action shake
怎么没人告诉我传送点在天上啊喂！

#ch 主角
#expr 主角难过闭眼
#action bounce
#action bounce
唉...没想到第一天收集"希望"就栽了个跟头啊

#ch 主角
#expr 主角惊讶
#action lean_left
话说回来这里是...？

#ch 主角
#expr 主角开心闭眼
#action jump
这里就是人类的小镇吗？

#ch 主角
#expr 主角默认
#action pulse
啊！得先把认知修正打开才行

// 主角的小人身上出现绿色粒子特效

#ch 主角
#expr 主角默认
#action pulse
嗯...这样应该就没问题了

#ch 主角
#expr 主角惊讶
#action lean_right
莉拉，莉拉能听到我说话吗？

#ch 主角
#expr 主角惊讶
#action shake
什么情况，信号不好吗？

#ch 主角
#expr 主角难过
好吧，总之先在这个小镇转转吧

-> level_1_start

=== level_1_start ===
#ch 主角
#expr 主角惊讶
话说回来，现在莉拉不在...怎么才能知道谁是需要"修复"的人呢?

#ch 主角
#expr 主角惊讶
难道要当面问吗...？

#ch 主角
#expr 主角难过闭眼
#action shake
不行不行，这也太尴尬了

#ch 主角
#expr 主角惊讶
#action lean_left
嗯嗯......

小男孩：哇啊！！


#ch 主角
#expr 主角惊讶
#action flash
诶？！发生什么了

// 主角走向前去

#ch 主角
#expr 主角难过闭眼
那个...小朋友？你怎么了吗？

#ch 小男孩
妈妈刚给我买的玩具车...呜呜

#ch 小男孩
被我不小心摔坏了...呜啊！！

#ch 主角
#expr 主角惊慌
#action bounce
诶诶...那个...你先不要哭了好不好？

（怎么办怎么办我最不擅长对付这种小孩子了啊...）

#ch 主角
#expr 主角无语
（要，要不然趁现在试一下吧）

#ch 主角
#expr 主角惊讶
#action pulse
（心理维修...）

// 玩家可移动，与小男孩交互后进入关卡
// 第一关开始

-> END
