%SystemRoot%\Microsoft.NET\Framework\v4.7.3056.0\installutil.exe ServiceMonitorU.exe
Net Start ServiceMonitorU
sc config ServiceMonitorU start= auto
pause