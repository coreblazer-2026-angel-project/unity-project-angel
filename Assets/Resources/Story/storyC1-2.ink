=== start ===
-> level_1_complete

=== level_1_complete ===
// 第一关全部解谜面通关，解谜区退出屏幕

#ch 主角
#expr 主角难过
感觉好些了吗

#ch 小男孩
嗯...但是小车还是...

#ch 主角
#expr 主角开心
这个小车对你一定很重要吧

#ch 主角
#expr 主角开心
嗯...可以把它给我一下吗？

#ch 小男孩
好...姐姐你会修这个吗？

#ch 主角
#expr 主角惊讶
#action lean_left
我试试……嗯......

#ch 主角
#expr 主角开心
#action bounce
嗯，这样差不多了

#ch 小男孩
哇！真的又能跑了！

#ch 小男孩
姐姐你好厉害！

#ch 主角
#expr 主角开心
#action jump
嘿嘿...没什么啦

#ch 主角
#expr 主角开心
以后要小心一点呀，这可是妈妈送你的礼物。

#ch 小男孩
嗯！我会好好爱护它的！

（小男孩的身上蹦出了一些闪闪发光的"希望"）


#ch 主角
#expr 主角开心
#action pulse
这应该就是希望了吧？

#ch 主角
#expr 主角开心
#action bounce
#action bounce
这么看这份工作好像也没有那么难呢

-> lila_appears

=== lila_appears ===
莉拉：诺艾尔，诺艾尔？

#ch 主角
#expr 主角开心
#action bounce
莉拉！刚才怎么回事？

#ch 莉拉
刚才不知道你被传送到哪里了，这不才找到你的信号嘛

#ch 主角
#expr 主角开心
#action jump
你看，这是刚收集到的希望，厉害吧

#ch 莉拉
这么快？

#ch 莉拉
刚下到人间就收集到希望了啊

#ch 主角
#expr 主角开心
#action bounce
#action bounce
哼哼~

#ch 莉拉
真是容易得意忘形呢

#ch 莉拉
别大意哦，人间的负面情绪可比学校里教的复杂多啦。

#ch 主角
#expr 主角难过
#action lean_left
知道啦知道啦

#ch 主角
#expr 主角难过
刚才其实是运气好，接下来还得靠你啊

#ch 莉拉
明白，我帮你搜索一下

#ch 莉拉
啊，你附近好像就有一位需要帮助的人哦

#ch 主角
#expr 主角惊讶
#action flash
诶？在哪里

#ch 莉拉
似乎在你的前面，往那边走走吧

// 长椅上坐着一女初中生
// 交互后触发下一段对话

-> END
