@echo off
echo.开始推送
git add .
set /p commit=请输入本次更新描述:
git commit -m %commit%
echo.默认推送到master分支，已启动推送
git push -u origin master
echo.如无错误信息则推送成功
pause