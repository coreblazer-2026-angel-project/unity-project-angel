// 示例剧情脚本 - 使用方法：保存后 Unity Ink 插件自动编译生成 .json
// 拖入 InkStoryPlayer 的 inkJsonAsset 字段即可

VAR player_name = "天使"
VAR flag_met_spirit = false

=== start ===
#ch 主角
#expr 主角惊讶
哇，这里就是目的地吗？

#ch 主角
#expr 主角默认
看起来……什么都没有呢。

（四周一片寂静，只有风声和远处传来的微弱光芒。）

#ch 主角
#expr 主角开心
啊！那边好像有什么东西在发光！

#ch 主角
#expr 主角开心闭眼
* 走过去看看
    -> check_glitter
* 再观察一下四周
    -> look_around

=== check_glitter ===
#ch 主角
#expr 主角惊讶
这……这是一只小精灵？！

#ch 主角
#expr 主角默认
你好呀，小家伙，你怎么会在这里？

~ flag_met_spirit = true
小精灵眨了眨眼睛，飞到了你面前。

#ch 主角
#expr 主角默认
-> END

=== look_around ===
#ch 主角
#expr 主角难过
这里好像……有些奇怪的符文？

（你蹲下身，仔细观察地面上的纹路。）

#ch 主角
#expr 主角默认
* 用手触摸符文
    -> touch_rune
* 还是不要乱碰了
    -> dont_touch

=== touch_rune ===
#ch 主角
#expr 主角惊讶
啊——！

（小精灵从光芒中出现，似乎守护着这片遗迹。）

~ flag_met_spirit = true
#ch 主角
#expr 主角默认
-> END

=== dont_touch ===
#ch 主角
#expr 主角难过闭眼
算了，安全第一……

#ch 主角
#expr 主角默认
（你决定不贸然行动，继续观察。）

（片刻后，符文自行亮起，一只小精灵从光芒中浮现。）

~ flag_met_spirit = true
#ch 主角
#expr 主角开心
哇，好可爱！

-> END
