Notes for usage with Kapsch.IS.JobScheduler
===========================================

1.) log4net.dll must be replaced with the version used within Kapsch.IS.Utils. Otherwise it does not function properly.
2.) Quartz.Server.exe.config must be replaced by the Quart.Server.exe.config file in JobScheduler
3.) log4net.config must exist. if not present take it from LogginTester project within Kapsch.IS.Utils
4.) quartz.config may remain original
5.) quart_jobs.xml must be replaced by the version in JobScheduler project

