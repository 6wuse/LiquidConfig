# Git 工作流约定（长期有效）

> 适用范围：本仓库全部开发（T1–T20）。
> 由来：2026-11-13 用户与教练复盘当日 git 操作后制定。见 `docs/CHANGELOG.md` CP-05。
> 操作原则：**优先用 VS Code 图形界面（Source Control 面板 / 命令面板）**；本文件每项都标注了 GUI 路径，只有 GUI 做不到的才写命令。GUI 菜单名随版本变化，找不到对应项时用命令面板输入英文名搜索。

---

## 1. 概念：commit 和 push 是两件事

| | `git commit` | `git push` |
|---|---|---|
| 写到哪里 | 本地 `.git` | 远程 `origin` |
| 需要网络 | **不需要** | 需要 |
| 需要凭据 | 不需要 | 需要 |
| 别人看得见 | 看不见 | 看得见 |
| 改写历史成本 | 随便改（`--amend`） | 已推送就要 `--force` |

**结论：白天放心频繁 commit，晚上统一 push 一次。** 一个 commit 应该是一个"能自洽解释的最小完整改动"，而不是一天的全部工作。

## 2. 分支策略：单主干

- 日常只在 `master` 上开发。不开 `login` / `test` / `新分支1` 这类与内容无关的分支。
- 建分支的唯一正当理由：**这张卡/这个练习预计跨天且不想污染主干**。命名 `task/T10-simulator` 这种「动词+卡号」格式，存活不超过 1 天，收工前合回 `master` 并立即删除（本地和远程都删）。
- 分支合回后**必须删**，不留"以后可能用得上"的分支。

## 3. 每日节奏

| 时段 | 做什么 | GUI 路径 | 等价命令 |
|---|---|---|---|
| 早上开工 | 拉最新 | 源代码管理面板 `⋯` → **拉取(变基) / Pull (Rebase)**；或命令面板 `Git: Pull (Rebase)` | `git pull --rebase origin master` |
| 开工前 | 确认工作区干净 | 面板"更改"列表为空即干净 | `git status` |
| 白天 | 做完一件自洽的事就 commit | 输入信息 → `⌘Enter` / `Ctrl+Enter` 提交 | `git add <文件>` → `git commit` |
| 晚上收工 | 先自查再推 | 状态栏分支旁的 **↑N** 悬浮查看待推送提交；再点 **同步/上传** 图标，或 `⋯` → **推送 / Push** | `git log --oneline origin/master..HEAD` → `git push origin master` |

`--rebase` 的拉取方式用于 solo 主干开发：避免每天多一个没有信息量的 `Merge branch 'master'` 提交。

## 4. 三条硬习惯

### 习惯一：`git add` 只加本次改动目的所在的文件

- **禁止** `git add -A` / `git add .` / 面板里点"更改"分组上的那个总 `+`。
- GUI：在"更改"列表里**逐个文件**点 `+`（暂存更改 / Stage Changes）。
- GUI（更细）：打开文件 → 在 diff 视图里**选中若干行** → 右键 → **暂存所选块 / Stage Selected Ranges**。这是把一次大改动拆成多次提交的关键工具。
- 撤回来重选：文件右键 → **取消暂存更改 / Unstage Changes**（等价 `git restore --staged <文件>`，不会丢代码）。

### 习惯二：提交信息 = 卡号 + 这次做了什么

格式：`<卡号或练习号>: <一句话说明做了什么>`

| 反例 | 正例 |
|---|---|
| `1` | `T10 练习1: 实现默认通道表 CreateDefaultTable` |
| `更新` | `T10 练习3: Next() 波形分派 + 噪声叠加 + 限幅量化` |
| `fix` | `T11 修复: Reconnecting 状态下未把 Tag 置 CommLost` |
| `1：定义了…2：创建了…3：将…` | 拆成 3 个提交，各带自己的卡号 |

要点：写**做了什么**而不是**改了哪些文件**；不要一行塞三件事；笔误在未 push 前用 **提交(修正) / Commit (Amend)** 改掉，别将错就错。

### 习惯三：push 前花 10 秒自查

1. 这次要上传哪些提交？→ 状态栏 `↑N` 悬浮列表，或 `git log --oneline origin/master..HEAD`。列表里每一项的信息都该能看懂。
2. `git status` 是否干净？不干净说明有该提交没提交、或该丢弃的没丢弃，先处理再推。
3. `dotnet build` 通过、相关测试转绿（铁律：每张卡交付必须能编译）。

## 5. 提交时机的项目特化规则

| 粒度 | 何时 commit |
|---|---|
| 任务卡（T1–T20） | DoD 全部达成 + `dotnet build` 通过 → 至少一次提交 |
| 卡内练习（如 T10 练习 1–5.8） | 该练习对应测试从红转绿 → 一次提交。**不要攒到整卡做完再提** |
| 调试中间态 | 不提交；要留现场用 `git stash`（GUI：`⋯` → **储藏 / Stash**），下班前恢复或丢弃 |

## 6. 危险操作分级

| 级别 | 操作 | 说明 |
|---|---|---|
| 随便用 | `git reset --soft <sha>` | 撤销提交，改动留在暂存区 |
| 随便用 | `git reset <sha>`（mixed） | 撤销提交，改动留在工作区 |
| 三思 | `git rebase -i`、`git push --force`、`git stash drop` | 会重写历史或丢东西；force 前确认没有别人在这个分支上工作 |
| **有未提交改动时禁用** | `git reset --hard`、`git checkout -- <文件>`、`git clean -fdx` | **真删代码**。VS Code 面板里的"放弃更改 / Discard Changes"同属此类，多选框全选时尤其危险 |

救命绳：`git reflog` 保留所有 HEAD 移动记录约 90 天。发现代码"不见了"，先 `git reflog` 找到那个 sha，再 `git branch rescue <sha>` 把它钉住，然后从容挑东西回来。**任何时候先建 rescue 分支，再做别的操作。**

## 7. GUI 做不到、只能用命令的少数情况

| 需求 | 命令 |
|---|---|
| 删除远程分支 | GUI 有 `Git: Delete Remote Branch`（命令面板），若不可用：`git push origin --delete <分支名>` |
| 查询远程真实默认分支 | `git ls-remote --symref origin HEAD`（本地 `origin/HEAD` 可能是缓存的旧值，不可信） |
| 改 GitHub 默认分支 | 只能网页：Settings → Branches → Default branch |
| 逐 hunk 交互式暂存 | `git add -p`（比 GUI 快，但 GUI 的"暂存所选块"够用） |
| 看某次提交改了什么 | `git show <sha>`；GUI 用提交图（Commit Graph）点提交即可 |
