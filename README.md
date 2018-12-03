# GMgardSigner

用于GMgard的自动登录与签到. 自己用就好, 禁止商业使用.

> GMgard是什么? 别问, 问就关掉.

### 特性:

 - **自动识别验证码并登录**, 识别GMgard当前的``AB+C``及``AB-C``模式验证码
 - 登录状态缓存
 - (间接)多用户签到

### 使用

#### 交互模式
直接运行程序, 会提示用户输入账号密码. 

> 登录成功后会缓存账密和Cookie至%APPDATA%/GMgardSigner下, 下次只需输入用户名即可使用缓存完成登录及签到

#### 命令行模式
使用命令行来完成登录签到: 
```
GMgardSigner.exe [-u USERNAME] [-p PASSWORD] [-s]
```
指定``-u``和``-p``参数来输入账号密码, 如果存在登录缓存, 可以只指定用户名.

指定``-s``以静默模式执行程序, 程序不会提示用户输入或等待用户退出(ReadKey&Exit), 而是直接结束.

### 多用户签到

虽然不会直接提供多用户管理, 但你可以通过批处理来实现多用户的登录和签到:

signin.bat
```shell
@echo [用户1]
@GMgardSigner.exe -u USERNAME1 -p PASSWORD1 -s
@echo [用户2]
@GMgardSigner.exe -u USERNAME2 -p PASSWORD2 -s
@echo [用户3]
@GMgardSigner.exe -u USERNAME3 -p PASSWORD3 -s
@pause
```