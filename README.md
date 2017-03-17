##Why?
CompatTelRunner telemetry runs very frequently and drains a lot of system resources. Even if the file is deleted, Windows will re-install it in a while.

##How?
This executable replaces the 'CompatTelRunner.exe' file in 'System32' with my own executable in order to prevent the telemetry functionality.
The 'dummy' will run an infinite sleep, so it won't drain any resources nor report back to Microsoft.

##Features?
Proper replace with security permissions restored
Automatic backup
Proper error handling and helfpul messages