Congratulations, you've just created a WebMatrix extension from a Visual Studio
project template!

You'll probably want to start by having a look at WebMatrixExtension.cs which
contains the framework for the new extension. Most of the interesting
extension points are found on the IWebMatrixHost interface which is available
to the extension via the WebMatrixHost property.

The extension is already configured (via pre- and post-build steps) to publish
itself to the right location when compiled. To configure it for easy debugging,
please perform the following steps:

1. Open the "Project" menu
2. Select "_ProjectName_ Properties" at the bottom
3. Switch to the "Debug" tab
4. Select "Start external program:"
5. Click the "..." browse button beside it
6. Browse to WebMatrix.exe at:
   C:\Program Files\Microsoft WebMatrix\WebMatrix.exe (for a 32-bit OS)
   -or-
   C:\Program Files (x86)\Microsoft WebMatrix\WebMatrix.exe (for a 64-bit OS)
7. Press F5 and WebMatrix will automatically start and load the extension

Have fun!
