SET MSBUILD=C:\Windows\Microsoft.NET\Framework\v3.5\MSBuild.exe

REM SET BUILD=Debug
SET BUILD=Release

REM %MSBUILD% build.msbuild

COPY ..\src\ServiceStack.ServiceInterface\bin\%BUILD%\ServiceStack.dll ..\NuGet\ServiceStack\lib\net35
COPY ..\src\ServiceStack.ServiceInterface\bin\%BUILD%\ServiceStack.pdb ..\NuGet\ServiceStack\lib\net35
COPY ..\src\ServiceStack.ServiceInterface\bin\%BUILD%\ServiceStack.xml ..\NuGet\ServiceStack\lib\net35
COPY ..\src\ServiceStack.ServiceInterface\bin\%BUILD%\ServiceStack.ServiceInterface.* ..\NuGet\ServiceStack\lib\net35
COPY ..\src\ServiceStack.RazorEngine\bin\%BUILD%\ServiceStack.RazorEngine.* ..\NuGet\ServiceStack\lib\net35

COPY ..\src\ServiceStack.ServiceInterface\bin\%BUILD%\ServiceStack.Common.* ..\NuGet\ServiceStack.Common\lib\net35
COPY ..\src\ServiceStack.ServiceInterface\bin\%BUILD%\ServiceStack.Interfaces.* ..\NuGet\ServiceStack.Common\lib\net35

COPY ..\src\ServiceStack.FluentValidation.Mvc3\bin\%BUILD%\ServiceStack.FluentValidation.Mvc3.* ..\NuGet\ServiceStack.Mvc\lib\net4

COPY ..\src\ServiceStack.ServiceInterface\bin\%BUILD%\*.* ..\..\chaweet\api\lib

COPY ..\src\ServiceStack.RazorEngine\bin\%BUILD%\*.* ..\..\ServiceStack.Examples\lib
COPY ..\src\ServiceStack.ServiceInterface\bin\%BUILD%\*.* ..\..\ServiceStack.Examples\lib
COPY ..\src\ServiceStack\bin\%BUILD%\*.* ..\..\ServiceStack.Contrib\lib
COPY ..\src\ServiceStack\bin\%BUILD%\*.* ..\..\ServiceStack.RedisWebServices\lib

COPY ..\src\ServiceStack\bin\%BUILD%\ServiceStack.Interfaces.dll ..\..\ServiceStack.Redis\lib
COPY ..\src\ServiceStack\bin\%BUILD%\ServiceStack.Text.dll ..\..\ServiceStack.Redis\lib
COPY ..\src\ServiceStack\bin\%BUILD%\ServiceStack.Text.pdb ..\..\ServiceStack.Redis\lib
COPY ..\src\ServiceStack\bin\%BUILD%\ServiceStack.Common.dll ..\..\ServiceStack.Redis\lib
COPY ..\src\ServiceStack\bin\%BUILD%\ServiceStack.Common.pdb ..\..\ServiceStack.Redis\lib

COPY ..\src\ServiceStack\bin\%BUILD%\ServiceStack.Interfaces.dll ..\..\ServiceStack.OrmLite\lib
COPY ..\src\ServiceStack\bin\%BUILD%\ServiceStack.Text.dll ..\..\ServiceStack.OrmLite\lib
COPY ..\src\ServiceStack\bin\%BUILD%\ServiceStack.Text.pdb ..\..\ServiceStack.OrmLite\lib
COPY ..\src\ServiceStack\bin\%BUILD%\ServiceStack.Common.dll ..\..\ServiceStack.OrmLite\lib
COPY ..\src\ServiceStack\bin\%BUILD%\ServiceStack.Common.pdb ..\..\ServiceStack.OrmLite\lib

COPY ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.SqlServer\lib\*.* ..\release\latest\ServiceStack
COPY ..\..\ServiceStack.Redis\NuGet\lib\*.* ..\release\latest\ServiceStack
COPY ..\..\ServiceStack.Text\NuGet\lib\*.* ..\release\latest\ServiceStack
COPY ..\NuGet\ServiceStack\lib\*.* ..\release\latest\ServiceStack
COPY ..\NuGet\ServiceStack.Common\lib\*.* ..\release\latest\ServiceStack

COPY ..\src\ServiceStack.ServiceInterface\bin\%BUILD%\*.* ..\..\SocialApiBootstrap\lib
COPY ..\src\ServiceStack.FluentValidation.Mvc3\bin\%BUILD%\ServiceStack.FluentValidation.Mvc3.* ..\..\SocialApiBootstrap\lib
COPY ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.SqlServer\lib\*.* ..\..\SocialApiBootstrap\lib
COPY ..\..\ServiceStack.Redis\NuGet\lib\*.* ..\..\SocialApiBootstrap\lib
