// 第二章：初中生与考试失利
// 主角在小镇遇到因考试失利而伤心的初中生

VAR collected_hope_2 = false

=== start ===
// 交互后触发对话

#ch 主角
#expr 难过
那个，你、你还好吗？

#ch 初中生
#expr 难过
你...你是谁，我好像没在镇里见过你

#ch 主角
#expr 开心
我，嗯...我正在旅游，最近会在这里停留一阵

#ch 主角
#expr 难过
我看你好像很难过的样子，是遇到什么事情了吗？

#ch 初中生
#expr 难过
#action shake
...考砸了

#ch 初中生
#expr 难过
#action shake
我明明熬了好几个晚上复习，结果到最后还是考的一塌糊涂...

#ch 初中生
#expr 难过
#action lean_left
回到家家长肯定又要说我了...

#ch 初中生
#expr 难过
#action shake
我是不是...是不是根本就不适合学习啊...

#ch 主角
#expr 惊讶
#action flash
不是这样的！

#ch 主角
#expr 开心
只是一次考试对吧？这可不能代表你的能力

#ch 初中生
#expr 难过
但是...我...

（好强的失落感，这就是需要修复的负面情绪吧）

#ch 主角
#expr 惊讶
（该进行心理维修了...）

// 进入第二关

=== level_2_complete ===
// 第二关全部解谜面通关，解谜区退出屏幕

#ch 主角
#expr 难过
你感觉好些了吗？

#ch 初中生
#expr 释然
嗯...谢谢你姐姐，我好像没那么难受了

#ch 初中生
#expr 反思
我总是因为一次挫折就失落好久,害怕辜负了父母的期待...

#ch 主角
#expr 开心
#action jump
嗯嗯，我懂这种感觉。不过愿意努力、愿意在意别人的期待的你已经很厉害啦

#ch 初中生
#expr 开心
#action bounce
嗯！我会再努力试试的

#ch 初中生
#expr 好奇
那个...请问你叫什么名字呢？

#ch 主角
#expr 开心
#action bounce
叫我主角就可以啦

#ch 初中生
#expr 开心
#action jump
主角...嗯！谢谢你，主角姐姐

#ch 主角
#expr 开心
不客气~

// 初中生的小人身上蹦出了一些闪闪发光的"希望"，然后吸附到了主角身上后消失了

（仿佛看到了在学校的自己啊）

#ch 莉拉
#expr 调侃
#action bounce
诶，你还会自己一个人偷偷哭的吗

#ch 主角
#expr 生气
#action shake
喂！

#ch 莉拉
#expr 严肃
咳咳，不开玩笑了。在你刚才心理修复的时候，我在你身边扫描到了很强烈的负面信号

#ch 主角
#expr 惊讶
#action flash
在哪里？

#ch 莉拉
#expr 指引
你得往回走了，在一个屋子里传来的。

#ch 主角
#expr 惊慌
#action shake
好...怎么有种要对战boss的感觉了

#ch 莉拉
#expr 无奈
#action bounce
想什么呢

// 玩家操控角色继续向左行走

-> END
