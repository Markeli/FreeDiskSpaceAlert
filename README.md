# FreeDiskSpaceAlert

Cross platform tool for detecting lack of free disk space and alerting via email. Useful when you need to monitor free disk space and you don't want to install large monitoring system such as `Zabix`, `NetXms`.

## Configuring
All configuring details described below:

```
# Period of alerting in minutes. A small value may increase alert frequency
AlertPeriodMin: 10
# Name of machine where app is running. Used to identify machine in alert notification.
MachineName: MS SQL Prod machine (SCWS120)
 
# Configuration of monitored devices
Drives:

#  Example. To add new drive copy this section and change value
#    Deice name (C, D, etc)
  - DeviceName: C 
# Threshold value of free disk space when app will alerting
    ThresholdValue: 10
#  Define how app will detect lack of free disk space. 
#  You can specify an exact threshold (Accuracy), or you can specify a percentage (Percentage) of the maximum disk size
    TriggerMode: Accuracy
# Allowed values: Bytes, KB, MB and GB
    MeasurementUnit: GB
  
  - DeviceName: C
    ThresholdValue: 10
    TriggerMode: Accuracy
    MeasurementUnit: GB

  - DeviceName: D
    ThresholdValue: 0.1
    TriggerMode: Percentage
    MeasurementUnit: GB

# Configuration of Email alert. Specify email params and Recipients of alert  
EmailConfiguration:
  Host: smtp server address
  Port: port of smrpt server
  Email: your@ema.il
  Password: your_password
  EnableSsl: true
  Recipients:
  - max.markelow@gmail.com
 ```

## Requirements

To run tool you need install `dotnet core 2.1`

## Running

Tool can be run as console and as service.
To run as console type in `CLI` next command:

```
dotnet fsa.dll
```

To run as service you need to pass argument `--service` or `-s`. 

To run at Windows use `scs`:

```
sc.exe create FDSA binPath= "dotnet %AppDir%\fdsa.dll -s" DisplayName="Free Disk Space Alert"
```

To run at Ubuntu use `systemd`. Example unit file:
```
[Unit]
Description=Free Disk Space Alert

[Service]
WorkingDirectory=%AppDir%
ExecStart=/usr/bin/dotnet %AppDir%/fdsa.dll -s
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT

[Install]
WantedBy=multi-user.target
```

## Logging

All logs stores at `%AppDir%\logs`. There are three type of logs:

- `info.log`: contains informational message, warning and exception message without details.
- `error.log`: contains only exceptions. If something went wrong, loot at this log.
- `trace.log`: contains detailed info about tool life. You can disable this log by changing `NLong.config`

Logs are archived every day and placed in `%AppDir%\logs\archive`. 
To disable tracing comment line in `rules` section in `NLog.config`:
```
 <logger name="*" level="Trace" writeTo="traceFile" final="true" />
 ```

