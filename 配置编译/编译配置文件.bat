@SET ProjectFolder=%cd%\..
@SET LibsFolder=%ProjectFolder%\Assets\Libs
@SET ProtoFolder=%ProjectFolder%\Assets\Casual\Bundle\ClientProto
@SET ManagerFolder=%ProjectFolder%\Assets\Casual\Scripts\Config

@ECHO 项目目录:%ProjectFolder%
@ECHO 库目录:%LibsFolder%
@ECHO 配置目录:%ProtoFolder%

@IF NOT EXIST %ProjectFolder% (
@ECHO 未设置项目目录
@PAUSE
@EXIT
)

@SET CSC6="C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
@IF NOT EXIST %CSC6% (
@ECHO 找不到csc.ext路径，请检查
@PAUSE
@EXIT
)

@ECHO ----------------开始编译xls目录下的配置文件----------------
@FOR %%P IN (xls\*) DO @CALL python xls_deploy_tool.py %%P

@%CSC6% /out:client-proto.dll /t:library /r:Google.Protobuf.dll /debug- /optimize+ csharp\*.cs

@ECHO ----------------复制配置文件至项目目录----------------

@XCOPY client-proto.dll %LibsFolder%\ /Y
@FOR %%P IN (protodata\*.bytes) DO @XCOPY %%P %ProtoFolder%\ /Y
@FOR %%P IN (csharp_manager\*.cs) DO @IF NOT EXIST %ManagerFolder%\%%~NXP @XCOPY %%P %ManagerFolder%\/Y

@PAUSE