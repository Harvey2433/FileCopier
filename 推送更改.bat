@echo off
echo.��ʼ����
git add .
set /p commit=�����뱾�θ�������:
git commit -m %commit%
echo.Ĭ�����͵�master��֧������������
git push -u origin master
echo.���޴�����Ϣ�����ͳɹ�
pause